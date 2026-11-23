// Unity Editor Tool: перенос чёрной 2-пиксельной обводки блоков из одного атласа в другой.
// Меню: Tools → Atlas Outline Transfer.
// Гарантии:
//  - Исходный атлас НИКОГДА не изменяется (только чтение пикселей как данных).
//  - Все пиксели целевого атласа, не являющиеся обнаруженной обводкой (в том числе
//    маркерные/отмеченные пользователем цвета), остаются нетронутыми.
//  - Перед записью создаётся резервная копия файла целевого атласа.
//  - Обводка только ОБНАРУЖИВАЕТСЯ в исходнике и копируется пиксель в пиксель
//    (без масштабирования, смещения, выравнивания и без генерации новой обводки).
// Обнаружение: чёрные (в пределах допуска) непрозрачные пиксели, лежащие не глубже
// заданной толщины от края непрозрачной области. Служебные (маркерные) цвета
// (например, розовый #F777F9) НЕ считаются ни обводкой, ни преградой: они исключаются
// из маски-барьера, поэтому обводка, «прижатая» к маркерам, не считается попадающей
// глубже толщины и не теряется. Внутренние чёрные детали (глубже края) НЕ копируются.

using System.Collections.Generic;
using System.IO;
using P = System.IO.Path;   // псевдоним Path (System.IO подключён)
using UnityEditor;
using UnityEngine;

public class AtlasOutlineTransferWindow : EditorWindow
{
    private const string MenuPath = "Tools/Atlas Outline Transfer";
    private const string BackupRoot = "Agent_Works/Atlas_Sprites_Work/backup";

    // ------------------------------------------------------------------ входы
    // Source Outline Atlas и Edit Atlas — ТОЛЬКО источники данных (никогда не изменяются).
    // Весь результат пишется только в Result Atlas.
    private Texture2D sourceAtlas;
    private Texture2D editAtlas;
    private Texture2D resultAtlas;

    // опциональная обрезка пикселей за внешней границей обводки (только альфа-канал)
    private bool removePixelsOutsideOutline = false;

    // настройки обнаружения обводки
    private Color outlineColor = Color.black;
    private float colorTolerance = 0.12f;   // допустимое отклонение канала от цвета обводки (0..0.5)
    private int outlineThickness = 2;        // толщина внешней обводки, px
    private float alphaThreshold = 0.03f;    // пиксели с меньшей альфой считаются прозрачными

    // маркерный (служебный) цвет: не обводка и не преграда; исключается из маски-барьера.
    // По умолчанию — розовый #F777F9, используемый в атласах проекта как служебная разметка.
    private Color markerColor = new Color32(247, 119, 249, 255);
    private bool excludeMarkers = true;

    // состояние
    private bool analyzed;
    private string summary;
    private readonly List<string> errors = new List<string>();
    private readonly List<string> warnings = new List<string>();
    private readonly List<int> outlineIndices = new List<int>();
    private readonly List<Color32> outlineColors = new List<Color32>();
    private int internalBlackCount;
    private int outlineRegionCount;
    private int markerCount;
    private int transferCount;      // сколько из обнаруженных реально отличается от Edit Atlas
    private int clippedCount;       // сколько пикселей Edit будет удалено (alpha=0) при обрезке
    private bool lastTransferOk;
    private string lastResultPath;
    private string lastBackupPath;
    private int lastTransferredCount;
    private int lastClipCount;
    private Vector2 scroll;
    private Texture2D previewTexture;

    private enum PreviewMode { ResultOverlay = 0, DetectedMask = 1, SourceOutline = 2 }
    private static readonly string[] PreviewModeLabels =
        { "Наложение на цель", "Обнаруженная маска", "Обводка исходника" };
    private PreviewMode previewMode = PreviewMode.ResultOverlay;

    private static readonly GUIContent DefaultsHint =
        new GUIContent("По умолчанию: чёрный, 2 px, допуск 0.12, маркер #F777F9 исключается — проверено на тестовых атласах outline_transfer");

    [MenuItem(MenuPath)]
    public static void ShowWindow()
    {
        GetWindow<AtlasOutlineTransferWindow>("Outline Transfer");
    }

    private void OnEnable()
    {
        // Анализ не сохраняется между открытиями окна; автоматический перенос исключён.
        analyzed = false;
        ResetRunState();
    }

    private void ResetRunState()
    {
        outlineIndices.Clear();
        outlineColors.Clear();
        errors.Clear();
        warnings.Clear();
        summary = null;
        internalBlackCount = 0;
        outlineRegionCount = 0;
        markerCount = 0;
        transferCount = 0;
        clippedCount = 0;
        lastTransferOk = false;
        lastResultPath = null;
        lastBackupPath = null;
        lastTransferredCount = 0;
        lastClipCount = 0;
        ReleasePreview();
    }

    private void OnDestroy() => ReleasePreview();

    // -------------------------------------------------------------------- GUI
    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Атласы (Source и Edit — только источники, никогда не изменяются)", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            var prev = sourceAtlas;
            sourceAtlas = (Texture2D)EditorGUILayout.ObjectField("Source Outline Atlas", sourceAtlas, typeof(Texture2D), false);
            EditorGUILayout.LabelField(TextureInfo(sourceAtlas), EditorStyles.miniLabel, GUILayout.Width(110));
            if (prev != sourceAtlas) ResetRunState();
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            var prev = editAtlas;
            editAtlas = (Texture2D)EditorGUILayout.ObjectField("Edit Atlas", editAtlas, typeof(Texture2D), false);
            EditorGUILayout.LabelField(TextureInfo(editAtlas), EditorStyles.miniLabel, GUILayout.Width(110));
            if (prev != editAtlas) ResetRunState();
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            var prev = resultAtlas;
            resultAtlas = (Texture2D)EditorGUILayout.ObjectField("Result Atlas", resultAtlas, typeof(Texture2D), false);
            EditorGUILayout.LabelField(TextureInfo(resultAtlas), EditorStyles.miniLabel, GUILayout.Width(110));
            if (prev != resultAtlas) ResetRunState();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Обработка", EditorStyles.boldLabel);
        var prevClip = removePixelsOutsideOutline;
        removePixelsOutsideOutline = EditorGUILayout.ToggleLeft(
            new GUIContent(
                "Remove Pixels Outside Outline",
                "При включении пиксели Edit Atlas, находящиеся за внешней границей обводки, становятся прозрачными (Alpha=0). Меняется только альфа-канал; сами обводочные пиксели и содержимое внутри контура сохраняются."),
            removePixelsOutsideOutline);
        if (prevClip != removePixelsOutsideOutline)
        {
            // настройка меняет итог: старый Preview и статус переноса больше не отражают текущий режим
            ReleasePreview();
            lastTransferOk = false;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Обнаружение обводки", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(DefaultsHint, EditorStyles.miniLabel);
        outlineColor = EditorGUILayout.ColorField(new GUIContent("Outline Color"), outlineColor, false, false, false);
        colorTolerance = EditorGUILayout.Slider(new GUIContent("Color Tolerance"), colorTolerance, 0f, 0.5f);
        outlineThickness = EditorGUILayout.IntSlider(new GUIContent("Outline Thickness (px)"), outlineThickness, 1, 4);
        alphaThreshold = EditorGUILayout.Slider(new GUIContent("Transparent Alpha <"), alphaThreshold, 0f, 0.5f);
        excludeMarkers = EditorGUILayout.ToggleLeft(
            new GUIContent(
                "Exclude Marker Color (не обводка и не преграда)",
                "Служебный цвет в исходном атласе (например розовый #F777F9) исключается из маски — обводка, прижатая к маркерам, не считается глубиной > толщины."),
            excludeMarkers);
        using (new EditorGUI.DisabledScope(!excludeMarkers))
        {
            markerColor = EditorGUILayout.ColorField(new GUIContent("Marker Color"), markerColor, false, false, false);
        }

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(!IsValidForAnalyze()))
            {
                if (GUILayout.Button("Analyze", GUILayout.Height(26)))
                    RunAnalyze();
            }
            using (new EditorGUI.DisabledScope(!IsValidForTransfer()))
            {
                if (GUILayout.Button("Transfer Outline", GUILayout.Height(26)))
                    RunTransfer();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Результат анализа", EditorStyles.boldLabel);
        if (!analyzed)
            EditorGUILayout.HelpBox("Назначьте все три атласа и нажмите Analyze.", MessageType.Info);
        else if (!string.IsNullOrEmpty(summary))
            EditorGUILayout.HelpBox(summary, MessageType.None);

        foreach (var e in errors)
            EditorGUILayout.HelpBox(e, MessageType.Error);
        foreach (var w in warnings)
            EditorGUILayout.HelpBox(w, MessageType.Warning);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(!IsValidForTransfer()))
            {
                if (GUILayout.Button("Preview"))
                    BuildPreview();
            }
            if (GUILayout.Button("Clear Preview"))
                ReleasePreview();
        }
        if (previewTexture != null)
        {
            var shownMode = (PreviewMode)GUILayout.Toolbar((int)previewMode, PreviewModeLabels);
            if (shownMode != previewMode)
            {
                previewMode = shownMode;
                BuildPreview();
            }
            string modeText;
            switch (previewMode)
            {
                case PreviewMode.DetectedMask:
                    modeText = "Preview: только обнаруженная маска обводки (на прозрачном фоне). Всё, что здесь видно, при Transfer переносится; разрывы здесь = пропуски обнаружения.";
                    break;
                case PreviewMode.SourceOutline:
                    modeText = "Preview: чёрные пиксели исходного атласа в его координатах (эталон обводки).";
                    break;
                default:
                    modeText = removePixelsOutsideOutline
                        ? "Preview: ОБРЕЗАННЫЙ Edit Atlas с наложенной обводкой (только во временной текстуре окна — ассеты не изменены)."
                        : "Preview: Edit Atlas с наложенной обводкой, без обрезки (только во временной текстуре окна — ассеты не изменены).";
                    break;
            }
            EditorGUILayout.HelpBox(modeText, MessageType.None);
            var rect = GUILayoutUtility.GetRect(200, 220, GUILayout.ExpandWidth(true));
            EditorGUI.DrawPreviewTexture(rect, previewTexture, null, ScaleMode.ScaleToFit);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Последний перенос", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Статус", analyzed ? (lastTransferOk ? "OK" : "не выполнялся") : "-");
        EditorGUILayout.LabelField("Записано пикселей обводки", lastTransferOk ? lastTransferredCount.ToString() : "-");
        EditorGUILayout.LabelField("Удалено обрезкой", lastTransferOk && removePixelsOutsideOutline ? lastClipCount.ToString() : "-");
        EditorGUILayout.LabelField("Файл результата", string.IsNullOrEmpty(lastResultPath) ? "-" : lastResultPath);
        EditorGUILayout.LabelField("Резервная копия", string.IsNullOrEmpty(lastBackupPath) ? "-" : lastBackupPath);
        if (GUILayout.Button("Открыть папку backup"))
            RevealPath(lastBackupPath != null ? lastBackupPath : BackupRoot);

        EditorGUILayout.EndScrollView();
    }

    private string TextureInfo(Texture2D t)
    {
        if (t == null) return "";
        return $"{t.name}\n{t.width}\u00d7{t.height}";
    }

    // ------------------------------------------------- вспомогательные пути
    private static string GetProjectRoot() => P.GetFullPath(Path.Combine(Application.dataPath, ".."));

    private void RevealPath(string relPath)
    {
        string abs = P.Combine(GetProjectRoot(), relPath);
        if (File.Exists(abs) || Directory.Exists(abs))
            EditorUtility.RevealInFinder(abs);
        else
            EditorUtility.RevealInFinder(P.Combine(GetProjectRoot(), "Agent_Works"));
    }

    private void AddError(string msg) => errors.Add(msg);
    private void AddWarning(string msg) => warnings.Add(msg);

    private void ShowLog()
    {
        if (errors.Count > 0)
            Debug.LogError("[Outline Transfer] " + string.Join(" | ", errors));
        else if (warnings.Count > 0)
            Debug.LogWarning("[Outline Transfer] " + string.Join(" | ", warnings));
    }

    // ------------------------------------------------- проверки корректности
    private bool AllAtlasesKnown => sourceAtlas != null && editAtlas != null && resultAtlas != null;

    private bool IsValidForAnalyze()
    {
        if (!AllAtlasesKnown) return false;
        int w = sourceAtlas.width, h = sourceAtlas.height;
        if (editAtlas.width != w || editAtlas.height != h) return false;
        if (resultAtlas.width != w || resultAtlas.height != h) return false;
        return true;
    }

    private bool IsValidForTransfer()
        => analyzed && errors.Count == 0 && outlineIndices.Count > 0;

    // ------------------------------------------------- чтение пикселей атласа
    /// <summary>
    /// Читает пиксели текстуры, не изменяя её. Для isReadable=false читает исходный
    /// файл с диска во временную RGBA32-текстуру (импорт-настройки не трогаются).
    /// </summary>
    private static bool TryReadPixels(Texture2D t, string label, List<string> errs, out Color32[] pixels)
    {
        pixels = null;
        if (t == null) { errs.Add($"{label}: текстура не назначена."); return false; }

        if (t.isReadable)
        {
            pixels = t.GetPixels32();
            return true;
        }

        string path = AssetDatabase.GetAssetPath(t);
        if (string.IsNullOrEmpty(path))
        {
            errs.Add($"{label}: не удаётся определить путь ассета для '{t.name}'.");
            return false;
        }

        string ext = P.GetExtension(path).ToLowerInvariant();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
        {
            errs.Add(
                $"{label}: '{t.name}' недоступна для GetPixels (Read/Write выключен), а формат файла '{ext}' " +
                "нельзя безопасно прочитать напрямую. Включите Read/Write Enabled в Import Settings текстуры " +
                "или используйте PNG/TGA→PNG файл.");
            return false;
        }

        string abs = P.GetFullPath(path);
        if (!File.Exists(abs))
        {
            errs.Add($"{label}: файл отсутствует по пути {path}.");
            return false;
        }

        // чтение с диска во временную текстуру — ассет не модифицируется
        var tmp = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
        try
        {
            if (!tmp.LoadImage(File.ReadAllBytes(abs), false))
            {
                errs.Add($"{label}: не удалось загрузить файл '{path}'.");
                return false;
            }
            pixels = tmp.GetPixels32();
            return true;
        }
        finally
        {
            Object.DestroyImmediate(tmp);
        }
    }

    // ------------------------------------------------------------------ анализ
    private void RunAnalyze()
    {
        errors.Clear();
        warnings.Clear();

        if (!AllAtlasesKnown)
        {
            AddError("Назначьте все три атласа (Source Outline Atlas, Edit Atlas и Result Atlas) перед анализом.");
            ShowLog();
            return;
        }

        int w = sourceAtlas.width, h = sourceAtlas.height;
        if (editAtlas.width != w || editAtlas.height != h || resultAtlas.width != w || resultAtlas.height != h)
        {
            AddError(
                $"Размеры атласов не совпадают: Source {sourceAtlas.width}×{sourceAtlas.height}, " +
                $"Edit {editAtlas.width}×{editAtlas.height}, Result {resultAtlas.width}×{resultAtlas.height}. " +
                "Перенос выполняется строго по координатам пикселей — атласы должны быть одинакового размера " +
                "(без масштабирования/смещения).");
            ShowLog();
            return;
        }

        Color32[] src, edit;
        if (!TryReadPixels(sourceAtlas, "Source Outline Atlas", errors, out src) ||
            !TryReadPixels(editAtlas, "Edit Atlas", errors, out edit))
        {
            ShowLog();
            return;
        }

        AnalyzeOutline(src, w, h);
        analyzed = true;
        clippedCount = 0;

        int detected = outlineIndices.Count;
        if (detected == 0)
        {
            AddWarning("Обводка не обнаружена с текущими настройками. Проверьте цвет/допуск/толщину. Перенос заблокирован.");
        }
        else
        {
            if (internalBlackCount > 0)
                AddWarning(
                    $"Найдено {internalBlackCount} чёрных пикселей глубже {outlineThickness} px от края " +
                    "(внутренние детали текстур) — они не считаются обводкой и не переносятся.");

            // сколько из обнаруженных реально отличается от Edit Atlas (это и будет записано)
            int same = 0;
            transferCount = 0;
            for (int k = 0; k < outlineIndices.Count; k++)
            {
                if (ColorsEqual(edit[outlineIndices[k]], outlineColors[k])) same++;
                else transferCount++;
            }
            if (same == detected)
                AddWarning("Все обнаруженные пиксели уже совпадают с Edit Atlas (возможно, перенос уже выполнялся). " +
                           "Перенос ничего не изменит — повторный запуск безопасен.");

            if (removePixelsOutsideOutline)
            {
                var outside = ComputeOutside(w, h, src);
                clippedCount = CountClippedPixels(edit, w, h, outside);
                if (clippedCount == 0)
                    AddWarning("Обрезка включена, но пикселей Edit Atlas за внешней границей обводки (у обведённых блоков) не найдено.");
            }
        }

        summary =
            $"Source: {sourceAtlas.width}×{sourceAtlas.height}; Edit: {editAtlas.width}×{editAtlas.height}; " +
            $"Result: {resultAtlas.width}×{resultAtlas.height}; совпадение размеров: OK.\n" +
            $"Обнаружено пикселей обводки: {detected}.\n" +
            $"Из них внутри (глубже толщины) исключено: {internalBlackCount}.\n" +
            $"Обнаруженных блоков: {outlineRegionCount}.\n" +
            $"Маркерных пикселей (исключены из маски): {markerCount} (не трогаются).\n" +
            $"Будет перенесено (несовпадающих с Edit): {transferCount}.\n" +
            (removePixelsOutsideOutline
                ? $"Обрезка: ВКЛ — станет прозрачными {clippedCount} пикселей Edit Atlas за границей обводки.\n"
                : "Обрезка: ВЫКЛ — пиксели Edit Atlas сохраняются целиком.\n") +
            $"Настройки: цвет {OutlineColorHex(outlineColor)}, допуск {colorTolerance:0.##} (~±{Mathf.RoundToInt(colorTolerance * 255f)}), " +
            $"толщина {outlineThickness}px, alpha-порог {alphaThreshold:0.###}, исключение маркера: {(excludeMarkers ? $"да ({MarkerColorHex(markerColor)})" : "нет")}.";
        ShowLog();
    }

    private static string OutlineColorHex(Color c)
        => $"#{Mathf.RoundToInt(c.r * 255f):X2}{Mathf.RoundToInt(c.g * 255f):X2}{Mathf.RoundToInt(c.b * 255f):X2}";

    private static string MarkerColorHex(Color c)
        => OutlineColorHex(c);

    /// <summary>
    /// Обнаружение внешней обводки в исходном атласе.
    /// mask = непрозрачные пиксели, КРОЕ маркерного цвета (если исключение включено):
    /// маркеры — служебная разметка между блоками, а не часть графики, поэтому
    /// не считаются ни обводкой, ни преградой. BFS от такого «края» даёт глубину d;
    /// обводка = чёрные (в допуске) непрозрачные пиксели с d &lt;= thickness.
    /// </summary>
    private void AnalyzeOutline(Color32[] src, int w, int h)
    {
        int n = w * h;
        var mask = new bool[n];
        markerCount = 0;
        byte mr = (byte)Mathf.RoundToInt(Mathf.Clamp(markerColor.r, 0f, 1f) * 255f);
        byte mg = (byte)Mathf.RoundToInt(Mathf.Clamp(markerColor.g, 0f, 1f) * 255f);
        byte mb = (byte)Mathf.RoundToInt(Mathf.Clamp(markerColor.b, 0f, 1f) * 255f);
        for (int i = 0; i < n; i++)
        {
            var c = src[i];
            if (c.a / 255f <= alphaThreshold) continue;                  // прозрачные вне маски
            if (excludeMarkers && c.r == mr && c.g == mg && c.b == mb)   // маркер: преградой не является
            {
                markerCount++;
                continue;
            }
            mask[i] = true;
        }

        // глубина от прозрачного края (4-связность), послойный BFS
        var dist = new ushort[n];
        for (int i = 0; i < n; i++) dist[i] = ushort.MaxValue;
        var layer = new Queue<int>();
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int i = y * w + x;
                if (!mask[i]) continue;
                bool edge =
                    (x == 0 || !mask[i - 1]) ||
                    (x == w - 1 || !mask[i + 1]) ||
                    (y == 0 || !mask[i - w]) ||
                    (y == h - 1 || !mask[i + w]);
                if (edge) { dist[i] = 1; layer.Enqueue(i); }
            }
        }
        int depthNow = 1;
        var nextLayer = new Queue<int>();
        while (layer.Count > 0)
        {
            int i = layer.Dequeue();
            int y = i / w, x = i % w;
            TryTouch(x + 1, y, depthNow + 1);
            TryTouch(x - 1, y, depthNow + 1);
            TryTouch(x, y - 1, depthNow + 1);
            TryTouch(x, y + 1, depthNow + 1);
            if (layer.Count == 0 && nextLayer.Count > 0)
            {
                depthNow++;
                var t = layer; layer = nextLayer; nextLayer = t;
            }
        }
        void TryTouch(int x, int y, int d)
        {
            if (x < 0 || y < 0 || x >= w || y >= h) return;
            int j = y * w + x;
            if (mask[j] && dist[j] == ushort.MaxValue) { dist[j] = (ushort)d; nextLayer.Enqueue(j); }
        }

        int tol = Mathf.RoundToInt(colorTolerance * 255f);
        int rc = Mathf.Clamp(Mathf.RoundToInt(outlineColor.r * 255f), 0, 255);
        int gc = Mathf.Clamp(Mathf.RoundToInt(outlineColor.g * 255f), 0, 255);
        int bc = Mathf.Clamp(Mathf.RoundToInt(outlineColor.b * 255f), 0, 255);

        // связные непрозрачные области (счёт «блоков»)
        var comp = new int[n];
        var stack = new Stack<int>();
        int compCount = 0;
        for (int i = 0; i < n; i++)
        {
            if (!mask[i] || comp[i] != 0) continue;
            compCount++;
            comp[i] = compCount;
            stack.Push(i);
            while (stack.Count > 0)
            {
                int j = stack.Pop();
                int y = j / w, x = j % w;
                if (x > 0 && mask[j - 1] && comp[j - 1] == 0) { comp[j - 1] = compCount; stack.Push(j - 1); }
                if (x < w - 1 && mask[j + 1] && comp[j + 1] == 0) { comp[j + 1] = compCount; stack.Push(j + 1); }
                if (y > 0 && mask[j - w] && comp[j - w] == 0) { comp[j - w] = compCount; stack.Push(j - w); }
                if (y < h - 1 && mask[j + w] && comp[j + w] == 0) { comp[j + w] = compCount; stack.Push(j + w); }
            }
        }
        outlineRegionCount = compCount;

        outlineIndices.Clear();
        outlineColors.Clear();
        internalBlackCount = 0;

        for (int i = 0; i < n; i++)
        {
            if (!mask[i]) continue;
            var c = src[i];
            bool black = Mathf.Abs(c.r - rc) <= tol && Mathf.Abs(c.g - gc) <= tol && Mathf.Abs(c.b - bc) <= tol;
            if (!black) continue;
            if (dist[i] <= outlineThickness)
            {
                outlineIndices.Add(i);
                outlineColors.Add(c);
            }
            else
            {
                internalBlackCount++;
            }
        }
    }

    private static bool ColorsEqual(Color32 a, Color32 b)
        => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;

    // -------------------------------------------- обрезка по границе обводки
    /// <summary>
    /// OUT = связная область «снаружи» от обводочного контура: фlood от границ рамки
    /// через пиксели, НЕ являющиеся преградой. Препятствия: обнаруженная обводка Source
    /// и маркерные пиксели (служебная разметка). Внутренность блоков (IN) = всё,
    /// что не OUT и не преграда.
    /// </summary>
    private bool[] ComputeOutside(int w, int h, Color32[] src)
    {
        int n = w * h;
        var wall = new bool[n];
        foreach (int i in outlineIndices) wall[i] = true;
        // маркеры — непроницаемы (защищают блоки и задают «карманы» между ними)
        if (excludeMarkers)
        {
            byte mr = (byte)Mathf.RoundToInt(Mathf.Clamp(markerColor.r, 0f, 1f) * 255f);
            byte mg = (byte)Mathf.RoundToInt(Mathf.Clamp(markerColor.g, 0f, 1f) * 255f);
            byte mb = (byte)Mathf.RoundToInt(Mathf.Clamp(markerColor.b, 0f, 1f) * 255f);
            for (int i = 0; i < n; i++)
            {
                var c = src[i];
                if (c.a / 255f > alphaThreshold && c.r == mr && c.g == mg && c.b == mb) wall[i] = true;
            }
        }
        var outside = new bool[n];
        var queue = new Queue<int>();
        for (int x = 0; x < w; x++) { Seed(x, 0); Seed(x, h - 1); }
        for (int y = 0; y < h; y++) { Seed(0, y); Seed(w - 1, y); }
        while (queue.Count > 0)
        {
            int i = queue.Dequeue();
            int y = i / w, x = i % w;
            TryTouch(x + 1, y);
            TryTouch(x - 1, y);
            TryTouch(x, y - 1);
            TryTouch(x, y + 1);
        }
        void Seed(int x, int y)
        {
            int i = y * w + x;
            if (!wall[i] && !outside[i]) { outside[i] = true; queue.Enqueue(i); }
        }
        void TryTouch(int x, int y)
        {
            if (x < 0 || y < 0 || x >= w || y >= h) return;
            int j = y * w + x;
            if (!wall[j] && !outside[j]) { outside[j] = true; queue.Enqueue(j); }
        }
        return outside;
    }

    /// <summary>
    /// Число пикселей Edit Atlas, которые станут прозрачными при обрезке:
    /// OUT-пиксели тех связных компонент Edit, которые напрямую примыкают к чёрной
    /// обводке Source. Компоненты без контакта с обводкой (блоки без обводки)
    /// не трогаются — для них контур не определён.
    /// </summary>
    private int CountClippedPixels(Color32[] edit, int w, int h, bool[] outside)
    {
        int n = w * h;
        var isOutline = new bool[n];
        foreach (int i in outlineIndices) isOutline[i] = true;
        // альфа-порог Edit как в узлавлении «прозрачный»
        float aThr = alphaThreshold * 255f;

        var comp = new int[n];
        var stack = new Stack<int>();
        int nextId = 0;
        var clippedComps = new HashSet<int>();
        int clipped = 0;
        for (int i = 0; i < n; i++)
        {
            if (edit[i].a <= aThr || comp[i] != 0) continue;
            nextId++;
            comp[i] = nextId;
            stack.Push(i);
            bool touchesOutline = false;
            var members = new List<int>();
            while (stack.Count > 0)
            {
                int j = stack.Pop();
                members.Add(j);
                int y = j / w, x = j % w;
                if (isOutline[j]) touchesOutline = true;
                if (x > 0) { int k = j - 1; if (edit[k].a > aThr && comp[k] == 0) { comp[k] = nextId; stack.Push(k); } if (isOutline[k]) touchesOutline = true; }
                if (x < w - 1) { int k = j + 1; if (edit[k].a > aThr && comp[k] == 0) { comp[k] = nextId; stack.Push(k); } if (isOutline[k]) touchesOutline = true; }
                if (y > 0) { int k = j - w; if (edit[k].a > aThr && comp[k] == 0) { comp[k] = nextId; stack.Push(k); } if (isOutline[k]) touchesOutline = true; }
                if (y < h - 1) { int k = j + w; if (edit[k].a > aThr && comp[k] == 0) { comp[k] = nextId; stack.Push(k); } if (isOutline[k]) touchesOutline = true; }
            }
            if (!touchesOutline) continue;
            foreach (int j in members)
                if (outside[j] && edit[j].a > 0) clipped++;
        }
        return clipped;
    }

    // -------------------------------------------------------------- перенос
    private void RunTransfer()
    {
        errors.Clear();
        warnings.Clear();

        if (!analyzed)
        {
            AddError("Сначала выполните Analyze.");
            ShowLog();
            return;
        }
        if (outlineIndices.Count == 0)
        {
            AddError("Нет обнаруженных пикселей обводки — перенос отменён.");
            ShowLog();
            return;
        }

        // База результата — Edit Atlas (никогда не изменяется сам по себе: копия пикселей).
        Color32[] edit;
        Color32[] src;
        if (!TryReadPixels(editAtlas, "Edit Atlas", errors, out edit) ||
            !TryReadPixels(sourceAtlas, "Source Outline Atlas", errors, out src))
        {
            ShowLog();
            return;
        }

        int w = editAtlas.width, h = editAtlas.height;
        var result = (Color32[])edit.Clone();

        // -------- опциональная обрезка: только альфа-канал OUT-пикселей обведённых блоков
        int removed = 0;
        if (removePixelsOutsideOutline)
        {
            var outside = ComputeOutside(w, h, src);
            ApplyClip(result, w, h, outside, edit, out removed);
        }

        // -------- обводка поверх обработанной текстуры (никогда не удаляется обрезкой)
        int written = 0;
        for (int k = 0; k < outlineIndices.Count; k++)
        {
            int i = outlineIndices[k];
            if (ColorsEqual(result[i], outlineColors[k])) continue;  // уже совпадает — без перезаписи
            result[i] = outlineColors[k];
            written++;
        }

        string resultPath = AssetDatabase.GetAssetPath(resultAtlas);
        string absDest = P.GetFullPath(resultPath);
        string ext = P.GetExtension(absDest).ToLowerInvariant();

        // -------- резервная копия Result Atlas (обязательна, до любых изменений)
        lastBackupPath = CreateBackup(absDest);
        if (lastBackupPath == null)
        {
            AddError("Перенос отменён: резервная копия не создана.");
            ShowLog();
            return;
        }

        // -------- сохранение (только Result Atlas)
        string sidecarPath = null;
        var tmpTexture = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
        try
        {
            tmpTexture.SetPixels32(result);
            tmpTexture.Apply();
            byte[] png = tmpTexture.EncodeToPNG();

            if (ext == ".png")
            {
                // Result Atlas PNG: обновляем его файл in-place (импорт-настройки и .meta остаются)
                if (!WriteAllBytesSafe(absDest, png))
                {
                    AddError($"Не удалось записать результат в {resultPath}. Result Atlas не изменён; резервная копия: {lastBackupPath}.");
                    ShowLog();
                    return;
                }
                lastResultPath = resultPath;
            }
            else
            {
                // формат результата нельзя безопасно перезаписать PNG-данными:
                // сохраняем отдельным PNG рядом
                string dir = P.GetDirectoryName(absDest);
                string baseName = P.GetFileNameWithoutExtension(absDest);
                string sidecar = P.Combine(dir, baseName + "_outlined.png");
                if (File.Exists(sidecar))
                {
                    AddError($"Файл {sidecar} уже существует — удалите его или переименуйте и повторите перенос.");
                    ShowLog();
                    return;
                }
                if (!WriteAllBytesSafe(sidecar, png))
                {
                    AddError($"Не удалось записать результат в {sidecar}.");
                    ShowLog();
                    return;
                }
                lastResultPath = "Assets/" + sidecar.Replace(P.GetFullPath(Application.dataPath), "Assets").Replace('\\', '/');
                sidecarPath = lastResultPath;
            }
        }
        finally
        {
            Object.DestroyImmediate(tmpTexture);
        }

        // реimport результата
        AssetDatabase.ImportAsset(lastResultPath, ImportAssetOptions.ForceUpdate);

        lastTransferredCount = written;
        lastClipCount = removed;
        lastTransferOk = true;
        summary += $"\nПеренос выполнен: записано {written} пикселей обводки (пиксель в пиксель); " +
                   $"обрезка: {(removePixelsOutsideOutline ? $"удалено {removed} пикселей Edit (alpha=0)" : "выключена")}. " +
                   $"Результат: {lastResultPath}. Резервная копия: {lastBackupPath}. Source и Edit не изменяются.";
        if (sidecarPath != null)
            AddWarning(
                $"Result Atlas был в формате '{ext}' — результат сохранён отдельным PNG: {lastResultPath}. " +
                "Оригинальный файл Result Atlas не изменён.");
        Debug.Log($"[Outline Transfer] Готово. Обводка: {written} пикс.; обрезка: {removed}. Результат: {lastResultPath}. Backup: {lastBackupPath}");
        ShowLog();
    }

    /// <summary>
    /// Реальная обрезка: OUT-пиксели компонентов Edit Atlas, примыкающих к обводке,
    /// получают Alpha=0 (только альфа-канал). Компоненты без контакта с обводкой,
    /// сама обводка и внутренность блоков не затрагиваются.
    /// </summary>
    private void ApplyClip(Color32[] target, int w, int h, bool[] outside, Color32[] editReference, out int removed)
    {
        removed = 0;
        int n = w * h;
        var isOutline = new bool[n];
        foreach (int i in outlineIndices) isOutline[i] = true;
        float aThr = alphaThreshold * 255f;

        var comp = new int[n];
        var stack = new Stack<int>();
        int nextId = 0;
        var clipComps = new HashSet<int>();
        for (int i = 0; i < n; i++)
        {
            if (editReference[i].a <= aThr || comp[i] != 0) continue;
            nextId++;
            comp[i] = nextId;
            stack.Push(i);
            bool touchesOutline = false;
            var members = new List<int>();
            while (stack.Count > 0)
            {
                int j = stack.Pop();
                members.Add(j);
                int y = j / w, x = j % w;
                if (isOutline[j]) touchesOutline = true;
                if (x > 0) { int k = j - 1; if (editReference[k].a > aThr && comp[k] == 0) { comp[k] = nextId; stack.Push(k); } if (isOutline[k]) touchesOutline = true; }
                if (x < w - 1) { int k = j + 1; if (editReference[k].a > aThr && comp[k] == 0) { comp[k] = nextId; stack.Push(k); } if (isOutline[k]) touchesOutline = true; }
                if (y > 0) { int k = j - w; if (editReference[k].a > aThr && comp[k] == 0) { comp[k] = nextId; stack.Push(k); } if (isOutline[k]) touchesOutline = true; }
                if (y < h - 1) { int k = j + w; if (editReference[k].a > aThr && comp[k] == 0) { comp[k] = nextId; stack.Push(k); } if (isOutline[k]) touchesOutline = true; }
            }
            if (!touchesOutline) continue;
            foreach (int j in members)
                if (outside[j] && target[j].a > 0)
                {
                    target[j].a = 0;  // удаление только через альфа-канал
                    removed++;
                }
        }
    }

    private static bool WriteAllBytesSafe(string path, byte[] bytes)
    {
        try
        {
            File.WriteAllBytes(path, bytes);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Outline Transfer] Ошибка записи {path}: {ex.Message}");
            return false;
        }
    }

    private string CreateBackup(string absDest)
    {
        try
        {
            string projRoot = GetProjectRoot();
            string backupDir = P.Combine(projRoot, BackupRoot);
            Directory.CreateDirectory(backupDir);
            string stamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string ext = P.GetExtension(absDest);
            string backupPath = P.Combine(backupDir, $"{P.GetFileNameWithoutExtension(absDest)}_{stamp}{ext}.backup{ext}");
            if (File.Exists(backupPath))
                backupPath = P.Combine(backupDir, $"{P.GetFileNameWithoutExtension(absDest)}_{stamp}_{System.Diagnostics.Stopwatch.GetTimestamp():x}{ext}.backup{ext}");
            File.Copy(absDest, backupPath);
            return backupPath;
        }
        catch (System.Exception ex)
        {
            AddError($"Ошибка создания резервной копии: {ex.Message}");
            return null;
        }
    }

    // -------------------------------------------------------------- Preview
    private void BuildPreview()
    {
        ReleasePreview();
        if (!IsValidForTransfer())
        {
            AddWarning("Preview доступен только после успешного Analyze с обнаруженной обводкой.");
            ShowLog();
            return;
        }

        try
        {
            var preview = new Texture2D(editAtlas.width, editAtlas.height, TextureFormat.RGBA32, false, true);
            Color32[] px;
            Color32[] edit;
            switch (previewMode)
            {
                case PreviewMode.DetectedMask:
                {
                    // маска обнаруженной обводки на прозрачном фоне — что именно будет перенесено
                    px = new Color32[editAtlas.width * editAtlas.height];
                    for (int k = 0; k < outlineIndices.Count; k++)
                        px[outlineIndices[k]] = outlineColors[k];
                    break;
                }
                case PreviewMode.SourceOutline:
                {
                    // чистые чёрные пиксели исходника в его координатах (эталон)
                    Color32[] src;
                    if (!TryReadPixels(sourceAtlas, "Source Outline Atlas", errors, out src))
                    {
                        Object.DestroyImmediate(preview);
                        ShowLog();
                        return;
                    }
                    px = src;
                    break;
                }
                default:
                {
                    // итог: Edit Atlas (опционально обрезанный) + обводка
                    Color32[] src;
                    if (!TryReadPixels(sourceAtlas, "Source Outline Atlas", errors, out src) ||
                        !TryReadPixels(editAtlas, "Edit Atlas", errors, out edit))
                    {
                        Object.DestroyImmediate(preview);
                        ShowLog();
                        return;
                    }
                    px = (Color32[])edit.Clone();
                    if (removePixelsOutsideOutline)
                    {
                        var outside = ComputeOutside(editAtlas.width, editAtlas.height, src);
                        ApplyClip(px, editAtlas.width, editAtlas.height, outside, edit, out _);
                    }
                    for (int k = 0; k < outlineIndices.Count; k++)
                        px[outlineIndices[k]] = outlineColors[k];
                    break;
                }
            }
            preview.SetPixels32(px);
            preview.Apply(false, false);
            previewTexture = preview;
        }
        finally
        {
            Repaint();
        }
    }

    private void ReleasePreview()
    {
        if (previewTexture != null)
        {
            DestroyImmediate(previewTexture);
            previewTexture = null;
        }
    }
}

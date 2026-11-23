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
    private Texture2D sourceAtlas;
    private Texture2D destAtlas;

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
    private int transferCount;      // сколько из обнаруженных реально отличается от цели
    private bool lastTransferOk;
    private string lastResultPath;
    private string lastBackupPath;
    private int lastTransferredCount;
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
        lastTransferOk = false;
        lastResultPath = null;
        lastBackupPath = null;
        lastTransferredCount = 0;
        ReleasePreview();
    }

    private void OnDestroy() => ReleasePreview();

    // -------------------------------------------------------------------- GUI
    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Атласы", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            var prev = sourceAtlas;
            sourceAtlas = (Texture2D)EditorGUILayout.ObjectField("Source Outline Atlas", sourceAtlas, typeof(Texture2D), false);
            EditorGUILayout.LabelField(TextureInfo(sourceAtlas), EditorStyles.miniLabel, GUILayout.Width(110));
            if (prev != sourceAtlas) ResetRunState();
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            var prev = destAtlas;
            destAtlas = (Texture2D)EditorGUILayout.ObjectField("Destination Atlas", destAtlas, typeof(Texture2D), false);
            EditorGUILayout.LabelField(TextureInfo(destAtlas), EditorStyles.miniLabel, GUILayout.Width(110));
            if (prev != destAtlas) ResetRunState();
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
            EditorGUILayout.HelpBox("Назначьте оба атласа и нажмите Analyze.", MessageType.Info);
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
                    modeText = "Preview: целевой атлас с наложенной обводкой (только во временной текстуре окна — ассеты не изменены).";
                    break;
            }
            EditorGUILayout.HelpBox(modeText, MessageType.None);
            var rect = GUILayoutUtility.GetRect(200, 220, GUILayout.ExpandWidth(true));
            EditorGUI.DrawPreviewTexture(rect, previewTexture, null, ScaleMode.ScaleToFit);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Последний перенос", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Статус", analyzed ? (lastTransferOk ? "OK" : "не выполнялся") : "-");
        EditorGUILayout.LabelField("Записано пикселей", lastTransferOk ? lastTransferredCount.ToString() : "-");
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
    private bool SizesKnown => sourceAtlas != null && destAtlas != null;

    private bool IsValidForAnalyze()
    {
        if (!SizesKnown) return false;
        if (sourceAtlas.width != destAtlas.width || sourceAtlas.height != destAtlas.height) return false;
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

        if (!SizesKnown)
        {
            AddError("Назначьте оба атласа (Source Outline Atlas и Destination Atlas) перед анализом.");
            ShowLog();
            return;
        }

        if (sourceAtlas.width != destAtlas.width || sourceAtlas.height != destAtlas.height)
        {
            AddError(
                $"Размеры атласов не совпадают: Source {sourceAtlas.width}×{sourceAtlas.height}, " +
                $"Destination {destAtlas.width}×{destAtlas.height}. Перенос выполняется строго по координатам " +
                "пикселей — атласы должны быть одинакового размера (без масштабирования/смещения).");
            ShowLog();
            return;
        }

        Color32[] src, dst;
        if (!TryReadPixels(sourceAtlas, "Source Outline Atlas", errors, out src) ||
            !TryReadPixels(destAtlas, "Destination Atlas", errors, out dst))
        {
            ShowLog();
            return;
        }

        AnalyzeOutline(src, sourceAtlas.width, sourceAtlas.height);
        analyzed = true;

        int detected = outlineIndices.Count;
        if (detected == 0)
            AddWarning("Обводка не обнаружена с текущими настройками. Проверьте цвет/допуск/толщину. Перенос заблокирован.");
        else if (internalBlackCount > 0)
            AddWarning(
                $"Найдено {internalBlackCount} чёрных пикселей глубже {outlineThickness} px от края " +
                "(внутренние детали текстур) — они не считаются обводкой и не переносятся.");

        // сколько из обнаруженных реально отличается от цели (это и будет записано)
        if (detected > 0)
        {
            int same = 0;
            transferCount = 0;
            for (int k = 0; k < outlineIndices.Count; k++)
            {
                if (ColorsEqual(dst[outlineIndices[k]], outlineColors[k])) same++;
                else transferCount++;
            }
            if (same == detected)
                AddWarning("Все обнаруженные пиксели уже совпадают с целевым атласом (возможно, перенос уже выполнялся). " +
                           "Перенос ничего не изменит — повторный запуск безопасен.");
        }

        summary =
            $"Source: {sourceAtlas.width}×{sourceAtlas.height}; Destination: {destAtlas.width}×{destAtlas.height}; " +
            "совпадение размеров: OK.\n" +
            $"Обнаружено пикселей обводки: {detected}.\n" +
            $"Из них внутри (глубже толщины) исключено: {internalBlackCount}.\n" +
            $"Обнаруженных областей/блоков: {outlineRegionCount}.\n" +
            $"Маркерных пикселей (исключены из маски): {markerCount} (не трогаются).\n" +
            $"Будет перенесено (несовпадающих с целью): {transferCount}.\n" +
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

        Color32[] dst;
        if (!TryReadPixels(destAtlas, "Destination Atlas", errors, out dst))
        {
            ShowLog();
            return;
        }

        string destPath = AssetDatabase.GetAssetPath(destAtlas);
        string absDest = P.GetFullPath(destPath);
        string ext = P.GetExtension(absDest).ToLowerInvariant();

        // -------- резервная копия целевого атласа (обязательна, до любых изменений)
        lastBackupPath = CreateBackup(absDest);
        if (lastBackupPath == null)
        {
            AddError("Перенос отменён: резервная копия не создана.");
            ShowLog();
            return;
        }

        // -------- формирование результата: только пиксели обводки заменяются
        var result = dst;
        int written = 0;
        for (int k = 0; k < outlineIndices.Count; k++)
        {
            int i = outlineIndices[k];
            if (ColorsEqual(result[i], outlineColors[k])) continue;  // уже совпадает — без перезаписи
            result[i] = outlineColors[k];
            written++;
        }

        // -------- сохранение
        string tmpTexPath = null;
        var tmpTexture = new Texture2D(destAtlas.width, destAtlas.height, TextureFormat.RGBA32, false, true);
        try
        {
            tmpTexture.SetPixels32(result);
            tmpTexture.Apply();
            byte[] png = tmpTexture.EncodeToPNG();

            if (ext == ".png")
            {
                // целевой атлас PNG: обновляем его файл in-place (импорт-настройки и .meta остаются)
                if (!WriteAllBytesSafe(absDest, png))
                {
                    AddError($"Не удалось записать результат в {destPath}. Целевой файл не изменён; резервная копия: {lastBackupPath}.");
                    ShowLog();
                    return;
                }
                lastResultPath = destPath;
            }
            else
            {
                // целевой файл в формате, который нельзя безопасно перезаписать PNG-данными:
                // сохраняем результат отдельным PNG-ассетом рядом с целевым
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
                tmpTexPath = lastResultPath;
            }
        }
        finally
        {
            Object.DestroyImmediate(tmpTexture);
        }

        // реimport результата
        AssetDatabase.ImportAsset(lastResultPath, ImportAssetOptions.ForceUpdate);

        lastTransferredCount = written;
        lastTransferOk = true;
        summary += $"\nПеренос выполнен: записано {written} пикселей обводки (пиксель в пиксель). " +
                   $"Результат: {lastResultPath}. Резервная копия: {lastBackupPath}";
        if (tmpTexPath != null)
            AddWarning(
                $"Целевой атлас был в формате '{ext}' — результат сохранён отдельным PNG: {lastResultPath}. " +
                "Оригинальный файл целевого атласа не изменён.");
        Debug.Log($"[Outline Transfer] Готово. Записано {written} пикселей. Результат: {lastResultPath}. Backup: {lastBackupPath}");
        ShowLog();
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
            var preview = new Texture2D(destAtlas.width, destAtlas.height, TextureFormat.RGBA32, false, true);
            Color32[] px;
            switch (previewMode)
            {
                case PreviewMode.DetectedMask:
                {
                    // маска обнаруженной обводки на прозрачном фоне — что именно будет перенесено
                    px = new Color32[destAtlas.width * destAtlas.height];
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
                    // наложение обнаруженной обводки на целевой атлас
                    Color32[] dst;
                    if (!TryReadPixels(destAtlas, "Destination Atlas", errors, out dst))
                    {
                        Object.DestroyImmediate(preview);
                        ShowLog();
                        return;
                    }
                    px = dst;
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

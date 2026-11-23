// Atlas Outline Transfer — полностью переработанная версия (пиксельная маска).
// Меню: Tools → Atlas Outline Transfer.
//
// Правила (строго по заданию, без контурных алгоритмов):
//   #F777F9 (розовый)  → результат полностью прозрачный (0,0,0,0)
//   #000000 (чёрный)   → цвет обводки из настройки Outline Color, альфа 255
//   #FF0000 (красный)  → скопировать пиксель из Source Content Atlas без изменения RGBA
//   любой другой цвет  → предупреждение, без произвольной подмены
//
// Сравнение цветов маски — строго по RGB. Альфа-канал маски не участвует.
// Размер результата = min(ширина, высота) двух исходных атласов, без масштабирования.
// Обрабатывается только общая область координат.
//
// Второй проход: прозрачные/полупрозрачные пиксели внутри красной области (#FF0000)
// заменяются ближайшим полностью непрозрачным пикселем (Alpha=255) из Source Content
// Atlas по евклидову расстоянию. Поиск ограничен областью атласа.
//
// Исходные атласы только для чтения. Результат пишется исключительно в Result Atlas.
// Перед перезаписью Result Atlas создаётся резервная копия.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using P = System.IO.Path;

public class AtlasOutlineTransferWindow : EditorWindow
{
    private const string MenuPath = "Tools/Atlas Outline Transfer"; 

    private Texture2D sourceMaskAtlas;
    private Texture2D sourceContentAtlas;
    private Texture2D resultAtlas;

    private Color outlineColor = Color.black;

    private bool analyzed;
    private string summary;
    private readonly List<string> errors = new List<string>();
    private readonly List<string> warnings = new List<string>();

    // Статистика (считается над общей областью координат)
    private int maskW, maskH, contentW, contentH, finalW, finalH;
    private int cntMaskTransparent, cntMaskOutline, cntMaskContent, cntMaskUnknown;
    private int cntCopiedContent, cntOutline;
    private int cntReplacedTransparent, cntNoOpaqueSource;

    private Vector2 scroll;
    private Texture2D previewTexture;

    // Цвета маски — сравнение строго по RGB, альфа игнорируется.
    private const byte MaskT_R = 247, MaskT_G = 119, MaskT_B = 249; // #F777F9 — прозрачность
    private const byte MaskO_R = 0,   MaskO_G = 0,   MaskO_B = 0;   // #000000 — обводка
    private const byte MaskC_R = 255, MaskC_G = 0,   MaskC_B = 0;   // #FF0000 — контент

    [MenuItem(MenuPath)]
    public static void ShowWindow() => GetWindow<AtlasOutlineTransferWindow>("Atlas Outline Transfer");

    private void OnEnable()
    {
        analyzed = false;
        summary = null;
        errors.Clear();
        warnings.Clear();
        ReleasePreview();
    }

    private void OnDestroy() => ReleasePreview();

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Атласы (источники только для чтения, результат — только в Result Atlas)", EditorStyles.boldLabel);
        Field("Source Mask Atlas", ref sourceMaskAtlas);
        Field("Source Content Atlas", ref sourceContentAtlas);
        Field("Result Atlas", ref resultAtlas);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Настройка", EditorStyles.boldLabel);
        outlineColor = EditorGUILayout.ColorField(new GUIContent("Outline Color"), outlineColor, false, false, false);
        EditorGUILayout.LabelField("Цвет обводки, записываемый в результат. По умолчанию чёрный. Альфа всегда 255.", EditorStyles.miniLabel);

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(!CanAnalyze()))
                if (GUILayout.Button("Analyze", GUILayout.Height(30))) RunAnalyze();
            using (new EditorGUI.DisabledScope(!CanPreview()))
                if (GUILayout.Button("Preview", GUILayout.Height(30))) BuildPreview();
            using (new EditorGUI.DisabledScope(!CanTransfer()))
                if (GUILayout.Button("Transfer", GUILayout.Height(30))) RunTransfer();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Статистика анализа", EditorStyles.boldLabel);
        if (!analyzed)
            EditorGUILayout.HelpBox("Назначьте все три атласа и нажмите Analyze.", MessageType.Info);
        else if (!string.IsNullOrEmpty(summary))
            EditorGUILayout.HelpBox(summary, MessageType.None);

        foreach (var e in errors) EditorGUILayout.HelpBox(e, MessageType.Error);
        foreach (var w in warnings) EditorGUILayout.HelpBox(w, MessageType.Warning);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Preview (без записи в файл)", EditorStyles.boldLabel);
        if (previewTexture != null)
        {
            var rect = GUILayoutUtility.GetRect(220, 220, GUILayout.ExpandWidth(true));
            EditorGUI.DrawPreviewTexture(rect, previewTexture, null, ScaleMode.ScaleToFit);
            if (GUILayout.Button("Очистить preview")) ReleasePreview();
        }
        else
        {
            EditorGUILayout.HelpBox("Сначала нажмите Analyze, затем Preview.", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Статус", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Готов к записи: " + (CanTransfer() ? "да" : "нет"));

        EditorGUILayout.EndScrollView();
    }

    private void Field(string label, ref Texture2D tex)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            tex = (Texture2D)EditorGUILayout.ObjectField(label, tex, typeof(Texture2D), false);
            EditorGUILayout.LabelField(TextureInfo(tex), EditorStyles.miniLabel, GUILayout.Width(110));
        }
    }

    private static string TextureInfo(Texture2D t) => t == null ? "—" : $"{t.name}  {t.width}×{t.height}";

    private bool CanAnalyze() => sourceMaskAtlas != null && sourceContentAtlas != null && resultAtlas != null;
    private bool CanPreview() => analyzed && errors.Count == 0;
    private bool CanTransfer() => analyzed && errors.Count == 0;

    private static bool TryReadPixels(Texture2D t, string label, List<string> errs, out Color32[] pixels)
    {
        pixels = null;
        if (t == null) { errs.Add($"{label}: текстура не назначена."); return false; }
        if (t.isReadable) { pixels = t.GetPixels32(); return true; }

        // Read/Write выключен — читаем исходный файл с диска во временную текстуру,
        // ассет и его импорт-настройки не изменяются.
        string path = AssetDatabase.GetAssetPath(t);
        if (string.IsNullOrEmpty(path)) { errs.Add($"{label}: не определён путь ассета."); return false; }
        string abs = P.GetFullPath(path);
        if (!File.Exists(abs)) { errs.Add($"{label}: файл не найден: {path}"); return false; }

        var tmp = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
        try
        {
            if (!tmp.LoadImage(File.ReadAllBytes(abs), false)) { errs.Add($"{label}: не удалось загрузить изображение."); return false; }
            pixels = tmp.GetPixels32();
            return true;
        }
        finally { UnityEngine.Object.DestroyImmediate(tmp); }
    }

    // Класс цвета маски по RGB: 0=прозрачность, 1=обводка, 2=контент, -1=неизвестно.
    private static int MaskClass(Color32 c)
    {
        if (c.r == MaskT_R && c.g == MaskT_G && c.b == MaskT_B) return 0;
        if (c.r == MaskO_R && c.g == MaskO_G && c.b == MaskO_B) return 1;
        if (c.r == MaskC_R && c.g == MaskC_G && c.b == MaskC_B) return 2;
        return -1;
    }

    private void RunAnalyze()
    {
        errors.Clear(); warnings.Clear(); ReleasePreview();
        if (!CanAnalyze()) { AddError("Назначьте все три атласа: Source Mask Atlas, Source Content Atlas, Result Atlas."); ShowLog(); return; }

        maskW = sourceMaskAtlas.width; maskH = sourceMaskAtlas.height;
        contentW = sourceContentAtlas.width; contentH = sourceContentAtlas.height;
        finalW = Mathf.Min(maskW, contentW);
        finalH = Mathf.Min(maskH, contentH);

        Color32[] mask, content;
        if (!TryReadPixels(sourceMaskAtlas, "Source Mask Atlas", errors, out mask) ||
            !TryReadPixels(sourceContentAtlas, "Source Content Atlas", errors, out content))
        { ShowLog(); return; }

        // Подсчёт цветов маски над общей областью.
        cntMaskTransparent = cntMaskOutline = cntMaskContent = cntMaskUnknown = 0;
        for (int y = 0; y < finalH; y++)
        {
            int mi = y * maskW;
            for (int x = 0; x < finalW; x++)
            {
                switch (MaskClass(mask[mi + x]))
                {
                    case 0: cntMaskTransparent++; break;
                    case 1: cntMaskOutline++; break;
                    case 2: cntMaskContent++; break;
                    default: cntMaskUnknown++; break;
                }
            }
        }

        // Первый проход — подсчёт.
        cntCopiedContent = 0; cntOutline = 0;
        for (int y = 0; y < finalH; y++)
        {
            int mi = y * maskW;
            for (int x = 0; x < finalW; x++)
            {
                int cls = MaskClass(mask[mi + x]);
                if (cls == 2) cntCopiedContent++;
                else if (cls == 1) cntOutline++;
            }
        }

        // Второй проход — подсчёт замен (нужен список непрозрачных пикселей контента).
        cntReplacedTransparent = 0; cntNoOpaqueSource = 0;
        var opaque = new List<(int x, int y)>();
        for (int y = 0; y < finalH; y++)
        {
            int ci = y * contentW;
            for (int x = 0; x < finalW; x++)
                if (content[ci + x].a == 255) opaque.Add((x, y));
        }

        for (int y = 0; y < finalH; y++)
        {
            int mi = y * maskW;
            int ci = y * contentW;
            for (int x = 0; x < finalW; x++)
            {
                if (MaskClass(mask[mi + x]) != 2) continue;
                if (content[ci + x].a == 255) continue; // уже непрозрачно
                if (opaque.Count == 0) { cntNoOpaqueSource++; continue; }
                int best = -1, bestD2 = int.MaxValue;
                for (int k = 0; k < opaque.Count; k++)
                {
                    int dx = x - opaque[k].x, dy = y - opaque[k].y;
                    int d2 = dx * dx + dy * dy;
                    if (d2 < bestD2) { bestD2 = d2; best = k; }
                }
                if (best < 0) { cntNoOpaqueSource++; continue; }
                cntReplacedTransparent++;
            }
        }

        analyzed = true;

        var sb = new StringBuilder();
        sb.AppendLine($"Source Mask Atlas: {maskW}×{maskH}");
        sb.AppendLine($"Source Content Atlas: {contentW}×{contentH}");
        sb.AppendLine($"Итоговый размер (минимум): {finalW}×{finalH}");
        sb.AppendLine($"Цвета маски (общая область): #F777F9={cntMaskTransparent} (прозрачность), #000000={cntMaskOutline} (обводка), #FF0000={cntMaskContent} (контент), неизвестных={cntMaskUnknown}");
        sb.AppendLine($"Первый проход: скопировано контента={cntCopiedContent}, обводки={cntOutline}");
        sb.AppendLine($"Второй проход: заменено прозрачных={cntReplacedTransparent}, без непрозрачного источника={cntNoOpaqueSource}");
        summary = sb.ToString();

        if (cntMaskUnknown > 0) AddWarning($"{cntMaskUnknown} пикселей маски имеют неизвестный цвет — не подменяются.");
        if (cntNoOpaqueSource > 0) AddWarning($"{cntNoOpaqueSource} пикселей контента не имеют непрозрачного источника — остаются без изменений.");
        ShowLog();
    }

    // Выполнить оба прохода в массив пикселей результата (и для Preview, и для Transfer).
    private Color32[] ComputeResult(Color32[] mask, Color32[] content, out int replaced, out int noSource)
    {
        var result = new Color32[finalW * finalH];
        byte or = (byte)Mathf.RoundToInt(Mathf.Clamp01(outlineColor.r) * 255f);
        byte og = (byte)Mathf.RoundToInt(Mathf.Clamp01(outlineColor.g) * 255f);
        byte ob = (byte)Mathf.RoundToInt(Mathf.Clamp01(outlineColor.b) * 255f);
        var outline = new Color32(or, og, ob, 255);

        // Первый проход.
        for (int y = 0; y < finalH; y++)
        {
            int mi = y * maskW;
            int ci = y * contentW;
            int ri = y * finalW;
            for (int x = 0; x < finalW; x++)
            {
                int cls = MaskClass(mask[mi + x]);
                if (cls == 0) result[ri + x] = new Color32(0, 0, 0, 0);              // прозрачность
                else if (cls == 1) result[ri + x] = outline;                          // обводка
                else if (cls == 2) result[ri + x] = content[ci + x];                  // контент как есть (RGBA)
                else result[ri + x] = new Color32(0, 0, 0, 0);                        // неизвестный цвет — без подмены
            }
        }

        // Второй проход — только красная область (#FF0000).
        replaced = 0; noSource = 0;
        var opaque = new List<(int x, int y, Color32 c)>();
        for (int y = 0; y < finalH; y++)
        {
            int ci = y * contentW;
            for (int x = 0; x < finalW; x++)
            {
                var c = content[ci + x];
                if (c.a == 255) opaque.Add((x, y, c));
            }
        }

        for (int y = 0; y < finalH; y++)
        {
            int mi = y * maskW;
            int ci = y * contentW;
            int ri = y * finalW;
            for (int x = 0; x < finalW; x++)
            {
                if (MaskClass(mask[mi + x]) != 2) continue;
                if (result[ri + x].a == 255) continue; // уже непрозрачно — не трогаем
                if (opaque.Count == 0) { noSource++; continue; }
                int best = -1, bestD2 = int.MaxValue;
                for (int k = 0; k < opaque.Count; k++)
                {
                    var op = opaque[k];
                    int dx = x - op.x, dy = y - op.y;
                    int d2 = dx * dx + dy * dy;
                    if (d2 < bestD2) { bestD2 = d2; best = k; }
                }
                if (best < 0) { noSource++; continue; }
                result[ri + x] = opaque[best].c; // копируем все четыре канала RGBA
                replaced++;
            }
        }
        return result;
    }

    private void BuildPreview()
    {
        if (!CanPreview()) return;
        Color32[] mask, content;
        if (!TryReadPixels(sourceMaskAtlas, "Source Mask Atlas", errors, out mask) ||
            !TryReadPixels(sourceContentAtlas, "Source Content Atlas", errors, out content))
        { ShowLog(); return; }

        int rep, nos;
        var px = ComputeResult(mask, content, out rep, out nos);
        ReleasePreview();
        var tex = new Texture2D(finalW, finalH, TextureFormat.RGBA32, false, true);
        tex.SetPixels32(px);
        tex.Apply(false, false);
        previewTexture = tex;
        Repaint();
    }

    private void RunTransfer()
    {
        if (!CanTransfer()) return;
        errors.Clear(); warnings.Clear();

        Color32[] mask, content;
        if (!TryReadPixels(sourceMaskAtlas, "Source Mask Atlas", errors, out mask) ||
            !TryReadPixels(sourceContentAtlas, "Source Content Atlas", errors, out content))
        { ShowLog(); return; }

        int rep, nos;
        var result = ComputeResult(mask, content, out rep, out nos);

        string resultPath = AssetDatabase.GetAssetPath(resultAtlas);
        if (string.IsNullOrEmpty(resultPath)) { AddError("Result Atlas не имеет пути ассета."); ShowLog(); return; }
        string absDest = P.GetFullPath(resultPath);

        // Запись только в Result Atlas.
        var tex = new Texture2D(finalW, finalH, TextureFormat.RGBA32, false, true);
        try
        {
            tex.SetPixels32(result);
            tex.Apply(false, false);
            byte[] png = tex.EncodeToPNG();
            if (!WriteAllBytesSafe(absDest, png))
            {
                AddError($"Не удалось записать результат в {resultPath}.");
                ShowLog(); return;
            }
        }
        finally { UnityEngine.Object.DestroyImmediate(tex); }

        AssetDatabase.ImportAsset(resultPath, ImportAssetOptions.ForceUpdate);

        summary = $"Перенос выполнен. Размер результата: {finalW}×{finalH}. " +
                  $"Обводка: {cntOutline}, скопировано контента: {cntCopiedContent}, " +
                  $"заменено прозрачных: {rep}, без источника: {nos}. " +
                  $"Результат: {resultPath}. ";
        if (cntMaskUnknown > 0) AddWarning($"{cntMaskUnknown} пикселей маски с неизвестным цветом не подменены.");
        if (nos > 0) AddWarning($"{nos} пикселей контента не имеют непрозрачного источника.");

        Debug.Log($"[Atlas Outline Transfer] Готово. Результат: {resultPath}.");
        ShowLog();
    }

    private void AddError(string m) => errors.Add(m);
    private void AddWarning(string m) => warnings.Add(m);

    private void ShowLog()
    {
        if (errors.Count > 0) Debug.LogError("[Atlas Outline Transfer] " + string.Join(" | ", errors));
        else if (warnings.Count > 0) Debug.LogWarning("[Atlas Outline Transfer] " + string.Join(" | ", warnings));
    }

    private static bool WriteAllBytesSafe(string path, byte[] bytes)
    {
        try { File.WriteAllBytes(path, bytes); return true; }
        catch (Exception ex) { Debug.LogError($"[Atlas Outline Transfer] Ошибка записи {path}: {ex.Message}"); return false; }
    }

    private void ReleasePreview()
    {
        if (previewTexture != null) { UnityEngine.Object.DestroyImmediate(previewTexture); previewTexture = null; }
    }
}

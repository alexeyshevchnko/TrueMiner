using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Восстановление отдельных спрайтов из атласа TerrariaTiles (1024x1024).
///
/// Источники границ (в порядке приоритета):
/// 1. SpriteRect из TextureImporter (serialized spriteSheet.sprites в .meta) — 26 прямоугольников,
///    полностью совпадающих с TerrariaTiles.tpsheet (TexturePacker) — подтверждено пиксельно.
/// 2. TerrariaTiles.tpsheet (TexturePacker-данные, идентичны meta).
/// 3. Анализ непокрытых нетранспарентных областей: у текущего атласа их НЕТ
///    (проверено попиксельно). Анализатор оставлен для перепроверки будущих атласов.
///
/// Инструмент:
///   - не изменяет атлас и его .meta (атлас читается через ImageConversion.LoadImage
///     во временную текстуру; import settings не трогаются);
///   - сохраняет каждый спрайт в отдельный PNG (Format32bppArgb, без потерь, с прозрачностью,
///     ориентация как в атласе) в Assets/Resources/TerrariaTiles_Sprites;
///   - пропускает существующие файлы (безопасный повторный запуск; принудительная
///     перезапись — удерживайте Alt при выборе пункта меню);
///   - строит ContactSheet.png для быстрой визуальной проверки.
///
/// Замечания по данным: fire1..fire7 в таблице имён .meta отсутствуют в атласе
/// (реальная графика fire1..7 — в атласе Items); Tiles_0/6/118/146/147/153/154/193 —
/// устаревшие имена (Tiles_0 отображён кодом на "Tiles_light2", см. SpriteData).
/// Tiles_73 и Tiles_74 указывают на ОДИН и тот же прямоугольник (2,988,358x34).
/// </summary>
public static class TerrariaTilesSpriteExtractor
{
    // --- настройки (не усложнены без необходимости) ---
    const string AtlasPath = "Assets/Resources/TerrariaTiles.png";   // исходный атлас
    const string OutputDir = "Assets/Resources/TerrariaTiles_Sprites"; // папка результата (только отдельные спрайты)
    const string ContactSheetPath = "Agent_Works/Atlas_Sprites_Work/preview/ContactSheet.png"; // визуальный контроль — ВНЕ папки спрайтов
    const int MinSpriteSize = 8;        // пропустить подозрительные области меньше N px по стороне
    const bool SaveTransparentPadding = true; // сохранять SpriteRect целиком (включая пустые прозрачные отступы)

    const string ExtractMenu = "Tools/Terraria Tiles/Extract Sprites";
    const string AnalyzeMenu = "Tools/Terraria Tiles/Analyze Uncovered Regions";
    const string VerifyMenu = "Tools/Terraria Tiles/Verify Imported Sprites";

    [MenuItem(ExtractMenu)]
    public static void Extract()
    {
        // Alt => перезаписать существующие файлы, иначе — безопасный повторный запуск.
        bool overwrite = Event.current != null && Event.current.alt;
        Extract(overwrite);
    }

    public static void Extract(bool overwrite)
    {
        var rects = GetRects();
        if (rects.Count == 0)
        {
            Debug.LogError("TerrariaTilesSpriteExtractor: SpriteRect не найдены (проверьте атлас)");
            return;
        }

        var texture = LoadAtlasPixels(AtlasPath);
        if (texture == null)
            return;

        Directory.CreateDirectory(OutputDir);
        int created = 0, skipped = 0;
        var seenNames = new HashSet<string>();
        foreach (var rect in rects)
        {
            if (rect.rect.width < MinSpriteSize || rect.rect.height < MinSpriteSize)
            {
                Debug.LogWarning($"TerrariaTilesSpriteExtractor: '{rect.name}' меньше MinSpriteSize ({MinSpriteSize}px) — пропущен");
                continue;
            }

            var fileName = rect.name;
            if (!seenNames.Add(fileName))
            {
                var suffix = 2;
                while (seenNames.Contains(fileName + "_" + suffix)) suffix++;
                fileName = fileName + "_" + suffix;
                Debug.LogWarning($"TerrariaTilesSpriteExtractor: дубликат имени '{rect.name}' — сохранён как '{fileName}'");
            }

            var dest = Path.Combine(OutputDir, fileName + ".png");
            if (File.Exists(dest) && !overwrite)
            {
                skipped++;
                continue;
            }

            var sprite = Crop(texture, rect.rect);
            var pngBytes = sprite.EncodeToPNG();
            File.WriteAllBytes(dest, pngBytes);
            Object.DestroyImmediate(sprite);
            created++;
        }

        BuildContactSheet(texture, rects);
        Object.DestroyImmediate(texture);

        // --- настройки импорта всех созданных/пропущенных PNG: Sprite (2D and UI), Single ---
        ApplyImportSettingsToExtracted();

        AssetDatabase.Refresh();
        Debug.Log($"TerrariaTilesSpriteExtractor: создано {created}, пропущено существующих {skipped}, всего SpriteRect {rects.Count}. Результат: {OutputDir}");
    }

    /// <summary>
    /// Устанавливает требуемые настройки импорта для КАЖДОГО PNG в папке извлечения:
    /// Texture Type = Sprite (2D and UI), Sprite Mode = Single, Alpha Is Transparency = on.
    /// Размеры, фильтрация и цветовые данные — без изменений; платформенная компрессия не
    /// форсируется (Default). После настройки выполняется Singleton SaveAndReimport, затем
    /// проверяются ФАКТИЧЕСКИЕ значения через TextureImporter.
    /// Применяется ТОЛЬКО к папке извлечения (OutputDir): пути фильтруются префиксом —
    /// текстуры проекта вне этой папки не затрагиваются (AssetPostprocessor не используется).
    /// </summary>
    public static void ApplyImportSettingsToExtracted()
    {
        AssetDatabase.Refresh();
        var prefix = OutputDir.Replace('\\', '/');
        var pngPaths = Directory.GetFiles(OutputDir, "*.png", SearchOption.AllDirectories);
        var report = new List<string>();
        foreach (var file in pngPaths)
        {
            var assetPath = file.Replace('\\', '/');
            var index = assetPath.LastIndexOf("Assets/", System.StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                continue;
            assetPath = assetPath.Substring(index);

            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                report.Add(assetPath + ": importer не найден");
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;

            // Alpha Is Transparency: включаем, если в изображении есть прозрачность
            // (у всех извлечённых спрайтов она есть), и не выключаем иначе.
            importer.alphaIsTransparency = true;

            importer.SaveAndReimport();

            // Фактическая проверка значений (не «наличие настроек в коде»).
            var actual = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            var ok = actual != null
                     && actual.textureType == TextureImporterType.Sprite
                     && actual.spriteImportMode == SpriteImportMode.Single
                     && actual.alphaIsTransparency;
            if (!ok)
                report.Add(assetPath + ": настройки не применились (" + actual.textureType + ", " + actual.spriteImportMode + ")");
        }

        if (report.Count > 0)
            Debug.LogWarning("TerrariaTilesSpriteExtractor: проблемы настроек импорта:\n" + string.Join("\n", report));
        else
            Debug.Log($"TerrariaTilesSpriteExtractor: настройки импорта Sprite (2D and UI)/Single применены и проверены для {pngPaths.Length} PNG.");
    }

    [MenuItem(VerifyMenu)]
    public static void VerifyImportedSprites()
    {
        // Перепроверка фактических настроек всех PNG папки извлечения (без перезаписи спрайтов)
        var prefix = OutputDir;
        var pngPaths = Directory.GetFiles(prefix, "*.png", SearchOption.AllDirectories);
        var problems = 0;
        foreach (var file in pngPaths)
        {
            var assetPath = file.Replace('\\', '/');
            var index = assetPath.LastIndexOf("Assets/", System.StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                continue;
            assetPath = assetPath.Substring(index);

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            bool typeOk = importer != null && importer.textureType == TextureImporterType.Sprite;
            bool modeOk = importer != null && importer.spriteImportMode == SpriteImportMode.Single;
            bool alphaOk = importer != null && importer.alphaIsTransparency;
            bool loadedOk = sprite != null && sprite.texture != null;
            // Проверка прозрачности фактической текстуры не обязательна: alpha сохраняется
            // форматом PNG; здесь достаточно importer-а и загружаемого Sprite.
            if (!(typeOk && modeOk && alphaOk && loadedOk))
            {
                problems++;
                Debug.LogWarning($"TerrariaTilesSpriteExtractor: {assetPath} — Sprite:{typeOk}, Single:{modeOk}, AlphaIsTransparency:{alphaOk}, загрузим как Sprite:{loadedOk}");
            }
        }

        if (problems == 0)
            Debug.Log($"TerrariaTilesSpriteExtractor: все {pngPaths.Length} PNG импортированы как Sprite (2D and UI), Mode Single, alpha сохранена.");
        else
            Debug.LogWarning($"TerrariaTilesSpriteExtractor: исправьте настройки через Extract Sprites — аномалий: {problems}/{pngPaths.Length}");
    }

    [MenuItem(AnalyzeMenu)]
    public static void AnalyzeUncovered()
    {
        var rects = GetRects();
        var texture = LoadAtlasPixels(AtlasPath);
        if (texture == null || rects.Count == 0)
            return;

        // Непокрытые SpriteRect'ами нетранспарентные области (BFS по пикселям).
        var covered = new bool[texture.width * texture.height];
        foreach (var r in rects)
        {
            int y0 = Mathf.RoundToInt(texture.height - r.rect.y - r.rect.height);
            int x0 = Mathf.RoundToInt(r.rect.x);
            for (var y = y0; y < y0 + r.rect.height; y++)
                for (var x = x0; x < x0 + r.rect.width; x++)
                    covered[y * texture.width + x] = true;
        }

        var visited = new bool[texture.width * texture.height];
        var found = 0;
        var colors = texture.GetPixels32();
        for (var i = 0; i < colors.Length; i++)
        {
            if (covered[i] || visited[i] || colors[i].a == 0)
                continue;
            // BFS
            int componentPixels = 0;
            int minX = int.MaxValue, maxX = -1, minY = int.MaxValue, maxY = -1;
            var stack = new Stack<int>();
            stack.Push(i);
            visited[i] = true;
            while (stack.Count > 0)
            {
                var p = stack.Pop();
                int px = p % texture.width, py = p / texture.width;
                componentPixels++;
                if (px < minX) minX = px; if (px > maxX) maxX = px;
                if (py < minY) minY = py; if (py > maxY) maxY = py;
                foreach (var d in new[] { -1, 1, -texture.width, texture.width })
                {
                    var np = p + d;
                    if (np < 0 || np >= colors.Length || visited[np] || covered[np] || colors[np].a == 0)
                        continue;
                    // не Уходим через правый/левый край
                    if (d == -1 && px == 0) continue;
                    if (d == 1 && px == texture.width - 1) continue;
                    visited[np] = true;
                    stack.Push(np);
                }
            }
            if (componentPixels >= MinSpriteSize)
            {
                found++;
                Debug.LogWarning($"TerrariaTilesSpriteExtractor: НЕПОКРЫТАЯ область {componentPixels}px, bbox({minX},{minY},{maxX - minX + 1}x{maxY - minY + 1}) — нет SpriteRect; нарезка не выполняется автоматически");
            }
        }

        Object.DestroyImmediate(texture);
        Debug.Log($"TerrariaTilesSpriteExtractor: SpriteRect={rects.Count}; непокрытых нетранспарентных областей: {found}");
    }

    // --- внутренние ---

    static List<SpriteMetaData> GetRects()
    {
        var result = new List<SpriteMetaData>();
        var importer = AssetImporter.GetAtPath(AtlasPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("TerrariaTilesSpriteExtractor: TextureImporter не найден для " + AtlasPath);
            return result;
        }

        // Приоритет 1: SpriteRect из .meta (spriteSheet.sprites).
        if (importer.spriteImportMode == SpriteImportMode.Multiple && importer.spritesheet != null)
        {
            foreach (var s in importer.spritesheet)
                result.Add(new SpriteMetaData
                {
                    name = s.name,
                    rect = s.rect,
                    alignment = s.alignment,
                    pivot = s.pivot,
                    border = s.border
                });
            return result;
        }

        // Приоритет 2: TerrariaTiles.tpsheet (TexturePacker): "name;x;y;w;h;px;py" (координаты как в .meta).
        var sheet = Path.ChangeExtension(AtlasPath, ".tpsheet");
        if (File.Exists(sheet))
        {
            foreach (var line in File.ReadAllLines(sheet))
            {
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;
                var p = line.Split(';');
                if (p.Length < 5 || !int.TryParse(p[1], out var x) || !int.TryParse(p[2], out var y) ||
                    !int.TryParse(p[3], out var w) || !int.TryParse(p[4], out var h))
                    continue;
                var name = p[0].Trim();
                result.Add(new SpriteMetaData { name = name, rect = new Rect(x, y, w, h) });
            }
            return result;
        }

        // Приоритет 3 (безопасная автоматизация отсутствует): получатель должен задать
        // границы вручную — произвольная нарезка по прозрачным промежуткам здесь не выполняется.
        Debug.LogError("TerrariaTilesSpriteExtractor: SpriteRect и .tpsheet недоступны; автоматическую нарезку не выполняю (см. Analyze Uncovered Regions)");
        return result;
    }

    static Texture2D LoadAtlasPixels(string path)
    {
        // Атлас читается из файла PNG во временную текстуру: import settings
        // (isReadable=false) и .meta не изменяются.
        if (!File.Exists(path))
        {
            Debug.LogError("TerrariaTilesSpriteExtractor: атлас не найден: " + path);
            return null;
        }
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        var bytes = File.ReadAllBytes(path);
        if (!ImageConversion.LoadImage(texture, bytes, false))
        {
            Debug.LogError("TerrariaTilesSpriteExtractor: не удалось декодировать " + path);
            Object.DestroyImmediate(texture);
            return null;
        }
        return texture;
    }

    static Texture2D Crop(Texture2D source, Rect rect)
    {
        int x = Mathf.RoundToInt(rect.x);
        // Rect.meta: y считается от НИЗА текстуры; GetPixels — тоже от нижнего ряда,
        // поэтому ориентация сохраняется без переворота.
        int y = Mathf.RoundToInt(source.height - rect.y - rect.height);
        int w = Mathf.RoundToInt(rect.width), h = Mathf.RoundToInt(rect.height);
        var pixels = source.GetPixels(x, y, w, h);
        var crop = new Texture2D(w, h, TextureFormat.RGBA32, false);
        crop.SetPixels(pixels);
        crop.Apply();
        return crop;
    }

    static void BuildContactSheet(Texture2D atlas, List<SpriteMetaData> rects)
    {
        // Контактный лист сохраняется ВНЕ папки извлечения (в документацию направления).
        // Папка результатов содержит только отдельные спрайты; лист — не игровой ресурс.
        var sheetAbsPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ContactSheetPath));
        var directory = Path.GetDirectoryName(sheetAbsPath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        // Сетка всех спрайтов (сортировка по высоте), прозрачный фон.
        const int spacing = 10;
        int cols = Mathf.CeilToInt(Mathf.Sqrt(rects.Count));
        int rows = Mathf.CeilToInt(rects.Count / (float)cols);
        float maxW = 0, maxH = 0;
        foreach (var r in rects) { maxW = Mathf.Max(maxW, r.rect.width); maxH = Mathf.Max(maxH, r.rect.height); }
        var sheet = new Texture2D((int)(cols * (maxW + spacing) + spacing), (int)(rows * (maxH + spacing) + spacing), TextureFormat.RGBA32, false);
        sheet.SetPixels(new Color[sheet.width * sheet.height]); // прозрачный фон
        var sorted = new List<SpriteMetaData>(rects);
        sorted.Sort((a, b) => { int c = a.rect.height.CompareTo(b.rect.height); return c != 0 ? c : a.rect.width.CompareTo(b.rect.width); });
        for (var i = 0; i < sorted.Count; i++)
        {
            var r = sorted[i];
            int col = i % cols, row = i / cols;
            int dx = (int)(spacing + col * (maxW + spacing) + (maxW - r.rect.width) / 2);
            int dy = (int)(spacing + row * (maxH + spacing));
            var pixels = atlas.GetPixels(
                Mathf.RoundToInt(r.rect.x),
                Mathf.RoundToInt(atlas.height - r.rect.y - r.rect.height),
                Mathf.RoundToInt(r.rect.width), Mathf.RoundToInt(r.rect.height));
            sheet.SetPixels(dx, dy, (int)r.rect.width, (int)r.rect.height, pixels);
        }
        sheet.Apply();
        File.WriteAllBytes(sheetAbsPath, sheet.EncodeToPNG());
        Object.DestroyImmediate(sheet);
    }
}

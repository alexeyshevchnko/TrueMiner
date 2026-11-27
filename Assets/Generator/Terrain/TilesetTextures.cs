using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Кэш отдельных текстур тайлсетов.
///
/// Каждый тайлсет (земля, камень, уголь, …) может лежать в собственной текстуре;
/// путь задаётся в <see cref="SpriteData.TexturePath"/> (путь относительно Assets/Resources,
/// без расширения — как требует Resources.Load).
///
/// Кэш статический: одна и та же текстура загружается ровно один раз на всё приложение,
/// сколько бы слоёв и тайлов её ни запрашивало. Во время отрисовки Resources.Load не вызывается.
///
/// Если путь пуст или текстура не найдена — возвращается null, а вызывающая сторона
/// откатывается на прежний общий атлас слоя (см. InfinityTileMapMesh.LoadSprites).
/// </summary>
public static class TilesetTextures {
    /// <summary>Каталог отдельных тайлсетов относительно Assets/Resources.</summary>
    public const string TilesetDirectory = "TerrariaTiles_Sprites/original/";

    private static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>(32);
    private static readonly HashSet<string> failed = new HashSet<string>();

    /// <summary>Сколько текстур реально загружено (для диагностики).</summary>
    public static int LoadedCount { get; private set; }

    /// <summary>Сколько раз текстура взята из кэша (для диагностики).</summary>
    public static int CacheHitCount { get; private set; }

    /// <summary>Сколько путей не удалось загрузить (для диагностики).</summary>
    public static int FailedCount { get; private set; }

    /// <summary>Текстура тайлсета по записи SpriteData; null — использовать атлас слоя.</summary>
    public static Texture2D Get(SpriteData data) {
        return data == null ? null : Get(data.TexturePath);
    }

    /// <summary>Текстура по пути Resources (без расширения).</summary>
    public static Texture2D Get(string resourcesPath) {
        if (string.IsNullOrEmpty(resourcesPath)) {
            return null;
        }

        Texture2D texture;
        if (cache.TryGetValue(resourcesPath, out texture)) {
            CacheHitCount++;
            return texture;
        }

        // Отрицательный результат тоже кэшируем, чтобы не спамить Resources.Load каждый кадр.
        if (failed.Contains(resourcesPath)) {
            return null;
        }

        texture = Resources.Load<Texture2D>(resourcesPath);
        if (texture == null) {
            failed.Add(resourcesPath);
            FailedCount++;
            Debug.LogError(
                "TilesetTextures: не найдена текстура '" + resourcesPath +
                "'. Проверьте SpriteData.TexturePath: путь должен быть относительно Assets/Resources, " +
                "без расширения, и ассет должен иметь тип Texture (не Sprite).");
            return null;
        }

        // Пиксель-арт: без сглаживания. Настройка импорта могла остаться Bilinear.
        if (texture.filterMode != FilterMode.Point) {
            texture.filterMode = FilterMode.Point;
        }

        cache[resourcesPath] = texture;
        LoadedCount++;
        Debug.Log("TilesetTextures: загружена '" + resourcesPath + "' " + texture.width + "x" + texture.height);
        return texture;
    }

    /// <summary>Сброс кэша (перезагрузка ресурсов, тесты).</summary>
    public static void Clear() {
        cache.Clear();
        failed.Clear();
        LoadedCount = 0;
        CacheHitCount = 0;
        FailedCount = 0;
    }

    /// <summary>Диагностическая строка для лога загрузки слоя.</summary>
    public static string GetDiagnostics() {
        return "текстур загружено " + LoadedCount + ", из кэша " + CacheHitCount +
               ", ошибок " + FailedCount;
    }
}

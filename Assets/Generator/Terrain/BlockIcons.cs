using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Спрайты-иконки блоков для интерфейса, собранные из СОБСТВЕННЫХ текстур тайлсетов.
/// Полностью заменяет прежнюю схему «имя кадра в gui.png»
/// (BlowBlockItem.GetUiIcon → GuiAtlas.Get).
///
/// Путь получения (существующая архитектура, ничего не дублируется):
///   TileType → SpriteData (запись тайлсета) → TilesetTextures (текстура тайлсета)
///   → UvProvider (кадр той же формулой, что у отрисовки слоёв)
///   → Sprite.Create → кэш по имени тайлсета.
///
/// Представляющий кадр НЕ задан вручную: берётся обычная схема тайлсета (data.Shema)
/// на синтетическом окне, где все соседи — тот же тип. Для Terraria-структуры это
/// внутренний полный блок (18,18), для старых наборов — их собственный «полный» кадр.
/// Отсутствие записи/текстуры/схемы даёт null — вызывающий код обязан показать
/// пустую ячейку, а не подставлять чужую иконку.
/// </summary>
public static class BlockIcons {
    private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
    private static UvProvider uvProvider;

    /// <summary>
    /// Спрайт-иконка блока по имени записи SpriteData (например, "Tiles_8").
    /// Иконка собирается один раз и кэшируется; повторные вызовы бесплатны.
    /// </summary>
    public static Sprite GetSprite(string tileCollectionName) {
        if (string.IsNullOrEmpty(tileCollectionName)) {
            return null;
        }

        Sprite cached;
        if (cache.TryGetValue(tileCollectionName, out cached)) {
            return cached;
        }

        Sprite created = Build(tileCollectionName);
        cache[tileCollectionName] = created; // null тоже кэшируем: не спамим ошибками каждый кадр
        return created;
    }

    private static Sprite Build(string name) {
        SpriteData data;
        if (!SpriteData.Sprites.TryGetValue(name, out data)) {
            Debug.LogWarning("BlockIcons: нет записи SpriteData для '" + name + "'");
            return null;
        }
        if (data.Shema == null) {
            Debug.LogWarning("BlockIcons: у '" + name + "' не задана схема кадров — иконка недоступна");
            return null;
        }

        Texture2D texture = TilesetTextures.Get(data);
        if (texture == null) {
            Debug.LogWarning("BlockIcons: не загружена текстура тайлсета '" + name + "'");
            return null;
        }

        // Представляющий кадр: синтетическое окно, где все соседи — тот же тип.
        int size = data.SquareSizeByShema;
        var window = new short[size, size];

        int center = size / 2;
        window[center, center] = data.TypeId;

        var frame = data.Shema.GetShema(window, new TileInfoStructure(data.TypeId, 0));

        // Тот же UvProvider и та же формула кадров, что у отрисовки слоёв:
        // сдвиги (shiftPadding) и размеры берутся из записи SpriteData.
        if (uvProvider == null) {
            uvProvider = new UvProvider();
        }
        uvProvider.LoadTiles(name, 0f, texture.height, texture.width, texture.height,
            data.ShiftPaddingX, data.ShiftPaddingY, data.CountHeight);
        var uv = uvProvider.GetUV(name, frame.FrameX, frame.FrameY, 0);
        if (uv == null || uv.rect.width <= 0) {
            Debug.LogWarning("BlockIcons: не удалось вычислить кадр иконки '" + name + "'");
            return null;
        }

        var sprite = Sprite.Create(texture, uv.rect, new Vector2(0.5f, 0.5f), 16f);
        sprite.name = "icon_" + name;
        return sprite;
    }
}

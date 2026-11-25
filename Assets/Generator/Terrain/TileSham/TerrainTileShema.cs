using System;

/// <summary>
/// Автотайлинг (tile framing) по принципу оригинальной Terraria.
///
/// Заменяет прежнюю упрощённую схему (выбор из ~20 кадров по 4 соседям) на полную
/// систему оригинала: кадр подбирается по 8 соседям (4 стороны + 4 диагонали) из
/// тайлсета Terraria-структуры — Tiles_0 (земля), Tiles_1 (камень) и любых будущих
/// наборов того же формата.
///
/// Как это работает:
///  1. TileDataProvider передаёт сюда окно карты (short[,]) 5x5 (центр — сам тайл)
///     и описание тайла. Значения окна — типы блоков (0 — пусто), отфильтрованные
///     по слою рендера.
///  2. 8 соседей классифицируются правилами TerrariaTilesetRules: «свой» /
///     «запрос слияния» / «граница». Пустота, деревья, лестницы, декор и вода
///     считаются границей, как неактивные тайлы в оригинале.
///  3. Для «земли» соседи-«камень» по сторонам проходят взаимную проверку, как
///     в оригинальном WorldGen.TileFrame: камень считается «своим», только если
///     он сам при кадрировании сливается в нашу сторону (для этого и нужно окно
///     5x5 — оно покрывает окрестность соседа). Диагональный камень, как и в
///     оригинале, считается «своим» без проверки.
///  4. TerrariaFraming.Solve повторяет ветвление оригинала (сверено с декомпиляцией
///     1.3.5.3 и 1.4.4.9): кадры слияния → частичные слияния → общий набор
///     (края, внешние/внутренние углы, полосы, одиночный тайл) → безопасный вариант.
///  5. Координаты кадра в пикселях листа переводятся в индексы сетки 18 px —
///     именно их ожидает UvProvider (16 px кадр + 2 px паддинг).
///
/// Класс не хранит состояния и не создаёт объектов в GetShema — безопасно вызывается
/// для каждого тайла при генерации и при локальных перерисовках (RedrawWorldTile3X
/// уже обновляет окрестность изменённого блока).
/// </summary>
public class TerrainTileShema : ITileShema {
    private readonly TerrariaTilesetRules rules;

    public TerrainTileShema(TerrariaTilesetRules rules) {
        this.rules = rules ?? TerrariaTilesetRules.Dirt;
    }

    public FrameStructure GetShema(short[,] map, TileInfoStructure info) {
        int size = map.GetLength(0);
        int c = size / 2;

        TerrariaFraming.ClassifyNeighbors(
            map, c, c, rules,
            out int up, out int down, out int left, out int right,
            out int upLeft, out int upRight, out int downLeft, out int downRight);

        if (rules.MutualMergeCheckTypes.Length > 0) {
            // Стороны: сосед «своего» типа по взаимной проверке (в оригинале —
            // tileMergeDirt-сосед земли, который сам слился в нашу сторону).
            up = ResolveMutualSlot(map, size, c, c, 0, -1, up, rules);
            down = ResolveMutualSlot(map, size, c, c, 0, 1, down, rules);
            left = ResolveMutualSlot(map, size, c, c, -1, 0, left, rules);
            right = ResolveMutualSlot(map, size, c, c, 1, 0, right, rules);

            // Диагонали: в оригинале такой сосед считается «своим» без проверки.
            if (rules.MutualMergeCheckDiagonalIsSelf) {
                if (IsMutualCheckType(map[c - 1, c - 1], rules)) {
                    upLeft = TerrariaFraming.SlotSelf;
                }
                if (IsMutualCheckType(map[c + 1, c - 1], rules)) {
                    upRight = TerrariaFraming.SlotSelf;
                }
                if (IsMutualCheckType(map[c - 1, c + 1], rules)) {
                    downLeft = TerrariaFraming.SlotSelf;
                }
                if (IsMutualCheckType(map[c + 1, c + 1], rules)) {
                    downRight = TerrariaFraming.SlotSelf;
                }
            }
        }

        // Вариант текстуры 0..2 — как tile.frameNumber() в оригинале.
        int variant = info.randomTile % 3;

        TerrariaFraming.Solve(
            up, down, left, right,
            upLeft, upRight, downLeft, downRight,
            variant, rules,
            out int frameX, out int frameY,
            out _, out _, out _, out _);

        // Пиксельные координаты листа → индексы сетки 18 px для UvProvider.
        return new FrameStructure(
            frameX / TerrariaFraming.FrameStride,
            frameY / TerrariaFraming.FrameStride);
    }

    /// <summary>
    /// Взаимная проверка кардинального соседа: он считается «своим», только если
    /// при собственном кадрировании он сливается в нашу сторону.
    /// </summary>
    private static int ResolveMutualSlot(
        short[,] map, int size, int cx, int cy, int dx, int dy, int slot, TerrariaTilesetRules rules) {
        short neighborType = map[cx + dx, cy + dy];
        if (slot == TerrariaFraming.SlotSelf || !IsMutualCheckType(neighborType, rules)) {
            return slot;
        }

        TerrariaTilesetRules neighborRules = rules.MutualMergeCheckRules;
        if (neighborRules == null || size < 5) {
            // Окрестность соседа недоступна (нужно окно 5x5) — безопасная граница.
            return TerrariaFraming.SlotEdge;
        }

        TerrariaFraming.ClassifyNeighbors(
            map, cx + dx, cy + dy, neighborRules,
            out int nUp, out int nDown, out int nLeft, out int nRight,
            out int nUpLeft, out int nUpRight, out int nDownLeft, out int nDownRight);

        TerrariaFraming.Solve(
            nUp, nDown, nLeft, nRight,
            nUpLeft, nUpRight, nDownLeft, nDownRight,
            0, neighborRules,
            out _, out _,
            out bool nMergeUp, out bool nMergeDown, out bool nMergeLeft, out bool nMergeRight);

        // Сосед сверху должен слиться вниз, снизу — вверх, слева — вправо, справа — влево
        // (в оригинале: TileFrame соседа + проверка WorldGen.mergeDown/Up/Left/Right).
        bool mergedTowardUs =
            (dy == -1 && nMergeDown) ||
            (dy == 1 && nMergeUp) ||
            (dx == -1 && nMergeRight) ||
            (dx == 1 && nMergeLeft);

        return mergedTowardUs ? TerrariaFraming.SlotSelf : TerrariaFraming.SlotEdge;
    }

    private static bool IsMutualCheckType(short type, TerrariaTilesetRules rules) {
        short[] types = rules.MutualMergeCheckTypes;
        for (int i = 0; i < types.Length; i++) {
            if (types[i] == type) {
                return true;
            }
        }
        return false;
    }
}

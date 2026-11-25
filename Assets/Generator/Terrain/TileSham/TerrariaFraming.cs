using System;

/// <summary>
/// Правила соединения (tile framing) одного тайлсета оригинальной структуры Terraria.
///
/// Тайлсеты Tiles_0 (земля) и Tiles_1 (камень) устроены так же, как в Terraria:
/// кадр 16x16 px лежит в сетке с шагом 18 px (2 px паддинга), координаты кадра —
/// (frameX, frameY) в пикселях листа. Один и тот же лист содержит:
///   • базовые кадры краёв/углов/центра (Y = 0..89);
///   • кадры внутренних углов (X = 108/180/198, Y = 18/36/72);
///   • кадры слияния с другим материалом (Y = 90..252) — используются, когда сосед
///     является «запросом слияния» (в оригинале — маркер -2).
///
/// Семантика оригинала для пары земля/камень:
///   • Земля (dirt, tileMergeDirt[0] = false) НЕ участвует в merge-блоках: земля-земля
///     → SlotSelf (обычные внутренние кадры), камень → SlotEdge (обычный край земли;
///     бесшовность на границе даётся merge-кадрами камня).
///   • Камень (tileMergeDirt[1] = true) участвует: камень-камень → SlotSelf,
///     земля → SlotMerge (кадры слияния камня в землю).
///   • MutualMergeCheckTypes — сосед, который проверяется на «встречное» слияние:
///     земля проверяет камень и, если камень рисует merge-кадр в её сторону,
///     считает его «своим» (убирает свой край — иначе видна двойная рамка).
/// </summary>
public sealed class TerrariaTilesetRules {
    /// <summary>Типы, которые считаются «своими» (SlotSelf: без границы между собой).</summary>
    public readonly short[] SelfTypes;

    /// <summary>Типы-соседи, запрашивающие слияние (SlotMerge: кадры слияния).</summary>
    public readonly short[] MergeTypes;

    /// <summary>
    /// Типы-соседи, для которых нужна взаимная проверка: сосед считается «своим»,
    /// только если он сам при кадрировании сливается в нашу сторону.
    /// В оригинале — TileMergeAttemptFrametest/TileFrame для dirt-mergeable соседей.
    /// </summary>
    public readonly short[] MutualMergeCheckTypes;

    /// <summary>Правила соседа при взаимной проверке. null — проверка не нужна.</summary>
    public readonly TerrariaTilesetRules MutualMergeCheckRules;

    /// <summary>
    /// Участвует ли тайл в merge-блоках оригинала (Main.tileMergeDirt[num]).
    /// </summary>
    public readonly bool ParticipatesInMergeBlocks;

    /// <summary>Тип относится к «травяному» семейству (для него пропускаются кадры 234-й колонки).</summary>
    public readonly bool IsGrassLike;

    public TerrariaTilesetRules(
        short[] selfTypes,
        short[] mergeTypes,
        short[] mutualMergeCheckTypes,
        TerrariaTilesetRules mutualMergeCheckRules,
        bool participatesInMergeBlocks,
        bool isGrassLike) {
        SelfTypes = selfTypes ?? EmptyTypes;
        MergeTypes = mergeTypes ?? EmptyTypes;
        MutualMergeCheckTypes = mutualMergeCheckTypes ?? EmptyTypes;
        MutualMergeCheckRules = mutualMergeCheckRules;
        ParticipatesInMergeBlocks = participatesInMergeBlocks;
        IsGrassLike = isGrassLike;
    }

    private static readonly short[] EmptyTypes = new short[0];

    /// <summary>
    /// Набор «твёрдых» типов терраина. Всё, чего нет в наборе (пустота, деревья,
    /// лестницы, декор, вода, свет), при кадрировании считается пустотой.
    /// </summary>
    private static readonly short[] TerrainSolidTypes = {
        (short) TileTypeEnum.Coal,
        (short) TileTypeEnum.Terrain0,
        (short) TileTypeEnum.TerrainGrass,
        (short) TileTypeEnum.Gold,
        (short) TileTypeEnum.Wood,
        (short) TileTypeEnum.Brick,
        (short) TileTypeEnum.Bone,
        (short) TileTypeEnum.Meat,
        (short) TileTypeEnum.Med,
        (short) TileTypeEnum.Iron,
        (short) TileTypeEnum.Stone,
    };

    public static bool IsTerrainSolid(short type) {
        for (int i = 0; i < TerrainSolidTypes.Length; i++) {
            if (TerrainSolidTypes[i] == type) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Камень (Tiles_1): камень-камень → SlotSelf, земля → SlotMerge (кадры слияния
    /// камня в землю), остальное → SlotEdge. Участвует в merge-блоках.
    /// </summary>
    public static readonly TerrariaTilesetRules Stone = new TerrariaTilesetRules(
        selfTypes: new[] { (short) TileTypeEnum.Stone },
        mergeTypes: new[] { (short) TileTypeEnum.Terrain0 },
        mutualMergeCheckTypes: null,
        mutualMergeCheckRules: null,
        participatesInMergeBlocks: true,
        isGrassLike: false);

    /// <summary>
    /// Земля (Tiles_0): земля-земля и земля-трава → SlotSelf (внутренние кадры без
    /// границ), камень → SlotEdge + взаимная проверка (если камень сливается в нашу
    /// сторону — земля убирает свой край, иначе видна двойная рамка), остальное →
    /// SlotEdge. В merge-блоках не участвует (в оригинале tileMergeDirt[0] = false).
    /// </summary>
    public static readonly TerrariaTilesetRules Dirt = new TerrariaTilesetRules(
        selfTypes: new[] { (short) TileTypeEnum.Terrain0, (short) TileTypeEnum.TerrainGrass },
        mergeTypes: null,
        mutualMergeCheckTypes: new[] { (short) TileTypeEnum.Stone },
        mutualMergeCheckRules: Stone,
        participatesInMergeBlocks: false,
        isGrassLike: false);
}

/// <summary>
/// Чистый решатель кадров Terraria для тайлов обычного («не frameImportant») типа.
///
/// Алгоритм повторяет оригинальный WorldGen.TileFrame (сверено с декомпиляцией
/// Terraria 1.3.5.3 и 1.4.4.9): merge-блоки (все 4 стороны не граница) → частичные
/// слияния с открытыми сторонами → общий набор кадров → безопасный вариант «центр».
///
/// Важное отличие частичных слияний от наивной реализации: «соединённая» сторона в
/// их условиях — ЛЮБАЯ не-граница (SlotSelf ИЛИ SlotMerge), как в оригинале, где
/// проверки вида `left == num || left == num32` охватывают и self, и merge-соседей.
/// Это устраняет чёрные разрывы на границах материалов в сложных конфигурациях.
///
/// Решатель не создаёт объектов и не обращается к карте: на вход подаются уже
/// классифицированные 8 соседей, на выходе — координаты кадра и флаги слияния
/// (флаги нужны земле для взаимной проверки с камнем).
/// </summary>
public static class TerrariaFraming {
    /// <summary>Сосед — «свой» того же материала (камень-камень, земля-земля).</summary>
    public const int SlotSelf = 1;

    /// <summary>Сосед — запрос слияния: кадры слияния на границе материалов.</summary>
    public const int SlotMerge = -2;

    /// <summary>Сосед — граница: пустота, чужой твёрдый тип или не-твёрдый тайл.</summary>
    public const int SlotEdge = -1;

    /// <summary>Шаг сетки кадров в пикселях листа Terraria (16 px кадр + 2 px паддинг).</summary>
    public const int FrameStride = 18;

    /// <summary>
    /// Классификация одного соседа по типу.
    /// </summary>
    public static int ClassifyNeighbor(short neighborType, TerrariaTilesetRules rules) {
        if (!TerrariaTilesetRules.IsTerrainSolid(neighborType)) {
            return SlotEdge;
        }
        if (Contains(rules.SelfTypes, neighborType)) {
            return SlotSelf;
        }
        if (Contains(rules.MergeTypes, neighborType)) {
            return SlotMerge;
        }
        // Чужой твёрдый тип: в оригинале TileMergeAttemptWeird превращает его
        // в границу — рисуется обычная рамка между материалами.
        return SlotEdge;
    }

    /// <summary>
    /// Классификация 8 соседей вокруг клетки (cx, cy) прямо из прямоугольника карты.
    /// map[x, y]: x — вправо, y — вниз (y = 0 — верхняя строка переданного окна).
    /// </summary>
    public static void ClassifyNeighbors(
        short[,] map, int cx, int cy, TerrariaTilesetRules rules,
        out int up, out int down, out int left, out int right,
        out int upLeft, out int upRight, out int downLeft, out int downRight) {
        left = ClassifyNeighbor(map[cx - 1, cy], rules);
        right = ClassifyNeighbor(map[cx + 1, cy], rules);
        up = ClassifyNeighbor(map[cx, cy - 1], rules);
        down = ClassifyNeighbor(map[cx, cy + 1], rules);
        upLeft = ClassifyNeighbor(map[cx - 1, cy - 1], rules);
        upRight = ClassifyNeighbor(map[cx + 1, cy - 1], rules);
        downLeft = ClassifyNeighbor(map[cx - 1, cy + 1], rules);
        downRight = ClassifyNeighbor(map[cx + 1, cy + 1], rules);
    }

    /// <summary>
    /// Подбор кадра по 8 классифицированным соседям.
    /// variant — вариант текстуры 0..2 (в оригинале tile.frameNumber()).
    /// </summary>
    public static void Solve(
        int up, int down, int left, int right,
        int upLeft, int upRight, int downLeft, int downRight,
        int variant, TerrariaTilesetRules rules,
        out int frameX, out int frameY,
        out bool mergeUp, out bool mergeDown, out bool mergeLeft, out bool mergeRight) {
        frameX = -1;
        frameY = -1;
        mergeUp = false;
        mergeDown = false;
        mergeLeft = false;
        mergeRight = false;

        int v = variant % 3;
        if (v < 0) {
            v += 3;
        }

        bool selected = false;

        // --- Блок 1: merge-блок, все четыре стороны не граница ---
        // (в оригинале — if (up != -1 && down != -1 && left != -1 && right != -1))
        if (rules.ParticipatesInMergeBlocks) {
            if (up != SlotEdge && down != SlotEdge && left != SlotEdge && right != SlotEdge) {
                if (up == SlotMerge && down == SlotSelf && left == SlotSelf && right == SlotSelf) {
                    // Слияние сверху: кадры (144..180, 108)
                    frameX = 144 + 18 * v;
                    frameY = 108;
                    mergeUp = true;
                } else if (up == SlotSelf && down == SlotMerge && left == SlotSelf && right == SlotSelf) {
                    // Слияние снизу: (144..180, 90)
                    frameX = 144 + 18 * v;
                    frameY = 90;
                    mergeDown = true;
                } else if (up == SlotSelf && down == SlotSelf && left == SlotMerge && right == SlotSelf) {
                    // Слияние слева: (162, 126..162)
                    frameX = 162;
                    frameY = 126 + 18 * v;
                    mergeLeft = true;
                } else if (up == SlotSelf && down == SlotSelf && left == SlotSelf && right == SlotMerge) {
                    // Слияние справа: (144, 126..162)
                    frameX = 144;
                    frameY = 126 + 18 * v;
                    mergeRight = true;
                } else if (up == SlotMerge && down == SlotSelf && left == SlotMerge && right == SlotSelf) {
                    // Слияние сверху и слева: (36, 90..162)
                    frameX = 36;
                    frameY = 90 + 36 * v;
                    mergeUp = true;
                    mergeLeft = true;
                } else if (up == SlotMerge && down == SlotSelf && left == SlotSelf && right == SlotMerge) {
                    // Слияние сверху и справа: (54, 90..162)
                    frameX = 54;
                    frameY = 90 + 36 * v;
                    mergeUp = true;
                    mergeRight = true;
                } else if (up == SlotSelf && down == SlotMerge && left == SlotMerge && right == SlotSelf) {
                    // Слияние снизу и слева: (36, 108..180)
                    frameX = 36;
                    frameY = 108 + 36 * v;
                    mergeDown = true;
                    mergeLeft = true;
                } else if (up == SlotSelf && down == SlotMerge && left == SlotSelf && right == SlotMerge) {
                    // Слияние снизу и справа: (54, 108..180)
                    frameX = 54;
                    frameY = 108 + 36 * v;
                    mergeDown = true;
                    mergeRight = true;
                } else if (up == SlotSelf && down == SlotSelf && left == SlotMerge && right == SlotMerge) {
                    // Слияние слева и справа: (180, 126..162)
                    frameX = 180;
                    frameY = 126 + 18 * v;
                    mergeLeft = true;
                    mergeRight = true;
                } else if (up == SlotMerge && down == SlotMerge && left == SlotSelf && right == SlotSelf) {
                    // Слияние сверху и снизу: (144..180, 180)
                    frameX = 144 + 18 * v;
                    frameY = 180;
                    mergeUp = true;
                    mergeDown = true;
                } else if (up == SlotMerge && down == SlotSelf && left == SlotMerge && right == SlotMerge) {
                    // Слияние с трёх сторон (сверху, слева, справа): (198, 90..126)
                    frameX = 198;
                    frameY = 90 + 18 * v;
                    mergeUp = true;
                    mergeLeft = true;
                    mergeRight = true;
                } else if (up == SlotSelf && down == SlotMerge && left == SlotMerge && right == SlotMerge) {
                    // Слияние с трёх сторон (снизу, слева, справа): (198, 144..180)
                    frameX = 198;
                    frameY = 144 + 18 * v;
                    mergeDown = true;
                    mergeLeft = true;
                    mergeRight = true;
                } else if (up == SlotMerge && down == SlotMerge && left == SlotSelf && right == SlotMerge) {
                    // Слияние с трёх сторон (сверху, снизу, справа): (216, 144..180)
                    frameX = 216;
                    frameY = 144 + 18 * v;
                    mergeUp = true;
                    mergeDown = true;
                    mergeRight = true;
                } else if (up == SlotMerge && down == SlotMerge && left == SlotMerge && right == SlotSelf) {
                    // Слияние с трёх сторон (сверху, снизу, слева): (216, 90..126)
                    frameX = 216;
                    frameY = 90 + 18 * v;
                    mergeUp = true;
                    mergeDown = true;
                    mergeLeft = true;
                } else if (up == SlotMerge && down == SlotMerge && left == SlotMerge && right == SlotMerge) {
                    // Слияние со всех сторон: (108..144, 198)
                    frameX = 108 + 18 * v;
                    frameY = 198;
                    mergeUp = true;
                    mergeDown = true;
                    mergeLeft = true;
                    mergeRight = true;
                } else {
                    // Все стороны «свои»: внутренние углы слияния по диагоналям
                    // (в оригинале — последовательные if, побеждает поздняя проверка).
                    bool diagonalMerge = false;
                    if (upLeft == SlotMerge) {
                        frameX = 18;
                        frameY = 108 + 36 * v;
                        diagonalMerge = true;
                    }
                    if (upRight == SlotMerge) {
                        frameX = 0;
                        frameY = 108 + 36 * v;
                        diagonalMerge = true;
                    }
                    if (downLeft == SlotMerge) {
                        frameX = 18;
                        frameY = 90 + 36 * v;
                        diagonalMerge = true;
                    }
                    if (downRight == SlotMerge) {
                        frameX = 0;
                        frameY = 90 + 36 * v;
                        diagonalMerge = true;
                    }
                    if (diagonalMerge) {
                        selected = true;
                    }
                }
                if (frameX >= 0 && frameY >= 0) {
                    // Одна из combo-веток (или диагональное слияние) выбрала кадр.
                    selected = true;
                }
            } else if (!rules.IsGrassLike) {
                // --- Блок 2: частичное слияние, хотя бы одна сторона — граница ---
                // ВАЖНО: «соединённая» сторона здесь — любая не-граница
                // (SlotSelf ИЛИ SlotMerge), как в оригинале, где проверки вида
                // `left == num || left == num32` охватывают оба варианта.
                // Иначе камень в сложных конфигурациях рисует generic-кадры
                // с тёмными рамками — видны чёрные разрывы на границах.

                // --- 2а: одно слияние + противоположная сторона граница ---
                if (up == SlotEdge && down == SlotMerge && left != SlotEdge && right != SlotEdge) {
                    // Слияние снизу при открытом верхе: (234..270, 0)
                    frameX = 234 + 18 * v;
                    frameY = 0;
                    mergeDown = true;
                    selected = true;
                } else if (up == SlotMerge && down == SlotEdge && left != SlotEdge && right != SlotEdge) {
                    // Слияние сверху при открытом низе: (234..270, 18)
                    frameX = 234 + 18 * v;
                    frameY = 18;
                    mergeUp = true;
                    selected = true;
                } else if (up != SlotEdge && down != SlotEdge && left == SlotEdge && right == SlotMerge) {
                    // Слияние справа при открытом леве: (234..270, 36)
                    frameX = 234 + 18 * v;
                    frameY = 36;
                    mergeRight = true;
                    selected = true;
                } else if (up != SlotEdge && down != SlotEdge && left == SlotMerge && right == SlotEdge) {
                    // Слияние слева при открытом праве: (234..270, 54)
                    frameX = 234 + 18 * v;
                    frameY = 54;
                    mergeLeft = true;
                    selected = true;
                }
                // --- 2б: слияние + «соединённые» стороны (self или merge) ---
                if (!selected && up != SlotEdge && down != SlotEdge && left == SlotEdge && right != SlotEdge) {
                    // Слияние сверху/снизу при открытом леве, справа соединена
                    if (up == SlotMerge && down == SlotSelf) {
                        frameX = 72;
                        frameY = 144 + 18 * v; // (72, 144..180)
                        mergeUp = true;
                        selected = true;
                    } else if (down == SlotMerge && up == SlotSelf) {
                        frameX = 72;
                        frameY = 90 + 18 * v; // (72, 90..126)
                        mergeDown = true;
                        selected = true;
                    } else if (up == SlotMerge && down == SlotMerge) {
                        // Оба слияния, лево открыто, право соединено
                        frameX = 126;
                        frameY = 144 + 18 * v; // (126, 144..180)
                        mergeUp = true;
                        selected = true;
                    }
                }
                if (!selected && up != SlotEdge && down != SlotEdge && left != SlotEdge && right == SlotEdge) {
                    // Слияние сверху/снизу при открытом праве, слева соединена
                    if (up == SlotMerge && down == SlotSelf) {
                        frameX = 90;
                        frameY = 144 + 18 * v; // (90, 144..180)
                        mergeUp = true;
                        selected = true;
                    } else if (down == SlotMerge && up == SlotSelf) {
                        frameX = 90;
                        frameY = 90 + 18 * v; // (90, 90..126)
                        mergeDown = true;
                        selected = true;
                    } else if (up == SlotMerge && down == SlotMerge) {
                        frameX = 108;
                        frameY = 144 + 18 * v; // (108, 144..180)
                        mergeUp = true;
                        selected = true;
                    }
                }
                if (!selected && up == SlotEdge && down != SlotEdge && left != SlotEdge && right != SlotEdge) {
                    // Слияние слева/справа при открытом верхе, снизу соединена
                    if (left == SlotMerge && right == SlotSelf) {
                        frameX = 18 * v; // (0..36, 198)
                        frameY = 198;
                        mergeLeft = true;
                        selected = true;
                    } else if (right == SlotMerge && left == SlotSelf) {
                        frameX = 54 + 18 * v; // (54..90, 198)
                        frameY = 198;
                        mergeRight = true;
                        selected = true;
                    } else if (left == SlotMerge && right == SlotMerge) {
                        frameX = 54 + 18 * v; // (54..90, 234) — сдвоенное, используем ближайший
                        frameY = 198;
                        mergeLeft = true;
                        selected = true;
                    }
                }
                if (!selected && up != SlotEdge && down == SlotEdge && left != SlotEdge && right != SlotEdge) {
                    // Слияние слева/справа при открытом низе, сверху соединена
                    if (left == SlotMerge && right == SlotSelf) {
                        frameX = 18 * v; // (0..36, 216)
                        frameY = 216;
                        mergeLeft = true;
                        selected = true;
                    } else if (right == SlotMerge && left == SlotSelf) {
                        frameX = 54 + 18 * v; // (54..90, 216)
                        frameY = 216;
                        mergeRight = true;
                        selected = true;
                    } else if (left == SlotMerge && right == SlotMerge) {
                        frameX = 54 + 18 * v;
                        frameY = 216;
                        mergeRight = true;
                        selected = true;
                    }
                }
                // --- 2в: два слияния + две открытые стороны ---
                if (!selected && up != SlotEdge && down != SlotEdge && left == SlotEdge && right == SlotEdge) {
                    // Слияние сверху/снизу при открытых леве и праве
                    if (up == SlotMerge && down == SlotMerge) {
                        frameX = 108;
                        frameY = 216 + 18 * v; // (108, 216..252)
                        mergeUp = true;
                        mergeDown = true;
                        selected = true;
                    } else if (up == SlotMerge) {
                        frameX = 126;
                        frameY = 144 + 18 * v; // (126, 144..180)
                        mergeUp = true;
                        selected = true;
                    } else if (down == SlotMerge) {
                        frameX = 126;
                        frameY = 90 + 18 * v; // (126, 90..126)
                        mergeDown = true;
                        selected = true;
                    }
                }
                if (!selected && up == SlotEdge && down == SlotEdge && left != SlotEdge && right != SlotEdge) {
                    // Слияние слева/справа при открытых верхе и низе
                    if (left == SlotMerge && right == SlotMerge) {
                        frameX = 162 + 18 * v; // (162..198, 198)
                        frameY = 198;
                        mergeLeft = true;
                        mergeRight = true;
                        selected = true;
                    } else if (left == SlotMerge) {
                        frameX = 18 * v; // (0..36, 252)
                        frameY = 252;
                        mergeLeft = true;
                        selected = true;
                    } else if (right == SlotMerge) {
                        frameX = 54 + 18 * v; // (54..90, 252)
                        frameY = 252;
                        mergeRight = true;
                        selected = true;
                    }
                }
                // --- 2г: одно слияние, остальные открыты ---
                if (!selected && up == SlotEdge && down == SlotEdge && left == SlotMerge && right == SlotEdge) {
                    // Слияние слева при открытых остальных сторонах: (0..36, 234)
                    frameX = 18 * v;
                    frameY = 234;
                    mergeLeft = true;
                    selected = true;
                } else if (!selected && up == SlotEdge && down == SlotEdge && left == SlotEdge && right == SlotMerge) {
                    // Слияние справа при открытых остальных сторонах: (54..90, 234)
                    frameX = 54 + 18 * v;
                    frameY = 234;
                    mergeRight = true;
                    selected = true;
                }
            }
        }

        // --- Блок 3: общий набор кадров по 4 сторонам и диагоналям ---
        if (!selected) {
            if (up == SlotSelf && down == SlotSelf && left == SlotSelf && right == SlotSelf) {
                // Внутренний тайл: варианты без границ, с внутренними углами по диагоналям.
                if (upLeft != SlotSelf && upRight != SlotSelf) {
                    frameX = 108 + 18 * v; // (108..144, 18)
                    frameY = 18;
                } else if (downLeft != SlotSelf && downRight != SlotSelf) {
                    frameX = 108 + 18 * v; // (108..144, 36)
                    frameY = 36;
                } else if (upLeft != SlotSelf && downLeft != SlotSelf) {
                    frameX = 180; // (180, 0..36)
                    frameY = 18 * v;
                } else if (upRight != SlotSelf && downRight != SlotSelf) {
                    frameX = 198; // (198, 0..36)
                    frameY = 18 * v;
                } else {
                    frameX = 18 + 18 * v; // (18..54, 18)
                    frameY = 18;
                }
            } else if (up != SlotSelf && down == SlotSelf && left == SlotSelf && right == SlotSelf) {
                // Верхний край: (18..54, 0)
                frameX = 18 + 18 * v;
                frameY = 0;
            } else if (up == SlotSelf && down != SlotSelf && left == SlotSelf && right == SlotSelf) {
                // Нижний край: (18..54, 36)
                frameX = 18 + 18 * v;
                frameY = 36;
            } else if (up == SlotSelf && down == SlotSelf && left != SlotSelf && right == SlotSelf) {
                // Левый край: (0, 0..36)
                frameX = 0;
                frameY = 18 * v;
            } else if (up == SlotSelf && down == SlotSelf && left == SlotSelf && right != SlotSelf) {
                // Правый край: (72, 0..36)
                frameX = 72;
                frameY = 18 * v;
            } else if (up != SlotSelf && down == SlotSelf && left != SlotSelf && right == SlotSelf) {
                // Внешний угол сверху-слева: (0..72, 54)
                frameX = 36 * v;
                frameY = 54;
            } else if (up != SlotSelf && down == SlotSelf && left == SlotSelf && right != SlotSelf) {
                // Внешний угол сверху-справа: (18..90, 54)
                frameX = 18 + 36 * v;
                frameY = 54;
            } else if (up == SlotSelf && down != SlotSelf && left != SlotSelf && right == SlotSelf) {
                // Внешний угол снизу-слева: (0..72, 72)
                frameX = 36 * v;
                frameY = 72;
            } else if (up == SlotSelf && down != SlotSelf && left == SlotSelf && right != SlotSelf) {
                // Внешний угол снизу-справа: (18..90, 72)
                frameX = 18 + 36 * v;
                frameY = 72;
            } else if (up == SlotSelf && down == SlotSelf && left != SlotSelf && right != SlotSelf) {
                // Горизонтальная полоса: (90, 0..36)
                frameX = 90;
                frameY = 18 * v;
            } else if (up != SlotSelf && down != SlotSelf && left == SlotSelf && right == SlotSelf) {
                // Вертикальная полоса: (108..144, 72)
                frameX = 108 + 18 * v;
                frameY = 72;
            } else if (up != SlotSelf && down == SlotSelf && left != SlotSelf && right != SlotSelf) {
                // «Шапка» сверху: (108..144, 0)
                frameX = 108 + 18 * v;
                frameY = 0;
            } else if (up == SlotSelf && down != SlotSelf && left != SlotSelf && right != SlotSelf) {
                // «Шапка» снизу: (108..144, 54)
                frameX = 108 + 18 * v;
                frameY = 54;
            } else if (up != SlotSelf && down != SlotSelf && left != SlotSelf && right == SlotSelf) {
                // Полоса, открытая сверху, снизу и слева: (162, 0..36)
                frameX = 162;
                frameY = 18 * v;
            } else if (up != SlotSelf && down != SlotSelf && left == SlotSelf && right != SlotSelf) {
                // Полоса, открытая сверху, снизу и справа: (216, 0..36)
                frameX = 216;
                frameY = 18 * v;
            } else {
                // Одиночный блок: (162..198, 54)
                frameX = 162 + 18 * v;
                frameY = 54;
            }
            selected = true;
        }

        // --- Блок 4: безопасный вариант, если ничего не подошло ---
        if (!selected) {
            frameX = 18 + 18 * v;
            frameY = 18;
        }
    }

    private static bool Contains(short[] types, short type) {
        for (int i = 0; i < types.Length; i++) {
            if (types[i] == type) {
                return true;
            }
        }
        return false;
    }
}

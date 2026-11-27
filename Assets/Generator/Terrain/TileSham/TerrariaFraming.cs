using System;

/// <summary>
/// Правила соединения (tile framing) одного тайлсета оригинальной структуры Terraria.
///
/// Тайлсеты Tiles_0 (земля) и Tiles_1 (камень) устроены так же, как в Terraria:
/// кадр 16x16 px лежит в сетке с шагом 18 px (2 px паддинга), координаты кадра —
/// (frameX, frameY) в пикселях листа.
///
/// Классификация соседей (три слота), как в WorldGen.TileFrame:
///   • SlotSelf  — сосед того же типа (в оригинале значение == num);
///   • SlotMerge — сосед-«запрос слияния» (в оригинале маркер -2);
///   • SlotEdge  — пустота/инородный тип (в оригинале -1).
///
/// Для пары земля/камень в оригинале Terraria 1.4.4.9:
///   • земля (type 0) — ветка TileID.Sets.Dirt: камень НЕ слияние, а проверяется
///     взаимно (TileMergeAttemptFrametest): земля считает камень «своим» только
///     если камень сам сливается в её сторону;
///   • камень (type 1) — ветка Main.tileMergeDirt: земля становится -2 (слияние).
/// </summary>
public sealed class TerrariaTilesetRules {
    /// <summary>Типы, которые считаются «своими» (SlotSelf: без границы между собой).</summary>
    public readonly short[] SelfTypes;

    /// <summary>Типы-соседи, запрашивающие слияние (SlotMerge: кадры слияния).</summary>
    public readonly short[] MergeTypes;

    /// <summary>
    /// Типы-соседи, для которых нужна взаимная проверка: сосед считается «своим»,
    /// только если он сам при кадрировании сливается в нашу сторону
    /// (в оригинале — TileMergeAttemptFrametest для dirt-ветки).
    /// </summary>
    public readonly short[] MutualMergeCheckTypes;

    /// <summary>Правила соседа при взаимной проверке. null — проверка не нужна.</summary>
    public readonly TerrariaTilesetRules MutualMergeCheckRules;

    /// <summary>
    /// Участвует ли тайл в merge-блоках оригинала (Main.tileMergeDirt[num]).
    /// </summary>
    public readonly bool ParticipatesInMergeBlocks;

    /// <summary>Тип относится к «травяному» семейству (в оригинале для него пропускаются кадры 234-й колонки).</summary>
    public readonly bool IsGrassLike;

    /// <summary>
    /// Диагональный сосед из MutualMergeCheckTypes считается «своим» БЕЗ проверки
    /// встречного слияния (в оригинале dirt-ветка просто делает upLeft = num).
    /// </summary>
    public readonly bool MutualMergeCheckDiagonalIsSelf;

    public TerrariaTilesetRules(
        short[] selfTypes,
        short[] mergeTypes,
        short[] mutualMergeCheckTypes,
        TerrariaTilesetRules mutualMergeCheckRules,
        bool participatesInMergeBlocks,
        bool isGrassLike,
        bool mutualMergeCheckDiagonalIsSelf = false) {
        SelfTypes = selfTypes ?? EmptyTypes;
        MergeTypes = mergeTypes ?? EmptyTypes;
        MutualMergeCheckTypes = mutualMergeCheckTypes ?? EmptyTypes;
        MutualMergeCheckRules = mutualMergeCheckRules;
        ParticipatesInMergeBlocks = participatesInMergeBlocks;
        IsGrassLike = isGrassLike;
        MutualMergeCheckDiagonalIsSelf = mutualMergeCheckDiagonalIsSelf;
        // Каждый правиловый набор регистрируется автоматически: типы SelfTypes/MergeTypes
        // становятся «твёрдыми» для IsTerrainSolid (см. ниже). Новый тайлсет того же
        // формата добавляет только свой набор правил — отдельная логика не нужна.
        RegisteredRules.Add(this);
    }

    private static readonly short[] EmptyTypes = new short[0];

    /// <summary>
    /// Регистр всех созданных наборов правил Terraria-структуры.
    /// ОБЪЯВЛЕН РАНЬШЕ инстансов (Stone/Dirt/Terrain8 …): статические поля
    /// инициализируются по порядку объявления, поэтому список готов к тому
    /// моменту, когда конструкторы наборов добавляют себя в него. Новый
    /// тайлсет того же формата добавляет ТОЛЬКО свой набор правил:
    /// решатель кадров и «твёрдость» типа работают для него автоматически.
    /// </summary>
    private static readonly System.Collections.Generic.List<TerrariaTilesetRules> RegisteredRules =
        new System.Collections.Generic.List<TerrariaTilesetRules>();

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
        // Типы зарегистрированных правил Terraria-структуры «твёрдые» автоматически:
        // сам тип тайлсета и типы, с которыми он сливается. Новый тайлсет той же
        // структуры попадает сюда без правок движка.
        for (int r = 0; r < RegisteredRules.Count; r++) {
            var rules = RegisteredRules[r];
            for (int i = 0; i < rules.SelfTypes.Length; i++) {
                if (rules.SelfTypes[i] == type) {
                    return true;
                }
            }
            for (int i = 0; i < rules.MergeTypes.Length; i++) {
                if (rules.MergeTypes[i] == type) {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Правила тайлсета, которому принадлежит тип (ищет по SelfTypes в регистре).
    /// Нужно для взаимной проверки: земля проверяет сосед-камень по правилам камня,
    /// сосед-руда по правилам руды — без отдельной логики на каждый тип.
    /// null — тип не относится ни к одному Terraria-набору (для него взаимодействие
    /// не проверяется, работаем по MutualMergeCheckRules вызывающей стороны).
    /// </summary>
    public static TerrariaTilesetRules FindRulesForType(short type) {
        for (int r = 0; r < RegisteredRules.Count; r++) {
            var rules = RegisteredRules[r];
            for (int i = 0; i < rules.SelfTypes.Length; i++) {
                if (rules.SelfTypes[i] == type) {
                    return rules;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Камень (Tiles_1, type 1): камень-камень → SlotSelf, земля/руда → SlotMerge
    /// (Main.tileMergeDirt[1] = true, TileMergeAttempt(-2, TileID.Sets.Dirt), в
    /// Terraria руда с Main.tileStone распознаётся камнем как своя семья слияния).
    /// </summary>
    public static readonly TerrariaTilesetRules Stone = new TerrariaTilesetRules(
        selfTypes: new[] { (short) TileTypeEnum.Stone },
        mergeTypes: new[] { (short) TileTypeEnum.Terrain0, (short) TileTypeEnum.TerrainGrass, (short) TileTypeEnum.Terrain8 },
        mutualMergeCheckTypes: null,
        mutualMergeCheckRules: null,
        participatesInMergeBlocks: true,
        isGrassLike: false);

    /// <summary>
    /// Земля (Tiles_0, type 0): земля-земля → SlotSelf; stone/руда → взаимная проверка
    /// (TileID.Sets.Dirt-ветка, TileMergeAttemptFrametest); иначе SlotEdge.
    /// В merge-блоках не участвует (Main.tileMergeDirt[0] = false).
    /// Правила соседа ищутся по его типу через FindRulesForType — взаимная проверка
    /// работает для любого merge-тайлсета той же структуры, без правки dirt-логики.
    /// </summary>
    public static readonly TerrariaTilesetRules Dirt = new TerrariaTilesetRules(
        selfTypes: new[] { (short) TileTypeEnum.Terrain0, (short) TileTypeEnum.TerrainGrass },
        mergeTypes: null,
        mutualMergeCheckTypes: new[] { (short) TileTypeEnum.Stone, (short) TileTypeEnum.Terrain8 },
        mutualMergeCheckRules: Stone,
        participatesInMergeBlocks: false,
        isGrassLike: false,
        mutualMergeCheckDiagonalIsSelf: true);

    /// <summary>
    /// Tiles_8 (руда): структура листа точно такая же, как у камня (288×270,
    /// шаг 18 px, все варианты соединений), поэтому правила полностью зеркальны камню:
    /// руда-руда → SlotSelf, земля/трава/камень → SlotMerge (взаимно: камень тоже
    /// держит руду в своих MergeTypes), участие в merge-блоках — как у Main.tileMergeDirt.
    /// Решатель один и тот же; тип автоматически «твёрдый» через регистр правил.
    /// </summary>
    public static readonly TerrariaTilesetRules Terrain8 = new TerrariaTilesetRules(
        selfTypes: new[] { (short) TileTypeEnum.Terrain8 },
        mergeTypes: new[] { (short) TileTypeEnum.Terrain0, (short) TileTypeEnum.TerrainGrass, (short) TileTypeEnum.Stone },
        mutualMergeCheckTypes: null,
        mutualMergeCheckRules: null,
        participatesInMergeBlocks: true,
        isGrassLike: false);
}

/// <summary>
/// Решатель кадров, точно повторяющий WorldGen.TileFrame (Terraria 1.4.4.9)
/// для обычных («не frameImportant») тайлов ветки Main.tileMergeDirt.
///
/// ВАЖНО: все сравнения соседей — строгие, как в оригинале:
///   `up == num`  →  `up == SlotSelf`
///   `up == -2`   →  `up == SlotMerge`
///   `up == -1`   →  `up == SlotEdge`
///   `up != num`  →  `up != SlotSelf`   (Merge и Edge одинаково «не num»)
/// В частности, сосед-SlotMerge в generic-блоке НЕ считается соединённым —
/// он попадает в те же условия, что и пустота. Именно это определяет кадры
/// на концах тонких полос камня в земле.
/// </summary>
public static class TerrariaFraming {
    /// <summary>Сосед того же типа (в оригинале == num).</summary>
    public const int SlotSelf = 1;

    /// <summary>Сосед-запрос слияния (в оригинале маркер -2).</summary>
    public const int SlotMerge = -2;

    /// <summary>Сосед-граница: пустота или инородный тип (в оригинале -1).</summary>
    public const int SlotEdge = -1;

    /// <summary>Шаг сетки кадров в пикселях листа Terraria (16 px кадр + 2 px паддинг).</summary>
    public const int FrameStride = 18;

    /// <summary>Классификация одного соседа по типу.</summary>
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
        return SlotEdge;
    }

    /// <summary>
    /// Классификация 8 соседей вокруг клетки (cx, cy) прямо из карты.
    /// map[x, y]: x — вправо, y — вниз (y = 0 — верхняя строка окна).
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
    /// variant — вариант текстуры 0..2 (в оригинале num27 / tile.frameNumber()).
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

        if (rules.ParticipatesInMergeBlocks) {
            if (up != SlotEdge && down != SlotEdge && left != SlotEdge && right != SlotEdge) {
                // ===== Блок 1 (WorldGen.cs 75580–75958): все 4 стороны != -1 =====
                if (up == SlotMerge && down == SlotSelf && left == SlotSelf && right == SlotSelf) {
                    frameX = 144 + 18 * v; frameY = 108; mergeUp = true;          // 75582
                } else if (up == SlotSelf && down == SlotMerge && left == SlotSelf && right == SlotSelf) {
                    frameX = 144 + 18 * v; frameY = 90; mergeDown = true;         // 75601
                } else if (up == SlotSelf && down == SlotSelf && left == SlotMerge && right == SlotSelf) {
                    frameX = 162; frameY = 126 + 18 * v; mergeLeft = true;        // 75620
                } else if (up == SlotSelf && down == SlotSelf && left == SlotSelf && right == SlotMerge) {
                    frameX = 144; frameY = 126 + 18 * v; mergeRight = true;       // 75639
                } else if (up == SlotMerge && down == SlotSelf && left == SlotMerge && right == SlotSelf) {
                    frameX = 36; frameY = 90 + 36 * v; mergeUp = true; mergeLeft = true;    // 75658
                } else if (up == SlotMerge && down == SlotSelf && left == SlotSelf && right == SlotMerge) {
                    frameX = 54; frameY = 90 + 36 * v; mergeUp = true; mergeRight = true;   // 75678
                } else if (up == SlotSelf && down == SlotMerge && left == SlotMerge && right == SlotSelf) {
                    frameX = 36; frameY = 108 + 36 * v; mergeDown = true; mergeLeft = true; // 75698
                } else if (up == SlotSelf && down == SlotMerge && left == SlotSelf && right == SlotMerge) {
                    frameX = 54; frameY = 108 + 36 * v; mergeDown = true; mergeRight = true;// 75718
                } else if (up == SlotSelf && down == SlotSelf && left == SlotMerge && right == SlotMerge) {
                    frameX = 180; frameY = 126 + 18 * v; mergeLeft = true; mergeRight = true; // 75738
                } else if (up == SlotMerge && down == SlotMerge && left == SlotSelf && right == SlotSelf) {
                    frameX = 144 + 18 * v; frameY = 180; mergeUp = true; mergeDown = true;    // 75758
                } else if (up == SlotMerge && down == SlotSelf && left == SlotMerge && right == SlotMerge) {
                    frameX = 198; frameY = 90 + 18 * v;                                   // 75778
                    mergeUp = true; mergeLeft = true; mergeRight = true;
                } else if (up == SlotSelf && down == SlotMerge && left == SlotMerge && right == SlotMerge) {
                    frameX = 198; frameY = 144 + 18 * v;                                  // 75799
                    mergeDown = true; mergeLeft = true; mergeRight = true;
                } else if (up == SlotMerge && down == SlotMerge && left == SlotSelf && right == SlotMerge) {
                    frameX = 216; frameY = 144 + 18 * v;                                  // 75820
                    mergeUp = true; mergeDown = true; mergeRight = true;
                } else if (up == SlotMerge && down == SlotMerge && left == SlotMerge && right == SlotSelf) {
                    frameX = 216; frameY = 90 + 18 * v;                                   // 75841
                    mergeUp = true; mergeDown = true; mergeLeft = true;
                } else if (up == SlotMerge && down == SlotMerge && left == SlotMerge && right == SlotMerge) {
                    frameX = 108 + 18 * v; frameY = 198;                                  // 75862
                    mergeUp = true; mergeDown = true; mergeLeft = true; mergeRight = true;
                } else if (up == SlotSelf && down == SlotSelf && left == SlotSelf && right == SlotSelf) {
                    // 75884–75958: внутренние углы по диагоналям; последовательные if,
                    // побеждает ПОСЛЕДНЯЯ подошедшая диагональ.
                    if (upLeft == SlotMerge) {
                        frameX = 18; frameY = 108 + 36 * v; selected = true;
                    }
                    if (upRight == SlotMerge) {
                        frameX = 0; frameY = 108 + 36 * v; selected = true;
                    }
                    if (downLeft == SlotMerge) {
                        frameX = 18; frameY = 90 + 36 * v; selected = true;
                    }
                    if (downRight == SlotMerge) {
                        frameX = 0; frameY = 90 + 36 * v; selected = true;
                    }
                    // Если ни одна диагональ не сливается — кадр не назначается (уходим в Блок 3).
                }

                if (!selected && frameX >= 0 && frameY >= 0) {
                    selected = true;
                }
            } else if (!rules.IsGrassLike) {
                // ===== Блок 2 (WorldGen.cs 75960–76403): хотя бы одна сторона == -1 =====

                // --- 75962–76040: кадры 234-й колонки (стороны строго == num) ---
                if (up == SlotEdge && down == SlotMerge && left == SlotSelf && right == SlotSelf) {
                    frameX = 234 + 18 * v; frameY = 0; mergeDown = true; selected = true;   // 75964
                } else if (up == SlotMerge && down == SlotEdge && left == SlotSelf && right == SlotSelf) {
                    frameX = 234 + 18 * v; frameY = 18; mergeUp = true; selected = true;    // 75983
                } else if (up == SlotSelf && down == SlotSelf && left == SlotEdge && right == SlotMerge) {
                    frameX = 234 + 18 * v; frameY = 36; mergeRight = true; selected = true; // 76002
                } else if (up == SlotSelf && down == SlotSelf && left == SlotMerge && right == SlotEdge) {
                    frameX = 234 + 18 * v; frameY = 54; mergeLeft = true; selected = true;  // 76021
                }

                // --- 76041–76122: открыта левая/правая сторона, противоположная == num ---
                if (!selected && up != SlotEdge && down != SlotEdge && left == SlotEdge && right == SlotSelf) {
                    if (up == SlotMerge && down == SlotSelf) {
                        frameX = 72; frameY = 144 + 18 * v; mergeUp = true; selected = true;    // 76043
                    } else if (down == SlotMerge && up == SlotSelf) {
                        frameX = 72; frameY = 90 + 18 * v; mergeDown = true; selected = true;   // 76062
                    }
                } else if (!selected && up != SlotEdge && down != SlotEdge && left == SlotSelf && right == SlotEdge) {
                    if (up == SlotMerge && down == SlotSelf) {
                        frameX = 90; frameY = 144 + 18 * v; mergeUp = true; selected = true;    // 76084
                    } else if (down == SlotMerge && up == SlotSelf) {
                        frameX = 90; frameY = 90 + 18 * v; mergeDown = true; selected = true;   // 76103
                    }
                }
                // --- 76123–76204: открыт верх/низ, горизонтальные слияния ---
                else if (!selected && up == SlotEdge && down == SlotSelf && left != SlotEdge && right != SlotEdge) {
                    if (left == SlotMerge && right == SlotSelf) {
                        frameX = 0 + 18 * v; frameY = 198; mergeLeft = true; selected = true;   // 76125
                    } else if (right == SlotMerge && left == SlotSelf) {
                        frameX = 54 + 18 * v; frameY = 198; mergeRight = true; selected = true; // 76144
                    }
                } else if (!selected && up == SlotSelf && down == SlotEdge && left != SlotEdge && right != SlotEdge) {
                    if (left == SlotMerge && right == SlotSelf) {
                        frameX = 0 + 18 * v; frameY = 216; mergeLeft = true; selected = true;   // 76166
                    } else if (right == SlotMerge && left == SlotSelf) {
                        frameX = 54 + 18 * v; frameY = 216; mergeRight = true; selected = true; // 76185
                    }
                }
                // --- 76205–76326: два слияния при двух открытых сторонах ---
                else if (!selected && up != SlotEdge && down != SlotEdge && left == SlotEdge && right == SlotEdge) {
                    if (up == SlotMerge && down == SlotMerge) {
                        frameX = 108; frameY = 216 + 18 * v;                                    // 76207
                        mergeUp = true; mergeDown = true; selected = true;
                    } else if (up == SlotMerge) {
                        frameX = 126; frameY = 144 + 18 * v; mergeUp = true; selected = true;   // 76227
                    } else if (down == SlotMerge) {
                        frameX = 126; frameY = 90 + 18 * v; mergeDown = true; selected = true;  // 76246
                    }
                } else if (!selected && up == SlotEdge && down == SlotEdge && left != SlotEdge && right != SlotEdge) {
                    if (left == SlotMerge && right == SlotMerge) {
                        frameX = 162 + 18 * v; frameY = 198;                                    // 76268
                        mergeLeft = true; mergeRight = true; selected = true;
                    } else if (left == SlotMerge) {
                        frameX = 0 + 18 * v; frameY = 252; mergeLeft = true; selected = true;   // 76288
                    } else if (right == SlotMerge) {
                        frameX = 54 + 18 * v; frameY = 252; mergeRight = true; selected = true; // 76307
                    }
                }
                // --- 76327–76402: одно слияние при трёх открытых сторонах ---
                else if (!selected && up == SlotMerge && down == SlotEdge && left == SlotEdge && right == SlotEdge) {
                    frameX = 108; frameY = 144 + 18 * v; mergeUp = true; selected = true;       // 76327
                } else if (!selected && up == SlotEdge && down == SlotMerge && left == SlotEdge && right == SlotEdge) {
                    frameX = 108; frameY = 90 + 18 * v; mergeDown = true; selected = true;      // 76346
                } else if (!selected && up == SlotEdge && down == SlotEdge && left == SlotMerge && right == SlotEdge) {
                    frameX = 0 + 18 * v; frameY = 234; mergeLeft = true; selected = true;       // 76365
                } else if (!selected && up == SlotEdge && down == SlotEdge && left == SlotEdge && right == SlotMerge) {
                    frameX = 54 + 18 * v; frameY = 234; mergeRight = true; selected = true;     // 76384
                }
            }
        }

        // ===== Блок 3 (WorldGen.cs 76551–76927): generic, строгое сравнение с num =====
        if (!selected) {
            if (up == SlotSelf && down == SlotSelf && left == SlotSelf && right == SlotSelf) {
                if (upLeft != SlotSelf && upRight != SlotSelf) {
                    frameX = 108 + 18 * v; frameY = 18;               // 76566
                } else if (downLeft != SlotSelf && downRight != SlotSelf) {
                    frameX = 108 + 18 * v; frameY = 36;               // 76584
                } else if (upLeft != SlotSelf && downLeft != SlotSelf) {
                    frameX = 180; frameY = 18 * v;                    // 76602
                } else if (upRight != SlotSelf && downRight != SlotSelf) {
                    frameX = 198; frameY = 18 * v;                    // 76620
                } else {
                    frameX = 18 + 18 * v; frameY = 18;                // 76638
                }
            } else if (up != SlotSelf && down == SlotSelf && left == SlotSelf && right == SlotSelf) {
                frameX = 18 + 18 * v; frameY = 0;                     // 76657 верхний край
            } else if (up == SlotSelf && down != SlotSelf && left == SlotSelf && right == SlotSelf) {
                frameX = 18 + 18 * v; frameY = 36;                    // 76675 нижний край
            } else if (up == SlotSelf && down == SlotSelf && left != SlotSelf && right == SlotSelf) {
                frameX = 0; frameY = 18 * v;                          // 76693 левый край
            } else if (up == SlotSelf && down == SlotSelf && left == SlotSelf && right != SlotSelf) {
                frameX = 72; frameY = 18 * v;                         // 76711 правый край
            } else if (up != SlotSelf && down == SlotSelf && left != SlotSelf && right == SlotSelf) {
                frameX = 36 * v; frameY = 54;                         // 76729 угол сверху-слева
            } else if (up != SlotSelf && down == SlotSelf && left == SlotSelf && right != SlotSelf) {
                frameX = 18 + 36 * v; frameY = 54;                    // 76747 угол сверху-справа
            } else if (up == SlotSelf && down != SlotSelf && left != SlotSelf && right == SlotSelf) {
                frameX = 36 * v; frameY = 72;                         // 76765 угол снизу-слева
            } else if (up == SlotSelf && down != SlotSelf && left == SlotSelf && right != SlotSelf) {
                frameX = 18 + 36 * v; frameY = 72;                    // 76783 угол снизу-справа
            } else if (up == SlotSelf && down == SlotSelf && left != SlotSelf && right != SlotSelf) {
                frameX = 90; frameY = 18 * v;                         // 76801 горизонтальная полоса
            } else if (up != SlotSelf && down != SlotSelf && left == SlotSelf && right == SlotSelf) {
                frameX = 108 + 18 * v; frameY = 72;                   // 76819 вертикальная полоса
            } else if (up != SlotSelf && down == SlotSelf && left != SlotSelf && right != SlotSelf) {
                frameX = 108 + 18 * v; frameY = 0;                    // 76837 «шапка» сверху
            } else if (up == SlotSelf && down != SlotSelf && left != SlotSelf && right != SlotSelf) {
                frameX = 108 + 18 * v; frameY = 54;                   // 76855 «шапка» снизу
            } else if (up != SlotSelf && down != SlotSelf && left != SlotSelf && right == SlotSelf) {
                frameX = 162; frameY = 18 * v;                        // 76873 (наш случай полосы)
            } else if (up != SlotSelf && down != SlotSelf && left == SlotSelf && right != SlotSelf) {
                frameX = 216; frameY = 18 * v;                        // 76891
            } else {
                frameX = 162 + 18 * v; frameY = 54;                   // 76909 одиночный блок
            }
            selected = true;
        }

        // ===== Блок 4 (WorldGen.cs 76928–76945): безопасный вариант =====
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

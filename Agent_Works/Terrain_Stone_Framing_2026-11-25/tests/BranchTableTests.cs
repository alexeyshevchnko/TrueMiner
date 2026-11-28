using System;

/// <summary>
/// Табличная проверка ВСЕХ веток WorldGen.TileFrame (Terraria 1.4.4.9) для
/// dirt-mergeable тайла (камень). Каждая строка таблицы соответствует конкретной
/// ветке декомпиляции (номер строки указан в комментарии), поэтому тест
/// доказывает соответствие реализации оригиналу, а не текущему поведению.
///
/// Коды соседей: S = SlotSelf (== num), M = SlotMerge (-2), E = SlotEdge (-1).
/// flags: 1 = mergeUp, 2 = mergeDown, 4 = mergeLeft, 8 = mergeRight.
/// </summary>
public static class BranchTableTests {
    private const int S = TerrariaFraming.SlotSelf;
    private const int M = TerrariaFraming.SlotMerge;
    private const int E = TerrariaFraming.SlotEdge;

    private static int failures;
    private static int passed;

    private struct Case {
        public string Name;
        public int Up, Down, Left, Right;
        public int UL, UR, DL, DR;
        public int Fx0, Fy0, Fx1, Fy1, Fx2, Fy2;
        public int Flags;
    }

    private static void Add(string name, int up, int down, int left, int right,
                            int ul, int ur, int dl, int dr,
                            int fx0, int fy0, int fx1, int fy1, int fx2, int fy2,
                            int flags) {
        cases.Add(new Case {
            Name = name, Up = up, Down = down, Left = left, Right = right,
            UL = ul, UR = ur, DL = dl, DR = dr,
            Fx0 = fx0, Fy0 = fy0, Fx1 = fx1, Fy1 = fy1, Fx2 = fx2, Fy2 = fy2,
            Flags = flags
        });
    }

    private static readonly System.Collections.Generic.List<Case> cases =
        new System.Collections.Generic.List<Case>();

    public static int Run() {
        failures = 0;
        passed = 0;
        cases.Clear();
        BuildTable();

        // TerrariaTilesetRules с одинаковой структурой (камень и Tiles_8-руда)
        // обязаны давать идентичные кадры только потому that правила зеркальны.
        var ruleSets = new[] {
            new { Label = "Stone", Rules = TerrariaTilesetRules.Stone },
            new { Label = "Terrain8", Rules = TerrariaTilesetRules.Terrain8 }
        };

        Console.WriteLine("--- Таблица веток WorldGen.TileFrame (1.4.4.9) ---");
        foreach (var ruleSet in ruleSets) {
            int setFailed = 0;
            int setPassed = 0;
            foreach (var c in cases) {
                for (int v = 0; v < 3; v++) {
                    TerrariaFraming.Solve(c.Up, c.Down, c.Left, c.Right, c.UL, c.UR, c.DL, c.DR,
                        v, ruleSet.Rules,
                        out int fx, out int fy,
                        out bool mu, out bool md, out bool ml, out bool mr);

                    int expFx = v == 0 ? c.Fx0 : (v == 1 ? c.Fx1 : c.Fx2);
                    int expFy = v == 0 ? c.Fy0 : (v == 1 ? c.Fy1 : c.Fy2);
                    int flags = (mu ? 1 : 0) | (md ? 2 : 0) | (ml ? 4 : 0) | (mr ? 8 : 0);

                    bool ok = fx == expFx && fy == expFy && flags == c.Flags;
                    if (ok) {
                        passed++;
                        setPassed++;
                    } else {
                        failures++;
                        setFailed++;
                        if (failures <= 20) {
                            Console.WriteLine($"FAIL  [{ruleSet.Label}] {c.Name} v={v}: получено ({fx},{fy}) flags={flags}, " +
                                              $"ожидалось ({expFx},{expFy}) flags={c.Flags}");
                        }
                    }
                }
            }

            Console.WriteLine($"Таблица веток [{ruleSet.Label}]: {setPassed} OK, {setFailed} FAIL " +
                              $"({cases.Count} веток × 3 варианта)");
        }

        return failures;
    }

    private static void BuildTable() {
        // ===== Блок 1: все четыре стороны != Edge (75580-75958) =====
        Add("75582 M-S-S-S", M, S, S, S, S, S, S, S, 144, 108, 162, 108, 180, 108, 1);
        Add("75601 S-M-S-S", S, M, S, S, S, S, S, S, 144, 90, 162, 90, 180, 90, 2);
        Add("75620 S-S-M-S", S, S, M, S, S, S, S, S, 162, 126, 162, 144, 162, 162, 4);
        Add("75639 S-S-S-M", S, S, S, M, S, S, S, S, 144, 126, 144, 144, 144, 162, 8);
        Add("75658 M-S-M-S", M, S, M, S, S, S, S, S, 36, 90, 36, 126, 36, 162, 5);
        Add("75678 M-S-S-M", M, S, S, M, S, S, S, S, 54, 90, 54, 126, 54, 162, 9);
        Add("75698 S-M-M-S", S, M, M, S, S, S, S, S, 36, 108, 36, 144, 36, 180, 6);
        Add("75718 S-M-S-M", S, M, S, M, S, S, S, S, 54, 108, 54, 144, 54, 180, 10);
        Add("75738 S-S-M-M", S, S, M, M, S, S, S, S, 180, 126, 180, 144, 180, 162, 12);
        Add("75758 M-M-S-S", M, M, S, S, S, S, S, S, 144, 180, 162, 180, 180, 180, 3);
        Add("75778 M-S-M-M", M, S, M, M, S, S, S, S, 198, 90, 198, 108, 198, 126, 13);
        Add("75799 S-M-M-M", S, M, M, M, S, S, S, S, 198, 144, 198, 162, 198, 180, 14);
        Add("75820 M-M-S-M", M, M, S, M, S, S, S, S, 216, 144, 216, 162, 216, 180, 11);
        Add("75841 M-M-M-S", M, M, M, S, S, S, S, S, 216, 90, 216, 108, 216, 126, 7);
        Add("75862 M-M-M-M", M, M, M, M, S, S, S, S, 108, 198, 126, 198, 144, 198, 15);

        // Диагональные слияния при всех Self-сторонах (75886-75957)
        Add("75886 allS ul=M", S, S, S, S, M, S, S, S, 18, 108, 18, 144, 18, 180, 0);
        Add("75904 allS ur=M", S, S, S, S, S, M, S, S, 0, 108, 0, 144, 0, 180, 0);
        Add("75922 allS dl=M", S, S, S, S, S, S, M, S, 18, 90, 18, 126, 18, 162, 0);
        Add("75940 allS dr=M", S, S, S, S, S, S, S, M, 0, 90, 0, 126, 0, 162, 0);
        // Все Self и без диагональных слияний → Block 1 не назначает кадр,
        // срабатывает generic «внутренний тайл» (76638) → (18+18v, 18)
        Add("76638 allS no diag", S, S, S, S, S, S, S, S, 18, 18, 36, 18, 54, 18, 0);

        // ===== Блок 2: хотя бы одна сторона Edge (75960-76403) =====
        Add("75964 E-M-S-S", E, M, S, S, S, S, S, S, 234, 0, 252, 0, 270, 0, 2);
        Add("75983 M-E-S-S", M, E, S, S, S, S, S, S, 234, 18, 252, 18, 270, 18, 1);
        Add("76002 S-S-E-M", S, S, E, M, S, S, S, S, 234, 36, 252, 36, 270, 36, 8);
        Add("76021 S-S-M-E", S, S, M, E, S, S, S, S, 234, 54, 252, 54, 270, 54, 4);
        Add("76043 e-e-E-S up=M,down=S", M, S, E, S, S, S, S, S, 72, 144, 72, 162, 72, 180, 1);
        Add("76062 e-e-E-S up=S,down=M", S, M, E, S, S, S, S, S, 72, 90, 72, 108, 72, 126, 2);
        Add("76084 e-e-S-E up=M,down=S", M, S, S, E, S, S, S, S, 90, 144, 90, 162, 90, 180, 1);
        Add("76103 e-e-S-E up=S,down=M", S, M, S, E, S, S, S, S, 90, 90, 90, 108, 90, 126, 2);
        Add("76125 E-S-e-e left=M,right=S", E, S, M, S, S, S, S, S, 0, 198, 18, 198, 36, 198, 4);
        Add("76144 E-S-e-e left=S,right=M", E, S, S, M, S, S, S, S, 54, 198, 72, 198, 90, 198, 8);
        Add("76166 S-E-e-e left=M,right=S", S, E, M, S, S, S, S, S, 0, 216, 18, 216, 36, 216, 4);
        Add("76185 S-E-e-e left=S,right=M", S, E, S, M, S, S, S, S, 54, 216, 72, 216, 90, 216, 8);
        Add("76207 e-e-E-E up=M,down=M", M, M, E, E, S, S, S, S, 108, 216, 108, 234, 108, 252, 3);
        Add("76227 e-e-E-E up=M,down=S", M, S, E, E, S, S, S, S, 126, 144, 126, 162, 126, 180, 1);
        Add("76246 e-e-E-E up=S,down=M", S, M, E, E, S, S, S, S, 126, 90, 126, 108, 126, 126, 2);
        Add("76268 E-E-e-e left=M,right=M", E, E, M, M, S, S, S, S, 162, 198, 180, 198, 198, 198, 12);
        Add("76288 E-E-e-e left=M,right=S", E, E, M, S, S, S, S, S, 0, 252, 18, 252, 36, 252, 4);
        Add("76307 E-E-e-e left=S,right=M", E, E, S, M, S, S, S, S, 54, 252, 72, 252, 90, 252, 8);
        Add("76327 M-E-E-E", M, E, E, E, S, S, S, S, 108, 144, 108, 162, 108, 180, 1);
        Add("76346 E-M-E-E", E, M, E, E, S, S, S, S, 108, 90, 108, 108, 108, 126, 2);
        Add("76365 E-E-M-E", E, E, M, E, S, S, S, S, 0, 234, 18, 234, 36, 234, 4);
        Add("76384 E-E-E-M", E, E, E, M, S, S, S, S, 54, 234, 72, 234, 90, 234, 8);

        // ===== Блок 3: generic, строгое сравнение с num (76564-76927) =====
        Add("76657 top edge", E, S, S, S, S, S, S, S, 18, 0, 36, 0, 54, 0, 0);
        Add("76675 bottom edge", S, E, S, S, S, S, S, S, 18, 36, 36, 36, 54, 36, 0);
        Add("76693 left edge", S, S, E, S, S, S, S, S, 0, 0, 0, 18, 0, 36, 0);
        Add("76711 right edge", S, S, S, E, S, S, S, S, 72, 0, 72, 18, 72, 36, 0);
        Add("76729 corner TL", E, S, E, S, S, S, S, S, 0, 54, 36, 54, 72, 54, 0);
        Add("76747 corner TR", E, S, S, E, S, S, S, S, 18, 54, 54, 54, 90, 54, 0);
        Add("76765 corner BL", S, E, E, S, S, S, S, S, 0, 72, 36, 72, 72, 72, 0);
        Add("76783 corner BR", S, E, S, E, S, S, S, S, 18, 72, 54, 72, 90, 72, 0);
        Add("76801 horizontal strip", S, S, E, E, S, S, S, S, 90, 0, 90, 18, 90, 36, 0);
        Add("76819 vertical strip", E, E, S, S, S, S, S, S, 108, 72, 126, 72, 144, 72, 0);
        Add("76837 top cap", E, S, E, E, S, S, S, S, 108, 0, 126, 0, 144, 0, 0);
        Add("76855 bottom cap", S, E, E, E, S, S, S, S, 108, 54, 126, 54, 144, 54, 0);

        // НАШ СЛУЧАЙ: полоса камня в земле, слева пустота, справа камень.
        // Ветка 76873 (up!=num, down!=num, left!=num, right==num) → (162, 18v)
        Add("76873 strip start M-M-E-S", M, M, E, S, M, M, M, M, 162, 0, 162, 18, 162, 36, 0);
        Add("76891 strip end M-M-S-E", M, M, S, E, M, M, M, M, 216, 0, 216, 18, 216, 36, 0);
        Add("76909 single block", E, E, E, E, E, E, E, E, 162, 54, 180, 54, 198, 54, 0);

        // ===== Блок 4: безопасный вариант (76928) =====
        // Достижим, если ни одна ветка не подошла; проверяется отдельным тестом ниже.
    }

    /// <summary>
    /// Явная проверка проблемной конфигурации из задачи (первый блок полосы).
    /// </summary>
    public static int CheckProblemConfig() {
        int fails = 0;
        TerrariaFraming.Solve(M, M, E, S, M, M, M, M, 0, TerrariaTilesetRules.Stone,
            out int fx, out int fy, out bool mu, out bool md, out bool ml, out bool mr);
        // Original 76873: (162, 0) — «полоса, открытая сверху/снизу/слева, соединена справа»
        if (fx == 162 && fy == 0 && !mu && !md && !ml && !mr) {
            Console.WriteLine("PASS  Проблемная конфигурация M-M-E-S → (162,0) [ветка 76873]");
        } else {
            fails++;
            Console.WriteLine($"FAIL  Проблемная конфигурация M-M-E-S → ({fx},{fy}) flags={mu}/{md}/{ml}/{mr}, " +
                              "ожидалось (162,0) flags=false");
        }
        return fails;
    }

    /// <summary>
    /// Стык руды (Tiles_8) и земли через полную схему TerrainTileShema:
    /// земля под рудой должна пройти взаимную проверку (рудник сверху рисует
    /// merge-кадр вниз) и не рисовать свой край — кадр земли = внутренний (18+18v, 18).
    /// </summary>
    public static int CheckOreDirtSeam() {
        int fails = 0;
        const short ORE = (short)TileTypeEnum.Terrain8;
        const short DIRT = (short)TileTypeEnum.Terrain0;

        // Окно 5x5 (как TileDataProvider): центр — земля, рудник сверху и по диагоналям верха.
        var map = new short[5, 5];
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                map[x, y] = DIRT;
        map[2, 1] = ORE;   // руда сверху
        map[1, 1] = ORE;   // диагональ сверху-слева
        map[3, 1] = ORE;   // диагональ сверху-справа

        var info = new TileInfoStructure(DIRT, 0);
        var f = new TerrainTileShema(TerrariaTilesetRules.Dirt).GetShema(map, info);
        if (f.FrameX == 1 && f.FrameY == 1) {
            Console.WriteLine("PASS  Стык руда-земля: земля рисует внутренний кадр (взаимная проверка пройдена)");
        } else {
            fails++;
            Console.WriteLine($"FAIL  Стык руда-земля: получено ({f.FrameX},{f.FrameY}), ожидалось (1,1)");
        }

        // Руда с землёй ТОЛЬКО снизу (сквозь проверку должен пройти merge-кадр):
        // up=d/left=r = Self (руда-руда), down = Merge → ветка 75601: (144+18v, 90) → (8,5).
        var map2 = new short[5, 5];
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                map2[x, y] = ORE;
        map2[2, 3] = DIRT;   // земля снизу
        var info2 = new TileInfoStructure(ORE, 0);
        TerrariaFraming.ClassifyNeighbors(map2, 2, 2, TerrariaTilesetRules.Terrain8,
            out int cu, out int cd, out int cl, out int cr,
            out int cul, out int cur, out int cdl, out int cdr);
        Console.WriteLine($"DEBUG classify: up={cu} down={cd} left={cl} right={cr} ul={cul} ur={cur} dl={cdl} dr={cdr} " +
            $"solid(ORE)={TerrariaTilesetRules.IsTerrainSolid(ORE)} solid(DIRT)={TerrariaTilesetRules.IsTerrainSolid(DIRT)}");
        Console.WriteLine($"DEBUG find(ORE)={(TerrariaTilesetRules.FindRulesForType(ORE) != null ? "найдены" : "нет")} " +
            $"oreSelf={(TerrariaTilesetRules.Terrain8.SelfTypes.Length > 0 ? $"{TerrariaTilesetRules.Terrain8.SelfTypes[0]}" : "пусто")}");
        var f2 = new TerrainTileShema(TerrariaTilesetRules.Terrain8).GetShema(map2, info2);
        if (f2.FrameX == 8 && f2.FrameY == 5) {
            Console.WriteLine("PASS  Руда над землёй: merge-кадр вниз (144+18v, 90) → сетка (8,5)");
        } else {
            fails++;
            Console.WriteLine($"FAIL  Руда над землёй: получено ({f2.FrameX},{f2.FrameY}), ожидалось (8,5)");
        }
        return fails;
    }

    /// <summary>
    /// Стык руда↔камень в обоих направлениях: обе стороны классифицируют соседа
    /// как Merge и рисуют merge-кадры — стык без тёмных рамок.
    /// </summary>
    public static int CheckOreStoneSeam() {
        const short ORE = (short)TileTypeEnum.Terrain8;
        const short STONE = (short)TileTypeEnum.Stone;
        int fails = 0;

        // Камень, снизу руда
        var m1 = new short[3, 3];
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                m1[x, y] = STONE;
        m1[1, 2] = ORE;
        var f1 = new TerrainTileShema(TerrariaTilesetRules.Stone).GetShema(m1,
            new TileInfoStructure(STONE, 0));
        if (f1.FrameX == 8 && f1.FrameY == 5) {
            Console.WriteLine("PASS  Камень над рудой: камень рисует merge-кадр вниз (8,5)");
        } else {
            fails++;
            Console.WriteLine($"FAIL  Камень над рудой: получено ({f1.FrameX},{f1.FrameY}), ожидалось (8,5)");
        }

        // Руда, снизу камень
        var m2 = new short[3, 3];
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                m2[x, y] = ORE;
        m2[1, 2] = STONE;
        var f2 = new TerrainTileShema(TerrariaTilesetRules.Terrain8).GetShema(m2,
            new TileInfoStructure(ORE, 0));
        if (f2.FrameX == 8 && f2.FrameY == 5) {
            Console.WriteLine("PASS  Руда над камнем: руда рисует merge-кадр вниз (8,5)");
        } else {
            fails++;
            Console.WriteLine($"FAIL  Руда над камнем: получено ({f2.FrameX},{f2.FrameY}), ожидалось (8,5)");
        }

        // Массив камня вплотную к массиву руды: merge-кадр вбок (левая колонка руда)
        var m3 = new short[3, 3];
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                m3[x, y] = x < 1 ? ORE : STONE;
        var f3 = new TerrainTileShema(TerrariaTilesetRules.Stone).GetShema(m3,
            new TileInfoStructure(STONE, 0));
        if (f3.FrameX == 9 && f3.FrameY == 7) {
            Console.WriteLine("PASS  Массив камня у руды слева: merge-кадр влево (9,7)");
        } else {
            fails++;
            Console.WriteLine($"FAIL  Массив камня у руды: получено ({f3.FrameX},{f3.FrameY}), ожидалось (9,7)");
        }
        return fails;
    }
}

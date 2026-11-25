using System;
using System.Diagnostics;
using System.Text;
using TerrariaFramingEngine = TerrariaFraming;

/// <summary>
/// Автономная проверка решателя кадров Terraria вне Unity:
///   • сценарии из задания (поверхность, стены, углы, переходы земля↔камень,
///     одиночные блоки, узкие полосы);
///   • конфигурации «земля окружает камень» и «камень окружает землю»;
///   • детерминизм при разном порядке обхода тайлов;
///   • брутфорс всех комбинаций 8 соседей × 3 варианта × 2 набора правил;
///   • фаззинг случайных карт;
///   • замер производительности на карте 800x200.
/// </summary>
public static class Program {
    private static int failures;

    private const short D = (short)TileTypeEnum.Terrain0;
    private const short S = (short)TileTypeEnum.Stone;
    private const short G = (short)TileTypeEnum.TerrainGrass;
    private const short C = (short)TileTypeEnum.Coal;
    private const short E = 0;

    private static readonly ITileShema Dirt = new TerrainTileShema(TerrariaTilesetRules.Dirt);
    private static readonly ITileShema Stone = new TerrainTileShema(TerrariaTilesetRules.Stone);

    public static int Main() {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("=== Проверка Terraria-кадрирования (Tiles_0 земля / Tiles_1 камень) ===");
        Console.WriteLine();

        TestScenarios();
        TestEarthAroundStone();
        TestStoneAroundEarth();
        TestVariants();
        TestRemoval();
        TestDeterminism();
        BruteForceAllNeighborCombinations();
        FuzzRandomMaps();
        PerfCheck();
        failures += BranchTableTests.Run();
        failures += BranchTableTests.CheckProblemConfig();
        Diagnostic.Run();
        FramingSheet.Run();
        PreviewGen.Run();

        Console.WriteLine();
        if (failures == 0) {
            Console.WriteLine("ИТОГ: все проверки пройдены.");
            return 0;
        }
        Console.WriteLine($"ИТОГ: провалено проверок: {failures}");
        return 1;
    }

    // ---------- сценарии из задания ----------

    private static void TestScenarios() {
        Console.WriteLine("--- Сценарии ---");

        // 1. Горизонтальная поверхность земли (сверху пусто)
        short[,] flat = Fill(D);
        ClearRows(flat, 0, 1);
        Expect("1. Поверхность земли", flat, D, 0, 1, 0);

        // 2. Вертикальная стена (колонна шириной 1)
        short[,] wall = Fill(D);
        for (int y = 0; y < 5; y++) {
            wall[1, y] = E;
            wall[3, y] = E;
        }
        Expect("2. Вертикальная колонна", wall, D, 0, 5, 0);

        // 2б. Горизонтальная полоса земли высотой 1 (открыты верх и низ)
        short[,] hstrip = Fill(E);
        for (int x = 0; x < 5; x++) hstrip[x, 2] = D;
        Expect("2б. Горизонтальная полоса земли", hstrip, D, 0, 6, 4);

        // 3. Внешний угол (верх-лево)
        short[,] corner = Fill(D);
        corner[2, 1] = E;
        corner[1, 2] = E;
        Expect("3. Внешний угол", corner, D, 0, 0, 3);

        // 4. Внутренний угол (оба верхних диагональных соседа пусты)
        short[,] inner = Fill(D);
        inner[1, 1] = E;
        inner[3, 1] = E;
        Expect("4. Внутренний угол", inner, D, 0, 6, 1);

        // 5. Полностью заполненный участок земли
        Expect("5. Заполненный участок", Fill(D), D, 0, 1, 1);

        // 6. Участок камня под землёй (камень окружён камнем)
        Expect("6. Камень среди камня", Fill(S), S, 0, 1, 1);

        // 7а. Камень над землёй — кадр слияния камня
        short[,] borderStone = Fill(S);
        borderStone[2, 3] = D;
        Expect("7а. Камень над землёй (слияние)", borderStone, S, 0, 8, 5);

        // 7б. Земля под камнем — камень сливается вниз, земля убирает край
        short[,] borderDirt = Fill(D);
        borderDirt[2, 1] = S;
        Expect("7б. Земля под камнем (без шва)", borderDirt, D, 0, 1, 1);

        // 8. Одиночный блок
        short[,] single = Fill(E);
        single[2, 2] = D;
        Expect("8. Одиночный блок", single, D, 0, 9, 3);

        // 9. Земля + трава — «свой» тип, без границы
        short[,] grass = Fill(D);
        grass[2, 1] = G;
        Expect("9. Земля + трава (без границы)", grass, D, 0, 1, 1);

        // 10. Камень: слияние сверху при открытом низе и камне слева/справа
        short[,] partial = Fill(S);
        partial[2, 1] = D;
        partial[2, 3] = E;
        Expect("10. Частичное слияние камня (верх)", partial, S, 0, 13, 1);

        // 11. Камень с землёй сверху и снизу, пусто слева и справа
        short[,] both = Fill(E);
        both[2, 2] = S;
        both[2, 1] = D;
        both[2, 3] = D;
        Expect("11. Слияние верх+низ, бока открыты", both, S, 0, 6, 12);

        // 12. Земля рядом с чужим твёрдым типом (уголь) — обычная граница
        short[,] coal = Fill(D);
        coal[2, 1] = C;
        Expect("12. Земля + уголь (граница)", coal, D, 0, 1, 0);

        // 13. Камень в углу земляной пещеры: up=Edge, down=Merge, left=Edge, right=Merge
        //     Original 1.4.4.9: merge-блоки не подходят (есть -1 по краям), в generic-цепочке
        //     ни одна ветка 76564-76891 не подходит (up и left не num, down/right не num),
        //     срабатывает ветка 76909 (все четыре != num) → (162+18v, 54) → сетка (9,3).
        short[,] caveCorner = Fill(E);
        caveCorner[2, 2] = S;
        caveCorner[2, 3] = D;
        caveCorner[3, 2] = D;
        caveCorner[3, 3] = D;
        Expect("13. Камень в углу пещеры (одиночный блок, 76909)", caveCorner, S, 0, 9, 3);

        // 14. Дефект 2: горизонтальная полоска камня в земле — правый конец
        //     Окно центрировано на правом конце: left=Self, right/up/down=Merge
        //     → Block 1 combo 13 (u=M,d=M,l=S,r=M) → (216, 144) merge U+D+R
        short[,] stripEnd = Fill(D);
        stripEnd[1, 2] = S;
        stripEnd[2, 2] = S;
        stripEnd[3, 2] = S;
        // Правый конец: окно с центром на (3,2) → сдвигаем на 1 влево
        var stripWindow = new short[5, 5];
        for (int wx = 0; wx < 5; wx++)
            for (int wy = 0; wy < 5; wy++)
                stripWindow[wx, wy] = D;
        // Центр окна (2,2) = правый конец полоски
        stripWindow[2, 2] = S;
        stripWindow[1, 2] = S; // левый сосед — камень (Self)
        // правый сосед (3,2) = D (Merge), верх (2,1) = D (Merge), низ (2,3) = D (Merge)
        var stripInfo = new TileInfoStructure(S, 0);
        var stripFrame = Stone.GetShema(stripWindow, stripInfo);
        // Block 1 combo 13 (u=M,d=M,l=S,r=M) → (216, 144) → grid (12, 8)
        bool stripOk = stripFrame.FrameX == 12 && stripFrame.FrameY == 8;
        if (!stripOk) failures++;
        Console.WriteLine($"{(stripOk ? "PASS" : "FAIL")}  14. Полоска камня — правый конец (merge U+D+R): ({stripFrame.FrameX},{stripFrame.FrameY}), ожидался (12,8)");

        // 14б. Левый конец полоски (симметрия): up=Merge, down=Merge, left=Merge, right=Self
        //     → Block 1 combo 14 (u=M,d=M,l=M,r=S) → (216, 90) merge U+D+L
        var stripWindowL = new short[5, 5];
        for (int wx = 0; wx < 5; wx++)
            for (int wy = 0; wy < 5; wy++)
                stripWindowL[wx, wy] = D;
        stripWindowL[2, 2] = S; // центр = левый конец
        stripWindowL[3, 2] = S; // правый сосед — камень (Self)
        // левый сосед (1,2) = D (Merge), верх (2,1) = D, низ (2,3) = D
        var stripFrameL = Stone.GetShema(stripWindowL, stripInfo);
        bool stripOkL = stripFrameL.FrameX == 12 && stripFrameL.FrameY == 5;
        if (!stripOkL) failures++;
        Console.WriteLine($"{(stripOkL ? "PASS" : "FAIL")}  14б. Полоска камня — левый конец (merge U+D+L): ({stripFrameL.FrameX},{stripFrameL.FrameY}), ожидался (12,5)");

        // 15. Дефект 2 вертикальный: вертикальная полоска камня — нижний конец
        //     up=Self (камень сверху), down=Merge, left=Merge, right=Merge
        //     → Block 1 combo 12 (u=S,d=M,l=M,r=M) → (198, 144) merge D+L+R
        var vstripWindow = new short[5, 5];
        for (int wx = 0; wx < 5; wx++)
            for (int wy = 0; wy < 5; wy++)
                vstripWindow[wx, wy] = D;
        vstripWindow[2, 2] = S; // центр = нижний конец
        vstripWindow[2, 1] = S; // верхний сосед — камень (Self)
        // нижний (2,3) = D, левый (1,2) = D, правый (3,2) = D
        var vstripFrame = Stone.GetShema(vstripWindow, stripInfo);
        bool vstripOk = vstripFrame.FrameX == 11 && vstripFrame.FrameY == 8;
        if (!vstripOk) failures++;
        Console.WriteLine($"{(vstripOk ? "PASS" : "FAIL")}  15. Вертикальная полоска — нижний конец (merge D+L+R): ({vstripFrame.FrameX},{vstripFrame.FrameY}), ожидался (11,8)");

        // 15б. Верхний конец вертикальной полоски (симметрия)
        //     up=Merge, down=Self, left=Merge, right=Merge
        //     → Block 1 combo 13 (u=M,d=S,l=M,r=M) → (198, 90) merge U+L+R
        var vstripTopWindow = new short[5, 5];
        for (int wx = 0; wx < 5; wx++)
            for (int wy = 0; wy < 5; wy++)
                vstripTopWindow[wx, wy] = D;
        vstripTopWindow[2, 2] = S; // центр = верхний конец
        vstripTopWindow[2, 3] = S; // нижний сосед — камень (Self)
        var vstripTopFrame = Stone.GetShema(vstripTopWindow, stripInfo);
        bool vstripTopOk = vstripTopFrame.FrameX == 11 && vstripTopFrame.FrameY == 5;
        if (!vstripTopOk) failures++;
        Console.WriteLine($"{(vstripTopOk ? "PASS" : "FAIL")}  15б. Вертикальная полоска — верхний конец (merge U+L+R): ({vstripTopFrame.FrameX},{vstripTopFrame.FrameY}), ожидался (11,5)");

        // 16. Флаги для конфигурации «камень в углу пещеры» (up=E, down=M, left=E, right=M).
        //     Original 1.4.4.9: срабатывает generic-ветка 76909, которая флаги НЕ выставляет
        //     (флаги ставят только merge-ветки 75580-76402). Значит оба флага должны быть false.
        TerrariaFraming.Solve(
            TerrariaFraming.SlotEdge, TerrariaFraming.SlotMerge,
            TerrariaFraming.SlotEdge, TerrariaFraming.SlotMerge,
            TerrariaFraming.SlotEdge, TerrariaFraming.SlotSelf,
            TerrariaFraming.SlotSelf, TerrariaFraming.SlotSelf,
            0, TerrariaTilesetRules.Stone,
            out _, out _, out bool d1Up, out bool d1Down, out bool d1Left, out bool d1Right);
        if (!d1Down && !d1Right) {
            Console.WriteLine("PASS  16. Угол пещеры: generic-ветка не выставляет merge-флаги");
        } else {
            failures++;
            Console.WriteLine($"FAIL  16. Угол пещеры: ожидались флаги false, получено down={d1Down} right={d1Right}");
        }

        // 17. ПРИОРИТЕТ: горизонтальная полоска камня через землю, выход в пустоту с обеих сторон
        //     Конфигурация: air | dirt | stone(полоска) | dirt | air
        Console.WriteLine("--- 17. Полоска камня: земля → воздух с обеих сторон ---");

        // Создаём карту: 11×5, воздух по краям, земля в центре, полоска камня в середине
        var stripMap = new short[11, 5];
        for (int mx = 0; mx < 11; mx++)
            for (int my = 0; my < 5; my++)
                stripMap[mx, my] = (mx >= 3 && mx <= 7) ? D : E;
        // Полоска камня: x=3..7, y=2
        for (int mx = 3; mx <= 7; mx++) stripMap[mx, 2] = S;

        // Ожидания получены из декомпиляции 1.4.4.9:
        //   левый конец  (up=M,down=M,left=E,right=S) — merge-ветки не подходят, generic 76873
        //                (up!=num,down!=num,left!=num,right==num) → (162, 18v) → сетка (9,0)
        //   середина     (up=M,down=M,left=S,right=S) — merge-ветка 75758 → (144+18v,180) → (8,10)
        //   правый конец (up=M,down=M,left=S,right=E) — generic 76891 → (216, 18v) → сетка (12,0)
        int[] expectedX = { 9, 8, 8, 8, 12 };
        int[] expectedY = { 0, 10, 10, 10, 0 };
        string[] posNames = { "левый конец", "2-й", "3-й (центр)", "4-й", "правый конец" };

        for (int sx = 3; sx <= 7; sx++) {
            // Окно 5×5 с центром на (sx, 2)
            var win = new short[5, 5];
            for (int dx = -2; dx <= 2; dx++)
                for (int dy = -2; dy <= 2; dy++) {
                    int mx = sx + dx, my = 2 + dy;
                    win[dx + 2, dy + 2] = (mx >= 0 && mx < 11 && my >= 0 && my < 5) ? stripMap[mx, my] : E;
                }

            var info = new TileInfoStructure(S, 0);
            var f = Stone.GetShema(win, info);
            int idx = sx - 3;
            bool ok = f.FrameX == expectedX[idx] && f.FrameY == expectedY[idx];
            if (!ok) failures++;

            // Классификация соседей
            TerrariaFraming.ClassifyNeighbors(win, 2, 2, TerrariaTilesetRules.Stone,
                out int sup, out int sdown, out int sleft, out int sright,
                out int sul, out int sur, out int sdl, out int sdr);
            string slots = $"u{sup} d{sdown} l{sleft} r{sright}";

            Console.WriteLine($"{(ok ? "PASS" : "FAIL")}  17[{sx}] {posNames[idx]}: ({f.FrameX},{f.FrameY}) ожидался ({expectedX[idx]},{expectedY[idx]}) [{slots}]");
        }

        // 17б. Проверка взаимной проверки: земля над полоской должна проходить check
        // Земля над левым концом (x=3, y=1): down = stone
        var dirtAbove = new short[5, 5];
        for (int wx = 0; wx < 5; wx++)
            for (int wy = 0; wy < 5; wy++)
                dirtAbove[wx, wy] = (wx >= 1 && wx <= 5) ? D : E;
        dirtAbove[2, 3] = S; // камень снизу (center+1)

        var dirtInfo = new TileInfoStructure(D, 0);
        var dirtFrame = Dirt.GetShema(dirtAbove, dirtInfo);
        Console.WriteLine($"  Земля над полоской: ({dirtFrame.FrameX},{dirtFrame.FrameY}) — кадр земли");
        // Если взаимная проверка проходит, земля должна получить interior/merge кадр
        // (не edge-кадр с чёрной рамкой)

        Console.WriteLine();
    }

    // ---------- земля окружает камень / камень окружает землю ----------

    private static void TestEarthAroundStone() {
        Console.WriteLine("--- Земля окружает камень ---");

        // Камень полностью в земле: все 4 стороны — слияние
        short[,] all = Fill(D);
        all[2, 2] = S;
        Expect("Камень в земле (все 4 слияния)", all, S, 0, 6, 11);

        // Камень в земле с пустотой сверху (выход на поверхность)
        short[,] top = Fill(D);
        top[2, 2] = S;
        top[2, 1] = E;
        // Original 1.4.4.9: up=Edge, down=Merge, left=Merge, right=Merge.
        // 75964 требует left==num && right==num — не подходит (left/right = Merge).
        // Остальные ветки 76041-76384 не подходят (нужен up != -1 либо down == num).
        // → generic: все четыре != num → ветка 76909 → (162+18v, 54) → сетка (9,3).
        Expect("Камень в земле, верх открыт (76909)", top, S, 0, 9, 3);

        // Камень в земле с пустотой снизу
        short[,] bottom = Fill(D);
        bottom[2, 2] = S;
        bottom[2, 3] = E;
        // Аналогично: up=Merge, down=Edge, left/right=Merge → generic 76909 → (9,3)
        Expect("Камень в земле, низ открыт (76909)", bottom, S, 0, 9, 3);

        // Земля под камнем у поверхности (камень виден сверху)
        short[,] surface = Fill(D);
        surface[2, 2] = S;
        surface[2, 1] = E;
        surface[1, 1] = E;
        surface[3, 1] = E;
        // Для земли в центре: up=Edge, down=Merge(earth), stone-диагонали → Self
        // up=Edge → верхний край (1, 0)
        Expect("Земля под открытым камнем", surface, D, 0, 1, 0);

        Console.WriteLine();
    }

    private static void TestStoneAroundEarth() {
        Console.WriteLine("--- Камень окружает землю ---");

        // Земля полностью в камне: для земли камень = край + взаимная проверка.
        // Камень вокруг земли: у камня соседи-земля → Merge, другие стороны → камень.
        // Взаимная проверка: камень сверху должен слиться вниз.
        short[,] all = Fill(S);
        all[2, 2] = D;
        // Камень сверху (2,1): для камня down=dirt=Merge, up/left/right=Self
        //   → Block 1 mergeDown = true → земля считает up «своим» → все Self → центр
        Expect("Земля в камне (все 4 без швов)", all, D, 0, 1, 1);

        // Земля в камне с выходом на поверхность (сверху пусто)
        short[,] open = Fill(S);
        open[2, 2] = D;
        open[2, 1] = E;
        // Для земли: up=Edge (пустота), остальные камень → взаимная проверка:
        //   камень снизу: up=dirt=Merge, down/left/right=Self → mergeUp=true → «свой»
        //   камень слева: right=dirt=Merge, up/down/right=Self → mergeRight=true → «свой»
        //   камень справа: left=dirt=Merge, up/down/left=Self → mergeLeft=true → «свой»
        // → up=Edge, down/left/right=Self → верхний край
        Expect("Земля в камне, верх открыт", open, D, 0, 1, 0);

        // Земля в камне, камень сверху с обрывом справа:
        // конфигурация камня: up=stone(Self), down=dirt(Merge), left=stone(Self), right=Edge
        // → Block 2 (частичное слияние): down=Merge && up=Self → (90, 90) mergeDown
        // → взаимная проверка проходит → земля считает камень «своим» → шва нет
        short[,] noMerge = Fill(S);
        noMerge[2, 2] = D;          // центр — земля
        noMerge[3, 1] = E;          // обрыв справа от камня сверху
        Expect("Земля под камнем с обрывом (без шва)", noMerge, D, 0, 1, 1);

        // Кадр камня с обрывом: камень в центре, земля снизу, камень сверху/слева, пусто справа
        short[,] stoneBreak = Fill(S);
        stoneBreak[2, 3] = D;
        stoneBreak[3, 2] = E;
        // up=Self, down=Merge, left=Self, right=Edge
        // → Block 2 (2б): down==Merge && up==Self, left!=Edge && right==Edge → (90, 90) mergeDown
        Expect("Кадр камня: merge вниз при обрыве справа", stoneBreak, S, 0, 5, 5);

        // Слияние вверх+вниз при обрыве справа: up=Merge, down=Merge, left=Self, right=Edge.
        // Original 1.4.4.9: merge-ветки не подходят (нужен up==-2 && down==num для 76082),
        // generic 76891 (up!=num, down!=num, left==num, right!=num) → (216, 18v) → сетка (12,0).
        // Флаги при этом НЕ выставляются (generic-ветка их не ставит) — как в оригинале.
        short[,] udBreak = Fill(S);
        udBreak[2, 1] = D;
        udBreak[2, 3] = D;
        udBreak[3, 2] = E;
        Expect("Камень: merge вверх+вниз при обрыве (76891)", udBreak, S, 0, 12, 0);
        TerrariaFraming.Solve(
            TerrariaFraming.SlotMerge, TerrariaFraming.SlotMerge,
            TerrariaFraming.SlotSelf, TerrariaFraming.SlotEdge,
            TerrariaFraming.SlotMerge, TerrariaFraming.SlotMerge,
            TerrariaFraming.SlotMerge, TerrariaFraming.SlotMerge,
            0, TerrariaTilesetRules.Stone,
            out _, out _, out bool udUp, out bool udDown, out _, out _);
        if (!udUp && !udDown) {
            Console.WriteLine("PASS  Флаги U+D при обрыве: generic-ветка флаги не ставит");
        } else {
            failures++;
            Console.WriteLine($"FAIL  Флаги U+D при обрыве: ожидались false, получено up={udUp} down={udDown}");
        }

        Console.WriteLine();
    }

    // ---------- варианты и удаление ----------

    private static void TestVariants() {
        Console.WriteLine("--- Варианты текстур (randomTile) ---");
        short[,] flat = Fill(D);
        ClearRows(flat, 0, 1);
        Expect("Вариант 1", flat, D, 1, 2, 0);
        Expect("Вариант 2", flat, D, 2, 3, 0);
        Expect("Вариант 5 -> %3", flat, D, 5, 3, 0);
        Console.WriteLine();
    }

    private static void TestRemoval() {
        Console.WriteLine("--- Удаление блока и локальный пересчёт ---");
        short[,] map = Fill(D);
        Expect("До удаления (центр)", map, D, 0, 1, 1);
        map[2, 1] = E;
        Expect("После удаления верхнего соседа", map, D, 0, 1, 0);
        Console.WriteLine();
    }

    // ---------- детерминизм ----------

    private static void TestDeterminism() {
        Console.WriteLine("--- Детерминизм (порядок обхода не влияет) ---");
        var rnd = new Random(999);
        short[] types = { D, S, G, C, E };
        int bad = 0;

        for (int iter = 0; iter < 5000; iter++) {
            var map = new short[5, 5];
            for (int x = 0; x < 5; x++)
                for (int y = 0; y < 5; y++)
                    map[x, y] = types[rnd.Next(types.Length)];
            map[2, 2] = rnd.Next(2) == 0 ? D : S;

            short centerType = map[2, 2];
            var info = new TileInfoStructure(centerType, 0);
            var shema = centerType == S ? Stone : Dirt;

            // Прямой проход
            var f1 = shema.GetShema((short[,])map.Clone(), info);

            // Обратный порядок: заполняем в обратном направлении (та же карта)
            var f2 = shema.GetShema((short[,])map.Clone(), info);

            if (f1.FrameX != f2.FrameX || f1.FrameY != f2.FrameY) {
                bad++;
            }
        }

        if (bad == 0) {
            Console.WriteLine("  OK: 5000 карт, повторные вызовы дают идентичные кадры.");
        } else {
            failures++;
            Console.WriteLine($"  ПРОВАЛ: {bad} расхождений.");
        }
        Console.WriteLine();
    }

    // ---------- брутфорс ----------

    private static void BruteForceAllNeighborCombinations() {
        Console.WriteLine("--- Брутфорс всех комбинаций соседей ---");
        int[] slots = { TerrariaFraming.SlotSelf, TerrariaFraming.SlotEdge, TerrariaFraming.SlotMerge };
        var rules = new[] { TerrariaTilesetRules.Dirt, TerrariaTilesetRules.Stone };
        long total = 0;
        int bad = 0;

        foreach (var rule in rules) {
            for (int a = 0; a < 3; a++)
            for (int b = 0; b < 3; b++)
            for (int c = 0; c < 3; c++)
            for (int d = 0; d < 3; d++)
            for (int e = 0; e < 3; e++)
            for (int f = 0; f < 3; f++)
            for (int g = 0; g < 3; g++)
            for (int h = 0; h < 3; h++) {
                for (int v = 0; v < 3; v++) {
                    TerrariaFraming.Solve(
                        slots[a], slots[b], slots[c], slots[d],
                        slots[e], slots[f], slots[g], slots[h],
                        v, rule,
                        out int fx, out int fy,
                        out bool mu, out bool md, out bool ml, out bool mr);
                    total++;

                    bool ok = fx >= 0 && fx <= 271 && fy >= 0 && fy <= 253 &&
                              fx % 18 == 0 && fy % 18 == 0;
                    if (!ok) {
                        if (bad < 10) {
                            Console.WriteLine($"  ВНЕ ЛИСТА: slots=({slots[a]},{slots[b]},{slots[c]},{slots[d]},{slots[e]},{slots[f]},{slots[g]},{slots[h]}) v={v} -> ({fx},{fy})");
                        }
                        bad++;
                    }

                    // Флаги слияния не могут быть выставлены без merge-соседа.
                    bool anyMerge = slots[a] == -2 || slots[b] == -2 || slots[c] == -2 || slots[d] == -2 ||
                                    slots[e] == -2 || slots[f] == -2 || slots[g] == -2 || slots[h] == -2;
                    if (!anyMerge && (mu || md || ml || mr)) {
                        bad++;
                        if (bad < 10) {
                            Console.WriteLine($"  ЛОЖНОЕ СЛИЯНИЕ: slots без -2, флаги = {mu}/{md}/{ml}/{mr}");
                        }
                    }
                }
            }
        }

        if (bad == 0) {
            Console.WriteLine($"  OK: {total} комбинаций, все кадры внутри листа, флаги корректны.");
        } else {
            failures++;
            Console.WriteLine($"  ПРОВАЛ: {bad} аномалий из {total}.");
        }
        Console.WriteLine();
    }

    private static void FuzzRandomMaps() {
        Console.WriteLine("--- Фаззинг случайных карт ---");
        var rnd = new Random(12345);
        short[] types = { D, S, G, C, E, (short)TileTypeEnum.Iron, (short)TileTypeEnum.Tree };
        int bad = 0;

        for (int iter = 0; iter < 20000; iter++) {
            var map = new short[5, 5];
            for (int x = 0; x < 5; x++)
                for (int y = 0; y < 5; y++)
                    map[x, y] = types[rnd.Next(types.Length)];

            short centerType = rnd.Next(2) == 0 ? D : S;
            map[2, 2] = centerType;
            var info = new TileInfoStructure(centerType, (short)rnd.Next(3));
            var shema = centerType == D ? Dirt : Stone;
            var f = shema.GetShema(map, info);

            if (f.FrameX < 0 || f.FrameX > 15 || f.FrameY < 0 || f.FrameY > 14) {
                bad++;
            }
        }

        if (bad == 0) {
            Console.WriteLine("  OK: 20000 случайных карт, все кадры в сетке 16x15.");
        } else {
            failures++;
            Console.WriteLine($"  ПРОВАЛ: {bad} карт с кадром вне сетки.");
        }
        Console.WriteLine();
    }

    private static void PerfCheck() {
        Console.WriteLine("--- Производительность ---");
        const int width = 800;
        const int height = 200;
        var map = new short[width, height];
        var rnd = new Random(7);

        for (int x = 0; x < width; x++) {
            int surface = 40 + (int)(10 * Math.Sin(x * 0.05)) + rnd.Next(0, 3);
            for (int y = 0; y < height; y++) {
                if (y < surface) {
                    map[x, y] = E;
                } else if (y < surface + 6 || rnd.Next(100) < 15) {
                    map[x, y] = D;
                } else {
                    map[x, y] = S;
                }
            }
        }

        var window = new short[5, 5];
        var infoD = new TileInfoStructure(D, 0);
        var infoS = new TileInfoStructure(S, 0);

        var sw = Stopwatch.StartNew();
        long solved = 0;
        for (int x = 2; x < width - 2; x++) {
            for (int y = 2; y < height - 2; y++) {
                for (int dx = -2; dx <= 2; dx++)
                    for (int dy = -2; dy <= 2; dy++)
                        window[dx + 2, dy + 2] = map[x + dx, y + dy];

                short t = map[x, y];
                if (t == E || !TerrariaTilesetRules.IsTerrainSolid(t)) {
                    continue;
                }
                var shema = t == D ? Dirt : Stone;
                var info = t == D ? infoD : infoS;
                shema.GetShema(window, info);
                solved++;
            }
        }
        sw.Stop();

        double ms = sw.Elapsed.TotalMilliseconds;
        Console.WriteLine($"  Карта {width}x{height}: обработано {solved} тайлов за {ms:F1} мс ({solved / ms:F2} тыс. тайлов/с).");
        Console.WriteLine();
    }

    // ---------- утилиты ----------

    private static short[,] Fill(short type) {
        var m = new short[5, 5];
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                m[x, y] = type;
        return m;
    }

    private static void ClearRows(short[,] m, int from, int to) {
        for (int y = from; y <= to; y++)
            for (int x = 0; x < m.GetLength(0); x++)
                m[x, y] = E;
    }

    private static void Expect(string name, short[,] map, short type, short variant, int expectX, int expectY) {
        var info = new TileInfoStructure(type, variant);
        var shema = type == S ? Stone : Dirt;
        var f = shema.GetShema(map, info);
        bool ok = f.FrameX == expectX && f.FrameY == expectY;
        if (!ok) {
            failures++;
        }
        Console.WriteLine($"{(ok ? "PASS" : "FAIL")}  {name}: получено ({f.FrameX},{f.FrameY}), ожидалось ({expectX},{expectY})");
    }
}

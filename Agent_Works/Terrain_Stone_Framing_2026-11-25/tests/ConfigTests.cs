using System;

/// <summary>
/// Тесты конфигурации совместимости тайлсетов: включение/отключение пар в
/// TilesetMergeConfig должно управлять слиянием без правок TerrariaFraming.
/// Парные правила симметричны; отсутствие правила = слияния нет.
/// </summary>
public static class ConfigTests {
    private const short DIRT = (short)TileTypeEnum.Terrain0;
    private const short STONE = (short)TileTypeEnum.Stone;
    private const short ORE = (short)TileTypeEnum.Terrain8;

    private static int passed;
    private static int failures;

    public static int Run() {
        passed = 0;
        failures = 0;

        ReloadDefaults();

        Console.WriteLine("--- Конфигурация совместимости тайлсетов ---");

        // 1. Базовое: все три пары включены (дефолт)
        CheckFrame("1a камень над землёй", MakeMap(STONE, DIRT), STONE, TerrariaTilesetRules.Stone, 8, 5);
        CheckFrame("1c камень над рудой", MakeMap(STONE, ORE), STONE, TerrariaTilesetRules.Stone, 8, 5);
        CheckFrame("1d руда над камнем", MakeMap(ORE, STONE), ORE, TerrariaTilesetRules.Terrain8, 8, 5);
        CheckFrame("1e земля-ст тыс (внутр., стык с камнем)", MakeMap(DIRT, STONE), DIRT, TerrariaTilesetRules.Dirt, 1, 1);

        // 2. Отключить Земля↔Камень: остальные пары живут
        TilesetMergeConfig.SetPairAllowed(DIRT, STONE, false);
        CheckFrame("2a камень над землёй: пара отключена → бок (18+18v,36) = (1,2)",
            MakeMap(STONE, DIRT), STONE, TerrariaTilesetRules.Stone, 1, 2);
        CheckFrame("2b земля под камнем: пара отключена → верхний совокуп. кадр камня...",
            MakeMap(DIRT, STONE), DIRT, TerrariaTilesetRules.Dirt, 1, 2);
        CheckFrame("2c камень над рудой (не затронут)", MakeMap(STONE, ORE), STONE, TerrariaTilesetRules.Stone, 8, 5);
        CheckFrame("2d руда над камнем (не затронут)", MakeMap(ORE, STONE), ORE, TerrariaTilesetRules.Terrain8, 8, 5);
        CheckFrame("2e земля-руда (не затронуто, направление земля)", MakeMap(DIRT, ORE), DIRT, TerrariaTilesetRules.Dirt, 1, 1);

        // 3. Симметрия: пара работает в обе стороны
        ReloadDefaults();
        CheckFrame("3a пара земля-камень: направление камень", MakeMap(STONE, DIRT), STONE, TerrariaTilesetRules.Stone, 8, 5);
        CheckFrame("3b пара земля-оруда: направление руда", MakeMap(ORE, DIRT), ORE, TerrariaTilesetRules.Terrain8, 8, 5);

        // 4. Отключить Земля↔Руда
        TilesetMergeConfig.SetPairAllowed(DIRT, ORE, false);
        CheckFrame("4a руда над землёй: пара отключена → (1,2)", MakeMap(ORE, DIRT), ORE, TerrariaTilesetRules.Terrain8, 1, 2);
        CheckFrame("4b камень над рудой (не затронут)", MakeMap(STONE, ORE), STONE, TerrariaTilesetRules.Stone, 8, 5);
        CheckFrame("4c камень над землёй (не затронуто)", MakeMap(STONE, DIRT), STONE, TerrariaTilesetRules.Stone, 8, 5);

        // 5. Отключить Камень↔Руда
        ReloadDefaults();
        TilesetMergeConfig.SetPairAllowed(STONE, ORE, false);
        CheckFrame("5a камень над рудой: пара отключена → бок (1,2)", MakeMap(STONE, ORE), STONE, TerrariaTilesetRules.Stone, 1, 2);
        CheckFrame("5b руда над камнем: пара отключена → бок (1,2)", MakeMap(ORE, STONE), ORE, TerrariaTilesetRules.Terrain8, 1, 2);
        CheckFrame("5c руда над землёй (не затронуто)", MakeMap(ORE, DIRT), ORE, TerrariaTilesetRules.Terrain8, 8, 5);
        CheckFrame("5d земля-камень (не затронут)", MakeMap(STONE, DIRT), STONE, TerrariaTilesetRules.Stone, 8, 5);

        // 6. Пара Руда↔Трава (TerrainGrass — dirt-семейство, часть конфигурации):
        //    по умолчанию включена (как и Камень↔Трава); отключается одной строкой конфига
        ReloadDefaults();
        const short GRASS = (short)TileTypeEnum.TerrainGrass;
        CheckFrame("6a руда над травой: пара по умолчанию включена → merge (8,5)",
            MakeMap(ORE, GRASS), ORE, TerrariaTilesetRules.Terrain8, 8, 5);
        CheckFrame("6b камень над травой: пара по умолчанию включена → merge (8,5)",
            MakeMap(STONE, GRASS), STONE, TerrariaTilesetRules.Stone, 8, 5);
        TilesetMergeConfig.SetPairAllowed(ORE, GRASS, false);
        CheckFrame("6c руда над травой: пара отключена через конфиг → бок (1,2)",
            MakeMap(ORE, GRASS), ORE, TerrariaTilesetRules.Terrain8, 1, 2);
        CheckFrame("6d камень над травой (не затронуто отключением руда↔трава) → (8,5)",
            MakeMap(STONE, GRASS), STONE, TerrariaTilesetRules.Stone, 8, 5);
        TilesetMergeConfig.SetPairAllowed(ORE, GRASS, true);
        CheckFrame("6e руда над травой: пара включена обратно → merge (8,5)",
            MakeMap(ORE, GRASS), ORE, TerrariaTilesetRules.Terrain8, 8, 5);

        // 7. Отсутствующее правило = слияния нет
        ReloadDefaults();
        CheckFrame("7a земля над землёй-не-пара (Self, всегда) (1,1)", MakeMap(DIRT, DIRT), DIRT, TerrariaTilesetRules.Dirt, 1, 1);
        CheckFrame("7b камень/дерево: дерева нет в правилах → нижний край (1,2)",
            MakeLateMap(STONE), STONE, TerrariaTilesetRules.Stone, 1, 2);

        Console.WriteLine($"Конфигурация: {passed} OK, {failures} FAIL");
        return failures;
    }

    private static void ReloadDefaults() {
        TilesetMergeConfig.ReloadForTests();
    }

    /// <summary>Карта 5×5 (взаимная проверка требует окно 5×5): центр — заданный тип,
    /// все соседи того же типа, снизу (2,3) — сосед пары.</summary>
    private static short[,] MakeMap(short centerType, short bottomType) {
        var m = new short[5, 5];
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                m[x, y] = centerType;
        m[2, 3] = bottomType;
        return m;
    }

    /// <summary>Карта: (2,3) заменяем на дерево (TileTypeEnum.Tree) — тип без правил.</summary>
    private static short[,] MakeLateMap(short centerType) {
        var m = MakeMap(centerType, centerType);
        m[2, 3] = (short)TileTypeEnum.Tree;
        return m;
    }

    private static void CheckFrame(string name, short[,] map, short centerType,
                                    TerrariaTilesetRules rules, int expGridX, int expGridY) {
        var f = new TerrainTileShema(rules).GetShema(map, new TileInfoStructure(centerType, 0));
        if (f.FrameX == expGridX && f.FrameY == expGridY) {
            passed++;
            Console.WriteLine($"PASS  {name}: ({f.FrameX},{f.FrameY})");
        } else {
            failures++;
            Console.WriteLine($"FAIL  {name}: получено ({f.FrameX},{f.FrameY}), ожидалось ({expGridX},{expGridY})");
        }
    }
}

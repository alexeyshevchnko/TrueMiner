// Заглушки для компиляции игровых файлов вне Unity (только для теста решателя).
namespace UnityEngine {
}

// Минимальный аналог TileInfoStructure: движку кадрирования нужны только
// type (тип блока) и randomTile (вариант текстуры).
public class TileInfoStructure {
    public short type;
    public short randomTile;

    public TileInfoStructure(short type, short randomTile) {
        this.type = type;
        this.randomTile = randomTile;
    }
}

// Заглушка конфигурации совместимости: тот же контракт, что у
// Assets/Generator/Terrain/TilesetMergeConfig.cs, но без UnityEngine.
// Стартует с дефолтных пар проекта (аналог чтения файла конфигурации).
public static class TilesetMergeConfig {
    private static System.Collections.Generic.HashSet<long> allowed = BuildDefault();

    public static bool AllowsMerge(short typeA, short typeB) {
        return allowed.Contains(Key(typeA, typeB));
    }

    public static void SetPairAllowed(short typeA, short typeB, bool isAllowed) {
        if (isAllowed) {
            allowed.Add(Key(typeA, typeB));
        } else {
            allowed.Remove(Key(typeA, typeB));
        }
    }

    /// <summary>Сброс к «файловым» дефолтам проекта (аналог чтения .txt).</summary>
    public static void ReloadForTests() {
        allowed = BuildDefault();
    }

    private static System.Collections.Generic.HashSet<long> BuildDefault() {
        var set = new System.Collections.Generic.HashSet<long>();
        set.Add(Key((short)TileTypeEnum.Terrain0, (short)TileTypeEnum.Stone));
        set.Add(Key((short)TileTypeEnum.Terrain0, (short)TileTypeEnum.Terrain8));
        set.Add(Key((short)TileTypeEnum.Stone, (short)TileTypeEnum.Terrain8));
        set.Add(Key((short)TileTypeEnum.Stone, (short)TileTypeEnum.TerrainGrass));
        set.Add(Key((short)TileTypeEnum.Terrain8, (short)TileTypeEnum.TerrainGrass));
        return set;
    }

    public static long Key(short typeA, short typeB) {
        short min = typeA < typeB ? typeA : typeB;
        short max = typeA < typeB ? typeB : typeA;
        return ((long)min << 16) | (ushort)max;
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Конфигурация совместимости тайлсетов (какие пары тайлсетов сливаются).
///
/// Источник данных — Assets/Resources/TilesetMergeRules.txt, читается ОДИН РАЗ при
/// первом использовании (преднамеренный минимал: без менеджеров и подписок).
///
/// Формат строки:  ИмяA, ИмяB, true|false
/// • правило симметрично — пара (A,B) разрешает слияние и в сторону B;
/// • строки, начинающиеся с '#' или пустые, игнорируются;
/// • отсутствующее правило = слияния нет (ColumnEncode будет границей);
/// • при явном false пара может быть переоткрыта поздней true-строкой (последняя выигрывает);
/// • файл отсутствует/не читается → встроенные дефолтные пары проекта
///   (Земля↔Камень, Земля↔Руда, Камень↔Руда), чтобы игра оставалась рабочей.
///
/// Влияние на алгоритм автотайлинга — ТОЛЬКО через TerrariaFraming.ClassifyNeighbor:
/// конфиг решает, считать ли соседа чужого тайлсета «слиянием». Все остальные условия
/// выбора кадров (окрестности, диагонали, варианты) остаются в TerrariaFraming.
/// </summary>
public static class TilesetMergeConfig {
    /// <summary>Имя ресурса (без расширения) с правилами пар.</summary>
    public const string ConfigResourceName = "TilesetMergeRules";

    // Разрешённые пары. Ключ симметричный: (min,max) → старший тип сдвинут на 16 бит.
    private static HashSet<long> allowed;

    private static bool loaded;

    public static bool AllowsMerge(short typeA, short typeB) {
        EnsureLoaded();
        return allowed.Contains(Key(typeA, typeB));
    }

    /// <summary>Ручное управление парой (для тестов и инструментов редактора).</summary>
    public static void SetPairAllowed(short typeA, short typeB, bool isAllowed) {
        EnsureLoaded();
        if (isAllowed) {
            allowed.Add(Key(typeA, typeB));
        } else {
            allowed.Remove(Key(typeA, typeB));
        }
    }

    /// <summary>Сброс состояния конфига к файловым настройкам (тесты/горячая перезагрузка).</summary>
    public static void Reload() {
        loaded = false;
        allowed = null;
        EnsureLoaded();
    }

    /// <summary>Симметричный ключ пары: (min,max).</summary>
    public static long Key(short typeA, short typeB) {
        short min = typeA < typeB ? typeA : typeB;
        short max = typeA < typeB ? typeB : typeA;
        return ((long)min << 16) | (ushort)max;
    }

    private static void EnsureLoaded() {
        if (loaded) {
            return;
        }
        loaded = true;

        if (allowed == null) {
            allowed = new HashSet<long>();
        }

        TextAsset asset = Resources.Load<TextAsset>(ConfigResourceName);
        if (asset == null || string.IsNullOrEmpty(asset.text)) {
            Debug.Log("TilesetMergeConfig: файл '" + ConfigResourceName + "' не найден — " +
                      "используются встроенные дефолтные пары (Земля↔Камень, Земля↔Руда, Камень↔Руда)");
            ApplyDefaults();
            return;
        }

        int lineNo = 0;
        foreach (var rawLine in asset.text.Split('\n')) {
            lineNo++;
            var line = rawLine.Trim(' ', '\r', '\t');
            if (line.Length == 0 || line[0] == '#') {
                continue;
            }

            var parts = line.Split(',');
            if (parts.Length != 3) {
                Debug.LogWarning("TilesetMergeConfig(" + ConfigResourceName + ":" + lineNo + "): " +
                                 "формат строки 'ИмяA, ИмяB, true|false' — пропущена: " + line);
                continue;
            }

            short typeA;
            short typeB;
            if (!ResolveType(parts[0], lineNo, out typeA) || !ResolveType(parts[1], lineNo, out typeB)) {
                continue;
            }

            var value = parts[2].Trim(' ', '\t');
            bool isMergeAllowed;
            if (value == "true") {
                isMergeAllowed = true;
            } else if (value == "false") {
                isMergeAllowed = false;
            } else {
                Debug.LogWarning("TilesetMergeConfig(" + ConfigResourceName + ":" + lineNo + "): " +
                                 "третье поле должно быть true или false — пропущена: " + line);
                continue;
            }

            if (isMergeAllowed) {
                allowed.Add(Key(typeA, typeB));
            } else {
                allowed.Remove(Key(typeA, typeB));
            }
        }

        Debug.Log("TilesetMergeConfig: загружено разрешённых пар " + allowed.Count +
                  " из '" + ConfigResourceName + "'");
    }

    /// <summary>Дефолтные пары текущего проекта — на случай отсутствия файла.
    /// (Трава — dirt-семейство: камень и руда сливаются с ней теми же merge-кадрами.)</summary>
    private static void ApplyDefaults() {
        SetPairAllowed((short)TileTypeEnum.Terrain0, (short)TileTypeEnum.Stone, true);
        SetPairAllowed((short)TileTypeEnum.Terrain0, (short)TileTypeEnum.Terrain8, true);
        SetPairAllowed((short)TileTypeEnum.Stone, (short)TileTypeEnum.Terrain8, true);
        SetPairAllowed((short)TileTypeEnum.Stone, (short)TileTypeEnum.TerrainGrass, true);
        SetPairAllowed((short)TileTypeEnum.Terrain8, (short)TileTypeEnum.TerrainGrass, true);
    }

    /// <summary>
    /// Имя тайлсета → short-тип. Порядок: алиасы на русском → имя TileTypeEnum →
    /// имя записи SpriteData (например, Tiles_8, Tiles_coal). Не распознано → warning, false.
    /// </summary>
    private static bool ResolveType(string raw, int lineNo, out short type) {
        string name = raw.Trim(' ', '\t');
        switch (name) {
            case "Земля":
                name = "Terrain0";
                break;
            case "Трава":
                name = "TerrainGrass";
                break;
            case "Камень":
                name = "Stone";
                break;
            case "Руда":
                name = "Terrain8";
                break;
        }

        // Точные имена TileTypeEnum (регистронезависимо)
        foreach (TileTypeEnum v in System.Enum.GetValues(typeof(TileTypeEnum))) {
            if (string.Equals(v.ToString(), name, System.StringComparison.OrdinalIgnoreCase)) {
                type = (short)v;
                return true;
            }
        }

        // Имена записей SpriteData (спрайтовые имена, например Tiles_8)
        SpriteData data;
        if (SpriteData.Sprites.TryGetValue(name, out data)) {
            type = data.TypeId;
            return true;
        }

        Debug.LogWarning("TilesetMergeConfig(" + ConfigResourceName + ":" + lineNo + "): " +
                         "unknown тайл '" + raw + "' (нет алиаса, TileTypeEnum или SpriteData) — строка пропущена");
        type = 0;
        return false;
    }
}

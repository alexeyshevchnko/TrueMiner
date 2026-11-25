using System;
using System.IO;
using System.Text;

/// <summary>
/// Диагностика: поиск тайлов, у которых выбор кадра противоречит наличию
/// merge-соседей. Тайл камня с соседом-землёй (SlotMerge) должен получать
/// merge-кадр (BLOCK 1/2). Если выбран generic-кадр (BLOCK 3) — на стороне
/// merge-соседа будет тёмная рамка → видимый разрыв.
/// </summary>
public static class Diagnostic {
    private const short D = (short)TileTypeEnum.Terrain0;
    private const short S = (short)TileTypeEnum.Stone;
    private const short G = (short)TileTypeEnum.TerrainGrass;
    private const short E = 0;

    private static readonly ITileShema Dirt = new TerrainTileShema(TerrariaTilesetRules.Dirt);
    private static readonly ITileShema Stone = new TerrainTileShema(TerrariaTilesetRules.Stone);

    public static void Run() {
        const int w = 64;
        const int h = 36;
        var map = BuildPreviewMap(w, h);

        var window = new short[5, 5];
        var csv = new StringBuilder();
        csv.AppendLine("x;y;type;fx;fy;slots;problem");

        int stoneCount = 0;
        int problems = 0;
        for (int x = 0; x < w; x++) {
            for (int y = 0; y < h; y++) {
                short t = map[x, y];
                if (t != S) continue;
                stoneCount++;

                for (int dx = -2; dx <= 2; dx++)
                    for (int dy = -2; dy <= 2; dy++) {
                        int mx = x + dx, my = y + dy;
                        window[dx + 2, dy + 2] =
                            (mx >= 0 && my >= 0 && mx < w && my < h) ? map[mx, my] : E;
                    }

                var rules = TerrariaTilesetRules.Stone;
                TerrariaFraming.ClassifyNeighbors(window, 2, 2, rules,
                    out int up, out int down, out int left, out int right,
                    out int ul, out int ur, out int dl, out int dr);

                TerrariaFraming.Solve(up, down, left, right, ul, ur, dl, dr,
                    0, rules, out int fx, out int fy, out _, out _, out _, out _);

                string slots = $"u{up} d{down} l{left} r{right} ul{ul} ur{ur} dl{dl} dr{dr}";

                // Проблема: есть merge-сосед по стороне, но кадр generic (Y < 90 и не
                // из merge-набора) — тёмная рамка на стороне merge-соседа.
                bool hasMergeSide =
                    up == TerrariaFraming.SlotMerge || down == TerrariaFraming.SlotMerge ||
                    left == TerrariaFraming.SlotMerge || right == TerrariaFraming.SlotMerge;
                bool isGenericFrame = fy < 5; // Y < 90 — общий набор (края/углы/центр)
                // Примечание: generic-кадр при наличии Merge-соседа — НЕ ошибка.
                // Оригинал (WorldGen.cs 75580/75960) вообще не назначает merge-кадр,
                // если комбинация не покрыта его ветками, и тогда работает generic-цепочка
                // (76551+), где сравнение строгое `== num`. Поэтому здесь считаем только
                // информационно: сколько тайлов ушло в generic при наличии merge-соседей.
                bool problem = hasMergeSide && isGenericFrame;

                if (problem) problems++;
                csv.AppendLine($"{x};{y};1;{fx / 18};{fy / 18};{slots};{(problem ? "GENERIC_WITH_MERGE" : "")}");
            }
        }

        var outPath = Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\diagnostic.csv");
        File.WriteAllText(outPath, csv.ToString());
        Console.WriteLine($"Диагностика: камней {stoneCount}, из них generic-кадр при merge-соседе (норма для непокрытых комбинаций): {problems}");
        Console.WriteLine($"Файл: {outPath}");
    }

    private static short[,] BuildPreviewMap(int w, int h) {
        var map = new short[w, h];
        int[] surface = new int[w];
        for (int x = 0; x < w; x++) {
            surface[x] = 8 + (int)(3.5 * Math.Sin(x * 0.22)) + (int)(2 * Math.Sin(x * 0.07 + 1.3));
            if (x > 12 && x < 17) surface[x] += 5;
            if (x > 40 && x < 44) surface[x] -= 4;
        }
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                map[x, y] = y >= surface[x] ? D : E;
        for (int x = 0; x < w; x++)
            map[x, surface[x]] = (x % 7 < 4) ? G : D;
        for (int x = 0; x < w; x++)
            for (int y = 20; y < h; y++)
                if (Math.Sin(x * 0.35) + Math.Cos(y * 0.5) > 0.2)
                    map[x, y] = S;
        for (int x = 18; x < 34; x++) {
            int cy = 24 + (int)(2 * Math.Sin(x * 0.4));
            for (int dy = -2; dy <= 2; dy++)
                if (cy + dy >= 0 && cy + dy < h)
                    map[x, cy + dy] = E;
        }
        map[8, 4] = D; map[9, 4] = D; map[8, 5] = D;
        map[50, 6] = D;
        for (int y = 3; y <= 6; y++) map[58, y] = D;
        for (int x = 20; x <= 25; x++) map[x, 3] = D;
        for (int y = 12; y <= 20; y++) map[47, y] = S;
        return map;
    }
}

using System;
using System.IO;
using System.Text;

/// <summary>
/// Генерация данных для визуального превью: синтетическая карта -> кадры решателя -> CSV.
/// Рендер выполняется отдельным скриптом (PowerShell + GDI+) по этому файлу.
/// </summary>
public static class PreviewGen {
    private const short D = (short)TileTypeEnum.Terrain0;
    private const short S = (short)TileTypeEnum.Stone;
    private const short G = (short)TileTypeEnum.TerrainGrass;
    private const short E = 0;

    private static readonly ITileShema Dirt = new TerrainTileShema(TerrariaTilesetRules.Dirt);
    private static readonly ITileShema Stone = new TerrainTileShema(TerrariaTilesetRules.Stone);

    public static void Run() {
        const int w = 64;
        const int h = 36;

        var map = new short[w, h];

        // Рельеф: синусоидальная поверхность с холмиком и ямой
        int[] surface = new int[w];
        for (int x = 0; x < w; x++) {
            surface[x] = 8 + (int)(3.5 * Math.Sin(x * 0.22)) + (int)(2 * Math.Sin(x * 0.07 + 1.3));
            if (x > 12 && x < 17) surface[x] += 5;
            if (x > 40 && x < 44) surface[x] -= 4;
        }
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                map[x, y] = y >= surface[x] ? D : E;

        // Трава на поверхности (частично)
        for (int x = 0; x < w; x++)
            map[x, surface[x]] = (x % 7 < 4) ? G : D;

        // Камень ниже глубины 20 (переходы земля-камень во всех направлениях)
        for (int x = 0; x < w; x++)
            for (int y = 20; y < h; y++)
                if (Math.Sin(x * 0.35) + Math.Cos(y * 0.5) > 0.2)
                    map[x, y] = S;

        // Пещера
        for (int x = 18; x < 34; x++) {
            int cy = 24 + (int)(2 * Math.Sin(x * 0.4));
            for (int dy = -2; dy <= 2; dy++)
                if (cy + dy >= 0 && cy + dy < h)
                    map[x, cy + dy] = E;
        }

        // Отдельные блоки и малые группы
        map[8, 4] = D;
        map[9, 4] = D;
        map[8, 5] = D;
        map[50, 6] = D;                                 // одиночный блок
        for (int y = 3; y <= 6; y++) map[58, y] = D;    // вертикальная колонна
        for (int x = 20; x <= 25; x++) map[x, 3] = D;   // горизонтальная полоса

        // Каменный столб из земли
        for (int y = 12; y <= 20; y++) map[47, y] = S;

        // Горизонтальная полоска камня толщиной 1 внутри земли, выходящая в пустоту
        // С ОБЕИХ сторон (как в задаче): воздушный карман — полоса камня — воздушный карман.
        int stripY = 17;
        int stripX0 = 3, stripX1 = 15;
        for (int x = stripX0; x <= stripX1; x++) {
            map[x, stripY] = S;
        }
        // Земля сверху и снизу от полоски (Merge-соседи)
        for (int x = stripX0; x <= stripX1; x++) {
            if (stripY - 1 >= 0) map[x, stripY - 1] = D;
            if (stripY + 1 < h) map[x, stripY + 1] = D;
        }
        // Воздушные карманы слева и справа: полоса выходит в пустоту с обеих сторон
        for (int x = 0; x < stripX0; x++) {
            for (int dy = -1; dy <= 1; dy++) {
                int ay = stripY + dy;
                if (ay >= 0 && ay < h) map[x, ay] = E;
            }
        }
        for (int x = stripX1 + 1; x < stripX1 + 4 && x < w; x++) {
            for (int dy = -1; dy <= 1; dy++) {
                int ay = stripY + dy;
                if (ay >= 0 && ay < h) map[x, ay] = E;
            }
        }

        var window = new short[5, 5];
        var csv = new StringBuilder();
        csv.AppendLine("x;y;type;fx;fy");
        int solved = 0;
        for (int x = 0; x < w; x++) {
            for (int y = 0; y < h; y++) {
                short t = map[x, y];
                if (t == E || !TerrariaTilesetRules.IsTerrainSolid(t)) continue;

                for (int dx = -2; dx <= 2; dx++)
                    for (int dy = -2; dy <= 2; dy++) {
                        int mx = x + dx, my = y + dy;
                        window[dx + 2, dy + 2] =
                            (mx >= 0 && my >= 0 && mx < w && my < h) ? map[mx, my] : E;
                    }

                var shema = t == S ? Stone : Dirt;
                var info = new TileInfoStructure(t, (short)((x * 7 + y * 3) % 3));
                var f = shema.GetShema(window, info);
                solved++;
                csv.AppendLine($"{x};{y};{(t == S ? 1 : 0)};{f.FrameX};{f.FrameY}");
            }
        }

        var outPath = Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\terrain_frames.csv");
        File.WriteAllText(outPath, csv.ToString());
        Console.WriteLine($"Кадры превью: {outPath} ({solved} тайлов)");
    }
}

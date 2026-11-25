using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

/// <summary>
/// Генератор визуального атласа кадров: рендерит ВСЕ комбинации соседей
/// (Self/Merge/Edge × up/down/left/right) для камня и земли как реальные
/// тайлы с кадрами решателя. Каждый случай подписан кодом слотов.
/// </summary>
public static class FramingSheet {
    private const short D = (short)TileTypeEnum.Terrain0;
    private const short S = (short)TileTypeEnum.Stone;
    private const short E = 0;

    private static readonly ITileShema Dirt = new TerrainTileShema(TerrariaTilesetRules.Dirt);
    private static readonly ITileShema Stone = new TerrainTileShema(TerrariaTilesetRules.Stone);
    private static Image _dirtSheet, _stoneSheet;

    public static void Run() {
        // Дампы тайлсетов копируются в выходной каталог из tests/assets (см. csproj).
        string sheetDir = AppContext.BaseDirectory;
        if (!File.Exists(Path.Combine(sheetDir, "tiles_0_dump.png"))) {
            // Запасной путь: запуск через `dotnet run` без копирования (bin/<cfg>/<tfm> → tests/assets).
            sheetDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "assets"));
        }
        _dirtSheet = Image.FromFile(Path.Combine(sheetDir, "tiles_0_dump.png"));
        _stoneSheet = Image.FromFile(Path.Combine(sheetDir, "tiles_1_dump.png"));
        GenerateSheet(S, "stone");
        GenerateSheet(D, "dirt");
        _dirtSheet.Dispose();
        _stoneSheet.Dispose();
    }

    private static void GenerateSheet(short centerType, string label) {
        var rules = centerType == S ? TerrariaTilesetRules.Stone : TerrariaTilesetRules.Dirt;
        var shema = centerType == S ? (ITileShema)Stone : Dirt;

        // Материал для Self и Merge категорий
        short selfType = centerType;
        short mergeType = centerType == S ? D : S;

        // Все 3^4 = 81 комбинация
        var combos = new List<string>();
        foreach (var u in new[] { 'S', 'M', 'E' })
            foreach (var d in new[] { 'S', 'M', 'E' })
                foreach (var l in new[] { 'S', 'M', 'E' })
                    foreach (var r in new[] { 'S', 'M', 'E' })
                        combos.Add($"{u}{d}{l}{r}");

        short CatToType(char cat) => cat == 'S' ? selfType : (cat == 'M' ? mergeType : E);

        const int cellTiles = 5;
        const int zoom = 2;
        const int px = 16 * zoom;
        int cols = 9;
        int rows = (combos.Count + cols - 1) / cols;
        int cellW = cellTiles * px;
        int cellH = cellTiles * px + 20;
        int bw = cols * cellW + 30;
        int bh = rows * cellH + 30;

        using var bmp = new Bitmap(bw, bh, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.FromArgb(255, 26, 20, 34));
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        var font = new Font("Consolas", 7);
        var header = new Font("Consolas", 11, FontStyle.Bold);
        g.DrawString($"Framing: {label} (S=Self M=Merge E=Edge, порядок up/down/left/right)",
            header, Brushes.Lime, 4, 4);

        int ox = 15, oy = 18;

        for (int i = 0; i < combos.Count; i++) {
            string combo = combos[i];
            int col = i % cols, row = i / cols;
            int cx = ox + col * cellW;
            int cy = oy + row * cellH;

            g.DrawString(combo, font, Brushes.Yellow, cx, cy);

            var map = new short[cellTiles, cellTiles];
            for (int mx = 0; mx < cellTiles; mx++)
                for (int my = 0; my < cellTiles; my++)
                    map[mx, my] = centerType;
            map[2, 1] = CatToType(combo[0]);
            map[2, 3] = CatToType(combo[1]);
            map[1, 2] = CatToType(combo[2]);
            map[3, 2] = CatToType(combo[3]);

            var window = new short[5, 5];
            for (int wx = 0; wx < 5; wx++)
                for (int wy = 0; wy < 5; wy++)
                    window[wx, wy] = map[wx, wy];

            var info = new TileInfoStructure(centerType, 0);
            var frame = shema.GetShema(window, info);

            // Рендер всех 5×5 тайлов
            for (int tx = 0; tx < cellTiles; tx++) {
                for (int ty = 0; ty < cellTiles; ty++) {
                    short tileType = map[tx, ty];
                    if (tileType == E) continue;

                    Image sheet;
                    int sx, sy;
                    if (tx == 2 && ty == 2) {
                        sheet = centerType == S ? _stoneSheet : _dirtSheet;
                        sx = frame.FrameX * 18;
                        sy = frame.FrameY * 18;
                    } else {
                        sheet = tileType == S ? _stoneSheet : _dirtSheet;
                        sx = 18; sy = 18; // внутренний кадр
                    }

                    g.DrawImage(sheet,
                        new Rectangle(cx + tx * px, cy + 20 + ty * px, px, px),
                        new Rectangle(sx, sy, 16, 16), GraphicsUnit.Pixel);
                }
            }

            g.DrawRectangle(Pens.DimGray, cx, cy + 20, cellW - 2, cellTiles * px);
        }

        var outPath = Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\framing_sheet_" + label + ".png");
        bmp.Save(outPath, ImageFormat.Png);
        Console.WriteLine($"Атлас {label}: {outPath} ({bw}x{bh}, {combos.Count} комбинаций)");
    }
}

using System;
using System.IO;
using System.Text;

/// <summary>
/// Р“РµРЅРµСЂР°С†РёСЏ РґР°РЅРЅС‹С… РґР»СЏ РІРёР·СѓР°Р»СЊРЅРѕРіРѕ РїСЂРµРІСЊСЋ: СЃРёРЅС‚РµС‚РёС‡РµСЃРєР°СЏ РєР°СЂС‚Р° -> РєР°РґСЂС‹ СЂРµС€Р°С‚РµР»СЏ -> CSV.
/// Р РµРЅРґРµСЂ РІС‹РїРѕР»РЅСЏРµС‚СЃСЏ РѕС‚РґРµР»СЊРЅС‹Рј СЃРєСЂРёРїС‚РѕРј (PowerShell + GDI+) РїРѕ СЌС‚РѕРјСѓ С„Р°Р№Р»Сѓ.
/// </summary>
public static class PreviewGen {
    private const short D = (short)TileTypeEnum.Terrain0;
    private const short S = (short)TileTypeEnum.Stone;
    private const short G = (short)TileTypeEnum.TerrainGrass;
    private const short E = 0;

    private static readonly ITileShema Dirt = new TerrainTileShema(TerrariaTilesetRules.Dirt);
    private static readonly ITileShema Stone = new TerrainTileShema(TerrariaTilesetRules.Stone);
    private static readonly ITileShema Ore8 = new TerrainTileShema(TerrariaTilesetRules.Terrain8);

    public static void Run() {
        const int w = 64;
        const int h = 36;

        var map = new short[w, h];

        // Р РµР»СЊРµС„: СЃРёРЅСѓСЃРѕРёРґР°Р»СЊРЅР°СЏ РїРѕРІРµСЂС…РЅРѕСЃС‚СЊ СЃ С…РѕР»РјРёРєРѕРј Рё СЏРјРѕР№
        int[] surface = new int[w];
        for (int x = 0; x < w; x++) {
            surface[x] = 8 + (int)(3.5 * Math.Sin(x * 0.22)) + (int)(2 * Math.Sin(x * 0.07 + 1.3));
            if (x > 12 && x < 17) surface[x] += 5;
            if (x > 40 && x < 44) surface[x] -= 4;
        }
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                map[x, y] = y >= surface[x] ? D : E;

        // РўСЂР°РІР° РЅР° РїРѕРІРµСЂС…РЅРѕСЃС‚Рё (С‡Р°СЃС‚РёС‡РЅРѕ)
        for (int x = 0; x < w; x++)
            map[x, surface[x]] = (x % 7 < 4) ? G : D;

        // РљР°РјРµРЅСЊ РЅРёР¶Рµ РіР»СѓР±РёРЅС‹ 20 (РїРµСЂРµС…РѕРґС‹ Р·РµРјР»СЏ-РєР°РјРµРЅСЊ РІРѕ РІСЃРµС… РЅР°РїСЂР°РІР»РµРЅРёСЏС…)
        for (int x = 0; x < w; x++)
            for (int y = 20; y < h; y++)
                if (Math.Sin(x * 0.35) + Math.Cos(y * 0.5) > 0.2)
                    map[x, y] = S;

        // РџРµС‰РµСЂР°
        for (int x = 18; x < 34; x++) {
            int cy = 24 + (int)(2 * Math.Sin(x * 0.4));
            for (int dy = -2; dy <= 2; dy++)
                if (cy + dy >= 0 && cy + dy < h)
                    map[x, cy + dy] = E;
        }

        // TILES_8: РєРѕРјРїР°РєС‚РЅС‹Р№ СѓС‡Р°СЃС‚РѕРє РЅРѕРІРѕРіРѕ С‚Р°Р№Р»СЃРµС‚Р° вЂ” РІС‹СЃС‚СѓРї РЅР°Рґ РїРѕРІРµСЂС…РЅРѕСЃС‚СЊСЋ
        // РїР»СЋСЃ Р¶РёР»Р° РІ РіР»СѓР±РёРЅРµ; РіСЂР°РЅРёС†С‹ РїСЂРѕС‚РёРІ Р·РµРјР»Рё/РєР°РјРЅСЏ/РІРѕР·РґСѓС…Р° РІРѕ РІСЃРµС… РЅР°РїСЂР°РІР»РµРЅРёСЏС….
        const short T8 = (short)TileTypeEnum.Terrain8;
        for (int x = 34; x <= 52; x++) {
            for (int y = surface[x] - 5; y < surface[x]; y++) {
                if (y >= 0 && map[x, y] == E) map[x, y] = T8;   // РІС‹СЃС‚СѓРї РЅР°Рґ РіСЂСѓРЅС‚РѕРј (С‚РѕР»СЊРєРѕ РІРѕР·РґСѓС…)
            }
            for (int y = surface[x] + 1; y <= surface[x] + 16 && y < h; y++) {
                map[x, y] = T8;                                  // Р¶РёР»Р°: Р·Р°РјРµРЅСЏРµС‚ Рё Р·РµРјР»СЋ, Рё РєР°РјРµРЅСЊ
            }
        }

        // РћС‚РґРµР»СЊРЅС‹Рµ Р±Р»РѕРєРё Рё РјР°Р»С‹Рµ РіСЂСѓРїРїС‹
        map[8, 4] = D;
        map[9, 4] = D;
        map[8, 5] = D;
        map[50, 6] = D;                                 // РѕРґРёРЅРѕС‡РЅС‹Р№ Р±Р»РѕРє
        for (int y = 3; y <= 6; y++) map[58, y] = D;    // РІРµСЂС‚РёРєР°Р»СЊРЅР°СЏ РєРѕР»РѕРЅРЅР°
        for (int x = 20; x <= 25; x++) map[x, 3] = D;   // РіРѕСЂРёР·РѕРЅС‚Р°Р»СЊРЅР°СЏ РїРѕР»РѕСЃР°

        // РљР°РјРµРЅРЅС‹Р№ СЃС‚РѕР»Р± РёР· Р·РµРјР»Рё
        for (int y = 12; y <= 20; y++) map[47, y] = S;

        // Р“РѕСЂРёР·РѕРЅС‚Р°Р»СЊРЅР°СЏ РїРѕР»РѕСЃРєР° РєР°РјРЅСЏ С‚РѕР»С‰РёРЅРѕР№ 1 РІРЅСѓС‚СЂРё Р·РµРјР»Рё, РІС‹С…РѕРґСЏС‰Р°СЏ РІ РїСѓСЃС‚РѕС‚Сѓ
        // РЎ РћР‘Р•РРҐ СЃС‚РѕСЂРѕРЅ (РєР°Рє РІ Р·Р°РґР°С‡Рµ): РІРѕР·РґСѓС€РЅС‹Р№ РєР°СЂРјР°РЅ вЂ” РїРѕР»РѕСЃР° РєР°РјРЅСЏ вЂ” РІРѕР·РґСѓС€РЅС‹Р№ РєР°СЂРјР°РЅ.
        int stripY = 17;
        int stripX0 = 3, stripX1 = 15;
        for (int x = stripX0; x <= stripX1; x++) {
            map[x, stripY] = S;
        }
        // Р—РµРјР»СЏ СЃРІРµСЂС…Сѓ Рё СЃРЅРёР·Сѓ РѕС‚ РїРѕР»РѕСЃРєРё (Merge-СЃРѕСЃРµРґРё)
        for (int x = stripX0; x <= stripX1; x++) {
            if (stripY - 1 >= 0) map[x, stripY - 1] = D;
            if (stripY + 1 < h) map[x, stripY + 1] = D;
        }
        // Р’РѕР·РґСѓС€РЅС‹Рµ РєР°СЂРјР°РЅС‹ СЃР»РµРІР° Рё СЃРїСЂР°РІР°: РїРѕР»РѕСЃР° РІС‹С…РѕРґРёС‚ РІ РїСѓСЃС‚РѕС‚Сѓ СЃ РѕР±РµРёС… СЃС‚РѕСЂРѕРЅ
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
                if (t == E) continue; // РїСѓСЃС‚РѕС‚Сѓ РЅРµ СЂРёСЃСѓРµРј; РЅРµ-terrain С‚Р°Р№Р»С‹ С‚РѕР¶Рµ РЅСѓР¶РЅС‹ (Terrain8)

                for (int dx = -2; dx <= 2; dx++)
                    for (int dy = -2; dy <= 2; dy++) {
                        int mx = x + dx, my = y + dy;
                        window[dx + 2, dy + 2] =
                            (mx >= 0 && my >= 0 && mx < w && my < h) ? map[mx, my] : E;
                    }

                var shema = t == S ? Stone : (t == T8 ? Ore8 : Dirt);
                var info = new TileInfoStructure(t, (short)((x * 7 + y * 3) % 3));
                var f = shema.GetShema(window, info);


                solved++;
                csv.AppendLine($"{x};{y};{(t == S ? 1 : (t == T8 ? 2 : 0))};{f.FrameX};{f.FrameY}");
            }
        }

        var outPath = Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\terrain_frames.csv");
        File.WriteAllText(outPath, csv.ToString());
        Console.WriteLine($"РљР°РґСЂС‹ РїСЂРµРІСЊСЋ: {outPath} ({solved} С‚Р°Р№Р»РѕРІ)");
    }
}

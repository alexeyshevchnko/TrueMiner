using UnityEngine;
using System.Collections;

public class GrassShema : ITileShema {
    private TileInfoStructure currentTileInfo = new TileInfoStructure(0, 0);

    private FrameStructure topRez = new FrameStructure(0, 0);
    private FrameStructure GetTop() {
        topRez.FrameX = currentTileInfo.randomTile;
        return topRez;
    }

    private FrameStructure bottomRez = new FrameStructure(0, 1);
    private FrameStructure GetBottom() {
        bottomRez.FrameX = currentTileInfo.randomTile;
        return bottomRez;
    }

    bool ChackMap(short[,] mapTemplate, short[,] map, TileInfoStructure info) {
        // 0 пусто
        // 1 тогоже типа
        // -1 не тогоже типа
        // -2 не рассматриваем
        // 2 не пусто

        for (int x = 0; x < mapTemplate.GetLength(0); x++) {
            for (int y = 0; y < mapTemplate.GetLength(1); y++) {

                if (mapTemplate[y, x] == 2) {
                    if (map[x, y] == 0)
                        return false;
                }

                if (mapTemplate[y, x] == 0) {
                    if (map[x, y] != 0)
                        return false;
                }

                if (mapTemplate[y, x] == 1) {
                    if (map[x, y] != info.type)
                        return false;
                }

                if (mapTemplate[y, x] == -1) {
                    if (map[x, y] == info.type)
                        return false;
                }
            }
        }

        return true;
    }

    private short[,] mapTop;
    private short[,] mapBottom;
    public FrameStructure GetShema(short[,] map, TileInfoStructure info) {
        FrameStructure rez = new FrameStructure();
        currentTileInfo = info;
        

        // GetTop();
        if (mapTop == null)
            mapTop = new short[,] {
                {-2, -1, -2},
                {-2, -2, -2},
                {-2, -2, -2}
            };
        if (ChackMap(mapTop, map, info)) {
            return GetTop();
        }

        // GetBottom();
        if (mapBottom == null)
            mapBottom = new short[,] {
                {-2, -2, -2},
                {-2, -2, -2},
                {-2, -1, -2}
            };
        if (ChackMap(mapBottom, map, info)) {
            return GetBottom();
        }

        return rez;
    }
}

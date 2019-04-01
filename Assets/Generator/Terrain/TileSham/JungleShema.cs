using UnityEngine;
using System.Collections;

public class JungleShema : ITileShema {
    private TileInfoStructure currentTileInfo = new TileInfoStructure(0, 0);

    private FrameStructure centerRez = new FrameStructure(0, 1);
    private FrameStructure GetCenter() {
        centerRez.FrameX = currentTileInfo.randomTile;
        return centerRez;
    }

    private FrameStructure topRez = new FrameStructure(0, 4);
    private FrameStructure GetTop() {
        topRez.FrameX = currentTileInfo.randomTile;
        return topRez;
    }

    private FrameStructure bottomRez = new FrameStructure(1, 0);
    private FrameStructure GetBottom() {
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

    private short[,] mapCenter;
    private short[,] mapTop;
    private short[,] mapBottom;
    public FrameStructure GetShema(short[,] map, TileInfoStructure info) {
        currentTileInfo = info;
        FrameStructure rez;
        rez = GetCenter();

        // GetCenter();
        if (mapCenter == null) {
            mapCenter = new short[,] {
                {-2, 1, -2},
                {-2, -2, -2},
                {-2, 1, -2}
            };
        }
        if (ChackMap(mapCenter, map, info)) {
            return GetCenter();
        }

        // GetTop();
        if (mapTop == null) {
            mapTop = new short[,] {
                {-2, 1, -2},
                {-2, -2, -2},
                {-2, -1, -2}
            };
        }
        if (ChackMap(mapTop, map, info)) {
            return GetTop();
        }

        // GetBottom();
        if (mapBottom == null) {
            mapBottom = new short[,] {
                {-2, -1, -2},
                {-2, -2, -2},
                {-2, 1, -2}
            };
        }
        if (ChackMap(mapBottom, map, info)) {
            return GetBottom();
        }

        return rez;
    }
}

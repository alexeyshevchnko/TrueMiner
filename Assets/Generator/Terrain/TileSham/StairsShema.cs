using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairsShema : ITileShema {


    private FrameStructure topLRez = new FrameStructure(0, 0);
    private FrameStructure GetTopL() {
       // topLRez.FrameX = currentTileInfo.randomTile;
        return topLRez;
    }

    private FrameStructure topRRez = new FrameStructure(1, 0);
    private FrameStructure GetTopR() {
      //  topRRez.FrameX = currentTileInfo.randomTile;
        return topRRez;
    }

    private FrameStructure bottomLRez = new FrameStructure(0, 1);
    private FrameStructure GetBottomL() {
       // bottomLRez.FrameX = currentTileInfo.randomTile;
        return bottomLRez;
    }

    private FrameStructure bottomRRez = new FrameStructure(1, 1);
    private FrameStructure GetBottomR() {
       // bottomRRez.FrameX = currentTileInfo.randomTile;
        return bottomRRez;
    }

    //GetBottomL

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

    private short[,] mapTopL;
    private short[,] mapBottomL;
    private short[,] mapTopR;
    private short[,] mapBottomR;
    private TileInfoStructure currentTileInfo = new TileInfoStructure(0, 0);

    public FrameStructure GetShema(short[,] map, TileInfoStructure info) {
        currentTileInfo = info;

       // return GetTopL();
        // GetTopL();
        if (mapTopL == null)
            mapTopL = new short[,] {
                {-2, -2, -2},
                {-2, 1, 1},
                {-2, 1, 1}
            };
        if (ChackMap(mapTopL, map, info)) {
            return GetTopL();
        }

        // GetTopR();
        if (mapTopR == null)
            mapTopR = new short[,] {
                {-2, -2, -2},
                {1, 1, -2},
                {1, 1, -2}
            };
        if (ChackMap(mapTopR, map, info)) {
            return GetTopR();
        }


        // mapBottomL();
        if (mapBottomL == null)
            mapBottomL = new short[,] {
                {-2, 1, 1},
                {-2, 1, 1},
                {-2, -2, -2}
            };
        if (ChackMap(mapBottomL, map, info)) {
            return GetBottomL();
        }


        // mapBottomR();
        if (mapBottomR == null)
            mapBottomR = new short[,] {
                {1, 1, -2},
                {1, 1, -2},
                {-2, -2, -2}
            };
        if (ChackMap(mapBottomR, map, info)) {
            return GetBottomR();
        }
        

        return new FrameStructure();
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class TerrariaTreeTileShema : ITileShema {
    private TileInfoStructure currentTileInfo = new TileInfoStructure(0, 0);

    Dictionary<int,Dictionary<int,FrameStructure>>  buffer = new Dictionary<int, Dictionary<int, FrameStructure>>();

    private FrameStructure RootBottomLeftRez1 = new FrameStructure(3, 8);
    private FrameStructure RootBottomLeftRez2 = new FrameStructure(4, 8);
    private FrameStructure RootBottomLeftRez3 = new FrameStructure(5, 8);
    private FrameStructure GetRootBottomLeft() {
        switch (currentTileInfo.randomTile) {
            case 1:
                return RootBottomLeftRez1;
            case 2:
                return RootBottomLeftRez2;
            default:
                return RootBottomLeftRez3;
        }
    }

    private FrameStructure RootBottomRightRez1 = new FrameStructure(7, 8);
    private FrameStructure RootBottomRightRez2 = new FrameStructure(8, 8);
    private FrameStructure RootBottomRightRez3 = new FrameStructure(9, 8);
    private FrameStructure GetRootBottomRight() {
        switch (currentTileInfo.randomTile) {
            case 1:
                return RootBottomRightRez1;
            case 2:
                return RootBottomRightRez2;
            default:
                return RootBottomRightRez3;
        }
    }

    private FrameStructure RootBottomCenterRez1 = new FrameStructure(6, 8);
 //   private FrameStructure RootBottomCenterRez2 = new FrameStructure(4, 7);
  //  private FrameStructure RootBottomCenterRez3 = new FrameStructure(4, 8);
    private FrameStructure GetRootBottomCenter() {
        return RootBottomCenterRez1;
       
    }

    private FrameStructure TopRez1 = new FrameStructure(5, 0);
    private FrameStructure TopRez2 = new FrameStructure(6, 0);
    private FrameStructure TopRez3 = new FrameStructure(0, 9);
    private FrameStructure GetTop() {
        switch (currentTileInfo.randomTile) {
            case 1:
                return TopRez1;
            case 2:
                return TopRez2;
            default:
                return TopRez3;
        }
    }

    private FrameStructure CenterRez1 = new FrameStructure(6, 5);
    private FrameStructure CenterRez2 = new FrameStructure(6, 6);
    private FrameStructure CenterRez3 = new FrameStructure(6, 7);
    private FrameStructure CenterRez5 = new FrameStructure(6, 2);
    private FrameStructure GetCenter() {
        switch (currentTileInfo.randomTile) {
            case 1:
                return CenterRez1;
            case 2:
                return CenterRez2;
            case 5:
                return CenterRez5;
            default:
                return CenterRez3;
        }
    }

    private FrameStructure BranchLeftRez1 = new FrameStructure(5, 1);
    private FrameStructure BranchLeftRez2 = new FrameStructure(5, 2);
    private FrameStructure BranchLeftRez3 = new FrameStructure(5, 3);
    private FrameStructure GetBranchLeft() {
        switch (currentTileInfo.randomTile) {
            case 1:
                return BranchLeftRez1;
            case 2:
                return BranchLeftRez2;
            default:
                return BranchLeftRez3;
        }
    }

 
    private FrameStructure GetBranchLeftCenter() {
        switch (currentTileInfo.randomTile) {
            case 1:
                return CenterRez1;
            case 2:
                return CenterRez2;
            case 5:
                return CenterRez5;
            default:
                return CenterRez3;
        }
    }

    private FrameStructure BranchRightRez1 = new FrameStructure(7, 0);
    private FrameStructure BranchRightRez2 = new FrameStructure(7, 1);
    private FrameStructure BranchRightRez3 = new FrameStructure(7, 2);
    private FrameStructure GetBranchRight() {
        switch (currentTileInfo.randomTile) {
            case 1:
                return BranchRightRez1;
            case 2:
                return BranchRightRez2;
            default:
                return BranchRightRez3;
        }
    }

    private FrameStructure GetBranchRightCenter() {
        switch (currentTileInfo.randomTile) {
            case 1:
                return CenterRez1;
            case 2:
                return CenterRez2;
            case 5:
                return CenterRez5;
            default:
                return CenterRez3;
        }
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

    private short[,] mapRootBottomCenter;
    private short[,] mapRootBottomLeft;
    private short[,] mapRootBottomRight;
    private short[,] mapCenter;
    private short[,] mapTop;
    private short[,] mapBranchLeft;
    private short[,] mapBranchRight;
    private short[,] mapBranchLeftCenter;
    private short[,] mapBranchRightCenter;
    public FrameStructure GetShema(short[,] map, TileInfoStructure info) {
        currentTileInfo = info;
       // return GetCenter();
        //new FrameStructure(7, 0);
        if (info.randomTile >= 100) {
            int y = info.randomTile%100;
            int x = (info.randomTile - y)/100 - 2;

            if (!buffer.ContainsKey(x) || !buffer[x].ContainsKey(y)) {
                buffer[x] = new Dictionary<int, FrameStructure>();
                buffer[x][y] = new FrameStructure(x, y);
            }

            return buffer[x][y];
        }

     //   return GetCenter();

        //GetRootBottomCenter();
        if (mapRootBottomCenter == null) {
            mapRootBottomCenter = new short[,] {
                {-2, 1, -2},
                {-2, 1, -2},
                {-1, -1, -1}
            };
        }
        if (ChackMap(mapRootBottomCenter, map, info)) {
            return GetRootBottomCenter();
        }

        //GetRootBottomLeft();
        if (mapRootBottomLeft == null) {
            mapRootBottomLeft = new short[,] {
                {-1, -1, 1},
                {-1,  1, 1},
                {-1, -1, -1}
            };
        }
        if (ChackMap(mapRootBottomLeft, map, info)) {
            return GetRootBottomLeft();
        }

        //GetRootBottomRight();
        if (mapRootBottomRight == null) {
            mapRootBottomRight = new short[,] {
                {1, -1, -1},
                {1, 1, -1},
                {-1, -1, -1}
            };
        }

      

        if (ChackMap(mapRootBottomRight, map, info)) {
            return GetRootBottomRight();
        }

        return GetCenter();

        //GetCenter();
        if (mapCenter == null) {
            mapCenter = new short[,] {
                {-2, 2, -2},
                {0, -2, 0},
                {-2, 2, -2}
            };
        }
        if (ChackMap(mapCenter, map, info)) {
            return GetCenter();
        }

        //GetTop();
        if (mapTop == null) {
            mapTop = new short[,] {
                {0, 0, 0},
                {-2, -2, -2},
                {-2, 1, -2}
            };
        }
        if (ChackMap(mapTop, map, info)) {
            return GetTop();
        }

        

        //GetBranchLeft();
        if (mapBranchLeft == null) {
            mapBranchLeft = new short[,] {
                {-2, -1, -2},
                {-1, -2, 1},
                {-2, -1, 1}
            };
        }
        if (ChackMap(mapBranchLeft, map, info)) {
            return GetBranchLeft();
        }


        //GetBranchRight();
        if (mapBranchRight == null) {
            mapBranchRight = new short[,] {
                {-2, -1, -2},
                {1, -2, -2},
                {1, -1, -2}
            };
        }
        if (ChackMap(mapBranchRight, map, info)) {
            return GetBranchRight();
        }


        //GetBranchLeftCenter();
        if (mapBranchLeftCenter == null) {
            mapBranchLeftCenter = new short[,] {
                {-2, -2, -2},
                {1, -2, -1},
                {-2, -2, -2}
            };
        }
        if (ChackMap(mapBranchLeftCenter, map, info)) {
            return GetBranchLeftCenter();
        }

        //GetBranchRightCenter();
        if (mapBranchRightCenter == null) {
            mapBranchRightCenter = new short[,] {
                {-2, -2, -2},
                {-1, -2, 1},
                {-2, -2, -2}
            };
        }
        if (ChackMap(mapBranchRightCenter, map, info)) {
            return GetBranchRightCenter();
        }

         
        return GetRootBottomRight();
    }
}

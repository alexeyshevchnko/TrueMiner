using UnityEngine;
using System.Collections;

public class LiquidTileShema : ITileShema {
    private TileInfoStructure currentTileInfo = new TileInfoStructure(0, 0);

    int CountToDeltaHeight(byte count) {
        int delta = Mathf.Clamp(Settings.WATER_COUNT - currentTileInfo.count, 0, Settings.WATER_COUNT);
        return Mathf.FloorToInt(Mathf.Lerp(2, 15, ((float)delta)/Settings.WATER_COUNT));
    }

    private FrameStructure centerRez = new FrameStructure(2, 2, "Liquid_000");
    private FrameStructure GetCenter() {
        return centerRez;    
    }

    private FrameStructure leftTopCornerRez12 = new FrameStructure(0, 4, "Liquid_000");
    private FrameStructure leftTopCornerRez11 = new FrameStructure(1, 4, "Liquid_000");
    private FrameStructure leftTopCornerRez10 = new FrameStructure(2, 4, "Liquid_000");
    private FrameStructure leftTopCornerRez9 = new FrameStructure(3, 4, "Liquid_000");
    private FrameStructure leftTopCornerRez8 = new FrameStructure(4, 4, "Liquid_000");
    private FrameStructure leftTopCornerRez7 = new FrameStructure(5, 4, "Liquid_000");
    private FrameStructure leftTopCornerRez6 = new FrameStructure(6, 4, "Liquid_000");
    private FrameStructure leftTopCornerRez5 = new FrameStructure(7, 4, "Liquid_000");
    private FrameStructure leftTopCornerRez4 = new FrameStructure(8, 4, "Liquid_000");
    private FrameStructure leftTopCornerRez3 = new FrameStructure(9, 4, "Liquid_000");
    private FrameStructure leftTopCornerRez2 = new FrameStructure(10, 4, "Liquid_000");
    private FrameStructure leftTopCornerRez1 = new FrameStructure(11, 4, "Liquid_000");
    private FrameStructure GetLeftTopCorner() {
        //    leftTopCornerRez.DeltaHeight = CountToDeltaHeight(currentTileInfo.count);
        if (currentTileInfo.count == 12)
            return leftTopCornerRez12;
        if (currentTileInfo.count == 11)
            return leftTopCornerRez11;
        if (currentTileInfo.count == 10)
            return leftTopCornerRez10;
        if (currentTileInfo.count == 9)
            return leftTopCornerRez9;
        if (currentTileInfo.count == 8)
            return leftTopCornerRez8;
        if (currentTileInfo.count == 7)
            return leftTopCornerRez7;
        if (currentTileInfo.count == 6)
            return leftTopCornerRez6;
        if (currentTileInfo.count == 5)
            return leftTopCornerRez5;
        if (currentTileInfo.count == 4)
            return leftTopCornerRez4;
        if (currentTileInfo.count == 3)
            return leftTopCornerRez3;
        if (currentTileInfo.count == 2)
            return leftTopCornerRez2;
        if (currentTileInfo.count == 1)
            return leftTopCornerRez1;

        return leftTopCornerRez12;
    }


    private FrameStructure rightTopCornerRez12 = new FrameStructure(0, 5, "Liquid_000");
    private FrameStructure rightTopCornerRez11 = new FrameStructure(1, 5, "Liquid_000");
    private FrameStructure rightTopCornerRez10 = new FrameStructure(2, 5, "Liquid_000");
    private FrameStructure rightTopCornerRez9 = new FrameStructure(3, 5, "Liquid_000");
    private FrameStructure rightTopCornerRez8 = new FrameStructure(4, 5, "Liquid_000");
    private FrameStructure rightTopCornerRez7 = new FrameStructure(5, 5, "Liquid_000");
    private FrameStructure rightTopCornerRez6 = new FrameStructure(6, 5, "Liquid_000");
    private FrameStructure rightTopCornerRez5 = new FrameStructure(7, 5, "Liquid_000");
    private FrameStructure rightTopCornerRez4 = new FrameStructure(8, 5, "Liquid_000");
    private FrameStructure rightTopCornerRez3 = new FrameStructure(9, 5, "Liquid_000");
    private FrameStructure rightTopCornerRez2 = new FrameStructure(10, 5, "Liquid_000");
    private FrameStructure rightTopCornerRez1 = new FrameStructure(11, 5, "Liquid_000");
    private FrameStructure GetRightTopCorner() {
        if (currentTileInfo.count == 12)
            return rightTopCornerRez12;
        if (currentTileInfo.count == 11)
            return rightTopCornerRez11;
        if (currentTileInfo.count == 10)
            return rightTopCornerRez10;
        if (currentTileInfo.count == 9)
            return rightTopCornerRez9;
        if (currentTileInfo.count == 8)
            return rightTopCornerRez8;
        if (currentTileInfo.count == 7)
            return rightTopCornerRez7;
        if (currentTileInfo.count == 6)
            return rightTopCornerRez6;
        if (currentTileInfo.count == 5)
            return rightTopCornerRez5;
        if (currentTileInfo.count == 4)
            return rightTopCornerRez4;
        if (currentTileInfo.count == 3)
            return rightTopCornerRez3;
        if (currentTileInfo.count == 2)
            return rightTopCornerRez2;
        if (currentTileInfo.count == 1)
            return rightTopCornerRez1;

        return rightTopCornerRez12;
    }

    private FrameStructure oneHorizontalCenterRez = new FrameStructure(0, 1, "Liquid_000");
    private FrameStructure GetOneHorizontalCenter() {
        return oneHorizontalCenterRez;
    }

    private FrameStructure oneVerticalCenterRez = new FrameStructure(1, 1, "Liquid_000");
    private FrameStructure GetOneVerticalCenter() {
        return oneVerticalCenterRez;
    }

    private FrameStructure oneRightRez = new FrameStructure(1, 0, "Liquid_000");
    private FrameStructure GetOneRight() {
        return oneRightRez;
    }

    private FrameStructure oneLeftRez = new FrameStructure(0, 0, "Liquid_000");
    private FrameStructure GetOneLeft() {
        return oneLeftRez;
    }

    private FrameStructure oneTopRez12 = new FrameStructure(0, 6, "Liquid_000");
    private FrameStructure oneTopRez11 = new FrameStructure(1, 6, "Liquid_000");
    private FrameStructure oneTopRez10 = new FrameStructure(2, 6, "Liquid_000");
    private FrameStructure oneTopRez9 = new FrameStructure(3, 6, "Liquid_000");
    private FrameStructure oneTopRez8 = new FrameStructure(4, 6, "Liquid_000");
    private FrameStructure oneTopRez7 = new FrameStructure(5, 6, "Liquid_000");
    private FrameStructure oneTopRez6 = new FrameStructure(6, 6, "Liquid_000");
    private FrameStructure oneTopRez5 = new FrameStructure(7, 6, "Liquid_000");
    private FrameStructure oneTopRez4 = new FrameStructure(8, 6, "Liquid_000");
    private FrameStructure oneTopRez3 = new FrameStructure(9, 6, "Liquid_000");
    private FrameStructure oneTopRez2 = new FrameStructure(10, 6, "Liquid_000");
    private FrameStructure oneTopRez1 = new FrameStructure(11, 6, "Liquid_000");

    private FrameStructure GetOneTop() {
        if (currentTileInfo.count == 12)
            return oneTopRez12;
        if (currentTileInfo.count == 11)
            return oneTopRez11;
        if (currentTileInfo.count == 10)
            return oneTopRez10;
        if (currentTileInfo.count == 9)
            return oneTopRez9;
        if (currentTileInfo.count == 8)
            return oneTopRez8;
        if (currentTileInfo.count == 7)
            return oneTopRez7;
        if (currentTileInfo.count == 6)
            return oneTopRez6;
        if (currentTileInfo.count == 5)
            return oneTopRez5;
        if (currentTileInfo.count == 4)
            return oneTopRez4;
        if (currentTileInfo.count == 3)
            return oneTopRez3;
        if (currentTileInfo.count == 2)
            return oneTopRez2;
        if (currentTileInfo.count == 1)
            return oneTopRez1;

        return oneTopRez12;
    }

    private FrameStructure oneBottomRez = new FrameStructure(2, 1, "Liquid_000");
    private FrameStructure GetOneBottom() {
        return oneBottomRez;
    }

    private FrameStructure topRez12 = new FrameStructure(0, 3, "Liquid_000");
    private FrameStructure topRez11 = new FrameStructure(1, 3, "Liquid_000");
    private FrameStructure topRez10 = new FrameStructure(2, 3, "Liquid_000");
    private FrameStructure topRez9 = new FrameStructure(3, 3, "Liquid_000");
    private FrameStructure topRez8 = new FrameStructure(4, 3, "Liquid_000");
    private FrameStructure topRez7 = new FrameStructure(5, 3, "Liquid_000");
    private FrameStructure topRez6 = new FrameStructure(6, 3, "Liquid_000");
    private FrameStructure topRez5 = new FrameStructure(7, 3, "Liquid_000");
    private FrameStructure topRez4 = new FrameStructure(8, 3, "Liquid_000");
    private FrameStructure topRez3 = new FrameStructure(9, 3, "Liquid_000");
    private FrameStructure topRez2 = new FrameStructure(10, 3, "Liquid_000");
    private FrameStructure topRez1 = new FrameStructure(11, 3, "Liquid_000");
    private FrameStructure GetTop() {
        if(currentTileInfo.count == 12)
            return topRez12;
        if (currentTileInfo.count == 11)
            return topRez11;
        if (currentTileInfo.count == 10)
            return topRez10;
        if (currentTileInfo.count == 9)
            return topRez9;
        if (currentTileInfo.count == 8)
            return topRez8;
        if (currentTileInfo.count == 7)
            return topRez7;
        if (currentTileInfo.count == 6)
            return topRez6;
        if (currentTileInfo.count == 5)
            return topRez5;
        if (currentTileInfo.count == 4)
            return topRez4;
        if (currentTileInfo.count == 3)
            return topRez3;
        if (currentTileInfo.count == 2)
            return topRez2;
        if (currentTileInfo.count == 1)
            return topRez1;

        return topRez12;
        //  topRez.FrameX = currentTileInfo.randomTile;
        //  topRez.DeltaHeight = CountToDeltaHeight(currentTileInfo.count);
        //return topRez;
    }

    private FrameStructure leftRez = new FrameStructure(5, 1, "Liquid_000");
    private FrameStructure GetLeft() {
        return leftRez;
    }

    private FrameStructure rightRez = new FrameStructure(4, 1, "Liquid_000");
    private FrameStructure GetRight() {
        return rightRez;
    }

    
    private FrameStructure oneRez12 = new FrameStructure(0, 7, "Liquid_000");
    private FrameStructure oneRez11 = new FrameStructure(1, 7, "Liquid_000");
    private FrameStructure oneRez10 = new FrameStructure(2, 7, "Liquid_000");
    private FrameStructure oneRez9 = new FrameStructure(3, 7, "Liquid_000");
    private FrameStructure oneRez8 = new FrameStructure(4, 7, "Liquid_000");
    private FrameStructure oneRez7 = new FrameStructure(5, 7, "Liquid_000");
    private FrameStructure oneRez6 = new FrameStructure(6, 7, "Liquid_000");
    private FrameStructure oneRez5 = new FrameStructure(7, 7, "Liquid_000");
    private FrameStructure oneRez4 = new FrameStructure(8, 7, "Liquid_000");
    private FrameStructure oneRez3 = new FrameStructure(9, 7, "Liquid_000");
    private FrameStructure oneRez2 = new FrameStructure(10, 7, "Liquid_000");
    private FrameStructure oneRez1 = new FrameStructure(11, 7, "Liquid_000");
    private FrameStructure GetOne() {
        if (currentTileInfo.count == 12)
            return oneRez12;
        if (currentTileInfo.count == 11)
            return oneRez11;
        if (currentTileInfo.count == 10)
            return oneRez10;
        if (currentTileInfo.count == 9)
            return oneRez9;
        if (currentTileInfo.count == 8)
            return oneRez8;
        if (currentTileInfo.count == 7)
            return oneRez7;
        if (currentTileInfo.count == 6)
            return oneRez6;
        if (currentTileInfo.count == 5)
            return oneRez5;
        if (currentTileInfo.count == 4)
            return oneRez4;
        if (currentTileInfo.count == 3)
            return oneRez3;
        if (currentTileInfo.count == 2)
            return oneRez2;
        if (currentTileInfo.count == 1)
            return oneRez1;

        return oneRez12;
    }

    private FrameStructure leftBottomCornerRez = new FrameStructure(4, 0, "Liquid_000");
    private FrameStructure GetLeftBottomCorner() {
        return leftBottomCornerRez;
    }

    private FrameStructure rightBottomCornerRez = new FrameStructure(5, 0, "Liquid_000");
    private FrameStructure GetRightBottomCorner() {
        return rightBottomCornerRez;
    }

    private FrameStructure bottomRez = new FrameStructure(1, 2, "Liquid_000");
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
                    if (map[x, y] != info.liquidType)
                        return false;
                }

                if (mapTemplate[y, x] == -1) {
                    if (map[x, y] == info.liquidType)
                        return false;
                }
            }
        }

        return true;
    }

    void PrintMap(short[,] map) {
        string rez = "";
        for (int y = 0; y < map.GetLength(0); y++) {
            for (int x = 0; x < map.GetLength(1); x++) {
                rez += ", " + map[x, y];
            }
            rez += "\n";
        }

        Debug.LogError(rez);
    }

    private short[,] mapCenter;
    private short[,] mapLeftCorner;
    private short[,] mapRightCorner;
    private short[,] mapOneHorizontalCenter;
    private short[,] mapOneVerticalCenter;
    private short[,] mapOneRight;
    private short[,] mapOneLeft;
    private short[,] mapOneTop;
    private short[,] mapOneBottom;
    private short[,] mapTop;
    private short[,] mapBottom;
    private short[,] mapLeft;
    private short[,] mapRight;
    private short[,] mapLeftBottomCorner;
    private short[,] mapmapLeftBottomCorner;
    public FrameStructure GetShema(short[,] map, TileInfoStructure info) {

      //  return new FrameStructure(1, 0, "Liquid_000");
       // topRez.FrameX = currentTileInfo.randomTile;
       // topRez.DeltaHeight = 0;//CountToDeltaHeight(currentTileInfo.count);
       // return topRez;


        currentTileInfo = info;

        var rez = new FrameStructure(0, 0);
        rez.TileCollection = "Liquid_000";
        //return GetTop();
        // 0 пусто
        // 1 тогоже типа
        // -1 не тогоже типа
        // -2 не рассматриваем
        // 2 не пусто

       // GetCenter();
        if (mapCenter == null) {
            mapCenter = new short[,] {
                {-2, 1, -2},
                {1, -2, 1},
                {-2, 1, -2}
            };
        }

        if (ChackMap(mapCenter, map, info)) {
            return GetCenter();
        }

        

        //GetLeftCorner();
        if (mapLeftCorner == null) {
            mapLeftCorner = new short[,] {
                {-2, -1, -2},
                {-1, -2, 1},
                {-2, 1, -2}
            };
        }
        if (ChackMap(mapLeftCorner, map, info)) {
            return GetLeftTopCorner();
        }

        //GetRightCorner();
        if (mapRightCorner == null) {
            mapRightCorner = new short[,] {
                {-2, -1, -2},
                {1, -2, -1},
                {-2, 1, -2}
            };
        }
        if (ChackMap(mapRightCorner, map, info)) {
            return GetRightTopCorner();
        }

        //GetOneHorizontalCenter();
        if (mapOneHorizontalCenter == null) {
            mapOneHorizontalCenter = new short[,] {
                {-2, -1, -2},
                {1, -2, 1},
                {-2, -1, -2}
            };
        }
        if (ChackMap(mapOneHorizontalCenter, map, info)) {
            return GetTop();
        }

        //GetOneVerticalCenter();
        if (mapOneVerticalCenter == null) {
            mapOneVerticalCenter = new short[,] {
                {-2, 1, -2},
                {-1, -2, -1},
                {-2, 1, -2}
            };
        }
        if (ChackMap(mapOneVerticalCenter, map, info)) {
            return GetOneVerticalCenter();  
        }

        //GetOneRight();
        if (mapOneRight == null) {
            mapOneRight = new short[,] {
                {-2, -1, -2},
                {-1, -2, 1},
                {-2, -1, -2}
            };
        }
        if (ChackMap(mapOneRight, map, info)) {
            return GetTop();
        }

        //GetOneLeft();
        if (mapOneLeft == null) {
            mapOneLeft = new short[,] {
                {-2, -1, -2},
                {1, -2, -1},
                {-2, -1, -2}
            };
        }
        if (ChackMap(mapOneLeft, map, info)) {
            return GetTop();
        }

        //GetOneTop();
        if (mapOneTop == null) {
            mapOneTop = new short[,] {
                {-2, -1, -2},
                {-1, -2, -1},
                {-2, 1, -2}
            };
        }
        if (ChackMap(mapOneTop, map, info)) {
            return GetOneTop();
        }

        //GetOneBottom();
        if (mapOneBottom == null) {
            mapOneBottom = new short[,] {
                {-2, 1, -2},
                {-1, -2, -1},
                {-2, -1, -2}
            };
        }
        if (ChackMap(mapOneBottom, map, info)) {
            return GetOneBottom();
        }

        //GetTop();
        if (mapTop == null) {
            mapTop = new short[,] {
                {-2, -1, -2},
                {1, -2, 1},
                {-2, 1, -2}
            };
        }
        if (ChackMap(mapTop, map, info)) {
            return GetTop();
        }

        //GetBottom();
        if (mapBottom == null) {
            mapBottom = new short[,] {
                {-2, 1, -2},
                {1, -2, 1},
                {-2, -1, -2}
            };
        }
        if (ChackMap(mapBottom, map, info)) {
            return GetBottom();
        }

        //GetLeft();
        if (mapLeft == null) {
            mapLeft = new short[,] {
                {-2, 1, -2},
                {-1, -2, 1},
                {-2, 1, -2}
            };
        }
        if (ChackMap(mapLeft, map, info)) {
            return GetLeft();
        }

        //GetRight();
        if (mapRight == null) {
            mapRight = new short[,] {
                {-2, 1, -2},
                {1, -2, -1},
                {-2, 1, -2}
            };
        }
        if (ChackMap(mapRight, map, info)) {
            return GetRight();
        }


        //GetLeftBottomCorner();
        if (mapLeftBottomCorner == null) {
            mapLeftBottomCorner = new short[,] {
                {-2, 1, -2},
                {-1, -2, 1},
                {-2, -1, -2}
            };
        }
        if (ChackMap(mapLeftBottomCorner, map, info)) {
            return GetLeftBottomCorner();
        }

        //GetRightBottomCorner();
        if (mapmapLeftBottomCorner == null) {
            mapmapLeftBottomCorner = new short[,] {
                {-2, 1, -2},
                {1, -2, -1},
                {-2, -1, -2}
            };
        }
        if (ChackMap(mapmapLeftBottomCorner, map, info)) {
            return GetRightBottomCorner();
        }


        return GetOne();
    }
} 

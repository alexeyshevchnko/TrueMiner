using UnityEngine;
using System.Collections;

public class TerrariaTreeCapTileShema : ITileShema {
    private TileInfoStructure currentTileInfo = new TileInfoStructure(0, 0);

    private FrameStructure rez = new FrameStructure();
    private FrameStructure Get00() {
        rez.FrameX = 0 + currentTileInfo.randomTile*5;
        rez.FrameY = 0;
        return rez;
    }
    private FrameStructure Get01() {
        rez.FrameX = 0 + currentTileInfo.randomTile * 5;
        rez.FrameY = 1;
        return rez;
    }

    private FrameStructure Get02() {
        rez.FrameX = 0 + currentTileInfo.randomTile * 5;
        rez.FrameY = 2;
        return rez;
    }

    private FrameStructure Get03() {
        rez.FrameX = 0 + currentTileInfo.randomTile * 5;
        rez.FrameY = 3;
        return rez;
    }

    private FrameStructure Get04() {
        rez.FrameX = 0 + currentTileInfo.randomTile * 5;
        rez.FrameY = 4;
        return rez;
    }

    private FrameStructure Get10() {
        rez.FrameX = 1 + currentTileInfo.randomTile * 5;
        rez.FrameY = 0;
        return rez;
    }
    private FrameStructure Get11() {
        rez.FrameX = 1 + currentTileInfo.randomTile * 5;
        rez.FrameY = 1;
        return rez;
    }

    private FrameStructure Get12() {
        rez.FrameX = 1 + currentTileInfo.randomTile * 5;
        rez.FrameY = 2;
        return rez;
    }

    private FrameStructure Get13() {
        rez.FrameX = 1 + currentTileInfo.randomTile * 5;
        rez.FrameY = 3;
        return rez;
    }

    private FrameStructure Get14() {
        rez.FrameX = 1 + currentTileInfo.randomTile * 5;
        rez.FrameY = 4;
        return rez;
    }

    private FrameStructure Get20() {
        rez.FrameX = 2 + currentTileInfo.randomTile * 5;
        rez.FrameY = 0;
        return rez;
    }
    private FrameStructure Get21() {
        rez.FrameX = 2 + currentTileInfo.randomTile * 5;
        rez.FrameY = 1;
        return rez;
    }

    private FrameStructure Get22() {
        rez.FrameX = 2 + currentTileInfo.randomTile * 5;
        rez.FrameY = 2;
        return rez;
    }

    private FrameStructure Get23() {
        rez.FrameX = 2 + currentTileInfo.randomTile * 5;
        rez.FrameY = 3;
        return rez;
    }

    private FrameStructure Get24() {
        rez.FrameX = 2 + currentTileInfo.randomTile * 5;
        rez.FrameY = 4;
        return rez;
    }

    private FrameStructure Get30() {
        rez.FrameX = 3 + currentTileInfo.randomTile * 5;
        rez.FrameY = 0;
        return rez;
    }
    private FrameStructure Get31() {
        rez.FrameX = 3 + currentTileInfo.randomTile * 5;
        rez.FrameY = 1;
        return rez;
    }

    private FrameStructure Get32() {
        rez.FrameX = 3 + currentTileInfo.randomTile * 5;
        rez.FrameY = 2;
        return rez;
    }

    private FrameStructure Get33() {
        rez.FrameX = 3 + currentTileInfo.randomTile * 5;
        rez.FrameY = 3;
        return rez;
    }

    private FrameStructure Get34() {
        rez.FrameX = 3 + currentTileInfo.randomTile * 5;
        rez.FrameY = 4;
        return rez;
    }

    private FrameStructure Get40() {
        rez.FrameX = 4 + currentTileInfo.randomTile * 5;
        rez.FrameY = 0;
        return rez;
    }
    private FrameStructure Get41() {
        rez.FrameX = 4 + currentTileInfo.randomTile * 5;
        rez.FrameY = 1;
        return rez;
    }

    private FrameStructure Get42() {
        rez.FrameX = 4 + currentTileInfo.randomTile * 5;
        rez.FrameY = 2;
        return rez;
    }

    private FrameStructure Get43() {
        rez.FrameX = 4 + currentTileInfo.randomTile * 5;
        rez.FrameY = 3;
        return rez;
    }

    private FrameStructure Get44() {
        rez.FrameX = 4 + currentTileInfo.randomTile * 5;
        rez.FrameY = 4;
        return rez;
    }

    bool ChackMap(short[,] mapTemplate, short[,] map, TileInfoStructure info) {
        // 0 пусто
        // 1 тогоже типа
        // -1 не тогоже типа
        // -2 не рассматриваем
        // 2 не пусто

        for (int x = 0; x < mapTemplate.GetLength(0); x++) {
            for (int y = 0; y < mapTemplate.GetLength(1); y++) {

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

    private short[,] map00;
    private short[,] map10;
    private short[,] map20;
    private short[,] map30;
    private short[,] map40;
    private short[,] map01;
    private short[,] map02;
    private short[,] map03;
    private short[,] map04;
    private short[,] map11;
    private short[,] map21;
    private short[,] map31;
    private short[,] map41;
    private short[,] map12;
    private short[,] map22;
    private short[,] map23;
    private short[,] map24;
    private short[,] map32;
    private short[,] map42;
    private short[,] map13;
    private short[,] map33;
    private short[,] map43;
    private short[,] map14;   
    private short[,] map34;
    private short[,] map44;
    
    //private short[,] map41;
    //map12
    public FrameStructure GetShema(short[,] map, TileInfoStructure info) {
       // PrintMap(map);
        currentTileInfo = info;

        // 0 пусто
        // 1 тогоже типа
        // -1 не тогоже типа
        // -2 не рассматриваем
        // 2 не пусто

        //Get00()
        if (map00 == null) {
            map00 = new short[,] {
                {-2, -2, -2, -2, -2},
                {-2, -1, -1, -1, -1},
                {-2, -1, 1, 1, 1},
                {-2, -1, 1, 1, 1},
                {-2, -1, 1, 1, 1}
            };
        }
        if (ChackMap(map00, map, info)) {
            return Get00();
        }

        //Get10()
        if (map10 == null) {
            map10 = new short[,] {
                {-2, -2, -2, -2, -2},
                {-1, -1, -1, -1, -1},
                {-1, 1, 1, 1, 1},
                {-1, 1, 1, 1, 1},
                {-1, 1, 1, 1, 1}
            };
        }
        if (ChackMap(map10, map, info)) {
            return Get10();
        }

        //Get20()
        if (map20 == null) {
            map20 = new short[,] {
                {-1, -1, -1, -1, -1},
                {-1, -1, -1, -1, -1},
                {1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1}
            };
        }
        if (ChackMap(map20, map, info)) {
            return Get20();
        }

        //Get30()
        if (map30 == null) {
            map30 = new short[,] {
                {-2, -2, -2, -2, -2},
                {-1, -1, -1, -1, -1},
                {1, 1, 1, 1, -1},
                {1, 1, 1, 1, -1},
                {1, 1, 1, 1, -1}
            };
        }
        if (ChackMap(map30, map, info)) {
            return Get30();
        }

        //Get40()
        if (map40 == null) {
            map40 = new short[,] {
                {-2, -2, -2, -2, -2},
                {-1, -1, -1, -1, -2},
                {1, 1, 1, -1, -2},
                {1, 1, 1, -1, -2},
                {1, 1, 1, -1, -2}
            };
        }
        if (ChackMap(map40, map, info)) {
            return Get40();
        }


        //Get01()
        if (map01 == null) {
            map01 = new short[,] {
                {-2, -1, -1, -1, -1},
                {-2, -1, 1, 1, 1},
                {-2, -1, 1, 1, 1},
                {-2, -1, 1, 1, 1},
                {-2, -1, 1, 1, 1}
            };
        }
        if (ChackMap(map01, map, info)) {
            return Get01();
        }

        //Get11()
        if (map11 == null) {
            map11 = new short[,] {
                {-1, -1, -1, -1, -1},
                {-1, 1, 1, 1, 1},
                {-1, 1, 1, 1, 1},
                {-1, 1, 1, 1, 1},
                {-1, 1, 1, 1, 1}
            };
        }
        if (ChackMap(map11, map, info)) {
            return Get11();
        }

        //Get21()
        if (map21 == null) {
            map21 = new short[,] {
                {-1, -1, -1, -1, -1},
                {1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1}
            };
        }
        if (ChackMap(map21, map, info)) {
            return Get21();
        }

        //Get31()
        if (map31 == null) {
            map31 = new short[,] {
                {-1, -1, -1, -1, -1},
                {1, 1, 1, 1, -1},
                {1, 1, 1, 1, -1},
                {1, 1, 1, 1, -1},
                {1, 1, 1, 1, -1}
            };
        }
        if (ChackMap(map31, map, info)) {
            return Get31();
        }

        //Get41()
        if (map41 == null) {
            map41 = new short[,] {
                {-1, -1, -1, -1, -2},
                {1, 1, 1, -1, -2},
                {1, 1, 1, -1, -2},
                {1, 1, 1, -1, -2},
                {1, 1, 1, -1, -2}
            };
        }
        if (ChackMap(map41, map, info)) {
            return Get41();
        }

        //Get02()
        if (map02 == null) {
            map02 = new short[,] {
                {-2, -1, 1, 1, 1},
                {-2, -1, 1, 1, 1},
                {-2, -1, 1, 1, 1},
                {-2, -1, 1, 1, 1},
                {-2, -1, 1, 1, 1}
            };
        }
        if (ChackMap(map02, map, info)) {
            return Get02();
        }

        //Get12()
        if (map12 == null) {
            map12 = new short[,] {
                {-1, 1, 1, 1, 1},
                {-1, 1, 1, 1, 1},
                {-1, 1, 1, 1, 1},
                {-1, 1, 1, 1, 1},
                {-1, 1, 1, 1, 1}
            };
        }
        if (ChackMap(map12, map, info)) {
            return Get12();
        }

        //Get22()
        if (map22 == null) {
            map22 = new short[,] {
                {1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1}
            };
        }
        if (ChackMap(map22, map, info)) {
            return Get22();
        }

        //Get32()
        if (map32 == null) {
            map32 = new short[,] {
                {1, 1, 1, 1, -1},
                {1, 1, 1, 1, -1},
                {1, 1, 1, 1, -1},
                {1, 1, 1, 1, -1},
                {1, 1, 1, 1, -1}
            };
        }
        if (ChackMap(map32, map, info)) {
            return Get32();
        }

        //Get42()
        if (map42 == null) {
            map42 = new short[,] {
                {1, 1, 1, -1, -2},
                {1, 1, 1, -1, -2},
                {1, 1, 1, -1, -2},
                {1, 1, 1, -1, -2},
                {1, 1, 1, -1, -2}
            };
        }
        if (ChackMap(map42, map, info)) {
            return Get42();
        }

        //Get03()
        if (map03 == null) {
            map03 = new short[,] {
                {-2, -1, 1, 1, 1},
                {-2, -1, 1, 1, 1},
                {-2, -1, 1, 1, 1},
                {-2, -1, 1, 1, 1},
                {-2, -1, -1, -1, -1}
            };
        }
        if (ChackMap(map03, map, info)) {
            return Get03();
        }

        //Get13()
        if (map13 == null) {
            map13 = new short[,] {
                {-1, 1, 1, 1, 1},
                {-1, 1, 1, 1, 1},
                {-1, 1, 1, 1, 1},
                {-1, 1, 1, 1, 1},
                {-1, -1, -1, -1, -1}
            };
        }
        if (ChackMap(map13, map, info)) {
            return Get13();
        }

        //Get23()
        if (map23 == null) {
            map23 = new short[,] {
                {1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1},
                {-1, -1, -1, -1, -1}
            };
        }
        if (ChackMap(map23, map, info)) {
            return Get23();
        }

        //Get33()
        if (map33 == null) {
            map33 = new short[,] {
                {1, 1, 1, 1, -1},
                {1, 1, 1, 1, -1},
                {1, 1, 1, 1, -1},
                {1, 1, 1, 1, -1},
                {-1, -1, -1, -1, -1}
            };
        }
        if (ChackMap(map33, map, info)) {
            return Get33();
        }

        //Get43()
        if (map43 == null) {
            map43 = new short[,] {
                {1, 1, 1, -1, -2},
                {1, 1, 1, -1, -2},
                {1, 1, 1, -1, -2},
                {1, 1, 1, -1, -2},
                {-1, -1, -1, -1, -2}
            };
        }
        if (ChackMap(map43, map, info)) {
            return Get43();
        }

        //Get04()
        if (map04 == null) {
            map04 = new short[,] {
                {-2, -1, 1, 1, 1},
                {-2, -1, 1, 1, 1},
                {-2, -1, 1, 1, 1},
                {-2, -1, -1, -1, -1},
                {-2, -2, -2, -2, -2}
            };
        }
        if (ChackMap(map04, map, info)) {
            return Get04();
        }

        //Get14()
        if (map14 == null) {
            map14 = new short[,] {
                {-1, 1, 1, 1, 1},
                {-1, 1, 1, 1, 1},
                {-1, 1, 1, 1, 1},
                {-1, -1, -1, -1, -1},
                {-2, -2, -2, -2, -2}
            };
        }
        if (ChackMap(map14, map, info)) {
            return Get14();
        }

        //Get24()
        if (map24 == null) {
            map24 = new short[,] {
                {1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1},
                {1, 1, 1, 1, 1},
                {-1, -1, -1, -1, -1},
                {-2, -2, -2, -2, -2}
            };
        }
        if (ChackMap(map24, map, info)) {
            return Get24();
        }

        //Get34()
        if (map34 == null) {
            map34 = new short[,] {
                {1, 1, 1, 1, -1},
                {1, 1, 1, 1, -1},
                {1, 1, 1, 1, -1},
                {-1, -1, -1, -1, -1},
                {-2, -2, -2, -2, -2}
            };
        }
        if (ChackMap(map34, map, info)) {
            return Get34();
        }

        //Get44()
        if (map44 == null) {
            map44 = new short[,] {
                {1, 1, 1, -1, -2},
                {1, 1, 1, -1, -2},
                {1, 1, 1, -1, -2},
                {-1, -1, -1, -1, -2},
                {-2, -2, -2, -2, -2}
            };
        }
        if (ChackMap(map44, map, info)) {
            return Get44();
        }

        return Get22();
    }
}

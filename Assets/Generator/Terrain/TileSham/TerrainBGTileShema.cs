using UnityEngine;
using System.Collections;


public class TerrainBGTileShema : ITileShema {
    private TileInfoStructure currentTileInfo = new TileInfoStructure(0, 0);

    private FrameStructure CenterRez1 = new FrameStructure(1, 1);
    private FrameStructure CenterRez2 = new FrameStructure(2, 1);
    private FrameStructure CenterRez3 = new FrameStructure(3, 1);
    private FrameStructure GetCenter() {
        switch (currentTileInfo.randomTile) {
            case 1:
                return CenterRez1;
            case 2:
                return CenterRez2;
            default:
                return CenterRez3;
        }
    }

    private FrameStructure TopCenterRez1 = new FrameStructure(1, 0);
    private FrameStructure TopCenterRez2 = new FrameStructure(2, 0);
    private FrameStructure TopCenterRez3 = new FrameStructure(3, 0);
    private FrameStructure GetTopCenter() {
        switch (currentTileInfo.randomTile) {
            case 1:
                return TopCenterRez1;
            case 2:
                return TopCenterRez2;
            default:
                return TopCenterRez3;
        }
    }

    private FrameStructure TopLeftRez = new FrameStructure(0, 3);
    private FrameStructure GetTopLeft() {
        return TopLeftRez;
       
    }

    private FrameStructure TopRightRez = new FrameStructure(1, 3);
    private FrameStructure GetTopRight() {
        return TopRightRez;
    }

    private FrameStructure BottomCenterRez1 = new FrameStructure(1, 2);
    private FrameStructure BottomCenterRez2 = new FrameStructure(2, 2);
    private FrameStructure BottomCenterRez3 = new FrameStructure(3, 2);

    private FrameStructure GetBottomCenter() {
        switch (currentTileInfo.randomTile) {
            case 1:
                return BottomCenterRez1;
            case 2:
                return BottomCenterRez2;
            default:
                return BottomCenterRez3;
        }
    }

    private FrameStructure BottomLeftRez = new FrameStructure(0, 4);
    private FrameStructure GetBottomLeft() {
        return BottomLeftRez;
    }

    private FrameStructure BottomRightRez = new FrameStructure(1, 4);
    private FrameStructure GetBottomRight() {
        return BottomRightRez;
    }

    private FrameStructure CenterLeftRez1 = new FrameStructure(0, 0);
    private FrameStructure CenterLeftRez2 = new FrameStructure(0, 1);
    private FrameStructure CenterLeftRez3 = new FrameStructure(0, 2);
    private FrameStructure GetCenterLeft() {
        switch (currentTileInfo.randomTile) {
            case 1:
                return CenterLeftRez1;
            case 2:
                return CenterLeftRez2;
            default:
                return CenterLeftRez3;
        }
    }

    private FrameStructure CenterRightRez1 = new FrameStructure(4, 0);
    private FrameStructure CenterRightRez2 = new FrameStructure(4, 1);
    private FrameStructure CenterRightRez3 = new FrameStructure(4, 2);
    private FrameStructure GetCenterRight() {
        switch (currentTileInfo.randomTile) {
            case 1:
                return CenterRightRez1;
            case 2:
                return CenterRightRez2;
            default:
                return CenterRightRez3;
        }
    }

    //
    private FrameStructure OneRez = new FrameStructure(9, 3);
    private FrameStructure GetOne() {
        return OneRez;
    }

    private FrameStructure OneHorizontalCenterRez = new FrameStructure(2, 3);
    private FrameStructure GetOneHorizontalCenter() {
        return OneHorizontalCenterRez;
    }

    private FrameStructure OneHorizontalLeftRez = new FrameStructure(3, 3);
    private FrameStructure GetOneHorizontalLeft() {
        return OneHorizontalLeftRez;
    }

    private FrameStructure OneHorizontalRightRez = new FrameStructure(4, 3);
    private FrameStructure GetOneHorizontalRight() {
        return OneHorizontalRightRez;
    }

    private FrameStructure OneVerticalCenterRez = new FrameStructure(4, 4);
    private FrameStructure GetOneVerticalCenter() {
        return OneVerticalCenterRez;
    }

    private FrameStructure OneVerticalTopRez = new FrameStructure(3, 4);
    private FrameStructure GetOneVerticalTop() {
        return OneVerticalTopRez;
    }

    private FrameStructure OneVerticalBottomRez = new FrameStructure(2, 4);
    private FrameStructure GetOneVerticalBottom() {
        return OneVerticalBottomRez;
    }
    //
    private FrameStructure InnerCornerTopLefrRez = new FrameStructure(0, 5);
    private FrameStructure GetInnerCornerTopLefr() {
        return InnerCornerTopLefrRez;
    }

    private FrameStructure InnerCornerTopRightRez = new FrameStructure(1, 5);
    private FrameStructure GetInnerCornerTopRight() {
        return InnerCornerTopRightRez;
    }

    private FrameStructure InnerCornerBottomLefrRez = new FrameStructure(0, 6);
    private FrameStructure GetInnerCornerBottomLefr() {
        return InnerCornerBottomLefrRez;
    }

    private FrameStructure InnerCornerBottomRightRez = new FrameStructure(1, 6);
    private FrameStructure GetInnerCornerBottomRight() {
        return InnerCornerBottomRightRez;
    }


    FrameStructure topCenter = new FrameStructure(1, 0);
    FrameStructure topLeft = new FrameStructure(0, 0);
    FrameStructure topRight = new FrameStructure(2, 0);
    FrameStructure bottomCenter = new FrameStructure(1, 2);
    FrameStructure bottomLeft = new FrameStructure(0, 2);
    FrameStructure bottomRight = new FrameStructure(2, 2);
    FrameStructure leftCenter = new FrameStructure(0, 1);
    FrameStructure rightCenter = new FrameStructure(2, 1);

    FrameStructure free = new FrameStructure(4, 6);

    public FrameStructure GetShema(short[,] map, TileInfoStructure info) {
        currentTileInfo = info;

        //return CenterLeftRez;
        if (map[1, 1] == 0) {
            return free; 
        }

        //GetOneHorizontalCenter();
        if (map[topCenter.FrameX, topCenter.FrameY] == 0 &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] == 0 &&
            map[leftCenter.FrameX, leftCenter.FrameY] != 0 &&
            map[rightCenter.FrameX, rightCenter.FrameY] != 0) {
            return GetOneHorizontalCenter();
        }
        //GetOneHorizontalLeft();
        if (map[topCenter.FrameX, topCenter.FrameY] == 0 &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] == 0 &&
            map[rightCenter.FrameX, rightCenter.FrameY] != 0 &&
            map[leftCenter.FrameX, leftCenter.FrameY] == 0
            ) {
            return GetOneHorizontalLeft();
        }
        //GetOneHorizontalRight();
        if (map[topCenter.FrameX, topCenter.FrameY] == 0 &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] == 0 &&
            map[rightCenter.FrameX, rightCenter.FrameY] == 0 &&
            map[leftCenter.FrameX, leftCenter.FrameY] != 0) {
            return GetOneHorizontalRight();
        }
        //GetOneVerticalCenter();
        if (map[leftCenter.FrameX, leftCenter.FrameY] == 0 &&
            map[rightCenter.FrameX, rightCenter.FrameY] == 0 &&
            map[topCenter.FrameX, topCenter.FrameY] != 0 &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] != 0) {
            return GetOneVerticalCenter();
        }
        //GetOneVerticalTop();
        if (map[topCenter.FrameX, topCenter.FrameY] == 0 &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] != 0 &&
            map[rightCenter.FrameX, rightCenter.FrameY] == 0 &&
            map[leftCenter.FrameX, leftCenter.FrameY] == 0 ) {
          //  Debug.LogError("op");
            /*
            string s = "";
            for (int x = 0; x < map.GetLength(0); x++) {

                for (int y = 0; y < map.GetLength(1); y++) {
                    s += map[x, y] + ", ";
                } 
                s += "\n";

            }

            Debug.LogError(s);
            */
            return GetOneVerticalTop();
        }

        //GetOneVerticalBottom();
        if (map[topCenter.FrameX, topCenter.FrameY] != 0 &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] == 0 &&
            map[rightCenter.FrameX, rightCenter.FrameY] == 0 &&
            map[leftCenter.FrameX, leftCenter.FrameY] == 0) {
            return GetOneVerticalBottom();
        }

        //GetTopCenter();
        if (map[topCenter.FrameX, topCenter.FrameY] == 0 &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] != 0 &&
            map[leftCenter.FrameX, leftCenter.FrameY] != 0 &&
            map[rightCenter.FrameX, rightCenter.FrameY] != 0) {
            return GetTopCenter();
        }

        //GetBottomCenter();
        if (map[bottomCenter.FrameX, bottomCenter.FrameY] == 0 &&
            map[topCenter.FrameX, topCenter.FrameY] != 0 &&
            map[leftCenter.FrameX, leftCenter.FrameY] != 0 &&
            map[rightCenter.FrameX, rightCenter.FrameY] != 0) {
            return GetBottomCenter();
        }

        //GetCenterLeft();
        if (map[leftCenter.FrameX, leftCenter.FrameY] == 0 &&
            map[rightCenter.FrameX, rightCenter.FrameY] != 0 &&
           map[topCenter.FrameX, topCenter.FrameY] != 0 &&
           map[bottomCenter.FrameX, bottomCenter.FrameY] != 0) {
            return GetCenterLeft();
        }

        //GetCenterRight();
        if (map[rightCenter.FrameX, rightCenter.FrameY] == 0 &&
            map[leftCenter.FrameX, leftCenter.FrameY] != 0 &&
            map[topCenter.FrameX, topCenter.FrameY] != 0 &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] != 0) {
            return GetCenterRight();
        }

        //GetTopLeft();
        if (map[topCenter.FrameX, topCenter.FrameY] == 0 &&
            map[leftCenter.FrameX, leftCenter.FrameY] == 0 &&
            map[rightCenter.FrameX, rightCenter.FrameY] != 0 &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] != 0) {
                return GetTopLeft();
        }

        //GetTopRight();
        if (map[topCenter.FrameX, topCenter.FrameY] == 0 &&
            map[leftCenter.FrameX, leftCenter.FrameY] != 0 &&
            map[rightCenter.FrameX, rightCenter.FrameY] == 0 &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] != 0) {
                return GetTopRight();
        }

        //GetBottomLeft();
        if (map[leftCenter.FrameX, leftCenter.FrameY] == 0 &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] == 0 &&
            map[topCenter.FrameX, topCenter.FrameY] != 0 &&
            map[rightCenter.FrameX, rightCenter.FrameY] != 0  ) {
                return GetBottomLeft();
        }

        //GetBottomRight();
        if (map[rightCenter.FrameX, rightCenter.FrameY] == 0 &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] == 0 &&
            map[leftCenter.FrameX, leftCenter.FrameY] != 0 &&
            map[topCenter.FrameX, topCenter.FrameY] != 0) {
                return GetBottomRight();
        } 

        //GetInnerCornerTopLefr()
        if (map[topCenter.FrameX, topCenter.FrameY] != info.type &&
            map[topRight.FrameX, topRight.FrameY] != info.type &&
            map[topLeft.FrameX, topLeft.FrameY] != info.type &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] == info.type &&
            map[bottomRight.FrameX, bottomRight.FrameY] != info.type &&
            map[bottomLeft.FrameX, bottomLeft.FrameY] != info.type &&
            map[leftCenter.FrameX, leftCenter.FrameY] == info.type &&
            map[rightCenter.FrameX, rightCenter.FrameY] != info.type) {
            return GetInnerCornerTopLefr();
        }

        //GetInnerCornerTopRight()
        if (map[topCenter.FrameX, topCenter.FrameY] != info.type &&
            map[topRight.FrameX, topRight.FrameY] != info.type &&
            map[topLeft.FrameX, topLeft.FrameY] != info.type &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] == info.type &&
            map[bottomRight.FrameX, bottomRight.FrameY] != info.type &&
            map[bottomLeft.FrameX, bottomLeft.FrameY] != info.type &&
            map[leftCenter.FrameX, leftCenter.FrameY] == info.type &&
            map[rightCenter.FrameX, rightCenter.FrameY] != info.type) {
            return GetInnerCornerTopRight();
        }

        //GetInnerCornerBottomLefr()
        if (map[topCenter.FrameX, topCenter.FrameY] == info.type &&
            map[topRight.FrameX, topRight.FrameY] != info.type &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] != info.type &&
            map[bottomLeft.FrameX, bottomLeft.FrameY] != info.type &&
            map[leftCenter.FrameX, leftCenter.FrameY] != info.type &&
            map[rightCenter.FrameX, rightCenter.FrameY] == info.type) {
            return GetInnerCornerBottomLefr();
        }


        //GetInnerCornerBottomRight()
        if (map[topCenter.FrameX, topCenter.FrameY] == info.type &&
            map[topLeft.FrameX, topLeft.FrameY] != info.type &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] != info.type &&
            map[bottomRight.FrameX, bottomRight.FrameY] != info.type &&
            map[leftCenter.FrameX, leftCenter.FrameY] == info.type &&
            map[rightCenter.FrameX, rightCenter.FrameY] != info.type) {
            return GetInnerCornerBottomRight();
        }

        //GetOne();
        if (map[rightCenter.FrameX, rightCenter.FrameY] == 0 &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] == 0 &&
            map[leftCenter.FrameX, leftCenter.FrameY] == 0 &&
            map[topCenter.FrameX, topCenter.FrameY] == 0) {
                return GetOne();
        }

        //GetCenter
        if (map[rightCenter.FrameX, rightCenter.FrameY] != 0 &&
            map[bottomCenter.FrameX, bottomCenter.FrameY] != 0 &&
            map[leftCenter.FrameX, leftCenter.FrameY] != 0 &&
            map[topCenter.FrameX, topCenter.FrameY] != 0 )  {
            return GetCenter();
        }

        return GetOne();
    }
}

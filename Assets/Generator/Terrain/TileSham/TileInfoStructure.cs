using UnityEngine;
using System.Collections;

public class TileInfoStructure {
    public short type;
    public byte typeBG;
    public bool isDestructible = true;
    public byte hp;
    public short randomTile;
    public byte layer;
    public byte count;  //для жидкостей
    public short id;    //для жидкости номер водаёма
    public byte liquidType;
    public string info;
    public float lightAlpha;
    //public bool lighted;
    public byte lightAlpha2;
    public short ItemId; //id предмета который расположен в клетке
    public short BodyItemId; //id предмета который расположен в клетке (само тело предмета)
    public LightInfoStructure lightInfo;


    

    public static TileInfoStructure Free = new TileInfoStructure(0,0);
    public static TileInfoStructure Free00 = new TileInfoStructure(0,0);
    /*
    public TileInfoStructure() {
        this.id = 0;
        this.type = 0;
        this.randomTile = 0;
        layer = 0;
        liquidType = 0;
        count = 0;
        info = string.Empty;
        lightAlpha = 0;
        lightInfo = null;
        //lighted = false;
        lightAlpha2 = 0;
        ItemId = 0;

        typeBG = 0;
        hp = 0;
        BodyItemId = 0;
    }*/

    public TileInfoStructure(short type, byte randomTile) {
        this.id = 0;
        this.type = type;
        this.randomTile = randomTile;
        layer = 0;
        liquidType = 0;
        count = 0;
        info = string.Empty;
        lightAlpha = 0;
        lightInfo = null;
        //lighted = false;
        lightAlpha2 = 0;
        ItemId = 0;
        
        typeBG = 0;
        hp = 0;
        BodyItemId = 0;
    }

    public void ChackWater(bool isWater) {
        if (isWater) {
            count = 0;
            liquidType = 1;
        } else {
            count = 0;
            liquidType = 0;
            id = -1;
        }
    }

    public bool IsWater() {
        return liquidType == 1 ;
    }

    public bool IsWater2() {
        return count > 9;
    }

    //лесница
    public bool IsStairs() {
        return type == (short)TileTypeEnum.Stairs;
    }


    public bool IsEmpty() {
        return (type == 0 ||
            /*IsTree()*/ type == (short)TileTypeEnum.Stairs || type == (short)TileTypeEnum.Tree || type == (short)TileTypeEnum.TreeBody || type == (short)TileTypeEnum.TreeTopNaked || type == (short)TileTypeEnum.Tree_Top
        || /*IsDecor()*/  type == (short)TileTypeEnum.Jungle || type == (short)TileTypeEnum.Grass1 || type == (short)TileTypeEnum.Grass2
       ||/*IsWater()*/ liquidType == 1);
    }

    public bool IsEmpty2() {
        return (/*typeBG == 0 &&*/ IsEmpty());
    }

    public bool IsTree() {
        return type == (short)TileTypeEnum.Tree || type == (short)TileTypeEnum.TreeBody || type == (short)TileTypeEnum.TreeTopNaked || type == (short)TileTypeEnum.Tree_Top;
    }

    public bool IsTreeTop() {
        return type == (short)TileTypeEnum.TreeTopNaked || type == (short)TileTypeEnum.Tree_Top;
    }

    public bool IsDecor() {
        return type == (short) TileTypeEnum.Jungle
               || type == (short) TileTypeEnum.Grass1
               || type == (short) TileTypeEnum.Grass2;
    }

    public bool CanPourWater() {
        return (IsEmpty() && (count < Settings.WATER_COUNT));
    }

    public override string ToString() {
        return string.Format("id = {0}\ncount = {1},\n info = {2}", id, count, info);
            //string.Format("type = {0} \ncount = {1}\nliquidType = {2}\nid = {3}", type, count, liquidType,id);
    }
}

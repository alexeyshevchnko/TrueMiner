using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpriteData {
    public static Dictionary<string,SpriteData>  Sprites = new Dictionary<string, SpriteData>();
    public static Dictionary<short, SpriteData> SpritesByTypeId = new Dictionary<short, SpriteData>();
    private static ITileShema terrainShema = new TerrainTileShema();
    private static ITileShema grassTerrainTileShema = new GrassTerrainTileShema();
    private static ITileShema terrainBgShema = new TerrainBGTileShema();
    
    private static ITileShema liquidShema = new LiquidTileShema();
    private static ITileShema jungleShema = new JungleShema();
    private static ITileShema grassShema = new GrassShema();
    private static ITileShema lightShema = new LightShema();

    private static ITileShema treeShema = new TerrariaTreeTileShema();
    private static ITileShema treeCapShema = new TerrariaTreeCapTileShema();
    private static ITileShema stairsShema = new StairsShema();
    

    public static SpriteData Tree = new SpriteData("Tiles_5_1", 1, 1, treeShema, 3, (short)TileTypeEnum.Tree);
    public static SpriteData TreeBody = new SpriteData("Tree_tiles", 1, 1, treeShema, 3, (short)TileTypeEnum.TreeBody);

    //string name, int shiftPaddingX, int shiftPaddingY, ITileShema shema,  int squareSizeByShema, short typeId, int countHeight = 1

    public static SpriteData Stairs = new SpriteData("stairs", 5, 5, stairsShema, 3, (short)TileTypeEnum.Stairs);

    public static SpriteData Tree_Tops = new SpriteData("Tree_top_naked", 5, 5, treeCapShema, 5, (short)TileTypeEnum.Tree_Top);
    public static SpriteData Tree_Tops_Naked = new SpriteData("Tree_top_leaves", 5, 5, treeCapShema, 5, (short)TileTypeEnum.TreeTopNaked);

    public static SpriteData Tiles_bg = new SpriteData("Tiles_bg", 1, 1, terrainBgShema, 3, (short)TileTypeEnum.Bg);
    public static SpriteData Tiles_0 = new SpriteData("Tiles_light2", 1, 1, terrainShema, 3, (short)TileTypeEnum.Terrain0);//Terrain0);
    public static SpriteData Tiles_coal = new SpriteData("Tiles_coal", 1, 1, terrainShema, 3, (short)TileTypeEnum.Coal);
    public static SpriteData Tiles_gold = new SpriteData("Tiles_gold", 1, 1, terrainShema, 3, (short)TileTypeEnum.Gold);

    public static SpriteData Tiles_wood = new SpriteData("TILES_wood", 1, 1, terrainShema, 3, (short)TileTypeEnum.Wood);

    public static SpriteData TILES_brick = new SpriteData("TILES_brick", 1, 1, terrainShema, 3, (short)TileTypeEnum.Brick);
    public static SpriteData Tiles_bone = new SpriteData("Tiles_kost", 1, 1, terrainShema, 3, (short)TileTypeEnum.Bone);
    public static SpriteData Tiles_meat = new SpriteData("Tiles_meat", 1, 1, terrainShema, 3, (short)TileTypeEnum.Meat);
    public static SpriteData Tiles_med = new SpriteData("Tiles_med", 1, 1, terrainShema, 3, (short)TileTypeEnum.Med);
    public static SpriteData Tiles_iron = new SpriteData("Tiles_stal", 1, 1, terrainShema, 3, (short)TileTypeEnum.Iron);
    public static SpriteData Tiles_stone = new SpriteData("Tiles_stone", 1, 1, terrainShema, 3, (short)TileTypeEnum.Stone);


    public static SpriteData Liquid_000 = new SpriteData("Liquid_000", 1, 1, liquidShema, 3,-10);

    public static SpriteData Tiles_2 = new SpriteData("Tiles_2", 1, 1, grassTerrainTileShema, 3, (short)TileTypeEnum.TerrainGrass);
    public static SpriteData Tiles_6 = new SpriteData("Tiles_6", 1, 1, terrainShema, 3, (short)TileTypeEnum.Terrain6);

    public static SpriteData Tiles_118 = new SpriteData("Tiles_118", 1, 1, terrainShema, 3, -1);
    public static SpriteData Tiles_146 = new SpriteData("Tiles_146", 1, 1, terrainShema, 3, -2);
    public static SpriteData Tiles_147 = new SpriteData("Tiles_147", 1, 1, terrainShema, 3, -3);
    public static SpriteData Tiles_153 = new SpriteData("Tiles_153", 1, 1, terrainShema, 3, -4);
    public static SpriteData Tiles_154 = new SpriteData("Tiles_154", 1, 1, terrainShema, 3, -5);
    public static SpriteData Tiles_193 = new SpriteData("Tiles_193", 1, 1, terrainShema, 3, -6);

    
    public static SpriteData Liquid_0 = new SpriteData("Liquid_0", 1, 0, liquidShema, 3, -8);//, 16);
    public static SpriteData Liquid_00 = new SpriteData("Liquid_00", 1, 0, liquidShema, 3, -9);//, 16);

    public static SpriteData Tiles_62 = new SpriteData("Tiles_62", 1, 1, jungleShema, 3, (short)TileTypeEnum.Jungle);
    public static SpriteData Tiles_73 = new SpriteData("Tiles_73", 1, 1, grassShema, 3, (short)TileTypeEnum.Grass1);
    public static SpriteData Tiles_74 = new SpriteData("Tiles_74", 1, 1, grassShema, 3, (short)TileTypeEnum.Grass2);
    public static SpriteData Light = new SpriteData("LightTiles", 1, 1, null, 3, (short)TileTypeEnum.Light);

    public SpriteData(string name, int shiftPaddingX, int shiftPaddingY, ITileShema shema, 
        int squareSizeByShema, short typeId, int countHeight = 1) {
        TypeId = typeId;  
        Shema = shema;
        SquareSizeByShema = squareSizeByShema;
        Name = name;
        ShiftPaddingX = shiftPaddingX;
        ShiftPaddingY = shiftPaddingY;
        CountHeight = countHeight;
        Sprites.Add(name, this);
        SpritesByTypeId.Add(typeId, this);
    }
    public int SquareSizeByShema { get; private set; }
    public short TypeId { get; private set; }
    public ITileShema Shema { get; private set; }
    public string Name { get; private set; }
    public int ShiftPaddingX { get; private set; }
    public int ShiftPaddingY { get; private set; }
    public int CountHeight { get; private set; } 

}

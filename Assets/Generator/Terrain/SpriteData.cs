using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpriteData {
    public static Dictionary<string,SpriteData>  Sprites = new Dictionary<string, SpriteData>();
    public static Dictionary<short, SpriteData> SpritesByTypeId = new Dictionary<short, SpriteData>();
    // Прежняя упрощённая схема — для тайлсетов старого пользовательского формата
    // (уголь, золото, дерево, кирпич и т.д.), нарезанных под неё.
    private static ITileShema legacyTerrainShema = new LegacyTerrainTileShema();

    // Система кадрирования Terraria для тайлсетов оригинальной структуры
    // (Tiles_0 — земля, Tiles_1 — камень). SquareSizeByShema = 5: земле необходимо
    // окно 5x5 для взаимной проверки слияния с камнем (см. TerrainTileShema).
    private static readonly TerrainTileShema terrainFramingDirt = new TerrainTileShema(TerrariaTilesetRules.Dirt);
    private static readonly TerrainTileShema terrainFramingStone = new TerrainTileShema(TerrariaTilesetRules.Stone);
    // Tiles_8: тот же решатель Terraria-структуры, но без merge-пар (правила Terrain8).
    private static readonly TerrainTileShema terrainFramingOre8 = new TerrainTileShema(TerrariaTilesetRules.Terrain8);
    private static ITileShema grassTerrainTileShema = new GrassTerrainTileShema();
    private static ITileShema terrainBgShema = new TerrainBGTileShema();
    
    private static ITileShema liquidShema = new LiquidTileShema();
    private static ITileShema jungleShema = new JungleShema();
    private static ITileShema grassShema = new GrassShema();
    private static ITileShema lightShema = new LightShema();

    private static ITileShema treeShema = new TerrariaTreeTileShema();
    private static ITileShema treeCapShema = new TerrariaTreeCapTileShema();
    private static ITileShema stairsShema = new StairsShema();
    

    public static SpriteData Tree = new SpriteData("Tiles_5_1", 1, 1, treeShema, 3, (short)TileTypeEnum.Tree, texturePath: TilesetDir + "Tiles_5_1");
    public static SpriteData TreeBody = new SpriteData("Tree_tiles", 1, 1, treeShema, 3, (short)TileTypeEnum.TreeBody, texturePath: TilesetDir + "Tree_tiles");

    //string name, int shiftPaddingX, int shiftPaddingY, ITileShema shema,  int squareSizeByShema, short typeId, int countHeight = 1

    public static SpriteData Stairs = new SpriteData("stairs", 5, 5, stairsShema, 3, (short)TileTypeEnum.Stairs, texturePath: TilesetDir + "stairs");

    public static SpriteData Tree_Tops = new SpriteData("Tree_top_naked", 5, 5, treeCapShema, 5, (short)TileTypeEnum.Tree_Top, texturePath: TilesetDir + "Tree_top_naked");
    public static SpriteData Tree_Tops_Naked = new SpriteData("Tree_top_leaves", 5, 5, treeCapShema, 5, (short)TileTypeEnum.TreeTopNaked, texturePath: TilesetDir + "Tree_top_leaves");

    public static SpriteData Tiles_bg = new SpriteData("Tiles_bg", 1, 1, terrainBgShema, 3, (short)TileTypeEnum.Bg, texturePath: TilesetDir + "Tiles_bg");
    public static SpriteData Tiles_0 = new SpriteData(/*"Tiles_light2"*/"Tiles_0", 1, 1, terrainFramingDirt, 5, (short)TileTypeEnum.Terrain0, texturePath: TilesetDir + "Tiles_0");//Terrain0);
    public static SpriteData Tiles_coal = new SpriteData("Tiles_coal", 1, 1, legacyTerrainShema, 3, (short)TileTypeEnum.Coal, texturePath: TilesetDir + "Tiles_coal");
    public static SpriteData Tiles_gold = new SpriteData("Tiles_gold", 1, 1, legacyTerrainShema, 3, (short)TileTypeEnum.Gold, texturePath: TilesetDir + "Tiles_gold");

    public static SpriteData Tiles_wood = new SpriteData("TILES_wood", 1, 1, legacyTerrainShema, 3, (short)TileTypeEnum.Wood, texturePath: TilesetDir + "TILES_wood");

    public static SpriteData TILES_brick = new SpriteData("TILES_brick", 1, 1, legacyTerrainShema, 3, (short)TileTypeEnum.Brick, texturePath: TilesetDir + "TILES_brick");
    public static SpriteData Tiles_bone = new SpriteData("Tiles_kost", 1, 1, legacyTerrainShema, 3, (short)TileTypeEnum.Bone, texturePath: TilesetDir + "Tiles_kost");
    public static SpriteData Tiles_meat = new SpriteData("Tiles_meat", 1, 1, legacyTerrainShema, 3, (short)TileTypeEnum.Meat, texturePath: TilesetDir + "Tiles_meat");
    public static SpriteData Tiles_med = new SpriteData("Tiles_med", 1, 1, legacyTerrainShema, 3, (short)TileTypeEnum.Med, texturePath: TilesetDir + "Tiles_med");
    public static SpriteData Tiles_iron = new SpriteData("Tiles_stal", 1, 1, legacyTerrainShema, 3, (short)TileTypeEnum.Iron, texturePath: TilesetDir + "Tiles_stal");
    public static SpriteData Tiles_stone = new SpriteData(/*"Tiles_stone"*/"Tiles_1", 1, 1, terrainFramingStone, 5, (short)TileTypeEnum.Stone, texturePath: TilesetDir + "Tiles_1");

    // Tiles_8 — отдельная текстура Terraria-структуры (288x270, шаг 18 px).
    // SquareSizeByShema = 3: окно 3x3 достаточно — взаимной проверки у Terrain8 нет.
    public static SpriteData Tiles_8 = new SpriteData("Tiles_8", 1, 1, terrainFramingOre8, 3, (short)TileTypeEnum.Terrain8, texturePath: TilesetDir + "Tiles_8");


    public static SpriteData Liquid_000 = new SpriteData("Liquid_000", 1, 1, liquidShema, 3,-10, texturePath: TilesetDir + "Liquid_000");

    public static SpriteData Tiles_2 = new SpriteData("Tiles_2", 1, 1, grassTerrainTileShema, 3, (short)TileTypeEnum.TerrainGrass, texturePath: TilesetDir + "Tiles_2");
    // Tiles_6 и записи с отрицательными TypeId (Tiles_118/146/147/153/154/193, LightTiles)
    // не имеют ни отдельной текстуры, ни спрайта в общем атласе — оставлены без TexturePath.
    public static SpriteData Tiles_6 = new SpriteData("Tiles_6", 1, 1, legacyTerrainShema, 3, (short)TileTypeEnum.Terrain6);

    public static SpriteData Tiles_118 = new SpriteData("Tiles_118", 1, 1, legacyTerrainShema, 3, -1);
    public static SpriteData Tiles_146 = new SpriteData("Tiles_146", 1, 1, legacyTerrainShema, 3, -2);
    public static SpriteData Tiles_147 = new SpriteData("Tiles_147", 1, 1, legacyTerrainShema, 3, -3);
    public static SpriteData Tiles_153 = new SpriteData("Tiles_153", 1, 1, legacyTerrainShema, 3, -4);
    public static SpriteData Tiles_154 = new SpriteData("Tiles_154", 1, 1, legacyTerrainShema, 3, -5);
    public static SpriteData Tiles_193 = new SpriteData("Tiles_193", 1, 1, legacyTerrainShema, 3, -6);

    
    public static SpriteData Liquid_0 = new SpriteData("Liquid_0", 1, 0, liquidShema, 3, -8, texturePath: TilesetDir + "Liquid_0");//, 16);
    public static SpriteData Liquid_00 = new SpriteData("Liquid_00", 1, 0, liquidShema, 3, -9, texturePath: TilesetDir + "Liquid_00");//, 16);

    public static SpriteData Tiles_62 = new SpriteData("Tiles_62", 1, 1, jungleShema, 3, (short)TileTypeEnum.Jungle, texturePath: TilesetDir + "Tiles_62");
    public static SpriteData Tiles_73 = new SpriteData("Tiles_73", 1, 1, grassShema, 3, (short)TileTypeEnum.Grass1, texturePath: TilesetDir + "Tiles_73");
    public static SpriteData Tiles_74 = new SpriteData("Tiles_74", 1, 1, grassShema, 3, (short)TileTypeEnum.Grass2, texturePath: TilesetDir + "Tiles_74");
    public static SpriteData Light = new SpriteData("LightTiles", 1, 1, null, 3, (short)TileTypeEnum.Light);

    // Путь к текстуре тайлсета — относительно Assets/Resources, без расширения
    // (так требует Resources.Load). null / "" — использовать прежний общий атлас слоя
    // (LayerData.TexrureName). Пример: TilesetTextures.TilesetDirectory + "Tiles_0".
    private const string TilesetDir = TilesetTextures.TilesetDirectory;

    public SpriteData(string name, int shiftPaddingX, int shiftPaddingY, ITileShema shema, 
        int squareSizeByShema, short typeId, int countHeight = 1, string texturePath = null) {
        TypeId = typeId;  
        Shema = shema;
        SquareSizeByShema = squareSizeByShema;
        Name = name;
        ShiftPaddingX = shiftPaddingX;
        ShiftPaddingY = shiftPaddingY;
        CountHeight = countHeight;
        TexturePath = texturePath;
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

    /// <summary>
    /// Путь к собственной текстуре тайлсета (относительно Assets/Resources, без расширения).
    /// null / пусто — тайлсет берётся из прежнего общего атласа слоя (LayerData.TexrureName).
    /// </summary>
    public string TexturePath { get; private set; }

}

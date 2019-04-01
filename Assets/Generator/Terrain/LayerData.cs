using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LayerData  {
    public static List<LayerData> Layers = new List<LayerData>();



    public static LayerData TerrainLayer = new LayerData("TerrainLayer", 0, "PixelSnap", "TerrariaTiles", 1, 0, new[] {
        (short) TileTypeEnum.Terrain0,
        (short) TileTypeEnum.Coal, (short) TileTypeEnum.TerrainGrass, (short) TileTypeEnum.Terrain6,
        (short) TileTypeEnum.Tree, (short) TileTypeEnum.Tree_Top,(short) TileTypeEnum.TreeTopNaked,(short) TileTypeEnum.TreeBody,
        (short) TileTypeEnum.Jungle, (short) TileTypeEnum.Grass1, (short) TileTypeEnum.Grass2,
        (short) TileTypeEnum.Empty,
        (short) TileTypeEnum.Gold,
        (short) TileTypeEnum.Wood,

        (short) TileTypeEnum.Brick,
        (short) TileTypeEnum.Bone,
        (short) TileTypeEnum.Meat,
        (short) TileTypeEnum.Med,
        (short) TileTypeEnum.Iron,
        (short) TileTypeEnum.Stone,
        (short) TileTypeEnum.Stairs,

    }, false, false, typeof(InfinityTileMapMesh/*InfinityTileMapSets*/), "Default");
    
    
     /*
    public static LayerData OverlayLight = new LayerData(3,"PixelSnapOverlayLight", "LightTiles", -1, new[] {
         (short)TileTypeEnum.Empty }, false, false, typeof(InfinityTileMapMeshLightOverlay), "layer3");
    */

    public static LayerData WaterLayer = new LayerData("WaterLayer", 1, "PixelSnap", "TerrariaTiles", 4, 0, new[] {
        (short) TileTypeEnum.Water,
        (short) TileTypeEnum.Empty,
       // (short) TileTypeEnum.Stairs,
    }, true, false, typeof(InfinityTileMapMesh/*InfinityTileMapMesh*/ ), "Default");

    /*
    public static LayerData LightLayer = new LayerData(3, "PixelSnap2", "LightTiles", 4, 0, new[] {
        (short) TileTypeEnum.Light,
        (short) TileTypeEnum.Empty 
    }, false, true, typeof(InfinityTileMapMeshLight), "layer2");
    */

    public static LayerData LightLayer = new LayerData("LightLayer", 3, "PixelSnap3", "LightTiles", 4, 0, new[] {
        (short) TileTypeEnum.Light,
        (short) TileTypeEnum.Empty 
    }, false, true, typeof(/*InfinityTileMapPixel*/EmptyTileMesh), "layer2");


    public static LayerData ObstacleLightLayer = new LayerData("ObstacleLightLayer", 2, "PixelSnapLight", "TerrariaTiles", 5, 14, new[] {
        (short) TileTypeEnum.Terrain0, 
        (short) TileTypeEnum.Coal, (short) TileTypeEnum.TerrainGrass,
        (short) TileTypeEnum.Terrain6,
        //(short)TileTypeEnum.Tree, (short)TileTypeEnum.Tree_Top ,
        (short) TileTypeEnum.Jungle, // (short)TileTypeEnum.Grass1, (short)TileTypeEnum.Grass2, 
        (short) TileTypeEnum.Empty,
        (short) TileTypeEnum.Gold,
        (short) TileTypeEnum.Wood,

         (short) TileTypeEnum.Brick,
        (short) TileTypeEnum.Bone,
        (short) TileTypeEnum.Meat,
        (short) TileTypeEnum.Med,
        (short) TileTypeEnum.Iron,
        (short) TileTypeEnum.Stone,
        (short) TileTypeEnum.Stairs,
    }, false, false, typeof(/*InfinityTileMapMeshLight*/EmptyTileMesh), "layer1");


    public static LayerData TerrainBgLayer = new LayerData("TerrainBgLayer", 4, "PixelSnap", "TerrariaTiles", -5, 0, new[] {
        (short) TileTypeEnum.Bg 
    }, false, false, typeof(/*InfinityTileMapMesh*/InfinityTileMapMesh), "Default");
    

    public LayerData(string name, int id, string material, string texrureName, int depth, int sizeAdd , short[] tileIds, bool isShowWater, bool isShowLight, Type renderType, string layerName= "Default") {
        this.IsShowWater = isShowWater;
        this.IsShowLight = isShowLight;
        this.Material = material;
        this.Depth = depth;
        this.Id = id;
       // this.TileIds = tileIds;
        this.TexrureName = texrureName;
        this.RenderType = renderType;
        this.LayerName = layerName;
        this.SizeAdd = sizeAdd;
        this.Name = name;
        Layers.Add(this);

        TileIds = new Dictionary<short, bool>();
        foreach (var idd in tileIds) {
            TileIds.Add(idd, true);
        }
    }

    public bool IsContainsTileType(short id) {
        return TileIds.ContainsKey(id);
    }

    public bool IsContainsTileType(TileTypeEnum id) {
        return TileIds.ContainsKey((short)id);
    }


    public int Id { get; private set; }

    public Dictionary<short,bool> TileIds { get; private set; }
    public string Material { get; private set; }
    public string TexrureName { get; private set; }
    public string LayerName { get; private set; }
    public int Depth { get; private set; }
    public bool IsShowWater { get; private set; }
    public bool IsShowLight { get; private set; }
    public Type RenderType { get; private set; }
    public int SizeAdd { get; private set; }

    public string Name { get; private set; }
}

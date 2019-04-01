using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using trasharia;
using Random = UnityEngine.Random;


public class MapGenerator : IMapGenerator {
    public enum MapType {
        world,
        home
    }

    public MapType mapType = MapType.world;

    public const float rightWorld = 16800;//67200;  //16800;// 134400f; 
    public const float bottomWorld = 4800;//19200; //4800;// 38400f;
    public const int maxTilesX = (int)rightWorld / 16 + 1;
    public const int maxTilesY = (int)bottomWorld / 16 + 1;

    public const int hillSize = 30;//(int)(maxTilesY * 0.2f);

    public const int hillMin = maxTilesY - ((int) (maxTilesY*0.4f));
    public const int hillMax = hillMin + hillSize;
    public const int hillHellMin = (int)((float)hillMin / 2);
    public const int hillHellMax = (int)((float)hillMax / 2);

    private TileInfoStructure[,] map;
    private TileInfoStructure[,] mapHome;

    public static List<Vector2Int> surface = new List<Vector2Int>();
    public static List<Vector2Int> surfaceHome = new List<Vector2Int>();

    public int SurfaceCenterY {
        get {
            if (mapType == MapType.world)
                return maxTilesY/2;
            return 0;
        }
    }

    private int surfaceMaxY;
    public int GetSurfaceMaxY {
        get {
            if (mapType == MapType.world)
                return maxTilesY/2;
             return 0;  
        }
    } 

    public int surfaceMinY = -1;
    public int GetSurfaceMinY {
        get {
            if (mapType == MapType.world)
                return surfaceMinY;
            return 60;
        }
    } 

    public int surfaceMineralsY = 10;
    public int GetSurfaceMineralsY {
        get {
            if (mapType == MapType.world)
                return surfaceMineralsY;
            return 0;
        }
    } 

    public int factorWater = 10000;

    [IoC.Inject]
    public IScreenUpdateManager ScreenUpdateManager { set; protected get; }

    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }

    [IoC.Inject]
    public IGOPoolManager IgoPoolManager { set; protected get; }

    [IoC.Inject]
    public ISwapItemManager SwapItemManager { set; protected get; }

    [IoC.Inject]
    public ITargetManager TargetManager { set; protected get; }

    [IoC.Inject]
    public IRandom Random { set; protected get; }

    public List<Vector2Int> Surface {
        get {
            if (mapType == MapType.world){
                return surface;
            } else {
                return surfaceHome;
            }
        }
    }

    public Vector2Int CenterPos {
        get {
            return Surface[(int)((float)Surface.Count / 2.0f)];
        }
    }

    public Action OnGenerate { get; set; }
    public Action OnChangeMap { get; set; }

    public bool IsGenerated { get; protected set; }

    public int SizeX {
        get {
            return GetMap().GetLength(0);
        }
    }

    public int SizeY {
        get {
            return GetMap().GetLength(1);
        }
    }


    public TileInfoStructure[,] GetMap() {
        if (mapType == MapType.world) {
            return map;
        } else {
            return mapHome;
        }
    }

    public void ChangeMap() {
        if (mapType == MapType.world) {
            mapType = MapType.home;
        } else {
            mapType = MapType.world;
        }

        OnChangeMap.TryCall();
        OnGenerate.TryCall();
    }


    public void GenerateHome() {
        mapType = MapType.home;
        mapHome = new TileInfoStructure[150, 150];
        for (int x = 0; x < mapHome.GetLength(0); x++) {
            for (int y = 0; y < mapHome.GetLength(1); y++) {
                var tile = new TileInfoStructure((short)TileTypeEnum.Terrain0, 0);
                tile.count = 0;

                if (y > hillMax) {
                    tile.type = 0;
                }

                mapHome[x, y] = tile;

       
            }
        }

        int deltaX = 25;

        //обрезаем верхушку
        for (int x = 0; x < mapHome.GetLength(0); x++) {
            for (int y = 0; y < mapHome.GetLength(1); y++) {

                if (y > mapHome.GetLength(1) / 2 && x > deltaX && x < mapHome.GetLength(0) - deltaX) {
                    mapHome[x, y].type = 0;
                }

                if (y == mapHome.GetLength(1) / 2 && x > deltaX && x < mapHome.GetLength(0) - deltaX) {
                    mapHome[x, y].type = (short)TileTypeEnum.TerrainGrass;
                    surfaceHome.Add(new Vector2Int(x, y));
                }
            }
        }

        float randomOffset = Random.Range(10, 120.4f);
        int lastTreeX = -1;
        //деревья 
        for (int i = 20; i < surfaceHome.Count - 20; i++) {
            var val = Mathf.PerlinNoise((surfaceHome[i].x + randomOffset + 100) * 0.05f, (surfaceHome[i].y + randomOffset + 100) * 0.05f);
            if (val > 0.2f && val < 0.5f) {
                if (mapHome[surfaceHome[i].x, surfaceHome[i].y].type != 0) {
                    if (surfaceHome[i].x > lastTreeX + 7) {
                        if (mapHome[surfaceHome[i].x + 1, surfaceHome[i].y].type == 0) {
                            mapHome[surfaceHome[i].x + 1, surfaceHome[i].y].type = mapHome[surfaceHome[i].x, surfaceHome[i].y].type;
                        }

                        if (mapHome[surfaceHome[i].x - 1, surfaceHome[i].y].type == 0) {
                            mapHome[surfaceHome[i].x - 1, surfaceHome[i].y].type = mapHome[surfaceHome[i].x, surfaceHome[i].y].type;
                        }

                        TreeGenerate(surfaceHome[i].x, surfaceHome[i].y + 1, 25);
                        lastTreeX = surfaceHome[i].x;


                    }
                }
            }
        }
        //трава
        for (int i = 15; i < surfaceHome.Count - 15; i++) {
            var topPoint = surfaceHome[i];
            // Map[topPoint.x, topPoint.y].type = 3;
            var val = Mathf.PerlinNoise((topPoint.x + randomOffset + 100) * 0.05f, (topPoint.y + randomOffset + 100) * 0.05f);
            if (val > 0.1f && val < 0.6f) {
                var point1 = new Vector2Int(topPoint.x, topPoint.y + 1);
                var point2 = new Vector2Int(topPoint.x, topPoint.y + 2);
                if (IsValidPoint(point1) && IsValidPoint(point2)) {
                    if (mapHome[topPoint.x, topPoint.y].type == (short)TileTypeEnum.TerrainGrass &&

                        mapHome[topPoint.x, topPoint.y].type != 0 &&
                        mapHome[point1.x, point1.y].type == 0 &&
                        mapHome[point2.x, point2.y].type == 0) {

                        int grassType1 = Random.Range(0, 15);
                        int grassType2 = Random.Range(-2, 2);

                        mapHome[point1.x, point1.y].type = grassType2 > 0
                            ? (short)TileTypeEnum.Grass1
                            : (short)TileTypeEnum.Grass2;
                        mapHome[point1.x, point1.y].randomTile = (byte)grassType1;
                        mapHome[point1.x, point1.y].layer = 3;

                        if (Random.Range(0, 100) > 50) {
                            mapHome[point2.x, point2.y].type = grassType2 > 0
                                ? (short)TileTypeEnum.Grass1
                                : (short)TileTypeEnum.Grass2;
                            mapHome[point2.x, point2.y].randomTile = (byte)grassType1;
                            mapHome[point2.x, point2.y].layer = 3;
                        }
                    }
                }
            }
        }

       
        
        var pos = TileDataProvider.OffsetTileToWorldPos(surfaceHome[surfaceHome.Count -10]);

        var offset =  TileDataProvider.WorldPosToOffsetTile(pos);

        for (int i = -4; i <= -1; i++) {
            mapHome[offset.x + i, offset.y].type = (short)TileTypeEnum.Iron;
            mapHome[offset.x + i, offset.y].isDestructible = false;
        }

        SwapItemManager.AddItem(ItemData.Flag.Id, pos, false);

        pos = TileDataProvider.OffsetTileToWorldPos(surfaceHome[ 10]);

        offset = TileDataProvider.WorldPosToOffsetTile(pos);

        for (int i = -4; i <= -1; i++) {
            mapHome[offset.x + i, offset.y].type = (short)TileTypeEnum.Iron;
            mapHome[offset.x + i, offset.y].isDestructible = false;
        }

        SwapItemManager.AddItem(ItemData.Flag.Id, pos,false);


        for (int x = 0; x < mapHome.GetLength(0); x++) {
            for (int y = 0; y < mapHome.GetLength(1); y++) {
                mapHome[x, y].isDestructible = false;
            }
        }
    }

    public void Generate(ILiquid liquid) {
        GenerateHome();
        mapType = MapType.world;

        float randomOffset = Random.Range(10, 120.4f);
        map = new TileInfoStructure[maxTilesX, maxTilesY];

        //1 генерируем квадрат
        for (int x = 0; x < map.GetLength(0); x++) {
            for (int y = 0; y < map.GetLength(1); y++) {
                var tile = new TileInfoStructure(1, 0);
                tile.count = 0;

                if (y > hillMax) {
                    tile.type = 0;
                }

                map[x, y] = tile;
            }
        }


        int minYsurface = -1;
        
        int lastHillSizeY = -1;
        //2 генерируем горы и траву
        for (int x = 0; x < map.GetLength(0); x++) {
            var hillSizeY01 = Mathf.PerlinNoise((x + randomOffset)*0.03f, (1 + randomOffset)*0.03f);
            var hillSizeY = (int) Mathf.Lerp(hillMin, hillMax, hillSizeY01);

            for (int y = hillMin; y < hillMax + 1; y++) {

                var tile = map[x, y];

                tile.type = (short)(y > hillSizeY ? (short)TileTypeEnum.Empty : (short)TileTypeEnum.Terrain0);

                if (y == hillSizeY) {
                    surface.Add(new Vector2Int(x, y));
                    if (minYsurface == -1 || minYsurface > y)
                        minYsurface = y;
                    if (surfaceMaxY < y)
                        surfaceMaxY = y;
                    if (surfaceMinY == -1 || surfaceMinY > y)
                        surfaceMinY = y;
                }
            }
        }
      
        //трава
        
        for (int i = 0; i < surface.Count; i++) {
            var hillSizeY01 = Mathf.PerlinNoise((i + randomOffset) * 0.05f, (1 + randomOffset) * 0.05f);
            var hillSizeY = (int)Mathf.Lerp(hillMin, hillMax, hillSizeY01);
            if (surface[i].y > hillSizeY)
                continue;

            var point = surface[i];
            if (point.x != 0) {
                var tile = map[point.x, point.y];
                var tileOldTop = map[point.x - 1, point.y + 1];

                tile.type = (short)TileTypeEnum.TerrainGrass;

                if (tileOldTop.type == (short)TileTypeEnum.TerrainGrass) {
                    map[point.x - 1, point.y].type = (short)TileTypeEnum.TerrainGrass;
                }
            }
        }
        
        
        for (int i = surface.Count - 1; i >=0; i--) {
            var hillSizeY01 = Mathf.PerlinNoise((i + randomOffset) * 0.05f, (1 + randomOffset) * 0.05f);
            var hillSizeY = (int)Mathf.Lerp(hillMin, hillMax, hillSizeY01);
            if (surface[i].y > hillSizeY)
                continue;

            var point = surface[i];
            if (point.x != surface.Count - 1) {
                var tile = map[point.x, point.y];
                var tileOldTop = map[point.x + 1, point.y + 1];

                tile.type = (short)TileTypeEnum.TerrainGrass;

                if (tileOldTop.type == (short)TileTypeEnum.TerrainGrass) {
                    map[point.x + 1, point.y].type = (short)TileTypeEnum.TerrainGrass;
                }
            }
        }
        

        //координаты пещер
        List<Vector2Int> cave = new List<Vector2Int>();
        //координаты верхушек пещер
        List<Vector2Int> caveTop = new List<Vector2Int>();
        //нижней части
        List<Vector2Int> caveBottom = new List<Vector2Int>();
        List<Vector2Int> cave3 = new List<Vector2Int>();

        //3 на оставшуюся сушу накладываем текстуры
        for (int x = 0; x < map.GetLength(0); x++) {
            for (int y = 0; y < map.GetLength(1); y++) {
                if (map[x, y].type != 0) {
                    var val = Mathf.PerlinNoise((x + randomOffset)*0.05f, (y + randomOffset)*0.05f);

                    var type = val > 0.3f && val < 0.5f
                        ? (short)TileTypeEnum.Terrain0
                        : val > 0.5f && val < 0.7f
                            ? (short)TileTypeEnum.Terrain0 //TODO
                            : val > 0.7f ? (short)3 : (short)(short)TileTypeEnum.Empty;


                    if (type == 0 && map[x, y].type == (short) TileTypeEnum.TerrainGrass)
                        map[x, y] = new TileInfoStructure(type, (byte) Random.Range(1, 3));

                    if (map[x, y].type != (short) TileTypeEnum.TerrainGrass)
                        map[x, y] = new TileInfoStructure(type, (byte) Random.Range(1, 3));

                    if (type == 0) {
                        cave.Add(new Vector2Int(x, y));
                    }
                    if (type == 3) {
                        cave3.Add(new Vector2Int(x, y));
                    }
                }

                map[x, y].randomTile = (byte)Random.Range(0, 3);
            }
        }

        
       
        /*
        CenterPos = surface[(int)((float)MapGenerator.surface.Count / 2.0f)];
        SizeX = Map.GetLength(0);
        SizeY = Map.GetLength(1);
        OnGenerate.TryCall();
        IsGenerated = true;
        return;
        */
        
        //обходим пещеры формируем caveTop caveBottom
        foreach (var caveXY in cave) {
            var pointTop = new Vector2Int(caveXY.x, caveXY.y + 1);
            var pointBottom = new Vector2Int(caveXY.x, caveXY.y - 1);

            if (IsValidPoint(pointTop) && IsValidPoint(pointBottom)) {
                if (map[pointTop.x, pointTop.y].type != 0 && map[pointBottom.x, pointBottom.y].type == 0) {
                    caveTop.Add(caveXY);
                }
           
                if (map[pointBottom.x, pointBottom.y].type != 0 && map[pointTop.x, pointTop.y].type == 0) {
                    caveBottom.Add(caveXY);
                }
            }
        }


        

      



       

        List<Cave> caves = new List<Cave>();
        int index = 0;

        int maxCaves = (int)((map.GetLength(0)*map.GetLength(1))/(float)factorWater);

        while (caves.Count < maxCaves) {
            
            index++;
            var start = cave[Random.Range(0, cave.Count)];
            bool isbreak = false;
            for (int i = 0; i < caves.Count; i++) {
                if (caves[i].IsIncludePoint(start)) {
                    isbreak = true;
                    break;
                }
            }
            if (isbreak) {
                continue;
            }

            Cave newCave = new Cave(map, start, minYsurface);


            caves.Add(newCave);
        }
       // Debug.LogError(caves.Count);

       // Debug.LogError(Map.GetLength(0) * Map.GetLength(1));

        /*
        //Water
        foreach (var cave1 in caves) {
            foreach (var topPoint in cave1.Body) {
                if (topPoint.y < minYsurface) {
                    var point = new Vector2Int(topPoint.x, topPoint.y);
                    if (IsValidPoint(point)) {
                        //Map[point.x, point.y].type = (short)TileTypeEnum.Gold; 
                          map[point.x, point.y].ChackWater(true);
                            liquid.AddWater(point);
                    }
                }     
            } 
        }
        */
        /*
        //Jungle
        foreach (var topPoint in caveTop) {
            var val = Mathf.PerlinNoise((topPoint.x + randomOffset + 100) * 0.05f, (topPoint.y + randomOffset + 100) * 0.05f);
            if (val > 0.2f && val < 0.5f) {


                int height = Random.Range(1, 15);

                for (int i = 0; i < height; i++) {
                    var point = new Vector2Int(topPoint.x, topPoint.y - i);
                    if (IsValidPoint(point)) {
                        if (map[point.x, point.y].type == 0 && map[point.x, point.y - 1].type == 0) {
                            map[point.x, point.y].type = (short)TileTypeEnum.Jungle;
                            map[point.x, point.y].randomTile = (byte)Random.Range(0, 8);
                            map[point.x, point.y].layer = 3;
                        } else {
                            break;
                        }
                    }
                }
            }
        }
        */
        foreach (var topPoint in cave3) {
            map[topPoint.x, topPoint.y].type = 0;
        }

        int lastTreeX = -1;
        //деревья 
        for (int i = 5; i < surface.Count - 5; i++) {
            var val = Mathf.PerlinNoise((surface[i].x + randomOffset + 100) * 0.05f, (surface[i].y + randomOffset + 100) * 0.05f);
            if (val > 0.2f && val < 0.5f) {
                if (map[surface[i].x, surface[i].y].type != 0) {
                    if (surface[i].x > lastTreeX + 7) {
                        if (map[surface[i].x + 1, surface[i].y].type == 0) {
                            map[surface[i].x + 1, surface[i].y].type = map[surface[i].x, surface[i].y].type;
                        }

                        if (map[surface[i].x - 1, surface[i].y].type == 0) {
                            map[surface[i].x - 1, surface[i].y].type = map[surface[i].x, surface[i].y].type;
                        }

                        TreeGenerate(surface[i].x, surface[i].y + 1, 25);
                        lastTreeX = surface[i].x;


                    }
                }
            }
        }
        
        //Grass РОСТИТЕЛЬНОСТЬ
        foreach (var topPoint in surface) {
           // Map[topPoint.x, topPoint.y].type = 3;
            var val = Mathf.PerlinNoise((topPoint.x + randomOffset + 100) * 0.05f, (topPoint.y + randomOffset + 100) * 0.05f);
            if (val > 0.1f && val < 0.6f) {
                var point1 = new Vector2Int(topPoint.x, topPoint.y + 1);
                var point2 = new Vector2Int(topPoint.x, topPoint.y + 2);
                if (IsValidPoint(point1) && IsValidPoint(point2)) {
                    if (map[topPoint.x, topPoint.y].type == (short)TileTypeEnum.TerrainGrass &&
                        
                        map[topPoint.x, topPoint.y].type != 0 && 
                        map[point1.x, point1.y].type == 0 &&
                        map[point2.x, point2.y].type == 0) {

                        int grassType1 = Random.Range(0, 15);
                        int grassType2 = Random.Range(-2, 2);

                        map[point1.x, point1.y].type = grassType2 > 0
                            ? (short) TileTypeEnum.Grass1
                            : (short) TileTypeEnum.Grass2;
                        map[point1.x, point1.y].randomTile = (byte) grassType1;
                        map[point1.x, point1.y].layer = 3;

                        if (Random.Range(0, 100) > 50) {
                            map[point2.x, point2.y].type = grassType2 > 0
                                ? (short) TileTypeEnum.Grass1
                                : (short) TileTypeEnum.Grass2;
                            map[point2.x, point2.y].randomTile = (byte) grassType1;
                            map[point2.x, point2.y].layer = 3;
                        }
                    }
                }
            }
        }

        //хп
        for (int x = 0; x < map.GetLength(0); x++) {
            for (int y = 0; y < map.GetLength(1); y++) {
                map[x, y].hp = 100;
            }
        }

        //фоновый бэграунд
        for (int x = 0; x < map.GetLength(0); x++) {
            for (int y = 0; y < map.GetLength(1); y++) {
                if (y < surface[x].y+ 1) {
                    map[x, y].typeBG = 1;


                    bool isDifferentTypes = false;
                    short t = map[x, y].type;

                    for (int x1 = -1; x1 <= 1; x1++) {
                        for (int y1 = -1; y1 < 1; y1++) {

                            var deltaX = x + x1;
                            var deltaY = y + y1;

                            if (!isDifferentTypes && deltaX > 0 && deltaX < map.GetLength(0) && deltaY > 0 && deltaY < map.GetLength(1)) {
                                

                                isDifferentTypes = map[deltaX, deltaY].type != t;
                            }
                        }
                    }



                    var max = 2;
                    if (surface[x].y - y  <20) {
                      //  max = 3;
                    }

                    if (surface[x].y - y < 10) {
                        max = 3;
                    }

                    if (surface[x].y - y < 3) {
                        max = 10;
                    }

                    if (surface[x].y - y > 20) {
                        max = 0;
                    }

                    if (y < minYsurface ) {
                        max = 0;
                    }

                    if (!isDifferentTypes && Random.Range(0, 100) < max) {
                        map[x, y].typeBG = 0;
                    }
                }
            }
        }


        //камень
        for (int x = 0; x < map.GetLength(0); x++) {
            for (int y = 0; y < map.GetLength(1); y++) {
                if (surface[x].y - 2 > y)
                if (map[x, y].type == (short)TileTypeEnum.Terrain0) {
                    var val = Mathf.PerlinNoise((x + randomOffset) * 0.12f, (y + randomOffset) * 0.20f);
                    //Debug.LogError(val);
                    if (val > 0.01f && val < 0.25f) {
                        map[x, y].type = (short)TileTypeEnum.Stone;
                    }
                }

            }
        }

        //Уголь
        for (int x = 0; x < map.GetLength(0); x++) {
            for (int y = 0; y < map.GetLength(1); y++) {
                if (surface[x].y - surfaceMineralsY > y)
                if (map[x, y].type == (short)TileTypeEnum.Terrain0) {
                    var val = Mathf.PerlinNoise((x + randomOffset) * 0.02f, (y + randomOffset) * 0.06f);
                    //Debug.LogError(val);
                    if (val > 0.11f && val < 0.2f) {
                        map[x, y].type = (short)TileTypeEnum.Coal;
                    }
                }

            }
        }


        //железо
        for (int x = 0; x < map.GetLength(0); x++) {
            for (int y = 0; y < map.GetLength(1); y++) {
                if (surface[x].y - surfaceMineralsY > y)
                if (map[x, y].type == (short)TileTypeEnum.Terrain0) {
                    var val = Mathf.PerlinNoise((x + randomOffset) * 0.05f, (y + randomOffset) * 0.16f);
                    //Debug.LogError(val);
                    if (val > 0.12f && val < 0.23f) {
                        map[x, y].type = (short)TileTypeEnum.Iron;
                    }
                }

            }
        }

        //золото
        for (int x = 0; x < map.GetLength(0); x++) {
            for (int y = 0; y < map.GetLength(1); y++) {
                if (surface[x].y - surfaceMineralsY > y)
                if (map[x, y].type == (short)TileTypeEnum.Terrain0 ||
                    map[x, y].type == (short)TileTypeEnum.Coal) {
                    var val = Mathf.PerlinNoise((x + randomOffset)*0.03f, (y + randomOffset)*0.05f);
                    //Debug.LogError(val);
                    if (val > 0.01f && val < 0.1f) {
                        map[x, y].type = (short) TileTypeEnum.Gold;
                    }
                }

            }
        }



        //медь
        for (int x = 0; x < map.GetLength(0); x++) {
            for (int y = 0; y < map.GetLength(1); y++) {
                if (surface[x].y - surfaceMineralsY > y)
                if (map[x, y].type == (short)TileTypeEnum.Terrain0 ) {
                    var val = Mathf.PerlinNoise((x + randomOffset) * 0.06f, (y + randomOffset) * 0.10f);
                    //Debug.LogError(val);
                    if (val > 0.01f && val < 0.15f) {
                        map[x, y].type = (short)TileTypeEnum.Med;
                    }
                }

            }
        }


        //hill02

        for (int x = 0; x < map.GetLength(0); x++) {

            var hillSizeY01 = Mathf.PerlinNoise((x + randomOffset) * 0.03f, (1 + randomOffset) * 0.03f);
            var hillSizeY = (int)Mathf.Lerp(hillHellMin, hillHellMax, hillSizeY01);

            for (int y = hillHellMin; y < hillHellMax + 1; y++) {

                var tile = map[x, y];
                if (tile.type == (short) TileTypeEnum.Terrain0) {
                    tile.type = (short)(y > hillSizeY ? (short)TileTypeEnum.Terrain0 : (short)TileTypeEnum.Meat);    
                }
            }


            for (int y = 0; y < hillHellMin; y++) {
                if (map[x, y].type == (short)TileTypeEnum.Terrain0) {
                    if (y <= hillHellMin) {
                        map[x, y].type = (short)TileTypeEnum.Meat;
                    }
                }


                if (map[x, y].type == (short)TileTypeEnum.Coal || map[x, y].type == (short)TileTypeEnum.Stone) {
                    if (y <= hillHellMin) {
                        map[x, y].type = (short)TileTypeEnum.Bone;
                    }
                }
            }

        }
      
        
        //CenterPos = surface[(int)((float)MapGenerator.surface.Count / 2.0f)];
       // SizeX = map.GetLength(0);
       // SizeY = map.GetLength(1);

        /*
        var pos = TileDataProvider.OffsetTileToWorldPos(surface[surface.Count / 2]);
        var offset = TileDataProvider.WorldPosToOffsetTile(pos);

        for (int i = -4; i <= -1; i++) {
            map[offset.x + i, offset.y].type = (short)TileTypeEnum.Iron;
            map[offset.x + i, offset.y].isDestructible = false;
        }


        SwapItemManager.AddItem(ItemData.Flag.Id, pos, false);
        */

        //изза воды тк её нужно непосредственно на карте генерировать
        mapType = MapType.world;


        OnGenerate.TryCall();
        IsGenerated = true;


       
    }

    public void StartApplication() {
        return;
        RemoveVoid();


        for (int i = 1; i < 30; i++) {
            CreateMine(i);
        }
         
    }

    void RemoveVoid() {
        var pos = TileDataProvider.OffsetTileToWorldPos(surface[surface.Count / 2]);
        var offset = TileDataProvider.WorldPosToOffsetTile(pos);
        for (int x = offset.x - 20; x < offset.x + 20; x++) {
            for (int y = offset.y - 8; y >=0; y--) {
                if (map[x, y].IsEmpty()) {
                    map[x, y].type = (short) TileTypeEnum.Coal;
                }
            }
        }

    }

    public void CreateMine(int id) {
        var pos = TileDataProvider.OffsetTileToWorldPos(surface[surface.Count/2]);
        var offset = TileDataProvider.WorldPosToOffsetTile(pos);
        offset = new Vector2Int(offset.x, offset.y - 8 - id*7);
        for (int x = 0; x < 15; x++) {
            for (int y = 0; y < 4; y++) {
                // map[offset.x + x, offset.y + y].type = 0;

                TileDataProvider.ChangeTile(new Vector2Int(offset.x + x, offset.y + y), 0, 0);
            }
        }

        for (int x = 0; x < 2; x++) {
            for (int y = 0; y < 8; y++) {
                TileDataProvider.ChangeTile(new Vector2Int(offset.x - x, offset.y + y), (short)TileTypeEnum.Stairs, 0);
            }
        }

        for (int i = 0; i < 5; i++) {
            var ai = TargetManager.CreateMinerUnit(1, new Vector2Int(offset.x + 7, offset.y + 2));
            var behaviour = (MinerBehaviour) ai.behaviour;

            behaviour.SetMinPos(TileDataProvider.OffsetTileToWorldPos(new Vector2Int(offset.x + 6, offset.y + 2)));
            behaviour.SetMaxPos(TileDataProvider.OffsetTileToWorldPos(new Vector2Int(offset.x + 13, offset.y + 2)));
        }
        SwapItemManager.AddItem(ItemData.Manager.Id, TileDataProvider.OffsetTileToWorldPos(
            new Vector2Int(offset.x+4, offset.y)), false);

        SwapItemManager.AddItem(ItemData.Box.Id, TileDataProvider.OffsetTileToWorldPos(
          new Vector2Int(offset.x + 2, offset.y)), false);
        // units.Add(CreateUnit(id, R));
    }

    short GetXY(int x, int y) {
        x = x + 2;
        return (short)(x * 100 + y);
    }
    
    void TreeGenerate(int rootX, int rootY, int maxHeight) {
        var map = GetMap();
        //1 определимся с высатой
        int height = (int)Random.Range(7, maxHeight);
        short bodyType = (short)TileTypeEnum.TreeBody;

       // Debug.LogError(Map[rootX - 1, rootY]);
        short typeTop = //Random.Range(0, 100) > 50
            map[rootX, rootY - 1].type == (short)TileTypeEnum.TerrainGrass
                      ? (short)TileTypeEnum.TreeTopNaked
                      : (short)TileTypeEnum.Tree_Top;

        for (int i = 0; i < height ; i++) {
            if (i < height - 1) {
                if (i == 0) {
                    //2 не у всех деревьев есть шапка снизу 
                    if (map[rootX - 1, rootY].IsEmpty() && !map[rootX - 1, rootY - 1].IsEmpty()) {
                        map[rootX - 1, rootY].type = bodyType;
                        map[rootX - 1, rootY].layer = 1;
                    }

                    if (map[rootX + 1, rootY].IsEmpty() && !map[rootX + 1, rootY - 1].IsEmpty()) {
                        map[rootX + 1, rootY].type = bodyType;
                        map[rootX + 1, rootY].layer = 1;
                    }

                    map[rootX, rootY].type = bodyType;
                    map[rootX, rootY].randomTile = 1;
                } else {
                    if (i >= 5 && i < height - 6) {
                        //рандомно делаем ветку или с лево или справа
                        int branch = Random.Range(-3, 2);

                        if (branch < 0) {
                            if (map[rootX - 1, rootY + i].type == 0 && map[rootX - 1, rootY + i - 1].type != bodyType) {
                                //Map[rootX - 1, rootY + i].type = bodyType;
                                //Map[rootX - 1, rootY + i].layer = 1;
                            //    Random.Init(Random.Range(0, 100));
                                switch (Random.Range(0, 5)) {
                                        
                                    case 0:
                                    case 1:

                                        if (typeTop == (short) TileTypeEnum.Tree_Top) {
                                            //1
                                            map[rootX - 1, rootY + i].type = bodyType;
                                            map[rootX - 1, rootY + i].layer = 1;
                                            map[rootX - 1, rootY + i].randomTile = GetXY(5, 5);

                                            map[rootX - 2, rootY + i].type = bodyType;
                                            map[rootX - 2, rootY + i].layer = 1;
                                            map[rootX - 2, rootY + i].randomTile = GetXY(4, 5);
                                            break;
                                        } else {
                                            //2
                                            map[rootX - 1, rootY + i].type = bodyType;
                                            map[rootX - 1, rootY + i].layer = 1;
                                            map[rootX - 1, rootY + i].randomTile = GetXY(2, 5);

                                            map[rootX - 2, rootY + i].type = bodyType;
                                            map[rootX - 2, rootY + i].layer = 1;
                                            map[rootX - 2, rootY + i].randomTile = GetXY(1, 5);
                                        }
                                        break;
                                        
                                    case 2:
                                    case 3:
                                        if (typeTop == (short) TileTypeEnum.Tree_Top) {
                                            //1
                                            map[rootX - 1, rootY + i].type = bodyType;
                                            map[rootX - 1, rootY + i].layer = 1;
                                            map[rootX - 1, rootY + i].randomTile = GetXY(5, 7);

                                            map[rootX - 2, rootY + i].type = bodyType;
                                            map[rootX - 2, rootY + i].layer = 1;
                                            map[rootX - 2, rootY + i].randomTile = GetXY(4, 7);

                                            map[rootX - 3, rootY + i].type = bodyType;
                                            map[rootX - 3, rootY + i].layer = 1;
                                            map[rootX - 3, rootY + i].randomTile = GetXY(3, 7);

                                            map[rootX - 2, rootY + 1 + i].type = bodyType;
                                            map[rootX - 2, rootY + 1 + i].layer = 1;
                                            map[rootX - 2, rootY + 1 + i].randomTile = GetXY(4, 6);

                                            map[rootX - 3, rootY + 1 + i].type = bodyType;
                                            map[rootX - 3, rootY + 1 + i].layer = 1;
                                            map[rootX - 3, rootY + 1 + i].randomTile = GetXY(3, 6);

                                            break;
                                        } else {

                                            //2
                                            map[rootX - 1, rootY + i].type = bodyType;
                                            map[rootX - 1, rootY + i].layer = 1;
                                            map[rootX - 1, rootY + i].randomTile = GetXY(2, 7);

                                            map[rootX - 2, rootY + i].type = bodyType;
                                            map[rootX - 2, rootY + i].layer = 1;
                                            map[rootX - 2, rootY + i].randomTile = GetXY(1, 7);

                                            map[rootX - 3, rootY + i].type = bodyType;
                                            map[rootX - 3, rootY + i].layer = 1;
                                            map[rootX - 3, rootY + i].randomTile = GetXY(0, 7);

                                            map[rootX - 2, rootY + 1 + i].type = bodyType;
                                            map[rootX - 2, rootY + 1 + i].layer = 1;
                                            map[rootX - 2, rootY + 1 + i].randomTile = GetXY(1, 6);

                                            map[rootX - 3, rootY + 1 + i].type = bodyType;
                                            map[rootX - 3, rootY + 1 + i].layer = 1;
                                            map[rootX - 3, rootY + 1 + i].randomTile = GetXY(0, 6);
                                        }
                                        break;
                                    case 4:

                                        switch (Random.Range(0, 3)) {
                                            case 0:
                                                 map[rootX - 1, rootY + i].type = bodyType;
                                                 map[rootX - 1, rootY + i].layer = 1;
                                                 map[rootX - 1, rootY + i].randomTile = GetXY(5, 1);
                                                break;
                                            case 1:
                                                map[rootX - 1, rootY + i].type = bodyType;
                                                map[rootX - 1, rootY + i].layer = 1;
                                                map[rootX - 1, rootY + i].randomTile = GetXY(5, 2);
                                                break;
                                            case 2:
                                                map[rootX - 1, rootY + i].type = bodyType;
                                                map[rootX - 1, rootY + i].layer = 1;
                                                map[rootX - 1, rootY + i].randomTile = GetXY(5, 3);
                                                break;
                                        }

                                        break;

                                }
                            }
                        }

                        if (branch > 0) {
                            if (map[rootX + 1, rootY + i].type == 0 && map[rootX + 1, rootY + i - 1].type != bodyType) {
                                //Map[rootX + 1, rootY + i].type = bodyType;
                                //Map[rootX + 1, rootY + i].layer = 1;
                           //     Random.Init(Random.Range(0, 100));
                                switch (Random.Range(0, 5)) {
                                        
                                    case 0:
                                    case 1:
                                        if (typeTop == (short) TileTypeEnum.Tree_Top) {
                                            //1
                                            map[rootX + 1, rootY + i].type = bodyType;
                                            map[rootX + 1, rootY + i].layer = 1;
                                            map[rootX + 1, rootY + i].randomTile = GetXY(7, 4);

                                            map[rootX + 2, rootY + i].type = bodyType;
                                            map[rootX + 2, rootY + i].layer = 1;
                                            map[rootX + 2, rootY + i].randomTile = GetXY(8, 4);

                                            map[rootX + 3, rootY + i].type = bodyType;
                                            map[rootX + 3, rootY + i].layer = 1;
                                            map[rootX + 3, rootY + i].randomTile = GetXY(9, 4);
                                            break;
                                        } else {
                                            //2
                                            map[rootX + 1, rootY + i].type = bodyType;
                                            map[rootX + 1, rootY + i].layer = 1;
                                            map[rootX + 1, rootY + i].randomTile = GetXY(10, 4);

                                            map[rootX + 2, rootY + i].type = bodyType;
                                            map[rootX + 2, rootY + i].layer = 1;
                                            map[rootX + 2, rootY + i].randomTile = GetXY(11, 4);

                                            map[rootX + 3, rootY + i].type = bodyType;
                                            map[rootX + 3, rootY + i].layer = 1;
                                            map[rootX + 3, rootY + i].randomTile = GetXY(12, 4);

                                            break;
                                        }

                                    case 2:
                                    case 3:

                                        if (typeTop == (short) TileTypeEnum.Tree_Top) {
                                            //1
                                            map[rootX + 1, rootY + i].type = bodyType;
                                            map[rootX + 1, rootY + i].layer = 1;
                                            map[rootX + 1, rootY + i].randomTile = GetXY(7, 6);

                                            map[rootX + 2, rootY + i].type = bodyType;
                                            map[rootX + 2, rootY + i].layer = 1;
                                            map[rootX + 2, rootY + i].randomTile = GetXY(8, 6);

                                            map[rootX + 3, rootY + i].type = bodyType;
                                            map[rootX + 3, rootY + i].layer = 1;
                                            map[rootX + 3, rootY + i].randomTile = GetXY(9, 6);

                                            map[rootX + 2, rootY + 1 + i].type = bodyType;
                                            map[rootX + 2, rootY + 1 + i].layer = 1;
                                            map[rootX + 2, rootY + 1 + i].randomTile = GetXY(8, 5);

                                            map[rootX + 3, rootY + 1 + i].type = bodyType;
                                            map[rootX + 3, rootY + 1 + i].layer = 1;
                                            map[rootX + 3, rootY + 1 + i].randomTile = GetXY(9, 5);

                                            break;
                                        } else {
                                            //2
                                            map[rootX + 1, rootY + i].type = bodyType;
                                            map[rootX + 1, rootY + i].layer = 1;
                                            map[rootX + 1, rootY + i].randomTile = GetXY(10, 6);

                                            map[rootX + 2, rootY + i].type = bodyType;
                                            map[rootX + 2, rootY + i].layer = 1;
                                            map[rootX + 2, rootY + i].randomTile = GetXY(11, 6);

                                            map[rootX + 3, rootY + i].type = bodyType;
                                            map[rootX + 3, rootY + i].layer = 1;
                                            map[rootX + 3, rootY + i].randomTile = GetXY(12, 6);

                                            map[rootX + 2, rootY + 1 + i].type = bodyType;
                                            map[rootX + 2, rootY + 1 + i].layer = 1;
                                            map[rootX + 2, rootY + 1 + i].randomTile = GetXY(11, 5);

                                            map[rootX + 3, rootY + 1 + i].type = bodyType;
                                            map[rootX + 3, rootY + 1 + i].layer = 1;
                                            map[rootX + 3, rootY + 1 + i].randomTile = GetXY(12, 5);

                                            break;
                                        }
                                    case 4:
                                        switch (Random.Range(0, 3)) {
                                            case 0:
                                                map[rootX + 1, rootY + i].type = bodyType;
                                                map[rootX + 1, rootY + i].layer = 1;
                                                map[rootX + 1, rootY + i].randomTile = GetXY(7, 0);
                                                break;
                                            case 1:
                                                map[rootX + 1, rootY + i].type = bodyType;
                                                map[rootX + 1, rootY + i].layer = 1;
                                                map[rootX + 1, rootY + i].randomTile = GetXY(7, 1);
                                                break;
                                            case 2:
                                                map[rootX + 1, rootY + i].type = bodyType;
                                                map[rootX + 1, rootY + i].layer = 1;
                                                map[rootX + 1, rootY + i].randomTile = GetXY(7, 2);
                                                break;
                                        }

                                        break;

                                }
                            }
                        }
                    }
                    map[rootX, rootY + i].type = bodyType;
                }

                if (map[rootX, rootY + i].randomTile < 100) {
                    map[rootX, rootY + i].randomTile = (byte) Random.Range(0, 3);
                    map[rootX, rootY + i].layer = 1;
                    if (Random.Range(0, 100) < 5) {
                        map[rootX, rootY + i].randomTile = 5;
                    }
                }

            } else {
                //заканчивается дерево всегда без ветки
                map[rootX, rootY + i].type = (short)TileTypeEnum.TreeBody;
                map[rootX, rootY + i].randomTile = (byte)Random.Range(0, 3);
                map[rootX, rootY + i].layer = 1;

                if (height > 5) {
                  
                    short[,] capMap = new short[,] {
                        {typeTop, typeTop, typeTop, typeTop, typeTop},
                        {typeTop, typeTop, typeTop, typeTop, typeTop},
                        {typeTop, typeTop, typeTop, typeTop, typeTop},
                        {typeTop, typeTop, typeTop, typeTop, typeTop},
                        {typeTop, typeTop, typeTop, typeTop, typeTop}
                    };
                    byte rand = (byte) Random.Range(0, 3);
                    for (int x = 0; x < capMap.GetLength(0); x++) {
                        int deltaX = rootX - 2 + x;
                        for (int y = 0; y < capMap.GetLength(1); y++) {
                            int deltaY = rootY + i - 4 + y;

                            map[deltaX, deltaY].type = typeTop; //capMap[x, y]; 

                            map[deltaX, deltaY].randomTile = rand;
                            map[deltaX, deltaY].layer = 1;
                        }
                    }
                }
            }
           
        }
    }

    public bool IsValidPoint(Vector2Int pos, int maxX = -1, int maxY = -1) {
        var countTileMapX = maxX == -1 ? GetMap().GetLength(0) - 1 : maxX;
        var countTileMapY = maxY == -1 ? GetMap().GetLength(1) - 1 : maxY;

        if (pos.x > 0 && pos.y > 0 && pos.x <= countTileMapX && pos.y <= countTileMapY) {
            return true;
        }
        return false;
    }


    public TileInfoStructure GetTile(Vector2Int pos) {
        if (IsValidPoint(pos)) {
            return GetMap()[pos.x, pos.y];
        }
        return new TileInfoStructure(0, 0);
    }


    public TileInfoStructure GetTileInfo(Vector2Int pos) {
        //if (IsValidPoint(pos)) {
        return GetMap()[pos.x, pos.y];
       // }
       //return new TileInfoStructure();
    }

    public int GetLenghtDown(Vector2Int startPos, short type) {
        int rez = 0;
         Vector2Int pos = new Vector2Int(startPos.x, startPos.y);

        while (IsValidPoint(pos) && GetTileInfo(pos).type == type) {
            rez ++;
            pos = new Vector2Int(pos.x, pos.y - 1);
        }
        return rez;
    }

    public void RemoveLineUp(Vector2Int startPos, List<short> typeList) {
        Vector2Int pos = new Vector2Int(startPos.x, startPos.y);
        Vector2 wirld = TileDataProvider.OffsetTileToWorldPos(pos);

        while (IsValidPoint(pos) && typeList.IndexOf(GetTileInfo(pos).type) != -1) {
            map[pos.x, pos.y].type = 0;
            ScreenUpdateManager.RedrawWorldTile3X(0, pos.x, pos.y);
            ScreenUpdateManager.RedrawWorldTile3X(2, pos.x, pos.y);
            ScreenUpdateManager.RedrawWorldTile3X(4, pos.x, pos.y);

            IgoPoolManager.Spawn<JungleRemoveEffect>(wirld, Quaternion.identity);
            pos = new Vector2Int(pos.x, pos.y + 1);
            wirld = TileDataProvider.OffsetTileToWorldPos(pos);
        }

        
    }

    public void RemoveLineDown(Vector2Int startPos, List<short> typeList) {
        Vector2Int pos = new Vector2Int(startPos.x, startPos.y);
        Vector2 wirld = TileDataProvider.OffsetTileToWorldPos(pos);
        while (IsValidPoint(pos) && typeList.IndexOf(GetTileInfo(pos).type) != -1) {
            map[pos.x, pos.y].type = 0;
            ScreenUpdateManager.RedrawWorldTile3X(0, pos.x, pos.y);
            ScreenUpdateManager.RedrawWorldTile3X(2, pos.x, pos.y);
            ScreenUpdateManager.RedrawWorldTile3X(4, pos.x, pos.y);

            IgoPoolManager.Spawn<JungleRemoveEffect>(wirld, Quaternion.identity);
            pos = new Vector2Int(pos.x, pos.y - 1);
            wirld = TileDataProvider.OffsetTileToWorldPos(pos);
        }
    }

    public void RemoveContactsUp(Vector2Int startPos, List<short> typeList) {
        HashSet<int> r = new HashSet<int>();
        
        TileInfoStructure tileData;

        Vector2Int pos = new Vector2Int(startPos.x, startPos.y);

        while (IsValidPoint(pos) && typeList.IndexOf(GetTileInfo(pos).type) != -1) {
            //1 определим максимальную ширину влево
            int maxWidthL = 0;
            Vector2Int nextPos = new Vector2Int(pos.x - 1, pos.y);
            if (IsValidPoint(nextPos)) {
                tileData = GetTileInfo(nextPos);
                while (typeList.IndexOf(tileData.type) != -1) {
                    maxWidthL++;
                    nextPos = new Vector2Int(nextPos.x - 1, nextPos.y);
                    if (!IsValidPoint(nextPos)) {
                        break;
                    }
                    tileData = GetTileInfo(nextPos);
                }
            }

            //1 определим максимальную ширину вправо
            int maxWidthR = 0;
            nextPos = new Vector2Int(pos.x + 1, pos.y);
            if (IsValidPoint(nextPos)) {
                tileData = GetTileInfo(nextPos);
                while (typeList.IndexOf(tileData.type) != -1 ) {
                    maxWidthR++;
                    nextPos = new Vector2Int(nextPos.x + 1, nextPos.y);
                    if (!IsValidPoint(nextPos)) {
                        break;
                    }
                    tileData = GetTileInfo(nextPos);
                }
            }
            int upX = pos.x;
            //удаляем
            for (int x = pos.x - maxWidthL; x <= pos.x + maxWidthR; x++) {
                map[x, pos.y].type = 0;
                ScreenUpdateManager.RedrawWorldTile3X(0, x, pos.y);
                ScreenUpdateManager.RedrawWorldTile3X(2, x, pos.y);
                ScreenUpdateManager.RedrawWorldTile3X(4, x, pos.y);
                Vector2Int upPos = new Vector2Int(x, pos.y + 1);
                if (IsValidPoint(upPos)) {
                    if (typeList.IndexOf(GetTileInfo(upPos).type ) != -1) {
                        upX = upPos.x;
                    }
                }
            }

            pos = new Vector2Int(upX, pos.y + 1);
        }
    }

    public void RemoveContactsDown(Vector2Int startPos, List<short> typeList) {
        HashSet<int> r = new HashSet<int>();

        TileInfoStructure tileData;

        Vector2Int pos = new Vector2Int(startPos.x, startPos.y);

        while (IsValidPoint(pos) && typeList.IndexOf(GetTileInfo(pos).type) != -1) {
            //1 определим максимальную ширину влево
            int maxWidthL = 0;
            Vector2Int nextPos = new Vector2Int(pos.x - 1, pos.y);
            if (IsValidPoint(nextPos)) {
                tileData = GetTileInfo(nextPos);
                while (typeList.IndexOf(tileData.type) != -1) {
                    maxWidthL++;
                    nextPos = new Vector2Int(nextPos.x - 1, nextPos.y);
                    if (!IsValidPoint(nextPos)) {
                        break;
                    }
                    tileData = GetTileInfo(nextPos);
                }
            }

            //1 определим максимальную ширину вправо
            int maxWidthR = 0;
            nextPos = new Vector2Int(pos.x + 1, pos.y);
            if (IsValidPoint(nextPos)) {
                tileData = GetTileInfo(nextPos);
                while (typeList.IndexOf(tileData.type) != -1) {
                    maxWidthR++;

                    nextPos = new Vector2Int(nextPos.x + 1, nextPos.y);
                    if (!IsValidPoint(nextPos)) {
                        break;
                    }
                    tileData = GetTileInfo(nextPos);
                }
            }
            int upX = pos.x;
            //удаляем
            for (int x = pos.x - maxWidthL; x <= pos.x + maxWidthR; x++) {
                map[x, pos.y].type = 0;
                ScreenUpdateManager.RedrawWorldTile3X(0, x, pos.y);
                ScreenUpdateManager.RedrawWorldTile3X(2, x, pos.y);
                ScreenUpdateManager.RedrawWorldTile3X(4, x, pos.y);
                Vector2Int upPos = new Vector2Int(x, pos.y - 1);
                if (IsValidPoint(upPos)) {
                    if (typeList.IndexOf(GetTileInfo(upPos).type) != -1) {
                        upX = upPos.x;
                    }
                }
            }

            pos = new Vector2Int(upX, pos.y - 1);
        }

        
    }

 
}
 
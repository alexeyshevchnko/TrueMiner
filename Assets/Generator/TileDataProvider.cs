using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IoC;
using System.Threading;
using Managers;
using trasharia;


public class TileDataProvider : ITileDataProvider,IoC.IInitialize {
    [IoC.Inject]
    public IContainer Container { set; protected get; }
    [IoC.Inject]
    public MapGenerator mapGenerator { set; protected get; }
    [IoC.Inject]
    public ILightShema LightShema { set; protected get; }
    //[IoC.Inject]
    //public ILightRendererOld LightRendererOld { set; protected get; }
    [IoC.Inject]
    public IOverlayLightShema OverlayLightShema { set; protected get; }
    [IoC.Inject]
    public ISpriteManager SpriteManager { set; protected get; }
    [IoC.Inject]
    public IItemManager ItemManager { set; protected get; }
    [IoC.Inject]
    public IPhysicsManager PhysicsManager { set; protected get; }
    [IoC.Inject]
    public IScreenUpdateManager ScreenUpdateManager { set; protected get; }
    [IoC.Inject]
    public ILightRenderer LightRenderer { set; protected get; }

    [IoC.Inject]
    public IRandom Random { set; protected get; }

    [IoC.Inject]
    public IGOPoolManager IgoPoolManager { set; protected get; }

    private static TileInfoStructure[,] Map;

    private static List<FrameStructure[,]> frameBuffer;
    private static Dictionary<LayerData, FrameStructure[,]> MapFrameStructure = new Dictionary<LayerData, FrameStructure[,]>();
    private static int mapSizeX;
    private static int mapSizeY;
    public float tileSize;
    public float TileSize { get { return tileSize; } private set { tileSize = value; } }
    private ILiquid liquid;
    private Thread liquidUpdateThread;
    private int sleepTime;
    public void OnInject() {
        Container.Inject(mapGenerator);

        mapGenerator.OnGenerate += () => {
            Map = mapGenerator.GetMap();
            mapSizeX = mapGenerator.SizeX - 1;
            mapSizeY = mapGenerator.SizeY - 1;
          
        };

        frameBuffer = new List<FrameStructure[,]>();
        for (int i = 0; i < LayerData.Layers.Count; i++) {
            frameBuffer.Add(null);
        }
    }


    public void Init(MonoBehaviour monoBehaviour, float tileScale) {
        TileSize = tileScale;
        liquid = new Liquid(mapGenerator);
        Container.Inject(liquid);
        mapGenerator.Generate(liquid);

        //liquidUpdateThread = new Thread(LiquidUpdate);
        sleepTime = (int)(Time.fixedDeltaTime * 1000);
        //liquidUpdateThread.Start();
    }

    public LightInfoStructure GetLightStructure(float x, float y, TileInfoStructure tileStructure) {
        return LightShema.GetLightShemaStructure(null, tileStructure);
    }

    public FrameStructure GetFrameStructurenew(float x, float y, TileInfoStructure tileStructure, LayerData layerData) {
        if (frameBuffer[layerData.Id] == null) {
            
        
     //   if (!MapFrameStructure.ContainsKey(layerData)) {
            frameBuffer[layerData.Id] = new FrameStructure[mapGenerator.SizeX - 1, mapGenerator.SizeY - 1];


            for (int xx = 0; xx < mapSizeX; xx++) {
                for (int yy = 0; yy < mapSizeY; yy++) {
                    frameBuffer[layerData.Id][xx, yy] = GetFrameStructure(xx * tileSize - 1, yy * tileSize - 1, Map[xx, yy], layerData);
                } 
            } 
        }

        int offsetX = Mathf.CeilToInt(x / tileSize);
        int offsetY = Mathf.CeilToInt(y / tileSize);
        return frameBuffer[layerData.Id][offsetX, offsetY];
    }

    public FrameStructure GetFrameStructure(float x, float y, TileInfoStructure tileStructure, LayerData layerData) {
        FrameStructure rez = FrameStructure.Empty();

        
        var pos = new Vector2(x, y);

        if (layerData.IsShowWater) {
            if (tileStructure.IsWater()) {
                var rectWater = GetRect(3, pos, 3, layerData);
                rez = SpriteData.Liquid_0.Shema.GetShema(rectWater, tileStructure);
                return rez;
            }
        }
        
        if (layerData.IsShowLight) {
            var rectLight = GetRectStructure(3, pos, 0);
            rez = LightShema.GetShemaStructure(rectLight, tileStructure);
            return rez;
        }

        if (tileStructure.type == 0 && tileStructure.typeBG == 0) {
            return rez;
        }

        if (layerData == LayerData.TerrainBgLayer) {
            if (tileStructure.typeBG == 0) {
                return rez;
            }


            if (IsAllNeighborsIsNotEmpty(x, y)) {
                return rez;
            }

          

            var rectBg = GetRect(3, pos, 3, layerData);
            rez = SpriteData.Tiles_bg.Shema.GetShema(rectBg, tileStructure);
            rez.TileCollection = SpriteData.Tiles_bg.Name;
            return rez;
        }

        if (tileStructure.type == 0) {
            return rez;
        }
        
        if (!layerData.IsContainsTileType(tileStructure.type)) {
            return rez;
        }

        var data = SpriteData.SpritesByTypeId[tileStructure.type];
        var rect = GetRect(data.SquareSizeByShema, pos, tileStructure.layer, layerData);
        rez = data.Shema.GetShema(rect, tileStructure);
        rez.TileCollection = data.Name;
        
       return rez;
    }

    public TileInfoStructure GetTileStructure(float x, float y) {
        if (x < 0 || y < 0) {
            //нет спрайта
            return TileInfoStructure.Free00;
        } else {
            int offsetX = Mathf.CeilToInt(x / TileSize);
            int offsetY = Mathf.CeilToInt(y / TileSize);


            if (offsetX > mapSizeX || offsetY > mapSizeY) {
                //нет спрайта
                return TileInfoStructure.Free00;
            } else {
                return mapGenerator.GetMap()[offsetX, offsetY];
            }
        }
        return TileInfoStructure.Free00;
    }


    private TileInfoStructure GetTileStructureOffset(int offsetX, int offsetY) {
        if (offsetX > mapSizeX || offsetY > mapSizeY || offsetX < 0 || offsetY < 0) {
            //нет спрайта
            return TileInfoStructure.Free00;
        } else {
            return mapGenerator.GetMap()[offsetX, offsetY];
        }
    }

    bool IsAllNeighborsIsNotEmpty(float x, float y) {
         if (x < 0 || y < 0) {
            //нет спрайта
            return false;
        } else {
            int offsetX = Mathf.CeilToInt(x / TileSize);
            int offsetY = Mathf.CeilToInt(y / TileSize);


             if (offsetX > mapSizeX || offsetY > mapSizeY) {
                 //нет спрайта
                 return false;
             }


             bool isDifferentTypes = false;
             short t = GetTileStructureOffset(offsetX, offsetY).type;

             for (int x1 = -2; x1 <= 2; x1++) {
                 for (int y1 = -2; y1 < 2; y1++) {
                     if (GetTileStructureOffset(offsetX + x1, offsetY + y1).IsEmpty()) {
                         return false;
                     } else {
                         if (!isDifferentTypes && (x1 >= -1 || x1 <= 1) && (y1 >= -1 || y1 <= 1)) {
                             isDifferentTypes = GetTileStructureOffset(offsetX + x1, offsetY + y1).type != t;
                         }
                     }
                 }
             }

             if (isDifferentTypes) {
                 return false;
             }
         }

        return true;
    }



    public void Test() {
        for (int x = 0; x < mapGenerator.SizeX; x++) {
            for (int y = 0; y < mapGenerator.SizeY; y++) {

                if (Random.Range(0, 100) < 70) {

                    if (mapGenerator.GetMap()[x, y].IsWater())
                        continue;

                    if (mapGenerator.GetMap()[x, y].IsEmpty())
                        continue;


                    mapGenerator.GetMap()[x, y].type = 0;
                    mapGenerator.GetMap()[x, y].layer = 0;
                    mapGenerator.GetMap()[x, y].count = 0; 

                  
                }
                /*
                //if (Random.Range(0, 100) < 60) {



                    var p1 = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
                    p1 = new Vector3(p1.x, p1.y, 0);

                    int caneraX = Mathf.CeilToInt(p1.x / TileDataProvider.tileScale);
                    int cameraY = Mathf.CeilToInt(p1.y / TileDataProvider.tileScale);
                    if (x > caneraX - 10 && x < caneraX + 60) {
                        if (y > cameraY - 10 && y < cameraY + 40) {
                            ChangeTile(x * tileScale, y * tileScale, 8, 3);
                        }
                    }

                //}*/
            }
        }
    }

    public Vector2Int WorldPosToOffsetTile(Vector2 wirldPos) {
        float x = wirldPos.x + 1.5f * tileSize;
        float y = wirldPos.y + 1.5f * tileSize;
        int offsetX = Mathf.CeilToInt(x / tileSize);
        int offsetY = Mathf.CeilToInt(y / tileSize);
        return new Vector2Int(offsetX, offsetY);
    }

    public Vector2 OffsetTileToWorldPos(Vector2Int offset) {
        /*float x = wirldPos.x + 1.5f * TileSize;
        float y = wirldPos.y + 1.5f * TileSize;
        int offsetX = Mathf.CeilToInt(x / TileSize);
        int offsetY = Mathf.CeilToInt(y / TileSize);
        return new Vector2Int(offsetX, offsetY);
        */
        var x = offset.x * tileSize - 1.5f * tileSize;
        var y = offset.y * tileSize - 1.5f * tileSize;
        return new Vector2(x, y);
    }

    public void СircleDamageTile(float x, float y, short tileType, byte layer, byte liquidType = 0, int radius = 1, int damag = 0) {
        int offsetX = Mathf.CeilToInt(x / tileSize);
        int offsetY = Mathf.CeilToInt(y / tileSize);
        //var center = new Vector2Int(offsetX, offsetY);

        for (int deltaX = offsetX - radius; deltaX <= offsetX + radius; deltaX++) {
            float invers = Mathf.InverseLerp(0, radius, Mathf.Abs(offsetX - deltaX));
            int delta = Mathf.CeilToInt(Mathf.Lerp(radius, 1, invers) );
            for (int deltaY = offsetY - delta; deltaY <= offsetY + delta; deltaY++) {
                if (damag <= 0) {
                    ChangeTile(new Vector2Int(deltaX, deltaY), tileType, layer, liquidType);
                } else {
                    DamageTile(OffsetTileToWorldPos(new Vector2Int(deltaX, deltaY)), damag);
                }
            }
        }
    }

    public void DamageTile(Vector2Int offset, int hp, bool ignoreTree = true) {
      //  Debug.LogError("DamageTile " + hp);
        int offsetX = offset.x;
        int offsetY = offset.y;
        var map = mapGenerator.GetMap();

        if (!mapGenerator.IsValidPoint(offset)) {
            return;
        }

        if (ignoreTree && map[offsetX, offsetY].IsTree()) {
            return;
        }
        if (!map[offsetX, offsetY].isDestructible) {
           // Debug.LogError("isDestructible");
            return;
        }

        int oldHp = map[offsetX, offsetY].hp;
        bool isEmpty = map[offsetX, offsetY].IsEmpty();
        bool isTree = map[offsetX, offsetY].IsTree();
        if (oldHp - hp > 0) {
            //эффект сломоной стрелы
            IgoPoolManager.Spawn<ArrowEffect>(OffsetTileToWorldPos(offset), Quaternion.identity);
          //  if (map[offsetX, offsetY].isDestructible) {
                map[offsetX, offsetY].hp -= (byte) hp;
          //  }
        } else {
            if (!isEmpty ) {
                var blockLayer = map[offsetX, offsetY].layer;
                var blockType = map[offsetX, offsetY].type;
                var frame = GetFrameStructure(offsetX, offsetY, map[offsetX, offsetY],
                    LayerData.TerrainLayer);
                ChangeTile(offset, 0, 0);
                if (map[offsetX, offsetY].IsEmpty()) {
                    var block = ItemManager.CreateBlock(
                        new Vector2(offsetX*TileSize - 2f*TileSize, offsetY*TileSize - 2f*TileSize),
                        frame.TileCollection);
                    block.blockLayer = blockLayer;
                    block.blockType = blockType;
                    block.blockTileCollection = frame.TileCollection;
                }
            } else {
                if (isTree) {
                    //Debug.LogError(isTree);
                    RemoveTree(offset);
                }
            }
        }
    }

    public void RemoveTree(Vector2Int offset) {
        var map = mapGenerator.GetMap();
        var delta = 0;
        

        //если это шапка дерева
        if (map[offset.x, offset.y].IsTreeTop())
            return;

        //если это корень то перемещаемся к стволу
        if (!map[offset.x, offset.y + 1].IsTree() && map[offset.x, offset.y].randomTile < 100) {
            if (map[offset.x + 1, offset.y + 1].IsTree()) {
                offset.x += 1;
            } else {
                if (map[offset.x - 1, offset.y + 1].IsTree()) {
                    offset.x -= 1;
                }
            }
        }
            

        //если это ветка дерева то перемещаемся к стволу
        if (map[offset.x, offset.y].randomTile >= 100) {

            int deltaX = 0; 
            int deltaY = 0;
            if (map[offset.x, offset.y - 1].IsTree() && map[offset.x, offset.y - 1].randomTile >= 100) {
                deltaY = -1;
            }

            //=>
            while (map[offset.x + deltaX, offset.y + deltaY].IsTree()) {
                deltaX ++;
            }
            deltaX--;
            if (map[offset.x + deltaX, offset.y + deltaY].IsTree() && map[offset.x + deltaX, offset.y + deltaY].randomTile >= 100) {
                //=>
                while (map[offset.x + deltaX, offset.y + deltaY].IsTree()) {
                    deltaX--;
                }
                deltaX++;
            }

            //Debug.LogError("deltaX = " + deltaX + "  deltaY = " + deltaY);
            offset.x = offset.x + deltaX;
            offset.y = offset.y + deltaY;
        }
            

        //ищем начало дерева
        while (map[offset.x, offset.y + delta].IsTree()) {
            delta--;
        }

        delta ++;

        while (map[offset.x, offset.y + delta].IsTree()) {
            var deltaL = -1;
            while (map[offset.x + deltaL, offset.y + delta].IsTree()) {
                SetTile(offset.x + deltaL, offset.y + delta, 0);
                if (map[offset.x + deltaL, offset.y + delta + 1].IsTree()) {
                    SetTile(offset.x + deltaL, offset.y + delta + 1, 0);
                }
                deltaL--;

            }
            var deltaR = 1;
            while (map[offset.x + deltaR, offset.y + delta].IsTree()) {
                SetTile(offset.x + deltaR, offset.y + delta, 0);
                if (map[offset.x + deltaR, offset.y + delta + 1].IsTree()) {
                    SetTile(offset.x + deltaR, offset.y + delta + 1, 0);
                }

                deltaR++;
            }

            SetTile(offset.x, offset.y + delta, 0);
            delta++;
        }
    }

    //покачто используется только для удаления дерева
    void SetTile(int x, int y, short type) {

        var map = mapGenerator.GetMap();
        if (!mapGenerator.GetMap()[x, y].IsTreeTop() && mapGenerator.GetMap()[x, y].IsTree()) {
           
            var block = ItemManager.CreateBlock(new Vector2(x*TileSize - 2f*TileSize, y*TileSize - 2f*TileSize),
                SpriteData.Tiles_wood.Name);
            
            block.blockLayer = 0;
            block.blockType = SpriteData.Tiles_wood.TypeId;
            block.blockTileCollection = SpriteData.Tiles_wood.Name;

            map[x, y].randomTile = (short)Random.Range(0, 3);
        }

        map[x, y].type = type;

        ScreenUpdateManager.RedrawWorldTile3X(0, x, y);
        ScreenUpdateManager.RedrawWorldTile3X(1, x, y);
        ScreenUpdateManager.RedrawWorldTile3X(2, x, y);
        ScreenUpdateManager.RedrawWorldTile3X(4, x, y);
    }

    public void DamageTile(Vector2 worldPos, int hp) {
        
        var x = worldPos.x + 1.5f * TileSize;
        var y = worldPos.y + 1.5f * TileSize;
        int offsetX = Mathf.CeilToInt(x/TileSize);
        int offsetY = Mathf.CeilToInt(y/TileSize);

        DamageTile(new Vector2Int(offsetX, offsetY), hp);

        
    }
    
    public void ChangeTile(float x, float y, short tileType, byte layer, byte liquidType = 0) {
        int offsetX = Mathf.CeilToInt(x/TileSize);
        int offsetY = Mathf.CeilToInt(y/TileSize);
        ChangeTile(new Vector2Int(offsetX, offsetY), tileType, layer, liquidType);
        if (tileType == 0 && layer == 0) {
            var physicItems = PhysicsManager.GetItemsInRange(new Vector2(x, y), 3*TileSize);
            foreach (var item in physicItems) {
                item.SetSleep(false);
            }
        }
    }

    public void ChangeTile(Vector2Int offset, short tileType, byte layer, byte liquidType = 0) {
        int offsetX = offset.x;
        int offsetY = offset.y;

        var currentPoint = new Vector2Int(offsetX, offsetY);
        var upPoint = new Vector2Int(offsetX, offsetY + 1);
        var downPoint = new Vector2Int(offsetX, offsetY - 1);
        var map = mapGenerator.GetMap();

        if (mapGenerator.IsValidPoint(currentPoint)) {

            if (map[currentPoint.x, currentPoint.y].IsTreeTop()) {
                return;
            }

            liquid.ChackNeighborInactive(currentPoint);
            bool isChangeType = false;
            bool isCurrentTree = map[currentPoint.x, currentPoint.y].type == (short)TileTypeEnum.Tree ||
                map[currentPoint.x, currentPoint.y].type == (short)TileTypeEnum.TreeBody;

            if (mapGenerator.IsValidPoint(upPoint)) {
                if (!isCurrentTree && (map[upPoint.x, upPoint.y].type == (short)TileTypeEnum.Tree || map[upPoint.x, upPoint.y].type == (short)TileTypeEnum.TreeBody)) {
                    return;
                }
            }

            //if (offsetX >= 0 && offsetY >= 0 && offsetX <= countTileMapX && offsetY <= countTileMapY) {
            if (!isCurrentTree || liquidType != 0 ) {
 
                //удаляем заросли (если прямиком в джунгли попали)
                if (Map[currentPoint.x, currentPoint.y].IsDecor()) {
                    mapGenerator.RemoveLineDown(currentPoint, new List<short>() { (short)TileTypeEnum.Jungle });
                    mapGenerator.RemoveLineUp(currentPoint, new List<short>() { (short)TileTypeEnum.Grass1, (short)TileTypeEnum.Grass2 });
                }

                if (liquidType == 0 ) {

                    /*if (tileType != 0 && isCurrentTree) {
                        return;
                    }*/

                    if (map[offsetX, offsetY].type == tileType)
                        return;

                    if (map[offsetX, offsetY].type != tileType) {
                        isChangeType = true;
                    }
                    map[offsetX, offsetY].type = tileType;
                    map[offsetX, offsetY].layer = layer;
                    //map[offsetX, offsetY].count = 0;
                     
                    ScreenUpdateManager.RedrawWorldTile3X(1, offsetX, offsetY);

                    ScreenUpdateManager.RedrawWorldTile3X(0, offsetX, offsetY);
                    ScreenUpdateManager.RedrawWorldTile3X(2, offsetX, offsetY);
                    ScreenUpdateManager.RedrawWorldTile3X(4, offsetX, offsetY);
                } else {


                    if (map[offsetX, offsetY].IsWater()) {
                        map[offsetX, offsetY].count = (byte)Settings.WATER_COUNT;
                        if (ScreenUpdateManager != null)
                            ScreenUpdateManager.RedrawWorldTile3X(1, offsetX, offsetY);
                        return;
                    }

                    if (!map[offsetX, offsetY].CanPourWater()) {
                        map[offsetX, offsetY].type = 0;
                        isChangeType = true;
                    }

                    if (liquidType == 1) {
                        map[offsetX, offsetY].ChackWater(true);
                        liquid.AddWater(new Vector2Int(offsetX, offsetY));
                    }

                }
                
                //удаляем заросли (если удалили не заросли а они на нём стояли)
                if (isChangeType) {
                    mapGenerator.RemoveLineDown(downPoint, new List<short>() { (short)TileTypeEnum.Jungle });
                    mapGenerator.RemoveLineUp(upPoint, new List<short>() { (short)TileTypeEnum.Grass1, (short)TileTypeEnum.Grass2 });
                }
                
            } else 

            if (isCurrentTree && tileType == 0) {
                if (map[currentPoint.x, currentPoint.y].type != (short)TileTypeEnum.Tree_Top &&
                    map[currentPoint.x, currentPoint.y].type != (short)TileTypeEnum.TreeTopNaked) {
                    //изза особенности дерева не оставляем пенёк из одного тайла
                    int LenghtTree = mapGenerator.GetLenghtDown(currentPoint, (short) TileTypeEnum.Tree);
                    if (LenghtTree > 0) {
                        mapGenerator.RemoveContactsUp(currentPoint,
                            new List<short>() { (short)TileTypeEnum.Tree, (short)TileTypeEnum.TreeBody, (short)TileTypeEnum.TreeTopNaked, (short)TileTypeEnum.Tree_Top });
                        if (LenghtTree == 2)
                            mapGenerator.RemoveContactsDown(downPoint,
                                new List<short>() { (short)TileTypeEnum.Tree, (short)TileTypeEnum.TreeBody });
                    }
                }
            }

        }
    }


    //Dictionary<int,short[,]>  rectsBuffer = new Dictionary<int, short[,]>();
    private short[,] rectsBuffer3;
    private short[,] rectsBuffer5;
    short[,] GetRect(int size, Vector2 wirldPos, short layer, LayerData layerData) {
        short[,] rez = null;
        if (size == 3) {
            if (rectsBuffer3 == null) {
                rectsBuffer3 = new short[size, size];
            }
            rez = rectsBuffer3;
        }

        if (size == 5) {
            if (rectsBuffer5 == null) {
                rectsBuffer5 = new short[size, size];
            }
            rez = rectsBuffer5;
        }

        int offsetX = Mathf.CeilToInt(wirldPos.x / tileSize);
        int offsetY = Mathf.CeilToInt(wirldPos.y / tileSize);

        int startX = offsetX - Mathf.FloorToInt((float)size / 2);
        int startY = offsetY + Mathf.FloorToInt((float)size / 2);


        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                short value = 0;
                {
                    var mapX = startX + x;
                    var mapY = startY - y;
                    if (mapX < 0 || mapY < 0) {
                        value = 0;
                    }else 
                    if (mapX > mapSizeX || mapY > mapSizeY) {
                        value = 0;
                    } else
                    if (layerData == LayerData.WaterLayer) {
                        value = Map[mapX, mapY].liquidType;
                    } else
                    if (layerData == LayerData.TerrainBgLayer) {
                        value = Map[mapX, mapY].typeBG;
                    } else
                    if (Map[mapX, mapY].layer == layer) {
                        value = Map[mapX, mapY].type;
                    } else
                    if (layer == -1) {
                        value = Map[mapX, mapY].type;
                    }

                }

                rez[x, y] = value; //RangeArray(startX + x, startY - y, layer, layerData);
            } 
        }
      
        return rez;
    }

    short RangeArray(int mapX, int mapY, short layer, LayerData layerData) {


        if (mapX < 0 || mapY < 0)
            return 0;

        if (mapX > mapSizeX || mapY > mapSizeY) {
            //нет спрайта
            return 0;
        }
        if (layerData == LayerData.WaterLayer)
            return Map[mapX, mapY].liquidType;

        if (layerData == LayerData.TerrainBgLayer)
            return Map[mapX, mapY].typeBG; 

        if (Map[mapX, mapY].layer == layer) {
            return Map[mapX, mapY].type;           
        }

        if (layer == -1)
            return Map[mapX, mapY].type;

        return 0;
    }

    private TileInfoStructure[,] rectsStructureBuffer3;
    private TileInfoStructure[,] rectsStructureBuffer5;
   // Dictionary<int, TileInfoStructure[,]> rectsStructureBuffer = new Dictionary<int, TileInfoStructure[,]>();
    TileInfoStructure[,] GetRectStructure(int size, Vector2 wirldPos, short layer) {
        TileInfoStructure[,] rez = null;
       /* if (!rectsBuffer.ContainsKey(size)) {
            rez = new TileInfoStructure[size, size];
            rectsStructureBuffer.Add(size, rez);
        } else {
            rez = rectsStructureBuffer[size];
        }*/
        if (size == 3) {
            if (rectsStructureBuffer3 == null) {
                rectsStructureBuffer3 = new TileInfoStructure[size, size];
            }
            rez = rectsStructureBuffer3;
        }

        if (size == 5) {
            if (rectsStructureBuffer5 == null) {
                rectsStructureBuffer5 = new TileInfoStructure[size, size];
            }
            rez = rectsStructureBuffer5;
        }


        int offsetX = Mathf.CeilToInt(wirldPos.x / tileSize);
        int offsetY = Mathf.CeilToInt(wirldPos.y / tileSize);

        int startX = offsetX - Mathf.FloorToInt((float)size / 2);
        int startY = offsetY + Mathf.FloorToInt((float)size / 2);


        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                rez[x, y] = RangeArrayStructure(startX + x, startY - y, layer);
            }
        }

        return rez;
    }

    TileInfoStructure RangeArrayStructure(int offsetX, int offsetY, short layer) {
        if (offsetX < 0 || offsetY < 0)
            return TileInfoStructure.Free;



        if (offsetX > mapSizeX || offsetY > mapSizeY) {
            //нет спрайта
            return TileInfoStructure.Free;
        }

        return Map[offsetX, offsetY];

    }

    public void LoadNext() {
        mapGenerator.Generate(liquid);
    }

    public void Update() {
        return;
        liquid.Update();
     //   LightRenderer.Update();
        
    }
     


    private void LiquidUpdate() {
        return;
        while (true) {
            Thread.Sleep(sleepTime);
            try {
                liquid.Update();
               // LightRenderer.Update();
            }
            catch (Exception ex) {
                Debug.LogError(ex);
            }
        }
    }

}

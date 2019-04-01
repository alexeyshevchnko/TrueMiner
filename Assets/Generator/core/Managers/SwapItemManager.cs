using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Managers;

public class SwapItemManager : ISwapItemManager {
    //отключил свап предметов тк мешает
    public bool isHideItems = false;

    [IoC.Inject]
    public IGOPoolManager Pool { set; protected get; }
    [IoC.Inject]
    public MapGenerator mapGenerator { set; protected get; }
    [IoC.Inject]
    public ITileDataProvider tileDataProvider { set; protected get; }
    [IoC.Inject]
    public IScreenUpdateManager ScreenUpdateManager { set; protected get; }

    [IoC.Inject]
    public IItemManager ItemManager { set; protected get; }
    

    //x - y - GameObject
    Dictionary<int,Dictionary<int,GameObject>> activeItems = new Dictionary<int, Dictionary<int, GameObject>>();

    private TileInfoStructure[,] _map;
    private TileInfoStructure[,] Map {
        get {

            return mapGenerator.GetMap();
          //  if (_map == null)
                _map = mapGenerator.GetMap();
            return _map;
        }
    }

    private int countTileMapX;
    private int countTileMapY;
    public bool IsValidPoint(int x, int y) {
       // if (countTileMapY == 0) {
            countTileMapX = mapGenerator.SizeX - 1;
            countTileMapY = mapGenerator.SizeY - 1;
     //   }

        if (x > 0 && y > 0 && x <= countTileMapX && y <= countTileMapY) {
            return true;
        }
        return false;
    }

    private int sizeX, sizeY;

    private Vector2Int minPos;
    private Vector2Int maxPos;

    private Vector2Int oldMinPos;
    private Vector2Int oldMaxPos;

    public void SetStartPos(Vector2Int val) {
       // return;
        oldMinPos = minPos;
        oldMaxPos = maxPos;

        minPos = val;
        maxPos = new Vector2Int(minPos.x + sizeX, minPos.y + sizeY);
     //   Debug.LogErrorFormat("minPos = {0}, maxPos = {1}", minPos.x, maxPos.x);
       // Debug.LogError("SetStartPos");
    }

    public void SetSize(int x, int y) {
        sizeX = x;
        sizeY = y;
    }

    public bool AddItem(short id, Vector2 worldPos, bool useNeighbors = true) {
        var offset = tileDataProvider.WorldPosToOffsetTile(worldPos);
        var rez = AddItem(id, offset.x, offset.y, useNeighbors);
       //ebug.LogError(rez);
        ScreenUpdateManager.RedrawFullScreenLight();
        return rez;
    }


    public bool AddItem(short id, int offseX, int offsetY, bool useNeighbors = true) {

        if (IsValidPoint(offseX, offsetY)) {
            // Debug.LogError(2);
            //проверяем нет ли посаседству другова предмета 
            var itemData = ItemData.GetById(id);
            if (useNeighbors) {
                for (int x = offseX - itemData.SizeXmm; x < offseX + itemData.SizeXpp; x++) {
                    for (int y = offsetY - itemData.SizeYmm; y < offsetY + itemData.SizeYpp; y++) {
                        if (IsValidPoint(x, y)) {
                            if (Map[x, y].BodyItemId != 0 || !Map[x, y].IsEmpty()
                                || (!itemData.InWater && Map[x, y].IsWater())) {

                                return false;
                            }
                        }
                    }
                }
            }

            if (offseX >= minPos.x && offseX <= maxPos.x &&
                offsetY >= minPos.y && offsetY <= maxPos.y) {
                var pos = tileDataProvider.OffsetTileToWorldPos(new Vector2Int(offseX, offsetY));
                var size12 = tileDataProvider.TileSize ;
                pos = new Vector2(pos.x - size12, pos.y - size12);
                var go = Pool.SpawnItem(id, pos);
                AddIndex(offseX, offsetY, go);
                
            }

            Map[offseX, offsetY].ItemId = id;

                      
            for (int x = offseX - itemData.SizeXmm; x < offseX + itemData.SizeXpp; x++) {
                for (int y = offsetY - itemData.SizeYmm; y < offsetY + itemData.SizeYpp; y++) {
                    if (IsValidPoint(x, y)) {
                        Map[x, y].BodyItemId = id;

                    }
                }   
            }
            return true;
        }
      // Debug.LogError(4);
        return false;
    }

    //вернёт оффсет с ItemId != 0
    private Vector2Int GetItemCenter(Vector2 worldPos) {
        var offset = tileDataProvider.WorldPosToOffsetTile(worldPos);


        if (IsValidPoint(offset.x, offset.y)) {
            if (Map[offset.x, offset.y].ItemId != 0) {
                return offset;
            }
            if(Map[offset.x, offset.y].BodyItemId == 0) {
                return new Vector2Int(-1, -1);
            }
            var itemData = ItemData.GetById(Map[offset.x, offset.y].BodyItemId);

            for (int x = offset.x - itemData.SizeXmm; x <= offset.x + itemData.SizeXpp; x++) {
                for (int y = offset.y - itemData.SizeYmm; y <= offset.y + itemData.SizeYpp; y++) {
                    if (Map[x, y].ItemId != 0) {
                        return new Vector2Int(x, y);
                    }
                }

            }
            return new Vector2Int(-1, -1);
        }

        return new Vector2Int(-1, -1);
    }

    //вернёт id предмета который удалили или 0
    public int RemoveItem(Vector2 worldPos) {
       // Debug.LogError("RemoveItem");
        var offset = GetItemCenter(worldPos);

        if (offset.x < 0 && offset.y < 0) {
            return 0;
        }

        var rez = RemoveItem(offset.x, offset.y);

        if (rez !=0 ) {
            ItemManager.CreateItem<TorchPoolItem>(worldPos);
            ScreenUpdateManager.RedrawFullScreenLight();
        }

        return rez;
    }

    public int RemoveItem(Vector2Int offset) {
       // Debug.LogError("RemoveItem");
        var rez = RemoveItem(offset.x, offset.y);

        if (rez != 0) {
            var worldPos = tileDataProvider.OffsetTileToWorldPos(offset);
            ItemManager.CreateItem<TorchPoolItem>(worldPos);

            ScreenUpdateManager.RedrawFullScreenLight();
        }

        return rez;
    }

    //вернёт id предмета который удалили или 0
    private int RemoveItem(int offseX, int offsetY) {
        if (IsValidPoint(offseX, offsetY)) {
            var id = Map[offseX, offsetY].ItemId;
            if (id == 0 || id == ItemData.Flag.Id)
                return 0;
            
            var itemData = ItemData.GetById(id);

            for (int x = offseX - itemData.SizeXmm; x < offseX + itemData.SizeXpp; x++) {
                for (int y = offsetY - itemData.SizeYmm; y < offsetY + itemData.SizeYpp; y++) {
                    if (IsValidPoint(x, y)) {
                        Map[x, y].BodyItemId = 0;
                        Map[x, y].ItemId = 0;
                    }
                }
            }

            Pool.Unspawn(activeItems[offseX][offsetY]);
            RemoveIndex(offseX, offsetY);
            return id;
        }

        return 0;
    }

    public void Swap(int deltaX, int deltaY) {

        // return;
        //1е определяыем облось которая появится на экране
        int minY = 0, maxY = 0, minX = 0, maxX = 0;
        if (deltaX != 0) {
            minY = minPos.y;
            maxY = maxPos.y;
            minX = deltaX > 0 ? oldMaxPos.x : minPos.x;
            maxX = deltaX > 0 ? maxPos.x : oldMinPos.x;
            if (minX > maxX) {
                var tmp = minX;
                minX = maxX;
                maxX = tmp;
            }
          //  Debug.LogErrorFormat("minX = {0}, maxX = {1} minY = {2}, maxY = {3}", minX, maxX, minY, maxY);
            ShowItems(minX, maxX, minY, maxY);
        }

        if (deltaY != 0) {
            minX = minPos.x;
            maxX = maxPos.x;
            minY = deltaY > 0 ? oldMaxPos.y : minPos.y;
            maxY = deltaY > 0 ? maxPos.y : oldMinPos.y;
            if (minY > maxY) {
                var tmp = minY;
                minY = maxY;
                maxY = tmp;
            }

          //  Debug.LogErrorFormat("minX = {0}, maxX = {1} minY = {2}, maxY = {3}", minX, maxX, minY, maxY);
            ShowItems(minX, maxX, minY, maxY);

        }


        //2е определяыем облось которая исчезает
        deltaX *= -1;
        deltaY *= -1;
        if (deltaX != 0) {
            minY = minPos.y;
            maxY = maxPos.y;
            minX = deltaX > 0 ? oldMaxPos.x : minPos.x;
            maxX = deltaX > 0 ? maxPos.x : oldMinPos.x;
            if (minX > maxX) {
                var tmp = minX;
                minX = maxX;
                maxX = tmp;
            }
            HideItems(minX, maxX, minY, maxY);
        }

        if (deltaY != 0) {
            minX = minPos.x;
            maxX = maxPos.x;
            minY = deltaY > 0 ? oldMaxPos.y : minPos.y;
            maxY = deltaY > 0 ? maxPos.y : oldMinPos.y;
            if (minY > maxY) {
                var tmp = minY;
                minY = maxY;
                maxY = tmp;
            }
            HideItems(minX, maxX, minY, maxY);

        }

    }

    private void ShowItems(int minX, int maxX, int minY, int maxY) {
        //Debug.LogErrorFormat("ShowItems - minX = {0}, maxX = {1}", minX, maxX);
        for (int x = minX; x < maxX; x++) {
            for (int y = minY; y < maxY; y++) {

                if (IsValidPoint(x, y) && Map[x, y].ItemId != 0) {
                    var pos = tileDataProvider.OffsetTileToWorldPos(new Vector2Int(x, y));
                    var size12 = tileDataProvider.TileSize ;
                    pos = new Vector2(pos.x - size12, pos.y - size12);
                    if (!ThereIs(x, y)) {
                        var go = Pool.SpawnItem(Map[x, y].ItemId, pos);
                     //   Debug.LogErrorFormat("ShowItems - minX = {0}, maxX = {1}", minX, maxX);
                        AddIndex(x, y, go);    
                    }
                }
            }
        }
    }

    private void HideItems(int minX, int maxX, int minY, int maxY) {
        if (isHideItems)
            return;
       // Debug.LogErrorFormat("HideItems - minX = {0}, maxX = {1}", minX, maxX);
        for (int x = minX; x < maxX; x++) {
            if (activeItems.ContainsKey(x)) {
                for (int y = minY; y < maxY; y++) {
                    if (!activeItems.ContainsKey(x)) {
                        break;
                    }

                    if (activeItems[x].ContainsKey(y)) {
                        Pool.Unspawn(activeItems[x][y]);
                     //   Debug.LogErrorFormat("HideItems - minX = {0}, maxX = {1}", minX, maxX);
                        RemoveIndex(x, y);

                    }
                }
            }
        }
    }

    void AddIndex(int x, int y, GameObject go) {
        Debug.LogError(go);
        if (!activeItems.ContainsKey(x)) {
            activeItems[x] = new Dictionary<int, GameObject>();
        }

        activeItems[x][y] = go;
    }

    private void RemoveIndex(int x, int y) {
        activeItems[x].Remove(y);

        if (activeItems[x].Count == 0) {
            activeItems.Remove(x);
        }
    }


    bool ThereIs(int x, int y) {
        if (activeItems.ContainsKey(x) && activeItems[x].ContainsKey(y)) {
            return true;
        }

        return false;
    }

}


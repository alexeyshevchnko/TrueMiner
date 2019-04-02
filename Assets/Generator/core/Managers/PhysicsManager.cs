using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using trasharia;

public class PhysicsManager : IPhysicsManager {
    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }

    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }

    public Dictionary<int, Dictionary<int, List<IPhysicItem>>> physicSectors =
        new Dictionary<int, Dictionary<int, List<IPhysicItem>>>();


    private float TileSize {
        get { return TileDataProvider.TileSize; }
    }

    private TileInfoStructure[,] Map {
        get { return MapGenerator.GetMap(); }
    }

    //вернёт сектора в котрых находится квадрат если его вершины в соответствующих секторах
    public Vector2Int[] GetSectors(Vector2Int topSector, Vector2Int bottomSector) {
        Vector2Int[] sectros = null;

        //1й случай если мы в одном секторе
        if (topSector.x == bottomSector.x && topSector.y == bottomSector.y) {
            //регистрируемся в topSector
            sectros = new[] { topSector };
        }

        //2й случай если мы в 2х секторах
        if ((topSector.x == bottomSector.x && topSector.y != bottomSector.y) ||
            (topSector.x != bottomSector.x && topSector.y == bottomSector.y)) {
            //регестрируемся в topSector и bottomSector
            sectros = new[] { topSector, bottomSector };
        }

        //3й случай если мы в 4х секторах
        if (topSector.x != bottomSector.x && topSector.y != bottomSector.y) {
            //регестрируемся в topSector и bottomSector + (topSector.x,bottomSector.y) (bottomSector.x,topSector.y)
            sectros = new[] {
                topSector, bottomSector, new Vector2Int(topSector.x, bottomSector.y),
                new Vector2Int(bottomSector.x, topSector.y)
            };
        }

        return sectros;
    }

    //перерегестрирует обьект в новом секторе
    public void ChackRect(IPhysicItem item) {
        var top = WorldPosToOffsetTile(item.GetTopLeftPos());
        var bottom = WorldPosToOffsetTile(item.GetBottomRightPos());

        var topSector = GetSector(top);
        var bottomSector = GetSector(bottom);

        Vector2Int[] sectros = GetSectors(topSector, bottomSector);

        //1е проверяем сменились ли сектора если да то удаляемся и регистрируемся
        if (item.PhysicsSectors != null && !CompareSectors(item.PhysicsSectors, sectros)) {
            //if (item.View.name != "Player(Clone)")
            //    Debug.LogError(item.View.name);

            RemovePhysicItem(item);
            item.PhysicsSectors = sectros;
            AddPhysicItem(item);
        } else {
            if (item.PhysicsSectors == null) {
              //  if (item.View.name != "Player(Clone)")
               //     Debug.LogError(item.View.name);

                item.PhysicsSectors = sectros;
                AddPhysicItem(item);
            } 
        }
    }

    //удаляет обьект из системы
    public void RemovePhysicItem(IPhysicItem item) {
        if (item.PhysicsSectors != null) {
            for (int i = 0; i < item.PhysicsSectors.Length; i++) {
                int x = item.PhysicsSectors[i].x;
                int y = item.PhysicsSectors[i].y;
                physicSectors[x][y].Remove(item);
            }
        }

        item.PhysicsSectors = null;
    }

    //добавляет
    public void AddPhysicItem(IPhysicItem item) {
       // string print = "";
        for (int i = 0; i < item.PhysicsSectors.Length; i++) {
            int x = item.PhysicsSectors[i].x;
            int y = item.PhysicsSectors[i].y;
           // int count = 0;

            if (physicSectors.ContainsKey(x) && physicSectors[x].ContainsKey(y)) {
                physicSectors[x][y].Add(item);
                
            }

            if (physicSectors.ContainsKey(x) && !physicSectors[x].ContainsKey(y)) {
                physicSectors[x].Add(y, new List<IPhysicItem>() {item});
            }

            if (!physicSectors.ContainsKey(x)) {
                physicSectors.Add(x,new Dictionary<int, List<IPhysicItem>>());
                physicSectors[x].Add(y, new List<IPhysicItem>() { item });
            }

          //  count = physicSectors[x][y].Count;


           //print += string.Format("x = {0}, y = {1} count = {2}; ", x, y, count);
        }

        //Debug.LogError(print);
    }

    //проверка изменились ли сектора
    bool CompareSectors(Vector2Int[] a, Vector2Int[] b) {
        if (a.Length != b.Length)
            return false;

        for (int i = 0; i < a.Length; i++) {
            int x = a[i].x;
            int y = a[i].y;
            bool finded = false;
            for (int j = 0; j < b.Length; j++) {
                if (b[j].x == x && b[j].y == y) {
                    finded = true;
                    break;
                }
            }

            if (!finded) {
                return false;
            }
        }

        return true;
    }

    //проверка на колизии обьекта вызовит OnCollision у обьектов которые пересеклись
    public void ChackCollision(IPhysicItem item) {
        //var aX1 = item.GetTopLeftPos().x
        var sectors = item.PhysicsSectors;
        for (int i = 0; i < sectors.Length; i++) {
            var sector = sectors[i];
            var objects = physicSectors[sector.x][sector.y];
            for (int j = 0; j < objects.Count; j++) {
                var otherItem = objects[j];

                if (otherItem != item && CollisionPhysicItems(item, otherItem)) {
                    item.OnCollision.TryCall(otherItem);
                    otherItem.OnCollision.TryCall(item);
                }
            }
        }
    }

    //проверяет пересекаются ли 2 квадрата
    public bool CollisionPhysicItems(IPhysicItem a, IPhysicItem b) {
        return a.GetRect().Overlaps(b.GetRect());
    }

    //вернёт сектор по тайлу
    public Vector2Int GetSector(Vector2Int tile) {
        return new Vector2Int(
            Mathf.CeilToInt(((float)tile.x) / ((float)Settings.PHYSICS_SECTOR_SIZE)),
            Mathf.CeilToInt(((float)tile.y) / ((float)Settings.PHYSICS_SECTOR_SIZE)));
    }

    //вернёт тайл по позиции
    public Vector2Int WorldPosToOffsetTile(Vector2 wirldPos) {
        float x = wirldPos.x + 1.5f*TileSize;
        float y = wirldPos.y + 1.5f*TileSize;
        int offsetX = Mathf.CeilToInt(x/TileSize);
        int offsetY = Mathf.CeilToInt(y/TileSize);
        return new Vector2Int(offsetX, offsetY);
    }

    //вернёт все обьекты которые входят в круг с центром center и радиусом radius
    public List<IPhysicItem> GetItemsInRange(Vector2 center, float radius) {
        List<IPhysicItem> rez = new List<IPhysicItem>();

        //1 определим сектора в которых находится круг
        Vector2 topLeftPos = new Vector2(center.x - radius, center.y + radius);
        Vector2 bottomRightPos = new Vector2(center.x + radius, center.y - radius);
        var top = WorldPosToOffsetTile(topLeftPos);
        var bottom = WorldPosToOffsetTile(bottomRightPos );

        var topSector = GetSector(top);
        var bottomSector = GetSector(bottom);

        Vector2Int[] sectros = GetSectors(topSector, bottomSector);

        //2 проверим всех обьектах в секторах на вхождение в круг
        for (int i = 0; i < sectros.Length; i++) {
            var sector = sectros[i];
            if (!physicSectors.ContainsKey(sector.x) || !physicSectors[sector.x].ContainsKey(sector.y)) {
                continue;
            }

            var objects = physicSectors[sector.x][sector.y];
           
            for (int j = 0; j < objects.Count; j++) {
                var item = objects[j];
                Vector2 itemCenterPos = item.Position;
                //1е если радиус меньше чем растояние между центрами то обьект входит в рейндж
                if (Vector2.Distance(center, itemCenterPos) < radius) {
                    rez.Add(item);
                    continue;
                }

                //2е если хоть одна вершина квадрата входит в круг то обьект входит в рейндж
                Vector2 itemTopLeftPos = item.GetTopLeftPos();
                if (Vector2.Distance(center, itemTopLeftPos) < radius) {
                    rez.Add(item);
                    continue; 
                }
                Vector2 itemBottomRightPos = item.GetBottomRightPos();
                if (Vector2.Distance(center, itemBottomRightPos) < radius) {
                    rez.Add(item);
                    continue;
                }
                Vector2 itemTopRightPos = item.GetTopRightPos();
                if (Vector2.Distance(center, itemTopRightPos) < radius) {
                    rez.Add(item);
                    continue;
                }
                Vector2 itemBottomLeftPos = item.GetBottomLeftPos();
                if (Vector2.Distance(center, itemBottomLeftPos) < radius) {
                    rez.Add(item);
                    continue;
                }
            }
        }

        return rez;

    }

    public List<IPhysicItem> GetItemsOfOffset(Vector2Int offset) {
        List<IPhysicItem> rez = new List<IPhysicItem>();

        var sector = GetSector(offset);

        if (!physicSectors.ContainsKey(sector.x) || !physicSectors[sector.x].ContainsKey(sector.y)) {
            return rez;
        }

        var objects = physicSectors[sector.x][sector.y];

        for (int j = 0; j < objects.Count; j++) {
            var item = objects[j];
            //пришлось подгонять мега кастыль
            var itemTopLeftPos = new Vector2(
                item.Position.x - (item.Size.x / 2f + item.Size.x * 0.2f),
                item.Position.y + (item.Size.y / 2f + item.Size.y * 0.1f));

            var itemBottomRightPos = new Vector2(
                item.Position.x + (item.Size.x / 2f + item.Size.x * 0.2f),
                item.Position.y - (item.Size.y / 2f + item.Size.y * 0.1f));

            //Vector2 itemTopLeftPos = item.GetTopLeftPos();
            //Vector2 itemBottomRightPos = item.GetBottomRightPos();
            var topLeftOffset = WorldPosToOffsetTile2(itemTopLeftPos);
            var bottomRightOffset = WorldPosToOffsetTile2(itemBottomRightPos);

            for (int x = topLeftOffset.x; x < bottomRightOffset.x; x++) {
                for (int y = bottomRightOffset.y; y < topLeftOffset.y; y++) {
                    if (x == offset.x && y == offset.y) {
                        rez.Add(item);
                        continue;
                    }
                }
            }

           
            
        }

        return rez;
    }

    public Vector2Int WorldPosToOffsetTile2(Vector2 wirldPos) {
        float x = wirldPos.x + 2f * TileSize;
        float y = wirldPos.y + 2f * TileSize;
        int offsetX = Mathf.CeilToInt(x / TileSize);
        int offsetY = Mathf.CeilToInt(y / TileSize);
        return new Vector2Int(offsetX, offsetY);
    }
}

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Collision : ICollision,IoC.IInitialize {
    private float tileSize;
    BufferList Buffer = new BufferList();

    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }

    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }

    private TileInfoStructure[,] _map;
    private TileInfoStructure[,] Map {
        get {
           // if (_map == null)
                _map = MapGenerator.GetMap();
            return _map;
        }
    }

    private int _countTileMapX=0;
    private int countTileMapX {
        get
        {
            if (_countTileMapX == 0)
            {
                _countTileMapX = MapGenerator.SizeX - 1;
            }
            return _countTileMapX;
        }
    }

    private int _countTileMapY = 0;
    private int countTileMapY {
        get
        {
            if (_countTileMapY == 0)
            {
                _countTileMapY = MapGenerator.SizeY - 1;
            }
            return _countTileMapY;
        }
    }

    
    public void OnInject()
    {
        tileSize = TileDataProvider.TileSize;
    
    }

    Vector2Int GetMapPos(Vector2 pos) {
        if (tileSize == 0)
        {
            tileSize = TileDataProvider.TileSize;
        }
        ///Mathf.CeilToInt
        int offsetX = (int)((pos.x + 1.5f * tileSize) / tileSize)+1;
        int offsetY = (int)((pos.y + 1.5f * tileSize) / tileSize)+1;
        
        if (offsetX < 0)
            offsetX = 0;
        if (offsetX > countTileMapX)
            offsetX = countTileMapX;

        if (offsetY < 0)
            offsetY = 0;
        if (offsetY > countTileMapY)
            offsetY = countTileMapY;
        
        return new Vector2Int(offsetX, offsetY);
    }

    Vector2 GetWorldPos(Vector2Int pos) {
        if (tileSize == 0)
        {
            tileSize = TileDataProvider.TileSize;
        }
        float offsetX = (pos.x - 2f) * tileSize;
        float offsetY = (pos.y - 2f) * tileSize;
        return new Vector2(offsetX, offsetY); 
    }

    public List<TileInfoStructure> GetTileInLine(Vector2 startPos, Vector2 endPos) {
        List<TileInfoStructure> rez = new List<TileInfoStructure>();

        var pos = GetPosTileInLine(startPos, endPos);
        foreach (var p in pos) {
            rez.Add(Map[p.x, p.y]);
        }
       
        return rez;
    }

    public List<Vector2Int> GetPosTileInLine(Vector2 startPos, Vector2 endPos) {
        List<Vector2Int> rezPos = new List<Vector2Int>();
        Vector2Int p0 = GetMapPos(startPos);
        Vector2Int p1 = GetMapPos(endPos);

        int minX = p0.x <= p1.x ? p0.x : p1.x;
        int minY = p0.y <= p1.y ? p0.y : p1.y;

        int maxX = p0.x >= p1.x ? p0.x : p1.x;
        int maxY = p0.y >= p1.y ? p0.y : p1.y;

        for (int x = minX; x <= maxX; x++) {
            for (int y = minY; y <= maxY; y++) {
                var k1 = (x - p0.x) * (p1.y - p0.y);
                var k2 = (y - p0.y) * (p1.x - p0.x);
                if (Mathf.Abs(k1) <12.5f &&  Mathf.Abs(k2) <12.5f) {
                    rezPos.Add(new Vector2Int(x, y));
                }
            }
        }

        return rezPos;
    }


    //вернёт ооффсеты не нулевые в квадрате startPos endPos
    public BufferItem GetCollidersInRect(Vector2 startPos, Vector2 endPos, bool includingAll = false)
    {
        //List<Vector2Int> rezPos = new List<Vector2Int>();
        Buffer.NextBuffer();

        Vector2Int p0 = GetMapPos(startPos);
        Vector2Int p1 = GetMapPos(endPos);

        int minX = p0.x <= p1.x ? p0.x : p1.x;
        int minY = p0.y <= p1.y ? p0.y : p1.y;

        int maxX = p0.x >= p1.x ? p0.x : p1.x;
        int maxY = p0.y >= p1.y ? p0.y : p1.y;

        for (int x = minX; x <= maxX; x++) {
            for (int y = minY; y <= maxY; y++) {
                if (includingAll)
                {
                    Buffer.Add(new Vector2Int(x, y));
                    
                } else {
                    if (!Map[x, y].IsEmpty()) {
                        Buffer.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        return Buffer.GetCurrent;
    }

    public Vector2 ChackVelocity(Vector2 position, Vector2 velocity, float width, float height) {
        var rezX = ChackVelocity0(position, new Vector2(velocity.x, 0), width, height);
        var rezY = ChackVelocity0(position, new Vector2(0, velocity.y), width, height);

        var rezX2 = ChackVelocity0(position + rezY, new Vector2(velocity.x, 0), width, height);
        var rezY2 = ChackVelocity0(position + rezX, new Vector2(0, velocity.y), width, height);

        var x0 = Vector2.Distance(Vector2.zero, rezX);
        var x1 = Vector2.Distance(Vector2.zero, rezX2);
        if (x1 < x0) {
            rezX = rezX2;
        }

        var y0 = Vector2.Distance(Vector2.zero, rezY);
        var y1 = Vector2.Distance(Vector2.zero, rezY2);

        if (y1 < y0) {
            rezY = rezY2;
        }

        return rezX + rezY;
    }

    public Vector2 ChackVelocity0(Vector2 position, Vector2 velocity, float width, float height) {
        if (velocity == Vector2.zero)
            return velocity;
        var normVelocity = velocity.normalized;
 
        float x = width / 2;
        float y = height / 2;

        var collisions = Raycast(position, new Vector2(width, height), velocity);

        if (collisions.Count == 0) {
            return velocity;
        }

        //находим ближайший к нам
        float minDistance = -1;
        Vector2 wPos = Vector2.zero;
        for (int i = 0; i < collisions.Count; i++)
        {
            var col = collisions[i];
            var wPos2 = GetWorldPos(col);
            var dist = Vector2.Distance(position, wPos2);
            if (wPos == Vector2.zero || dist < minDistance)
            {
                minDistance = dist;
                wPos = wPos2;
            }
        }


        var rez = Vector2.zero;
        float tileSize12 = TileDataProvider.TileSize/2;
        if (velocity.x == 0 && velocity.y != 0) {
            float newY = (wPos.y - tileSize12 * Mathf.Sign(normVelocity.y) - position.y - y * Mathf.Sign(normVelocity.y));
            rez = new Vector2(0, newY);
        } else
            if (velocity.y == 0 && velocity.x != 0) {
                float newX = (wPos.x - tileSize12 * Mathf.Sign(normVelocity.x) - position.x - x * Mathf.Sign(normVelocity.x));
                rez = new Vector2(newX, 0);
            }
        return rez;
    }

    public BufferItem Raycast(Vector2 position, Vector2 size, Vector2 ray, bool includingAll = false)
    {
        BufferItem rezX = Raycast0(position, size, /*new Vector2(ray.x, 0)*/ray, includingAll);
        //List<Vector2Int> rezY = Raycast0(position, size, new Vector2(0, ray.y), includingAll);
        //rezX.AddRange(rezY);



        return rezX;
    }

    
    private BufferItem Raycast0(Vector2 position, Vector2 size, Vector2 ray, bool includingAll = false)
    {
        if (ray == Vector2.zero)
        {
            Buffer.NextBuffer();
            return Buffer.GetCurrent;
        }
            

        float x = size.x / 2;
        float y = size.y / 2;

        var normVelocity = ray.normalized;
        var normVelocity90 = Vector2.zero;
        //-2 начинает колизится юнит при ходьбе угол об угол 
        if (ray.x == 0) {
            normVelocity90 = new Vector2(x - 2f, 0);
        } else {
            normVelocity90 = new Vector2(0, y - 2f);
        }

        var normVelocityD = new Vector2(normVelocity.x * x, normVelocity.y * y);

        var frontPos = (position - normVelocity90) + normVelocityD;
        var newPos = (position + normVelocity90) + normVelocityD + ray;

        return GetCollidersInRect(frontPos, newPos, includingAll);
    }

    private Vector2 test = Vector2.zero;
    public Vector2 GetTest() {
        return test;
    }
}

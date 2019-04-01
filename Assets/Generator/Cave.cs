using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave {
    public List<Vector2Int> Body;
    public List<Vector2Int> Top = new List<Vector2Int>();
    public List<Vector2Int> Bottom = new List<Vector2Int>();

    public int minX = int.MaxValue;
    public int maxX = int.MinValue;

    public int minY = int.MaxValue;
    public int maxY = int.MinValue;
    

    private TileInfoStructure[,] Map;
    public Cave(TileInfoStructure[,] map, Vector2Int start, int surfase = -1) {
        Map = map;
        Body = GetBodyCave(start, -1, surfase);
        foreach (var point in Body) {
            if (point.x < minX) {
                minX = point.x;
            }

            if (point.x > maxX) {
                maxX = point.x;
            }

            if (point.y < minY) {
                minY = point.y;
            }

            if (point.y > maxY) {
                maxY = point.y;
            }
        }


        //обходим пещеры формируем caveTop caveBottom
        foreach (var caveXY in Body) {
            var pointTop = new Vector2Int(caveXY.x, caveXY.y + 1);
            var pointBottom = new Vector2Int(caveXY.x, caveXY.y - 1);

            if (IsValidPoint(pointTop) && IsValidPoint(pointBottom)) {
                if (Map[pointTop.x, pointTop.y].type != 0 && Map[pointBottom.x, pointBottom.y].type == 0) {
                    Top.Add(caveXY);
                }

                if (Map[pointBottom.x, pointBottom.y].type != 0 && Map[pointTop.x, pointTop.y].type == 0) {
                    Bottom.Add(caveXY);
                }
            }
        }
    }

    public bool IsValidPoint(Vector2Int pos, int maxX = -1, int maxY = -1) {
        var countTileMapX = maxX == -1 ? Map.GetLength(0) - 1 : maxX;
        var countTileMapY = maxY == -1 ? Map.GetLength(1) - 1 : maxY;

        if (pos.x > 0 && pos.y > 0 && pos.x <= countTileMapX && pos.y <= countTileMapY) {
            return true;
        }
        return false;
    }

    List<Vector2Int> GetBodyCave(Vector2Int start, int maxX = -1, int maxY = -1) {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int point = start;

        Dictionary<Vector2Int,bool> mask = new Dictionary<Vector2Int, bool>();

        int delta = 0;

        while (true) {
            if (!mask.ContainsKey(point)){ //path.TrueForAll(x => !x.IsEqual(point))) {
                path.Add(point);
                mask.Add(point, true);
            }

            var left = point.Left();
            if (IsValidPoint(left, maxX, maxY) &&
                Map[left.x, left.y].type == 0 &&
                //path.TrueForAll(x => !x.IsEqual(left))
                !mask.ContainsKey(left)
                ) {

                point = left;
                delta = 0;
                continue;

            }

            var right = point.Right();
            if (IsValidPoint(right, maxX, maxY) &&
                Map[right.x, right.y].type == 0 &&
                //path.TrueForAll(x => !x.IsEqual(right))
                !mask.ContainsKey(right)
                ) {

                point = right;
                delta = 0;
                continue;
            }

            var top = point.Top();
            if (IsValidPoint(top, maxX, maxY) &&
                Map[top.x, top.y].type == 0 &&
                //path.TrueForAll(x => !x.IsEqual(top))
                !mask.ContainsKey(top)
                ) {

                point = top;
                delta = 0;
                continue;

            }

            var bottom = point.Bottom();
            if (IsValidPoint(bottom, maxX, maxY) &&
                Map[bottom.x, bottom.y].type == 0 &&
                //path.TrueForAll(x => !x.IsEqual(bottom))
                !mask.ContainsKey(bottom)
                ) {

                point = bottom;
                delta = 0;
                continue;
            }

            //вернулись в начало
            if (point.IsEqual(start)) {
                break;
            }

            delta++;
            point = path[path.Count - 1 - delta];
        }

        return path;
    }

    public bool IsIncludePoint(Vector2Int pos) {
        if (pos.x > maxX || pos.x < minX || pos.y > maxY || pos.y < minY)
            return false;
        
        for (int i = 0; i < Body.Count; i++) {
            if (pos.IsEqual(Body[i]))
                return true;
        }
        
        return false;
        
        return true;
    }
}

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using System.Linq;

public interface ILightRendererOld {
    /*void Update();
    void AddPoint(LightPoint point);
    List<LightPoint> GetAll();*/
}

public class LightRendererOldOld : ILightRendererOld {
    /*
    [IoC.Inject]
    public MapGenerator mapGenerator { set; protected get; }

    [IoC.Inject]
    public ITileDataProvider tileDataProvider { set; protected get; }

    private TileInfoStructure[,] Map {
        get {
            return mapGenerator.GetMap();
        }
    }

    public bool IsValidPoint(int x, int y) {
        var countTileMapX = mapGenerator.SizeX - 1;
        var countTileMapY = mapGenerator.SizeY - 1;

        if (x > 0 && y > 0 && x <= countTileMapX && y <= countTileMapY) {
            return true;
        }
        return false;
    }

    private TileInfoStructure GetMap(int x, int y) {
        if (IsValidPoint(x, y))
            return Map[x, y];
        return new TileInfoStructure();
    }


    private List<LightPoint> points = new List<LightPoint>();

    public void AddPoint(LightPoint point) {
        points.Add(point);
    }

    public List<LightPoint> GetAll() {
        return points;
    }

    struct SourceDist {
        public Vector2Int pos;
        public float dist;
    }

    float maxTime = 20;
    float currTime = 0;
    public void Update() {

        List<Vector2Int> L = new List<Vector2Int>();
        List<Vector2Int> R = new List<Vector2Int>();
        List<Vector2Int> U = new List<Vector2Int>();
        List<Vector2Int> D = new List<Vector2Int>();
        
        List<Vector2Int> letsTiles = new List<Vector2Int>();
        List<SourceDist> sources = new List<SourceDist>();

        byte light = 0;
        for (int y = TileRenderer.screenEnd.y; y > TileRenderer.screenStart.y; y--) {
            
            for (int x = TileRenderer.screenStart.x; x < TileRenderer.screenEnd.x; x++) {

                
                var point = new Vector2Int(x, y);
                var currEmpty = GetMap(x, y).IsEmpty();
                if (!currEmpty) {
                    letsTiles.Add(point);
                }

                if (x > 0 && x < MapGenerator.surface.Count && y > MapGenerator.surface[x].y) {
    
                    var lastEmpty = GetMap(x - 1, y).IsEmpty();
                     
                    var uEmpty = GetMap(x, y + 1).IsEmpty();
                    var dEmpty = GetMap(x, y - 1).IsEmpty();

                    
                    if (lastEmpty && !currEmpty) {
                       //R.Add(new Vector2Int(x - 1, y));
                        var pos = new Vector2Int(x - 1, y);
                        sources.Add(new SourceDist() {
                            pos = pos,
                           // dist = Vector2.Distance(pos.GetVector2(), Vector2.zero)
                        });
                    }
                    else
                    if (!lastEmpty && currEmpty) {
                       // L.Add(point);
                        sources.Add(new SourceDist() {
                            pos = point,
                           // dist = Vector2.Distance(point.GetVector2(), Vector2.zero)
                        });
                    }else
                    if (!uEmpty && currEmpty) {
                      //  U.Add(point);
                        sources.Add(new SourceDist() {
                            pos = point,
                           // dist = Vector2.Distance(point.GetVector2(), Vector2.zero)
                        });
                    }else
                    if (!dEmpty && currEmpty) {
                    //    D.Add(point);
                        sources.Add(new SourceDist() {
                            pos = point,
                          //  dist = Vector2.Distance(point.GetVector2(), Vector2.zero)
                        });
                    }
                    else
                    if (dEmpty && currEmpty && y -1== MapGenerator.surface[x].y) {
                   //     D.Add(point);
                        sources.Add(new SourceDist() {
                            pos = point,
                           // dist = Vector2.Distance(point.GetVector2(), Vector2.zero)
                        });
                    }
                } else {

                
                }
            }   
        }

        for (int i = 0; i < points.Count; i++) {
            if (points[i].isUpdate) {
                points[i].isUpdate = false;
                var lastOffser = tileDataProvider.WorldPosToOffsetTile(points[i].LastPos);
                var lastSize = (int) points[i].LastSize;
                var newOffser = tileDataProvider.WorldPosToOffsetTile(points[i].Pos);
                var newSize = (int) points[i].Size;
                if (lastOffser.x != newOffser.x || lastOffser.y != newOffser.y || lastSize != newSize) {
                    СircleLightPoint(lastOffser, (int) points[i].LastSize, points[i].GetId(), false);
                    СircleLightPoint(newOffser, (int) points[i].Size, points[i].GetId(), true);
                }
            }
        }

    //    sources = sources.OrderBy(x => x.dist).ToList();
        
        
        int[,] letsScreen = new int[TileRenderer.screenEnd.x - TileRenderer.screenStart.x +1, TileRenderer.screenEnd.y - TileRenderer.screenStart.y+1];

        foreach (var letTile in letsTiles) {
            var dist = Vector2.Distance(letTile.GetVector2(), Vector2.zero);
            float oldDelta = float.MaxValue;
            Vector2Int source = new Vector2Int(int.MaxValue, int.MaxValue);
            //находим ближайший источник света
            for (int i = 0; i < sources.Count; i++) {
                var newDelta = Vector2.Distance(sources[i].pos.GetVector2(), letTile.GetVector2()); 
                if (newDelta < oldDelta) {
                    oldDelta = newDelta;
                    source = sources[i].pos;
                } else {
                    //break;
                }
            }
            //находим точку в матрице letsScreen
            var letsPosX = letTile.x - TileRenderer.screenStart.x;
            var letsPosY = letTile.y - TileRenderer.screenStart.y;
            try {
                letsScreen[letsPosX, letsPosY] = (byte)Vector2.Distance(source.GetVector2(), letTile.GetVector2());
            }
            catch (Exception) {
                Debug.LogErrorFormat("letsPosX = {0}, letsPosY = {1}, sizeX = {2}, sizeY = {3}", letsPosX, letsPosY, letsScreen.GetLength(0), letsScreen.GetLength(1));
            }
            
        }

        float radius = 10;
        var onePointLight = 1 / (float)radius;
        //light
        for (int x = 0; x < letsScreen.GetLength(0); x++) {
            for (int y = 0; y < letsScreen.GetLength(1); y++) {
                var mapPosX = TileRenderer.screenStart.x + x;
                var mapPosY = TileRenderer.screenStart.y + y;
                var a = onePointLight*letsScreen[x, y];
                byte alpha = (byte) (Mathf.Clamp(light + Mathf.Lerp(0, 255, a), 0, 255));

                if (GetMap(mapPosX, mapPosY).lightInfo == null) {
                    GetMap(mapPosX, mapPosY).lightInfo = new LightInfoStructure() {
                        R = 0,
                        G = 0,
                        B = 0,
                        A = alpha
                    };
                } else {
                    GetMap(mapPosX, mapPosY).lightInfo.IncAlpha(-1, alpha);
                }
            }
        }

    }

    int[,] lets = new int[100,100];
    public void СircleLightPoint(Vector2Int center, int radius, int sourceId, bool isInc) {     
        for (int x = 0; x < radius; x++) {
            int letUpL = 0;
            int letUpR = 0;
            int letDownL = 0;
            int letDownR = 0;
            if (x != 0) {
                letUpL = lets[radius + (x - 1), radius];
                letUpR = lets[radius - (x-1), radius];
                letDownL = lets[radius + (x - 1), radius];
                letDownR = lets[radius - (x - 1), radius];
            }

            for (int y = 0; y < radius; y++) {
                Vector2Int upL = new Vector2Int(center.x + x, center.y + y);
                Vector2Int upR = new Vector2Int(center.x - x, center.y + y);
                Vector2Int downL = new Vector2Int(center.x + x, center.y - y);
                Vector2Int downR = new Vector2Int(center.x - x, center.y - y);

                if (!GetMap(upL.x, upL.y).IsEmpty()) {
                    letUpL++;
                    letUpL++;
                }
                if (!GetMap(upR.x, upR.y).IsEmpty()) {
                    letUpR++;
                    letUpR++;
                }
                if (!GetMap(downL.x, downL.y).IsEmpty()) {
                    letDownL++;
                    letDownL++;
                }
                if (!GetMap(downR.x, downR.y).IsEmpty()) {
                    letDownR++;
                    letDownR++;
                }

                lets[radius + x, radius + y] = letUpL;
                lets[radius - x, radius + y] = letUpR;
                lets[radius + x, radius - y] = letDownL;
                lets[radius - x, radius - y] = letDownR;
            }
        }

        for (int y = 0; y < radius; y++) {
            int letUpL = lets[radius , radius + y];
            int letUpR = lets[radius , radius + y];
            int letDownL = lets[radius + 0, radius - y];
            int letDownR = lets[radius - 0, radius - y];

            for (int x = 0; x < radius; x++) {
                Vector2Int upL = new Vector2Int(center.x + x, center.y + y);
                Vector2Int upR = new Vector2Int(center.x - x, center.y + y);
                Vector2Int downL = new Vector2Int(center.x + x, center.y - y);
                Vector2Int downR = new Vector2Int(center.x - x, center.y - y);

                if (!GetMap(upL.x, upL.y).IsEmpty()) {
                    letUpL++;
                    letUpL++;
                }
                if (!GetMap(upR.x, upR.y).IsEmpty()) {
                    letUpR++;
                    letUpR++;
                }
                if (!GetMap(downL.x, downL.y).IsEmpty()) {
                    letDownL++;
                    letDownL++;
                }
                if (!GetMap(downR.x, downR.y).IsEmpty()) {
                    letDownR++;
                    letDownR++;
                }
                lets[radius + x, radius + y] = Mathf.Min(lets[radius + x, radius + y], letUpL);
                lets[radius - x, radius + y] = Mathf.Min(lets[radius - x, radius + y], letUpR);
                lets[radius + x, radius - y] = Mathf.Min(lets[radius + x, radius - y], letDownL);
                lets[radius - x, radius - y] = Mathf.Min(lets[radius - x, radius - y], letDownR);
            }
        }

        var onePointLight = 1/(float)radius;
        for (int x = center.x - radius; x < center.x + radius; x++) {
            for (int y = center.y - radius; y < center.y + radius; y++) {
                var coordinateX = Mathf.Abs(x - center.x);
                var coordinateY = Mathf.Abs(y - center.y);
                var let = lets[x - center.x + radius, y - center.y + radius];
                GetMap(x, y).lightAlpha = onePointLight * coordinateX + onePointLight * coordinateY +let * onePointLight;
                var a = GetMap(x, y).lightAlpha;
                a = a*a*a;
                byte alpha = (byte) Mathf.Lerp(0, 255, a);

        

                if (GetMap(x, y).lightInfo == null) {
                    GetMap(x, y).lightInfo = new LightInfoStructure() {
                        R = 0,
                        G = 0,
                        B = 0,
                        A = alpha
                    };
                } else {
                    if (isInc)
                        GetMap(x, y).lightInfo.IncAlpha(sourceId, alpha);
                    else {
                        GetMap(x, y).lightInfo.DecAlpha(sourceId);
                    }
                }
            }
        }
    }


    void PrintMap(int[,] map) {
        string rez = "";
        for (int x = 0; x < map.GetLength(1); x++) {
            for (int y = 0; y < map.GetLength(0); y++) {

                rez += " " + map[x, y];
            }
            rez += "\n";
        }

        Debug.LogError(rez);
    }


    //Алгоритм Брезенхема  вернёт координаты окружности
    private List<Vector2Int> GetСirclePoints(Vector2 pos, int radius = 1) {
        var offset = tileDataProvider.WorldPosToOffsetTile(pos);
        int offsetX = offset.x;
        int offsetY = offset.y;
        var center = new Vector2Int(offsetX, offsetY);
        List<Vector2Int> circlePoints = new List<Vector2Int>();

        int x = 0;
        int y = radius;
        int delta = 1 - 2 * radius;
        int error = 0;
        while (y >= 0) {
            circlePoints.Add(new Vector2Int(center.x + x, center.y + y));
            circlePoints.Add(new Vector2Int(center.x + x, center.y - y));
            circlePoints.Add(new Vector2Int(center.x - x, center.y + y));
            circlePoints.Add(new Vector2Int(center.x - x, center.y - y));

            error = 2 * (delta + y) - 1;
            if ((delta < 0) && (error <= 0)) {
                delta += 2 * ++x + 1;
                continue;
            }
            error = 2 * (delta - x) - 1;
            if ((delta > 0) && (error > 0)) {
                delta += 1 - 2 * --y;
                continue;
            }
            x++;
            delta += 2 * (x - y);
            y--;
        }

        return circlePoints;
    }

    //Алгоритм Брезенхема линия
    private List<Vector2Int> GetLinePoints(Vector2Int pos1, Vector2Int pos2) {
        List<Vector2Int> rez = new List<Vector2Int>();
        int x1 = pos1.x;
        int y1 = pos1.y;

        int x2 = pos2.x;
        int y2 = pos2.y;

        int deltaX = Mathf.Abs(x2 - x1);
        int deltaY = Mathf.Abs(y2 - y1);
        int signX = x1 < x2 ? 1 : -1;
        int signY = y1 < y2 ? 1 : -1;
        int error = deltaX - deltaY;

        rez.Add(new Vector2Int(x2, y2));

        while (x1 != x2 || y1 != y2) {
            rez.Add(new Vector2Int(x1, y1));
            int error2 = error * 2;
            if (error2 > -deltaY) {
                error -= deltaY;
                x1 += signX;
            }
            if (error2 < deltaX) {
                error += deltaX;
                y1 += signY;
            }
        }
        return rez;
    }

    //алгоритм рисования равномерного пятна
    void SpotRender(Vector2Int center, List<Vector2Int> borderPoints) {
        foreach (var point in borderPoints) {
            int x1 = center.x;
            int y1 = center.y;

            int x2 = point.x;
            int y2 = point.y;

            int deltaX = Mathf.Abs(x2 - x1);
            int deltaY = Mathf.Abs(y2 - y1);
            int signX = x1 < x2 ? 1 : -1;
            int signY = y1 < y2 ? 1 : -1;
            int error = deltaX - deltaY;

            Vector2 posNew = new Vector2(x2, y2);
            Vector2 pos0 = center.GetVector2();
            var r = Vector2.Distance(pos0, posNew);
            float invers = Mathf.InverseLerp(0, r, Vector2.Distance(pos0, posNew));
            var alpha = Mathf.Lerp(0, 1f, invers);

            GetMap(x2, y2).lightAlpha = alpha;
            int letCount = 0;

            while (x1 != x2 || y1 != y2) {
                posNew = new Vector2(x1, y1);
                pos0 = center.GetVector2();
                invers = Mathf.InverseLerp(0, r, Vector2.Distance(pos0, posNew));
                alpha = Mathf.Lerp(0, 1f, invers);

                if (!GetMap(x1, y1).IsEmpty()) {
                    letCount++;
                    // alpha = 1;
                }

                var add = 0;// Mathf.Lerp(0, 0.5f, letCount / 6f);
                alpha = Mathf.Clamp(alpha + add, 0, 1);

                GetMap(x1, y1).lightAlpha = alpha;
                int error2 = error * 2;
                //
                if (error2 > -deltaY) {
                    error -= deltaY;
                    x1 += signX;
                }
                if (error2 < deltaX) {
                    error += deltaX;
                    y1 += signY;
                }
            }
        }
    }

    void LineRender(Vector2Int center, List<Vector2Int> points, int radius) {
        Vector2 pos0 = center.GetVector2();
        foreach (var point in points) {
            Vector2 posNew = new Vector2(point.x, point.y);
            float invers = Mathf.InverseLerp(0, radius, Vector2.Distance(pos0, posNew));
            var alpha = Mathf.Lerp(0, 1f, invers);

            GetMap(point.x, point.y).lightAlpha = alpha;
        }
    }

    void Render(List<Vector2Int> points, float alpha) {
        foreach (var point in points) {
            GetMap(point.x, point.y).lightAlpha = alpha;
        }
    }

    private void test(List<Vector2Int> ray, List<Vector2Int> border) {
        for (int i = 0; i < ray.Count; i++) {
            var fromAlpha = GetMap(ray[i].x, ray[i].y).lightAlpha;
            var toAlpha = 0f;

            var fromY = ray[i].y;
            var toY = border[i].y;

            float count = Mathf.Abs(toY - fromY);
            int currentIndex = 0;
            if (fromY < toY) {
                for (int y = fromY; y < toY; y++) {
                    currentIndex++;
                    GetMap(ray[i].x, y).lightAlpha = Mathf.Lerp(fromAlpha, 1, currentIndex/count); 
                }
            } else {
                for (int y = fromY; y > toY; y--) {
                    currentIndex++;
                    GetMap(ray[i].x, y).lightAlpha =  Mathf.Lerp(fromAlpha, 1, currentIndex/count); 
                }
            }
        }
    }
    */
}

public class LightPoint {
    private static int Id;
    private int id;
    public LightPoint() {
        id = Id++;
    }

    public int GetId() {
        return id;
    }

    public Vector2 Pos = Vector2.zero;
    public float Size = -1;

    public Vector2 LastPos;
    public float LastSize;
    public bool isUpdate = true;
    public void SetPosition(Vector2 pos) {
        if (Pos != pos) {
            isUpdate = true;

            if (Pos != Vector2.zero) {
                LastPos = Pos;
            } else {
                LastPos = pos;
            }

            
            Pos = pos;
        }
    }

    public void SetSize(float size) {
        if (Size != size) {
            isUpdate = true;
            if (Size != -1) {
                LastSize = Size;
            } else {
                LastSize = size;
            }
            Size = size;
        }
    }


}


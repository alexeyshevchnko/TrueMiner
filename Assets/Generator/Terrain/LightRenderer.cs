using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Managers;
using trasharia;
using UnityEngine;

public class LightRenderer : ILightRenderer,IoC.IInitialize {
    [IoC.Inject]
    public MapGenerator mapGenerator { set; protected get; }
    [IoC.Inject]
    public IScreenUpdateManager ScreenUpdateManager { set; protected get; }

    [IoC.Inject]
    public IBackgroundManager BackgroundManager { set; protected get; }

    [IoC.Inject]
    public IRandom Random { set; protected get; }

    private Camera camera;
    public void OnInject() {
        
        mapGenerator.OnGenerate += () => {
            Map = mapGenerator.GetMap();
            countTileMapX = mapGenerator.SizeX - 1;
            countTileMapY = mapGenerator.SizeY - 1; 
        };


        camera = Camera.main;
    }

    private static TileInfoStructure[,] Map;

    private static int countTileMapX;
    private static int countTileMapY;
    public bool IsValidPoint(int x, int y) {
        if (x > 0 && y > 0 && x <= countTileMapX && y <= countTileMapY) {
            return true;
        }
        return false;
    }

    private Vector2Int startScreen;
    private Vector2Int endScreen;

    public Vector2Int updateScreenMin;
    public Vector2Int updateScreenMax;
    public Vector2Int UpdateScreenMin { get { return updateScreenMin; } }
    public Vector2Int UpdateScreenMax { get { return updateScreenMax; } }

    private int screenWidth;
    private int screenHeight;

    public static Vector2Int minPos;
    public static Vector2Int maxPos;
    private static Vector2Int oldMinPos;
    private static Vector2Int oldMaxPos;

    public void SetScreenRect(Vector2Int start, Vector2Int end) {
        startScreen = start;
        endScreen = end;
        screenWidth = endScreen.x - startScreen.x;
        screenHeight = endScreen.y - startScreen.y;

        oldMinPos = minPos;
        oldMaxPos = maxPos;

        minPos = new Vector2Int(startScreen.x - RaySize, startScreen.y - RaySize);
        maxPos = new Vector2Int(endScreen.x + RaySize, endScreen.y + RaySize);
       // Debug.LogError(screenWidth);
       // Debug.LogError(screenHeight);
        if (color == null ){//}|| screenWidth + 2*offScreenTiles + 10 != color.GetLength(0)) {
            color = new ColorByte[screenWidth + 2 * RaySize + 10, screenHeight + 2 * RaySize + 10];
        }
    }

    public float currTime ;
    public float time = 100000;
    public float maxTime = 100000;
    public int dirTime = 1;

    public void Update() {
      //  return;
        time += Time.fixedDeltaTime * dirTime;
        if (time > maxTime) {
           // return;
        }
        if (time < 0) {
            time = 0; 
            dirTime = 1;
        }
        if (time > maxTime) {
            time = maxTime;
            dirTime = -1;
        }

       // Debug.LogError(time);

        var newTime = (byte)(ColorByte.white.A * (time / maxTime));
        var oldTime = (byte)(ColorByte.white.A * (currTime));
        if (newTime != oldTime) {
        //if (currTime != 0.9f){
            currTime = time / maxTime;
            ScreenUpdateManager.RedrawFullScreenLight();
        }
        //Debug.LogError(newTime);

        float val = Mathf.Clamp(currTime, 0,0.6f);

        camera.backgroundColor = new Color(val, val, val, 1);
        BackgroundManager.SetTime(currTime);
    }

    

    private int firstTileX;

    private int lastTileX;

    private int firstTileY;

    private int lastTileY;

    public static ColorByte[,] color;

    private static int maxTempLights = 100;

    private int[] tempLightX = new int[maxTempLights];

    private int[] tempLightY = new int[maxTempLights];

    private int[] tempLight = new int[maxTempLights];

    private int tempLightCount;

    private int minX;

    private int minY;

    private int maxX;

    private int maxY;

    public  static int RaySize = 13;

    public void CalculateChangeScreen(Vector2Int min, Vector2Int max) {
        if (color != null) {
        //    ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => {
                OutRectLightTiles(min.x, max.x, min.y, max.y, true);
        //    }));
        }
    }

    public void Swap(int deltaX, int deltaY) {
      //  OutRectLightTiles(minX, maxX, minY, maxY, true);
        
        if (deltaY != 0 || deltaX != 0) {
            var sizeX = color.GetLength(0) - 1;
            var sizeY = color.GetLength(1) - 1;

        //    UnityEngine.Profiling.Profiler.BeginSample("Swap");
             

                var startX = deltaX >= 0 ? 0 : sizeX - 1;
                var incX = deltaX >= 0 ? 1 : -1;
                var startY = deltaY >= 0 ? 0 : sizeY - 1;
                var incY = deltaY >= 0 ? 1 : -1;

                for (int x = startX; x >= 0 && x <= sizeX; x += incX) {
                    for (int y = startY; y >= 0 && y <= sizeY; y += incY) {

                        int fromX = x + deltaX;
                        var fromY = y + deltaY;
                        int toX = x;
                        int toY = y;
                        if (fromX < sizeX && fromX >= 0 && fromY < sizeY && fromY >= 0) {
                            color[toX, toY] = color[fromX, fromY];
                            color[fromX, fromY] = ColorByte.whiteTransparent;

                        } else {
                            color[toX, toY] = ColorByte.whiteTransparent;
                        }
                    } 
                }
            
          //  UnityEngine.Profiling.Profiler.EndSample();
  
            if (Mathf.Abs(deltaX) >= screenWidth || Mathf.Abs(deltaY) >= screenHeight) {
                OutRectLightTiles(startScreen.x - 21, endScreen.x + 21, startScreen.y - 21, endScreen.y + 21);
                OutRectLightTiles(startScreen.x - 21, endScreen.x + 21, startScreen.y - 21, endScreen.y + 21);
                return;
            }
         //   UnityEngine.Profiling.Profiler.BeginSample("OutRectLightTiles");
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

                
             //   ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => {
                    OutRectLightTiles(minX, maxX, minY, maxY);
             //   }));
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
                
              //  ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => {
                    OutRectLightTiles(minX, maxX, minY, maxY);
              //  }));
            }
            
        //    UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    public void OutRectLightTiles(int firstX, int lastX, int firstY, int lastY, bool reDrdaw = false) {
        //**обрезаем квадрат чтобы 1 он не вышел за граници масcива 2е чтобы был не меньше луча
        minX = firstX - RaySize;
        maxX = lastX + RaySize;

       // if (minX < minPos.x) {
            minX = minPos.x;
       // }
       // if (maxX > maxPos.x) {
            maxX = maxPos.x;
       // }

        minY = firstY - RaySize;
        maxY = lastY + RaySize;  

       // if (minY < minPos.y) {
            minY = minPos.y;
       // }

       // if (maxY > maxPos.y) {
            maxY = maxPos.y;  
       // }

        //Debug.LogErrorFormat("({0},{1}) ({2},{3})", minX, minY, maxX, maxY);
        //**
         
        //граници экрана которые нужно перерисовать v
        updateScreenMin = new Vector2Int(minX, minY);
        updateScreenMax = new Vector2Int(maxX, maxY);


        //если перерисовываем квадрат а не5 свап то обнуляем всё кроме границ
        if (reDrdaw) {
            for (int x = minX + 1; x < maxX - 1; x++) {
                for (int y = minY + 1; y < maxY - 1; y++) {
                    color[x - minPos.x, y - minPos.y] = ColorByte.whiteTransparent;
                }
            }
        }

        for (int x = minX; x <= maxX; x++) { 
            for (int y = minY; y <= maxY; y++) {
                if (x > 0 && y > 0 && x <= countTileMapX && y <= countTileMapY/*IsValidPoint(x, y)*/) {
                    var map = Map[x, y];
                    if (map.lightAlpha != 0) {
                        color[x - minPos.x, y - minPos.y] = ColorByte.white;
                        color[x - minPos.x, y - minPos.y].A =
                            (byte) (color[x - minPos.x, y - minPos.y].A*map.lightAlpha);
                    }else
                    if (map.ItemId == 1) {
                        color[x - minPos.x, y - minPos.y] = ColorByte.white;
                    } else {

                        bool isWater = //map.count > 9;//
                            map.IsWater2();
                        bool isEmpty = /*(map.type == 0 ||
                         map.type == 6 || map.type == 7//IsTree()
                    || map.type == (short)TileTypeEnum.Jungle || map.type == (short)TileTypeEnum.Grass1 || map.type == (short)TileTypeEnum.Grass2//IsDecor()
                   || map.liquidType == 1);//IsWater()*/
                            map.IsEmpty2() && !isWater;
                        //ToDo убрал затемнение в пустотах
                        bool lighted = (/*map.typeBG == 0 &&*/ isEmpty /*&& (double)y > mapGenerator.SurfaceCenterY*/);
                            


                        if ( /*isEmpty &&*/ lighted) {
                            color[x - minPos.x, y - minPos.y] = ColorByte.white;
                            color[x - minPos.x, y - minPos.y].A = (byte) (color[x - minPos.x, y - minPos.y].A*currTime);

                        }
                    }

                }
            }
        }
        
        //в color 0й это rectMin.x максимальный  rectMax.x
        int add = 0;//reDrdaw ? 1 : 0;
        ColorByte marker;
        int count = 2;//reDrdaw ? 2 : 1;
        for (int i = 0; i < count; i++) {
            //y = >> x ; x <<-
            for (int y = minY; y <= maxY; y++) {
                marker = GetMarker(minX , y);
                for (int x = minX + add; x <= maxX - add; x++) {
                    marker = LightColor(x, y, marker);
                }
                marker = GetMarker(maxX, y);
                for (int x = maxX - add; x > minX + add; x--) {
                   marker = LightColor(x, y, marker);
                }
            }
            //x = >> y ; y <<-
            for (int x = minX; x <= maxX; x++) {
                marker = GetMarker(x, minY );
                for (int y = minY + add; y <= maxY - add; y++) {
                    marker = LightColor(x, y, marker);
                }
                marker = GetMarker(x, maxY );
                for (int y = maxY - add; y > minY + add; y--) {
                    marker = LightColor(x, y, marker);
                    
                }
            }
        } 

    } 
   //     [MethodImpl(MethodImplOptions.Unmanaged)]
    private static ColorByte GetMarker(int x, int y) {
        var rez = ColorByte.whiteTransparent;
        if (x - minPos.x >= 0 && y - minPos.y >= 0 && 
            x - minPos.x <= maxPos.x && y - minPos.y <= maxPos.y) {
                rez = color[x - minPos.x, y - minPos.y];
        }

        return rez;
    }

  //  [MethodImpl(MethodImplOptions.Unmanaged)]
    private static ColorByte LightColor(int x, int y, ColorByte marker) {

        int offsetX = x - minPos.x;
        int offsetY = y - minPos.y;

        if (color[offsetX, offsetY].A > marker.A) {
            marker = color[offsetX, offsetY];
        } else {
            color[offsetX, offsetY] = marker;
        }
        int deltaA;

        var map = TileInfoStructure.Free;
        if (x > 0 && y > 0 && x <= countTileMapX && y <= countTileMapY /*IsValidPoint(x, y)*/) {
            map = Map[x, y];
        }
        bool isWater = map.count > 9;//map.IsWater2();
        bool isEmpty = (map.type == 0 ||
            /*IsTree()*/ map.type == 6 || map.type == 7 || map.type == 16 || map.type == 15
        || /*IsDecor()*/  map.type == (short)TileTypeEnum.Jungle || map.type == (short)TileTypeEnum.Grass1 || map.type == (short)TileTypeEnum.Grass2
       ||/*IsWater()*/ map.liquidType == 1) && !isWater;//map.IsEmpty2() && !isWater;

        if (isEmpty) { 
            deltaA = 10;
        } else
            if (isWater) {
                deltaA =  5; //20;
            } else {
                deltaA = 40; 
            }

      //  if (map.ItemId == 1)
      //      deltaA = 10;

        int resultA = (int)marker.A - deltaA;
        if (resultA < 0) {
            resultA = 0;
        }
        if (resultA > 255) {
            resultA = 255;
        }
        marker.A = (byte)resultA;

        map.lightAlpha2 = marker.A;

        /*
        if (isEmpty) {
            //if ((double) y > MapGenerator.surface[x].y - 50) {
            map.lighted = true;
            // }
        } else {
            map.lighted = false;
        } */

        return marker;
    }


    public void Test() {
        

        ColorByte [,] colorTmp = new ColorByte[screenWidth + 2 * RaySize + 10, screenHeight + 2 * RaySize + 10];
        var sizeX = color.GetLength(0) - 1;
        var sizeY = color.GetLength(1) - 1;
        for (int x = 0; x < sizeX; x++) {
            for (int y = 0; y < sizeY; y++) {
                colorTmp[x, y] = new ColorByte(0, 0, 0, (int)color[x, y].A);
            }
        }

        for (int i = 0; i < 100; i++) {
            var xTest = UnityEngine.Random.Range(minPos.x, maxPos.x);
            var yTest = UnityEngine.Random.Range(minPos.y, maxPos.y);
            OutRectLightTiles(xTest, xTest, yTest, yTest, true);
            Test2(ref colorTmp, ref color);      
        }
       
    }

    public void Test2(ref ColorByte [,] m1,ref ColorByte [,] m2) {
        var sizeX = m1.GetLength(0) - 1;
        var sizeY = m1.GetLength(1) - 1;
        for (int x = 0; x < sizeX; x++) {
            for (int y = 0; y < sizeY; y++) { 
                if (m1[x, y].A != m2[x, y].A) {
                    Debug.LogErrorFormat("bed {0} != {1}", m1[x, y].A, m2[x, y].A);
                    
                    return;
                }
            }
        }

        Debug.Log("ok");
    }


    /*
     public void addLight(int i, int j, byte Lightness) {
         if (tempLightCount != maxTempLights) {
             if (i - firstTileX + 21 >= 0 && i - firstTileX + 21 < screenWidth + 42 + 10 &&
                 j - firstTileY + 21 >= 0 && j - firstTileY + 21 < screenHeight / 16 + 42 + 10) {
                 tempLightX[tempLightCount] = i;
                 tempLightY[tempLightCount] = j;
                 tempLight[tempLightCount] = (int)Lightness;
                 tempLightCount++;
             }
         }
     }
    public int LightingX(int lightX) {
        int result;
        if (lightX < 0) {
            result = 0;
        } else
            if (lightX >= screenWidth + 42 + 10) {
                result = screenWidth + 42 + 10 - 1;
            } else {
                result = lightX;
            }
        return result;
    }

    public int LightingY(int lightY) {
        int result;
        if (lightY < 0) {
            result = 0;
        } else
            if (lightY >= screenHeight + 42 + 10) {
                result = screenHeight + 42 + 10 - 1;
            } else {
                result = lightY;
            }
        return result;
    }*/
   
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Managers {
    public class ScreenUpdateManager : IScreenUpdateManager {
        private int sixeX;
        private int sizeY;

        private int sixeObstacleLightX;
        private int sizeObstacleLightY;

        private Vector2Int startScreenOffset;
        private Vector2Int startObstacleLightOffset;
        private Vector2Int endScreenOffset;

        private Vector2Int LightUptateMin;
        private Vector2Int LightUptateMax;
        
        private List<byte[,]> screenMask;

        private byte[,] obstacleLightScreenMask;

        public void SetScreenRect(Vector2Int start, Vector2Int end) {
            startScreenOffset = start;
            endScreenOffset = end;
            var addSize = LayerData.ObstacleLightLayer.SizeAdd;
            startObstacleLightOffset = new Vector2Int(startScreenOffset.x - addSize, startScreenOffset.y - addSize);
        }

        public void Init(int sixeX, int sizeY) {
            var addSize = LayerData.ObstacleLightLayer.SizeAdd * 2;

            this.sixeX = sixeX;
            this.sizeY = sizeY;
            this.sixeObstacleLightX = sixeX + addSize;
            this.sizeObstacleLightY = sizeY + addSize;

            screenMask = new List<byte[,]>();
            foreach (var layer in LayerData.Layers) {
                var newLayer = new byte[sixeX, sizeY];

                for (int x = 0; x < newLayer.GetLength(0); x++) {
                    for (int y = 0; y < newLayer.GetLength(1); y++) {
                        newLayer[x, y] = 1;
                    }
                }
                screenMask.Add(newLayer);
            }
            //Debug.LogError(screenMask.Count);

            obstacleLightScreenMask = new byte[sixeX + addSize, sizeY + addSize];
            for (int x = 0; x < obstacleLightScreenMask.GetLength(0); x++) {
                for (int y = 0; y < obstacleLightScreenMask.GetLength(1); y++) {
                    obstacleLightScreenMask[x, y] = 1;
                }
            }
        }

        public void RedrawWorldTile(int layer, int worldOffsetX, int worldOffsetY) {
            var screenOffsetX = worldOffsetX - startScreenOffset.x;
            var screenOffsetY = worldOffsetY - startScreenOffset.y;

            if (screenOffsetX >= 0 && screenOffsetX < sixeX &&
                screenOffsetY >= 0 && screenOffsetY < sizeY) {
                screenMask[layer][screenOffsetX, screenOffsetY] = 1;
                isApplayChanges = true;
            }

            
            
            var obstacleOffsetX = worldOffsetX - (startObstacleLightOffset.x);
            var obstacleOffsetY = worldOffsetY - (startObstacleLightOffset.y);
            if (obstacleOffsetX >= 0 && obstacleOffsetX < sixeObstacleLightX &&
                obstacleOffsetY >= 0 && obstacleOffsetY < sizeObstacleLightY) {

                obstacleLightScreenMask[obstacleOffsetX, obstacleOffsetY] = 1;
                isChangeObstacleLight = true;
            }
        } 

        public void RedrawWorldTile3X(int layer, int worldOffsetX, int worldOffsetY) {
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    RedrawWorldTile(layer, worldOffsetX + i, worldOffsetY + j);
                }
            }

            if (TryAddUpdateLightPoint(worldOffsetX, worldOffsetY)) {
                isChangeLight = true;
            }
            
        }


        public void EndDrawObstacleLightTile(int screenOffsetX, int screenOffsetY) {
            if (screenOffsetX >= 0 && screenOffsetX < sixeObstacleLightX &&
                screenOffsetY >= 0 && screenOffsetY < sizeObstacleLightY) {
                obstacleLightScreenMask[screenOffsetX, screenOffsetY] = 0;
            }
        }

        public bool IsUpdateOffsetObstacleLight(int x, int y) {
            if (x >= 0 && x < sixeObstacleLightX &&
                y >= 0 && y < sizeObstacleLightY) {
                if (obstacleLightScreenMask[x, y] == 1) {
                    return true;
                }
            }
            return false;
        }


        public void EndDrawScreenTile(int layer, int screenOffsetX, int screenOffsetY) {
            if (screenOffsetX >= 0 && screenOffsetX < sixeX &&
                screenOffsetY >= 0 && screenOffsetY < sizeY) {
                screenMask[layer][screenOffsetX, screenOffsetY] = 0;
            }
        }

        public bool IsUpdateOffset(int x, int y) {
            if (x >= 0 && x < sixeX &&
                y >= 0 && y < sizeY) {
                for (int i = 0; i < screenMask.Count; i++) {
                    if (screenMask[i][x, y] == 1) {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsUpdate(int layerId, int x, int y) {
            if (x >= 0 && x < sixeX &&
                y >= 0 && y < sizeY) {
                return screenMask[layerId][x, y] == 1;
            }
            return false;
        }

     

        private bool isApplayChanges = false;

        public void SetApplayChanges(bool b) {
            isApplayChanges = b;
        }

        public bool GetApplayChanges() {
            return isApplayChanges;
        }

        private bool isChangeObstacleLight = false;

        public void SetChangeObstacleLight(bool b) {
            isChangeObstacleLight = b;
        }

        public bool GetChangeObstacleLight() {
            return isChangeObstacleLight;
        }

        private bool isChangeLight = false;

        public void SetChangeLight(bool b) {
            isChangeLight = b;
        }

        public bool GetChangeLight() {
            return isChangeLight;
        }


       public bool TryAddUpdateLightPoint(int worldOffsetX, int worldOffsetY) {
            //LightRenderer.offScreenTiles

            var screenOffsetX = worldOffsetX - startScreenOffset.x;
            var screenOffsetY = worldOffsetY - startScreenOffset.y;

            

            if (screenOffsetX >= -LightRenderer.RaySize && screenOffsetX < sixeX + LightRenderer.RaySize &&
                screenOffsetY >= -LightRenderer.RaySize && screenOffsetY < sizeY + LightRenderer.RaySize) {
                
                if (LightUptateMin.x == -1 && LightUptateMin.y == -1 &&
                    LightUptateMax.x == -1 && LightUptateMax.y == -1) {
                    LightUptateMin.x = worldOffsetX;
                    LightUptateMax.x = worldOffsetX;
                    LightUptateMin.y = worldOffsetY;
                    LightUptateMax.y = worldOffsetY;
                } else {
                    if (LightUptateMin.x > worldOffsetX) {
                        LightUptateMin.x = worldOffsetX;
                    }

                    if (LightUptateMin.y > worldOffsetY) {
                        LightUptateMin.y = worldOffsetY;
                    }

                    if (LightUptateMax.x < worldOffsetX) {
                        LightUptateMax.x = worldOffsetX;
                    }

                    if (LightUptateMax.y < worldOffsetY) {
                        LightUptateMax.y = worldOffsetY;
                    }
                }
                return true;
            }
          
           return false;
        }

       public void EndUpdateLight() {
           LightUptateMin = new Vector2Int(-1, -1);
           LightUptateMax = new Vector2Int(-1, -1);
           SetChangeLight(false);
       } 

        public Vector2Int GetLightUptateWorldMin() {
            return LightUptateMin;
        }

        public Vector2Int GetLightUptateWorldMax() {
            return LightUptateMax;
        }


        public Vector2Int GetLightUptateScreenMin() {
            return new Vector2Int(LightUptateMin.x - startScreenOffset.x, LightUptateMin.y - startScreenOffset.y);
        }

        public Vector2Int GetLightUptateScreenMax() {
            return new Vector2Int(LightUptateMax.x - startScreenOffset.x, LightUptateMax.y - startScreenOffset.y);
        }

        public void RedrawFullScreenLight() {
            //var addSize = 0;
            //startObstacleLightOffset = new Vector2Int(startScreenOffset.x - addSize, startScreenOffset.y - addSize);

            LightUptateMin = new Vector2Int(startScreenOffset.x , startScreenOffset.y );
            //endScreenOffset
            LightUptateMax = new Vector2Int(endScreenOffset.x , endScreenOffset.y );
            SetChangeLight(true);
        }
    }
}

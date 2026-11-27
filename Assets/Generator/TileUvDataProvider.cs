using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UvProvider : IUvProvider {

    protected virtual float GetTileSize() {
        return 16;
    }

    protected virtual float GetPadding() {
        return 2;
    }

    protected virtual int GetFrameCountX() {
        return 16;
    }

    protected virtual int GetFrameCountY() {
        return 16;
    }

    protected Dictionary<string, TileUVCoordinate> spriteAtlasUV = new Dictionary<string, TileUVCoordinate>();
    protected Dictionary<string, Dictionary<long, TileUVCoordinate>> spriteAtlasUV3 = new Dictionary<string, Dictionary<long, TileUVCoordinate>>();
    public TileUVCoordinate uvEmpty { get; protected set; }

    protected long GetUniqueKey(long flameX, long fameY, int height) {
        return 100*(100*flameX + fameY) + height;
    }

    private TileUVCoordinate empty;
    public UvProvider() {
        uvEmpty = new TileUVCoordinate();
        uvEmpty.uvP1 = Vector2.one;
        uvEmpty.uvP2 = Vector2.one;
        uvEmpty.uvP3 = Vector2.one;
        uvEmpty.uvP4 = Vector2.one;

        empty = uvEmpty;
    }

    public virtual void LoadTiles(Sprite sprite, long shiftPaddingX = 1, long shiftPaddingY = 1, int countHeight = 1) {
        var rect = sprite.textureRect;
        LoadTilesCore(sprite.name, rect.xMin, rect.yMax, sprite.texture.width, sprite.texture.height,
            shiftPaddingX, shiftPaddingY, countHeight);
    }

    /// <summary>
    /// Регистрация кадров тайлсета, лежащего в СВОЕЙ текстуре (без Sprite-обёртки).
    /// rectXMin/rectYMax — левый верхний угол области тайлсета в пикселях текстуры;
    /// textureWidth/textureHeight — размеры всей текстуры (для нормализации UV).
    /// Формула кадров та же, что и для атласа (16 px кадр, 2 px паддинг).
    /// </summary>
    public void LoadTiles(string spriteName, float rectXMin, float rectYMax,
        int textureWidth, int textureHeight,
        long shiftPaddingX = 1, long shiftPaddingY = 1, int countHeight = 1) {
        LoadTilesCore(spriteName, rectXMin, rectYMax, textureWidth, textureHeight,
            shiftPaddingX, shiftPaddingY, countHeight);
    }

    protected void LoadTilesCore(string spriteName, float rectXMin, float rectYMax,
        int textureWidth, int textureHeight,
        long shiftPaddingX, long shiftPaddingY, int countHeight) {

        if (textureWidth <= 0 || textureHeight <= 0) {
            Debug.LogError("UvProvider: некорректный размер текстуры для '" + spriteName + "': " +
                           textureWidth + "x" + textureHeight);
            return;
        }

        float shiftX = 0;
        float shiftY = 0;

        for (int x = 0; x < GetFrameCountX(); x++) {
            for (int y = 0; y < GetFrameCountY(); y++) {
                for (int h = 0; h < countHeight; h++) {
                    shiftX = GetTileSize() * x + GetPadding() * Mathf.FloorToInt((float)x / (float)shiftPaddingX);
                    shiftY = GetTileSize() * y + GetPadding() * Mathf.FloorToInt((float)y / (float)shiftPaddingY);

                    Rect newRect = new Rect(
                        rectXMin + shiftX,
                        rectYMax - GetTileSize() - shiftY + h,
                        GetTileSize(),
                        GetTileSize()
                        );

                    var v1 = new Vector2(newRect.x/textureWidth, newRect.y/textureHeight);
                    var v2 = new Vector2((newRect.x + newRect.width)/textureWidth,
                        newRect.y/textureHeight);
                    var v3 = new Vector2((newRect.x + newRect.width)/textureWidth,
                        (newRect.y + newRect.height)/textureHeight);
                    var v4 = new Vector2((newRect.x)/textureWidth,
                        (newRect.y + newRect.height)/textureHeight);
                    TileUVCoordinate uv = new TileUVCoordinate();
                    uv.uvP1 = v1;
                    uv.uvP2 = v2;
                    uv.uvP3 = v3; 
                    uv.uvP4 = v4;
                    uv.rect = newRect;
                    SetUV2(uv, spriteName, x, y, h);
                }
            }
        }  
    }

    protected virtual void SetUV2(TileUVCoordinate uv, string spriteName, long flameX, long fameY, int height) {
        if (!spriteAtlasUV3.ContainsKey(spriteName)) {
            spriteAtlasUV3[spriteName] = new Dictionary<long, TileUVCoordinate>();
        }

       

        spriteAtlasUV3[spriteName][GetUniqueKey(flameX, fameY, height)] = uv;
    }

    public virtual TileUVCoordinate GetUV(string spriteName, long flameX, long fameY, int height = 0) {
        if (spriteName != "empty") {
            try {
              
                return spriteAtlasUV3[spriteName][GetUniqueKey(flameX, fameY, height)];
            }
            catch  {
                Debug.LogError(spriteName + " x = " + flameX + " y = " + fameY + " height = " + height);
                return empty;
            }
           
        } else {
            return empty;
        } 
    }
}

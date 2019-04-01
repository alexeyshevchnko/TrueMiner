using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class LightUvProvider : ILightUvProvider {

    public Texture2D texture;
    private Sprite sprite;
    private int size;
    Dictionary<int,TileUVCoordinate> cache = new Dictionary<int, TileUVCoordinate>();


    public TileUVCoordinate uvEmpty { get; private set; }

    public void LoadTiles(Texture2D texture) {
        uvEmpty = new TileUVCoordinate();
        uvEmpty.uvP1 = Vector2.one;
        uvEmpty.uvP2 = Vector2.one;
        uvEmpty.uvP3 = Vector2.one;
        uvEmpty.uvP4 = Vector2.one;

        size = Settings.LIGHT_SIZE;
        this.texture = texture;
        //делаем спрайт
        sprite = Sprite.Create(texture, new Rect(
              0,
              0,
              size,
              size),
              new Vector2(0.5f, 0.5f),
              1
              );

        sprite.name = "LightPalitra";

        for (int i = 0; i < 10; i++) {
            var index1 = getIndex(i, i);
            cache[index1] = uvEmpty;
     
        }
       
    }

    public virtual TileUVCoordinate GetUV(string spriteName, long flameX, long fameY, int height = 0) {
        if (!ContainsKey(flameX, fameY)) {

            var rect = sprite.textureRect;

            Rect newRect = new Rect(
                rect.xMin + (flameX),
                rect.yMin + (fameY),
                1,
                1
                );

            var v1 = new Vector2(newRect.x / texture.width, newRect.y / texture.height);
            var v2 = new Vector2((newRect.x + newRect.width) / texture.width, newRect.y / texture.height);
            var v3 = new Vector2((newRect.x + newRect.width) / texture.width, (newRect.y + newRect.height) / texture.height);
            var v4 = new Vector2((newRect.x) / texture.width, (newRect.y + newRect.height) / texture.height);

            TileUVCoordinate uv = new TileUVCoordinate();
            uv.uvP1 = v1;
            uv.uvP2 = v2;
            uv.uvP3 = v3;
            uv.uvP4 = v4;
            AddCacheValue(flameX, fameY, uv);
            return uv;
        } else {
            return GetCacheValue(flameX, fameY); 
        }
    }

    public void LateUpdate() {
    }


    bool ContainsKey(long flameX, long fameY) {
        var index1 = getIndex(flameX, fameY);

        if (!cache.ContainsKey(index1))
            return false;

        return cache[index1] != null;
    }

    TileUVCoordinate GetCacheValue(long flameX, long fameY) {
        var index1 = getIndex(flameX, fameY);
        return cache[index1];
    }

    void AddCacheValue(long flameX, long fameY, TileUVCoordinate val) {
        var index1 = getIndex(flameX, fameY);

        if (!cache.ContainsKey(index1)) {
            cache.Add(index1, val);
            return;
        }
        cache[index1] = val;
    }

    int getIndex(long i, long j) {
        return (int)(i * 1000 + j);
    }


} 
 
using UnityEngine;
using System.Collections;

public class SpriteManager : ISpriteManager {
    private IInfinityTileMap provider;
    public void SetTileMap(IInfinityTileMap provider) {
        this.provider = provider;
    }
    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }
    public Sprite CreateSprite(string tileName, int flameX = 0, int fameY = 0, int deltaHeight = 0) {
        var rect = provider.GetRect(tileName, flameX, fameY, deltaHeight);
        return Sprite.Create(provider.GetTexture(), rect, new Vector2(0.5f, 0.5f), Settings.TILE_SIZE / TileDataProvider.TileSize);
    }

    /*
    public GameObject CreateBlock(string tileCollection) {
        GameObject go = new GameObject("block");
        //go.transform.parent = transform;
        //go.transform.localPosition = Vector3.zero; 
        var spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateSprite(tileCollection, 9, 3);
        return go;
    }*/
}

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

        // Выпавшие блоки (дроп) рисуются спрайтом, а не мешом слоя, поэтому текстуру
        // берём из SpriteData напрямую. Координаты GetRect вычисляются под отдельную
        // текстуру тайлсета (у слоя они теперь кусаются из TerrariaTiles_Sprites),
        // значит и текстура должна быть ТА ЖЕ: прямоугольник отдельного тайлсета,
        // наложенный на общий атлас, попадает в чужую/пустую область и спрайт прозрачен.
        Texture2D texture = null;
        SpriteData data;
        if (SpriteData.Sprites.TryGetValue(tileName, out data)) {
            texture = TilesetTextures.Get(data);
        }
        if (texture == null) {
            // Запись без собственной текстуры (старые тайлсеты) — прежний общий атлас слоя.
            texture = provider.GetTexture();
        }

        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), Settings.TILE_SIZE / TileDataProvider.TileSize);
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

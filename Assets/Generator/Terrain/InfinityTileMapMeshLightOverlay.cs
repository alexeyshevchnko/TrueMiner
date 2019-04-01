using UnityEngine;
using System.Collections;

public class InfinityTileMapMeshLightOverlay : InfinityTileMapMesh {
    public Texture2D texture;

    public override void LoadMesh(GameObject go, int countHorizontal, int countVertical, float tileSize, LayerData layer) {

        base.LoadMesh(go, countHorizontal, countVertical, tileSize, layer);
        go.GetComponent<MeshRenderer>().material.mainTexture = texture;
    }


    protected override void LoadSprites(string texrureName) {
        uvProvider = new LightUvProvider();

        //создаём текстуру
        int size = Settings.LIGHT_SIZE;
        int size12 = (int)((float)size / 2);

        texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        for (int x = 0; x <= size; x++) {
            for (int y = 0; y <= size; y++) {
                texture.SetPixel(x, y, new Color32(0, 0, 0, 0));
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        CreateCircle(new Vector2Int(size12, size12), size12);

        //делаем спрайт
        Sprite sprite = Sprite.Create(texture, new Rect(
              0,
              0,
              size,
              size),
              new Vector2(0.5f, 0.5f),
              1
              );

        sprite.name = texrureName;

        ((UvProvider)uvProvider).LoadTiles(sprite);

    }

    void CreateCircle(Vector2Int center, int r) {
        for (int x = center.x - r; x <= center.x + r; x++) {
            for (int y = center.y - r; y <= center.y + r; y++) {
                float alpha = Mathf.Lerp(0, 1, Vector2.Distance(center.GetVector2(), new Vector2(x, y)) / (float)r);
                texture.SetPixel(x, y, new Color(137, 137, 137, alpha));
            }
        }

        texture.Apply();
    }

   
}
using UnityEngine;
using System.Collections;

public interface IInfinityTileMap : IBaseInfinityTileMap {
    void DrawMesh(Vector3 pos, Camera cam);
    LayerData layerData { get; }
    void ChangeTile(int x, int y, string tileName, int flameX = 0, int fameY = 0, int deltaHeight = 0);
    void ChangeColor(int x, int y, Color32 color);
    void ShiftCells(int deltaX, int deltaY);
    Texture2D GetTexture();
    Rect GetRect(string tileName, int flameX = 0, int fameY = 0, int deltaHeight = 0);
}

public interface IInfinityTileMapLight : IBaseInfinityTileMap {
    LayerData layerData { get; }
}

public interface IBaseInfinityTileMap {
    LayerData layerData { get; }
    void LoadMesh(GameObject go, int countHorizontal, int countVertical, float tileSize, LayerData layer);
    
    void RenderMesh();

    void SetLayer(int layerId);
}

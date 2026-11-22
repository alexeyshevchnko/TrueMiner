using UnityEngine;
using System.Collections;

public interface IInfinityTileMap : IBaseInfinityTileMap {
    void DrawMesh(Vector3 pos, Camera cam);
    // layerData унаследован от IBaseInfinityTileMap (повторное объявление скрывало
    // базовое свойство — CS0108; контракт не изменился).
    void ChangeTile(int x, int y, string tileName, int flameX = 0, int fameY = 0, int deltaHeight = 0);
    void ChangeColor(int x, int y, Color32 color);
    void ShiftCells(int deltaX, int deltaY);
    Texture2D GetTexture();
    Rect GetRect(string tileName, int flameX = 0, int fameY = 0, int deltaHeight = 0);
}

public interface IInfinityTileMapLight : IBaseInfinityTileMap {
    // layerData унаследован от IBaseInfinityTileMap (см. комментарий выше).
}

public interface IBaseInfinityTileMap {
    LayerData layerData { get; }
    void LoadMesh(GameObject go, int countHorizontal, int countVertical, float tileSize, LayerData layer);
    
    void RenderMesh();

    void SetLayer(int layerId);
}

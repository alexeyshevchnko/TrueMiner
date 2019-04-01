using UnityEngine;
using System.Collections;

public class EmptyTileMesh : IInfinityTileMap {
    public void DrawMesh(Vector3 pos, Camera cam) {
        
    }

    public LayerData layerData {
        get { return LayerData.LightLayer; }
    }

    public void ChangeTile(int x, int y, string tileName, int flameX = 0, int fameY = 0, int deltaHeight = 0) {
        
    }

    public void ChangeColor(int x, int y, Color32 color) {
        
    }

    public void ShiftCells(int deltaX, int deltaY) {
        
    }

    public Texture2D GetTexture() {
        throw new System.NotImplementedException();
    }

    public Rect GetRect(string tileName, int flameX = 0, int fameY = 0, int deltaHeight = 0) {
        throw new System.NotImplementedException();
    }

    LayerData IBaseInfinityTileMap.layerData {
        get { throw new System.NotImplementedException(); }
    }

    public void LoadMesh(GameObject go, int countHorizontal, int countVertical, float tileSize, LayerData layer) {
        
    }

    public void RenderMesh() {
     
    }

    public void SetLayer(int layerId) {
       
    }
}

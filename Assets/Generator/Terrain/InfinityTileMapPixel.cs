using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InfinityTileMapPixel : IInfinityTileMap {

    [IoC.Inject]
    public ILightUvProvider LightUvProvider { set; protected get; }

    protected TileUVIndex[,] tileUVIndexs;

    protected MeshFilter meshFilter;
    
    public LayerData layerData { get; private set; }

    protected Mesh mesh;
    protected bool isRenderMesh = false;

    protected GameObject gameObject;

    private int sizeX;
    private int sizeY;
    private float TileSize;

    protected Material material;

    public virtual void LoadMesh(GameObject go, int x, int y, float tileSize, LayerData layer) {
        TileSize = tileSize;
        this.layerData = layer;
        gameObject = go;

        sizeX = y;
        sizeY = x;

        mesh = CreateMesh(sizeY, sizeX, tileSize);
        texture = CreateTexture(sizeX, sizeY);
        material = Resources.Load<Material>(layer.Material);
        material.mainTexture = texture;

        LoadSprites(layer.TexrureName);

        meshFilter = (MeshFilter)go.AddComponent(typeof(MeshFilter));
        MeshRenderer renderer = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = material;
        renderer.material.mainTexture = texture;
        meshFilter.mesh = mesh;
    }

    public void SetLayer(int layerId) {
        var renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null) {
            renderer.sortingOrder = layerId;
        }
    }

    public void DrawMesh(Vector3 pos, Camera cam) {
    }


    private Texture2D texture;

    public Texture2D GetTexture() {
        return texture;
    }

    protected virtual void LoadSprites(string texrureName) {

        //создаём текстуру
        int size = Settings.LIGHT_SIZE;
        int size12 = (int)((float)size / 2);

        Texture2D texture2 = new Texture2D(size, size, TextureFormat.RGBA32, false);

        for (int x = 0; x <= size; x++) {
            for (int y = 0; y <= size; y++) {
                texture2.SetPixel(x, y, new Color32(255, 255, 255, (byte)x));
            }
        }
        texture2.filterMode = FilterMode.Point;
        texture2.Apply();

        LightUvProvider.LoadTiles(texture2);
    }

    Texture2D CreateTexture(int sizeX, int sizeY) {
        Texture2D texture = new Texture2D(sizeX, sizeY, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        return texture;
    }

    private Mesh CreateMesh(int x, int y, float tileSize) {
        var mesh = new Mesh();
        var vertices = new Vector3[4];
        var width = (x*tileSize);
        var height = (y*tileSize);
        float mm = tileSize/2f;
        vertices[0] = new Vector3(-mm, -mm, 0);
        vertices[1] = new Vector3(width - mm, -mm, 0);
        vertices[2] = new Vector3(-mm, height - mm, 0);
        vertices[3] = new Vector3(width - mm, height - mm, 0);

        mesh.vertices = vertices;

        int[] tri = new int[6];

        tri[0] = 0;
        tri[1] = 2;
        tri[2] = 1;

        tri[3] = 2;
        tri[4] = 3;
        tri[5] = 1;

        mesh.triangles = tri;

        Vector3[] normals = new Vector3[4];

        normals[0] = -Vector3.forward;
        normals[1] = -Vector3.forward;
        normals[2] = -Vector3.forward;
        normals[3] = -Vector3.forward;

        mesh.normals = normals;

        Vector2[] uv = new Vector2[4];

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 0);
        uv[3] = new Vector2(1, 1);

        mesh.uv = uv;

        mesh.RecalculateNormals();
        mesh.MarkDynamic();
        return mesh;
    }

    public virtual void ChangeTile(int x, int y, string tileName, int flameX = 0, int fameY = 0, int deltaHeight = 0) {
        texture.SetPixel(x, y, new Color32(255, 255, 255, (byte)(flameX)));
        isRenderMesh = true;
    }

    public void ChangeColor(int x, int y, Color32 color) {
        
    }

    private bool b = false;
    //поменять ячейки местами

    private Color32 cacheIndexFrom;
    private Color32 cacheIndexTo;

    //сдвинуть ячейки
    public void ShiftCells(int deltaY, int deltaX) {
        if (deltaY >= sizeY || deltaX >= sizeX)
            return;
        
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
                    var tmp = texture.GetPixel(fromX, fromY);
                    texture.SetPixel(toX, toY, tmp);
                   // texture.SetPixel(fromX, fromY, tmp);
                   // meshs[toX, toY] = meshs[fromX, fromY];
                   // meshs[fromX, fromY] = tmp;
                }
            }
        }
        
        isRenderMesh = true;
    }

    public Rect GetRect(string tileName, int flameX = 0, int fameY = 0, int deltaHeight = 0) {
        var uv = LightUvProvider.GetUV(tileName, flameX, fameY, deltaHeight);
        return uv.rect;
    }

    public void RenderMesh() {
        if (isRenderMesh && texture != null) {

            texture.Apply();

            isRenderMesh = false;
            //  chackUv = null;
        }
    }


}
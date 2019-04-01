using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InfinityTileMapSets : IInfinityTileMap {
    protected Vector2[] setUv = new Vector2[4];

    protected Mesh[,] meshs;
    protected bool[,] isActiveSet;

    protected IBaseUVProvider uvProvider;
    public LayerData layerData { get; private set; }

    protected Mesh mesh;
    protected Vector2[] chackUv;

    protected GameObject gameObject;

    private int sizeX;
    private int sizeY;
    private float TileSize;

    protected Material material;

    public virtual void LoadMesh(GameObject go, int countHorizontal, int countVertical, float tileSize, LayerData layer) {
        TileSize = tileSize;
        this.layerData = layer;
        gameObject = go;

        sizeX = countVertical;
        sizeY = countHorizontal;

        CreateSets(countVertical , countHorizontal, tileSize);

        var texture = Resources.Load<Texture>(layer.TexrureName);
        material = Resources.Load<Material>(layer.Material);
        material.mainTexture = texture;

        LoadSprites(layer.TexrureName);
       // chackUv = new Vector2[mesh.uv.Length];
    }

    public void SetLayer(int layerId) {
        var renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null) {
            renderer.sortingOrder = layerId;
        }
    }

    public void DrawMesh(Vector3 pos, Camera cam) {
     //   if (layerData == LayerData.TerrainLayer)
     //       Graphics.DrawMesh(mesh, pos, Quaternion.identity, material, 1, cam);

        for (int x = 0; x < sizeX; x++) {
            for (int y = 0; y < sizeY; y++) {
               if (isActiveSet[x, y]) {
                    var mesh = meshs[x, y];
                    var newpos = new Vector3(pos.x + y*TileSize, pos.y + x*TileSize, pos.z);
                    Graphics.DrawMesh(mesh, newpos, Quaternion.identity, material, 20, cam);
                }
            }
        }
    }


    private Texture2D texture;

    public Texture2D GetTexture() {
        return texture;
    }

    protected virtual void LoadSprites(string texrureName) {
        uvProvider = new UvProvider();

        Sprite[] sprites = Resources.LoadAll<Sprite>(texrureName);
        foreach (var sprite in sprites) {
            Debug.Log("load tile collection = " + sprite.name);
            if (SpriteData.Sprites.ContainsKey(sprite.name)) {
                var data = SpriteData.Sprites[sprite.name];
                ((UvProvider)uvProvider).LoadTiles(sprite, data.ShiftPaddingX, data.ShiftPaddingY, data.CountHeight);
            }
        }

        texture = sprites[0].texture;
    }

    void CreateSets(int countHorizontal, int countVertical, float tileSize) {
        var tileSize2 = tileSize / 2;
        var s = Mathf.Max(countVertical, countHorizontal) + 10;
        meshs = new Mesh[s, s];
        isActiveSet = new bool[s,s];
        for (int x = 0; x < s; x++) {
            for (int y = 0; y < s; y++) {
                Mesh m = new Mesh();
                m.name = "ScriptedMesh";

                List<Vector3> vertices = new List<Vector3>();
                List<int> triangles = new List<int>();
                List<Vector2> uv = new List<Vector2>();

                var x1 = - tileSize2;
                var x2 = tileSize2;

                var y1 = - tileSize2;
                var y2 = tileSize2;


                vertices.Add(new Vector3(x1, y1));
                int idP0 = vertices.Count - 1;

                vertices.Add(new Vector3(x2, y1));
                int idP1 = vertices.Count - 1;

                vertices.Add(new Vector3(x2, y2));
                int idP2 = vertices.Count - 1;

                vertices.Add(new Vector3(x1, y2));
                int idP3 = vertices.Count - 1;

                triangles.Add(idP0); //0
                triangles.Add(idP1); //1
                triangles.Add(idP2); //2
                triangles.Add(idP2); //2
                triangles.Add(idP3); //3
                triangles.Add(idP0); //0

                var v1 = Vector2.zero;
                var v2 = Vector2.zero;
                var v3 = Vector2.zero;
                var v4 = Vector2.zero;

                uv.Add(v1);
                uv.Add(v2);
                uv.Add(v3);
                uv.Add(v4);

                m.vertices = vertices.ToArray();
                m.triangles = triangles.ToArray();

                m.uv = uv.ToArray();
                m.RecalculateNormals();
                m.MarkDynamic();

                meshs[x, y] = m;
            }
        }
    }

    public virtual void ChangeTile(int x, int y, string tileName, int flameX = 0, int fameY = 0, int deltaHeight = 0) {
        var uv = ((IUvProvider)uvProvider).GetUV(tileName, flameX, fameY, deltaHeight);

        setUv[0] = uv.uvP1;
        setUv[1] = uv.uvP2;
        setUv[2] = uv.uvP3;
        setUv[3] = uv.uvP4;

        var mesh = meshs[x, y];
        mesh.uv = setUv.ToArray();


        isActiveSet[x, y] = uv != ((IUvProvider) uvProvider).uvEmpty;

    }

    public void ChangeColor(int x, int y, Color32 color) {
        
    }

    //поменять ячейки местами
    private Vector2 cacheUvP1;
    private Vector2 cacheUvP2;
    private Vector2 cacheUvP3;
    private Vector2 cacheUvP4;
    private int cacheIndexFrom;
    private int cacheIndexTo;

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
                    var tmp = meshs[toX, toY];
                    meshs[toX, toY] = meshs[fromX, fromY];
                    meshs[fromX, fromY] = tmp;
                    var tmp2 = isActiveSet[toX, toY];
                    isActiveSet[toX, toY] = isActiveSet[fromX, fromY];
                    isActiveSet[fromX, fromY] = tmp2;
                } 
            }
        }
    }

    public Rect GetRect(string tileName, int flameX = 0, int fameY = 0, int deltaHeight = 0) {
        var uv = ((IUvProvider)uvProvider).GetUV(tileName, flameX, fameY, deltaHeight);
        return uv.rect;
    }

    public void RenderMesh() {

    }

}

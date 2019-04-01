
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class InfinityTileMapMesh : IInfinityTileMap {
    protected TileUVIndex[,] tileUVIndexs;

    protected MeshFilter meshFilter;
    protected IBaseUVProvider uvProvider;
    public LayerData layerData { get; private set; }

    protected Mesh mesh;
    protected Mesh mesh2;
  //  protected Mesh mesh3;

    protected Vector2[] chackUv;
    protected Vector3[] chackVertices;
    protected Vector3[] normalVertices;
    protected Color32[] chackColors;

    

    protected bool isChangeColors = false;
    protected bool isChangeVertices = false;
    protected bool isChangeUv = false;

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


        mesh = CreateMesh(countHorizontal, countVertical, tileSize);
        mesh2 = CreateMesh(countHorizontal, countVertical, tileSize);

        var texture = Resources.Load<Texture>(layer.TexrureName);
        material = Resources.Load<Material>(layer.Material);
        material.mainTexture = texture;

        LoadSprites(layer.TexrureName);
        chackUv = new Vector2[mesh.uv.Length];
       
        


        meshFilter = (MeshFilter)go.AddComponent(typeof(MeshFilter));
        MeshRenderer renderer = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = material;
        renderer.material.mainTexture = texture;
        meshFilter.mesh = mesh; 
       
        //mesh.
        
     /*   
     //   if (layer == LayerData.TerrainLayer) {
            Color[] colors = new Color[mesh.vertices.Length];            
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = Color.Lerp(Color.black, Color.white, UnityEngine.Random.Range(0, 10));
            }
            mesh.colors = colors;
     //   }
   */
    }

    public void SetLayer(int layerId) {
        var renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null) {
            renderer.sortingOrder = layerId;
        }
        //gameObject.GetComponent<Renderer>().sortingOrder = layerId;
    }

    public void DrawMesh(Vector3 pos,Camera cam) {
       // Graphics.DrawMeshNow(mesh, pos, Quaternion.identity);//layerData.Depth);
      //  if (layerData == LayerData.TerrainLayer)
            
      //  Graphics.DrawMesh(mesh, pos, Quaternion.identity, material, 1, cam);//layerData.Depth);
       // mesh.Optimize(); 
    }


    private Texture2D texture;

    public Texture2D GetTexture() {
        return texture;
    }

    protected  virtual void LoadSprites(string texrureName) {
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

    Mesh CreateMesh(int countHorizontal, int countVertical, float tileSize) {
        Mesh m = new Mesh();
        m.name = "ScriptedMesh";

        float deltaX = 0;
        float deltaY = 0;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();
        tileUVIndexs = new TileUVIndex[countVertical, countHorizontal];
        var tileSize2 = tileSize / 2;
        for (int x = 0; x < countVertical; x++) {
            for (int y = 0; y < countHorizontal; y++) {
                tileUVIndexs[x,y] = new TileUVIndex();

                var x1 = deltaX - tileSize2;
                var x2 = deltaX + tileSize2;

                var y1 = deltaY - tileSize2;
                var y2 = deltaY + tileSize2;


                vertices.Add(new Vector3(x1, y1));
                tileUVIndexs[x, y].verticesIndex = vertices.Count - 1;
               // normalVertices[vertices.Count - 1] = new Vector3(x1, y1);
               // chackVertices[vertices.Count - 1] = new Vector3(x1, y1);
                int idP0 = vertices.Count - 1;

                vertices.Add(new Vector3(x2, y1));
              //  normalVertices[vertices.Count - 1] = new Vector3(x2, y1);
              //  chackVertices[vertices.Count - 1] = new Vector3(x2, y1);
                int idP1 = vertices.Count - 1;

                vertices.Add(new Vector3(x2, y2));
              //  normalVertices[vertices.Count - 1] = new Vector3(x2, y2);
              //  chackVertices[vertices.Count - 1] = new Vector3(x2, y2);
                int idP2 = vertices.Count - 1;

                vertices.Add(new Vector3(x1, y2));
              //  normalVertices[vertices.Count - 1] = new Vector3(x1, y2);
              //  chackVertices[vertices.Count - 1] = new Vector3(x1, y2);
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
                tileUVIndexs[x,y].uvIndex1 = uv.Count - 1;
                uv.Add(v2);
                tileUVIndexs[x,y].uvIndex2 = uv.Count - 1;
                uv.Add(v3);
                tileUVIndexs[x,y].uvIndex3 = uv.Count - 1;
                uv.Add(v4);
                tileUVIndexs[x,y].uvIndex4 = uv.Count - 1;

                deltaX += tileSize;
            }

            deltaX = 0;
            deltaY += tileSize;
        }
        normalVertices = vertices.ToArray();
        chackVertices = vertices.ToArray();
        chackColors = new Color32[normalVertices.Length];

        for (int i = 0; i < chackColors.Length; i++) {
            chackColors[i] = Color.white;
        }

        m.vertices = vertices.ToArray();
        m.triangles = triangles.ToArray();

        m.uv = uv.ToArray();
        m.RecalculateNormals();
        m.MarkDynamic();
        return m;
    } 

    public virtual void ChangeTile(int x, int y, string tileName, int flameX = 0, int fameY = 0, int deltaHeight = 0) {
        var uv = ((IUvProvider) uvProvider).GetUV(tileName, flameX, fameY,deltaHeight);// 0);//
      //  var uv = ((IUvProvider) uvProvider).uvEmpty;
        int index = tileUVIndexs[x,y].uvIndex1;
        int index2 = tileUVIndexs[x, y].verticesIndex;
        chackUv[index] = uv.uvP1;
        chackUv[++index] = uv.uvP2;
        chackUv[++index] = uv.uvP3;
        chackUv[++index] = uv.uvP4;
        /*
        try {

            if (layerData != LayerData.ObstacleLightLayer &&
                layerData != LayerData.LightLayer &&
                LightRenderer.color[y + LightRenderer.RaySize, x + LightRenderer.RaySize].A == 0) {
                
            }

        }
        catch  {
            Debug.LogErrorFormat("sixeX = {0}, sixeY = {1}, x = {2}, y = {3}", LightRenderer.color.GetLength(0), LightRenderer.color.GetLength(1),y,x);
            
        }*/
        /*
        bool b = layerData != LayerData.ObstacleLightLayer &&
                 layerData != LayerData.LightLayer &&
                 LightRenderer.color[y + LightRenderer.RaySize, x + LightRenderer.RaySize].A == 0;
          */ 
        if (uv == ((IUvProvider)uvProvider).uvEmpty /*|| b*/) {
            chackVertices[index2] = Vector3.zero;
            chackVertices[++index2] = Vector3.zero;
            chackVertices[++index2] = Vector3.zero;
            chackVertices[++index2] = Vector3.zero;
        } else {
            chackVertices[index2] = normalVertices[index2];
            chackVertices[index2+1] = normalVertices[index2+1];
            chackVertices[index2+2] = normalVertices[index2+2];
            chackVertices[index2+3] = normalVertices[index2+3];

            var color =
                layerData != LayerData.ObstacleLightLayer &&
                layerData != LayerData.LightLayer
                    ? LightRenderer.color[y + LightRenderer.RaySize, x + LightRenderer.RaySize]
                    : ColorByte.white;
            float val = 1;//color.A/255f;
            Color c = new Color(val, val, val, 1);
                //UnityEngine.Random.Range(-10, 10) > 0 ? Color.white : Color.black;
            chackColors[index2] = c;
            chackColors[index2 + 1] = c;
            chackColors[index2 + 2] = c;
            chackColors[index2 + 3] = c;
        }

        

        isChangeUv = true;
        isChangeVertices = true;
        isChangeColors = true;
    }

    public void ChangeColor(int x, int y, Color32 color) {
        int index = tileUVIndexs[x, y].verticesIndex;

        if (chackUv[tileUVIndexs[x, y].uvIndex1].x != 1) {
            chackColors[index] = color;
            chackColors[index + 1] = color;
            chackColors[index + 2] = color;
            chackColors[index + 3] = color;

            isChangeColors = true;
        }
    }

    //поменять ячейки местами
    private Vector2 cacheUvP1;
    private Vector2 cacheUvP2;
    private Vector2 cacheUvP3;
    private Vector2 cacheUvP4;
    private int cacheIndexFrom;
    private int cacheIndexTo; 
    private int cacheIndexVerticesFrom;
    private int cacheIndexVerticesTo;
    private Vector3 V3zero = Vector3.zero;
    //сдвинуть ячейки
    public void ShiftCells(int deltaX, int deltaY) {
       if (deltaY >= sizeY || deltaX >= sizeX)
            return;
        /*
        if (deltaY != 0 ) {
            var lineSizeY = (sizeY) * 4;

            var abs = Mathf.Abs(deltaY); 

            if (deltaY > 0) {
                Array.Copy(chackUv, lineSizeY * abs, chackUv, 0, chackUv.Length - lineSizeY * abs);
                Array.Copy(normalVertices, lineSizeY * abs, chackVertices, 0, chackVertices.Length - lineSizeY * abs); 
            } else {
                Array.Copy(chackUv, 0, chackUv, lineSizeY * abs, chackUv.Length - lineSizeY * abs);
                Array.Copy(normalVertices, 0, chackVertices, lineSizeY * abs, chackVertices.Length - lineSizeY * abs);
            }

        }*/
        

        /*
        if (deltaX != 0 ) {
            var lineSizeY = (sizeY) * 4;
            var abs = Mathf.Abs(deltaX);

            for (int offset = 0; offset < sizeX; offset++) {
                //0 lineSizeY ->    1 lineSizeY
                //1 lineSizeY ->    2 lineSizeY
                //2 lineSizeY ->    3 lineSizeY
                if (deltaX < 0) {
                    var d = offset*lineSizeY;
                    Array.Copy(
                        chackUv, d,
                        chackUv, d + (4*abs),
                        lineSizeY - (4 * abs));
                    Array.Copy(
                        normalVertices, d,
                        chackVertices, d + (4 * abs),
                        lineSizeY - (4 * abs));
                } else {
                    var d = offset * lineSizeY;
                    Array.Copy( 
                        chackUv, d + (4 * abs),
                        chackUv, d,
                        lineSizeY - (4 * abs));
                    Array.Copy(
                       normalVertices, d + (4 * abs),
                       chackVertices, d,
                       lineSizeY - (4 * abs));
                }
            }


        }else
        */
        
        if (deltaY != 0 || deltaX != 0) {
            var startX = deltaY >= 0 ? 0 : sizeX - 1;
            var incX = deltaY >= 0 ? 1 : -1;
            var startY = deltaX >= 0 ? 0 : sizeY - 1;
            var incY = deltaX >= 0 ? 1 : -1;

            for (int x = startX; x >= 0 && x < sizeX; x += incX) {
                for (int y = startY; y >= 0 && y < sizeY; y += incY) {

                    int fromX = x + deltaY;
                    var fromY = y + deltaX;
                    int toX = x;
                    int toY = y;
                    if (fromX < sizeX && fromX >= 0) {
                        if (fromY < sizeY && fromY >= 0) {

                            //from
                            cacheIndexVerticesFrom = tileUVIndexs[fromX, fromY].verticesIndex;
                            //to
                            cacheIndexVerticesTo = tileUVIndexs[toX, toY].verticesIndex;

                            //from
                            cacheIndexFrom = tileUVIndexs[fromX, fromY].uvIndex1;
                            //to
                            cacheIndexTo = tileUVIndexs[toX, toY].uvIndex1;
                           
                            

                            chackUv[cacheIndexTo] = chackUv[cacheIndexFrom];
                            chackUv[++cacheIndexTo] = chackUv[++cacheIndexFrom];
                            chackUv[++cacheIndexTo] = chackUv[++cacheIndexFrom];
                            chackUv[++cacheIndexTo] = chackUv[++cacheIndexFrom];

                            /*
                            bool b = layerData != LayerData.ObstacleLightLayer &&
                                     layerData != LayerData.LightLayer &&
                                     LightRenderer.color[y + LightRenderer.RaySize, x + LightRenderer.RaySize].A == 0;
                            */
                            if (chackUv[tileUVIndexs[toX, toY].uvIndex1].x == 1 
                               /*|| b*/) {

                                chackVertices[cacheIndexVerticesTo] = V3zero;
                                chackVertices[cacheIndexVerticesTo + 1] = V3zero;
                                chackVertices[cacheIndexVerticesTo + 2] = V3zero;
                                chackVertices[cacheIndexVerticesTo + 3] = V3zero;
                               
                            } else {
                                chackVertices[cacheIndexVerticesTo] = normalVertices[cacheIndexVerticesTo];
                                chackVertices[cacheIndexVerticesTo + 1] = normalVertices[cacheIndexVerticesTo + 1];
                                chackVertices[cacheIndexVerticesTo + 2] = normalVertices[cacheIndexVerticesTo + 2];
                                chackVertices[cacheIndexVerticesTo + 3] = normalVertices[cacheIndexVerticesTo + 3]; 
                            }

                            chackColors[cacheIndexVerticesTo] = chackColors[cacheIndexVerticesFrom];
                            chackColors[cacheIndexVerticesTo + 1] = chackColors[cacheIndexVerticesFrom + 1];
                            chackColors[cacheIndexVerticesTo + 2] = chackColors[cacheIndexVerticesFrom + 2];
                            chackColors[cacheIndexVerticesTo + 3] = chackColors[cacheIndexVerticesFrom + 3];
                            
                        }
                    }
                }
            }
        }
        

        
        isChangeUv = true;
        isChangeVertices = true;
        isChangeColors = true;
        //chackUv.SetValue
    }

    public Rect GetRect(string tileName, int flameX = 0, int fameY = 0, int deltaHeight = 0) {
        var uv = ((IUvProvider)uvProvider).GetUV(tileName, flameX, fameY, deltaHeight);
        return uv.rect;
    }

    private bool ping;
    public void RenderMesh() {
        var changeMesh = ping ? mesh : mesh2;
        bool isChange = false;
        if (isChangeUv) {
              mesh.MarkDynamic();
            changeMesh.uv = chackUv;
            isChangeUv = false;
            //   mesh.Optimize(); 
            isChange = true;
        }

         
        if (isChangeVertices) {
              mesh.MarkDynamic();
            changeMesh.vertices = chackVertices;
            isChangeVertices = false;
            //  mesh.Optimize(); 
            isChange = true;
        }

        if (isChangeColors) {
              mesh.MarkDynamic();
            changeMesh.colors32 = chackColors;
            isChangeColors = false;
            //  mesh.Optimize();
            isChange = true;
        }

        if (isChange) { 
         //   changeMesh.UploadMeshData(false);
            meshFilter.sharedMesh = changeMesh;
          //  ping = !ping;
        }
    }
    
  
}

public class TileUVIndex {
    public int uvIndex1;
    public int uvIndex2;
    public int uvIndex3;
    public int uvIndex4;
    public int verticesIndex;
    public bool isUvEmpty;
}


public class TileUVCoordinate {
    public Vector2 uvP1;
    public Vector2 uvP2;
    public Vector2 uvP3;
    public Vector2 uvP4;

    public Rect rect;

    public void print() {
        Debug.LogError(string.Format("uvP1 = {0} uvP2 = {1}, uvP3 = {2}, uvP4 = {3}", uvP1, uvP2, uvP3, uvP4));
    }
}
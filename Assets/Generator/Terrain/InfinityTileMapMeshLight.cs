using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InfinityTileMapMeshLight : InfinityTileMapMesh, IInfinityTileMapLight {
    public Texture2D texture;

    [IoC.Inject]
    public ILightUvProvider LightUvProvider { set; protected get; }

    public override void LoadMesh(GameObject go, int countHorizontal, int countVertical, float tileSize, LayerData layer) {
      
        base.LoadMesh(go, countHorizontal, countVertical, tileSize, layer);
        material.mainTexture = texture; 

        var meshRenderer = go.GetComponent<MeshRenderer>();
        if (meshRenderer != null) {
            meshRenderer.material = material;
        }
        // go.GetComponent<MeshRenderer>().material.mainTexture = texture;
    } 

    protected override void LoadSprites(string texrureName) {
        uvProvider = LightUvProvider;
        
        //создаём текстуру
        int size = Settings.LIGHT_SIZE;
        int size12 = (int)((float)size / 2);
        
        texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
         
        for (int x = 0; x <= size; x++) {
            for (int y = 0; y <= size; y++) {
                texture.SetPixel(x, y, new Color32(0,0, 0, (byte)x));
            }
        } 
        texture.filterMode = FilterMode.Point;
        texture.Apply();

        ((ILightUvProvider)uvProvider).LoadTiles(texture);
    }

    public override void ChangeTile(int x, int y, string tileName, int flameX = 0, int fameY = 0, int deltaHeight = 0) {
        var uv = ((ILightUvProvider)uvProvider).GetUV(tileName, flameX, fameY, deltaHeight);

        int index = tileUVIndexs[x, y].uvIndex1;
        int index2 = tileUVIndexs[x, y].verticesIndex;

        chackUv[index] = uv.uvP1;
        chackUv[++index] = uv.uvP2;
        chackUv[++index] = uv.uvP3;
        chackUv[++index] = uv.uvP4;





         
        if (uv == ((ILightUvProvider)uvProvider).uvEmpty) {
            chackVertices[index2] = Vector3.zero;
            chackVertices[++index2] = Vector3.zero;
            chackVertices[++index2] = Vector3.zero;
            chackVertices[++index2] = Vector3.zero;
        } else {
            chackVertices[index2] = normalVertices[index2];
            chackVertices[index2 + 1] = normalVertices[index2 + 1];
            chackVertices[index2 + 2] = normalVertices[index2 + 2];
            chackVertices[index2 + 3] = normalVertices[index2 + 3];
        }


        isChangeUv = true;
        isChangeVertices = true;
        isChangeColors = true;
    }

}

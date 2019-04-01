using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using IoC;

using Managers;
using trasharia;

//using UnityEditor;

//using UnityEditor;

//using UnityEditor;

public class TileRenderer : MonoBehaviour,IoC.IInitialize {

    [IoC.Inject]
    public ICollision Collision { set; protected get; }
    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }
    private ITileDataProvider tileDataProvider;
    [IoC.Inject]
    public IContainer Container { set; protected get; }
    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }
    [IoC.Inject]
    public ILightUvProvider LightUvProvider { set; protected get; }
    [IoC.Inject]
    public ISpriteManager SpriteManager { set; protected get; }
    [IoC.Inject]
    public IScreenUpdateManager ScreenUpdateManager { set; protected get; }
    [IoC.Inject]
    public ILightRenderer LightRenderer { set; protected get; }

    [IoC.Inject]
    public ISwapItemManager SwapItemManager { set; protected get; }
   
    public Camera camera;
    public bool useLight = true;
    public float tileSize = 1;

    private float countTileX, countTileY;

    private GameObject tixtuteGO;
    private readonly List<IInfinityTileMap> layerMeshs = new List<IInfinityTileMap>();
    private Vector3 lastP1 = Vector3.one * -1000;
    private IInfinityTileMap lightLayer;
    private IInfinityTileMap obstacleLight;

    public Action OnSwap;
    
    public bool Is_Edit_mod;

    void Start() {
        Settings.IS_EDIT_MOD = Is_Edit_mod;
        this.Inject();
    }

    public void OnInject() {
        tileDataProvider = TileDataProvider;
        //this.Inject();
        UpdateScreenTiles();

        Container.Inject(tileDataProvider);

        tileDataProvider.Init(this, tileSize);

        tixtuteGO = new GameObject("tixtuteGO");

        foreach (var layer in LayerData.Layers) { 
            GameObject layerGO = new GameObject( layer.Name);
            layerGO.transform.SetParent(tixtuteGO.transform, false);
         //   layerGO.transform.localPosition = new Vector3(0, 0, /*-layer.Depth*/0);
            layerGO.layer = LayerMask.NameToLayer(layer.LayerName);
            IInfinityTileMap layerMesh = (IInfinityTileMap)Activator.CreateInstance(layer.RenderType);

            Container.Inject(layerMesh);

            if (layer == LayerData.ObstacleLightLayer) {
                obstacleLight = layerMesh;
            }else
            if (layer == LayerData.LightLayer) {
                lightLayer = layerMesh;
            } else {
                
            }

            if (layer != LayerData.ObstacleLightLayer) {
                layerMeshs.Add(layerMesh);
            }
            //

            layerMesh.LoadMesh(layerGO, (int)(countTileX) + 2 * layer.SizeAdd, (int)(countTileY) + 2 * layer.SizeAdd, tileSize, layer);
            layerMesh.SetLayer(layer.Depth);
            layerGO.transform.localPosition = new Vector3(-layer.SizeAdd*tileSize, -layer.SizeAdd*tileSize, 0);
        }

        SpriteManager.SetTileMap(layerMeshs[0]);  

        var centerPos = MapGenerator.CenterPos;
        camera.transform.position = new Vector3(centerPos.x * tileSize, centerPos.y * tileSize, camera.transform.position.z);

        ScreenUpdateManager.Init((int)(countTileX), (int)(countTileY));

      //  Debug.LogErrorFormat("({0}, {1})",Screen.width, Screen.height);
        
    }

    public bool usePixelPerfect = true;
    public float orthographic = 7f;
    void UpdateScreenTiles() {

        if (QualitySettings.vSyncCount != 0 || Application.targetFrameRate != 30) {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;
        }

        if (usePixelPerfect) {
            if (camera.orthographicSize != ((float) (Screen.height))/2f) {
                camera.orthographicSize = ((float) (Screen.height))/2f;

                //   Debug.LogError(Screen.height);
            }

        } else {
            if (camera.orthographicSize != ((float)(Screen.height)) / orthographic) {
                camera.orthographicSize = ((float)(Screen.height)) / orthographic;
            }
        }

        var p1 = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
        var p2 = camera.ViewportToWorldPoint(new Vector3(1, 1, camera.nearClipPlane));
        p1 = new Vector3(p1.x, p1.y, 0);
        p2 = new Vector3(p2.x, p2.y, 0);

        countTileX = (p2.x - p1.x)/(tileSize) + 4;
        countTileY = (p2.y - p1.y)/(tileSize) + 4;

        //Debug.LogErrorFormat("({0}, {1})", countTileX, countTileY);
    }


    private int oldTime = -1;
    public static Vector2Int screenStart;
    public static Vector2Int screenEnd;
    private Vector3 p1;
    public void Render() {
        p1 = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));

        int offsetX = Mathf.CeilToInt(p1.x / tileSize);
        int offsetY = Mathf.CeilToInt(p1.y / tileSize);
        Vector3 newPos = new Vector2(offsetX * tileSize - 2 * tileSize, offsetY * tileSize - 2 * tileSize);

        int dist = (int)Vector2.Distance(lastP1, newPos);
        if (dist >= tileSize) {

            Vector2 delta = newPos - lastP1;
            var deltaX = (int)(Mathf.Sign(delta.x) * Mathf.CeilToInt(Mathf.Abs(delta.x / tileSize)));
            var deltaY = (int)(Mathf.Sign(delta.y) * Mathf.CeilToInt(Mathf.Abs(delta.y / tileSize)));
            lastP1 = newPos;
          
            screenStart = new Vector2Int(Mathf.CeilToInt(p1.x / tileSize), Mathf.CeilToInt(p1.y / tileSize));
            screenEnd = new Vector2Int((int)(screenStart.x + countTileX), (int)(screenStart.y + countTileY));
            ScreenUpdateManager.SetScreenRect(screenStart, screenEnd);
            LightRenderer.SetScreenRect(screenStart, screenEnd);

            var add = obstacleLight.layerData.SizeAdd;

            SwapItemManager.SetSize(screenEnd.x - screenStart.x + 2 * add, screenEnd.y - screenStart.y + 2 * add);
            //SwapItemManager.SetStartPos(new Vector2Int(screenStart.x - add, screenStart.y - add));
            


            //определяем свапнутую область
            int swapStartX = 0;
            int swapEndX = Mathf.CeilToInt(countTileX);
            int swapStartY = 0;
            int swapEndY = Mathf.CeilToInt(countTileY);
            
            if (deltaX > 0) {
                swapStartX = 0;
                swapEndX = Mathf.CeilToInt(countTileX- deltaX) - 3;   
            }
            if (deltaX < 0) {
                swapStartX = -(int)deltaX; 
                swapEndX = Mathf.CeilToInt(countTileX);
            }
            if (deltaY > 0) {
                swapStartY = 0;
                swapEndY = Mathf.CeilToInt(countTileY- deltaY) - 3;                
            }
            if (deltaY < 0) {
                swapStartY = -(int)deltaY;
                swapEndY = Mathf.CeilToInt(countTileY);
            }

            for (int ii = 0; ii < layerMeshs.Count; ii++) {
                layerMeshs[ii].ShiftCells((int)deltaX, (int)deltaY);
            }
            obstacleLight.ShiftCells((int)deltaX, (int)deltaY);


            LightRenderer.Swap((int)deltaX, (int)deltaY);  // -<

            SwapItemManager.SetStartPos(new Vector2Int(screenStart.x - add, screenStart.y - add));
            SwapItemManager.Swap((int)deltaX, (int)deltaY);


            for (int i = 0; i < countTileX - 1; i++) {
                for (int j = 0; j < countTileY - 1; j++) {
                    bool inSwapRect = i >= swapStartX && i <= swapEndX &&
                                      j >= swapStartY && j <= swapEndY;

                    if (inSwapRect) {  
                        continue;
                    }

                    var x = p1.x + i * tileSize;
                    var y = p1.y + j * tileSize;

                    
                    var tileStructure = tileDataProvider.GetTileStructure(x, y);
                    for (int ii = 0; ii < layerMeshs.Count; ii++) {
                        var frameStructure = tileDataProvider.GetFrameStructure(x, y, tileStructure, layerMeshs[ii].layerData);
                        layerMeshs[ii].ChangeTile(j, i, frameStructure.TileCollection, frameStructure.FrameX,  frameStructure.FrameY, frameStructure.DeltaHeight);
                    } 
                } 
            }
            

            for (int i = 0; i < layerMeshs.Count; i++) {
                layerMeshs[i].RenderMesh(); 
            }

         //   RenderLayerAddSize(obstacleLight, obstacleLight.layerData.SizeAdd, deltaX, deltaY); // -<

            tixtuteGO.transform.position = newPos;
            OnSwap.TryCall();
        }
    }

    void RedrawTile(int i, int j) {
        var x = p1.x + i * tileSize;
        var y = p1.y + j * tileSize;


        var tileStructure = tileDataProvider.GetTileStructure(x, y);
        for (int ii = 0; ii < layerMeshs.Count; ii++) {
            var frameStructure = tileDataProvider.GetFrameStructure(x, y, tileStructure, layerMeshs[ii].layerData);
            layerMeshs[ii].ChangeTile(j, i, frameStructure.TileCollection, frameStructure.FrameX, frameStructure.FrameY, frameStructure.DeltaHeight);
        } 
    }

    void RenderLayerAddSize(IInfinityTileMap layer, int addSize, int deltaX, int deltaY) {
        //определяем свапнутую область
        
        var sizeX = countTileX + 2 * addSize;
        var sizeY = countTileY + 2 * addSize;

        int swapStartX = 0;
        int swapEndX = Mathf.CeilToInt(sizeX);
        int swapStartY = 0;
        int swapEndY = Mathf.CeilToInt(sizeY);

        if (deltaX > 0) {
            swapStartX = 0;
            swapEndX = Mathf.CeilToInt(sizeX - deltaX) - 2;
        }
        if (deltaX < 0) {
            swapStartX = -(int)deltaX;
            swapEndX = Mathf.CeilToInt(sizeX);
        }
        if (deltaY > 0) {
            swapStartY = 0;
            swapEndY = Mathf.CeilToInt(sizeY - deltaY) - 2;
        }
        if (deltaY < 0) {
            swapStartY = -(int)deltaY;
            swapEndY = Mathf.CeilToInt(sizeY);
        }
        
       // layer.ShiftCells((int)deltaX, (int)deltaY);

        //int swapCount = 0;
        for (int i = 0; i < sizeX - 1; i++) {
            for (int j = 0; j < sizeY - 1; j++) {
                bool inSwapRect = i >= swapStartX && i <= swapEndX &&
                                  j >= swapStartY && j <= swapEndY;

                if (inSwapRect) {
                    continue;
                }
                //swapCount++;

                var x = p1.x + i * tileSize - (tileSize * addSize);
                var y = p1.y + j * tileSize - (tileSize * addSize);

                var tileStructure = tileDataProvider.GetTileStructure(x, y);
                var frameStructure = tileDataProvider.GetFrameStructure(x, y, tileStructure, layer.layerData);
                layer.ChangeTile(j, i, frameStructure.TileCollection, frameStructure.FrameX, frameStructure.FrameY, frameStructure.DeltaHeight);
            }
        }
        //Debug.LogError("swap = " + swapCount + "  sizeY = " + sizeY + "sizeX = " + sizeX);
        layer.RenderMesh();
    }

    void ApplayScreenUpdateLight() {
        if (ScreenUpdateManager.GetChangeLight()) {
            LightRenderer.CalculateChangeScreen(ScreenUpdateManager.GetLightUptateWorldMin(), ScreenUpdateManager.GetLightUptateWorldMax());
            LightLayerRender();
            ScreenUpdateManager.EndUpdateLight();
        }
    }

    void ApplayScreenUpdate() {
        if (ScreenUpdateManager.GetApplayChanges()) {
            for (int i = 0; i < countTileX - 1; i++) {
                for (int j = 0; j < countTileY - 1; j++) {

                    if (!ScreenUpdateManager.IsUpdateOffset(i, j)) {
                        continue;
                    }
                     
                    var x = p1.x + i*tileSize;
                    var y = p1.y + j*tileSize;


                    var tileStructure = tileDataProvider.GetTileStructure(x, y);
                    for (int ii = 0; ii < layerMeshs.Count; ii++) {
                        if (layerMeshs[ii] == lightLayer)
                            continue;
                        if (ScreenUpdateManager.IsUpdate(layerMeshs[ii].layerData.Id, i, j)) {
                            var frameStructure = tileDataProvider.GetFrameStructure(x, y, tileStructure, layerMeshs[ii].layerData);
                            layerMeshs[ii].ChangeTile(j, i, frameStructure.TileCollection, frameStructure.FrameX, frameStructure.FrameY, frameStructure.DeltaHeight);
                            ScreenUpdateManager.EndDrawScreenTile(layerMeshs[ii].layerData.Id, i, j);
                        }
                    }
                }
            }

            for (int i = 0; i < layerMeshs.Count; i++) {
                if (layerMeshs[i] == lightLayer)
                    continue;

                layerMeshs[i].RenderMesh();
            }
        
            ScreenUpdateManager.SetApplayChanges(false);
        }

      //  ApplayObstacleLightUpdate();
    }


    void ApplayObstacleLightUpdate() {
        if (ScreenUpdateManager.GetChangeObstacleLight()) {
            var addSize = obstacleLight.layerData.SizeAdd;
            for (int i = 0; i < countTileX + 2*addSize - 1; i++) {
                for (int j = 0; j < countTileY + 2*addSize - 1; j++) {

                    if (!ScreenUpdateManager.IsUpdateOffsetObstacleLight(i, j)) {
                        continue;
                    }

                    var x = p1.x + i * tileSize - (tileSize * addSize);
                    var y = p1.y + j * tileSize - (tileSize * addSize);

                    var tileStructure = tileDataProvider.GetTileStructure(x, y);
                    var frameStructure = tileDataProvider.GetFrameStructure(x, y, tileStructure, obstacleLight.layerData);
                    obstacleLight.ChangeTile(j, i, frameStructure.TileCollection, frameStructure.FrameX, frameStructure.FrameY, frameStructure.DeltaHeight);
                    ScreenUpdateManager.EndDrawObstacleLightTile(i, j);
                }
            }

            obstacleLight.RenderMesh();
            ScreenUpdateManager.SetChangeObstacleLight(false);
        }
    }

    void LightLayerRender(bool isUpdate = false) {
        if (isUpdate) {
            Debug.LogError("isUpdate");
        }
        var p11 = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
        var p = tileDataProvider.WorldPosToOffsetTile(new Vector2(p11.x, p11.y));
        for (int i = 0; i < countTileX - 1; i++) {
            for (int j = 0; j < countTileY - 1; j++) {
                //var x = p11.x + i * tileSize;
                //var y = p11.y + j * tileSize;

                if (!isUpdate) {
                    //  var p = tileDataProvider.WorldPosToOffsetTile(new Vector2(x, y));
                    if (p.x + i < LightRenderer.UpdateScreenMin.x || p.x + i > LightRenderer.UpdateScreenMax.x ||
                        p.y + j < LightRenderer.UpdateScreenMin.y || p.y + j > LightRenderer.UpdateScreenMax.y) {
                        continue;
                    }
                }

                //var tileStructure = tileDataProvider.GetTileStructure(x, y);
                //var frameStructure = tileDataProvider.GetFrameStructure(x, y, tileStructure, lightLayer.layerData);
                //lightLayer.ChangeTile(j, i, frameStructure.TileCollection, frameStructure.FrameX, frameStructure.FrameY, frameStructure.DeltaHeight);
                
                var color = global::LightRenderer.color[i + global::LightRenderer.RaySize,
                       j + global::LightRenderer.RaySize];

                byte val = color.A ;/// 255f;
                Color32 c = new Color32(val, val, val, 255);

                for (int ii = 0; ii < layerMeshs.Count; ii++) {
                    if (layerMeshs[ii] == LayerData.LightLayer)
                        continue;
                    if (layerMeshs[ii] == LayerData.ObstacleLightLayer)
                        continue;

                   

                    layerMeshs[ii].ChangeColor(j, i, c);
                }

            }
        }

        for (int ii = 0; ii < layerMeshs.Count; ii++) {
            if (layerMeshs[ii] == LayerData.LightLayer)
                continue;
            if (layerMeshs[ii] == LayerData.ObstacleLightLayer)
                continue;
            layerMeshs[ii].RenderMesh();
        }

        //lightLayer.RenderMesh();
    }

    private FrameStructure fs = FrameStructure.Empty();
    public byte carrentMarkerLayer = 3;
    public short carrentMarkerType = 8;
    public byte carrentMarkeLiquidType = 1;
    private Vector2 lastPos;
    private int countClick;
	// Update is called once per frame
    //void LateUpdate() {
    private int countFixedUpdate = -1;
    void Update() {
         
	    if (LightUvProvider != null)
	        LightUvProvider.LateUpdate();


        if (tileDataProvider == null)
            return;


        if (Input.GetKeyUp(KeyCode.T)) {
           LightRenderer.Test();
           LightLayerRender(true);
        }

	    if (Input.GetKeyUp(KeyCode.G)) {
	        MapGenerator.ChangeMap();
	    }

	    if (Input.GetKeyUp(KeyCode.Alpha0)) {
	        carrentMarkerType = 0;
	        carrentMarkerLayer = 0;
	    }

        if (Input.GetKeyUp(KeyCode.Alpha8)) {
            carrentMarkerType = 8;
            carrentMarkerLayer = 3;
        }

	    if (Is_Edit_mod) {

	        if (Input.GetMouseButton(0)) {
	            countClick++;
	            var pos = camera.ScreenToWorldPoint(Input.mousePosition);
	       
	            tileDataProvider.СircleDamageTile(pos.x + 1.5f*tileSize, pos.y + 1.5f*tileSize, carrentMarkerType,
	                carrentMarkerLayer, carrentMarkeLiquidType, 1);
	            lastP1 = new Vector3();

	            UpdateScreenTiles();
	           // Render();

	        }

	        if (Input.GetMouseButton(1)) {
	            var pos = camera.ScreenToWorldPoint(Input.mousePosition);
	            var tile = tileDataProvider.GetTileStructure(pos.x + 1.5f*tileSize, pos.y + 1.5f*tileSize);
	                // pos.x, pos.y);
	            carrentMarkerType = tile.type;
	            carrentMarkerLayer = tile.layer;
	            carrentMarkeLiquidType = tile.liquidType;
	        }
	    }

	    if (Input.GetKeyUp(KeyCode.R)) {
            tileDataProvider.LoadNext();
            lastP1 = new Vector3();

            UpdateScreenTiles();  
           // Render();
        }
          
        //Profiler.BeginSample("1");
        UpdateScreenTiles();
        //Profiler.EndSample();
        //Profiler.BeginSample("2");
        ApplayScreenUpdate();
        //Profiler.EndSample();
        //Profiler.BeginSample("3");
        Render();
        //Profiler.EndSample();
        //Profiler.BeginSample("4");
        if (useLight) {
            ApplayScreenUpdateLight();
        }
        //Profiler.EndSample();


        LightRenderer.Update();
        ScreenUpdateManager.RedrawFullScreenLight();
    }

    

    private void LateUpdate() {
        tileDataProvider.Update();
        if (Input.GetKeyUp(KeyCode.P)) { 
            LightLayerRender(true);
        }

        Update1();
    }
    
    
    void Update1() {
        var vect = new Vector3(tixtuteGO.transform.position.x, tixtuteGO.transform.position.y, 0);
        //layerMeshs[0].DrawMesh(vect, camera);
        for (int i = 0; i < layerMeshs.Count; i++) {
            layerMeshs[i].DrawMesh(vect, camera);
        } 
           
    }
    
    /*
    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        var vect = new Vector3(tixtuteGO.transform.position.x, tixtuteGO.transform.position.y, 0);
      
        for (int i = 0; i < layerMeshs.Count; i++) {
            layerMeshs[i].DrawMesh(vect, camera);
        }
     
    }*/
    
    /*
    
    
    private List<Gizm> gizmoInfo = new List<Gizm>();

    private void OnDrawGizmos() {
        GUIStyle myStyle = new GUIStyle(GUI.skin.GetStyle("label"));
        myStyle.fontSize = 32;
        //myStyle.s
        if (gizmoInfo != null) {
            foreach (var gizmo in gizmoInfo) {
                Handles.Label(gizmo.Vector2, gizmo.Tile.ToString(), myStyle);
            }
        }
    }

    class Gizm {
        public TileInfoStructure Tile;
        public Vector2 Vector2;
    }    
    
    
    */
    
}
using UnityEngine;
using System.Collections;
using IoC;
using Managers;

public class SetColor : MonoBehaviour ,IInitialize{
    [Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }
    [IoC.Inject]
    public MapGenerator mapGenerator { set; protected get; }
    [IoC.Inject]
    public IScreenUpdateManager ScreenUpdateManager { set; protected get; }

    private int countTileMapX;
    private int countTileMapY;

    public bool ignoreWater = false;
    public float colorSize = 1;

    void Start() {
        this.Inject();
    }

    void Awake() {
        this.Inject();
    }

    private Vector2Int oldOffset = new Vector2Int();
    private float oldColorSize = 0;
    void FixedUpdate() {
        if (TileDataProvider == null)
            return;

        var offse = TileDataProvider.WorldPosToOffsetTile(transform.position);
        if (oldColorSize == colorSize && oldOffset.x == offse.x && oldOffset.y == offse.y)
            return;

        if (!(offse.x > 0 && offse.y > 0 && offse.x <= countTileMapX && offse.y <= countTileMapY)) {
            return;
        }

       // if (offse.x >= LightRenderer.minPos.x && offse.y >= LightRenderer.minPos.y &&
       //     offse.x <= LightRenderer.maxPos.x && offse.y <= LightRenderer.maxPos.y) {

        if (!ignoreWater && mapGenerator.GetMap()[offse.x, offse.y].IsWater()) {
            mapGenerator.GetMap()[oldOffset.x, oldOffset.y].lightAlpha = 0;
            mapGenerator.GetMap()[offse.x, offse.y].lightAlpha = 0;
            } else {
                mapGenerator.GetMap()[oldOffset.x, oldOffset.y].lightAlpha = 0;
                mapGenerator.GetMap()[offse.x, offse.y].lightAlpha = colorSize;

            } 
             

            oldOffset = offse;
            ScreenUpdateManager.RedrawFullScreenLight();

            oldColorSize = colorSize;
        //}

        
    }

    public void OnDisable() {
        OnDestroy();

    }

    public void OnDestroy() {
        if (mapGenerator != null) {
            mapGenerator.GetMap()[oldOffset.x, oldOffset.y].lightAlpha = 0;
        }
        if (ScreenUpdateManager != null) {
            ScreenUpdateManager.RedrawFullScreenLight();
        }
        
    }

    public void OnInject() {
        countTileMapX = mapGenerator.SizeX - 1;
        countTileMapY = mapGenerator.SizeY - 1; 
    }
}

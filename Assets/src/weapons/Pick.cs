using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using trasharia.weapons;

//кирка
public class Pick : MonoBehaviour, IWeapon {

    public Transform view;
    public Transform firePoint;
    public float cooldown;
    public int range;

    [IoC.Inject]
    public ICooldownManager CooldownManager { set; protected get; }
    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }

    [IoC.Inject]
    public ITileDataProvider tileDataProvider { set; protected get; }

    [IoC.Inject]
    public ISwapItemManager SwapItemManager { set; protected get; }

    [IoC.Inject]
    public ICollision Collision { set; protected get; }

    private bool isTouch;

    private TileInfoStructure[,] Map {
        get {
            return MapGenerator.GetMap();
        }
    }

    private DamagePointBase damagePoint;
    private IItem itemData;
    void Start() {
        this.Inject();
        damagePoint = firePoint.gameObject.GetComponent<DamagePointBase>();
        if (damagePoint == null) {
            damagePoint = firePoint.gameObject.AddComponent<DamagePointBase>();
        }
        damagePoint.enabled = false;
    }

    public void Init(ref IItem data, PlayerModel playerModel) {
        itemData = data;
        firePoint = transform.transform.GetComponentInParent<PlayerController>().transform;
        var go = UltimateJoystick.GetJoystick("Joystick2");
        go.SetActive(true);
    }

    public IItem GetItem() {
        return itemData;
    }

    private bool isFireing = false;
    public void Fire(Vector2 direct, bool isTouch)
    {
        
        this.isTouch = isTouch;
        if (isFireing)
            return;
        isFireing = true;
        var tween = TweenRotation.Begin(gameObject, cooldown, Quaternion.identity);
        tween.from = new Vector3(0, 0, 90);
        tween.to = new Vector3(0, 0, -90);
        tween.SetOnFinished(() => {
            OnFinishedFire();
        });

        damagePoint.enabled = true;
    }

    
    public void FireCycle(Vector2 direct, bool isTouch)
    {
        Fire(direct, isTouch);
    }

    void OnFinishedFire() {
        isFireing = false;
        transform.localRotation = new Quaternion();
        damagePoint.enabled = false;

        var ray = Vector3.zero;

        var v = UltimateJoystick.GetVerticalAxis("Joystick2");
        var h = UltimateJoystick.GetHorizontalAxis("Joystick2");
        ray = new Vector3(h, v) *100;

        if (!isTouch)
        {

            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            ray = (pos - firePoint.position);
           // Debug.LogError(Vector2.Distance(ray,Vector2.zero));

        }

        var tilesDown = Collision.Raycast(firePoint.position, new Vector2(1, 1), ray, false);

        if(tilesDown.Count>0){
            // ищем ближайшею
            int index = 0;
            float minDist = float.MaxValue;
            for (int i = 0; i < tilesDown.Count; i++) {
                var newDist = Vector3.Distance(firePoint.position, tileDataProvider.OffsetTileToWorldPos(tilesDown[i])) / tileDataProvider.TileSize;
                if (newDist < minDist) {
                    minDist = newDist;
                    index = i;
                }
            }


            if (minDist <= range) {
                tileDataProvider.DamageTile(tilesDown[index], 100);
                var pos = tileDataProvider.OffsetTileToWorldPos(tilesDown[index]);
                //var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var removeItemId = SwapItemManager.RemoveItem(pos);
                if (removeItemId != 0) {
                    //сделать предмет
                }
            }

        }
    }

    public void Rotate(Vector2 target) {

    }

}



public class DamagePointBase : MonoBehaviour {
    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }
    [IoC.Inject]
    public ICollision Collision { set; protected get; }
    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }
    [IoC.Inject]
    public IGOPoolManager IgoPoolManager { set; protected get; }
    [IoC.Inject]
    public ITargetManager TargetManager { set; protected get; }


    private Vector3 startFirePos;
    private Vector3 endFirePos;

    private TileInfoStructure[,] Map {
        get {
            return MapGenerator.GetMap();
        }
    }

    private void Start() {
        this.Inject();
    }

    protected virtual void OnEnable() {
        startFirePos = transform.position;
    }

    void OnDisable() {
        // Do(startFirePos, endFirePos);
    }

    void Do(Vector3 start, Vector3 end) {
        var tiles = Collision.Raycast(start, Vector2.one, end - start, true);
        for (int i = 0; i < tiles.Count; i++)
        {
            var tile = tiles[i];
            if (Map[tile.x, tile.y].IsDecor())
            {
                TileDataProvider.ChangeTile(tile, 0, 0);
            }
        }

    }

    void Update() {
        endFirePos = transform.position;
        Do(startFirePos, endFirePos);
        startFirePos = endFirePos;
    }
}
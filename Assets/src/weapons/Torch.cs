using UnityEngine;
using System.Collections;
using trasharia.weapons;
using IoC;

public class Torch : MonoBehaviour, IWeapon, IoC.IInitialize {
    [IoC.Inject]
    public ISwapItemManager SwapItemManager { set; protected get; }
    [IoC.Inject]
    public ITileDataProvider tileDataProvider { set; protected get; }
    [IoC.Inject]
    public MapGenerator mapGenerator { set; protected get; }

    private int countTileMapX;
    private int countTileMapY;

    public int range;
    public GameObject flame;
    private PlayerModel playerModel;
    public void OnInject() {
        countTileMapX = mapGenerator.SizeX - 1;
        countTileMapY = mapGenerator.SizeY - 1; 
    }

    void Start() {
        this.Inject();
    }

    private IItem itemData;
    public void Init(ref IItem data, PlayerModel playerModel) {
        this.playerModel = playerModel;
        itemData = data;
    }

    public IItem GetItem() {
        return itemData;
    }

    void Update() {
        var offse = tileDataProvider.WorldPosToOffsetTile(transform.position);

        if (!(offse.x > 0 && offse.y > 0 && offse.x <= countTileMapX && offse.y <= countTileMapY)) {
            flame.SetActive(true);
            return;
        }

        flame.SetActive(!mapGenerator.GetMap()[offse.x, offse.y].IsWater());
    }

    public void Fire(Vector2 direct, bool isTouch)
    {
        var tween = TweenRotation.Begin(gameObject, 0.3f, Quaternion.identity);
        tween.from = new Vector3(0, 0, 90);
        tween.to = new Vector3(0, 0, -90);
        tween.SetOnFinished(() => {
            // OnFinishedFire();
            transform.rotation = Quaternion.identity;
        });


        var distance = Vector3.Distance(transform.position, direct) / tileDataProvider.TileSize;
        if (distance > range) {
            return;
        }


        if (SwapItemManager.AddItem(ItemData.Torch.Id, direct)) {
            playerModel.GetBag().UseItem(itemData);    
        }
    }

    public void FireCycle(Vector2 direct, bool isTouch)
    {
        ///throw new System.NotImplementedException();
    }

    public void Rotate(Vector2 target) {
       // throw new System.NotImplementedException();
    }

   
}

using UnityEngine;
using System.Collections;
using trasharia.weapons;

public class Block : MonoBehaviour, IWeapon, IoC.IInitialize {
    [IoC.Inject]
    public ITileDataProvider tileDataProvider { set; protected get; }
    [IoC.Inject]
    public ISpriteManager SpriteManager { set; protected get; }

    [IoC.Inject]
    public IPhysicsManager PhysicsManager { set; protected get; }
    [IoC.Inject]
    public MapGenerator mapGenerator { set; protected get; }

    
    [IoC.Inject]
    public ITargetManager TargetManager { set; protected get; }

    public int range;
    private float tileSize;

    public SpriteRenderer spriteRenderer;

    public void OnInject() {
        tileSize = tileDataProvider.TileSize;
    }
    public byte layer = 1;
    public short type = 0;

    private BlowBlockItem itemData;
    private PlayerModel playerModel;

    public void Init(ref  IItem data, PlayerModel playerModel) {
        this.playerModel = playerModel;
        this.Inject();
        var blowBlock = data as BlowBlockItem;
        itemData = blowBlock;
        layer = (byte)blowBlock.Layer;
        type = (short)blowBlock.Type;
        spriteRenderer.sprite = SpriteManager.CreateSprite(blowBlock.TileCollection, 2, 3);

        var go = UltimateJoystick.GetJoystick("Joystick2");
        go.SetActive(false);
    }

    public IItem GetItem() {
         return itemData;
    }

    void Start() {
        this.Inject();
    }

    void Update() {

    }

   

    private bool isFireing = false;
    private Vector2 pos;
    public void FireCycle(Vector2 pos, bool isTouch)
    {
        if (isFireing)
            return;

        isFireing = true;
        var tween = TweenRotation.Begin(gameObject, 0.0001f, Quaternion.identity);
        tween.from = new Vector3(0, 0, 90);
        tween.to = new Vector3(0, 0, -90);
        tween.SetOnFinished(() => {
             OnFinishedFire();
            transform.rotation = Quaternion.identity;
        });

        this.pos = pos;
    }

    void OnFinishedFire() {
        isFireing = false;

        var distance = Vector3.Distance(transform.position, pos) / tileSize;
        if (distance > range) {
            return;
        }

        var offset = tileDataProvider.WorldPosToOffsetTile(pos);
        if (mapGenerator.IsValidPoint(offset)) {
            var map = mapGenerator.GetMap()[offset.x, offset.y];

            if (map.IsEmpty() && map.BodyItemId == 0 && (!map.IsWater() || type == (short)TileTypeEnum.Stairs)) {
                var items = PhysicsManager.GetItemsOfOffset(offset);
                //проверяем не пересекаемся ли мы с физическим обьектом
                if (items.Count == 0 || type == (short)TileTypeEnum.Stairs) {

                    var oldType = mapGenerator.GetMap()[offset.x, offset.y].type;
                    tileDataProvider.ChangeTile(pos.x + 1.5f * tileSize, pos.y + 1.5f * tileSize, type, layer);
                    if (type == (short) TileTypeEnum.Stairs)
                    {
                        var map2 = mapGenerator.GetMap()[offset.x + 1, offset.y];
                        if(map2.IsEmpty())
                        tileDataProvider.ChangeTile(pos.x + tileSize + 1.5f * tileSize, pos.y + 1.5f * tileSize, type, layer);
                    }

                    var newType = mapGenerator.GetMap()[offset.x, offset.y].type;

                    if (newType != oldType) {
                        playerModel.GetBag().UseItem(itemData);
                    }
                } else {
                  //  Debug.LogError(items.Count);
                }
            } else {
                //Debug.LogError(map.IsEmpty());
            }
        }
    }

    public void Fire(Vector2 direct, bool isTouch)
    {
        ///throw new System.NotImplementedException();
    }

    public void Rotate(Vector2 target) {
        // throw new System.NotImplementedException();
    }

    
}

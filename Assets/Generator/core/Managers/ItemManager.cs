using UnityEngine;
using System.Collections;

public class ItemManager : IItemManager, IoC.IInitialize {
    [IoC.Inject]
    public ISpriteManager SpriteManager { set; protected get; }

    [IoC.Inject]
    public IGOPoolManager IgoPoolManager { set; protected get; }

    private GameObject parent;

    public void OnInject() {
        var GameContext = GameObject.Find("GameContext");
        parent = new GameObject("ParantItems");
        parent.transform.parent = GameContext.transform;
    }

    public BlockPoolBagItem CreateBlock(Vector2 worldPos, string tileCollection) {
        var go = IgoPoolManager.Spawn<BlockPoolBagItem>(worldPos, new Quaternion());
        var spriteRenderer = go.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = SpriteManager.CreateSprite(tileCollection, 2, 3);
        var mbPhysicItem = go.GetComponent<MonoBehaviourPhysicItem>();

        var PhysicItem = mbPhysicItem.GetPhysicItem();
        if (PhysicItem != null) {
            PhysicItem.SetPosition(worldPos);
        }

        return go.GetComponent<BlockPoolBagItem>();
    }


    public void CreateItem<T>(Vector2 worldPos) where T : IBasePoolObject {
        var go = IgoPoolManager.Spawn<T>(worldPos, new Quaternion());

        var mbPhysicItem = go.GetComponent<MonoBehaviourPhysicItem>();

        var PhysicItem = mbPhysicItem.GetPhysicItem();
        if (PhysicItem != null) {
            PhysicItem.SetPosition(worldPos);
        }
    }
}

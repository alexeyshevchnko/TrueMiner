using UnityEngine;
using System.Collections;
using trasharia.weapons;

public class BlockPoolBagItem : BasePoolEffect, IPoolBagItem {
    public BlockItem blockItem;
    public int blockLayer;
    public int blockType;
    public string blockTileCollection;

    public override void OnSpawn() {
        this.Inject();
        var pi = blockItem.GetPhysicItem();
        if (pi != null) {
            pi.SetSleep(false);
           // pi.Update();

            pi.AddVelocity(-pi.velocity);

            var pref = Random.Range(0, 100) % 2 == 0 ? 1 : -1;
            var pref2 = Random.Range(0, 5);
            pi.AddVelocity(new Vector2(pref * 2, 2.4f + pref2));
            pi.ReboundFactor = 0.2f;
            pi.Update();
        }
    }

    protected override void UnSpawn() {
        IgoPoolManager.Unspawn(gameObject);
    }

    public override void OnUnSpawn() {
        var pi = blockItem.GetPhysicItem();
        if (pi != null) {
            pi.UnRegister();
        }
    }

    public IItem GetBagItem() {
        return new BlowBlockItem(blockLayer, (short)blockType) { Count = 1 };
    }
}

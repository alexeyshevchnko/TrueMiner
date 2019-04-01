using UnityEngine;
using System.Collections;
using trasharia.weapons;

public class TorchPoolItem : BasePoolEffect, IPoolBagItem {
    public MonoBehaviourPhysicItem blockItem;

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
            pi.Register();
        }
    }
    /*
    private void Update() {
        if (blockItem != null) {
            var pi = blockItem.GetPhysicItem();
            if (pi != null) {
                pi.Register();
            }
        }
    }*/

    public override void OnUnSpawn() {
        //Debug.LogError("OnUnSpawn");
         var pi = blockItem.GetPhysicItem();
        if (pi != null) {
            pi.UnRegister();
        }
        var setColor = GetComponent<SetColor>();
        setColor.OnDestroy();
    }
    /*
    void OnDisable() {
        //Debug.LogError("OnDisable");
        var pi = blockItem.GetPhysicItem();
        if (pi != null) {
            pi.UnRegister();
        }
    }*/

    public IItem GetBagItem() {
        return new TorchItem() {Count = 1};
    }
}

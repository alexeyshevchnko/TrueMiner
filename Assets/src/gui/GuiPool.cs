using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GuiPool : GOPool {
    public static GuiPool guiPool; 
    public override void OnInject() {
        //RegisterPool();

        foreach (Transform child in transform) {
            if (child.gameObject.activeInHierarchy)
                Unspawn(child.gameObject);
        }
    }

    protected override void Awake() {
        guiPool = this;
        Pool = this;
        int amount = 0;
        for (var i = 0; i < caches.Count; i++) {
            caches[i].Initialize(this);
            amount += caches[i].cacheSize;
        }
        activeCachedObjects = new Hashtable();
       // Debug.LogError(gameObject.name);
       // this.Inject();

        OnInject();
    }

}

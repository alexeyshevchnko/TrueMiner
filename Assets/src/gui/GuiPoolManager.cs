using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuiPoolManager : IGuiPoolManager {

    private GOPool pool;

    public void SetPool(GOPool pool) {
        this.pool = pool;
        pool.OnUnspawnGO += FreeResources;
    }

    private void FreeResources(GameObject objectToDestroy) {

    }

    public GameObject Spawn<T>(Vector3 pos, Quaternion rot) where T : IBasePoolObject {
        return pool.Spawn<T>(pos, rot);
    }

    public void Unspawn(GameObject toDestroy) {
        pool.Unspawn(toDestroy);
    }

    public void AddCache(GameObject prefab) {
        var item = new GOPool.ObjectCache() { prefab = prefab, cacheSize = 10, isDynamic = true };
        pool.caches.Add(item);
        item.Initialize(pool);
    }
}

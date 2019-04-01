using UnityEngine;
using System.Collections;

public class GOPoolManager : IGOPoolManager {
    [IoC.Inject]
    public IPrefubManager PrefubManager { set; protected get; }

    private GOPool pool;

    public void SetPool(GOPool pool) {
        this.pool = pool;
        pool.OnUnspawnGO += FreeResources;
    }

    private void FreeResources(GameObject objectToDestroy) {

    }

    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot) {
        return pool.Spawn(prefab, pos, rot);
    }

    public void AddCache(GameObject prefab) {
        var item = new GOPool.ObjectCache() { prefab = prefab, cacheSize = 10, isDynamic = true };
        pool.caches.Add(item);
        item.Initialize(pool);
    }

    public GameObject Spawn(GameObject prefab) {
        return pool.Spawn(prefab, Vector3.zero, Quaternion.identity);
    }

    public GameObject Spawn<T>() where T : IBasePoolObject {
        return pool.Spawn<T>(Vector3.zero, Quaternion.identity);
    }

    public GameObject Spawn<T>(Vector3 pos, Quaternion rot) where T : IBasePoolObject {
        return pool.Spawn<T>(pos, rot);
    }

    public void Unspawn(GameObject toDestroy) {

        pool.Unspawn(toDestroy);
    }

    public void UnspawnAll() {
        pool.UnspawnAll();
    }


    public GameObject SpawnItem(short id) {
        var go =  PrefubManager.GetItem(id);
        return Spawn(go);
    }


    public GameObject SpawnItem(short id, Vector3 pos, Quaternion rot) {
        var go = PrefubManager.GetItem(id);
        return pool.Spawn(go, pos, rot);
    }


    public GameObject SpawnItem(short id, Vector3 pos) {
        var go = PrefubManager.GetItem(id);
        return pool.Spawn(go, pos, Quaternion.identity);
    }
}

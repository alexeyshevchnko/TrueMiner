using UnityEngine;
using System.Collections;

public interface IGOPoolManager {
    void SetPool(GOPool pool);
    GameObject Spawn<T>(Vector3 pos, Quaternion rot) where T : IBasePoolObject;
    void Unspawn(GameObject toDestroy);
    void AddCache(GameObject prefab);
    GameObject SpawnItem(short id, Vector3 pos);
}

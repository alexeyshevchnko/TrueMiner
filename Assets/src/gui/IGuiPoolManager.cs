using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGuiPoolManager  {
    void SetPool(GOPool pool);
    GameObject Spawn<T>(Vector3 pos, Quaternion rot) where T : IBasePoolObject;
    void Unspawn(GameObject toDestroy);
    void AddCache(GameObject prefab);
}

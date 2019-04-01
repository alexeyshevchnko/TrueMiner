using System.Linq;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using trasharia;


public class GOPool : MonoBehaviour, IoC.IInitialize {
    [Serializable]
    public class ObjectCache {
        [SerializeField] public GameObject prefab;

        public IBasePoolObject Type;

        [SerializeField] public int cacheSize = 10;

        [SerializeField] public bool isDynamic = true;

        private int cacheIndex = 0;
        private List<GameObject> objects;

        private GOPool Pool;

        [HideInInspector]
        public void Initialize(GOPool pool) {
            Pool = pool;
            Type = prefab.GetComponent<IBasePoolObject>();
            objects = new List<GameObject>();
            for (var i = 0; i < cacheSize; i++) {
                objects.Add(Pool.InstantiateGameObject(prefab, new Vector3(), Quaternion.identity));
                objects[i].SetActive(false);
                objects[i].name = objects[i].name + i;
                objects[i].transform.parent = Pool.transform;
            }
        }

        public GameObject GetNextObjectInCache() {
            GameObject obj = null;

            for (var i = 0; i < cacheSize; i++) {
                obj = objects[cacheIndex];

                if (obj == null) {
                    Debug.LogWarning("Try spawn of " + prefab.name +
                                     ", but cached object is destroyed! Instantiate object.");
                    obj = objects[cacheIndex] = Pool.InstantiateGameObject(prefab, new Vector3(), Quaternion.identity);
                    cacheIndex = (cacheIndex + 1)%cacheSize;
                    return obj;
                }

                if (!obj.activeSelf) {
                    break;
                }

                // If not, increment index and make it loop around
                // if it exceeds the size of the cache
                cacheIndex = (cacheIndex + 1)%cacheSize;
            }


            if (obj == null || (isDynamic && obj.activeSelf)) {
                cacheSize++;
                objects.Add(Pool.InstantiateGameObject(prefab, new Vector3(), Quaternion.identity));
                GameObject nextObjectInCache = objects.Last();
                nextObjectInCache.SetActive(false);
                nextObjectInCache.name = nextObjectInCache.name + (cacheSize - 1);
                nextObjectInCache.transform.parent = Pool.transform;
                cacheIndex = (cacheIndex + 1)%cacheSize;
                Pool.activeCachedObjects.Add(nextObjectInCache, true);
                return nextObjectInCache;
            }

            cacheIndex = (cacheIndex + 1)%cacheSize;
            return obj;
        }

        public void UnspawnAll() {
            foreach (var o in objects) {
                if (o.activeSelf) {
                    o.SetActive(false);
                }
            }
            cacheIndex = 0;
        }

        public void RemoveFromPool(GameObject go) {
            if (objects!=null && objects.Remove(go)) {
                cacheSize--;
                if (cacheSize != 0) {
                    cacheIndex = (cacheIndex + 1)%cacheSize;
                } else {
                    cacheIndex = 0;
                }
            }
        }
    }

    [IoC.Inject]
    public IGOPoolManager IGOPoolManager { set; protected get; }

    protected GOPool Pool;

    public List<ObjectCache> caches;

    public Hashtable activeCachedObjects;

    public Action<GameObject> OnUnspawnGO;

    private void Start() {
        
    }


    protected virtual void RegisterPool() {
        IGOPoolManager.SetPool(this);
    }

    public virtual void OnInject() {
        RegisterPool();

        foreach (Transform child in transform) {
            if (child.gameObject.activeInHierarchy)
                Unspawn(child.gameObject);
        }
    }

    protected virtual void Awake() {
        Pool = this;
        int amount = 0;
        for (var i = 0; i < caches.Count; i++) {
            caches[i].Initialize(this);
            amount += caches[i].cacheSize;
        }
        activeCachedObjects = new Hashtable();
     //   Debug.LogError(gameObject.name);
        this.Inject(); 
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation) {
        ObjectCache cache = null;
        for (var i = 0; i < Pool.caches.Count; i++) {
            if (Pool.caches[i].prefab == prefab) {
                cache = Pool.caches[i];
                break;
            }
        }

        if (cache == null) {
            cache = new ObjectCache() {prefab = prefab, cacheSize = 1};
            cache.Initialize(this);
            Pool.caches.Add(cache);
        }

        GameObject obj = cache.GetNextObjectInCache();

        obj.transform.position = position;
        obj.transform.rotation = rotation;

        obj.SetActive(true);

        Pool.activeCachedObjects[obj] = true;

        

        return obj;
    }

    public GameObject Spawn<T>(Vector3 position, Quaternion rotation) where T : IBasePoolObject {

        ObjectCache cache = null;
        for (var i = 0; i < Pool.caches.Count; i++) {
            if (Pool.caches[i].Type != null && Pool.caches[i].Type is T) {
                cache = Pool.caches[i];
                break;
            }
        }

        GameObject obj = cache.GetNextObjectInCache();

        obj.transform.position = position;
        obj.transform.rotation = rotation;

        obj.SetActive(true);

        Pool.activeCachedObjects[obj] = true;

        obj.GetComponent<T>().OnSpawn();

        return obj;
    }

    private GameObject InstantiateGameObject(GameObject prefab, Vector3 position, Quaternion rotation) {
        GameObject newObj = GameObject.Instantiate(prefab, position, rotation) as GameObject;
        newObj.transform.parent = Pool.transform;

        var item = newObj.AddComponent<GameObjectPoolItem>();
        item.onDestroyAction += RemoveFromPool;

        return newObj;
    }

    private void RemoveFromPool(GameObject go) {
        for (var i = 0; i < Pool.caches.Count; i++) {
            Pool.caches[i].RemoveFromPool(go);
        }
    }

    public void Unspawn(GameObject objectToDestroy) {

        DispoceGO(objectToDestroy);

        if (Pool != null && Pool.activeCachedObjects.ContainsKey(objectToDestroy)) {
            var obj = objectToDestroy.GetComponent<BasePoolEffect>();
            if (obj != null) {
                obj.OnUnSpawn();
            }
            objectToDestroy.SetActive(false);
            
            Pool.activeCachedObjects[objectToDestroy] = false;
            objectToDestroy.SetInParent(transform);
        }

        if (!Pool.activeCachedObjects.ContainsKey(objectToDestroy)) {
            Destroy(objectToDestroy);
        }
    }

    public void UnspawnAll() {
        foreach (var cache in Pool.activeCachedObjects.Keys) {
            DispoceGO((GameObject) cache);
        }

        Pool.activeCachedObjects.Clear();
        foreach (var cache in caches) {
            cache.UnspawnAll();
        }

    }

    private void DispoceGO(GameObject objectToDestroy) {
        OnUnspawnGO.TryCall(objectToDestroy);
    }

}

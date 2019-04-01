using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrefubManager : IPrefubManager {
    private GameObject player;
    private List<PrefubItem> units;

    public Dictionary<short, GameObject> Items;

    [IoC.Inject]
    public IGOPoolManager IgoPoolManager { set; protected get; }

    public void InitPlayer(GameObject go) {
        player = go;
    }

    public void InitUnits(List<PrefubItem> units) {
        this.units = units;
    }

    public void InitItems(List<PrefubItem> items) {
        Items = new Dictionary<short, GameObject>();
        foreach (var item in items) {
            Items[(short)item.id] = item.go;
            IgoPoolManager.AddCache(item.go);
        }
    }

    public GameObject GetPlayer() {
        return player;
    }

    public GameObject GetUnit(int id) {
        return units.Find(x=>x.id == id).go;
    }

    public GameObject GetItem(short id) {
        return Items[id];
    }


}

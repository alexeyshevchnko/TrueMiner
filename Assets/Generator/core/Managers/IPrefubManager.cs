using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IPrefubManager {
    void InitPlayer(GameObject go);
    void InitUnits(List<PrefubItem> units);
    void InitItems(List<PrefubItem> items);
    GameObject GetPlayer();
    GameObject GetUnit(int id);
    GameObject GetItem(short id);
}

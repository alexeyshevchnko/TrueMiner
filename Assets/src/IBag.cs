using UnityEngine;
using System.Collections;
using trasharia.weapons;

public interface IBag {
    void Init(PlayerModel model);
    void AddBagItem(IItem item);
    void Move(int slotFrom, int slotTo);
    IItem GetItem(int id);
    void Remove(IItem item);
    void UseItem(IItem itemData);
}

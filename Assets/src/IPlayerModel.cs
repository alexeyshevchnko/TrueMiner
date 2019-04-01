using UnityEngine;
using System.Collections;
using trasharia.weapons;

public interface IPlayerModel {
    IWeapon CurrentWeapon { get; }
    IBag GetBag();
    void Init(Transform view,Transform weaponSlo1,  Transform weaponSlo2);
    void UseItem(int bagIndex);
    void SetWeaponSlo1(IItem wepon);
    void SetWeaponSlo2(IItem wepon);
    void RemoveItem(IItem item);

    void SetWepoanById(int id);
    int GetСurrentWeaponId();

    
}

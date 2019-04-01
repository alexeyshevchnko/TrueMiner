using UnityEngine;
using System.Collections;
using trasharia.weapons;

public interface IWeapon {
    void Init(ref IItem data, PlayerModel playerModel);
    IItem GetItem();
    void Fire(Vector2 direct);
    void FireCycle(Vector2 direct);
    void Rotate(Vector2 target);
}

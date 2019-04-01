using UnityEngine;
using System.Collections;

public class GrenadeItem : BaseWeaponItem {
    public override string GetMainPrefubPath() {
        return "Items/grenade";
    }

    public override string GetUiIcon() {
        return "granade1";
    }


    public override int GetId() {
        return 4;
    }


    public override string GetName() {
        return "Граната";
    }
}

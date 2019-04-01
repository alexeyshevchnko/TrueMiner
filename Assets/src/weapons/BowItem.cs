using UnityEngine;
using System.Collections;

public class BowItem : BaseWeaponItem {
    public override string GetMainPrefubPath() {
        return "Items/Bow";
    }

    public override string GetUiIcon() {
        return "bow2";
    }


    public override int GetId() {
        return 3;
    }

    public override string GetName() {
        return "Лук";
    }
}

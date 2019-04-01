using UnityEngine;
using System.Collections;

public class TorchItem : BaseWeaponItem {
    public override string GetMainPrefubPath() {
        return "Items/Torch";
    }

    public override string GetUiIcon() {
        return "torch";
    }

    //собирается в набор количеством
    public override bool IsCountKit() {
        return true;
    }

    public override int GetId() {
        return 6;
    }

    public override string GetName() {
        return "Факел";
    }
}

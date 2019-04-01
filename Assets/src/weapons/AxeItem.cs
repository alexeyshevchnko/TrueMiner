using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeItem : BaseWeaponItem {
    public override string GetMainPrefubPath() {
        return "Items/Axe";
    }

    public override string GetUiIcon() {
        return "Axe";
    }

    public override int GetId() {
        return 7;
    }

    public override string GetName() {
        return "Топор";
    }
}


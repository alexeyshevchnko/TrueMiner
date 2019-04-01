using UnityEngine;
using System.Collections;

public class BackswordItem : BaseWeaponItem {
    public override string GetUiIcon() {
        return "Item1";
    }

    public override string GetMainPrefubPath() {
        return "Items/Backsword";
    }


    public override int GetId() {
        return 1;
    }


    public override string GetName() {
        return "Тесак";
    }
}

using UnityEngine;
using System.Collections;

public class PickItem : BaseWeaponItem {
    public override string GetMainPrefubPath() {
        return "Items/Pick";
    }

    public override string GetUiIcon() {
        return "kirka";
    }

    public override int GetId() {
        return 5;
    }

    public override string GetName() {
        return "Кирка";
    }
}

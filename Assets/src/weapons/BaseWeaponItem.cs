using UnityEngine;
using System.Collections;
using trasharia.weapons;

public abstract class BaseWeaponItem : IItem {
    public int Count { get; set; }
    public abstract string GetUiIcon();
    public abstract string GetMainPrefubPath();

    public virtual void Use(IPlayerModel model) {
        model.SetWeaponSlo1(this);
    }


    public virtual bool IsEqualType(IItem other) {
        if (this.GetType() == other.GetType()) {
            return true;
        }

        return false;
    }

    //собирается в набор количеством
    public virtual bool IsCountKit() {
        return false;
    }

    public virtual int GetId() {
        return 0;
    }

    public virtual string GetName() {
        return "";
    }
}

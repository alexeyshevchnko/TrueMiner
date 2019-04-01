using UnityEngine;
using System.Collections;
using IoC;
using trasharia.weapons;

public class PlayerModel : IPlayerModel {
    [IoC.Inject]
    public IGOPoolManager IgoPoolManager { set; protected get; }
    [IoC.Inject]
    public IContainer Container { set; protected get; }

    public IWeapon CurrentWeapon { get; private set; }
    private IBag bag = new Bag();

    private Transform view;
    private Transform weaponSlo1;
    private Transform weaponSlo2;

    public int UseItemIndex { get; private set; }

    public IBag GetBag() {
        return bag;
    }

    public void Init(Transform view, Transform weaponSlo1, Transform weaponSlo2) {
        this.view = view;
        this.weaponSlo1 = weaponSlo1;
        this.weaponSlo2 = weaponSlo2;
        Container.Inject(bag);
        bag.Init(this);
        
        //Debug.LogError(RootContext.Instance);
        //Debug.LogError(RootContext.Instance.Player);
        //RootContext.Instance.Player.UpdateBag();

        
    }

    public void UseItem(int bagIndex) {
        if (RootContext.Instance.Player.IsShowBag)
            return;

        var item = bag.GetItem(bagIndex);
        if (item != null) {
            item.Use(this);
        } else {
            CurrentWeapon = null;
            ClearSlot(weaponSlo1); 
        }
        
        RootContext.Instance.Player.SelectItem(bagIndex);
        UseItemIndex = bagIndex;
    }

    

    public void RemoveItem(IItem item) {
        bag.Remove(item);
    }

    public void SetWeaponSlo1(IItem weapon) {
        ClearSlot(weaponSlo1);
        if (weapon != null) {
            SetWeapon(weapon, weaponSlo1);
        } else {
            CurrentWeapon = null;
        }
    }

    public void SetWeaponSlo2(IItem weapon) {
        ClearSlot(weaponSlo2);
        if (weapon != null) {
            SetWeapon(weapon, weaponSlo2);
        } else {
            CurrentWeapon = null;
        }
    }

    private void SetWeapon(IItem wepon, Transform slot) {
        var go = Resources.Load(wepon.GetMainPrefubPath()) as GameObject;
        var weapon = GameObject.Instantiate(go);
        weapon.transform.parent = slot;
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = new Quaternion();
        weapon.transform.localScale = Vector3.one;

        CurrentWeapon = weapon.GetComponent<IWeapon>();
        CurrentWeapon.Init(ref wepon, this);
    }

    private void ClearSlot(Transform slot) {
        for (int i = slot.childCount - 1; i >= 0; i--) {
            MonoBehaviour.Destroy(slot.GetChild(i).gameObject);
        }
    }

    public int GetСurrentWeaponId() {
        if (CurrentWeapon == null)
            return 0;

        return CurrentWeapon.GetItem().GetId();
    }

    public void SetWepoanById(int id) {
        IItem item = null;
        switch (id) {
            case  0:
                break;
            case 1:
                item = new BackswordItem() { Count = 9999 };
                break;
            //земля
            case 102:
                item = new BlowBlockItem( 0, 2) { Count = 9999 };
                break;
            //трава
            case 105:
                item = new BlowBlockItem( 0, 5) { Count = 9999 };
                break;
            //угаль
            case 101:
                item = new BlowBlockItem( 0, 1) { Count = 9999 };
                break;
            case 3:
                item = new BowItem() { Count = 9999 };
                break;
            case 4:
                item = new GrenadeItem() { Count = 9999 };
                break;
            case 5:
                item = new PickItem() { Count = 9999 };
                break;
            case 6:
                item = new TorchItem() { Count = 9999 };
                break;
        }


        if (item != null) {
            item.Use(this);
        } else {
            CurrentWeapon = null;
            ClearSlot(weaponSlo1);
        }
    }



   
}

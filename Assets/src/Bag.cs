using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using trasharia.weapons;

public class Bag : IBag {
    [IoC.Inject]
    public IGuiPoolManager GuiPoolManager { set; protected get; }
    [IoC.Inject]
    public ITargetManager TargetManager { set; protected get; }

    public Dictionary<int ,IItem>  items = new Dictionary<int, IItem>();
    private PlayerModel playerModel;
    public void Init(PlayerModel model) {
        playerModel = model;
        for (int i = 1; i <= 45; i++) {
            items.Add(i, null);
        }


        items[1] = new BowItem(){Count =  1};
        items[2] = new BackswordItem() { Count = 1 };
        items[3] = new GrenadeItem() { Count = 999 };
        items[4] = new PickItem() { Count = 1 };
        items[5] = new TorchItem() { Count = 100 };
        items[6] = new BlowBlockItem(0, 2) { Count = 10 };
        
        items[7] = new AxeItem() { Count = 1 };

        items[8] = new BlowBlockItem( 0, (short)TileTypeEnum.Brick) { Count = 100 };
        items[9] = new BlowBlockItem(0, (short)TileTypeEnum.Stairs) { Count = 1000 };
        items[10] = new BlowBlockItem(0, (short)TileTypeEnum.Iron) { Count = 100 };
        items[11] = new BlowBlockItem(0, (short)TileTypeEnum.Meat) { Count = 100 };
        items[12] = new BlowBlockItem(0, (short)TileTypeEnum.Med) { Count = 100 };
        items[13] = new BlowBlockItem(0, (short)TileTypeEnum.Bone) { Count = 100 };

        RootContext.Instance.Player.UpdateBag(this);

        
    }

    public IItem GetItem(int id) {
        if (items.ContainsKey(id)) {
            return items[id];
        }
        return null;
    }

    public void Remove(IItem item) {
        int key = -1;
        foreach (var keyval in items) {
            if (keyval.Value == item) {
                key = keyval.Key;
                break;
            }
        }
        if (key != -1) {
            items[key] = null;
        }

        RootContext.Instance.Player.UpdateBag(this);
    }

    public void UseItem(IItem itemData) {
        
        //UnitManager
        itemData.Count--;
        if (itemData.Count <= 0) {
            playerModel.SetWeaponSlo1(null);
            Remove(itemData);
        }

        RootContext.Instance.Player.UpdateBag(this);
        
    }

    public void AddBagItem(IItem item) {
       // Debug.LogError( "AddBagItem " + item.Count);
        
        var guiLbl = GuiPoolManager.Spawn<GuiLbl>(TargetManager.PlayerController.transform.position + Vector3.up * (50 ), Quaternion.identity).GetComponent<GuiLbl>();
        guiLbl.Init(item.GetName(), item.Count);

        int key = -1;
        bool isNewItem = false;

        if (item.IsCountKit()) {
            foreach (var keyval in items) {
                var other = keyval.Value;
                if (other != null && item.IsEqualType(other)) {
                    other.Count += item.Count;
                    key = keyval.Key;
                    break;
                }
            }
        }

        
        if (key == -1) {
            for (int i = 1; i <= 45; i++) {
                if (items[i] == null) {
                    key = i;
                    items[i] = item;
                    isNewItem = true;
                    break;
                }
            }
        }

        if (isNewItem && key != -1 && playerModel.UseItemIndex == key) {
            playerModel.UseItem(key);
        }

        RootContext.Instance.Player.UpdateBag(this);
    }

    public void Move(int slotFrom, int slotTo) {
        var fromItem = items[slotFrom];
        var toItem = items[slotTo];
        items.Remove(slotFrom);
        items.Remove(slotTo);
        items[slotFrom] = toItem;
        items[slotTo] = fromItem;
    }
}

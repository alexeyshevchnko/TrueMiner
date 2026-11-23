using UnityEngine;
using System.Collections.Generic;
using trasharia.weapons;

public class PlayerComtext : IoC.IInitialize {
    [IoC.Inject]
    public ITargetManager TargetManager { get; set; }

    public int selectId = -1;

    public BagItemContext MoveItem { get; set; }
    public bool IsMoveItem { get; set; }
    public BagContext Bag { get; set; }
    public bool IsShowBag { get; set; }
    public float HpPct { get; set; }
    public float HpMax { get; set; }

    public void ShowBag() {
        if (MoveItem != null) {
            MoveItem.Empty = false;
        }

        MoveItem = null;
        IsMoveItem = false;
        IsShowBag = !IsShowBag;
        if (selectId != -1) {
            SelectItem(selectId);
        }

        if (IsShowBag) {
            UpdateBag((Bag) TargetManager.PlayerController.GetModel().GetBag());
        } else {
            if (selectId != -1) {
                TargetManager.PlayerController.GetModel().UseItem(selectId);
            }
        }
    }

    public void OnInject() {
        IsShowBag = false;
    }

    public void UpdateBag(Bag bag) {
        for (int i = 0; i < 9; i++) {
            UpdateCell(bag, Bag.Items, i, 0);
        }

        if (IsShowBag) {
            for (int i = 9; i <= 4*9 + 9 - 1; i++) {
                UpdateCell(bag, Bag.BagItems, i, 9);
            }
        }
    }

    void UpdateCell(Bag bag, List<BagItemContext> viewBag, int i, int delta) {
        var view = viewBag[i - delta];
        IItem data = null;
        if (bag.items.ContainsKey(i + 1)) {
            data = bag.items[i + 1];
        }
        if (data != null) {
            view.Count = data.Count;
            view.Icon = data.GetUiIcon();
            view.Empty = false;
        } else {
            view.Empty = true;
        }
        view.Number = i + 1;
    }

    public void SelectItem(int id) {
        if (!IsShowBag) {
            UnselectAll();
            Bag.Items[id - 1].BgIcon = "cellSelect";
            selectId = id;
        }
    }

    void UnselectAll() {
        for (int i = 0; i < 9; i++) {
            var view = Bag.Items[i];
            view.BgIcon = "cell";
        }
    }
}
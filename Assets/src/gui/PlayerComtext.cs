using UnityEngine;
using System.Collections;
using trasharia.weapons;

public class PlayerComtext : EZData.Context,IoC.IInitialize {
    [IoC.Inject]
    public ITargetManager TargetManager { set; protected get; }

   // public BagItemContext MoveItem;



    public int selectId = -1;



    #region MoveItem
    public readonly EZData.VariableContext<BagItemContext> MoveItemEzVariableContext = new EZData.VariableContext<BagItemContext>(null);
    public BagItemContext MoveItem {
        get { return MoveItemEzVariableContext.Value; }
        set { MoveItemEzVariableContext.Value = value; }
    }
    #endregion



    #region Property IsMoveItem
    private readonly EZData.Property<bool> _privateIsMoveItemProperty = new EZData.Property<bool>();
    public EZData.Property<bool> IsMoveItemProperty { get { return _privateIsMoveItemProperty; } }
    public bool IsMoveItem {
        get { return IsMoveItemProperty.GetValue(); }
        set { IsMoveItemProperty.SetValue(value); }
    }
    #endregion



    #region Property Bag
    private readonly EZData.Property<BagContext> _privateBagProperty = new EZData.Property<BagContext>();
    public EZData.Property<BagContext> BagProperty { get { return _privateBagProperty; } }
    public BagContext Bag {
        get { return BagProperty.GetValue(); }
        set { BagProperty.SetValue(value); }
    }
    #endregion

    #region Property IsShowBag
    private readonly EZData.Property<bool> _privateIsShowBagProperty = new EZData.Property<bool>();
    public EZData.Property<bool> IsShowBagProperty { get { return _privateIsShowBagProperty; } }
    public bool IsShowBag {
        get { return IsShowBagProperty.GetValue(); }
        set { IsShowBagProperty.SetValue(value); }
    }
    #endregion



    #region Property HpPct
    private readonly EZData.Property<float> _privateHpPctProperty = new EZData.Property<float>();
    public EZData.Property<float> HpPctProperty { get { return _privateHpPctProperty; } }
    public float HpPct {
        get { return HpPctProperty.GetValue(); }
        set { HpPctProperty.SetValue(value); }
    }
    #endregion


    #region Property HpMax
    private readonly EZData.Property<float> _privateHpMaxProperty = new EZData.Property<float>();
    public EZData.Property<float> HpMaxProperty { get { return _privateHpMaxProperty; } }
    public float HpMax {
        get { return HpMaxProperty.GetValue(); }
        set { HpMaxProperty.SetValue(value); }
    }
    #endregion




    public void ShowBag() {
        if (MoveItem != null) {
            MoveItem.Empty = false;
        }

        MoveItem = null;
        IsMoveItem = false;
        IsShowBag = !IsShowBag;
       // UnselectAll();
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

    void UpdateCell(Bag bag, EZData.Collection<BagItemContext> viewBag, int i, int delta) {
        var view = viewBag.GetItem(i - delta);
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
    }


    public void SelectItem(int id) {
        if (!IsShowBag) {
            UnselectAll();
            Bag.Items.GetItem(id - 1).BgIcon = "cellSelect";
            selectId = id;
        }
    }

    void UnselectAll() {
        for (int i = 0; i < 9; i++) {
            var view = Bag.Items.GetItem(i);
            view.BgIcon = "cell";
        }
    }

    
}

using UnityEngine;
using System.Collections;

public class BagItemContext : EZData.Context {

    [IoC.Inject]
    public ITargetManager TargetManager { set; protected get; }

    #region Property Icon
    private readonly EZData.Property<string> _privateIconProperty = new EZData.Property<string>();
    public EZData.Property<string> IconProperty { get { return _privateIconProperty; } }
    public string Icon {
        get { return IconProperty.GetValue(); }
        set { IconProperty.SetValue(value); }
    }
    #endregion

    #region Property Count
    private readonly EZData.Property<int> _privateCountProperty = new EZData.Property<int>();
    public EZData.Property<int> CountProperty { get { return _privateCountProperty; } }
    public int Count {
        get { return CountProperty.GetValue(); }
        set { CountProperty.SetValue(value); }
    }
    #endregion

    #region Property Number
    private readonly EZData.Property<int> _privateNumberProperty = new EZData.Property<int>();
    public EZData.Property<int> NumberProperty { get { return _privateNumberProperty; } }
    public int Number {
        get { return NumberProperty.GetValue(); }
        set { NumberProperty.SetValue(value); }
    }
    #endregion

    #region Property Empty
    private readonly EZData.Property<bool> _privateEmptyProperty = new EZData.Property<bool>();
    public EZData.Property<bool> EmptyProperty { get { return _privateEmptyProperty; } }
    public bool Empty {
        get { return EmptyProperty.GetValue(); }
        set { EmptyProperty.SetValue(value); }
    }
    #endregion

    #region Property BgIcon
    private readonly EZData.Property<string> _privateBgIconProperty = new EZData.Property<string>();
    public EZData.Property<string> BgIconProperty { get { return _privateBgIconProperty; } }
    public string BgIcon {
        get { return BgIconProperty.GetValue(); }
        set { BgIconProperty.SetValue(value); }
    }
    #endregion


    PlayerComtext playerComtext {
        get { return RootContext.Instance.Player; }
    }

    IBag bag {
        get { return TargetManager.PlayerController.GetModel().GetBag(); }
    }

    public void Click() {
        //Debug.LogError("Click");


        if (playerComtext.IsShowBag) {
            if (playerComtext.MoveItem == null) {
                if (!Empty) {
                    Get();
                }
            } else {
                Set();
            }
        } else {
            TargetManager.PlayerController.GetModel().UseItem(Number);    
        }
    }


    public void Get() {
        Empty = true;
        playerComtext.MoveItem = this;
        playerComtext.IsMoveItem = true;
    }


    public void Set() {
        
        bag.Move(playerComtext.MoveItem.Number, Number);
        playerComtext.UpdateBag((Bag)bag);
        playerComtext.MoveItem = null;
        playerComtext.IsMoveItem = false;
         
        

    }
}

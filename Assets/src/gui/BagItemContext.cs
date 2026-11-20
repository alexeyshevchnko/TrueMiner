using UnityEngine;

public class BagItemContext {

    [IoC.Inject]
    public ITargetManager TargetManager { set; protected get; }

    public string Icon { get; set; }
    public int Count { get; set; }
    public int Number { get; set; }
    public bool Empty { get; set; }
    public string BgIcon { get; set; }

    PlayerComtext playerComtext {
        get { return RootContext.Instance.Player; }
    }

    IBag bag {
        get { return TargetManager.PlayerController.GetModel().GetBag(); }
    }

    public void Click() {
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
        playerComtext.UpdateBag((Bag) bag);
        playerComtext.MoveItem = null;
        playerComtext.IsMoveItem = false;
    }
}
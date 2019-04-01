using UnityEngine;
using System.Collections;
using IoC;

public class BagContext : EZData.Context,IoC.IInitialize {

    [IoC.Inject]
    public IContainer Container { set; protected get; }

    #region Collection Items
    private readonly EZData.Collection<BagItemContext> _privateItems = new EZData.Collection<BagItemContext>(false);
    public EZData.Collection<BagItemContext> Items { get { return _privateItems; } }
    #endregion


    #region Collection BagItems
    private readonly EZData.Collection<BagItemContext> _privateBagItems = new EZData.Collection<BagItemContext>(false);
    public EZData.Collection<BagItemContext> BagItems { get { return _privateBagItems; } }
    #endregion


    public void OnInject() {
        
        for (int i = 0; i < 9; i++) {
            var item = new BagItemContext() {Number = i + 1, Empty = true, BgIcon = "cell"};
            Container.Inject(item);
            Items.Add(item);
        }


        for (int i = 0; i < 4*9; i++) {
            var item = new BagItemContext() { Number = i + 10, Empty = true, BgIcon = "cell" };
            Container.Inject(item);
            BagItems.Add(item);
        }
    }
}

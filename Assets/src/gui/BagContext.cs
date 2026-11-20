using UnityEngine;
using System.Collections.Generic;

public class BagContext : IoC.IInitialize {

    [IoC.Inject]
    public IoC.IContainer Container { set; protected get; }

    public List<BagItemContext> Items { get; private set; }
    public List<BagItemContext> BagItems { get; private set; }

    public BagContext() {
        Items = new List<BagItemContext>();
        BagItems = new List<BagItemContext>();
    }

    public void OnInject() {
        for (int i = 0; i < 9; i++) {
            var item = new BagItemContext() { Number = i + 1, Empty = true, BgIcon = "cell" };
            Container.Inject(item);
            Items.Add(item);
        }

        for (int i = 0; i < 4 * 9; i++) {
            var item = new BagItemContext() { Number = i + 10, Empty = true, BgIcon = "cell" };
            Container.Inject(item);
            BagItems.Add(item);
        }
    }
}
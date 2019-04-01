using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemData  {
    public static Dictionary<short, ItemData> Items = new Dictionary<short, ItemData>();

    //факел
    public static ItemData Torch = new ItemData(1, 1, 1, 2, 1, false, @"");
    //флаг
    public static ItemData Flag = new ItemData(2, 1, 1, 2, 1, false, @"");

    public static ItemData Manager = new ItemData(3, 1, 1, 2, 1, false, @"");

    public static ItemData Box = new ItemData(4, 1, 1, 2, 1, false, @"");

    public ItemData(short id, int sizeXmm, int sizeXpp, int sizeYmm, int sizeYpp, bool inWater, string prefub) {
        Id = id;
        SizeXmm = sizeXmm;
        SizeXpp = sizeXpp;
        SizeYmm = sizeYmm;
        SizeYpp = sizeYpp;
        InWater = inWater;
        ItemPrefub = prefub;
        Items.Add(id, this);
    }

    public short Id { get; private set; }
    public int SizeXmm { get; private set; }
    public int SizeXpp { get; private set; }
    public int SizeYmm { get; private set; }
    public int SizeYpp { get; private set; }
    //неможет находится в воде например факел
    public bool InWater { get; private set; }

    public string ItemPrefub { get; private set; }


    public static ItemData GetById(short id) {
        return Items[id];
    }
}

using UnityEngine;
using System.Collections;

public interface IItemManager {
    BlockPoolBagItem CreateBlock(Vector2 worldPos, string tileCollection);
    void CreateItem<T>(Vector2 worldPos) where T : IBasePoolObject;
}

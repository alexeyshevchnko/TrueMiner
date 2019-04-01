using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IPhysicsManager {
    void ChackCollision(IPhysicItem item);
    void ChackRect(IPhysicItem item);
    void RemovePhysicItem(IPhysicItem item);
    List<IPhysicItem> GetItemsInRange(Vector2 center, float radius);
    List<IPhysicItem> GetItemsOfOffset(Vector2Int offset);
}

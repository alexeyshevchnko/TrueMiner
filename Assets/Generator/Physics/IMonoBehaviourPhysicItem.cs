using UnityEngine;
using System.Collections;

public interface IMonoBehaviourPhysicItem {
    void AddVelocity(Vector2 val);
    IPhysicItem GetPhysicItem();
}

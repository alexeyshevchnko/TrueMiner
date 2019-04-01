using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetBase {
    int maxHp { get; set; }
    float currHp { get; set; }

    void Damage(float hp);
    void SetHP(int val);

    Transform GetTransform();
}

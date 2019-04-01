using UnityEngine;
using System.Collections;

public interface IAIBehaviour {
    void Init(IPhysicItem pysicItem, IView view);
    void Update();

    int GetDirection();
    //test
    void Jamp();
    IPhysicItem pysicItem { get; }
}

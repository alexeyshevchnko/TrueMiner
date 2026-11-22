using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GuiPool : GOPool {
    public static GuiPool guiPool; 
    public override void OnInject() {
        //RegisterPool();

        foreach (Transform child in transform) {
            if (child.gameObject.activeInHierarchy)
                Unspawn(child.gameObject);
        }
    }

    protected override void Awake() {
        guiPool = this;
        // Runs the base lifecycle (cache initialisation and this.Inject()); the base OnInject is
        // overridden here and does not register the pool, which ApplicationManager does.
        base.Awake();
    }

}

using System;
using UnityEngine;
using System.Collections;
using trasharia;

public class GameObjectPoolItem : MonoBehaviour {

    public Action<GameObject> onDestroyAction;

    private void OnDestroy() {
        onDestroyAction.TryCall(gameObject);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagBtn : MonoBehaviour {

    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }

    void Start() {
        this.Inject();
    }

    void OnClick() {
        MapGenerator.ChangeMap();
    }

}

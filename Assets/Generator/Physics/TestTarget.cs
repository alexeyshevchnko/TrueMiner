using UnityEngine;
using System.Collections;
using IoC;

public class TestTarget : MonoBehaviour {
    [IoC.Inject]
    public ICollision Collision { set; protected get; }

    void Start() {
        this.Inject();
    }

	
	// Update is called once per frame
	void Update () {
	    if (Collision!=null) {
	        transform.position = Collision.GetTest();
	    }
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchJoystickBtn : MonoBehaviour {
    public TouchInput controller;


    public bool isLeft;
    public bool isRight;
    public bool isJamp;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnPress (bool isDown) {
        if (isLeft) {
            controller.PressLeft(isDown);
        }

        if (isRight) {
            controller.PressRight(isDown);
        }

        if (isJamp ) {

            controller.PressJamp(isDown);
        }
    }


    public void OnSelect(bool isDown) {
        if (isJamp) {

          //  controller.PressJamp(isDown);
        }
    }
}

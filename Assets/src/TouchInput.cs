using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInput : MonoBehaviour {
    public static TouchInput instance;


    public bool isLeft;
    public bool isRight;
    public bool isJampPress;
    public bool isJampDown;


    public void Awake() {
        instance = this;
    }

    public void LateUpdate() {
        isJampDown = false;
    }


    public void PressLeft(bool val) {
        isLeft = val;
    }


    public void PressRight(bool val) {
        isRight = val;
    }

    public void PressJamp(bool val) {
        isJampPress = val;
        isJampDown = val;
    }
}

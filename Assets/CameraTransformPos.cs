using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CameraTransformPos : MonoBehaviour {
    public float delta = 10;

    public TileRenderer tileRenderer;
	
	// Update is called once per frame
	void Update () {
        
	    bool b = false;
        if (Keyboard.current?.upArrowKey.isPressed == true || Keyboard.current?.wKey.isPressed == true) {
            transform.position = new Vector3(transform.position.x, transform.position.y + delta * Time.deltaTime, transform.position.z);
	        b = true;
	    }

        if (Keyboard.current?.downArrowKey.isPressed == true || Keyboard.current?.sKey.isPressed == true) {
            transform.position = new Vector3(transform.position.x, transform.position.y - delta * Time.deltaTime, transform.position.z);
            b = true;
        }

        if (Keyboard.current?.leftArrowKey.isPressed == true || Keyboard.current?.aKey.isPressed == true) {
            transform.position = new Vector3(transform.position.x - delta * Time.deltaTime, transform.position.y, transform.position.z);
            b = true;
        }

        if (Keyboard.current?.rightArrowKey.isPressed == true || Keyboard.current?.dKey.isPressed == true) {
            transform.position = new Vector3(transform.position.x + delta * Time.deltaTime, transform.position.y, transform.position.z);
            b = true;
        }

	    if (b) {
           // tileRenderer.Render();
	    }

	}
}

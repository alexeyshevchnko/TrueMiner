using UnityEngine;
using System.Collections;

public class CameraTransformPos : MonoBehaviour {
    public float delta = 10;

    public TileRenderer tileRenderer;
	
	// Update is called once per frame
	void Update () {
        
	    bool b = false;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
            transform.position = new Vector3(transform.position.x, transform.position.y + delta * Time.deltaTime, transform.position.z);
	        b = true;
	    }

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
            transform.position = new Vector3(transform.position.x, transform.position.y - delta * Time.deltaTime, transform.position.z);
            b = true;
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
            transform.position = new Vector3(transform.position.x - delta * Time.deltaTime, transform.position.y, transform.position.z);
            b = true;
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
            transform.position = new Vector3(transform.position.x + delta * Time.deltaTime, transform.position.y, transform.position.z);
            b = true;
        }

	    if (b) {
           // tileRenderer.Render();
	    }

	}
}

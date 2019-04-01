using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour,IoC.IInitialize {

    [IoC.Inject]
    public ITargetManager TargetManager { set; protected get; }

	void Start () {
	    this.Inject();	
	}

    public void OnInject() {
        
    }

    private static int FrameTrue = 0;

	void FixedUpdate () {
	    if (TargetManager != null) {
            var unitPos = TargetManager.PlayerController.transform.position;
	        var dist = Vector3.Distance(transform.position,unitPos);
            //Debug.LogError(dist);
	        bool isVisible = dist < 100;

	        if (!isVisible && Time.frameCount != FrameTrue || isVisible)
	            MainMenu.instance.ChangeElement(MainMenuElement.Flag, isVisible);

            if (isVisible)
                FrameTrue = Time.frameCount;
	    }
	}


    void OnDisable() {
       // Debug.LogError("OnDisable");
        MainMenu.instance.ChangeElement(MainMenuElement.Flag, false);
    }
    
}

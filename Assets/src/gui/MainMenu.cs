using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {
    public static MainMenu instance;

    public GameObject btnFlag;
	
	void Awake () {
	    instance = this;
	}

    public void ChangeElement(MainMenuElement type, bool visible ) {
        
        switch (type) {
        
            case MainMenuElement.Flag:
                btnFlag.SetActive(visible);
                break;
        }
        
    }


}

public enum MainMenuElement {
    Flag
}

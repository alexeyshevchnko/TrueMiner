using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinerView : MonoBehaviour, IView {
    public GameObject Kirka;
    public GameObject Truck;


    void Start() {
        Kirka.SetActive(false);
        Truck.SetActive(false);
    }

    public void PlayKirka() {
        Kirka.SetActive(true);

        var tween = TweenRotation.Begin(Kirka, UnityEngine.Random.Range(0.1f, 1f), Quaternion.identity);
        tween.style = UITweener.Style.PingPong;
        tween.from = new Vector3(0, 0, 90);
        tween.to = new Vector3(0, 0, -90);
        //tween.SetOnFinished(() => {
           // OnFinishedFire();
       // });

    }

    public void StopKirka() {
        Kirka.SetActive(false);
    }


    public void PlayTruck() {
        Truck.SetActive(true);


    }

    public void StopTruck() {
        Truck.SetActive(false);
    }

	
	
}

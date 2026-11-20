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

        StopCoroutine(KirkaSwing());
        StartCoroutine(KirkaSwing());
        //tween.SetOnFinished(() => {
           // OnFinishedFire();
       // });

    }

    System.Collections.IEnumerator KirkaSwing() {
        float t = 0f;
        while (Kirka != null && Kirka.activeSelf) {
            t += Time.deltaTime * 6f;
            float z = Mathf.Lerp(90f, -90f, Mathf.PingPong(t, 1f));
            Kirka.transform.localRotation = Quaternion.Euler(0f, 0f, z);
            yield return null;
        }
    }

    public void StopKirka() {
        StopCoroutine(KirkaSwing());
        Kirka.SetActive(false);
    }


    public void PlayTruck() {
        Truck.SetActive(true);


    }

    public void StopTruck() {
        Truck.SetActive(false);
    }

	
	
}

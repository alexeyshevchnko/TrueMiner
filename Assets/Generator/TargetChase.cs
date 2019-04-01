using UnityEngine;
using System.Collections;

public class TargetChase : MonoBehaviour {
    [IoC.Inject]
    public MapGenerator mapGenerator { set; protected get; }
    [IoC.Inject]
    public ICooldownManager CooldownManager { set; protected get; }

    public float speed;
    public float oldSpeed;
    private bool inNoTime = false;
    public Transform target;

    void Start() {
        this.Inject();

        mapGenerator.OnChangeMap += OnChangeMap;
    }


    void OnChangeMap() {
        oldSpeed = speed;
        speed = 1000;
        inNoTime = true;
        CooldownManager.AddCooldown(10, null, () => {
            speed = oldSpeed;
            inNoTime = false;
        });
    }

	void FixedUpdate () {

	    
		if (target != null) {
		    if (inNoTime) {
		        transform.position = target.position;

		    } else {
		        var mewPos = Vector3.Lerp(transform.position, target.position, speed);
		        mewPos.z = transform.position.z;
		        transform.position = mewPos;
		    }
		    // transform.position = new Vector3(Mathf.FloorToInt(mewPos.x), Mathf.FloorToInt(mewPos.y), Mathf.FloorToInt(mewPos.z));
		}
	}
}

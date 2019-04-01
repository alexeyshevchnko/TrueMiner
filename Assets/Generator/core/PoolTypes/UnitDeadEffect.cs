using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDeadEffect : MonoBehaviour {
    public List<SpriteRenderer> sprites;
    public List<СircleShell> physicItems;

    private float startTime;

	void Start () {
	    startTime = Time.time;
        foreach (var item in physicItems) {
            item.AddVelocity(new Vector2(Random.Range(-10f,10f),Random.Range(0f,15f)));
        }
		
	}
	


	void FixedUpdate () {
	    var dTime = Time.time - startTime;
	    if (dTime > 20) {
            float alpha = 1 - (dTime - 20) / 5f;
            foreach (var item in sprites) {
                item.color = new Color(item.color.r, item.color.g, item.color.b, alpha);
            }
	    }

	    if (dTime > 25) {
	        Destroy(gameObject);
	    }
	}
}

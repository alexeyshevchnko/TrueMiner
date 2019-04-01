using UnityEngine;
using System.Collections;
using IoC;

public class GetColor : MonoBehaviour,IoC.IInitialize {
    private SpriteRenderer sr;
    [Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }

    public void OnInject() {
        
    }


    void Start() {
	    this.Inject();
	    sr = GetComponent<SpriteRenderer>();
	    if (sr == null) {
	        sr = GetComponentInChildren<SpriteRenderer>();
	    }
	    FixedUpdate();
	}
	
	
	void FixedUpdate () {
	    if(TileDataProvider == null)
            return;
	    
        //transform.position
	    var offse = TileDataProvider.WorldPosToOffsetTile(transform.position);
        if (offse.x >= LightRenderer.minPos.x && offse.y >= LightRenderer.minPos.y &&
            offse.x <= LightRenderer.maxPos.x && offse.y <= LightRenderer.maxPos.y) {

            var lightOffset = new Vector2Int(offse.x - LightRenderer.minPos.x, offse.y - LightRenderer.minPos.y);
            var color =  LightRenderer.color[lightOffset.x, lightOffset.y];
            float val = color.A / 255f;
            Color c = new Color(val, val, val, sr.color.a);
            sr.color = c;
        }

	}

   
}

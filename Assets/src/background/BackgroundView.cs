using UnityEngine;
using System.Collections;
using Managers;

public class BackgroundView : MonoBehaviour,IoC.IInitialize {
    [IoC.Inject]
    public IBackgroundManager BackgroundManager { set; protected get; }

    public TiledSpriteRenderer main;
    public TiledSpriteRenderer main2;
    public TiledSpriteRenderer main3;

    public Camera camera;
    public  float delta = 100;
    public float deltaM = 100;
    public float k1 = 2;
    public float k2 = 1.5f;

    public Color min;
    public Color max;

	void Start () {
	    this.Inject();
	}

    void FixedUpdate() {
        if (BackgroundManager != null) {
            BackgroundManager.FixedUpdate();
        }
    }

    public void OnInject() {
        BackgroundManager.SetView(this);
        BackgroundManager.Init();
    }
}

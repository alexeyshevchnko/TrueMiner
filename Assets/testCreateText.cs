using UnityEngine;
using System.Collections;
using trasharia;

public class testCreateText : MonoBehaviour {
    public Texture2D texture;
    public int sizeX = 130;
    public int sizeY = 80;
    private TileRenderer TileRenderer;

    [IoC.Inject]
    public IRandom Random { set; protected get; }

	// Use this for initialization
    private void Start() {
        texture = new Texture2D(sizeX, sizeY, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        TileRenderer = GetComponentInParent<TileRenderer>();
        TileRenderer.OnSwap += OnSwap;

        gameObject.AddComponent<SpriteRenderer>().sprite = Sprite.Create(texture, new Rect(
            0,
            0,
            130,
            80),
            new Vector2(0.5f, 0.5f),
            1
            );
    }

    // Update is called once per frame
    void OnSwap() {


        for (int x = 0; x <= sizeX; x++) {
            for (int y = 0; y <= sizeY; y++) {
                var t = (byte) Random.Range(0, 255);
                texture.SetPixel(x, y, new Color32(t, t, t, t));
            }
        }
    //    texture.pix
        texture.Apply();
	}
}

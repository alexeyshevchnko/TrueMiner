/* **************************************************************************
 * FPS COUNTER
 * **************************************************************************
 * Written by: Annop "Nargus" Prapasapong
 * Created: 7 June 2012
 * *************************************************************************/

using UnityEngine;
using System.Collections;

/* **************************************************************************
 * CLASS: FPS COUNTER
 * *************************************************************************/
public class FPSCounter : MonoBehaviour {
    /* Public Variables */
    public float frequency = 0.5f;

    /* **********************************************************************
     * PROPERTIES
     * *********************************************************************/
    public int FramesPerSec { get; protected set; }

    /* **********************************************************************
     * EVENT HANDLERS
     * *********************************************************************/
    /*
     * EVENT: Start
     */

    public UILabel GUIText;
    private void Start() {
        GUIText = gameObject.GetComponent<UILabel>();

     //   StartCoroutine(FPS());
    }

    /*
     * EVENT: FPS
     */
   /* private IEnumerator FPS() {
        for (; ; ) {
            // Capture frame-per-second
            int lastFrameCount = Time.frameCount;
            float lastTime = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(frequency);
            float timeSpan = Time.realtimeSinceStartup - lastTime;
            int frameCount = Time.frameCount - lastFrameCount;

            // Display it
            FramesPerSec = Mathf.RoundToInt(frameCount / timeSpan);
            GUIText.text = FramesPerSec.ToString() + " fps";
        }
    }*/
    private float deltaTime = 0.0f;

    private void Update() {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        GUIText.text = string.Format("{0:0.} ", fps);
        GUIText.color = (fps < 20) ? Color.red : Color.green;
     
    }

}
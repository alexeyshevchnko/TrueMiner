using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ParticleScale))]
public class ParticleScaleEditor : Editor {

    private float scale = 1;
    private string tempText;

    public override void OnInspectorGUI() {
        var particleScale = target as ParticleScale;
        if (particleScale == null) {
            return;
        }
        string txt;
        
        scale = EditorGUILayout.FloatField("value", scale);
        
            

            if (GUILayout.Button("SetSacle")) {
                particleScale.SetScale(scale);
            }
        
    }
}

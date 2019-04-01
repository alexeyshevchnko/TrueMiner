using UnityEngine;
using System.Collections;

public class ParticleScale : MonoBehaviour {
    private bool first = true;
    private float defStartStartSize;
    private float defStartSpeed;

    public void SetScale(float value) {
        var ps = GetComponent<ParticleSystem>();

        if (first) {
            defStartStartSize = ps.startSize;
            defStartSpeed = ps.startSpeed;
            first = false;
        }

        ps.startSize = defStartStartSize * value;
        ps.startSpeed = defStartSpeed * value;
    }
}

using UnityEngine;
using System.Collections;

public class ParticleScale : MonoBehaviour {
    private bool first = true;
    private ParticleSystem.MinMaxCurve defStartSize;
    private ParticleSystem.MinMaxCurve defStartSpeed;

    public void SetScale(float value) {
        var ps = GetComponent<ParticleSystem>();

        if (first) {
            defStartSize = ps.main.startSize;
            defStartSpeed = ps.main.startSpeed;
            first = false;
        }

        var main = ps.main;
        main.startSize = Scale(defStartSize, value);
        main.startSpeed = Scale(defStartSpeed, value);
    }

    /// <summary>
    /// РњР°СЃС€С‚Р°Р±РёСЂСѓРµС‚ MinMaxCurve СЃ СЃРѕС…СЂР°РЅРµРЅРёРµРј РёСЃС…РѕРґРЅРѕРіРѕ СЂРµР¶РёРјР° РєСЂРёРІРѕР№
    /// (СЌРєРІРёРІР°Р»РµРЅС‚ РїСЂРµР¶РЅРµРіРѕ ps.startSize * value / ps.startSpeed * value).
    /// </summary>
    private static ParticleSystem.MinMaxCurve Scale(ParticleSystem.MinMaxCurve curve, float scale) {
        switch (curve.mode) {
            case ParticleSystemCurveMode.Constant:
                return new ParticleSystem.MinMaxCurve(curve.constant * scale);
            case ParticleSystemCurveMode.Curve:
                return new ParticleSystem.MinMaxCurve(curve.curveMultiplier * scale, curve.curve);
            case ParticleSystemCurveMode.TwoConstants:
                return new ParticleSystem.MinMaxCurve(curve.constantMin * scale, curve.constantMax * scale);
            case ParticleSystemCurveMode.TwoCurves:
                return new ParticleSystem.MinMaxCurve(curve.curveMultiplier * scale, curve.curveMin, curve.curveMax);
            default:
                return curve;
        }
    }
}

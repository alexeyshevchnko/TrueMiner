using UnityEngine;
using System.Collections;

public class BoomEffect : BasePoolEffect {
    public ParticleSystem OtherParticle;
    public TweenScale tweenScale;

    public override void OnSpawn() {
        this.Inject();
        MainParticle.Play();
        OtherParticle.Play();
        tweenScale.ResetToBeginning();
        
        tweenScale.enabled = true;
        tweenScale.Play();
        CooldownManager.AddCooldown(MainParticle.duration, null, UnSpawn, 0, Time.fixedDeltaTime);

       //GameObjectPoolManager.Spawn<LightBoomEffect>(transform.position, Quaternion.identity);

        for (int i = 0; i < 5; i++) {
            var point =
                IgoPoolManager.Spawn<LightPointEffect>(transform.position, Quaternion.identity)
                    .GetComponent<LightPointEffect>();

            point.Init(5, Random.Range(0.2f, 0.7f), Random.Range(-0.4f, -0.1f), new Vector2(Random.Range(-2f, 2), Random.Range(-2f, 2)));
        }
    }
}

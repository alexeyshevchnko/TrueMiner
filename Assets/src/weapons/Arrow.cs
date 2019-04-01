using UnityEngine;
using System.Collections;

public class Arrow : Shell {

    public override void OnInject() {
        base.OnInject();
        pysicItem.OnTerrainCollision += OnTerrainCollision;
        pysicItem.OnCollision += OnCollision;
    }

    public override void OnDestroy() {
        if (pysicItem != null) {
            pysicItem.OnTerrainCollision -= OnTerrainCollision;
            pysicItem.OnCollision -= OnCollision;
        }
        base.OnDestroy();
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (PhysicsManager != null && pysicItem != null) {
            PhysicsManager.ChackCollision(pysicItem);
        }
    }

    private void OnCollision(IPhysicItem item) {
        var ai = item.View.GetComponent<AiController>();
        if (ai != null) {
            ai.behaviour.pysicItem.AddVelocity(pysicItem.velocity.normalized * 4);
            ai.Damage(10);
            IgoPoolManager.Spawn<BloodEffect>(ai.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    void OnTerrainCollision() {
        Destroy(gameObject);

        //эффект сломоной стрелы
        IgoPoolManager.Spawn<ArrowEffect>(transform.position, Quaternion.identity);

        //световой эфект
        for (int i = 0; i < 4; i++) {
            var point =
                IgoPoolManager.Spawn<LightPointEffect>(transform.position, Quaternion.identity)
                    .GetComponent<LightPointEffect>();

            point.Init(4, Random.Range(0.1f, 0.4f), Random.Range(-0.4f, -0.1f),
                new Vector2(Random.Range(-2f, 2), Random.Range(5f, 2)));
        }
    }
}

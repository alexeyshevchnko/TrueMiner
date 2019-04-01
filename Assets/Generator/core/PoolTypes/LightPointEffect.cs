using UnityEngine;
using System.Collections;

public class LightPointEffect : BasePoolEffect {
    public MonoBehaviourPhysicItem MonoBehaviourPhysicItem;
    public SetColor SetColor;
    private float maxColorSize;
    private ICooldownItem cooldown;

    public override void OnSpawn() {
        this.Inject();
    }



    public void Init(float dutarion, float maxColorSize, float gravityY, Vector2 startVelocity) {
        this.maxColorSize = maxColorSize;
        var physicItem = MonoBehaviourPhysicItem.GetPhysicItem();
        physicItem.SetGravity(new Vector2(0, gravityY));
        physicItem.SetPosition(transform.position);
        physicItem.AddVelocity(-physicItem.velocity);
        physicItem.AddVelocity(startVelocity);
        physicItem.IsCollisionTerrain = false;
        physicItem.IsReactExplosion = false;
        cooldown = CooldownManager.AddCooldown(dutarion, CooldownUpdate, UnSpawn, 0, Time.fixedDeltaTime);
    }

    void CooldownUpdate() {
        SetColor.colorSize = Mathf.Lerp(maxColorSize, 0, cooldown.GetPCT());
    }

}

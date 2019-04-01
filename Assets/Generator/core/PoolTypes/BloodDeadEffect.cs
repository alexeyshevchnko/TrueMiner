using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodDeadEffect : BasePoolEffect {
    public AnimationClip anim;

    public override void OnSpawn() {
        
        this.Inject();
        CooldownManager.AddCooldown(anim.length, null, UnSpawn, 0, Time.fixedDeltaTime);
    }

}

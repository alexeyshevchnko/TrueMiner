using UnityEngine;
using System.Collections;

public class LightBoomEffect : BasePoolEffect {
    public Animation anim;


    public override void OnSpawn() {
        this.Inject();
        CooldownManager.AddCooldown(anim["LightEffect4"].length, null, UnSpawn, 0, Time.fixedDeltaTime);

        

        
    }
     
}
 
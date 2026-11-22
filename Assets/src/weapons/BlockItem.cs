using UnityEngine;
using System.Collections;
//using UnityEditor;

//на блоки вешается управляет физикой
public class BlockItem : MonoBehaviourPhysicItem {
    // MapGenerator унаследован от MonoBehaviourPhysicItem (повторное свойство
    // скрывало базовое — CS0108; внедрение IoC идёт по базовому свойству).

    public override void OnInject() {
        
        var sr = GetComponent<SpriteRenderer>();
        size = new Vector2(sr.sprite.bounds.size.x, sr.sprite.bounds.size.y);
        gravity = new Vector2(0, -0.4f);
        
        base.OnInject();
      
        
    }
}

using UnityEngine;
using System.Collections;
//using UnityEditor;

//на блоки вешается управляет физикой
public class BlockItem : MonoBehaviourPhysicItem {
    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }

    public override void OnInject() {
        
        var sr = GetComponent<SpriteRenderer>();
        size = new Vector2(sr.sprite.bounds.size.x, sr.sprite.bounds.size.y);
        gravity = new Vector2(0, -0.4f);
        
        base.OnInject();
      
        
    }
}

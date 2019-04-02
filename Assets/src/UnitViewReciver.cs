using UnityEngine;
using System.Collections;
using IoC;

public class UnitViewReciver : MonoBehaviour,IoC.IInitialize {
    [IoC.Inject]
    public IContainer Container { set; protected get; }
//    [IoC.Inject]
//    public IPhysicsManager PhysicsManager { set; protected get; }

    public Vector2 size;
    public IPhysicItem pysicItem;
   public Vector2 gravity  = new Vector2(0, -2.4f);
    Animator anim;

    public void OnDestroy() {
        if (pysicItem != null) {
            pysicItem.Destroy();
        }
    }

    void Awake() {
        pysicItem = new PhysicItem(this);
        
        pysicItem.SetGravity(gravity);
    }

    void Start() {
        
        anim = GetComponent<Animator>();
        anim.SetBool("walk", false);
        anim.SetBool("dead", false);
        anim.SetBool("jump", false);
        this.Inject();
    }
    

    public void OnInject() {
        
        pysicItem.SetPosition(transform.position);
        Container.Inject(pysicItem);

        pysicItem.itemWidth = size.x;
        pysicItem.itemHeight = size.y;
    }

    void ChackAnim() {
        if (Mathf.Abs(pysicItem.velocity.x) < 0.01f && Mathf.Abs(pysicItem.velocity.y) < 0.01f) {
            anim.SetBool("walk", false);
            anim.SetBool("dead", false);
            anim.SetBool("jump", false);
        }

        if (Mathf.Abs(pysicItem.velocity.x) >= 0.01f) {
            anim.SetBool("walk", true);
            anim.SetBool("dead", false);
            anim.SetBool("jump", false);
        }

        if (Mathf.Abs(pysicItem.velocity.y) >= 0.01f) {
            anim.SetBool("jump", true);
            anim.SetBool("dead", false);
        }
    }

    public void PlayAttck() {
        anim.SetBool("walk", false);
        anim.SetBool("dead", false);
        anim.SetBool("jump", false);
        anim.SetBool("attack", true);
    }

    private void FixedUpdate() {

        if (Container == null)
            return;
        anim.SetBool("dead", false);
        //Debug.LogError(anim.GetBool("dead"));

        ChackAnim();
        pysicItem.Update();
        transform.position = pysicItem.Position;

      //  PhysicsManager.ChackCollision(pysicItem);
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Vector3 tl = new Vector3(transform.position.x - size.x / 2, transform.position.y + size.y / 2, transform.position.z);
        Vector3 dl = new Vector3(transform.position.x - size.x / 2, transform.position.y - size.y / 2, transform.position.z);
        Vector3 tr = new Vector3(transform.position.x + size.x / 2, transform.position.y + size.y / 2, transform.position.z);
        Vector3 dr = new Vector3(transform.position.x + size.x / 2, transform.position.y - size.y / 2, transform.position.z);
        Gizmos.DrawLine(tl, dl);
        Gizmos.DrawLine(tr, dr);

        Gizmos.DrawLine(tl, tr);
        Gizmos.DrawLine(dl, dr);
    }
}

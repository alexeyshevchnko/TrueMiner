using UnityEngine;
using System.Collections;
using IoC;

public class MonoBehaviourPhysicItem : MonoBehaviour, IoC.IInitialize, IMonoBehaviourPhysicItem {
    public Vector2 size;
    //public float Speed = 1;
    protected IPhysicItem pysicItem;
    public float ReboundFactor;
    [IoC.Inject]
    public IContainer Container { set; protected get; }
    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }
    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }
    [IoC.Inject]
    public ICollision Collision { set; protected get; }
    [IoC.Inject]
    public IPhysicsManager PhysicsManager { set; protected get; }

    [IoC.Inject]
    public IGOPoolManager IgoPoolManager { set; protected get; }

    public Vector2 momentVector = new Vector2(0.9f, 0.99f);
    public Vector2 gravity = new Vector2(0, -2.4f);

    void Awake() {
        pysicItem = new PhysicItem(this);
    }

    void Start() {
        this.Inject();
    }

    public virtual void OnInject() {
        Container.Inject(pysicItem);

        pysicItem.itemWidth = size.x;
        pysicItem.itemHeight = size.y;

        pysicItem.SetMomentVector(momentVector);
        pysicItem.SetGravity(gravity);
        pysicItem.SetPosition(transform.position);
        pysicItem.ReboundFactor = ReboundFactor;
    }

    protected virtual void Update() {
        if (Container == null)
            return;
    }

    protected Vector2 DeltaPosition;

    protected virtual void FixedUpdate() {
        if (Container == null || pysicItem.Sleep)
            return;

        
        DeltaPosition = pysicItem.GetPosition() - (Vector2)transform.position;
        transform.position = pysicItem.GetPosition();

        RotateOnDirection();
        pysicItem.Update();
    }

    protected virtual float MinVelocity {
        get { return 0.5f; }
    }

    protected bool IsMinVelocity() {
        return Vector2.Distance(pysicItem.velocity, Vector2.zero) > MinVelocity;
    }

    protected virtual void RotateOnDirection() {
        if (IsMinVelocity()) {
            var target = (Vector2) transform.position + pysicItem.velocity;

            Vector3 target3 = new Vector3(target.x, target.y, transform.position.z);
            Vector3 vectorToTarget = target3 - transform.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x)*Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
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

    public void AddVelocity(Vector2 val) {
        pysicItem.AddVelocity(val);
    }

    public virtual void OnDestroy() {
        if (pysicItem != null) {
            pysicItem.Destroy();
        }

        
    }

    public void OnDisable() {
        if (pysicItem != null) {
            //pysicItem.UnRegister();
        }
    }

    public void OnEnable() {
        if (pysicItem != null) {
            pysicItem.SetSleep(false);
        }
    }

    public IPhysicItem GetPhysicItem() {
        return pysicItem;
    }
}
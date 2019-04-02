using UnityEngine;
using System.Collections;
using trasharia;

public class ExplosionShell : MonoBehaviour, IoC.IInitialize {
    [IoC.Inject]
    public ITileDataProvider tileDataProvider { set; protected get; }

    [IoC.Inject]
    public ICooldownManager CooldownManager { set; protected get; }

   // [IoC.Inject]
   // public ILightRendererOld LightRendererOld { set; protected get; }

    [IoC.Inject]
    public IPhysicsManager PhysicsManager { set; protected get; }

    [IoC.Inject]
    public ITargetManager TargetManager { set; protected get; }

    [IoC.Inject]
    public IGOPoolManager IgoPoolManager { set; protected get; }

    [IoC.Inject]
    public IRandom Random { set; protected get; }

    private LightPoint lightPoint;

    public float cooldown;

    public float damage;

	void Start () {
        lightPoint = new LightPoint();
	    lightPoint.SetSize(12);
	    lightPoint.SetPosition(transform.position);

	    this.Inject();
      
	}

    void Update() {
        lightPoint.SetPosition(transform.position);
        lightPoint.SetSize(Mathf.Lerp(lightPoint.Size, 3, Time.deltaTime));
    }
	
    public void OnInject() {
        CooldownManager.AddCooldown(cooldown, null, Explosion);
      //  LightRendererOld.AddPoint(lightPoint);
    }

    private Vector2 pos;
    private float floarRange;
    private void Explosion() {
        var tileSize = tileDataProvider.TileSize;
        pos = transform.position;
        var tileRange = Random.Range(1, 5);
        floarRange = tileRange + tileSize;


        tileDataProvider.СircleDamageTile(pos.x + 1.5f * tileSize, pos.y + 1.5f * tileSize, 0, 0, 0, tileRange, 100);
        IgoPoolManager.Spawn<BoomEffect>(transform.position, Quaternion.identity);


        findObjects = true;
       

       
    }

    private bool findObjects = false;
    void FixedUpdate() {
        if (findObjects) {
            var physicItems = PhysicsManager.GetItemsInRange(pos, floarRange * 2);
            //Debug.LogError(physicItems.Count);

            foreach (var item in physicItems) {
                if (item.IsReactExplosion) {
                    Vector2 addVelosity = item.Position - (Vector2) pos;
                    //addVelosity.normalized
                    item.AddVelocity(50*addVelosity.normalized);
                }
            }  
            findObjects = false;
            Destroy(gameObject);

            TargetManager.ExplosionDamage(pos, floarRange * 5, damage);
        }
    }
}

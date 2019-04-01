using UnityEngine;
using System.Collections;
using IoC;

public class AiController : MonoBehaviour, IoC.IInitialize, ITargetBase {

    [IoC.Inject]
    public IContainer Container { set; protected get; }
    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }

    [IoC.Inject]
    public IGuiPoolManager GuiPoolManager { set; protected get; }

    [IoC.Inject]
    public ITargetManager TargetManager { set; protected get; }

    [IoC.Inject]
    public IGOPoolManager IgoPoolManager { set; protected get; }

    public IAIBehaviour behaviour;

    public GameObject DeadObj;

    private IView View;
    /*
    void Start() {
        this.Inject();
    }*/

    public void OnInject() {
        View = GetComponent<IView>();
    }

    public void Init(Vector2Int pos) {
        var viewReciver = GetComponent<UnitViewReciver>();

        transform.position = new Vector3(
            pos.x*TileDataProvider.TileSize,
            pos.y*TileDataProvider.TileSize + 3,
            transform.position.z);
        viewReciver.pysicItem.SetPosition(transform.position);


        maxHp = 100;
        currHp = 100;
    }

    public void InitBaseBehaviour() {
        var viewReciver = GetComponent<UnitViewReciver>();
        behaviour = new BaseAIBehaviour();
        Container.Inject(behaviour);
        behaviour.Init(viewReciver.pysicItem, View);
    }

    public void InitZombieBehaviour() {
        var viewReciver = GetComponent<UnitViewReciver>();
        behaviour = new ZombieBehaviour();
        Container.Inject(behaviour);
        behaviour.Init(viewReciver.pysicItem, View);
    }


    public void InitMinerBehaviour() {
        var viewReciver = GetComponent<UnitViewReciver>();
        behaviour = new MinerBehaviour();
        Container.Inject(behaviour);
        behaviour.Init(viewReciver.pysicItem, View);
    }

    private void FixedUpdate() {
        if (Container == null)
            return;

        if (behaviour != null) {
            behaviour.Update();
            if (oldDir != behaviour.GetDirection()) {
                oldDir = behaviour.GetDirection();
                Rotat(oldDir);
            }
        }
    }


    private int oldDir = 0;
    void Rotat(float direct) {
        if (direct > 0) {
            transform.rotation = new Quaternion();
        } else {
            if (direct < 0) {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }

    public int maxHp { get; set; }

    public float currHp {get;set; }

    public void Damage(float hp) {


        currHp = Mathf.Clamp(currHp - hp, 0, maxHp);
      //  RootContext.Instance.Player.HpMax = maxHp;
      //  RootContext.Instance.Player.HpPct = currHp / (float)maxHp;

       //if (hp > 0)
        //    GameObjectPoolManager.Spawn<BloodEffect>(transform.position, Quaternion.identity);

        var guiLbl = GuiPoolManager.Spawn<GuiLbl>(transform.position + Vector3.up * (50 + Random.Range(0,50)), Quaternion.identity).GetComponent<GuiLbl>();
        guiLbl.Init(Color.white, ((int)hp).ToString());

        if (currHp == 0) {
            TargetManager.DestroyUnit(this);

            //световой эффект
            for (int i = 0; i < 5; i++) {
                var point =
                    IgoPoolManager.Spawn<LightPointEffect>(transform.position, Quaternion.identity)
                        .GetComponent<LightPointEffect>();

                point.Init(5, Random.Range(0.2f, 0.7f), Random.Range(-0.4f, -0.1f),
                    new Vector2(Random.Range(-2f, 2), Random.Range(5f, 2)));
            }

            //GameObjectPoolManager.Spawn<BloodDeadEffect>(transform.position, Quaternion.identity);
            var dead = GameObject.Instantiate(DeadObj, transform.position, Quaternion.identity);
            var parent = GameObject.Find("Units");
            dead.transform.parent = parent.transform;
        }
    }

    public void SetHP(int val) {
        currHp = val;
      //  RootContext.Instance.Player.HpMax = maxHp;
      //  RootContext.Instance.Player.HpPct = currHp / (float)maxHp;
    }

    public Transform GetTransform() {
        return transform;
    }
}

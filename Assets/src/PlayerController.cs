using UnityEngine;
using System.Collections;
using GoogleMobileAds.Api;
using IoC;
using trasharia;

public class PlayerController : MonoBehaviour, IoC.IInitialize, ITargetBase {
    public int Id;

    public IPhysicItem pysicItem;

    [IoC.Inject]
    public IContainer Container { set; protected get; }
    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }
    [IoC.Inject]
    public IGOPoolManager IgoPoolManager { set; protected get; }

    [IoC.Inject]
    public ICooldownManager CooldownManager { set; protected get; }
    [IoC.Inject]
    public IGuiPoolManager GuiPoolManager { set; protected get; }
  

    [IoC.Inject]
    public IRandom Random { set; protected get; }

    public UnitViewReciver viewReciver;
    
    private IPlayerModel model = new PlayerModel();

    private Vector2Int spawnPos;

    public GameObject DeadObj;

    public IPlayerModel GetModel() {
        return model;
    }

    private LightPoint lightPoint;
    private bool isJamp = false;

    void Start() {
        viewReciver = GetComponent<UnitViewReciver>();
        pysicItem = viewReciver.pysicItem;

        lightPoint = new LightPoint();
        lightPoint.SetSize(32);
    }

    public void OnInject() {
        
        Container.Inject(model);
        model.Init(transform, transform.Find("L"), transform.Find("R"));
       // Damage(0);
        SetHP(100);
        //LightRendererOld.AddPoint(lightPoint);
    }

    public void SetPos(Vector2Int pos) {
        spawnPos = pos;
        viewReciver = GetComponent<UnitViewReciver>();
        pysicItem = viewReciver.pysicItem;
        transform.position = new Vector3(pos.x * TileDataProvider.TileSize, pos.y * TileDataProvider.TileSize + 3, transform.position.z);
        pysicItem.SetPosition(transform.position);
    }

    public void Init(Vector2Int pos) {
        spawnPos = pos;
        viewReciver = GetComponent<UnitViewReciver>();
        pysicItem = viewReciver.pysicItem;

        transform.position = new Vector3(pos.x * TileDataProvider.TileSize, pos.y * TileDataProvider.TileSize + 3, transform.position.z);
        pysicItem.SetPosition(transform.position);
        pysicItem.OnCollision += OnCollision;

        maxHp = 100;
        currHp = 100;

      //  TargetManager.Player = this;
    }

    void OnCollision(IPhysicItem item) {
     

        var bagItem = item.View.GetComponent<IPoolBagItem>();
        if (bagItem != null) {
            model.GetBag().AddBagItem(bagItem.GetBagItem());
            IgoPoolManager.Unspawn(item.View.gameObject);
            return;
        }


        var ai = item.View.GetComponent<AiController>();
        if (ai != null) {
            if (Time.time - oldAiCollisionT >0.5f) {
                oldAiCollisionT = Time.time;


                var direcr = Mathf.Sign(transform.position.x - ai.transform.position.x);
                var vect = transform.position - ai.transform.position;
                pysicItem.AddVelocity(-pysicItem.velocity);
                pysicItem.AddVelocity(vect.normalized * 14);//new Vector2(direcr*7, 8));

                ai.GetComponent<UnitViewReciver>().PlayAttck();

                Damage(5);
                TweenAlpha(0.2f, true);
                CooldownManager.AddCooldown(0.5f, null, () => {
                    TweenAlpha(0.2f, false);
                });


            }
            ;
           // ai.behaviour.Jamp();
            return;
        }
    }


    public void TweenAlpha(float duration, bool play) {
        var sprites = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sprite in sprites) {
            
            TweenAlpha tween = sprite.gameObject.GetComponent<TweenAlpha>();
            if (tween == null && play) {
                tween = sprite.gameObject.AddComponent<TweenAlpha>();
                tween.from = 0.3f;
                tween.to = 1;
                tween.style = UITweener.Style.PingPong;
            }

            if (tween != null) {
                tween.enabled = play;
            }

            if (play) {
                tween.duration = duration;
                tween.PlayReverse();
            }

            if (!play) {
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1);
            }
        }
    }

    private float oldAiCollisionT = 0;

    public void OnDestroy() {
        if (pysicItem != null) {
            pysicItem.OnCollision -= OnCollision;
            pysicItem.Destroy();
        }
    }


    public void SyncPhysicItem(byte[] data) {
        pysicItem.Sync(data);
    }

    public float direct;

    public void Rotat(float direct) {
        if (direct > 0) {
            transform.rotation = new Quaternion();
        } else {
            if (direct < 0) {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
        this.direct = direct;
    }


    private RewardBasedVideoAd rewardBasedVideo;

    public void ReSpawnFromDead() {

        this.rewardBasedVideo = RewardBasedVideoAd.Instance;
        if (rewardBasedVideo.IsLoaded())
        {
            rewardBasedVideo.Show();
        }

        gameObject.SetActive(false);
        CooldownManager.AddCooldown(3, null, () => {
            pysicItem.AddVelocity(-pysicItem.velocity);
            transform.position = new Vector3(spawnPos.x * TileDataProvider.TileSize, spawnPos.y * TileDataProvider.TileSize + 3, transform.position.z);
            pysicItem.SetPosition(transform.position);
            SetHP(100);
            gameObject.SetActive(true);    


        });
    }

    //ITargetBase
    public int maxHp { get; set; }
    public float currHp { get; set; }
    public void Damage(float hp) {
        currHp = Mathf.Clamp(currHp - hp, 0, maxHp);
        RootContext.Instance.Player.HpMax = maxHp;
        RootContext.Instance.Player.HpPct = currHp / (float)maxHp;

        if (hp > 0)
        IgoPoolManager.Spawn<BloodEffect>(transform.position, Quaternion.identity);

        var guiLbl = GuiPoolManager.Spawn<GuiLbl>(transform.position + Vector3.up * (50 + Random.Range(0, 50)), Quaternion.identity).GetComponent<GuiLbl>();
        guiLbl.Init(Color.red, ((int)hp).ToString());

        if (currHp == 0) {
            ReSpawnFromDead();

            for (int i = 0; i < 5; i++) {
                var point =
                    IgoPoolManager.Spawn<LightPointEffect>(transform.position, Quaternion.identity)
                        .GetComponent<LightPointEffect>();

                point.Init(5, Random.Range(0.2f, 0.7f), Random.Range(-0.4f, -0.1f),
                    new Vector2(Random.Range(-2f, 2), Random.Range(5f, 2)));
            }

           // GameObjectPoolManager.Spawn<BloodDeadEffect>(transform.position, Quaternion.identity);
            var dead = GameObject.Instantiate(DeadObj, transform.position, Quaternion.identity);
            var parent = GameObject.Find("Units");
            dead.transform.parent = parent.transform;
        }
    }
    

    public void SetHP(int val) {
        currHp = val;
        RootContext.Instance.Player.HpMax = maxHp;
        RootContext.Instance.Player.HpPct = currHp / (float)maxHp;
    }

    public Transform GetTransform() {
        return transform;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IoC;
using JetBrains.Annotations;
using trasharia.weapons;

//сикира
public class Backsword : MonoBehaviour, IWeapon {
    
    public Transform view;
    public Transform firePoint;
    public float cooldown;
    [IoC.Inject]
    public ICooldownManager CooldownManager { set; protected get; }
    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }

   

    private TileInfoStructure[,] Map {
        get {
            return MapGenerator.GetMap();
        }
    }

    private IItem itemData;
    private DamagePoint damagePoint;

    public float Damage;

    void Start() {
        this.Inject();

        damagePoint = firePoint.gameObject.GetComponent<DamagePoint>();

        if (damagePoint == null) {
            damagePoint = firePoint.gameObject.AddComponent<DamagePoint>();
        }
        damagePoint.enabled = false;



        damagePoint.damageHP = Damage;
      
    }

    public void Init(ref IItem data, PlayerModel playerModel) {
        itemData = data;
    }

    public IItem GetItem() {
        return itemData;
    }

    private bool isFireing = false;
    public void Fire(Vector2 direct, bool isTouch)
    {
        if(isFireing)
            return;
        isFireing = true;
        var tween = TweenRotation.Begin(gameObject, cooldown, Quaternion.identity);
        tween.from = new Vector3(0, 0, 90);
        tween.to = new Vector3(0, 0, -90);
        tween.SetOnFinished(() => {
            OnFinishedFire();
        });

        damagePoint.enabled = true;
    }

    public void FireCycle(Vector2 direct, bool isTouch)
    {
        
    }

    void OnFinishedFire() {
        isFireing = false;
        transform.localRotation = new Quaternion();
        damagePoint.enabled = false;
    }

    public void Rotate(Vector2 target) {

    }

}


public class DamagePoint : DamagePointBase {
    [IoC.Inject]
    public IPhysicsManager PhysicsManager { set; protected get; }
    public float damageHP;

    List<int> DamagableIds = new List<int>();


    protected override void OnEnable() {
        base.OnEnable();
        DamagableIds.Clear();
    }

    private void FixedUpdate() {

        var list = PhysicsManager.GetItemsInRange(transform.position, 16f);
        //   Debug.LogError(list.Count);
        foreach (var item in list) {
            var ai = item.View.GetComponent<AiController>();
            if (ai != null) {
                DoDamage(ai);
            }
        }

    }

    private void DoDamage(AiController ai) {
        if (DamagableIds.IndexOf(ai.GetInstanceID()) == -1) {
            DamagableIds.Add(ai.GetInstanceID());

            var dir = new Vector2(TargetManager.PlayerController.direct, 0);
            ai.behaviour.pysicItem.AddVelocity(dir * 8);
            //ai.behaviour.pysicItem.AddVelocity(TargetManager.PlayerController.pysicItem.velocity.normalized * 4);

            ai.Damage(damageHP);

            
            //ai.behaviour.pysicItem.AddVelocity(transform.forward * 4);
            
            IgoPoolManager.Spawn<BloodEffect>(ai.transform.position, Quaternion.identity);

        }
    }
}
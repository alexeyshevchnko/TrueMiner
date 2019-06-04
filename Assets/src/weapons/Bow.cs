using UnityEngine;
using System.Collections;
using trasharia.weapons;

//Лук
public class Bow : MonoBehaviour, IWeapon {
    public Transform view;
    public float maxSpeed = 5;
    public GameObject ArrowPref;
    public Transform firePoint;
    public float cooldown = 0.1f;
    private float lastTimeFire = 0;


    [IoC.Inject]
    public ICooldownManager CooldownManager { set; protected get; }

    private IItem itemData;

    private Transform rootPos;

    public void Init(ref IItem data, PlayerModel playerModel) {
        itemData = data;
        rootPos = transform.transform.GetComponentInParent<PlayerController>().transform;
        var go = UltimateJoystick.GetJoystick("Joystick2");
        go.SetActive(true);
    }

    public IItem GetItem() {
        return itemData;
    }

    void Start() {
        this.Inject();
    }

    private ICooldownItem cooldownShow;

    public void Fire(Vector2 direct, bool isTouch)
    {
        if (Time.time - lastTimeFire < cooldown)
            return;
        lastTimeFire = Time.time;

        Show(0.1f);

        var go = GameObject.Instantiate(ArrowPref, firePoint.position, firePoint.rotation) as GameObject;
        go.transform.parent = view.root;
        var item = go.GetComponent<IMonoBehaviourPhysicItem>();

        Vector3 direct3 = new Vector3(direct.x, direct.y, firePoint.position.z);

        
        
        if (isTouch)
        {
            var h = UltimateJoystick.GetHorizontalAxis("Joystick2");
            var v = UltimateJoystick.GetVerticalAxis("Joystick2");
            var vPos = new Vector3(h, v) * 100;
            direct3 = vPos + rootPos.position;
            direct3 = new Vector3(direct3.x, direct3.y, firePoint.position.z);
        }
        


       

        item.AddVelocity((direct3 - firePoint.position).normalized * maxSpeed);
    }

    public void FireCycle(Vector2 direct, bool isTouch)
    {
        
    }

    void Show(float duration) {
        if (cooldownShow != null) {
            CooldownManager.RemoveCooldown(cooldownShow);
        }
        view.gameObject.SetActive(true);
        cooldownShow = CooldownManager.AddCooldown(duration, null, () => {
            view.gameObject.SetActive(false);
            cooldownShow = null;
        });
    }

    void OnDestroy() {
        if (cooldownShow != null) {
            CooldownManager.RemoveCooldown(cooldownShow);
        }
    }

    public void Rotate(Vector2 target) {

        Vector3 target3 = new Vector3(target.x, target.y, view.position.z);

        Vector3 vectorToTarget = target3 - view.position;
        if (Vector3.Angle(view.parent.parent.right, vectorToTarget) > 90) {
            return;
        }

        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        view.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

    }
}

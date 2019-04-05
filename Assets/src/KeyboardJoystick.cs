using System.Collections;
using System.Collections.Generic;
using IoC;
using trasharia;
using UnityEngine;

public class KeyboardJoystick : MonoBehaviour, IJoystick {
    [IoC.Inject]
    public IContainer Container { set; protected get; }
    [IoC.Inject]
    public ICollision Collision { set; protected get; }
    [IoC.Inject]
    public IPhysicsManager PhysicsManager { set; protected get; }

    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }
    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }

    [IoC.Inject]
    public IMaskManager MaskManager { set; protected get; }

    public float Speed = 1;
    public Vector2 jampVelocity = new Vector2(0, -21);

    private PlayerController controller;
    private IPlayerModel model;
    private UnitViewReciver viewReciver;

	void Start () {
        this.Inject();
        viewReciver = GetComponent<UnitViewReciver>();
	    controller = GetComponent<PlayerController>();
	    model = controller.GetModel();
	}

    private bool a1, a2, a3, a4, a5, a6;
    private int oldJampFrame = 0;
    private int oldDownFrame = 0;
    private int oldLeftFrame = 0;
    private int oldRightFrame = 0;
    private int countJampFrame = 0;

    private IPhysicItem pysicItem {
        get { return viewReciver.pysicItem; }
    }

    private float joystickH;
    private float joystickV;
    private float oldJ;
    void Update() {

        if (Container == null)
            return;

        var offset = TileDataProvider.WorldPosToOffsetTile(pysicItem.GetBottomPos());
        bool isStairs = MapGenerator.GetTile(new Vector2Int(offset.x, offset.y)).IsStairs();
        joystickH = UltimateJoystick.GetHorizontalAxis("Joystick1");
        joystickV = UltimateJoystick.GetVerticalAxis("Joystick1");

        //jamp
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) /*|| (joystickV >= 0 && joystickV < 0.4)*/)
        {
            
            if (oldJampFrame != Time.frameCount && (Collision.Raycast(pysicItem.Position, viewReciver.size, Vector2.down).Count != 0 || isStairs)) {
                countJampFrame = 0;
                pysicItem.AddVelocity(new Vector2(0, -pysicItem.velocity.y));
                oldJampFrame = Time.frameCount;
            }
        }
     

        //jamp isStairs
        if (isStairs &&( Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)
            ||  joystickV > .5)) {
                pysicItem.ApplayVelocity(new Vector2(0, 4));
        }

        // Down
        if (oldDownFrame != Time.frameCount && Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)
            || joystickV < -.5)
        {
            pysicItem.AddVelocity(new Vector2(0, -Speed));
            oldDownFrame = Time.frameCount;

            if (isStairs) {
                pysicItem.ApplayVelocity(new Vector2(0, -4));               
            }
        }

        
        
        // Left
        if (oldLeftFrame != Time.frameCount && Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) ||
            (joystickH < -.2))
        {
            var speed = joystickH != 0 ? Speed * joystickH : -Speed;

            pysicItem.AddVelocity(new Vector2(speed, 0));
            oldLeftFrame = Time.frameCount;
            controller.Rotat(-1);

            if (!isStairs)
            {
                if (Collision.Raycast(pysicItem.Position, viewReciver.size, Vector2.down).Count != 0)
                {
                    var countTile = MaskManager.GetMaskLJamp(pysicItem.GetBottomLeftPos());
                    if (countTile > 0)
                    {
                        pysicItem.AddVelocity(new Vector2(0, -pysicItem.velocity.y));
                        pysicItem.SetPosition(new Vector2(pysicItem.Position.x - 1,
                            pysicItem.Position.y + 16*countTile + 1));
                    }

                }
            }


        }



        // Right
        if (oldRightFrame != Time.frameCount && Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) ||
            (joystickH > .2))
        {
            var speed = joystickH != 0 ? Speed * joystickH : Speed;
            pysicItem.AddVelocity(new Vector2(speed, 0));
            oldRightFrame = Time.frameCount;
            controller.Rotat(1);


            if (!isStairs)
            {
                if (Collision.Raycast(pysicItem.Position, viewReciver.size, Vector2.down).Count != 0)
                {
                    var countTile = MaskManager.GetMaskRJamp(pysicItem.GetBottomRightPos());
                    //Debug.LogError(countTile);
                    if (countTile > 0)
                    {
                        pysicItem.AddVelocity(new Vector2(0, -pysicItem.velocity.y));
                        pysicItem.SetPosition(new Vector2(pysicItem.Position.x + 1,
                            pysicItem.Position.y + 16 * countTile + 1));
                    }

                }
            }

        }


        var joystickH2 = UltimateJoystick.GetHorizontalAxis("Joystick2");
        var joystickV2 = UltimateJoystick.GetVerticalAxis("Joystick2");
        
        bool joystick = Vector2.Distance(new Vector2(joystickH2, joystickV2), Vector2.zero) > 0.1;
        bool joystick0 = UltimateJoystick.GetIsDown("Joystick1");

        var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (!Settings.IS_EDIT_MOD && (
            (!joystick0 || joystick0 && joystick) && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.LeftControl) || joystick)) && !UICamera.IsGuiClick)
        {
            if (model.CurrentWeapon != null) {
                if (!joystick)
                {
                    var dir = (pos - transform.position).normalized;
                    controller.Rotat(dir.x);
                }
                else
                {
                    controller.Rotat(joystickH2);
                }
                model.CurrentWeapon.Fire(pos, joystick);
            }
        }

        if (((!Settings.IS_EDIT_MOD) && (
            (!joystick0 || joystick0 && joystick) && (Input.GetMouseButton(0) || Input.GetKey(KeyCode.LeftControl) || joystick)) && !UICamera.IsGuiClick))
        {
            if (model.CurrentWeapon != null) {
                if (!joystick)
                {
                    var dir = (pos - transform.position).normalized;
                    controller.Rotat(dir.x);
                }
                else
                {
                    controller.Rotat(joystickH2);
                }
                model.CurrentWeapon.FireCycle(pos, joystick);
            }
        }

        if (model.CurrentWeapon != null)
        {
            var rot = pos;
            if (joystick)
            {
                var v = UltimateJoystick.GetVerticalAxis("Joystick2");
                var h = UltimateJoystick.GetHorizontalAxis("Joystick2");
                rot = transform.position + new Vector3(h, v) *100 ;
            }
            model.CurrentWeapon.Rotate(rot);
        }


        //Bag
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            model.UseItem(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            model.UseItem(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            model.UseItem(3);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            model.UseItem(4);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            model.UseItem(5);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6)) {
            model.UseItem(6);
        }

        if (Input.GetKeyDown(KeyCode.Alpha7)) {
            model.UseItem(7);
        }

        if (Input.GetKeyDown(KeyCode.Alpha8)) {
            model.UseItem(8);
        }

        if (Input.GetKeyDown(KeyCode.Alpha9)) {
            model.UseItem(9);
        }

    }

    private float oldJoystickV = 0;


    public int jampUpdateCount = 3;
    public Vector2 Gravity;
    public void FixedUpdate() {
        pysicItem.SetGravity(Gravity);
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) /*|| joystickV > 0.4*/)
        {

            if (countJampFrame < jampUpdateCount) {
                countJampFrame++;
                pysicItem.AddVelocity(jampVelocity - new Vector2(0, countJampFrame));
            }
        }


        if (joystickV > 0)
        {
            if (joystickV - oldJoystickV >= 0.4) //joystickV >= 0.4)
            {

                var offset = TileDataProvider.WorldPosToOffsetTile(pysicItem.GetBottomPos());
                bool isStairs = MapGenerator.GetTile(new Vector2Int(offset.x, offset.y)).IsStairs();

                if (oldJampFrame != Time.frameCount &&
                    (Collision.Raycast(pysicItem.Position, viewReciver.size, Vector2.down).Count != 0 || isStairs))
                {
                    pysicItem.AddVelocity(new Vector2(0, -pysicItem.velocity.y));
                    pysicItem.AddVelocity(jampVelocity - new Vector2(0, 0));
                    pysicItem.AddVelocity(jampVelocity - new Vector2(0, 1));
                    pysicItem.AddVelocity(jampVelocity - new Vector2(0, 2));
                    //  pysicItem.AddVelocity(jampVelocity - new Vector2(0, 3));
                    //   pysicItem.AddVelocity(jampVelocity - new Vector2(0, 4));
                    // pysicItem.AddVelocity(jampVelocity - new Vector2(0, 4));
                    // pysicItem.AddVelocity(jampVelocity - new Vector2(0, 6));
                    oldJoystickV = joystickV;
                }
            }

            if (joystickV - oldJoystickV < 0)
            {
                oldJoystickV = joystickV;
            }
        }


        if (PhysicsManager != null && viewReciver != null && viewReciver.pysicItem != null) {
            PhysicsManager.ChackCollision(viewReciver.pysicItem);
        }

    }


   
}

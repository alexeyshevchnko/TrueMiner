using System.Collections;
using System.Collections.Generic;
using IoC;
using trasharia;
using UnityEngine;
using UnityEngine.InputSystem;

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
        if (Keyboard.current?.spaceKey.wasPressedThisFrame == true || Keyboard.current?.upArrowKey.wasPressedThisFrame == true || Keyboard.current?.wKey.wasPressedThisFrame == true /*|| (joystickV >= 0 && joystickV < 0.4)*/)
        {
            
            if (oldJampFrame != Time.frameCount && (Collision.Raycast(pysicItem.Position, viewReciver.size, Vector2.down).Count != 0 || isStairs)) {
                countJampFrame = 0;
                pysicItem.AddVelocity(new Vector2(0, -pysicItem.velocity.y));
                oldJampFrame = Time.frameCount;
            }
        }
     

        //jamp isStairs
        if (isStairs &&( Keyboard.current?.spaceKey.isPressed == true || Keyboard.current?.upArrowKey.isPressed == true || Keyboard.current?.wKey.isPressed == true
            ||  joystickV > .5)) {
                pysicItem.ApplayVelocity(new Vector2(0, 4));
        }

        // Down
        if (oldDownFrame != Time.frameCount && Keyboard.current?.downArrowKey.isPressed == true || Keyboard.current?.sKey.isPressed == true
            || joystickV < -.5)
        {
            pysicItem.AddVelocity(new Vector2(0, -Speed));
            oldDownFrame = Time.frameCount;

            if (isStairs) {
                pysicItem.ApplayVelocity(new Vector2(0, -4));               
            }
        }

        
        
        // Left
        if (oldLeftFrame != Time.frameCount && Keyboard.current?.leftArrowKey.isPressed == true || Keyboard.current?.aKey.isPressed == true ||
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
        if (oldRightFrame != Time.frameCount && Keyboard.current?.rightArrowKey.isPressed == true || Keyboard.current?.dKey.isPressed == true ||
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

        var pos = Camera.main.ScreenToWorldPoint(Mouse.current?.position.ReadValue() ?? Vector3.zero);

        if (!Settings.IS_EDIT_MOD && (
            (!joystick0 || joystick0 && joystick) && (Mouse.current?.leftButton.wasPressedThisFrame == true || Keyboard.current?.leftCtrlKey.wasPressedThisFrame == true || joystick)) && !(UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()))
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
            (!joystick0 || joystick0 && joystick) && (Mouse.current?.leftButton.isPressed == true || Keyboard.current?.leftCtrlKey.isPressed == true || joystick)) && !(UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())))
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
        if (Keyboard.current?.digit1Key.wasPressedThisFrame == true) {
            model.UseItem(1);
        }

        if (Keyboard.current?.digit2Key.wasPressedThisFrame == true) {
            model.UseItem(2);
        }

        if (Keyboard.current?.digit3Key.wasPressedThisFrame == true) {
            model.UseItem(3);
        }

        if (Keyboard.current?.digit4Key.wasPressedThisFrame == true) {
            model.UseItem(4);
        }

        if (Keyboard.current?.digit5Key.wasPressedThisFrame == true) {
            model.UseItem(5);
        }

        if (Keyboard.current?.digit6Key.wasPressedThisFrame == true) {
            model.UseItem(6);
        }

        if (Keyboard.current?.digit7Key.wasPressedThisFrame == true) {
            model.UseItem(7);
        }

        if (Keyboard.current?.digit8Key.wasPressedThisFrame == true) {
            model.UseItem(8);
        }

        if (Keyboard.current?.digit9Key.wasPressedThisFrame == true) {
            model.UseItem(9);
        }

    }

    private float oldJoystickV = 0;


    public int jampUpdateCount = 3;
    public Vector2 Gravity;
    public void FixedUpdate() {
        pysicItem.SetGravity(Gravity);
        if (Keyboard.current?.spaceKey.isPressed == true || Keyboard.current?.upArrowKey.isPressed == true || Keyboard.current?.wKey.isPressed == true /*|| joystickV > 0.4*/)
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

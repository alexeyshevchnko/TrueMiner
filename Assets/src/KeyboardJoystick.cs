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

    void Update() {

        if (Container == null)
            return;

        var offset = TileDataProvider.WorldPosToOffsetTile(pysicItem.GetBottomPos());
        bool isStairs = MapGenerator.GetTile(new Vector2Int(offset.x, offset.y)).IsStairs();

        //jamp
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) 
            || TouchInput.instance.isJampDown) {

            if (oldJampFrame != Time.frameCount 
                && 
                (Collision.Raycast(pysicItem.GetPosition(), viewReciver.size, Vector2.down).Count != 0 ||
                isStairs)) {
                countJampFrame = 0;
                pysicItem.AddVelocity(new Vector2(0, -pysicItem.velocity.y));
                oldJampFrame = Time.frameCount;
            }

         
        }

        //jamp isStairs
        if (isStairs &&( Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)
            || TouchInput.instance.isJampDown)) {

          //  var offsetCenter = TileDataProvider.WorldPosToOffsetTile(pysicItem.GetPosition());
          //  bool isStairsCenter = MapGenerator.GetTile(new Vector2Int(offsetCenter.x, offsetCenter.y-2)).IsStairs();

          //  if (isStairsCenter) {
                pysicItem.ApplayVelocity(new Vector2(0, 4));
          //  }
        }

        // Down
        if (oldDownFrame != Time.frameCount && Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
            pysicItem.AddVelocity(new Vector2(0, -Speed));
            oldDownFrame = Time.frameCount;

            if (isStairs) {
                pysicItem.ApplayVelocity(new Vector2(0, -4));               
            }
        }

        // Left
        if (oldLeftFrame != Time.frameCount && Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || TouchInput.instance.isLeft)
        {
            
            pysicItem.AddVelocity(new Vector2(-Speed, 0));
            oldLeftFrame = Time.frameCount;
            controller.Rotat(-1);

            if (!isStairs)
            {
                if (Collision.Raycast(pysicItem.GetPosition(), viewReciver.size, Vector2.down).Count != 0)
                {
                    var countTile = MaskManager.GetMaskLJamp(pysicItem.GetBottomLeftPos());
                    if (countTile > 0)
                    {
                        pysicItem.AddVelocity(new Vector2(0, -pysicItem.velocity.y));
                        pysicItem.SetPosition(new Vector2(pysicItem.GetPosition().x - 1,
                            pysicItem.GetPosition().y + 16*countTile + 1));
                    }

                }
            }


        }

        // Right
        if (oldRightFrame != Time.frameCount && Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || TouchInput.instance.isRight) {
            pysicItem.AddVelocity(new Vector2(Speed, 0));
            oldRightFrame = Time.frameCount;
            controller.Rotat(1);


            if (!isStairs)
            {
                if (Collision.Raycast(pysicItem.GetPosition(), viewReciver.size, Vector2.down).Count != 0)
                {
                    var countTile = MaskManager.GetMaskRJamp(pysicItem.GetBottomRightPos());
                    //Debug.LogError(countTile);
                    if (countTile > 0)
                    {
                        pysicItem.AddVelocity(new Vector2(0, -pysicItem.velocity.y));
                        pysicItem.SetPosition(new Vector2(pysicItem.GetPosition().x + 1,
                            pysicItem.GetPosition().y + 16 * countTile + 1));
                    }

                }
            }

        }



        var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (!Settings.IS_EDIT_MOD && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.LeftControl)) && !UICamera.IsGuiClick) {
            if (model.CurrentWeapon != null) {
                var dir = (pos - transform.position).normalized;
                controller.Rotat(dir.x);
          //      PhotonManager.inst.SendPlayerRotate(controller.Id, dir.x);
            //    Debug.LogError("fire");
                model.CurrentWeapon.Fire(pos);
           //     PhotonManager.inst.SendFire(controller.Id, pos);
                
            }
        }

        if (((!Settings.IS_EDIT_MOD) && (Input.GetMouseButton(0) || Input.GetKey(KeyCode.LeftControl)) && !UICamera.IsGuiClick)) {
            if (model.CurrentWeapon != null) {
                var dir = (pos - transform.position).normalized;
                controller.Rotat(dir.x);
               // PhotonManager.inst.SendPlayerRotate(controller.Id, dir.x);
                model.CurrentWeapon.FireCycle(pos);
          //      PhotonManager.inst.SendFireCycle(controller.Id, pos);
            }
        }

        if (model.CurrentWeapon != null) {
            model.CurrentWeapon.Rotate(pos);
        }


        //Bag
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            model.UseItem(1);
           // PhotonManager.inst.SendChangeWeapon(controller.Id, model.GetСurrentWeaponId());
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            model.UseItem(2);
          //  PhotonManager.inst.SendChangeWeapon(controller.Id, model.GetСurrentWeaponId());
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            model.UseItem(3);
          //  PhotonManager.inst.SendChangeWeapon(controller.Id, model.GetСurrentWeaponId());
        }

        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            model.UseItem(4);
         //   PhotonManager.inst.SendChangeWeapon(controller.Id, model.GetСurrentWeaponId());
        }

        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            model.UseItem(5);
         //   PhotonManager.inst.SendChangeWeapon(controller.Id, model.GetСurrentWeaponId());
        }

        if (Input.GetKeyDown(KeyCode.Alpha6)) {
            model.UseItem(6);
          //  PhotonManager.inst.SendChangeWeapon(controller.Id, model.GetСurrentWeaponId());
        }

        if (Input.GetKeyDown(KeyCode.Alpha7)) {
            model.UseItem(7);
         //   PhotonManager.inst.SendChangeWeapon(controller.Id, model.GetСurrentWeaponId());
        }

        if (Input.GetKeyDown(KeyCode.Alpha8)) {
            model.UseItem(8);
         //   PhotonManager.inst.SendChangeWeapon(controller.Id, model.GetСurrentWeaponId());
        }

        if (Input.GetKeyDown(KeyCode.Alpha9)) {
            model.UseItem(9);
          //  PhotonManager.inst.SendChangeWeapon(controller.Id, model.GetСurrentWeaponId());
        }

      //  PhotonManager.inst.SendPlayerPysicItem(controller.Id, pysicItem.GetSyncData());
    }

    public int jampUpdateCount = 3;
    public Vector2 Gravity;
    public void FixedUpdate() {

        pysicItem.SetGravity(Gravity);
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || TouchInput.instance.isJampPress) {

            if (countJampFrame < jampUpdateCount) {
                countJampFrame++;
                pysicItem.AddVelocity(jampVelocity - new Vector2(0, countJampFrame));
            }
        }



        if (PhysicsManager != null && viewReciver != null && viewReciver.pysicItem != null) {
            PhysicsManager.ChackCollision(viewReciver.pysicItem);
        }

    }


   
}

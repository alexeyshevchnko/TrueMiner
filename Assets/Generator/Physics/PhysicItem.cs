using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using trasharia;

public class PhysicItem : IPhysicItem {
    [IoC.Inject]
    public ICollision Collision { set; protected get; }
    [IoC.Inject]
    public IPhysicsManager PhysicsManager { set; protected get; }
    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }
    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }

    public Vector2 position;
    public Vector2 velocity { get; protected set; }
    public Vector2 oldVelocity { get; protected set; }
    public Vector2 momentum = Vector2.zero;
    public Vector2 momentum0 = Vector2.zero;
    private int countTrySleep = 0;
    private bool _sleep;
    public bool Sleep { get { return _sleep; }
        protected set {
            _sleep = value;
            countTrySleep = 0;
        }
    }
    public Vector2 oldPosition;

    public Action<IPhysicItem> OnCollision { get; set; }
    public Action OnTerrainCollision { get; set; }

    private float _itemWidth12;
    private float _itemWidth;
    public float itemWidth {
        get { return _itemWidth; }
        set {
            _itemWidth = value;
            _itemWidth12 = value / 2;
        }
    }

    private float _itemHeight12;
    private float _itemHeight;
    public float itemHeight {
        get { return _itemHeight; }
        set {
            _itemHeight = value;
            _itemHeight12 = value/2;
        }
    }

    public Vector2 Size {
        get {
            return new Vector2(itemWidth, itemHeight);
        }
    }
    public float maxRunSpeed = 3f;
    private int jumpHeight = 15;
    private float jumpSpeed = 5.01f;
    public Vector2 gravity = new Vector2(0, -2.4f);

    public Vector2 momentVector = new Vector2(0.9f, 0.99f);

    public float ReboundFactor { get; set; }

    public Vector2Int[] PhysicsSectors { get; set; }

    public MonoBehaviour View { get; protected set; }

    public PhysicItem(MonoBehaviour view, bool isCollisionTerrain = true, bool isReactExplosion = true) {
        Sleep = false;
        View = view;
        IsCollisionTerrain = isCollisionTerrain;
        IsReactExplosion = isReactExplosion;
    }

    public void SetGravity(Vector2 val) {
        Sleep = false;
        gravity = val;
    }

    public void SetMomentVector(Vector2 val) {
        momentVector = val;
    }

    public Vector2 Position
    {
        get { return position; }
    }

 //   public Vector2 Position() {
  //      return position;
  //  }

    public void SetPosition(Vector2 pos) {
        Sleep = false;
        position = pos;
    }

    public void AddVelocity(Vector2 val) {
        Sleep = false;
        velocity += val;
    }

    public void AddMomentum(Vector2 val) {
        Sleep = false;
        momentum += val;
        momentum0 = momentum;
        momentum = Vector2.zero;
    }

    public void SetVelocity(Vector2 val) {
        Sleep = false;
        velocity = val;
    }

    public Vector2 GetTopLeftPos() {
        Vector2 rez = new Vector2(position.x - _itemWidth12, position.y + _itemHeight12);
        return rez;
    }

    public Vector2 GetTopRightPos() {
        Vector2 rez = new Vector2(position.x + _itemWidth12, position.y + _itemHeight12);
        return rez;
    }

    public Vector2 GetBottomRightPos() {
        Vector2 rez = new Vector2(position.x + _itemWidth12, position.y - _itemHeight12);
        return rez;
    }

    public Vector2 GetBottomLeftPos() {
        Vector2 rez = new Vector2(position.x - _itemWidth12, position.y - _itemHeight12);
        return rez;
    }

    public Vector2 GetBottomPos() {
        Vector2 rez = new Vector2(position.x , position.y - _itemHeight12);
        return rez;
    }

    public Rect GetRect() {
        return new Rect(position, Size);
    }

    public void SetSleep(bool val) {
        Sleep = val;
    }

    private Vector2 oldPos = Vector2.zero;
    private Vector2 oldVel = Vector2.zero;

    public bool IsCollisionTerrain { get; set; }
    public bool IsReactExplosion { get; set; }

    public void Update() {
        if (Sleep || Collision == null)
            return;
        /*
        if (trace) {
            position = oldPos;
            velocity = oldVel;
            trace = false;
            return;
        }*/

        var offset = TileDataProvider.WorldPosToOffsetTile(position);
        //GetBottomPos

        bool isWater = MapGenerator.GetTile(offset).IsWater2();
        var offsetBottom = TileDataProvider.WorldPosToOffsetTile(GetBottomPos());
        bool isStairs = MapGenerator.GetTile(offsetBottom).IsStairs();
       
        
            
        float d = isWater ? 2 : 1;
     
        oldVel = velocity;

        //Эффект поплавка например не тонуть  в воде 
        //d = gravity.y;

        if (isWater && velocity.y >= -5) {
            velocity += gravity/d;
        } else {
            if (!isWater) {
                velocity += gravity/d;
            }

            if (isWater && velocity.y < -5) {
                velocity = new Vector2(velocity.x,-5);
            }
        }

        velocity = new Vector2(velocity.x*momentVector.x, velocity.y*momentVector.y);

        if (isStairs) {
            
            bool isStairsCenter = MapGenerator.GetTile(offset).IsStairs();

            if (!isStairsCenter && velocity.y < 0) {
                velocity = new Vector2(velocity.x, 0);
            }
            if (isStairsCenter) {
                velocity = new Vector2(velocity.x, 0);
            }
        }

        //урезаем 
        oldVelocity = velocity;
   

        var newVelocity = velocity;

        if (IsCollisionTerrain)
            newVelocity = Collision.ChackVelocity(position, velocity, itemWidth, itemHeight);
        
        if (velocity != newVelocity) {
            OnTerrainCollision.TryCall();
        }

        velocity = newVelocity;
        oldPos = position;
        position += velocity;
        if (ReboundFactor != 0) {
            if (Math.Abs(oldVelocity.x - velocity.x) > 0.1f) {
                velocity = new Vector2(-oldVelocity.x*ReboundFactor, velocity.y);
            }

            if (Math.Abs(oldVelocity.y - velocity.y) > 0.1f) {
                velocity = new Vector2(velocity.x, -oldVelocity.y*ReboundFactor);
            }
        }

        PhysicsManager.ChackRect(this);

        if (Vector2.Distance(oldPos, position) == 0) {
            countTrySleep++;
            if (countTrySleep > 10) {
                Sleep = true;
            }
        } else {
            countTrySleep = 0;
        }

        /*
        var offset = TileDataProvider.WorldPosToOffsetTile(position); 
        if (!MapGenerator.GetMap()[offset.x, offset.y].IsEmpty()) {
            if (View.name.IndexOf("block") != -1) {
                Debug.LogError("type != 0" + View.name + "oldPos = " + oldPos + " position = " + position);
                trace = true;
            }
        }*/
    }

    public void ApplayVelocity(Vector2 velocity) {
        var newVelocity = velocity;
        if (IsCollisionTerrain)
           newVelocity = Collision.ChackVelocity(position, velocity, itemWidth, itemHeight);
        position += newVelocity;
    }


    private bool trace = false;
    
    ~PhysicItem() {

    }

    public void Destroy() {
        UnRegister();
    }

    public void Register() {
        if (PhysicsManager != null) {
            PhysicsManager.ChackRect(this);
        }
    }

    public void UnRegister() {
        if (PhysicsManager != null) {
            PhysicsManager.RemovePhysicItem(this);
        }
    }

    public byte[] GetSyncData() {
        List<byte> rez = new List<byte>();
        rez.AddRange(BitConverter.GetBytes(position.x));
        rez.AddRange(BitConverter.GetBytes(position.y));
        rez.AddRange(BitConverter.GetBytes(velocity.x));
        rez.AddRange(BitConverter.GetBytes(velocity.y));

        return rez.ToArray();
    }

    public void Sync(byte[] data) {
        var posX = BitConverter.ToSingle(data, sizeof(float) * 0);
        var posY = BitConverter.ToSingle(data, sizeof(float) * 1);
        var velX = BitConverter.ToSingle(data, sizeof(float) * 2);
        var velY = BitConverter.ToSingle(data, sizeof(float) * 3);


        position = new Vector2(posX, posY);
        velocity = new Vector2(velX, velY);
    }
}

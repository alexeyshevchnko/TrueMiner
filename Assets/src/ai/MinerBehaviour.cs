using System.Collections;
using System.Collections.Generic;
using trasharia;
using UnityEngine;

public enum MinerState {
    idle, 
    runMine,
    runReturn,
    idleMine,
    idleReturn,
}

public class MinerBehaviour  : IAIBehaviour {

    private MinerState State = MinerState.runMine;

    private float direcr = 1;
    private float speed = 0.1f;
    private float lastJampPosX;
    private int attemptJampCount = 4;
    private float dirJamp = 15;


    private int countTryJamp = 1;
    [IoC.Inject]
    public ICollision Collision { set; protected get; }
    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }
    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }

    [IoC.Inject]
    public ITargetManager TargetManager { set; protected get; }

    [IoC.Inject]
    public ICooldownManager CooldownManager { set; protected get; }

    [IoC.Inject]
    public IRandom Random { set; protected get; }

    private MinerView view;

    public IPhysicItem pysicItem { get; protected set; }

    private ICooldownItem cooldownIdleMine;
    private ICooldownItem cooldownIdleReturn;


    private TileInfoStructure[,] Map {
        get {
            return MapGenerator.GetMap();
        }
    }

    ~MinerBehaviour() {
        if (pysicItem != null) {
            pysicItem.Destroy();
        }

     //  if (CooldownManager != null && cooldownItem!= null) {
     //       CooldownManager.RemoveCooldown(cooldownItem);
     //   }
    }

    public void Init(IPhysicItem pysicItem, IView view) {
        this.pysicItem = pysicItem;
        this.view = (MinerView)view;
        
    }

    void ChangeDirect() {
        direcr = -direcr;
    }


    public void Jamp() {
        //throw new System.NotImplementedException();
    }


   

    public void Update() {
        var dir = new Vector2();
        var pos = pysicItem.Position;

        switch (State) {
            case MinerState.idleReturn:
                if (cooldownIdleReturn == null) {
                    cooldownIdleReturn = CooldownManager.AddCooldown(1, null, () => {
                        CooldownManager.RemoveCooldown(cooldownIdleReturn);
                        cooldownIdleReturn = null;

                        direcr = 1;

                        State = MinerState.runMine;
                        
                        view.StopTruck();
                    });
                }

                break;

            case MinerState.idleMine:
                if (cooldownIdleMine == null) {

                    cooldownIdleMine = CooldownManager.AddCooldown(1, null, () => {
                        direcr = -1;

                        State = MinerState.runReturn;
                        CooldownManager.RemoveCooldown(cooldownIdleMine);
                        cooldownIdleMine = null;
                        view.StopKirka();

                        view.PlayTruck();
                    });
                    view.PlayKirka();
                }
                break;

            case MinerState.runMine:
                dir = new Vector2(speed * direcr, 0);
                pysicItem.AddVelocity(dir);

                if (pos.x > maxPos.x && direcr != -1) {
                    pysicItem.SetVelocity(new Vector2());

                   

                    State = MinerState.idleMine;
                }

                break;


            case MinerState.runReturn:
                dir = new Vector2(speed * direcr, 0);
                pysicItem.AddVelocity(dir);

                if (pos.x < minPos.x && direcr != 1) {
                    pysicItem.SetVelocity(new Vector2());
                    pysicItem.SetPosition(new Vector2(minPos.x, pysicItem.Position.y));
                 //   direcr = 1;
                    speed = UnityEngine.Random.Range(0.1f, 4.5f);

                  

                    State = MinerState.idleReturn;
                }
                
                break;
        }
    }


    public int GetDirection() {
        return (int)direcr;
    }

    private Vector2 minPos;
    public void SetMinPos(Vector2 pos) {
        minPos = pos;
    }

    private Vector2 maxPos;
    public void SetMaxPos(Vector2 pos) {
        maxPos = pos;
    }

}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using IoC;
using trasharia;
using Random = UnityEngine.Random;

public class TargetManager : ITargetManager{

    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }
    [IoC.Inject]
    public IPrefubManager PrefubManager { set; protected get; }
    [IoC.Inject]
    public IContainer Container { set; protected get; }
    [IoC.Inject]
    public ICooldownManager CooldownManager { set; protected get; }
    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }
    [IoC.Inject]
    public IGOPoolManager IgoPoolManager { set; protected get; }
    [IoC.Inject]
    public IRandom Random { set; protected get; }
    
    private GameObject player;
    private List<AiController> units = new List<AiController>();
    private int maxUnitts = 0; 

    List<PlayerController> otherPlayers = new List<PlayerController>();

    public void Init() {
        MapGenerator.OnChangeMap += OnChangeMap;
        if (!MapGenerator.IsGenerated) {
            MapGenerator.OnGenerate += OnMapGenerate;
        } else {
            OnMapGenerate();
        }
    }

    void OnChangeMap() {
        PlayerController.SetPos(MapGenerator.CenterPos);
    }

    void OnMapGenerate() {
        CreatePlayer();
        //CreateUnit(0);

        CooldownManager.AddInfinityCooldown(OnTick);
    }

    public PlayerController PlayerController { get; private set; }

    void CreatePlayer() {
        var parent = GameObject.Find("Units");
        var prefub = PrefubManager.GetPlayer();
        player = GameObject.Instantiate(prefub, Vector3.zero, new Quaternion()) as GameObject;
        player.transform.parent = parent.transform;
        var controller = player.GetComponent<PlayerController>();
        Container.Inject(controller);
        controller.Init(MapGenerator.CenterPos);
        PlayerController = controller;

        var worldPos = TileDataProvider.OffsetTileToWorldPos(MapGenerator.CenterPos);
        
        Camera.main.transform.GetComponent<TargetChase>().target = player.transform;


        controller.Id = 1;// PhotonNetwork.player.ID;
        otherPlayers.Add(controller);

      //  PhotonManager.inst.SendSpawnUnit(controller.Id, worldPos);
    }


    public void CreateOtherPlayer(int id, Vector3 worldPos) {
        var offset = TileDataProvider.WorldPosToOffsetTile(worldPos);
        var parent = GameObject.Find("Units");
        var prefub = PrefubManager.GetPlayer();
        player = GameObject.Instantiate(prefub, Vector3.zero, new Quaternion()) as GameObject;
        player.transform.parent = parent.transform;

        var controller = player.GetComponent<PlayerController>();
        Container.Inject(controller);
        controller.Init(offset);
        //PlayerController = controller;

        var joystick = player.GetComponent<IJoystick>();
        MonoBehaviour.Destroy(joystick as MonoBehaviour);

        otherPlayers.Add(controller);


        controller.Id = id;
    }

    public PlayerController GetPlayer(int id) {
        PlayerController rez = null;
        foreach (var other in otherPlayers) {
            if (other.Id == id) {
                rez = other;
                break;
            }
        }

        return rez;
    }

    public AiController CreateUnit(int id, Vector2Int pos) {
        var parent = GameObject.Find("Units");
        var prefub = PrefubManager.GetUnit(id);
        var unit = GameObject.Instantiate(prefub, Vector3.zero, new Quaternion()) as GameObject;
        unit.transform.parent = parent.transform;
        var controller = unit.GetComponent<AiController>();
        Container.Inject(controller);
        controller.Init(pos);
        controller.InitZombieBehaviour();
        return controller;
    }

    public AiController CreateMinerUnit(int id, Vector2Int pos) {
        var parent = GameObject.Find("Units");
        var prefub = PrefubManager.GetUnit(id);
        var unit = GameObject.Instantiate(prefub, Vector3.zero, new Quaternion()) as GameObject;
        unit.transform.parent = parent.transform;
        var controller = unit.GetComponent<AiController>();
        Container.Inject(controller);
        controller.Init(pos);
        controller.InitMinerBehaviour();
        return controller;
    }

    public void DestroyUnit(AiController unit) {
        for (int i = units.Count - 1; i >= 0; i--) {
            if (units[i] == unit) {
                GameObject.Destroy(units[i].gameObject);
                units.RemoveAt(i);
                break;
            }
        }
    }

    void OnTick() {
       // CreateUnit(0);

        if (player != null) {
            int screenSize = 100;


            var playerOffset = TileDataProvider.WorldPosToOffsetTile(player.transform.position);

            //1 удаляем юнитов которые слишком далеко
            for (int i = units.Count - 1; i >= 0; i--) {
                var unitOffset = TileDataProvider.WorldPosToOffsetTile(units[i].transform.position);
                if (Mathf.Abs(unitOffset.x - playerOffset.x) > screenSize*2) {
                    GameObject.Destroy(units[i].gameObject);
                    units.RemoveAt(i);
                }
            }


            if (units.Count < maxUnitts) {
                for (int j = 0; j < maxUnitts - units.Count; j++) {

                    //2 определяем точку слева и справа
                    Vector2Int L, R;
                    if (playerOffset.x - screenSize > 0) {
                        L = MapGenerator.Surface[playerOffset.x - screenSize];
                    } else {
                        L = MapGenerator.Surface[0];
                    }
                    if (playerOffset.x + screenSize < MapGenerator.Surface.Count) {
                        R = MapGenerator.Surface[playerOffset.x + screenSize];
                    } else {
                        R = MapGenerator.Surface[MapGenerator.Surface.Count-1];
                    }

                    var id = Random.Range(0, 7);
                    if (id == 6)
                        id = 9;

                    var r = Random.Range(0, 10);
                    if (r > 5) {
                        units.Add(CreateUnit(id, R));
                    } else {
                        units.Add(CreateUnit(id, L));
                    }
                }
            }

        }
    }


    public List<ITargetBase> GetUnitsInRange(Vector2 center, float radius) {
        List<ITargetBase> rez = new List<ITargetBase>();
        foreach (var unit in units) {
            if (Vector2.Distance(unit.transform.position, center) <= radius) {
                rez.Add(unit);
            }
        }

        if (Vector2.Distance(PlayerController.transform.position, center) <= radius) {
            rez.Add(PlayerController);
        }

        return rez;
    }

    //чем ближе к эпицентру тем больше урон
    public void ExplosionDamage(Vector2 center, float radius, float hp) {
        var units = GetUnitsInRange(center, radius);
        
        for (int i = units.Count-1; i >= 0; i--) {
            var unit = units[i];
            var dist = Vector2.Distance(unit.GetTransform().position, center);
            var factor = 1 - dist / radius;
            unit.Damage(hp * factor);
            IgoPoolManager.Spawn<BloodEffect>(unit.GetTransform().position, Quaternion.identity);
        }
    }
}

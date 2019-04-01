using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ApplicationManager : MonoBehaviour,IoC.IInitialize {
    [IoC.Inject]
    public ICooldownManager CooldownManager { set; protected get; }
    [IoC.Inject]
    public IPrefubManager PrefubManager { set; protected get; }
    [IoC.Inject]
    public ITargetManager TargetManager { set; protected get; }
    [IoC.Inject]
    public IGuiPoolManager GuiPoolManager { set; protected get; }

    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }
  

    public GameObject Player;
    public List<PrefubItem> Units;
    public List<PrefubItem> Items;

    public void OnInject() {
        
        PrefubManager.InitPlayer(Player);
        PrefubManager.InitUnits(Units);
        PrefubManager.InitItems(Items);
        TargetManager.Init();
        GuiPoolManager.SetPool(GuiPool.guiPool);

        MapGenerator.StartApplication();
    }

	void Start () {
	    this.Inject();
	}
	
	void FixedUpdate () {
	    ((CooldownManager)CooldownManager).Update();
	}
}

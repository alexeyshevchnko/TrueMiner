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
        
        if (Player == null) {
            Debug.LogError("[ApplicationManager] 'Player' prefab is not assigned in the scene!", this);
        } else {
            PrefubManager.InitPlayer(Player);
        }

        if (Units == null || Units.Count == 0) {
            Debug.LogError("[ApplicationManager] 'Units' list is empty!", this);
        } else {
            PrefubManager.InitUnits(Units);
        }

        if (Items == null || Items.Count == 0) {
            Debug.LogError("[ApplicationManager] 'Items' list is empty!", this);
        } else {
            PrefubManager.InitItems(Items);
        }

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

    /*
    void Update()
    {
        if (QualitySettings.vSyncCount != 0 || Application.targetFrameRate != 30)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;
        }
    }*/
}

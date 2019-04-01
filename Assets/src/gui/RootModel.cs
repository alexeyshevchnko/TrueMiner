using UnityEngine;
using System.Collections;
using IoC;

public class RootModel : MonoBehaviour,IoC.IInitialize {
    [IoC.Inject]
    public IContainer Container { set; protected get; }

    static public RootModel instance;
    public RootContext rootContext;
    public NguiRootContext view;

   

    void Awake() {
        if (instance != null) {
            Debug.LogError("лишний RootModel");
            Destroy(this);
            return;
        }
        instance = this;
        rootContext = new RootContext();
        view.SetContext(rootContext);
    }

    void Start() { 
        this.Inject();
    }

    public void OnInject() {
        Container.Inject(rootContext);
        rootContext.Player = new PlayerComtext();
        Container.Inject(rootContext.Player);
        rootContext.Player.Bag = new BagContext();
        Container.Inject(rootContext.Player.Bag);
        //Debug.LogError("OnInject");
    }
}

using UnityEngine;

public class RootModel : MonoBehaviour, IoC.IInitialize {
    [IoC.Inject]
    public IoC.IContainer Container { set; protected get; }

    static public RootModel instance;
    // Контекст создаётся только в рантайме (Awake/OnInject); Unity-сериализация для
    // него не применяется — [NonSerialized] делает это намеренным (UAC1001 без
    // изменения поведения сериализации).
    [System.NonSerialized]
    public RootContext rootContext;

    void Awake() {
        if (instance != null) {
            Debug.LogError("лишний RootModel");
            Destroy(this);
            return;
        }
        instance = this;
        rootContext = new RootContext();
        GameUi.EnsureCreated();
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

        if (GameUi.Instance != null) {
            Container.Inject(GameUi.Instance);
        }
    }
}
using UnityEngine;

public class RootContext : IoC.IInitialize {
    public static RootContext Instance;

    [IoC.Inject]
    public IoC.IContainer Container { set; protected get; }

    public PlayerComtext Player { get; set; }

    public RootContext() {
        Instance = this;
    }

    public void OnInject() {
    }
}
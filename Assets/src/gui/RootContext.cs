using UnityEngine;
using System.Collections;
using IoC;

public class RootContext : EZData.Context,IoC.IInitialize {
    public static RootContext Instance;

    [IoC.Inject]
    public IContainer Container { set; protected get; }


    #region Property TEST
    private readonly EZData.Property<int> _privateTESTProperty = new EZData.Property<int>();
    public EZData.Property<int> TESTProperty { get { return _privateTESTProperty; } }
    public int TEST {
        get { return TESTProperty.GetValue(); }
        set { TESTProperty.SetValue(value); }
    }
    #endregion


    #region Player
    public readonly EZData.VariableContext<PlayerComtext> PlayerEzVariableContext = new EZData.VariableContext<PlayerComtext>(null);
    public PlayerComtext Player {
        get { return PlayerEzVariableContext.Value; }
        set { PlayerEzVariableContext.Value = value; }
    }
    #endregion



    public RootContext() {
        TEST = 314;
        Instance = this;
    }

    public void OnInject() {
        
    }
}

using System.Collections;
using Managers;
using trasharia;
using UnityEngine;

public class Main : IContextRoot {

    public IoC.IContainer container { get; private set; }

    public Main() {
        SetupContainer();
        StartGame();
    }

    private void SetupContainer() {

        container = new IoC.UnityContainer();
        container.Bind<IRandom>().AsSingle<trasharia.Random>();
        container.Bind<ITargetManager>().AsSingle<TargetManager>();

        container.Bind<IBackgroundManager>().AsSingle<BackgroundManager>();
        container.Bind<IScreenUpdateManager>().AsSingle<ScreenUpdateManager>();
        container.Bind<ICooldownManager>().AsSingle<CooldownManager>();
        container.Bind<ITileDataProvider>().AsSingle<TileDataProvider>();
        container.Bind<ICollision>().AsSingle<Collision>();
        container.Bind<IMapGenerator>().AsSingle<MapGenerator>();
       // container.Bind<ILightRendererOld>().AsSingle<LightRendererOldOld>();
        container.Bind<ILightRenderer>().AsSingle<LightRenderer>();
        container.Bind<IOverlayLightShema>().AsSingle<OverlayLightShema>();
        container.Bind<ILightShema>().AsSingle<LightShema>(); 
        container.Bind<ILightUvProvider>().AsSingle<LightUvProvider>();
        container.Bind<IPrefubManager>().AsSingle<PrefubManager>();
        container.Bind<ITargetManager>().AsSingle<TargetManager>();
        container.Bind<ISpriteManager>().AsSingle<SpriteManager>();
        container.Bind<IItemManager>().AsSingle<ItemManager>();
        container.Bind<IPhysicsManager>().AsSingle<PhysicsManager>();
        container.Bind<IGOPoolManager>().AsSingle<GOPoolManager>();
        container.Bind<IGuiPoolManager>().AsSingle<GuiPoolManager>();

        container.Bind<ISwapItemManager>().AsSingle<SwapItemManager>();
        container.Bind<IMaskManager>().AsSingle<MaskManager>();
        
    }
    private void StartGame() {
        //инициализируем нужный контроллер (мышка или тач)
    }
}

// Script Execution Order в проекте не задан, поэтому порядок Awake между объектами
// сцены недетерминирован. Корень IoC-контейнера должен инициализироваться раньше
// всех остальных Awake: иначе MonoInjection.Inject (SendMessageUpwards "OnAdded") из
// дочерних пулов (например GuiPool) попадает в UnityContext<T>.OnAdded до создания
// _applicationRoot и падает с NullReferenceException (см. Agent_Works/GUI_Agent_Work, ISSUE-3).
// Проверка DesignByContract.Check.Require в UnityContext<T>.OnAdded вырезана атрибутами
// [Conditional] (символы DBC_CHECK_* не определены), поэтому падение проявлялось как NRE.
[DefaultExecutionOrder(-1000)]
public class GameContext : UnityContext<Main> {

}
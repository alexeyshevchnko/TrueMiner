using System.Collections;
using Managers;
using trasharia;

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
        
    }
    private void StartGame() {
        //инициализируем нужный контроллер (мышка или тач)
    }
}

public class GameContext : UnityContext<Main> {

}
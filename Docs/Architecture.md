# Архитектура TrueMiner (снимок на момент аудита)

Документ описывает **текущее** состояние кода, а не целевое. Все утверждения получены
чтением исходников; запусков и профилирования не было. Пометка ⚠ означает «выведено из кода,
в рантайме не проверено».

- Unity: **6000.0.60f1**
- Пользовательских `.cs`: **163**, около **17 500** строк (без `Assets/external`, `Assets/Plugins`, `Assets/Editor`)
- Всего `.cs` в `Assets`: 214
- Сборки: `.asmdef` нет; всё в `Assembly-CSharp`, редакторные скрипты — `Assembly-CSharp-Editor`
- Сцен в `Assets`: 8, игровых две — `MainScene.unity`, `MainSceneLight.unity`; `EditorBuildSettings` содержит **пустой** список сцен ⚠
- Префабов: 1082, из них проектных ~76 (`Prefubs`, `Resources`, `PoolPefubs`, `sprites`, `Clowns_And_Monsters`)

## 1. Запуск и композиция

```
GameContext : UnityContext<Main>        (MonoBehaviour в сцене)
  └─ Main()                             Generator/core/GameContext.cs
       ├─ SetupContainer()              биндинги AsSingle
       └─ StartGame()                   пусто (заглушка)
ApplicationManager.Start()              Generator/core/Managers/ApplicationManager.cs
  └─ this.Inject()
       └─ OnInject()                    порядок важен:
            1. PrefubManager.InitPlayer/InitUnits/InitItems  (из полей сцены)
            2. TargetManager.Init()
            3. GuiPoolManager.SetPool(GuiPool.guiPool)
            4. MapGenerator.StartApplication()   ← сейчас сразу return
```

Ключевое следствие: **игровая логика собирается в рантайме**. Сцены и префабы почти не
содержат ссылок на игровые скрипты, поэтому поиск мёртвого кода по GUID неинформативен —
связи идут по типу через контейнер и `AddComponent`.

⚠ Не подтверждено, что `UnityContext.OnAdded` (private, вызывается через
`SendMessageUpwards("OnAdded")`) доходит до объекта в рантайме. Если нет — ни одна
инъекция не сработает.

## 2. IoC-контейнер (`Assets/external/IoC`)

Самописный, ~10 файлов.

| Свойство | Поведение |
|---|---|
| `AsSingle<T>()` | Настоящий синглтон: `Register(type, typeof(T))`, кэш в `_uniqueInstances` по конкретному типу (`Container.cs:141-144, 169-178`) |
| Транзиентных регистраций | Нет |
| Два интерфейса на один тип | Один общий экземпляр |
| Инъекция | Рефлексия по **публичным свойствам** с `[IoC.Inject]` (`Container.cs:103-109`); `AttributeUsage(Property)` — **поля не инжектятся** |
| Незарегистрированный тип | `Get` возвращает `null`, сеттер пропускается (`Container.cs:124-125, 161`) — **молча** |
| Идемпотентность `Inject()` | Нет: каждый вызов заново присваивает свойства и вызывает `OnInject()` (`Container.cs:131-132`) |
| `Build<T>()` | Использует `DesignByContract.Check.Ensure` — поведение в плеере ⚠ |

Практический вывод: `null` в поле с `[IoC.Inject]` — это почти всегда отсутствующий `Bind`
или инъекция по конкретному типу вместо интерфейса.

### Зарегистрированные сервисы (`GameContext.cs:17-40`)

`IRandom`, `ITargetManager` (**дважды**, строки 18 и 32), `IBackgroundManager`,
`IScreenUpdateManager`, `ICooldownManager`, `ITileDataProvider`, `ICollision`,
`IMapGenerator`**, `ILightRenderer`, `IOverlayLightShema`, `ILightShema`, `ILightUvProvider`,
`IPrefubManager`, `ISpriteManager`, `IItemManager`, `IPhysicsManager`, `IGOPoolManager`,
`IGuiPoolManager`, `ISwapItemManager`, `IMaskManager`.

**`MapGenerator` зарегистрирован только как `IMapGenerator`**, при этом `Torch.cs:12` и
`Block.cs:14` инжектят конкретный `MapGenerator`. ⚠ Резолвится ли конкретный тип — не проверено
(`Container.Get` прочитан не полностью). Если нет — `OnInject` у факела падает.

## 3. Подсистемы

### 3.1 Генерация мира — `Assets/Generator/MapGenerator.cs` (1321)

Пять ответственностей в одном классе:

| Блок | Строки |
|---|---|
| Константы, размеры, полосы высот | 19-29 |
| Инъекции, свойства поверхности | 31-125 |
| Режим мир/дом (`GetMap`, `ChangeMap`) | 126-143 |
| Генерация дома | 146-271 |
| Поверхность, холмы, трава | 273-361 |
| Базовые тайлы и шум | 363-400 |
| Пещеры (активная часть) | 413-462 |
| Вода и джунгли (**отключены**) | 467-505 |
| Деревья, трава на поверхности | 506-567 |
| HP и фоновый слой | 569-628 |
| Руды и камень | 631-708 |
| Нижний мир | 711-742 |
| `OnGenerate` / `IsGenerated` | 745-771 |
| Шахты и спавны (**мёртвые**) | 773-828 |
| Генератор деревьев | 830-1132 |
| Доступ к карте, `IsValidPoint` | 1134-1169 |
| Разрушение тайлов (`RemoveLine*`) | 1171-1317 |

Статическое состояние: `surface`, `surfaceHome` (`:34-35`) — **не очищаются** при регенерации.
⚠ После второй генерации `surface.Count` ≈ 2102, `CenterPos` (`:104`) указывает на край мира,
а проверка `point.x != surface.Count - 1` (`:350-352`) может выйти за массив.

### 3.2 Тайлы: данные и рендер

- `TileDataProvider.cs` (762) — доступ к карте, спрайты, конвертация координат, буфер кадров.
  Статические `Map`, `frameBuffer`, `mapSizeX/Y`; `OnGenerate` обновляет только `Map` и размеры,
  `frameBuffer` не пересоздаётся.
- `Terrain/InfinityTileMapMesh.cs` (488), `InfinityTileMapSets.cs` (217), `InfinityTileMapPixel.cs` (203) —
  три варианта одного и того же тайлового рендера, код во многом скопирован.
- `Terrain/TileRenderer.cs` (580) — оркестрация слоёв, `Update` с обработкой мыши и клавиш.
- `Terrain/LightRenderer.cs` (487) + `LightRendererOld.cs` (542) — свет; старый не используется.
- `Terrain/TileSham/*` — 7 схем автотайлинга. Каждая содержит свою копию `ChackMap`.

Три реализации рендера различаются стратегией: тайл = меш на тайл (Mesh), набор мешей
по размеру экрана (Sets), работа с текстурой попиксельно (Pixel). Слой `ObstacleLightLayer`
создаётся, но в `layerMeshs` не попадает — его `RenderMesh` не вызывается ни из одного места.

### 3.3 Вода — `Terrain/Liquid.cs` (811)

Симуляция сейчас **выключена**: `TileDataProvider.Update()` (`:738-741`) делает `return` до
`liquid.Update()`, `LiquidUpdate()` (`:748-749`) аналогично.

При этом `AddWater` продолжает вызываться из `TileDataProvider.cs:560` и писать в карту.
`Liquid` имеет общее изменяемое поле `posCenter` (`:106, :120`), которое читает `AddWater`
(`:25`) — это источник порчи «чужой» клетки.

Бюджет обновления (`maxWaterUpdate = 500`, `:111`) и троттлинг по кадрам (`:35`) закомментированы.

### 3.4 Физика

- `PhysicsManager.cs` (301) — секторная сетка `physicSectors`, регистрация, `ChackCollision`,
  `GetItemsInRange`. Никогда не очищается при смене мира.
- `Physics/Collision.cs` (267) — тайловые коллизии, рейкасты, кэш размеров карты
  (`_countTileMapX/Y`, `:25-47`) — **не инвалидируется** при `ChangeMap`.
- `Physics/PhysicItem.cs` (309), `MonoBehaviourPhysicItem.cs` (125) — тело и его поведение.
- `Physics/BufferList.cs` — фиксированный буфер на 100 записей; параметр `size` в
  `BufferItem` игнорируется.

### 3.5 Пулы

`core/utils/GOPool.cs` — базовый пул: кэши по префабу, `Spawn(prefab)`, `Spawn<T>`,
`Unspawn`, `activeCachedObjects`. `src/gui/GuiPool.cs` наследует его, но переопределяет
`Awake` **без** `base.Awake()`, поэтому `Inject()` и регистрация пула не выполняются —
регистрация делается вручную из `ApplicationManager.cs:44`.

### 3.6 Игрок, ИИ, оружие, предметы

- `src/PlayerController.cs` (247) — тело игрока, HP, смерть/респавн, подписка на
  `pysicItem.OnCollision` (снимается в `OnDestroy` — корректно).
- `src/PlayerModel.cs` (150) — модель: сумка, экипировка, `Resources.Load` оружия.
- `src/TargetSystem/TargetManager.cs` (226) — создание игрока и юнитов, трекинг `units`,
  урон по площади. `PlayerController` появляется только в `CreatePlayer()`.
- `src/ai/*` — `AiController` + три поведения. `BaseAIBehaviour` и `ZombieBehaviour`
  во многом скопированы, но различаются инициализацией направления и разворотом.
- `src/weapons/*` — `DamagePointBase` (в `Pick.cs`) и `DamagePoint` (в `Backsword.cs`)
  наносят урон тайлам/юнитам; включаются/выключаются из оружия.

### 3.7 UI

`src/gui/GameUi.cs` (269) строит uGUI в рантайме в `Awake` и **опрашивает модель каждый кадр**
в `Update` (строка HP, 36 слотов сумки, в том числе когда сумка скрыта).
`RootContext` / `RootModel` / `BagContext` / `PlayerComtext` — ручная «модель» UI поверх
статического `RootContext.Instance`.

## 4. Глобальное состояние

Полный список статических изменяемых полей, найденных в пользовательском коде:

| Поле | Файл | Сбрасывается |
|---|---|---|
| `RootContext.Instance` | `src/gui/RootContext.cs:4` | никогда (обычный C#-объект, Unity-проверка не работает) |
| `RootModel.instance` | `src/gui/RootModel.cs:7` | никогда |
| `MainMenu.instance` | `src/gui/MainMenu.cs:6` | никогда |
| `GameUi.Instance` | `src/gui/GameUi.cs:13` | никогда |
| `TouchInput.instance` | `src/TouchInput.cs:6` | никогда; **никем не читается** |
| `GuiPool.guiPool` | `src/gui/GuiPool.cs:7` | никогда |
| `MapGenerator.surface`, `surfaceHome` | `MapGenerator.cs:34-35` | никогда |
| `LightRenderer.color/minPos/maxPos` | `LightRenderer.cs:56-57,126` | ⚠ |
| `TileRenderer.screenStart/End` | `TileRenderer.cs:143-144` | никогда |
| `TileDataProvider.Map/frameBuffer/mapSizeX/Y` | `TileDataProvider.cs:39-44` | частично, в `OnGenerate` |
| `SpriteData.Sprites/SpritesByTypeId`, `LayerData.Layers` | `SpriteData.cs:6-7`, `LayerData.cs:7` | статические справочники, заполняются один раз |
| `ItemData.Items` | `src/Items/ItemData.cs:6` | сид-данные |
| `TileInfoStructure.Free/Free00` | `TileSham/TileInfoStructure.cs:25-26` | общий изменяемый экземпляр ⚠ |

## 5. Координаты «мир ↔ тайл»

В проекте сосуществуют три прямые формулы и три обратные:

| Формула | Где | Вид |
|---|---|---|
| A (канон) | `TileDataProvider.cs:269-275` | `Ceil((w + 1.5r) / r)` |
| A′ (копия A) | `PhysicsManager.cs:182-188` | то же самое |
| B | `Collision.cs:62-63` | `trunc((w + 1.5r)/r) + 1`, с клампом |
| C | `PhysicsManager.cs:295-298` | `Ceil((w + 2.0r) / r)` |
| a | `TileDataProvider.cs:284-285` | `p − 1.5r` |
| b | `Collision.cs:83-84` | `p − 2.0r` |
| c | `SwapItemManager.cs:107-108, 293-294` | `p − 2.5r` |

При `r = 16` формулы A и B расходятся на 1 клетку там, где `w + 1.5r` попадает ровно на
границу тайла, то есть в центрах тайлов (`w = 8, 24, 40 …`). ⚠ Значение `r` берётся из
`TileDataProvider.tileSize` (задаётся при `Init`), константа `Settings.TILE_SIZE = 16`
использовалась только для примеров.

## 6. Обработка событий

| Подписка | Снимается |
|---|---|
| `MapGenerator.OnGenerate += OnMapGenerate` (`TargetManager.cs:35`) | **нет** — повторный `CreatePlayer` при каждом переключении мир/дом ⚠ |
| `MapGenerator.OnChangeMap += OnChangeMap` (`TargetManager.cs:33`) | нет (синглтон, низкий риск) |
| `pysicItem.OnCollision += OnCollision` (`PlayerController.cs:77`) | да, `OnDestroy` (`:164`) |
| `Arrow.OnTerrainCollision/OnCollision` (`Arrow.cs:8-9`) | да (`:14-15`) |
| `cooldownShow` в `Bow`/`Grenade` | да, `OnDestroy` |
| `pool.OnUnspawnGO += FreeResources` (`GuiPoolManager.cs`) | нет; обработчик пустой |

Корутины: `WeaponAnim.Swing` запускается из оружия; при уничтожении оружия (смена слота)
корутина обрывается и `OnFinishedFire` не вызывается — для `Pick` это оставляет
`damagePoint` включённым.

## 7. Unity Lifecycle — как есть

- `ApplicationManager`: инъекция в `Start()`, кулдауны тикают в `FixedUpdate`.
- `GameUi`: строит UI в `Awake`, читает модель в `Update`.
- `RootModel`: `Start()` → `Inject()` → `OnInject()` создаёт `PlayerComtext`/`BagContext`.
- `GOPool`/`GuiPool`: `Awake` вызывает `Inject()`; у `GuiPool` переопределён без `base`.
- `BasePoolEffect`: `Start` **и** каждый `OnSpawn` вызывают `Inject()`.
- `OnDestroy` есть только у `PlayerController`, `Bow`, `Grenade`, `Arrow`. У тайлового рендера,
  `LightRenderer`, `Collision`, `PhysicsManager` очистки нет: меши, текстуры, инстансы
  материалов и статические поля живут до выгрузки.

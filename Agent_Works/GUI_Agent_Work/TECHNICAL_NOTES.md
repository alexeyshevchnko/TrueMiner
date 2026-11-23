# TECHNICAL_NOTES — устройство GUI проекта TrueMiner

## Drag & Drop предметов сумки (дополнение после ISSUE-13)
- Компонент `GameUi.BagSlotDrag` (вложенный, на каждой ячейке сумки И хотбаре, ставится в `MakeSlot`): IPointerDownHandler/IDragHandler/IPointerUpHandler — передаёт события в `GameUi.OnSlotPointerDown/OnSlotDrag/OnSlotPointerUp`. Button остаётся: OnPointerClick срабатывает только для короткого нажатия (ниже порога `pixelDragThreshold`); после фактического переноса Click подавляется (проверки: `dragCtx != null && dragMoved` и `Time.frameCount == lastDragEndFrame` — устойчиво к обоим порядкам доставки OnPointerUp/OnPointerClick в uGUI).
- «Призрак»: `GameUi.dragGhost` (Image, последний ребёнок канваса, raycastTarget=false, размер = ячейка сумки 46.2, спрайт `GuiAtlas.Get(Icon)`); позиция — `RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pos, null)` (ScreenSpaceOverlay → камера null); работает мышью и touch идентично.
- Жизненный цикл: PointerDown (открытая сумка + непустая ячейка + свободный указатель) запоминает pointerId и ячейку; Drag (порог uGUI Модуля превышен и модуль начал слать drag-события) поднимает призрак и скрывает иконку+количество источника (`Slot.dragHidden` учитывается в `SyncSlots`); PointerUp — целевая ячейка определяется `FindSlotUnder` (прямоугольники слотов, hotbar → bag).
- Завершение: пустая ячейка → `Bag.Move(srcKey, dstKey)` + `UpdateBag`; занятая — тот же Move (обмен слотов — существующее правило); исходная/вне ячеек/отмена — состояние сброшено, модель не тронута (предмет не теряется никогда; источник восстановлен).
- Concurrency: захват pointerId первого нажатия (мышь −1, палец ≥0); drag/up другого указателя игнорируются; новый PointerDown при активном драге отклоняется.
- Слоты → модель: `Slot.modelKey` (хотбар 1–9, сумка 10–45) — явная карта; `BagItemContext.Number` теперь тоже назначается в `UpdateCell` (раньше был 0 — Click/Set использовали неверный ключ; фикс возвращает и семантику двухэтапного переноса кликом и использование хотбара кликом).

## Масштаб интерфейсов
- Хотбар: `hotbarScale = 0.7` (сессия 3), сумка: `BagScale = 0.7` (сессия 8) — применяются ко всем размерам панели (ячейки/промежутки/поля/цифры/размер панелей). Панель сумки (0.5,0), y=92, поверх хотбара; при reference 1024×768 внутри экрана (491.4×225.4).

## Единый источник активации джойстиков (дополнение после ISSUE-11)
- Цепочка активации: палец/мышь (мышь — только в редакторе) → `TouchAreaRaycaster` (TouchAreaCanvas, sortingOrder −20; зоны TouchArea_Left и TouchArea_Right симметрично от краёв шириной `TouchAreaRaycaster.TOUCH_AREA_WIDTH_PERCENT`% — по умолчанию 50: 0–50% и 50–100%, нижние `TouchAreaHeightPercent`% высоты) → `JoystickTouchArea` (IPointerDown/IDrag/IPointerUp/IPointerEnter) → форвардинг того же PointerEventData в UltimateJoystick (Joystick1/Joystick2).
- Собственная графика джойстиков (Base, Joystick/Handle, Highlight, Tension) — `raycastTarget = false` (ставит `JoystickTouchVisibility.Awake`): их прямоугольники больше не зона активации; касание обрабатывается ровно один раз — зоной.
- Мышь над зоной: `OnPointerEnter` обнуляет `eventData.pointerEnter` (pointerId < 0) → `IsPointerOverGameObject(-1) == false` → прицел мышью работает в мировых областях; над UI pointerEnter остаётся → guard (KeyboardJoystick) блокирует выстрел. В билдах (`!Application.isEditor`) TouchAreaRaycaster мышь полностью игнорирует.
- Захват драга: module pointerPress = TouchArea → drag/pointerUp приходят в зону даже при уходе пальца за её границы → форвардинг продолжает работать; `_pointerId` ассета не меняется (multi-touch независим).
- Порядок канвасов: TouchAreaCanvas (−20) < GameUi (−10) < Ultimate UI Canvas (0): raycast-победа за UI, отрисовка джойстиков поверх сумки.

## Масштаб джойстиков и преобразование координат (дополнение после ISSUE-10)
- Пользовательский коммит `ec5a1ea scale` ставит localScale 0.7 на оба экземпляра джойстиков (root 22473068). UltimateJoystick оперирует мировыми пикселями (Screen Space Overlay: world == screen), но размеры/смещения хранит в ЛОКАЛЬНЫХ единицах папки → любой микс даёт смещение `(1 − scale) × половина размера` (при 0.7: 23.54px — совпало с измерением).
- Конвертация: `world = local × folder.lossyScale.x` (хелпер `GetFolderScale()`).
- Точки применения: dynamicPositioning (`folder.position = touch − textureCenter × scale`) и `joystickCenter` (полразмер × scale) в `UpdateSizeAndPlacement`; OnPointerDown дополнительно центрирует Handle.
- Чувствительность/скорость НЕ менялись: оси = `(handle − center)/radius` в мировых пикселях; `radius` остаётся локальным (`size × radiusModifier/10`).
- Замечание (косметика, вне объёма): при полной отклонённости на 0.7-масштабном джойстике Handle может чуть выступать за видимый обод (обoд мировой = radius × 0.7, кламп — radius). Если потребуется идеальная посадка — уменьшить `joystickSize` (сценовый оверрайд, например 3.27 → 2.29) и убрать scale 0.7, тогда радиусы совпадут.
- Параметр высоты зон: `JoystickTouchArea.touchAreaHeightPercent` (`[SerializeField, Range(10,100)]`; значение по умолчанию — константа `TouchAreaRaycaster.TOUCH_AREA_HEIGHT_PERCENT`, 50) — в Inspector каждой зоны (TouchArea_Left/TouchArea_Right в Play Mode, объект → компонент JoystickTouchArea → «Touch Area Height Percent»). Геометрия пересчитывается в Awake и каждый Update: якорьMin.y=0, якорьMax.y=процент/100. Ширина зон — константа `TouchAreaRaycaster.TOUCH_AREA_WIDTH_PERCENT` (по умолчанию 50), задаётся при создании зон в `GameUi`: `anchorMin.x = 1−w` / `anchorMax.x = w` симметрично от краёв (константа 50 → 0–0.5 / 0.5–1).

## Мобильное управление (дополнение после ISSUE-7..9)
- Ассет Ultimate Joystick: `UltimateJoystick` (IPointerDown/IDrag/IPointerUp на корневом GO), визуал — `JoystickSizeFolder` (pivot 0,0; Base/Joystick/Tension — stretch-дети). Ось = `(handle.position − joystickCenter)/radius`, БЕЗ проверки удержания → любое рассогласование хэндла и центра даёт постоянный дрейф персонажа.
- `UltimateJoystickScreenSizeUpdater` (UIBehaviour на канвасе джойстиков) на `OnRectTransformDimensionsChange` запускает в конце кадра `UpdatePositioning` для всех джойстиков = `ResetJoystick` (по старому центру) → `UpdateSizeAndPlacement` (новый центр). Хэндл при этом не возвращался в центр — первопричина дрейфа при старте/ресайзе (ISSUE-7).
- Патчи ассета (минимальные): `UpdateSizeAndPlacement` в конце центрирует хэндл; `Awake` (play mode) сразу вычисляет размещение; `OnApplicationFocus(false)/OnApplicationPause(true)` → `ResetJoystick` (анти-залипание).
- Канвасы по порядку: TouchAreaCanvas (−20, кастомный `TouchAreaRaycaster`) → GameUi (−10) → Ultimate UI Canvas (0). TouchAreaRaycaster игнорирует мышь (pointerId<0) и возвращает левую/правую половины экрана (anchors 0–0.5 / 0.5–1, полная высота) для тач-пальцев; межканвасный raycast-приоритет EventSystem = sortingOrder → интерфейс всегда важнее зон; в свободных областях зоны получают события.
- `JoystickTouchArea` (на зоне) пересылает PointerEventData (с fingerId) в целевой джойстик; multi-touch обеспечивает `_pointerId` ассета.
- `JoystickTouchVisibility` — только видимость визуала по `GetJoystickState` (создание зоны/подавление при сумке удалены — не нужны при правильной маршрутизации).
- Связь с персонажем: `KeyboardJoystick` читает `GetHorizontalAxis/GetVerticalAxis("Joystick1")` (движение) и `"Joystick2"` (прицел/огонь), `GetIsDown("Joystick1")`, огонь также по `Input.GetMouseButton(0)` с исключением `IsPointerOverGameObject()` (мышь в зоны не попадает — десктопное прицеливание не пострадало).

## Мобильное управление (дополнение после ISSUE-5)
- Джойстики — ассет Ultimate Joystick (внешний, `Assets/external/Ultimate Joystick`), два экземпляра префаба в сцене: `Joystick1` (движение, экземпляр с именем «Minimalist Joystick 00», dynamicPositioning=1) и `Joystick2` («Ultimate Joystick2», dynamicPositioning прописан 1 коммитом 260a74f), оба в «Ultimate UI Canvas» (ScreenSpaceOverlay, sortingOrder 0, GraphicRaycaster).
- Канвас GameUi создаётся программно с `sortingOrder = -10` — ниже канваса джойстиков: джойстики рисуются поверх сумки/ячеек/HUD.
- Структура префаба: корневой GO (обработчики IPointerDown/IDrag/IPointerUp + `UltimateJoystick`) → дочерний `JoystickSizeFolder` (весь визуал: Base, Joystick, 4 tension). Визуал можно прятать независимо от ввода.
- Multi-touch: ассет хранит `_pointerId` пальца, начавшего касание; OnDrag/OnPointerUp с другим pointerId игнорируются; `joystickState` блокирует второй палец на том же джойстике. Два джойстика = два независимых пальца.
- Hit-area: raycast идёт по Graphic — раньше это был визуал; теперь невидимый `TouchArea` (Image alpha 0, 160×160, raycastTarget=true) у корня; при открытой сумке его raycastTarget=false (новые касания не активируют джойстик, удерживаемый — продолжает: модуль держит pointerPress до pointerUp).
- Регистрация: `UltimateJoystick.Awake` кладёт экземпляры в статический словарь по `joystickName`; `GetUltimateJoystick(name)` возвращает null, если нет.
- `JoystickTouchVisibility` подключается в `GameUi.Start` (после всех Awake, чтобы реестр был заполнен).

## Хотбар (дополнение после ISSUE-6)
- `GameUi.Build`: hotbarSlots=7, hotbarScale=0.7; размеры панели из исходной формулы ×0.7 (365.4×54.6), ячейка 46.2, промежуток 4.2, отступы 8/4 (.RoundToInt(12*0.7) и др.).
- Модель: `BagContext.Items` — 9 элементов (Number 1..9); UI-массив `hotbar` — 7 `Slot`; `SyncSlots(hotbar, p.Bag.Items)` маппит первые 7 элементов; сумка — 36 ячеек без изменений.
- Выбор активного слота: `PlayerComtext.SelectItem(id)` (BgIcon=cellSelect) — работает на уровне модели для 1..9; отображается подсветка только для видимых ячеек.
- Оформление количества масштабируется scale-параметром `MakeSlot/AddCountText`: шрифт 14 (=20×0.7), BestFit 7–14, Outline 1.05 (=1.5×0.7), rect поля цифр 36.4×16.8 у правого нижнего угла ячейки.

## IoC-инициализация и порядок Awake (дополнение после ISSUE-3)
- Корень контейнера: `GameContext : UnityContext<Main>` на корневом объекте сцены MainScene (guid скрипта 6027da7a…). `UnityContext<T>.Awake()` создаёт `_applicationRoot = new T()` = `Main()`, который конструирует `IoC.UnityContainer` и все биндинги.
- `MonoInjection.Inject(this MonoBehaviour)` — extension: `SendMessageUpwards("OnAdded", script)`. Сообщение поднимается по всем предкам, обработчики `OnAdded` срабатывают на каждом объекте-предке, где есть такой метод.
- `GOPool.Awake()` (строка 137) вызывает `this.Inject()` — все наследники пула (GuiPool, POOL наchildren…) инъекцируются через `OnAdded` на `GameContext`. Так как `scriptingDefineSymbols: {}`, проверки `DesignByContract.Check.Require` вырезаны `[Conditional]`-атрибутами.
- Исправление: `[DefaultExecutionOrder(-1000)]` на `GameContext` (Awake корня раньше всех default-объектов) + ленивый `EnsureApplicationRoot()` в `UnityContext<T>` (без двойных контейнеров).

## Общая архитектура GUI
- GUI полностью программный (без префабов для работающего UI): `RootModel` (компонент на объекте «UI Root» сцены `MainScene.unity`) в `Awake()` вызывает `GameUi.EnsureCreated()`, тот создаёт `GameObject("GameUi", GameUi)`.
- `GameUi.Build()` строит: ScreenSpaceOverlay Canvas (CanvasScaler 1024×768), EventSystem (создаётся только если в сцене его нет — в сцене он есть, со `StandaloneInputModule`), HP-бар, кнопки Flag («Домой» — смена карты/возврат на сгенерированную домашнюю зону через `MapGenerator.ChangeMap()`), Bag, FPS-текст, хотбар (9 слотов, GridLayoutGroup), BagPanel (36 слотов, открытие по `p.IsShowBag` и клавишам B/Tab).
- Данные читаются из plain-моделей каждый кадр (`GameUi.Update()` → `RootContext.Instance.Player` → `PlayerComtext`):
  - `PlayerComtext.Bag` — `BagContext` с двумя списками `Items` (9 шт., хотбар, Number 1..9) и `BagItems` (36 шт., Number 10..45).
  - `PlayerComtext.UpdateBag(Bag)` (`Assets/src/Bag.cs` — модель) заполняет поля `BagItemContext.Count/Icon/Empty`.
  - `BagItemContext.Icon` — строка с именем иконки, пришедшая из `IItem.GetUiIcon()`.
  - Клик по ячейке: `BagItemContext.Click()` → хотбар: `UseItem(Number)`; в открытой сумке — перенос предметов (Get/Set через `PlayerComtext.MoveItem`).

## Иконки и атлас
- Атлас: `Assets/Resources/gui/gui.png` (256×512, RGBA, загружается как Texture2D), описание кадров — `Assets/Resources/gui/gui.txt` (TexturePacker JSON, формат RGBA8888, scale 1). Шрифт comic.ttf используется старым NGUI.
- `gui.png.meta`: TextureImporter `spriteMode: 1` (Single), `spriteSheet.sprites: []` → Unity-спрайтов отдельных кадров не существует.
- Кадры gui.txt (32×32 у предметов; cell/cellSelect 48×48; BtnFlag/Button/ButtonFree 97×97; inventary 30×29; pgLife/pgLifeBG; arrow1/arrow2): Axe, BtnFlag, Button, ButtonFree, Item1, arrow1, arrow2, bow2, brick, cell, cellSelect, coal, gold, granade1, green, ground, ice, inventary, kirka, kost, meat, med, pgLife, pgLifeBG, snow, stal, stone, torch, wood.
- Имена иконок предметов из `GetUiIcon()`: torch (TorchItem), kirka (PickItem), granade1 (GrenadeItem), bow2 (BowItem), Axe (AxeItem), Item1 (BackswordItem), и блоки BlowBlockItem: ground, green, coal, gold, wood, brick, kost, meat, med, stal, stone (в коде есть проверка в switch по типу блока).
- TexturePacker хранит координаты кадра от верхнего левого угла; Unity `Sprite.Create_rect` требует прямоугольник от нижнего левого → `rectY = textureHeight - (frame.y + frame.h)`.
- Регистронезависимый lookup обязателен («Axe», «Item1» — с заглавными; остальные маленькие). Ключи хранятся и с расширением .png, и без него.
- Бинарный `guiMain.prefab` (Unity 2017, NativeFormatImporter) содержит старый UIAtlas со своей таблицей координат — она НЕ соответствует текущей раскладке gui.png/gui.txt (в префабе x почти всегда 2, y растёт по 34px — это раскладка какого-то старого атласа). Префаб не используется ни сценой, ни кодом; источник истины — gui.txt.

## Загрузка ресурсов
- Только `Resources.Load` по путям без расширений: `gui/gui` → и TextAsset (gui.txt), и Texture2D (gui.png).
- Иных атласов/локаторов спрайтов для GUI не существует (`SpriteManager` в Generator — только тайлы карты).

## Кнопки и ввод
- `ProjectSettings.activeInputHandler: 0` (Input Manager / legacy), пакет `com.unity.inputsystem` не установлен → используется `StandaloneInputModule` из сцены.
- Кнопки — `Button` (ugui) с дефолтной Transition ColorTint и Navigation Automatic. Решение проблемы «Selected + Пробел»: рассинхронизация выбора после Pointer Click (компонент `DeselectOnClick`).
- `Input.GetKeyDown(B/Tab)` в `GameUi.Update()` — открытие сумки с клавиатуры (не ломать).

## Смешанные оболочки (не активные)
- «UI Root» (позиция -10000): NGUI-остатки, `MainMenu` + объект `btnFlag` (кнопка флага), `MainMenu.ChangeElement(MainMenuElement.Flag,...)` вызывается из `Assets/src/Items/Flag.cs` по дистанции до игрока — старый виджет, лежит вне действующего Canvas.
- `guiPOOL`/`GuiPoolManager` — пул объектов GUI для спавна прочей игровой графики (ячейки инвентаря не использует).
- Ultimate Joystick / TouchJoystick — мобильное управление, отдельный canvas.

## Проверка компиляции
- Unity 6000.6.10f1 установлена в `D:\Unity\6000.6.10f1\Editor\Unity.exe`. Проверка: `Unity.exe -batchmode -quit -projectPath D:\projects\TrueMiner -logFile ...` и анализ лога на ошибки компиляции. Пока проект открыт в редакторе, batch-прогон недоступен; альтернатива — `dotnet msbuild` по Unity-сгенерированным csproj (использовано в сессии 7).

## Расположение документации
- С 2026-11-23 документация всех задач перенесена в `Agent_Works/`: `Agent_Works/GUI_Agent_Work` (GUI и TouchAreaRaycaster — основная), `Agent_Works/Unity_InputSystem_Work`, `Agent_Works/Unity_Migration_Work`, `Agent_Works/Unity_Warnings_Work`. Новые отчёты GUI пишутся только здесь.

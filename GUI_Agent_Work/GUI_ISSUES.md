# GUI_ISSUES — реестр проблем интерфейса

## ISSUE-7: Персонаж самопроизвольно движется влево при запуске — ИСПРАВЛЕНО

**Фактическая причина:** движение персонажа в `KeyboardJoystick.Update` определяется `UltimateJoystick.GetHorizontalAxis("Joystick1")` = `(handle.position − joystickCenter)/radius` **без проверки удержания**. Хэндл («Joystick», stretch-потомок JoystickSizeFolder) обязан совпадать с центром. `UltimateJoystickScreenSizeUpdater` при любом изменении размеров канваса (в т.ч. при запуске в редакторе/на устройстве) вызывает `UpdatePositioning` = `ResetJoystick()` (ставит хэндл по **старому** центру) → `UpdateSizeAndPlacement()` (пересчитывает **новый** центр), и хэндл остаётся в старой точке → постоянное смещение оси (влево, если окно стало шире) без всякого касания. `Awake` джойстика тоже не вычислял центр до Start.

**Исправление (ассет, коммит `Fix mobile joystick input and touch areas`):**
- В конце `UpdateSizeAndPlacement()` хэндл ставится точно в центр (`joystick.position = joystickCenter`) — любая перестройка размещения заканчивается нейтральным состоянием.
- `Awake` (игровой режим) вызывает `UpdateSizeAndPlacement()` — центр валиден с первого кадра.
- `OnApplicationFocus(false)` / `OnApplicationPause(true)` → `ResetJoystick()` — потеря фокуса/пауза не оставляют «залипший» ввод; после отпускания пальца сброс уже обеспечивался `OnPointerUp`.

Активное значение оси при удержании не тронуто — механика управления прежняя.

**Статус:** исправлено; PlayMode-тест проверяет нейтраль всех 4 осей обоих джойстиков до касаний и после отпускания (Passed).

## ISSUE-8: Base появляется со смещением относительно точки касания — ИСПРАВЛЕНО

**Фактическая причина:** весь визуал (Base/Handle/Tension) — дети `JoystickSizeFolder` с pivot (0,0); `OnPointerDown` (dynamicPositioning) ставит `folder.position = touch − textureCenter`, т.е. центр = точке касания, **но** каждую перестройку канваса та же корутина `UltimateJoystickScreenSizeUpdater` в конце кадра возвращала папку в угол (`defaultPos`) и пересчитывала центр — при живом касании джойстик «уползал» в угол/смещался относительно пальца. Видимого смещения также добавлял хэндл, не возвращённый в центр (см. ISSUE-7, тот же механизм).

**Исправление:** пересчёт размещения заканчивается точной центровкой хэндла и не мешает dynamicPositioning — при удержании события Drag вновь смещают хэндл относительно центра точного; корутина-апдейтер больше не оставляет рассинхрона, потому что `UpdateSizeAndPlacement` теперь полностью приводит состояние к согласованному (папка в defaultPos/точке, центр пересчитан, хэндл по центру). PlayMode-тест: dispatch pointer down в точку экрана → центр визуальной папки совпадает с точкой касания (допуск 4px, канвас overlay: world == screen pixels) — Passed на реальном разрешении окна Unity.

**Статус:** исправлено; позиция Handle при движении пальца считается от центра Base (рансформ-логика ассета не менялась).

## ISSUE-9: TouchArea должны занимать половины экрана и не блокировать интерфейс — ИСПРАВЛЕНО

**Было (сессия 3):** зоны касания джойстиков — невидимые Image 160×160 по центру корневого прямоугольника джойстика на канвасе выше интерфейса GameUi: угловые зоны могли перекрывать ячейки хотбара/сумки (канвас джойстиков выигрывал raycast) — блокировка интерфейса и «случайная» активация джойстиков.

**Исправление (коммит `Fix mobile joystick input and touch areas`):**
- Новый программный канвас `TouchAreaCanvas` (GameUi.Start, ScreenSpaceOverlay, sortingOrder −20 — ниже GameUi −10 и Ultimate UI Canvas 0):
  - `TouchArea_Left`: якоря (0,0)–(0.5,1) — вся левая половина экрана по X и вся высота;
  - `TouchArea_Right`: якоря (0.5,0)–(1,1) — правая половина.
- Вместо GraphicRaycaster на канвасе — собственный `TouchAreaRaycaster : BaseRaycaster`:
  - игнорирует мышь (pointerId < 0) — на десктопе мышь остаётся прицеливанием/огнём (`KeyboardJoystick`, `Input.GetMouseButton`), зоны — только тач-пальцы;
  - возвращает зоны только если палец внутри их прямоугольника (`RectTransformUtility.RectangleContainsScreenPoint`).
- `JoystickTouchArea` (на каждой зоне) реализует `IPointerDownHandler/IDragHandler/IPointerUpHandler` и пересылает события в целевой джойстик тем же `PointerEventData` (сохраняется fingerId) — multi-touch независимость и `_pointerId`-логика ассета сохраняются; касание левой половины не активирует правый и наоборот.
- Маршрутизация: EventSystem выбирает верхний raycast-результат; сортировка межканвасная по sortingOrder (GameUi −10 > TouchAreas −20) → кнопки/сумка/ячейки всегда важнее зон; в свободных областях зоны получают события. Старые 160×160 зоны-дети джойстиков удалены (компонент `JoystickTouchVisibility` упрощён до видимости визуала).
- Открытая сумка не подавляет зоны (это больше не нужно — маршрутизация корректна): в свободной области экрана рядом с открытой сумкой джойстик работает.

**Статус:** исправлено; PlayMode-тест проверяет геометрию половин, прозрачность, порядок канвасов, raycast (мышь — нет, пальцы — да, правильные половины) и сквозной форвардинг (PointerDown прямо в TouchArea → джойстик удерживается) — Passed.


## ISSUE-5: Джойстики постоянно видны и перекрываются сумкой — ИСПРАВЛЕНО

**Было:** левый (Joystick1) и правый (Joystick2, Ultimate Joystick, канвас «Ultimate UI Canvas») всегда видны; канвас GameUi (сумка) создавался с sortingOrder 0 — как и канвас джойстиков, порядок отрисовки не гарантирован.

**Исправление (коммит `260a74f`):**
- `GameUi.Build()`: `canvas.sortingOrder = -10` — джойстики теперь всегда поверх сумки/ячеек/HUD.
- Новый `JoystickTouchVisibility` (подключается в `GameUi.Start` к обоим джойстикам через `UltimateJoystick.GetUltimateJoystick`): визуальная часть (`JoystickSizeFolder`) скрыта без удержания и появляется на время касания; при отпускании скрывается. Визуал не создаётся заново — переиспользуется существующий (без мигания/пересоздания).
- Невидимая зона касания: `Image` (alpha 0, raycastTarget=true) 160×160 по центру корневого прямоугольника джойстика — раньше зона касания совпадала с видимыми графами, при скрытом визуале касания бы пропали. 160×160 выбрано меньше корневой 223×223, чтобы не перекрывать ячейки хотбара (см. TECHNICAL_NOTES).
- Открытая сумка: у зоны джойстика `raycastTarget=false` — новые касания идут в ячейки сумки; уже удерживаемый палец продолжает управлять (StandaloneInputModule держит захват pointerPress до отпускания).
- Joystick2 в сцене: `dynamicPositioning: 0 → 1` — правый джойстик появляется в месте начала касания (как и левый); оси/радиус/механика не менялись.
- Multi-touch: уже реализован в UltimateJoystick (`_pointerId` в OnDrag/OnPointerUp, `joystickState` блокирует второй палец на том же джойстике) — два джойстика управляются двумя пальцами одновременно и независимо; проверено тестом с pointerId 1 и 2.

**Статус:** исправлено; PlayMode-тест `GameStartSmokeTests` (появление/скрытие обоих джойстиков, независимость, порядок канвасов, raycastTarget при открытой сумке) — Passed.

## ISSUE-6: Панель быстрого доступа — 9 ячеек, слишком крупная — ИСПРАВЛЕНО

**Было:** хотбар GameUi — 9 ячеек 66×66.

**Исправление (коммит `f9b26e6`):** 7 ячеек; масштаб 0.7 — ячейка 46.2, промежутки 4.2, отступы пропорционально, размеры панели из прежней формулы ×0.7 (365.4×54.6); панель по-прежнему прижата к низу и отцентрирована по X (pivot 0.5/якорь 0.5); иконки и количество масштабируются вместе с ячейками (BestFit max 14 = 20×0.7, контур 1.05 = 1.5×0.7, поле цифр 36.4×16.8). **Модель не изменена:** `Bag.Items` хранит 9 элементов, сумка — 36 ячеек; панели видны первые 7, слоты 8–9 остаются доступны модели (клавиши 8/9, выбор активного слота через `PlayerComtext.SelectItem`).

**Статус:** исправлено; PlayMode-тест `HotbarSevenSlotsTests` — Passed (7 ячеек, размеры ×0.7, центрирование, модель 9/36).


## ISSUE-3: NullReferenceException при запуске (UnityContext<T>.OnAdded, GuiPool/GOPool) — ИСПРАВЛЕНО

**Симптом (стек из задания):**
```
NullReferenceException: Object reference not set to an instance of an object
UnityContext`1[T].OnAdded (Assets/external/IoC/IoC/UnityContext.cs:36)
UnityEngine.Component:SendMessageUpwards(String, Object, SendMessageOptions)
MonoInjection:Inject(MonoBehaviour) (Assets/external/IoC/IoC/MonoInjection.cs:7)
GOPool:Awake() (Assets/Generator/core/utils/GOPool.cs:137)
GuiPool:Awake() (Assets/src/gui/GuiPool.cs:21)
```

**Разбор цепочки вызовов:**
- `GuiPool.Awake()` (Assets/src/gui/GuiPool.cs:21) → `base.Awake()` = `GOPool.Awake()` (строка 137) → `this.Inject()` (extension `MonoInjection.Inject`, MonoInjection.cs:7) → `SendMessageUpwards("OnAdded", ...)`.
- Объект `guiPOOL` — прямой ребёнок корневого объекта `GameContext` сцены MainScene (Transform father fileID 1893496967), поэтому `SendMessageUpwards` находит метод `OnAdded` на компоненте `GameContext` (класс `UnityContext<Main>`, guid 6027da7a...).
- Строка 36 `UnityContext<T>.OnAdded`: `_applicationRoot.container.Inject(component)`. `_applicationRoot` создаётся только в `Awake()` этого же компонента (строка 20). Если `Awake` ещё не выполнялся — `_applicationRoot == null` → NRE на строке 36.
- Защитная проверка на строке 33 `DesignByContract.Check.Require(...)` **не работает**: метод помечен `[Conditional("DBC_CHECK_ALL"), ...]`, а в `ProjectSettings.asset` `scriptingDefineSymbols: {}` — символы не определены, вызов вырезается компилятором. Поэтому падение выглядит как «голый» NRE вместо понятного исключения (сам текст проверки прямо называет причину: «possible script execution order problem»).

**Перво причина:** Script Execution Order в проекте не задан (`ProjectSettings` не содержит настроек порядка), значит порядок `Awake` между объектами сцены недетерминирован. Объект `GameContext` стоит в конце файла сцены, `guiPOOL` — раньше; Unity выполнил `GuiPool.Awake` раньше `GameContext.Awake`, и инъекция пришла в незапущенный контекст. Это проблема конфигурации инициализации (порядок/запуск `Awake`), а не наследования `GuiPool : GOPool` и не «повторного вызова Awake» — вызов один, корректный.

**Исправление (первопричина, минимальное):**
1. `Assets/Generator/core/GameContext.cs` (вне GUI — оправдание: это точка создания IoC-контейнера, без неё любой Awake дочернего объекта может упасть): атрибут `[DefaultExecutionOrder(-1000)]` на `GameContext`. Теперь `Awake` корня контейнера выполняется раньше всех объектов с порядком по умолчанию (0), включая оба пула — тот самый «script execution order fix», который подразумевался исходным `Check.Require`, но не был настроен.
2. `Assets/external/IoC/IoC/UnityContext.cs`: `EnsureApplicationRoot()` — ленивая инициализация в `OnAdded`, если `Awake` ещё не успел выполниться. Это НЕ маскировка «return при null»: инъекция выполняется полностью, компонент получает рабочее окружение. Повторная инициализация в `Awake` защищена — контейнер не пересоздаётся, чтобы не плодить вторые синглтоны.
3. Символы `DBC_CHECK_*` сознательно не включались (глобальное изменение компиляции не требуется для исправления).

**Статус:** исправлено; проверка — batch Play Mode smoke-прогон (см. TEST_REPORT, раздел «Прогон запуска»).

## ISSUE-4: Оформление количества предметов (Comic + чёрный Outline) — ИСПРАВЛЕНО

**Было:** цифры количества — обычный Unity UI `Text` (TextMeshPro в проекте не используется) встроенным шрифтом LegacyRuntime/Arial, без контура.

**Исправление (`GameUi.AddCountText`):**
- Шрифт: существующий ресурс `Resources/gui/comic.ttf` (импортирован как обычный `Font`, грузится `Resources.Load<Font>("gui/comic")`), с fallback на прежний шрифт и warning, если ресурс пропадёт. Остальные надписи (HP, кнопки, FPS) продолжают использовать прежний шрифт — их оформление не менялось.
- Контур: компонент `UnityEngine.UI.Outline`, `effectColor` — непрозрачный чёрный, `effectDistance = (1.5, -1.5)` (4 диагональных копии глифов — ровный контур, читается на светлых и тёмных иконках).
- Разметка: прежняя позиция — правый нижний угол ячейки (offset −4,4; 52×24), чтобы не закрывать центр иконки; включён BestFit (10…20 pt) — многозначные числа не обрезаются и не переносятся, однозначные остаются крупными.
- Общее для сумки и хотбара: обе панели создаются одним `MakeSlot`, оформление единое по построению.

**Статус:** исправлено; визуальная и Play Mode-проверка — см. TEST_REPORT.


## ISSUE-1: Иконки предметов не отображаются (цветная заливка) — ИСПРАВЛЕНО; автотесты и компиляция пройдены (ручной Play Mode-клик — не выполнялся)

**Симптом:** количество предметов в ячейках отображается, иконки — нет (вместо них плоская цветная заливка).

**Причина (подтверждено кодом):**
- `GameUi.SyncSlots()` (Assets/src/gui/GameUi.cs, строки 219 и 224–229) назначает иконке только `Image.color` через метод `IconColor()`, в котором есть комментарий-признак заглушки: «Временная заливка; точные спрайты атласов подключу следующим шагом». Спрайт (`Image.sprite`) никогда не назначается.
- Атлас `Assets/Resources/gui/gui.png` (256×512, фактический размер проверен) никак не загружается в действующем GUI: в проекте нигде нет `Resources.Load` по пути `gui/gui` (единственные `Resources.Load` в src — шрифт в GameUi и оружие в PlayerModel).
- `gui.txt` — это TexturePacker JSON с описанием кадров (Axe.png, BtnFlag.png, Item1.png, kirka.png, torch.png, cell.png, cellSelect.png и т.д.).
- Импорт `gui.png.meta`: `spriteMode: 1` (Single), `spriteSheet.sprites: []` — Unity-спрайтов с именами кадров не существует, поэтому даже загрузка `Sprite` целиком не дала бы иконки предметов.
- Бинарный `guiMain.prefab` содержит старый UIAtlas со своей таблицей координат, которая НЕ совпадает с текущей раскладкой `gui.txt` (в префабе вертикальная раскладка x=2/y=136..., в txt — другая). Полагаться на префаб нельзя; источник истины — `gui.txt`.
- Имена `GetUiIcon()` предметов (torch, kirka, granade1, bow2, Axe, Item1, ground/green/coal/gold/wood/brick/kost/meat/med/stal/stone) совпадают с именами кадров `gui.txt` (без расширения .png) — регистр кадров сохранён («Axe», «Item1»), значит lookup должен быть регистронезависимым.

**Путь данных (было/стало):** модель Bag (`Assets/src/Bag.cs`) → `PlayerComtext.UpdateBag/UpdateCell` → `BagItemContext.Icon` (string) → `GameUi.SyncSlots`. Строка Icon корректно доходит до GUI (поэтому счётчик работает), но GUI не превращал её в спрайт.

**Исправление:** новый класс `GuiAtlas` (Assets/src/gui/GuiAtlas.cs) — ленивая загрузка `Resources.Load<Texture2D>("gui/gui")` + парсинг `Resources.Load<TextAsset>("gui/gui")` мини-парсером JSON, создание и кэширование спрайтов через `Sprite.Create` (Y атласа пересчитывается: Unity меряет снизу). `GameUi.SyncSlots` теперь назначает клиентам `Image.sprite` из GuiAtlas по `ctx.Icon`, `Color.clear` для пустых ячеек; иконка меняется только при изменении значения.

**Статус:** исправлено (коммит `baa2db6`). Компиляция — без ошибок; EditMode-тесты атласа 4/4 Passed (`editmode_final.xml`); имена кадров сверены с изображениями визуально (`preview/atlas_check_row.png`). Ограничение: реальный «клик курсором» в Play Mode — не выполнялся (см. TEST_REPORT §4).

## ISSUE-2: Кнопка остаётся Selected и активируется пробелом после Pointer Click — ИСПРАВЛЕНО; EditMode+PlayMode автотесты пройдены (ручной «живой» ввод — не выполнялся)

**Симптом:** клик мышью/тачем по кнопке «Домой» (Flag) или сумке (Bag) оставляет кнопку выбранным элементом; пробел затем повторно её активирует (Submit).

**Причина (подтверждено кодом и сценой):**
- Действующие кнопки — не экземпляры `guiMain.prefab`, а программные `Button` из `GameUi.Build()`: Flag (`AddButton(canvasGo.transform, "Flag", ...)`, onClick → `MapGenerator.ChangeMap()` — генерация домашней карты) и Bag (onClick → `ToggleBag()`).
- Это стандартное поведение uGUI: `StandaloneInputModule` (в сцене `MainScene.unity`, объект EventSystem, activeInputHandler = 0) при нажатии на `Selectable` делает его `currentSelectedGameObject`; выбранный объект получает Submit (пробел) как `OnSubmit` → `Button.Invoke()` до следующего выбора.
- В коде GameUi нет никакого сброса выбора после клика.

**Исправление (две штатные меры uGUI):**
1. `Navigation.Mode.None` для Flag и Bag (`GameUi.AddButton`): `Selectable.OnPointerDown` (uGUI 6000.0, Selectable.cs:1206) выбирает кнопку только при navigation.mode != None, поэтому после клика мышью/тачем кнопка вообще не становится Selected и не может получить Submit от пробела.
2. Новый компонент `DeselectOnClick` (Assets/src/gui/DeselectOnClick.cs), `IPointerClickHandler`: в `OnPointerClick` сбрасывает `SetSelectedGameObject(null)`, если выбран именно этот GameObject — страховка на случай выбора иным путём. EventSystem ищется через `FindFirstObjectByType` (статический `EventSystem.current` в batch-среде не заполнен; в игре это тот же единственный EventSystem). Подключён также к кнопкам ячеек слотов (`MakeSlot`) — тот же дефект.

Это не маскировка: клавиатурный выбор других элементов, переходы стрелками/Tab и Submit на явно выбранных элементах работают (покрыто контролем в PlayMode-тесте). Навигация слотов оставлена Automatic.

**Статус:** исправлено (коммит `bf7c2e7`). Компиляция без ошибок; PlayMode-тесты 3/3 Passed (`playmode_results.xml`), EditMode-тесты компонента 2/2 Passed (`editmode_final.xml`). Ограничение: реальный ввод курсором в Play Mode — не выполнялся (см. TEST_REPORT §4).

## Наблюдения вне объёма работ (не чинить, только зафиксировать)
- `FlagBtn.cs` не вызывается никаким serialized onClick в сцене; класс почти мёртвый, `MainMenu` используется только `Flag.cs` (перемещение btnFlag в скрытой NGUI «UI Root», вынесенной в (-10000,-10000,-10000)).
- `TouchJoystick`/Ultimate Joystick canvas — отдельная manage-система, изменений не требует.

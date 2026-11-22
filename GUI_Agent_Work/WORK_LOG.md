# Журнал работы (WORK_LOG)

## Сессия 6 (2026-11-22): TouchArea как единственный источник активации

### Проблема
Пользователь: джойстики появляются только при касании собственных прямоугольников Ultimate Joystick/Ultimate Joystick2, TouchArea не управляют появлением.

### Причина
Реализация сессий 4–5 оставляла два параллельных источника: TouchAreaRaycaster пропускал мышь (pointerId<0) → в редакторе зоны не отвечали; активация шла через raycast-графику самих джойстиков (Base/Handle/Highlight), чьи rect'ы и оставались фактическими зонами появления.

### Исправление
- `JoystickTouchVisibility.Awake`: всем Graphic под джойстиками `raycastTarget = false` — прямая активация через прямоугольники джойстиков исключена (двойная обработка исключена).
- `TouchAreaRaycaster`: мышь принимается в редакторе (`Application.isEditor`), в билдах игнорируется (десктопное прицеливание мышью не сломано).
- `JoystickTouchArea`: + `IPointerEnterHandler` — для мыши `pointerEnter = null` → `IsPointerOverGameObject(-1)` false над зонами → прицел мышью в свободных областях работает; над кнопками/сумкой — pointerEnter на их графике, guard блокирует выстрел как раньше.
- Цепочка: TouchArea → форвардер (PointerDown/Drag/Up, тот же PointerEventData/fingerId) → UltimateJoystick (визуал + расчёт Handle). Захват pointerPress модулем — палец может уйти за пределы зоны, управление продолжается.
- Разделение зон (0–50/50–100 ширины, высота по `Touch Area Height (%)`) — не менялось.

### Проверки
- PlayMode (`touch_results3.xml`): **5/5 Passed**: пробы 6 позиций (угол/центр/верхняя граница обеих зон) → правильные зоны; мышь в редакторе попадает в зону; графика джойстиков raycastTarget=false; форвардинг активирует оба джойстика; drag первого пальца за пределы зоны → `_pointerId` правого остаётся 2 (независимость), ось левого продолжает отрабатываться; хотбар 7×0.7 — регрессия.
- EditMode (`editmode_final6.xml`): 6/6 Passed, 0 Failed. Лог прогона: ошибок нет.
- Непроверено: живое сенсорное устройство (событийная имитация); ручная визуальная приёмка.

### Коммиты сессии 6
1. `Fix joystick activation through touch areas` — переключение источника активации.
2. (следующий) docs: сессия 6.

---
# Сессия 5 (2026-11-22): Handle в центре Base и настраиваемые TouchArea

### Исходное состояние
- Ветка `refactor`, HEAD = `c48f2e2`, дерево чистое. В истории обнаружен пользовательский коммит `ec5a1ea scale` (12:20, MainScene.unity) — localScale 0.7 на оба экземпляра джойстиков; не тронут, включён в расчёты (тесты последнего прогона выполнялись поверх него).

### Диагностика (измерение, не догадка)
- В smoke-тест временно добавлен вывод прямоугольников: touch (160,192); folder pos (81.52,113.52), size 156.96, pivot (0,0), **lossyScale 0.70**; handle (160,192); base (136.46,168.46).
- Вычисление: смещение Handle от центра Base = (23.54, 23.54) = (1 − 0.7) × локальный полуразмер (78.48) — точное совпадение. Причина: dynamicPositioning перемешивает локальные и мировые единицы (`folder.position = touch − textureCenter`, textureCenter — ЛОКАЛЬНЫЙ полуразмер) при отскейленном экземпляре.

### Исправления
1. `UltimateJoystick.GetFolderScale()` + компенсация масштаба в dynamicPositioning (`touch − textureCenter × scale`) и в `joystickCenter` (полразмер × scale) — центр Base точно в точке касания при любом масштабе/разрешении; Handle явно центрируется в OnPointerDown.
2. `JoystickTouchArea`: `[SerializeField, Range(10,100)] touchAreaHeightPercent = 50` — «Touch Area Height (%)» в Inspector каждой зоны; пересчёт якорей в Awake и каждый Update (меняется в Inspector — размеры обновляются сразу); зоны = половина ширины + нижние N% высоты; горизонтальные границы (0–50 / 50–100) без зазора задаются при создании.
3. Тест: параметры (50 по умолчанию; 30 → anchorMax.y 0.3; 70 → 0.7), Handle == центр Base (обоих джойстиков), Base == точка касания, drag вправо → ось > 0.1 и Handle внутри радиуса, нейтраль после отпускания.

### Проверки
- PlayMode (`touch_results2.xml` финальный): **5/5 Passed**.
- EditMode (`editmode_final5.xml`): **6/6 Passed**.
- Лог: ошибок нет.
- Непроверено: живой multitouch-стек устройства; визуальная оценка «красоты» в ручном запуске (позиции проверены численно с допуском 4px); заметка про выступ Handle за обод при полном отклонении на 0.7-масштабных джойстиках (осевые значения не изменены — вне правил задачи менять чувствительность).

### Коммиты сессии 5
1. `Fix joystick handle alignment and touch area layout` — ассет (scale-aware dynamicPositioning/joystickCenter, центровка Handle), JoystickTouchArea (параметр высоты), обновление smoke-теста.
2. (следующий) docs: сессия 5.

---
# Сессия 4 (2026-11-22): исправление мобильного управления и TouchArea

### Исходное состояние
- Ветка `refactor`, HEAD = `a281293`, рабочее дерево чистое, посторонних изменений нет.

### ISSUE-7 — самопроизвольное движение влево (исследовано и исправлено)
- Прочитаны: `UltimateJoystick.cs` (ось = `(handle.position − joystickCenter)/radius`, без проверки удержания; ResetJoystick/UpdatePositioning; OnPointerDown/Up c dynamicPositioning), `UltimateJoystickScreenSizeUpdater.cs` (OnRectTransformDimensionsChange → в конце кадра UpdatePositioning для всех джойстиков), `KeyboardJoystick.cs` (порог движения `joystickH < −0.2` без проверки isDown), сцена/префаб (JoystickSizeFolder pivot (0,0), Base/Handle — stretch-дети).
- Установлено: `UpdatePositioning` = ResetJoystick (по СТАРОМУ центру) → UpdateSizeAndPlacement (новый центр); хэндл оставался в старой точке → постоянная отрицательная ось (влево) без касаний. Срабатывает при старте (изменение размеров канваса при первом кадре) и любом ресайзе.
- Исправлено в ассете (минимально): центр хэндла в конце UpdateSizeAndPlacement; размещение сразу в Awake; ResetJoystick при потере фокуса/паузе.
- Коммит: см. ниже.

### ISSUE-8 — Base со смещением от точки касания
- Подтверждено тестом: до фикса рассинхрон хэндла/центра после перестроек канваса давал и смещение визуала при касании (апдейтер перезаписывал папку/центр в конце кадра). После централизации размещения (ISSUE-7-фикс) состояние согласовано: тест проверяет, что центр визуальной папки == точка dispatch pointer down (допуск 4px, overlay canvas: world == screen) — Passed на реальном разрешении Unity (Screen.width/height, не константами).

### ISSUE-9 — TouchArea = половины экрана, ниже интерфейса
- Реализовано: программный `TouchAreaCanvas` (sortingOrder −20), `TouchArea_Left` (0–50%) и `TouchArea_Right` (50–100%) на всю высоту; вместо GraphicRaycaster — собственный `TouchAreaRaycaster` (игнорирует мышь pointerId<0, возвращает зону только для пальца в её прямоугольнике); `JoystickTouchArea` пересылает PointerEventData (с fingerId) в целевой джойстик; `JoystickTouchVisibility` упрощён (только видимость); старые 160×160 зоны удалены. `KeyboardJoystick` не менялся — десктопное прицеливание мышью сохранено (мышь в зоны не попадает).
- Маршрутизация: сортировка raycast-результатов EventSystem идёт по sortingOrder канвасов → сумка/ячейки/кнопки (−10) всегда важнее зон (−20), джойстик-визуал (0) рисуется поверх.

### Проверки
- PlayMode (`touch_results.xml`): **5/5 Passed** — в smoke добавлены: нейтраль 4 осей до касаний и после отпусканий; Base в точке касания для обоих джойстиков; геометрия половин; мышь игнорируется raycaster'ом; пальцы попадают в правильные половины; форвардинг TouchArea→джойстик (удержание через PointerDown в зоне); multi-touch-независимость; порядок канвасов. Хотбар 7×0.7 — Passed (регрессия).
- EditMode (`editmode_final4.xml`): **6/6 Passed**.
- Лог прогона: ошибок нет.
- Непроверено (честно): живой сенсорный экран с двумя пальцами (событийная имитация uGUI покрывает механику ассета, но не физический стек тач-ввода); потеря фокуса/пауза устройства в ручном прогоне (OnApplicationFocus/Pause добавлены, автоматического теста нет); «быстрые повторные касания» покрыты косвенно (pointerId-сброс в OnPointerUp/ResetJoystick).

### Коммиты сессии 4
1. `Fix mobile joystick input and touch areas` — ассет-фиксы (центр хэндла, Awake-плейсмент, фокус/пауза), TouchAreaRaycaster/JoystickTouchArea/TouchAreaCanvas, упрощение JoystickTouchVisibility, обновление smoke-теста.
2. (следующий) docs: сессия 4.

---
# Сессия 3 (2026-11-22): мобильные джойстики и хотбар 7×0.7

### Исходное состояние
- Ветка `refactor`, HEAD = `b0c84e8`, рабочее дерево чистое, посторонних незакоммиченных изменений нет.

### Этап 1 — Исследование (выполнено, детали в TECHNICAL_NOTES)
- Джойстики: два экземпляра Ultimate Joystick в «Ultimate UI Canvas» (Joystick1 — движение, dynamicPositioning=1; Joystick2 — прицел, dynamicPositioning был 0). Обработчики указателя — на корневом GO; весь визуал — в дочернем `JoystickSizeFolder`; multitouch реализован в ассете (`_pointerId`, `joystickState`); hit-area совпадала с видимыми графами.
- Канвасы: GameUi (runtime, sortingOrder 0) и Ultimate UI Canvas (sortingOrder 0) — порядок не гарантирован.
- Хотбар: `GameUi.Build` — 9 ячеек 66×66, `Bag.Items` — 9, выбор слота — `PlayerComtext.SelectItem`, использование — `UseItem(Number)`.

### Этап 2 — Джойстики (коммит `260a74f`)
- sortingOrder −10 у канваса GameUi → джойстики поверх сумки.
- Новый `Assets/src/gui/JoystickTouchVisibility.cs`: скрытие/появление визуала по `GetJoystickState`, невидимая зона касания 160×160, отключение зоны при открытой сумке (raycastTarget=false — новые касания не активируют; удерживаемый палец продолжает работать из-за захвата pointerPress).
- `GameUi.Start` подключает компонент обоим джойстикам (`GetUltimateJoystick`).
- Сцена: Joystick2 `dynamicPositioning: 0 → 1` (появление под пальцем, как у левого).
- Тесты: smoke-тест дополнен — скрытые по умолчанию, появление при pointer down (pointerId 1 и 2 независимо — multi-touch), скрытие после pointer up, порядок канвасов, raycastTarget=false при открытой сумке.

### Этап 3 — Хотбар (коммит `f9b26e6`)
- `GameUi.Build`: 7 ячеек, scale 0.7 (ячейка 46.2, промежутки 4.2, панель 365.4×54.6 из прежней формулы ×0.7, центр низа сохранён); иконки/количество масштабируются (BestFit 7–14, Outline 1.05).
- Модель не тронута: `Bag.Items` = 9, сумка 36; видны первые 7; клавиши 8/9 и выбор слотов работают на уровне модели.
- Новый тест `HotbarSevenSlotsTests`.

### Этап 4 — Проверка
- PlayMode (`mobile_results.xml`): **5/5 Passed** — smoke запуска MainScene (включая джойстик-сценарий multi-touch: два пальца pointerId 1/2, независимое появление/скрытие, порядок канвасов, подавление зон при открытой сумке), кнопки 3/3, хотбар 7×0.7.
- EditMode (`editmode_final3.xml`): **6/6 Passed**.
- Лог прогона: ошибок нет.
- Непроверенное (честно): реальный тач-экран/эмулятор с двумя пальцами не использовался — multi-touch проверен событиями uGUI с разными pointerId через ExecuteEvents (механика ассета), но не «живыми» пальцами; портретная ориентация не проверялась — при узких экранах зоны джойстиков (160×160 в нижних углах) могут перекрывать края хотбара/сумки (в ландшафте 4:3 и шире перекрытий нет: крайние ячейки начинаются с x≥161px, зоны ≤160px); скриншот в batch не записывается.
- Попутно исправлен процессный дефект: у `HotbarSevenSlotsTests.cs.meta` был 31-символьный GUID — Unity игнорировал файл; исправлен на корректный 32-символьный (все остальные .cs.meta проверены скриптом).

### Коммиты сессии 3
1. `260a74f` Improve mobile joystick visibility (+ smoke-тест).
2. `f9b26e6` Resize hotbar to seven slots (+ HotbarSevenSlotsTests).
3. (следующий) docs: сессия 3.

---
# Сессия 2 (2026-11-22): оформление количества предметов + исправление NRE при запуске

### Исходное состояние
- Ветка `refactor`, рабочее дерево чистое, HEAD = `6d87ee7` (docs из сессии 1). Посторонних незакоммиченных изменений нет.

### ISSUE-3: NullReferenceException при запуске (UnityContext<T>.OnAdded) — исследовано и исправлено
Полный разбор — в `GUI_ISSUES.md`, технические детали — в `TECHNICAL_NOTES.md` (раздел «IoC-инициализация»). Кратко:
- `GuiPool.Awake` (дочерний `guiPOOL`) → `GOPool.Awake:137` → `this.Inject()` → `SendMessageUpwards("OnAdded")` → `UnityContext<T>.OnAdded` на корневом `GameContext`.
- NRE на `UnityContext.cs:36` потому, что `_applicationRoot` ещё null: `Awake` корня контейнера не успел выполниться (порядок Awake между объектами сцены не задан; Script Execution Order пуст).
- Защита `DesignByContract.Check.Require` вырезана атрибутами `[Conditional]` — `scriptingDefineSymbols: {}`, поэтому падение «голый NRE».
- Наследование `GuiPool : GOPool` и повторный вызов Awake не при чём: вызов один и корректный.

Изменения:
- `Assets/Generator/core/GameContext.cs` (вне GUI, объяснение: точка создания IoC-контейнера, без неё любой Awake дочернего объекта может упасть): `[DefaultExecutionOrder(-1000)]`.
- `Assets/external/IoC/IoC/UnityContext.cs`: `EnsureApplicationRoot()` — ленивая инициализация в `OnAdded` (инъекция не пропускается; контейнер не пересоздаётся — нет двойных синглтонов).
- Коммит `e1230ef` «Fix IoC initialization NullReferenceException».

### ISSUE-4: количество предметов — Comic + чёрный Outline — исправлено
- Подтверждено: счётчик — обычный Unity UI `Text` (TextMeshPro не используется); ресурс `Resources/gui/comic.ttf` существует (импорт TrueTypeFontImporter → `Resources.Load<Font>("gui/comic")`).
- `GameUi.cs`: новый хелпер `AddCountText` — Comic, BestFit 10–20 pt (без обрезания многозначных чисел), прежняя позиция (правый нижний угол, offset −4,4), `Outline` чёрный непрозрачный `(1.5, −1.5)`. Подключено в `MakeSlot` → одинаково в сумке и хотбаре. Остальные надписи не менялись.
- Коммит `d2da9bb` «Style inventory item counts with Comic font and outline».

### Проверки (подробности в TEST_REPORT)
- PlayMode smoke-тест полного запуска `MainScene` (`Assets/Tests/GameStartSmokeTests.cs`): загрузка сцены по пути через `EditorSceneManager.LoadSceneAsyncInPlayMode` (без правки build settings); утверждения: RootContext/Player/Bag созданы, GameUi.Instance существует, `GuiPool.guiPool` инициализирован, EventSystem живой, шрифт Comic и чёрный Outline на Count-текстах хотбара и сумки, сумка открывается, тестовые количества 1 и 128 подставлены. **Passed.**
- PlayMode-кнопочные тесты 3/3 **Passed** (переведены на использование существующего EventSystem сцены — нужен для совместного прогона со smoke-тестом).
- EditMode 6/6 **Passed** (атлас + DeselectOnClick).
- Лог smoke-прогона: **ни одного** NullReferenceException/Exception/[Error] за всю сессию игры (единственный замеченный warning «referenced script missing» — предсуществующий, в префабе кэша guiPOOL, вне объёма работ).
- В процессе отладки попутно выяснено: `-executeMethod` в batchmode завершает редактор после метода (Play через него недостижим), `SceneManager.LoadScene` по пути требует сцену в build profile (Unity 6), PlayMode-раннер не видит тесты в editor-сборке — поэтому smoke-тест сделан в тестовой asmdef с рефлексией к Assembly-CSharp. Скриншот ScreenCapture в batch не записывается — файл не создан, оформление проверено программными утверждениями.

### Коммиты сессии 2
1. `e1230ef` Fix IoC initialization NullReferenceException (+ smoke-тест запуска).
2. `d2da9bb` Style inventory item counts with Comic font and outline.
3. (следующий) docs: журнал/отчёты сессии 2.

---
# Сессия 1 (2026-11-22): иконки предметов и кнопки — история

## Статус
Завершено. Обе проблемы исправлены, изменения закоммичены (2 логических коммита + коммит документации). Компиляция и автотесты пройдены; ручная Play Mode-проверка «живым» вводом — не выполнена (см. TEST_REPORT, раздел 4).

## Исходное состояние репозитория (проверено перед началом)
- Проект: `D:\projects\TrueMiner`, Unity 6000.0.60f1 (`ProjectSettings/ProjectVersion.txt`).
- Ветка: `refactor`, опережала `origin/refactor` на 23 коммита; `git status` чистый.
- Unity Editor 6000.0.60f1 в `D:\Unity\6000.0.60f1` (batchmode доступен). В начале работы в редакторе оставался открытый экземпляр проекта (PID 18976) — первый batch-прогон был им заблокирован; после его закрытия прогоны выполнялись без конфликтов. Посторонних незакоммиченных изменений, кроме автогенерируемого `TrueMiner.slnx` в конце (см. ниже), не было.

## Этап 1 — Исследование (выполнено)
Изучены: `Assets/src/gui/GameUi.cs`, `BagItemContext.cs`, `PlayerComtext.cs`, `RootContext.cs`, `RootModel.cs`, `BagContext.cs`, `FlagBtn.cs`, `MainMenu.cs`, `GuiPool*.cs`; `Assets/src/weapons/*.cs` (все `GetUiIcon()`); `Assets/Resources/gui/*` (gui.txt, gui.png 256×512, меты, бинарный guiMain.prefab); сцена `MainScene.unity`; `Packages/manifest.json`; `ProjectSettings` (activeInputHandler=0); исходники uGUI из встроенного пакета (EventSystem.cs, ExecuteEvents.cs, Button.cs, Selectable.cs, PointerInputModule.cs). Результаты: `TECHNICAL_NOTES.md`, причины — `GUI_ISSUES.md`.

Ключевые находки:
1. Иконки: `GameUi.SyncSlots` рисовал цветовую заливку (`IconColor()` — заглушка с комментарием «Временная заливка»), спрайт никогда не назначался; атлас `gui.png` нигде не загружался; `gui.txt` — TexturePacker JSON; кадры в `gui.png.meta` не нарезаны (spriteMode: Single); координаты старого UIAtlas в guiMain.prefab не соответствуют текущей раскладке.
2. Кнопки: активный UI — программный Canvas из `GameUi.Build()`; кнопка «Домой» = Flag (`MapGenerator.ChangeMap()`), сумка = Bag. Стандартное поведение uGUI: pointer down делает `Selectable` выбранным объектом EventSystem, затем Submit (пробел) его активирует.

## Этап 2 — Исправление иконок (выполнено)
- Новый `Assets/src/gui/GuiAtlas.cs`: `Resources.Load<Texture2D>("gui/gui")` + `Resources.Load<TextAsset>("gui/gui")`, мини-JSON-парсер (в проекте нет сторонних JSON-библиотек, JsonUtility не умеет динамические ключи «frames»), `Sprite.Create` с пересчётом Y (TexturePacker — сверху, Unity — снизу), кэш, регистронезависимый lookup (с `.png` и без), предупреждение один раз по неизвестному имени.
- `GameUi.cs`: `SyncSlots` → `UpdateIcon()`: назначение `Image.sprite` из GuiAtlas по `ctx.Icon`. Пустые ячейки: sprite=null и Color.clear. Иконка/цвет меняются только при фактической смене значения (нет Rebuild каждый кадр). Метод `IconColor` удалён (заменён реальными спрайтами — доказано, что заливка была заглушкой).
- Проверено: компиляция (exit 0), типы в `Assembly-CSharp.dll`; EditMode-тесты атласа 4/4; визуальная сверка вырезок атласа (`preview/atlas_check_row.png`).
- Коммит `baa2db6`.

## Этап 3 — Исправление кнопок (выполнено)
- Мера 1 (корневая, штатный механизм uGUI): `Navigation.Mode.None` для кнопок Flag и Bag — `Selectable.OnPointerDown` (uGUI 6000.0, строка 1206 Selectable.cs) выбирает кнопку только при navigation.mode != None. С Navigation.None кнопка после клика мышью/тачем не оказывается в Selected и не может получить Submit.
- Мера 2 (страховка): новый `Assets/src/gui/DeselectOnClick.cs` (IPointerClickHandler) — после Pointer Click сбрасывает `SetSelectedGameObject(null)`, если выбрана именно эта кнопка. Ищет EventSystem через `FindFirstObjectByType` (замечено в тестах, что статический `EventSystem.current` не всегда заполнен; в игре это тот же объект). Подключён к Flag/Bag и к кнопкам ячеек (`MakeSlot`) — у ячеек тот же дефект.
- Навигация остальных элементов не менялась: слотам Navigation оставлен Automatic, клавиатурные клавиши B/Tab не тронуты.
- Коммит `bf7c2e7` (аменд `a58fabd`, ветка не пушнута) — включает тесты и авторегистрацию `TrueMiner.slnx` (Unity-генерируемая запись о новой тестовой сборке).

## Этап 4 — Проверка (выполнено; ограничения в TEST_REPORT)
- Компиляция: 2 batch-прогона, exit 0, ошибок нет; предупреждения — только предсуществующие, в `Assets/src/gui` их нет.
- Автотесты: EditMode 6/6, PlayMode 3/3 (итоговые прогоны `editmode_final.xml`, `playmode_results.xml`). Подробности и отбракованные промежуточные прогоны — в TEST_REPORT.md.
- Всплыло в процессе: batch EditMode не заполняет `EventSystem.current`/`m_EventSystems` — учтено в дизайне компонента и в размещении тестов (PlayMode); `Unity.exe -batchmode -quit` блокировался живым редактором (инцидент задокументирован).
- Visual check: вырезки атласа соответствуют именам кадров.

## Этап 5 — Итог
Изменённые файлы:
- `Assets/src/gui/GuiAtlas.cs` (+meta) — новый.
- `Assets/src/gui/DeselectOnClick.cs` (+meta) — новый.
- `Assets/src/gui/GameUi.cs` — SyncSlots/UpdateIcon, MakeSlot/AddButton, удалена IconColor.
- `Assets/Editor/GuiFixTests.cs` (+meta) — новый (EditMode-тесты).
- `Assets/Tests/GuiButtonPlayTests.cs`, `Assets/Tests/TrueMiner.GuiTests.asmdef`, `Assets/Tests.meta` (+меты) — новые (PlayMode-тесты).
- `TrueMiner.slnx` — автозапись регистраций тестовых csproj.
- `GUI_Agent_Work/*` — документация и логи прогонов.

Коммиты (ветка `refactor`):
1. `baa2db6` — иконки предметов.
2. `bf7c2e7` — поведение кнопок (+ тесты).
3. (следующий) `docs` — документация `GUI_Agent_Work`.

## Осталось / не проверено
- Ручной «живой» клик мышью/тачем и пробел в реальном Play Mode (автотесты покрывают механику, но не реальный курсор) — оставлено разработчику, см. TEST_REPORT §4.
- Тач отдельно не тестировался (pointer-события общие).
- Старые NGUI-остатки (UI Root, MainMenu/btnFlag, FlagBtn.cs) не использовались и не изменялись.

## Неподтверждённые предположения
- (изначально) что «Домой» — это кнопка Flag: подтверждено связкой ChangeMap→GenerateHome и отсутствием других домашних кнопок в активном Canvas.

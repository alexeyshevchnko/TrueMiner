# TECHNICAL_NOTES — устройство GUI проекта TrueMiner

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
- Unity 6000.0.60f1 установлена в `D:\Unity\6000.0.60f1\Editor\Unity.exe`. Проверка: `Unity.exe -batchmode -quit -projectPath D:\projects\TrueMiner -logFile ...` и анализ лога на ошибки компиляции.

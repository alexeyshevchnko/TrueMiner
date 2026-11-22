# GUI_ISSUES — реестр проблем интерфейса

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

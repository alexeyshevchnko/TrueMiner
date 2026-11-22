# TEST_REPORT — результаты проверок

## Сессия 3 (2026-11-22): джойстики + хотбар 7×0.7

### Прогон Play Mode, MainScene целиком (`mobile_results.xml`) — 5/5 Passed
| Тест | Результат | Что добавлено/проверяет |
|---|---|---|
| GameStartSmokeTests.MainScene_InitializesAndGuiShows | Passed | + джойстик-сценарий: канвас GameUi ниже Ultimate UI Canvas (sortingOrder −10 < 0); JoystickTouchVisibility подключён к обоим джойстикам; без касаний визуал скрыт; pointer down (pointerId 1) → левый появился, (pointerId 2) → правый появился — **одновременно и независимо**; pointer up первого → левый скрыт, правый остался; pointer up второго → скрыт; при открытой сумке TouchArea.raycastTarget=false у обоих (новые касания не активируют джойстик); IoC/GUI/иконки Comic+Outline — как раньше |
| HotbarSevenSlotsTests.MainScene_HotbarIsSevenSlotsAtScale07 | Passed | новый: 7 ячеек; cellSize 46.2 (=66×0.7), spacing 4.2, constraintCount 7; панель прижата к низу и отцентрована (anchors/pivot); hotbar-массив UI = 7, bagSlots = 36, `Bag.Items` = 9 (модель не изменена); Count хотбара: BestFit 7–14 (=10–20×0.7), Outline ±1.05 (=±1.5×0.7), поле цифр 36.4 |
| GuiButtonPlayTests (3 теста) | Passed | регрессия кнопок без изменений |

### EditMode-регрессия (`editmode_final3.xml`) — 6/6 Passed
Атлас (4) + DeselectOnClick (2) — без изменений.

### Лог прогона
`unity_mobile_tests.log`: NullReferenceException/Exception/[Error] — не обнаружено.

### Проверено / не проверено
- Проверено автотестами: скрытие/появление джойстиков по касанию, multi-touch-независимость (события uGUI с разными pointerId), джойстики поверх сумки, подавление зон при открытой сумке, 7 ячеек × 0.7, модель 9/36, центрирование панели.
- Не проверено (честно): реальное сенсорное устройство/эмулятор с двумя живыми пальцами (только событийная имитация через ExecuteEvents — механику ассета это покрывает, физический multitouch-стек устройства нет); портретная ориентация (в узких экранах угловые зоны 160×160 могут перекрывать края хотбара/сумки; в ландшафте 4:3+ перекрытий нет — ячейки начинаются с x≥161px); скриншот в batch не пишется.
- Выбор/переключение предметов: горячие клавиши 1–9 (`KeyboardJoystick`) и клик по ячейке (`BagItemContext.Click`) не изменялись; клик по первым 7 ячейкам хотбара проверен косвенно (кнопки ячеек не менялись, тест SlotButton_ClickIsNotRepeatedBySubmit).

### Коммиты
1. `260a74f` Improve mobile joystick visibility (+ smoke-тест).
2. `f9b26e6` Resize hotbar to seven slots (+ HotbarSevenSlotsTests).

---
## Сессия 2 (2026-11-22): NRE запуска + Comic/Outline

### Прогон запуска (Play Mode, batch, MainScene целиком)
- Тест `GameStartSmokeTests.MainScene_InitializesAndGuiShows` (`smoke_results.xml`): **Passed**.
  Загружается реальная `Assets/MainScene.unity` (`EditorSceneManager.LoadSceneAsyncInPlayMode`), 3 c на инициализацию:
  - `RootContext.Instance`, `Player`, `Bag` созданы — IoC-цепочка живая;
  - `GameUi.Instance` создан; `GuiPool.guiPool` инициализирован (Inject прошёл без NRE);
  - EventSystem активен; сумка открывается; тестовые количества 1 и 128 подставлены в модель.
  - Шрифт Count-текстов содержит "comic" (проверено для хотбара[0] и сумки[0]); Outline чёрный, непрозрачный.
- **Лог прогона `unity_smoke_run.log`: ни одного NullReferenceException / Exception / [Error]** за всю сессию игры. `UnityContext Awaked` в логе подтверждает порядок: корень контекста → инъекции пулов.
- Замеченный в логе warning «The referenced script on this Behaviour (Game Object '<null>') is missing!» при `InstantiateGameObject` кэша guiPOOL — **предсуществующий** (скрипт отсутствует в одном из префабов кэша), не является исключением, вне объёма работ; занесён как наблюдение.
- EditMode-регрессия (`editmode_final2.xml`): **6/6 Passed** (атлас 4 + DeselectOnClick 2).
- PlayMode-кнопочные тесты: **3/3 Passed** (переведены на использование существующего EventSystem сцены).

### Ограничения проверки (честно)
- Скриншот `ScreenCapture.CaptureScreenshot` в batch-среде не записывается — файл `playmode_smoke.png` не создан; оформление проверено программными утверждениями (имя шрифта, компонент Outline и его цвет) + предыдущей визуальной сверкой атласа. Человеческая оценка «красиво/не читается» остаётся за ручным запуском.
- Сенсорный ввод отдельно не эмулировался (pointer-события общие).
- Попутные технические находки среды: `-executeMethod` в batch сам завершает редактор (Play недостижим через него); `SceneManager.LoadScene` по пути требует сцену в build profile (использован `LoadSceneAsyncInPlayMode`); PlayMode-раннер не видит тесты в editor-сборке (smoke-тест сделан в asmdef с рефлексией к Assembly-CSharp).

### Коммиты
1. `e1230ef` Fix IoC initialization NullReferenceException (+ smoke-тест запуска).
2. `d2da9bb` Style inventory item counts with Comic font and outline.

---
## Сессия 1 (2026-11-22): иконки + кнопки

Дата: 2026-11-22. Unity 6000.0.60f1, проверка в `-batchmode` (редактор закрался/не запущен во время прогонов; проект компилировался и тестировался headless).

## 1. Компиляция
- Прогон 1 (после исправления иконок): `Unity.exe -batchmode -quit -nographics -projectPath ... -logFile unity_compile_log_1.log` → exit 0, ошибок CS нет, `CompileScripts: 3458ms`, `Assembly-CSharp.dll` пересобран; типы `GuiAtlas`/`MiniJson` присутствуют в dll (проверено бинарным поиском).
- Прогон 2 (после исправления кнопок): `unity_compile_log_2.log` → exit 0, ошибок CS нет. В логе — только предупреждения **предсуществующего** кода (CS0618/CS0108/CS0162 и т.п. в `Assets/Generator`, `Assets/external`, `Assets/src/weapons`); в `Assets/src/gui` предупреждений и ошибок нет.
- Первый batch-прогон (unity_compile_log_0 attempt) был отвергнут Unity: «another Unity instance is running» — в тот момент редактор был ещё жив; повторён после его закрытия.

## 2. Автотесты (новые, вошли в коммиты)
Файлы:
- `Assets/Editor/GuiFixTests.cs` — EditMode: атлас и логика DeselectOnClick.
- `Assets/Tests/GuiButtonPlayTests.cs` (+ `TrueMiner.GuiTests.asmdef`) — Play Mode: полный поток кнопок с живым EventSystem.

### EditMode, финальный прогон (`editmode_final.xml`) — 6/6 Passed
| Тест | Результат | Что проверяет |
|---|---|---|
| Atlas_ItemIconNames_ResolveToSprite | Passed | все имена из `GetUiIcon()` предметов (torch, kirka, granade1, bow2, Axe, Item1, ground, green, coal, gold, wood, brick, kost, meat, med, stal, stone) резолвятся в спрайты атласа |
| Atlas_LookupIsCaseInsensitive_AndAcceptsPngExtension | Passed | lookup не зависит от регистра и принимает имя с `.png`; null/пустая строка → null |
| Atlas_RectMatchesGuiTxt_WithYFlipped | Passed | прямоугольники kirka и cell точно совпадают с gui.txt с пересчётом Y (сверху→снизу) |
| Atlas_UnknownIconReturnsNull | Passed | неизвестное имя → null (без падения) |
| DeselectOnClick_PointerClickClearsSelection | Passed | после Pointer Click выбранной остаётся null |
| DeselectOnClick_LeavesOtherSelectionUntouched | Passed | клик по кнопке не трогает выбор другого элемента |

### Play Mode, финальный прогон (`playmode_results.xml`) — 3/3 Passed
| Тест | Результат | Что проверяет |
|---|---|---|
| DeselectOnClick_ClearsSelectionAfterPointerClick | Passed | pointer down выбирает кнопку (стандарт uGUI), click выполняет действие, после клика selected = null; пробел (Submit) не активирует кнопку, т.к. у модуля ввода нет цели; **штатный** Submit при явном клавиатурном выборе работает (контроль) |
| NavigationNone_ButtonIsNeverSelectedByPointerDown | Passed | с Navigation.None pointer down вообще не выбирает кнопку |
| SlotButton_ClickIsNotRepeatedBySubmit | Passed | клик по ячейке работает и не повторяется пробелом |

### Промежуточные (отбракованные) прогоны — оставлены в истории для честности
- `editmode_results.xml`: 4 атлас-теста Passed; 2 кнопочных Failed → причина: в batch EditMode статический `EventSystem.current` не заполняется окружением тестового раннера (диагностировано рефлексией по `m_EventSystems` в `editmode_results3/4.xml`: goActive/isActiveAndEnabled=True, но статический список пуст). Код обработчика переписан на `FindFirstObjectByType<EventSystem>()` (в игре это тот же единственный EventSystem), кнопочные тесты перенесены в Play Mode.
- неиспользуемый теперь `playmode_results` промежуточный: Submit-проверка доставляла событие напрямую кнопке (так модуль ввода не делает); исправлено на реальную маршрутизацию — Submit шлётся только `currentSelectedGameObject`.

## 3. Визуальная проверка иконок
Кадры `kirka, torch, Axe, bow2, stone, cell, wood, granade1` вырезаны из `gui.png` по координатам `gui.txt` и увеличены (`GUI_Agent_Work/preview/atlas_check_row.png`): изображения соответствуют именам (кирка, факел, топор, лук, камень, рамка ячейки, дрова, граната). Т.е. имя→картинка в атласе корректны, а не только непусты.

## 4. Что НЕ проверено (честные ограничения)
- **Ручная игра (мышь/тач/пробел) в Play Mode с реальным вводом не выполнялась.** Автотесты имитируют поток StandaloneInputModule на уровне событий uGUI (down → click → submit-маршрутизация), что соответствует механике модуля, но клик «настоящим курсором» в окне игры не совершался — нужна финальная ручная проверка разработчиком.
- Сенсорный ввод (Touch) не проверялся отдельно — тесты используют pointer-события, Common для мыши и тача.
- Производительность (GC/скорость поиска спрайтов) не замерялась; спрайты создаются один раз, затем кэш.
- PlayMode-тесты в batch `-nographics` выполнялись в Editor Play Mode, не в standalone-сборке игрока.

## 5. Ожидаемое поведение после исправлений
1) Перезапуск редактора не требуется — Library уже пересобрана прогонами; 2) при первом обращении создаются все спрайты атласа (один раз, затем кэш); 3) неизвестная иконка → один warning и пустая ячейка вместо падения.

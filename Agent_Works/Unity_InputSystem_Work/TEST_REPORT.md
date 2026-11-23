# TEST_REPORT — Миграция на Unity Input System (проверки)

## 1. Компиляция
- `compile_migrated.log`: **0 ошибок, 0 предупреждений** (после всех замен API).
- `compile_final.log`, `compile_verify.log`: **0 ошибок, 0 предупреждений** (контрольные, после удаления зонда и правки настроек).
- Старый API в исходниках (.cs): **0 обращений** (в единственном остатке — текст комментария в `TouchAreaRaycaster.cs:12`, не код). `instantiate.js` — UnityScript, не компилируется современным Unity, вне охвата.

## 2. Предупреждение об Input Manager
- До: `activeInputHandler: 0` + StandaloneInputModule → Console показывает предупреждение «This project uses Input Manager, which is marked for deprecation.»
- После: grep по полному логу открытия и Play-прогона `play_isx.log`/`play_verify.log`: **«Input Manager» — 0 совпадений, «deprecated» — 0 совпадений**. Предупреждение исчезло. ✔

## 3. Play Mode (автоматические прогоны через GUI-зонд, сцена MainScene)
| Прогон | Фазы | Исключения | Предупреждения |
|---|---|---|---|
| `play_isx.log` | Play#1 (25с) → стоп → Play#2 (15с) → стоп → выход | 0 | 0 |
| `play_verify.log` (итоговое состояние файлов) | Play (12с) → стоп → выход | 0 | 0 |

- Модуль Input System: `Input System module state changed to: Initialized` … `Shutdown` — штатные переходы при Play/остановке.
- Игра инициализируется: карта генерируется (156 загрузок спрайтов), игрок создаётся, IoC работает; отсутствует и ошибка, и missing script.

## 4. Проверенный ввод
| Сценарий | Проверка | Результат |
|---|---|---|
| Компиляция нового API (Keyboard/Mouse) | компиляция + Play-прогон без `InvalidOperationException` | ✔ |
| EventSystem UI | InputSystemUIInputModule загрузился в Play без ошибок/Exceptions; дефолтные действия созданы автоматически | ✔ (функционально) |
| Клавиатурное управление персонажем (Space/↑/W, ↓/S, ←/→, WASD) | прямые замены 1:1 (`wasPressedThisFrame`/`isPressed`); в Play-прогоне клавиши не эмулировались | замены верифицированы компиляцией и отсутствием ошибок; живой ввод не прогонялся |
| Хотбар 1–9, сумка B/Tab | то же | ✔ компиляция |
| Атака ЛКМ/LeftCtrl + прицел мыши | то же; `IsPointerOverGameObject()` работает и с InputSystemUIInputModule | ✔ компиляция |
| Два джойстика одновременно | архитектура не менялась: JoystickTouchArea + UltimateJoystick через EventSystem; модуль в режиме MultiTouchAndTrack; pointerId-разделение сохраняется | код не изменён; реальное касание двух пальцев не проверялось (нет сенсорного устройства) |
| Нажатие/удержание/отпускание (залипание) | API-эквиваленты `wasPressedThisFrame/isPressed/wasReleasedThisFrame`; измерений залипания в живом геймплее не проводилось | части проверок не выполнены |
| Touch/Android устройство | не выполнялось (нет устройства) | ⚠ требуется ручная проверка |

## 5. Что не проверено автоматически (см. REMAINING_ISSUES)
- Реальные нажатия клавиш/мыши в Play Mode (живой ввод).
- Сенсорное управление двух пальцев на реальном/эмулируемом устройстве.
- Сборка под Android.
- Демо-сцены CartoonFx (правки компилируются; сцены не запускались).

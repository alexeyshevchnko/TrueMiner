# MIGRATION_NOTES — Миграция TrueMiner со старого Input Manager на Unity Input System

## 1. Установленный пакет
- `com.unity.inputsystem@1.20.1` (последняя версия реестра Unity для ветки 6000.x; установлена через `Packages/manifest.json`).
- **1.14.2 не совместима с 6000.6.10f1**: сборка 1.14.2 содержит вызовы API, помеченные в 6000.6 как ошибки-устаревания (`Object.GetInstanceID()`, `EditorUtility.InstanceIDToObject(int)`, `AssetDatabase.GetAssetPath(int)`) — пакет не собирался (12 ошибок CS0619). Переход на `EntityId` в пакете выполнен в 1.19.0 (changelog), поэтому рабочая версия — 1.19+/1.20.x.

## 2. Настройки проекта
| Настройка | Было | Стало | Зачем |
|---|---|---|---|
| `ProjectSettings/ProjectSettings.asset → activeInputHandler` | `0` (Old Input Manager) | `1` (Input System Package (New)) | устранение Prediction-warning «This project uses Input Manager, which is marked for deprecation.» и единая система ввода; **не Both** |
| `Packages/manifest.json` | без inputsyystem | + `"com.unity.inputsystem": "1.20.1"` | установка пакета |
| `Packages/packages-lock.json` | — | + блок inputsyystem | генерируется Unity |
| `Assets/MainScene.unity (EventSystem)` | `StandaloneInputModule` | `InputSystemUIInputModule` | модуль UI для новой системы; все действия (`m_ActionsAsset` и др.) = null → модуль сам создаёт дефолтные действия; `m_PointerBehavior = SingleMouseOrPenButMultiTouchAndTrack` (дефолт) — два одновременных касания работают |

## 3. Замены старого API (прямые обращения к устройствам, без избыточных Input Actions)
| Старый API | Новый эквивалент |
|---|---|
| `Input.GetKeyDown(KeyCode.X)` | `Keyboard.current?.xKey.wasPressedThisFrame == true` |
| `Input.GetKey(KeyCode.X)` | `Keyboard.current?.xKey.isPressed == true` |
| `Input.GetKeyUp(KeyCode.X)` | `Keyboard.current?.xKey.wasReleasedThisFrame == true` |
| `Input.GetMouseButtonDown(0)` | `Mouse.current?.leftButton.wasPressedThisFrame == true` |
| `Input.GetMouseButton(0)` | `Mouse.current?.leftButton.isPressed == true` |
| `Input.GetMouseButtonDown(1/2)`, `GetMouseButton(1)` | `rightButton`/`middleButton` аналогично |
| `Input.mousePosition` | `Mouse.current?.position.ReadValue() ?? Vector3.zero` |

### Ключи (map)
Space→`spaceKey`, ↑→`upArrowKey`, ↓→`downArrowKey`, ←/→→`leftArrowKey/rightArrowKey`, W/A/S/D→`wKey/aKey/sKey/dKey`, LeftControl→`leftCtrlKey`, 1–9→`digit1Key...digit9Key`, B→`bKey`, Tab→`tabKey`, T/G/R/P→`tKey/gKey/rKey/pKey`, Delete→`deleteKey`.

### Null-безопасность
На мобильных устройствах клавиатуры/мыши нет: `Keyboard.current`/`Mouse.current` = null. Все вызовы — через `?.… == true` (false при отсутствии устройства), позиция мыши — `?? Vector3.zero`. Сенсорные джойстики на EventSystem — не затронуты.

## 4. Изменённые файлы
| Файл | Что заменено |
|---|---|
| `Assets/src/KeyboardJoystick.cs` | прыжок (Space/↑/W down+hold), ↓/S, ←/A, →/D, хотбар 1–9, прицел `mousePosition`, атака ЛКМ down/hold + LeftCtrl down/hold |
| `Assets/Generator/Terrain/TileRenderer.cs` | редактор: T/G/R/P, 0/8 (release), ЛКМ/ПКМ (hold), `mousePosition` |
| `Assets/CameraTransformPos.cs` | WASD/стрелки (hold) — камера |
| `Assets/src/gui/GameUi.cs` | B/Tab (down) — сумка |
| `Assets/src/weapons/Pick.cs`, `Axe.cs` | прицел `mousePosition` |
| `Assets/external/CartoonFx/.../CFX2_Demo.cs`, `CFX3_Demo.cs` | демо-навигация (←/→/Delete), мышь 3 кнопок, `mousePosition` |
| `Assets/MainScene.unity` | StandaloneInputModule → InputSystemUIInputModule (fileID 1247130979 сохранён — порядок компонентов GameObject не изменился) |
| `Packages/manifest.json`, `packages-lock.json`, `ProjectSettings.asset` | см. §2 |

## 5. Что НЕ изменилось (специально)
- **Сенсорная архитектура**: `JoystickTouchArea`, UltimateJoystick, `TouchAreaRaycaster` работают через EventSystem (IPointerDown/Drag/Up с PointerEventData) — миграция не потребовала изменений; pointerId-модель InputSystemUIInputModule обеспечивает независимость двух джойстиков (для мыши pointerId по-прежнему < 0 — проверка в `JoystickTouchArea.OnPointerEnter` сохраняет смысл).
- `UltimateJoystick.GetHorizontalAxis/GetVerticalAxis/GetIsDown` — собственный API ассета, не часть старого Input Manager.
- `instantiate.js` (UnityScript 2017) — современным Unity не компилируется вообще, файл не активен; не тронут.
- Комментарий-документация в `TouchAreaRaycaster.cs` упоминает `Input.GetMouseButton` — это текст, не код.
- Визуал джойстиков/зоны/хотбар — без изменений.

## 6. Инциденты в процессе (устранены)
1. Конвейер правок через `Get-Content -Raw` (без явной кодировки) исказил UTF-8 кириллицу в 8 файлах (идентификатор `СircleDamageTile` сломался) — файлы восстановлены из Git-HEAD, правки повторены через `[System.IO.File]::ReadAllText/WriteAllText` с явным UTF-8; контроль: 0 mojibake, компиляция чистая.
2. Лишний байт `i` в конце hex-строки `m_StackTraceTypes` в `ProjectSettings.asset` — удалён; итоговый diff файла: только `activeInputHandler: 0→1` (+ ранее существовавшие пользовательские изменения serializedVersion).

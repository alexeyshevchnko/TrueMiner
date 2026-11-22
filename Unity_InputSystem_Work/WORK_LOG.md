# WORK_LOG — Миграция TrueMiner со старого Input Manager на Unity Input System

> Отдельная задача. Папки `GUI_Agent_Work`, `Unity_Migration_Work`, `Unity_Warnings_Work` не используются.

---

## Запись 1 — 2026-11-22 — Диагностика

### Состояние проекта
- Unity `6000.6.10f1`; ветка `refactor`; HEAD `46b9a38 «убрал варнинги»` (пользователь закоммитил свои meta-изменения и результаты прошлых задач).
- Незакоммиченные изменения пользователя: `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/PackageManagerSettings.asset`, `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/ProjectVersion.txt` — это состояние обновления пакетов при переходе на 6000.6 (ugui 2.6.0, timeline 6.6.0 и модули). **Сохраняются; миграция добавит свои строки поверх.**
- Пакет `com.unity.inputsystem` отсутствует; `activeInputHandler: 0` (старый Input Manager) — источник предупреждения «This project uses Input Manager, which is marked for deprecation».

### Архитектура ввода проекта (по коду)
1. **Клавиатура/мышь** (десктоп): `Assets/src/KeyboardJoystick.cs` — движение/прыжок (Space/↑/W; ↓/S; ←/A; →/D), хотбар (1–9), прицел мыши, атака ЛКМ/LeftCtrl, проверка `IsPointerOverGameObject()`; `GameUi.cs` — сумка (B/Tab); `CameraTransformPos.cs` — камера (WASD/стрелки); `TileRenderer.cs` — редактор тайлов (T/G/R/P, 0/8, ЛКМ/ПКМ); `Pick.cs`, `Axe.cs` — прицел `Input.mousePosition`.
2. **Мобилъный ввод**: UltimateJoystick + зоны `JoystickTouchArea` (половины экрана) — **полностью через EventSystem** (IPointerDown/IDrag/IPointerUp, пересылка PointerEventData с fingerId); multi-touch обеспечивает UltimateJoystick по `_pointerId`. Старого `Input.GetTouch` в проекте **нет**.
3. **EventSystem**: `MainScene.unity` — StandaloneInputModule + EventSystem; `MainSceneLight.unity` — только EventSystem (без модуля).
4. **Сторонние**: `CFX2_Demo.cs`/`CFX3_Demo.cs` (демо-скрипты, не запускаются игрой); `instantiate.js` (UnityScript 2017, современным Unity вообще не компилируется — не трогаем).

### План
1. Установить `com.unity.inputsystem` (совместимую с 6000.6), проверить версию.
2. `activeInputHandler: 1` (Input System Package (New), не Both).
3. Замены старого API прямыми обращениями к устройствам (`Keyboard.current.*`, `Mouse.current.*`) — без избыточных Input Actions (инструкция про простую миграцию).
4. EventSystem: StandaloneInputModule → InputSystemUIInputModule (MainScene).
5. Batch-компиляция + 2-цикловый Play-прогон + проверка отсутствия предупреждения об Input Manager.

### Текущий статус
В работе: установка пакета.

---

## Запись 2 — 2026-11-22 — Установка пакета и миграция кода

1. `com.unity.inputsystem@1.14.2` → **не собрался** (12 × CS0619: пакет использует API, запрещённые в 6000.6 — `GetInstanceID`, `InstanceIDToObject(int)` и др.). По реестру Unity (packages.unity.com) миграция на `EntityId` сделана в 1.19.0 → выбрана **1.20.1** — установилась и скомпилировалась без ошибок.
2. Замены старого API выполнены во 8 файлах (прямые `Keyboard.current`/`Mouse.current`, null-безопасно для мобильных). Инциденты:
   - правка через `Get-Content -Raw` исказила UTF-8 кириллицу (сломался идентификатор `СircleDamageTile` в TileRenderer) → файлы восстановлены из Git-HEAD, правки повторены через `[System.IO.File]::ReadAllText/WriteAllText` (явный UTF-8);
   - искажение из-за ошибки в списке пар замен (flatten) → повторный checkout файла и аккуратная правка;
   - лишний байт `i` в `m_StackTraceTypes` ProjectSettings.asset → удалён (теперь diff файла = только `activeInputHandler: 0→1`).
3. `MainScene.unity`: StandaloneInputModule → InputSystemUIInputModule (actions = null, автосоздание дефолтных; MultiTouchAndTrack).
4. `ProjectSettings.asset`: `activeInputHandler: 1`.

## Запись 3 — 2026-11-22 — Верификация

- `compile_migrated.log`, `compile_final.log`, `compile_verify.log`: **0 ошибок / 0 предупреждений**.
- В исходниках (.cs) старый `Input.*` — 0 (только упоминание в комментарии TouchAreaRaycaster.cs:12).
- GUI-прогон `play_isx.log` (2 цикла Play): предупреждение «Input Manager» — **0**; «deprecated» — 0; исключения — 0 во всех фазах; Input System Initialized/Shutdown штатно; игра инициализируется (карта, игрок, IoC).
- GUI-прогон `play_verify.log` на итоговом состоянии файлов: Play вошёл/вышел, 0 исключений/предупреждений.
- Зонд удалён; контрольная компиляция чистая.

### Git-коммит
- `Migrate project to Unity Input System` — 8 .cs + MainScene.unity + manifest.json + packages-lock.json + ProjectSettings.asset. Посторонние файлы (PackageManagerSettings, ProjectVersion, новые ProjectSettings/*.asset, все meta-изменения пользователя) в коммит не включены.

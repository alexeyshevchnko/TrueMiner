# WORK_LOG — Устранение предупреждений Unity (после миграции на 6000.6.10f1)

> Отдельная задача. Папки `GUI_Agent_Work` и `Unity_Migration_Work` не используются.

---

## Запись 1 — 2026-11-22 ~15:15 — Первичная диагностика

### Git
- Ветка `refactor`; незакоммиченных изменений пользователя — 791 (meta-файлы спрайтов, Packages/*.json, ProjectSettings, TrueMiner.slnx) — **не трогаем**.
- HEAD: `3413bdd docs: миграция на Unity 6000.6.10f1 (Unity_Migration_Work)`.

### Версия Unity
- `6000.6.10f1 (40e81493c67d)`, редактор `D:\Unity\6000.6.10f1`.

### Источники списка предупреждений
1. **Предупреждения компиляции** (появляются в Console после открытия/компиляции): полный уникальный набор взят из `Unity_Migration_Work/compile_batch_2.log` — **91 уникальное** (в полный набор входят редакторские сборки; первая Assembly-CSharp-волна ≈42 совпадает с числом пользователя при открытии).
2. **Рантайм-предупреждения Play Mode** (по счётчику пользователя ≈+110 повторов): содержимое неизвестно — будет получено собственным batch-запуском Play Mode (временный probe-скрипт, добавляется в Assets только на время прогона, затем удаляется; фиксируется в логе).
3. Лог GUI-сессии пользователя 15:13 (`Logs/Editor.log`) не содержит ни одного warning-стpоки — предупреждения в его Console были кэшем последней компиляции; Play Mode в этой сессии, судя по логу, не запускался.

### План
1. `WARNINGS_INVENTORY.md` — инвентарь 91 предупреждения с классификацией.
2. Исправление собственного кода (Assets/Editor, Assets/Generator, Assets/src).
3. Изучение и точечное исправление сторонних ассетов (CartoonFx, Ultimate Joystick, Tiled Sprite, TexturePacker) — только механические замены устаревших API с сохранением поведения.
4. Batch-компиляция до 0 предупреждений.
5. Batch Play Mode probe → фиксация рантайм-предупреждений → исправление → повторные прогоны (проверки A–D).
6. Документация + коммиты.

### Текущий статус
В работе: инвентаризация и чтение исходников.

---

## Запись 2 — 2026-11-22 — Исправления предупреждений компилятора

### Выполнено
1. Инвентарь 91 предупреждения создан (`WARNINGS_INVENTORY.md`), исходный лог сохранён (`evidence_compile_before.log`, `evidence_warnings_initial.txt`).
2. Исправлен весь собственный код (17 файлов) и сторонние ассеты (CartoonFx, Ultimate Joystick, Tiled Sprite, TexturePacker) — см. WARNINGS_INVENTORY (все ✔). Подавления предупреждений не применялись.
3. Значимые находки 6000.6 (усложнили простые замены):
   - `MinMaxCurve.multiplier` не существует → `curveMultiplier` (CS1061 в итерации 1);
   - `FindObjectsByType(Type, FindObjectsSortMode)` тоже устарел → перегрузка без сортировки;
   - `PlayModeStateChange.ExitedPlayMode` отсутствует → `EnteredEditMode`;
   - `TextureImporter.spritesheet` переписан на официальный `ISpriteEditorDataProvider` с переносом `spriteID` (стабильные ссылки на спрайты Items/TerrariaTiles).

### Проверка компиляции
| Прогон | Результат |
|---|---|
| `compile_1.log` | 2 warning (новый API-эффект: `FindObjectsByType(...SortMode)`), 2 error (`multiplier`) |
| `compile_2.log` | 0 warning, 1 error (`ExitedPlayMode`) |
| `compile_3.log` | **0 warning, 0 error** — чистая компиляция (ExitCode 0) |

### Текущий статус
Batch Play Mode probe запущен (временный `Assets/Editor/WarningsProbe/TempBatchPlayRunner.cs`, будет удалён; сцена MainScene; 25 с в Play + 8 с после выхода).

---

## Запись 3 — 2026-11-22 — Рантайм-исследование (Play Mode)

### Как проводилось
- Batch-режим не поддерживает Play Mode: `EditorApplication.isPlaying = true` из `-executeMethod` падает с `NotImplementedException` (probe 1; зависший процесс останавливался, удалён устаревший `Temp/UnityLockfile`).
- Использован **GUI-сеанс с автозапуском**: `[InitializeOnLoad]`-зонд сам открывает `Assets/MainScene.unity`, входит в Play, выходит и закрывает редактор; все сообщения Console попадают в лог (`play_probe_2.log`).
- Аудит потерянных скриптов выполнен отдельным зондом `MissingScriptsProbe.Scan` (все `.prefab`/`.unity`, включая бинарные, через `PrefabUtility.LoadPrefabContents`).

### Результат probe 2 (25 с Play)
- **Единственные рантайм-предупреждения — «The referenced script on this Behaviour is missing!»** при инстанцировании префабов GOPool/GuiPool (Awake → caches[i].Initialize → Instantiate) и создании игрока (TargetManager.CreatePlayer). Повторы (~ dozens) — это и есть «+110» из счётчика пользователя.
- NullReferenceException и прочих runtime-ошибок **нет**; IoC-инициализация, генерация карты, физика, GUI отработали.

### Выявленная причина missing script
По истории Git пользователем **намеренно удалены**:
- коммит `f3d1fb1` (2026-11-20) — «удалили нгуи»: фреймворки NGUI/NData (`Assets/external/NData/**`);
- коммит `9fd866c` (2026-11-22) — «remove unused LightRendererOld (dead code)».

Аудит `MissingScriptsProbe.Scan` нашёл потерянные компоненты в:
| Ресурс | GO | missing |
|---|---|---|
| `Assets/PoolPefubs/Boom.prefab` | Boom/PointLight | 2 |
| `Assets/PoolPefubs/gui/RedLbl.prefab` | RedLbl | 4 |
| `Assets/Prefubs/gui/bagItem.prefab` | bagItem, icon, count, number | 11 |
| `Assets/Resources/gui/guiMain.prefab` | guiMain | 1 |
| `Assets/Resources/units/Player.prefab` | Player/PointLight | 2 |
| `Assets/MainSceneLight.unity` | Main Camera, Pirate5/Light, Pirate5/WaterParticles, player | 4 |
Итого: **24 потерянных компонента** (остатки NGUI/UI-байндингов NData и LightRendererOld).

### Исправление
- `MissingScriptsProbe.RemoveMissing` (batch): `GameObjectUtility.RemoveMonoBehavioursWithMissingScript` по иерархии, сохранение через `SavePrefabAsset`/`SaveScene`. Удалено ровно 24 компонента.
- Обоснование безопасности: компоненты с отсутствующим скриптом не выполняют ни одного вызова (Unity их не запускает) — поведение игры идентично; функциональность NGUI была удалена пользователем ранее сознательно (в репозитории миграция UI на uGUI уже сделана).
- Контроль: повторный `Scan` — **0 потерянных скриптов** во всех префабах и сценах (`missing_scan2.log`).

### Проверки дальше
- `compile_final.log`: **0 предупреждений, 0 ошибок** (batch, после всех правок).
- Запущен финальный GUI-прогон с двумя циклами Play (проверки B/C/D): `play_final.log`.

---

## Запись 4 — 2026-11-22 — Прогон вскрыл скрытый баг; финальная верификация

1. `play_final.log` (2 цикла): missing script — 0, предупреждений — 0, НО обнаружен
   `NotImplementedException` каждый кадр из `EmptyTileMesh.IBaseInfinityTileMap.get_layerData`
   — моя правка CS0108 (унификация слота `layerData` в интерфейсах) переключила рантайм
   с публичного свойства (LayerData.LightLayer) на явный бросающий член. Исправлено:
   бросающий явный член удалён в `EmptyTileMesh.cs`, рабочая семантика сохранена.
2. `play_final2.log` — эталонный прогон: открытие → Play#1 (25 с) → стоп → Play#2 (15 с) → стоп → выход:
   **все фазы: missing=0, exceptions=0, warnings=0**. Игра инициализировалась корректно
   (карта сгенерирована, игрок создан, IoC отработал, пулы префетчены, ресурсы выгружаются штатно).
3. Временные зонды удалены (`Assets/Editor/WarningsProbe/**`); контрольная batch-сборка
   `compile_after_probe_removal.log`: **0 предупреждений, 0 ошибок**.
4. Проверение A (открытие) — чисто; B/C/D (Play, повтор, остановка) — чисто. См. TEST_REPORT.

### Git-коммиты (итог)
- `13b7593 fix: устранены предупреждения компилятора Unity 6000.6 в коде проекта (CS0618/CS0108/CS0114/CS01xx/UAC)` (24 файла)
- `01c0aca fix: замены устаревших API в сторонних ассетах (CartoonFx, Ultimate Joystick, Tiled Sprite, TexturePacker)` (8 файлов)
- `756f294 fix: удалены 24 потерянных компонента (NGUI/LightRendererOld) из префабов и MainSceneLight` (6 файлов)
- `fbdda01 docs: журнал и отчёты по устранению предупреждений (Unity_Warnings_Work)` (5 файлов документации)
- Незакоммиченные изменения пользователя (meta-файлы спрайтов, Packages/*.json, ProjectSettings, TrueMiner.slnx) полностью сохранены в рабочей копии, не коммитились.
- Инцидент при коммитовании: `git add Assets/external/CartoonFx` (каталогом) захватил пользовательские meta-изменения; исправлено без потери данных — `git reset --mixed 13b7593` (дерево не трогалось), после чего коммиты пересозданы с явными списками файлов.
- Логи-доказательства остались на диске (`Unity_Warnings_Work/*.log`) без коммита (объём).

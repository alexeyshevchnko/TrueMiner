# WORK_LOG — Миграция TrueMiner на Unity 6000.6.10f1

> Отдельный журнал миграции проекта с Unity `6000.0.60f1` на `6000.6.10f1`.
> Не связан с документацией предыдущих задач (`GUI_Agent_Work` не используется).
> История записей ведётся последовательно, не перезаписывается.

---

## Запись 1 — 2026-11-22 — Первичная диагностика

### Определение версий
- `ProjectSettings/ProjectVersion.txt` (текущее состояние): `m_EditorVersion: 6000.6.10f1`
- Исходная версия (по `git diff ProjectSettings/ProjectVersion.txt` и истории): `6000.0.60f1`
- Установленный редактор: `D:\Unity\6000.6.10f1\Editor\Unity.exe` (запуск проекта зафиксирован в логах Hub, 2026-11-22 11:42 UTC).

### Состояние Git до начала работы
- Ветка: `refactor`, опережает `origin/refactor` на 6 коммитов.
- Незакоммиченные изменения пользователя (обнаружены, **не трогаются**):
  - ~600+ изменённых `.png.meta` (реимпорт спрайтов новой Unity);
  - `Packages/manifest.json`, `Packages/packages-lock.json` (обновление пакетов при открытии);
  - `ProjectSettings/PackageManagerSettings.asset`, `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/ProjectVersion.txt`;
  - `TrueMiner.slnx` (новый формат решения Unity 6);
  - новые файлы: `ProjectSettings/PhysicsCoreProjectSettings2D.asset`, `ProjectSettings/ProjectAuditorSettings.asset`.

### Сбор пустого набора ошибок
- Источник: `D:\projects\TrueMiner\Logs\Editor.log` (лог последней сессии, 16 569 строк).
- Проект находился в «safe mode» («Exiting safe mode» в логе) из-за ошибок компиляции.
- Единственный блок ошибок: **Script Compilation Error for Assembly-CSharp.dll (+2 others)**.
- **Итого: 5 ошибок CS0619 в 2 файлах** (одна первопричина — `Object.GetInstanceID()` запрещён как ошибка в 6000.6).
- Дополнительно в логе: `Shader error in 'Sprites/Clipped'` (дублирование ключевого слова OFF — предупреждение о вариантах, не блокирует компиляцию C#), `Curl error 42` (прерванная сетевая операция Package Manager, не влияет).
- Предупреждений компилятора (CS) сгруппировано: **44 уникальных** (в основном CS0618 — устаревшие API, CS0414/CS0162/CS0108 — стиль кода, существовавшие и ранее).
- asmdef в проекте: только `Assets/Tests/TrueMiner.GuiTests.asmdef` (тестовая сборка).

### Решения
1. Ошибку CS0619 устранять через замену `GetInstanceID()` → `GetEntityId()`.
2. В Unity 6000.6 `GetEntityId()` возвращает структуру `UnityEngine.EntityId`; подтверждено по XML-документации движка (`D:\Unity\6000.6.10f1\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.xml`) и коду встроенных пакетов: у `EntityId` есть **неявное преобразование EntityId → int**. Это позволяет сохранить существующие типы `List<int>` и `Dictionary<int, ...>` без изменения архитектуры.
3. Предупреждения CS0618 не исправлять массово (не миграционная блокирующая проблема; изменение устаревших API способно изменить поведение). Зафиксированы в TEST_REPORT / REMAINING_ISSUES для оценки.

### Текущий статус
Выполняется исправление ошибок CS0619.

---

## Запись 2 — 2026-11-22 — Итерация 1: исправление CS0619 (GetInstanceID)

### Действия
1. Сохранён исходный лог как доказательство: `Unity_Migration_Work/evidence_EditorLog_beforeFix.log`.
2. Заменены 5 вызовов `GetInstanceID()` на `GetEntityId()`:
   - `Assets/src/weapons/Backsword.cs` (строки 112–113);
   - `Assets/external/CartoonFx/Pack 2/JMO Assets/Cartoon FX/Spawn System/CFX_SpawnSystem.cs` (строки 25, 104, 146).
3. Первая версия правки полагалась на неявное преобразование `EntityId → int` (переменные `int uniqueId`, `List<int>` сохранялись).
4. Пакетная компиляция (`Unity.exe -batchmode -quit`, лог `compile_batch_1.log`) показала **следующий уровень** той же проблемы: сам оператор `EntityId.implicit operator int(EntityId)` в 6000.6 помечен `[Obsolete(..., true)]` — 5 новых ошибок CS0619: *"EntityId will not be representable by an int in the future. This casting operator will be removed in a future version."*

### Решение (итерация 2)
- Отказаться от хранения идентификаторов в `int`: типы изменены на нативный `UnityEngine.EntityId`:
  - `Backsword.cs`: `List<int> DamagableIds` → `List<EntityId>`;
  - `CFX_SpawnSystem.cs`: `Dictionary<int,...>` → `Dictionary<EntityId,...>` (оба словаря), локальные переменные `EntityId uniqueId`.
- Логика (IndexOf/ContainsKey/Add, курсоры пула) не изменена; сериализуемых полей среди них нет (оба контейнера приватные, не сериализуются) — поведение механик сохранено.
- Запущена повторная пакетная компиляция (`compile_batch_2.log`).

### Текущий статус
Ожидание результата итерации 2.

---

## Запись 3 — 2026-11-22 — Итерация 2: компиляция успешна

### Результат пакетной компиляции (`compile_batch_2.log`)
- **Ошибок компиляции нет** (`error CS` — 0 совпадений).
- `ExitCode: 0`, «Exiting batchmode successfully now!».
- Собраны все сборки: `Assembly-CSharp.dll`, `Assembly-CSharp-firstpass.dll`, `Assembly-CSharp-Editor.dll`, `TrueMiner.GuiTests.dll` + все сборки пакетов (`Library/ScriptAssemblies`, время сборки 15:03).
- Время компиляции скриптов: 3.58 с.
- Ошибок импорта / «missing script» / повреждённых ссылок в логах импорта не найдено (проверены `compile_batch_2.log` и `Logs/AssetImportWorkerHW0.log`).

### Предупреждения (не ошибки)
- 91 уникальное предупреждение (в полный список попали и редакторские сборки, которые ранее не компилировались из-за ошибок основного кода):
  - ~40 × CS0618 — устаревшие API (ParticleSystem.*, FindObjectOfType/FindObjectsOfType/FindFirstObjectByType, Renderer.useLightProbes, TextureImporter.spritesheet, PrefabUtility.GetPrefabType, EditorApplication.playmodeStateChanged, PreferenceItem). Критично-потенциальные: `TexturePackerImporter` (sprite sheet API удалена — должен быть проверен вручную при импорте атласов), остальное — внешний ассетный/демо/редакторский код.
  - Новые предупреждения анализатора: UAC0005 (RegenProjects.cs), UAC1001 (RootModel.cs), UAC1009 (GOPool.cs) — описаны в TECHNICAL_NOTES §1.2.
  - Стилевые CS (0414/0219/0162/0108/0114/0109/0659/0661/0168) — существовали до миграции, к ней не относятся, не тронуты.

### Текущий статус
Код миграции завершён. Остались: документация (TEST_REPORT, REMAINING_ISSUES) и Git-коммиты.

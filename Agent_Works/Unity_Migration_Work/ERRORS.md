# ERRORS — Полный список первоначальных ошибок компиляции

Источник: `D:\projects\TrueMiner\Logs\Editor.log` (сессия Unity 6000.6.10f1, 2026-11-22).
Проект остановился в safe mode; единственные ошибки — **5 × CS0619**, все от одной первопричины.

| # | Файл | Строка:колонка | Код | Сообщение | Первопричина | Способ устранения | Статус |
|---|------|----------------|-----|-----------|--------------|-------------------|--------|
| 1 | `Assets/external/CartoonFx/Pack 2/JMO Assets/Cartoon FX/Spawn System/CFX_SpawnSystem.cs` | 25,18 | CS0619 | `'Object.GetInstanceID()' is obsolete: 'Use GetEntityId instead.'` | В Unity 6 (к 6000.6) `GetInstanceID()` переведён из предупреждения в ошибку компиляции (`Obsolete(..., true)`); API заменён на `GetEntityId()`, возвращающий структуру `UnityEngine.EntityId` | Вызов `sourceObj.GetEntityId()`; переменная и ключевые типы переведены на `EntityId` (`Dictionary<EntityId,...>`) | ✔ исправлено |
| 2 | `Assets/external/CartoonFx/Pack 2/JMO Assets/Cartoon FX/Spawn System/CFX_SpawnSystem.cs` | 104,18 | CS0619 | то же | то же | то же (`sourceObject.GetEntityId()`) | ✔ исправлено |
| 3 | `Assets/external/CartoonFx/Pack 2/JMO Assets/Cartoon FX/Spawn System/CFX_SpawnSystem.cs` | 146,18 | CS0619 | то же | то же | то же (`sourceObject.GetEntityId()`) | ✔ исправлено |
| 4 | `Assets/src/weapons/Backsword.cs` | 112,34 | CS0619 | то же | то же (`ai.GetInstanceID()` в `DamagePoint.DoDamage`) | Замена на `ai.GetEntityId()`; `DamagableIds` переведён с `List<int>` на `List<EntityId>` | ✔ исправлено |
| 5 | `Assets/src/weapons/Backsword.cs` | 113,30 | CS0619 | то же | то же | то же (`ai.GetEntityId()`) | ✔ исправлено |

## Замечания
- Ошибки возникли в двух группах кода: собственный код (`Assets/src/weapons/Backsword.cs`) и встроенный сторонний ассет Cartoon FX (`CFX_SpawnSystem.cs`).
- Сторонний код изменён минимально — три вызова API и тип ключей словарей; логика пула (курсоры, добавление/удаление объектов) не тронута.
- Сериализуемых данных с instance id в проекте нет (оба затронутых контейнера приватные, не сериализуются) — поведение механик сохранено.
- **Дополнительная волна той же ошибки (итерация 1):** первый вариант правки опирался на неявное преобразование `EntityId → int`; в 6000.6 сам оператор `EntityId.implicit operator int(EntityId)` помечен `[Obsolete(..., true)]` — компилятор выдал 5 тех же ошибок CS0619 по новым локациям (см. WORK_LOG запись 2). Окончательное решение — нативное хранение `EntityId`. После него обе волны исчезли: компиляция успешна (verify: `compile_batch_2.log`, return code 0).
- Другие ошибки компиляции C# в логе отсутствовали.
- Отдельно в логе (не компиляция C#):
  - `Shader error in 'Sprites/Clipped': Keyword 'OFF' is duplicated…` — предупреждение о дублированном ключевом слове в шейдере, варианты компилируются; миграции не блокирует. См. REMAINING_ISSUES.
  - `Curl error 42: Callback aborted` — прерванная сетевая операция Package Manager, устойчиво не воспроизводится как проблема.

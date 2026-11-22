# TEST_REPORT — Результаты проверки после миграции на Unity 6000.6.10f1

## 1. Проверка компиляции (автоматическая, пакетный режим)
Метод: `D:\Unity\6000.6.10f1\Editor\Unity.exe -batchmode -quit -projectPath D:\projects\TrueMiner -logFile <файл> -acceptSoftwareTermsForThisRunOnly`
- **Итерация 1** (`compile_batch_1.log`): после замены `GetInstanceID()` → `GetEntityId()` с сохранением `int`-контейнеров — провал: 5 × CS0619 на неявное преобразование `EntityId → int` (сам оператор тоже устарел-запрещён в 6000.6). ExitCode 1.
- **Итерация 2** (`compile_batch_2.log`): после перевода контейнеров на `UnityEngine.EntityId` — **успех**:
  - ошибок `error CS…`: 0;
  - `AssetDatabase: script compilation time: 3.578760s`;
  - `ExitCode: 0`, «Exiting batchmode successfully now!»;
  - построены сборки: `Assembly-CSharp.dll`, `Assembly-CSharp-firstpass.dll`, `Assembly-CSharp-Editor.dll`, `TrueMiner.GuiTests.dll`, `UnityEngine.UI.dll`, `Unity.TextMeshPro.dll`, `Unity.Timeline.dll` и др. (`Library/ScriptAssemblies`, 2026-11-22 15:03).

## 2. Проверка импорта и ссылок (по логам)
- `compile_batch_2.log`, `Logs/AssetImportWorkerHW0.log`: сообщений «missing script», «could not be loaded», «failed to import», ошибок сериализации импорта **не найдено**.
- Изменённые правкой контейнеры (`List<EntityId>`, `Dictionary<EntityId,…>`) — приватные, не сериализуются; сериализованные данные сцен/префабов правкой не затронуты ⇒ риск потери ссылок от этих правок отсутствует.
- Полная валидация сцен/префабов в открытом редакторе (инспектор, осмотр инспекторских предупреждений) агентом **не выполнялась** — Unity Editor GUI не был открыт (см. §5).

## 3. Результаты Play Mode
- **Play Mode не запускался.** Причины:
  1. Unity Editor не запущен; запуск Play Mode требует GUI-редактора (интерактивного) — пакетный Play (`batchmode + isPlaying`) для этого проекта не даёт воспроизводимого результата (GUI/джойстики/ввод) и может «зависнуть» без графики;
  2. добавление временного редакторного скрипта-раннера в `Assets` для пакетного Play сочтено риском для состояния проекта (задача запрещает изменение работающих систем без необходимости).
- Компиляция всех игровых скриптов, включая GUI/джойстики/игрока/AI, успешна — статическая проверка выполнена.

## 4. Проверенные системы и механики (статически, по коду и логам)
| Система | Файл | Проверка | Результат |
|---|---|---|---|
| Оружие «сикира» (урон, отбрасывание) | `Assets/src/weapons/Backsword.cs` | замена id-логики на `EntityId`; `List<EntityId>` — приватное поле, не сериализуется | компилируется; логика IndexOf/Add сохранена 1:1 |
| Пул эффектов Cartoon FX | `CFX_SpawnSystem.cs` | ключи словарей `EntityId`; курсоры пула `int` не тронуты | компилируется; GetNextObject/addObjectToPool/removeObjectsFromPool сохранены |
| Генератор карты, физика, AI, GUI | `Assets/Generator`, `Assets/src` | полная компиляция Assembly-CSharp | ошибок нет |

## 5. Что НЕ проверено (требует ручного запуска Unity Editor)
- Визуальный осмотр Console в открытом редакторе после импорта.
- Play Mode: UI (джойстики Ultimate Joystick / TouchArea), управление персонажем, инвентарь, эффекты.
- `TexturePackerImporter` — предупреждение CS0618 `TextureImporter.spritesheet` («Support … removed») — фактическая работа импортера атласов не проверялась (см. REMAINING_ISSUES).
- Загрузка сцен в Editor, осмотр префабов на потерянные ссылки визуально.

## 6. Предупреждения компиляции (91 уникальное, ошибки отсутствуют)
Группы (полный список — `compile_batch_2.log`):
1. **CS0618 — устаревшие API (≈45):** ParticleSystem.* (CartoonFx Pack 2/3, Generator/ParticleScale, BasePoolEffect, BoomEffect), FindObjectOfType/FindObjectsOfType/FindFirstObjectByType (Ultimate Joystick, DeselectOnClick.cs), Renderer.useLightProbes (TiledSpriteRenderer), TextureImporter.spritesheet + PreferenceItem (TexturePackerImporter), PrefabUtility.GetPrefabType (UltimateJoystickEditor), EditorApplication.playmodeStateChanged (StopPlayingOnCompilation.cs).
   Оценка: сейчас не блокируют; большинство — внешний/демо/редакторский код. Риск будущих поломок при следующем обновлении Unity — средний. Отдельно значим только TexturePacker (см. REMAINING_ISSUES).
2. **UAC — анализатор сериализации (3, новые в 6000.6):** UAC0005 (Editor/_AuditFix/RegenProjects.cs — AppDomain.GetAssemblies), UAC1001 (src/gui/RootModel.cs — rootContext без [Serializable]), UAC1009 (Generator/core/utils/GOPool.cs — Hashtable). Оценка: описывают давно существующее поведение Unity (эти типы и раньше не сериализовались), новых поломок в 6000.6 не вносят; учтены для наблюдения в Play Mode.
3. **Стилевые CS0414/CS0219/CS0162/CS0108/CS0114/CS0109/CS0659/CS0661/CS0168 (≈43):** не связаны с миграцией (существовали и в 6000.0), не тронуты — вне рамок задачи.

## 7. Итог
- Компиляция проекта в Unity 6000.6.10f1: **успешна, без ошибок**.
- Функциональность миграционными правками не удалялась и не отключалась; изменены только типы идентификаторов и вызовы API.
- Доказательства: `Unity_Migration_Work/compile_batch_2.log` (успех), `compile_batch_1.log` (итерация 1), `evidence_EditorLog_beforeFix.log` (исходные ошибки). Логи не коммитились (объём; см. WORK_LOG).

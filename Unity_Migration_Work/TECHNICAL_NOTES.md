# TECHNICAL_NOTES — Технические заметки миграции на Unity 6000.6.10f1

## 1. Изменения API Unity (6000.0.60f1 → 6000.6.10f1)

### 1.1 GetInstanceID → GetEntityId (блокирующее изменение)
- В Unity 6 `UnityEngine.Object.GetInstanceID()` поэтапно устаревает: в ветке 6000.6 он помечен `[Obsolete("Use GetEntityId instead.", true)]` — **ошибка компиляции** (CS0619), а не предупреждение.
- `GetEntityId()` возвращает структуру `UnityEngine.EntityId` (подтверждено по `D:\Unity\6000.6.10f1\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.xml` и исходникам встроенных пакетов 6000.6).
- **Важная деталь 6000.6:** оператор `EntityId.implicit operator int(EntityId)` сам помечен `[Obsolete(..., true)]` — компилятор выдаёт CS0619 на любые попытки положить `EntityId` в `int` (включая неявное преобразование). Сообщение: *«EntityId will not be representable by an int in the future»*. В ранних версиях Unity 6 преобразование было допустимым — код, мигрированный «в лоб» (int + implicit cast), здесь не скомпилируется.
- Правильная схема миграции: хранить идентификаторы в `EntityId` (структура поддерживает `Equals`, `GetHashCode`, `==`, `!=`, `CompareTo`, `EntityId.None`, `IsValid()`), т.е. `List<EntityId>`, `Dictionary<EntityId, T>` и т.п.
- В проекте идентификаторы использовались только для рантайм-дедупликации и ключей словарей — сериализуемых данных с instance id нет, значит сохранение поведения гарантировано.

### 1.2 Новые предупреждения анализатора сериализации (UAC)
Появились в логе компиляции 6000.6 (анализатор правил сериализации скриптов):
- `Assets/Generator/core/utils/GOPool.cs(106): warning UAC1009` — поле `activeCachedObjects` типа `System.Collections.Hashtable` не поддерживается сериализацией Unity.
- `Assets/src/gui/RootModel.cs(8): warning UAC1001` — поле `rootContext` типа `RootContext` пропускается сериализацией (нет `[Serializable]`).
- Не блокируют компиляцию; оценены как **существовавшее и ранее поведение** Unity (не-Serializable типы всегда пропускались; Hashtable никогда не сериализовался). Проверить фактическую работу систем при Play Mode не представилось возможным — см. TEST_REPORT / REMAINING_ISSUES.

## 2. Обновления пакетов (выполнены Unity автоматически при открытии проекта)
Источник: `Logs/Packages-Update.log` (2026-11-22). Изменения принадлежат пользователю (не перезаписывались, не коммитились агентом):
- Добавлены модули: `com.unity.modules.adaptiveperformance`, `com.unity.modules.physicscore2d`, `com.unity.modules.tetgen`, `com.unity.modules.timelinefoundation`, `com.unity.modules.vectorgraphics`;
- Удалён: `com.unity.modules.vr`;
- Обновлены: `com.unity.ide.rider` 3.0.40 → 3.1.0, `com.unity.multiplayer.center` 1.0.0 → 2.1.0, `com.unity.test-framework` 1.6.0 → 1.8.0, `com.unity.timeline` 1.8.13 → 6.6.0, `com.unity.ugui` 2.0.0 → 2.6.0.
- Сторонние ассеты в `Assets/external` (CartoonFx, Tiled Sprite, Ultimate Joystick) — обычные скрипты, без asmdef; собраны в Assembly-CSharp. Их адаптация ограничена точечной заменой API (см. ERRORS.md).

## 3. Конфигурация проекта
- Новый формат решения: `TrueMiner.slnx` (Unity 6 генерирует `.slnx` вместо `.sln`) — изменение пользователя, сохранено.
- Новые файлы настроек: `ProjectSettings/PhysicsCoreProjectSettings2D.asset`, `ProjectSettings/ProjectAuditorSettings.asset` — созданы новой версией Unity, сохранены.
- Изменены: `ProjectSettings/ProjectSettings.asset`, `PackageManagerSettings.asset`, `ProjectVersion.txt` — пользователем при открытии; сохранены.

## 4. Прочее
- `Shader error in 'Sprites/Clipped': Keyword 'OFF' is duplicated in several directives` (`Assets/external/Tiled Sprite/Materials/Sprites-Clipped.shader`) — сообщение уровня предупреждения о вариантах шейдера (дублирование ключевого слова), компиляцию не блокирует, миграции не препятствует. Зафиксировано в REMAINING_ISSUES.
- `Curl error 42: Callback aborted` в логе — прерванная сетевая операция (Package Manager), не воспроизводится как устойчивая проблема.
- Пакетная проверка компиляции: `Unity.exe -batchmode -quit -projectPath D:\projects\TrueMiner -logFile <файл> -acceptSoftwareTermsForThisRunOnly`; лицензирование через Hub Licensing Client работает, `ExitCode: 1` при ошибках скриптов («Scripts have compiler errors»).

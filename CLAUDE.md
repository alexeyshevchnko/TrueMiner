# TrueMiner — рабочая инструкция для агента

2D-песочница («копатель»). Unity **6000.0.60f1**, C#, `.NET` стандартный профиль Unity.
Все пользовательские скрипты компилируются в `Assembly-CSharp` — `.asmdef` в проекте нет.

Документация: [Docs/Architecture.md](Docs/Architecture.md) · [Docs/RefactoringPlan.md](Docs/RefactoringPlan.md) · [Docs/Decisions.md](Docs/Decisions.md)

## Главное правило

**Цель — поддерживаемость без изменения игровой логики.** Любая правка, которая меняет
поведение в штатном сценарии, — это не рефакторинг, а изменение игры: её нужно вынести
в [Docs/Decisions.md](Docs/Decisions.md) и согласовать.

## Как устроен проект (кратко)

- **Точки входа нет в привычном виде.** `GameContext : UnityContext<Main>` на объекте в сцене
  создаёт `Main()`, который регистрирует сервисы в самописном IoC (`Assets/external/IoC`).
  Затем `ApplicationManager.OnInject()` (`Generator/core/Managers/ApplicationManager.cs`)
  инициализирует префабы, `TargetManager` и запускает `MapGenerator.StartApplication()`.
- **Свой DI, не Zenject.** `AsSingle<T>()` — настоящий ленивый синглтон на контейнер.
  Инъекция — только **публичные свойства** с `[IoC.Inject]` (поля не инжектятся).
  Незарегистрированный тип даёт `null` **молча** — `null` в инжектированном поле
  почти всегда означает «забыли `Bind`», а не «объект ещё не создан».
- **Сцены пустые.** Из 163 пользовательских скриптов только 43 имеют ссылку в `.unity`/`.prefab`,
  остальные создаются кодом. Остальное — префабы в `Assets/Prefabs`, `Assets/Resources`, `Assets/PoolPefubs`.
- **Своя физика** (`Generator/Physics`, `Generator/core/Managers/PhysicsManager.cs`),
  **свой тайловый рендер** (`Generator/Terrain`), **своя вода** (`Generator/Terrain/Liquid.cs`).
  `UnityEngine`-физика и Tilemap не используются для мира.

## Правила безопасных правок

1. **Не переименовывать и не перемещать `.cs` без `.meta`.** Ссылки в сценах/префабах
   привязаны к GUID из `.meta`. Переносить `.cs` и `.meta` вместе, одним коммитом.
   Скрипт с ненулевым числом ссылок в сценах: `GetColor`, `UnitViewReciver`, `AiController`,
   `SetColor`, `СircleShell`, `TouchJoystickBtn`, `TargetChase`, `ApplicationManager`,
   `GameContext`, `TileRenderer`, `MonoBehaviourPhysicItem`, `PlayerController`, `Flag`,
   `MainMenu`, `GuiPool`, `RootModel` и оружейные `*Item`/`Pick`/`Axe`/`Bow`/`Backsword`/`Torch`/`Grenade`/`Block`/`BlockItem`.
2. **Не менять `Bind<>` и названия интерфейсов** без проверки всех `[IoC.Inject]`-потребителей.
3. **Файл `src/weapons/СircleShell.cs` содержит кириллическую «С» (U+0421)** в имени файла
   и в имени класса. Совпадает сама с собой, но латинская `C` не подойдёт. Не «исправлять»
   без переноса `.meta`.
4. **Две разные формулы «мир↔тайл» не объединять**, пока не принято решение в `Decisions.md`.
   Это изменит попадание урона и коллизий на границах тайлов.
5. **Статические поля** (`MapGenerator.surface`, `surfaceHome`, `LightRenderer.color`,
   `TileDataProvider.Map`, `TileRenderer.screenStart/End`) переживают смену мира и сцены.
   Не добавлять новые статические изменяемые поля.
6. **Мёртвый код не включать.** `MapGenerator.StartApplication()` начинается с `return;`,
   `TileDataProvider.Update()` тоже (симуляция воды выключена). Это осознанное текущее
   состояние, а не баг для починки — см. `Decisions.md`.
7. **Не добавлять `.asmdef`**, пока нет отдельной задачи ускорить сборку: сейчас всё в одной
   сборке и любое разделение потребует переписать порядок зависимостей.

## Проверка после правки

1. Проект компилируется (`Assembly-CSharp`), без новых предупреждений.
2. Плей-режим: копание (кирка/топор), удар мечом, лук и граната, спавн предметов,
   подсветка, открытие сумки, флаг, переключение мир ↔ дом.
3. Мир генерируется тем же, что и до правки (при фиксированном seed, если он есть).
   Сейчас `IRandom` не гарантирует детерминизм — см. `Decisions.md`.

## Где что искать

| Что | Где |
|---|---|
| Композиция, биндинги | `Assets/Generator/core/GameContext.cs`, `Managers/ApplicationManager.cs` |
| Генерация мира | `Assets/Generator/MapGenerator.cs` (1321), `Cave.cs` |
| Схемы тайлов (автотайлинг) | `Assets/Generator/Terrain/TileSham/` |
| Данные и рендер тайлов | `Assets/Generator/TileDataProvider.cs`, `Terrain/*` |
| Вода | `Assets/Generator/Terrain/Liquid.cs` |
| Физика | `Assets/Generator/Physics/*`, `Managers/PhysicsManager.cs` |
| Пулы | `Assets/Generator/core/utils/GOPool.cs`, `src/gui/GuiPool.cs` |
| Игрок, ИИ, оружие | `Assets/src/*` |
| UI | `Assets/src/gui/*` |
| IoC | `Assets/external/IoC/` |

## Известные ловушки

- `GOPool.Spawn<T>` бросает NRE, если подходящего кэша нет (в отличие от `Spawn(prefab)`).
- `BufferList` (в `Generator/Physics`) имеет жёсткий лимит 100 записей на буфер.
- `TargetManager.PlayerController` равен `null` до `CreatePlayer()`; обращений без проверки много.
- `CooldownManager` считает время по `DateTime.Now`, а не по игровому времени.

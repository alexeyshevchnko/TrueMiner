# WARNINGS_INVENTORY — Первоначальный перечень предупреждений

Источник: 91 уникальное предупреждение компилятора из лога полной сборки Unity 6000.6.10f1
(`Unity_Warnings_Work/evidence_compile_before.log`, сессия миграции). На счётчик Console пользователя
(~42 при открытии) приходится первая волна Assembly-CSharp; остальные — редакторские сборки, которые
в той сессии не компилировались отдельно. Рантайм-предупреждения Play Mode (~+110 повторов по счётчику)
получены отдельно собственным batch-прогоном (см. WORK_LOG, TEST_REPORT).

Статусы: ✔ устранено · ⏸ отложено (стороннее/нельзя безопасно) · ✖ подавлено (не допускается — не применялось)

## 1. Собственный код проекта

### 1.1 Устаревшие API Unity (Obsolete)
| # | Файл:строка | Предупреждение | Причина | Статус |
|---|---|---|---|---|
| 1 | `Assets/Editor/StopPlayingOnCompilation.cs:22,34` | CS0618 `EditorApplication.playmodeStateChanged` | API заменён на `playModeStateChanged` (PlayModeStateChange) | ✔ заменён; подписка на `ExitedPlayMode` эквивалентна прежней проверке `isPlaying==false` |
| 2 | `Assets/Generator/core/PoolTypes/BasePoolEffect.cs:25` | CS0618 `ParticleSystem.duration` | чтение длительности эффекта | ✔ `main.duration` (эквивалент) |
| 3 | `Assets/Generator/core/PoolTypes/BoomEffect.cs:11` | CS0618 `ParticleSystem.duration` | то же | ✔ `main.duration` |
| 4 | `Assets/Generator/ParticleScale.cs:13,14,18,19` | CS0618 `ParticleSystem.startSize/startSpeed` | масштабирование частиц | ✔ `main.startSize`/`main.startSpeed` с сохранением режима MinMaxCurve (Scale()) |
| 5 | `Assets/src/gui/DeselectOnClick.cs:22` | CS0618 `FindFirstObjectByType<T>()` | поиск EventSystem | ✔ `FindAnyObjectByType<EventSystem>()` (EventSystem в игре один) |

### 1.2 Предупреждения компилятора C#
| # | Файл:строка | Предупреждение | Причина | Статус |
|---|---|---|---|---|
| 6 | `Assets/Generator/core/Managers/SwapItemManager.cs:32` | CS0162 unreachable | присваивание после безусловного `return` в свойстве Map | ✔ удалено недостижимое присваивание + неиспользуемое поле `_map` |
| 7–9 | `Assets/Generator/TileDataProvider.cs:176,772,781` | CS0162 unreachable ×3 | 176: недостижимый `return` после if/else; 772/781: тела методов отключены автором через `return;` (жидкость) | ✔ удалён лишний return; тела закомментированы (поток жидкости был отключён автором ранее) |
| 10 | `Assets/Generator/MapGenerator.cs:301` | CS0219 `lastHillSizeY` | мёртвая переменная | ✔ удалена |
| 11 | `Assets/Generator/MapGenerator.cs:779` | CS0162 unreachable | `StartApplication()` отключён автором (`return;` перед телом) | ✔ тело закомментировано (поведение идентично) |
| 12–13 | `Assets/Generator/Physics/PhysicItem.cs:61,62` | CS0414 `jumpHeight/jumpSpeed` | мёртвые приватные поля | ✔ удалены |
| 14 | `Assets/Generator/Physics/PhysicItem.cs:267` | CS0414 `trace` | мёртвое поле | ✔ удалено |
| 15 | `Assets/Generator/Terrain/InfinityTileMapPixel.cs:151` | CS0414 `b` | мёртвое поле | ✔ удалено |
| 16 | `Assets/Generator/Terrain/Liquid.cs:30` | CS0414 `lastUpdateFrame` | используется только в закомментированном коде | ✔ удалено |
| 17 | `Assets/Generator/Terrain/Liquid.cs:111` | CS0414 `maxWaterUpdate` | используется только в закомментированном коде | ✔ удалено |
| 18 | `Assets/Generator/Terrain/Liquid.cs:114` | CS0219 `countUpdate` | локальная переменная из закомментированной логики | ✔ удалена |
| 19–20 | `Assets/Generator/Terrain/TileRenderer.cs:142,445` | CS0414 `oldTime/countFixedUpdate` | мёртвые поля | ✔ удалены |
| 21 | `Assets/Generator/Terrain/TileSham/Vector2Int.cs:5` | CS0659/CS0661 | Equals/== без GetHashCode | ✔ добавлен согласованный `GetHashCode()` |
| 22 | `Assets/Generator/TileDataProvider.cs:176` (см. 7–9) | — | — | ✔ |
| 23–25 | `Assets/src/ai/MinerBehaviour.cs:21,22,25` | CS0414 `attemptJampCount/dirJamp/countTryJamp` | мёртвые приватные поля | ✔ удалены |
| 26 | `Assets/src/PlayerController.cs:40` | CS0414 `isJamp` | мёртвое приватное поле | ✔ удалено |
| 27 | `Assets/src/weapons/BlowBlockItem.cs:87,91,94,98,103` | CS0162 unreachable ×5 | `break` после `return` в switch GetName | ✔ удалены `break` |
| 28 | `Assets/Editor/ParticleScaleEditor.cs:16` | CS0168 `txt` | неиспользуемая локальная | ✔ удалена |

### 1.3 Скрытие членов (hiding) и сериализация
| # | Файл:строка | Предупреждение | Причина | Статус |
|---|---|---|---|---|
| 29–30 | `Assets/Generator/Terrain/IInfinityTileMap.cs:6,15` | CS0108 `layerData` | повторное объявление свойства, идентичного базовому `IBaseInfinityTileMap.layerData` | ✔ повторные объявления удалены (контракт интерфейсов не изменился) |
| 31 | `Assets/src/weapons/BlockItem.cs:8` | CS0108 `MapGenerator` | повторное IoC-свойство, идентичное базовому `MonoBehaviourPhysicItem.MapGenerator` | ✔ повторное свойство удалено; внедрение идёт по базовому `[IoC.Inject]`-свойству |
| 32 | `Assets/src/weapons/Shell.cs:12` | CS0114 `FixedUpdate` | базовый `MonoBehaviourPhysicItem.FixedUpdate()` виртуальный; Shell объявлял `virtual` (скрытие), тогда как аналогичные Arrow/СircleShell — `override` | ✔ заменено на `override` (исправляет и виртуальный вызов из базового типа; Unity-магический вызов не меняется) |
| 33 | `Assets/src/gui/RootModel.cs:8` | UAC1001 `rootContext` | тип без [Serializable] всегда пропускался сериализацией | ✔ `[NonSerialized]` (намеренность; сериализуемых данных не было — потерь Inspector нет) |
| 34 | `Assets/Generator/core/utils/GOPool.cs:106` | UAC1009 `activeCachedObjects` (Hashtable) | Hashtable никогда не сериализовался Unity | ✔ `[NonSerialized]` (поле — рантайм-кэш, активно используется; тип не менялся) |
| 35 | `Assets/Editor/_AuditFix/RegenProjects.cs:15` | UAC0005 `AppDomain.GetAssemblies()` | может вернуть выгруженные сборки | ✔ поиск типа через `Type.GetType("Type, Assembly", false)` |

## 2. Сторонние ассеты (Assets/external)

### 2.1 Cartoon FX Pack 2/3
| # | Файл:строка | Предупреждение | Решение | Статус |
|---|---|---|---|---|
| 36–37 | `CFX_ShurikenThreadFix.cs:20,31` | CS0618 `enableEmission` | `emission.enabled` (эквивалент) | ✔ |
| 38 | `CFX3_Demo.cs:140` | CS0618 `loop` | `main.loop` | ✔ |
| 39 | `CFXEasyEditor.cs:881` | CS0618 `playbackSpeed` | `main.simulationSpeed` | ✔ |
| 40 | `CFXEasyEditor.cs:921` | CS0618 `startDelay` | `main.startDelay` | ✔ |
| 41–51 | `CFXEasyEditor.cs:947–957` | CS0618 `startDelay/loop/playOnAwake/playbackSpeed/emissionRate/startSpeed/startSize/startColor/startRotation/startLifetime/gravityModifier` | копирование параметров через модули `main`/`emission` (MinMaxCurve/Gradient целиком) | ✔ |
| 52–54 | `CFXEasyEditor.cs:1170–1173` | CS0618 `startSize/gravityModifier/startSpeed` | `ScaleCurve()`/`GetCurveScalar()` с сохранением режима кривой | ✔ |

### 2.2 Ultimate Joystick
| # | Файл:строка | Предупреждение | Решение | Статус |
|---|---|---|---|---|
| 55 | `_Examples/Spaceship Example/Scripts/GameManager.cs:159` | CS0618 `FindObjectOfType<T>` | `FindAnyObjectByType<PlayerController>()` | ✔ |
| 56 | `Scripts/UltimateJoystickScreenSizeUpdater.cs:18` | CS0618 `FindObjectsOfType(Type)` | `FindObjectsByType(type, FindObjectsSortMode.None)` | ✔ |
| 57–60 | `Editor/UltimateJoystickEditor.cs:120,155,718,733` | CS0618 `GetPrefabType/PrefabType` | `GetPrefabAssetType`/`PrefabAssetType.Regular` (эквивалент проверки «ассет-префаб») | ✔ |
| 61 | `Editor/UltimateJoystickEditor.cs:986` | CS0618 `FindObjectOfType<T>` | `FindAnyObjectByType<EventSystem>()` | ✔ |
| 62 | `Editor/UltimateJoystickEditor.cs:1003` | CS0618 `FindObjectsOfType(Type)` | `FindObjectsByType(..., FindObjectsSortMode.None)` | ✔ |

### 2.3 Tiled Sprite
| # | Файл:строка | Предупреждение | Решение | Статус |
|---|---|---|---|---|
| 63 | `TiledSpriteRenderer.cs:296` | CS0618 `useLightProbes` | `lightProbeUsage = LightProbeUsage.Off` (эквивалент false) | ✔ |
| 64 | `TiledSpriteRenderer.cs:1319` | CS0109 `new` на поле `renderer` | базовый `Component.renderer` удалён из API — `new` не нужен | ✔ убран `new` |

### 2.4 TexturePacker (используется: `Assets/Resources/Items.tpsheet`, `TerrariaTiles.tpsheet`)
| # | Файл:строка | Предупреждение | Решение | Статус |
|---|---|---|---|---|
| 65 | `Editor/TexturePackerImporter.cs:36` | CS0618 `[PreferenceItem]` | `[SettingsProvider]` с той же секцией Preferences и тем же EditorPrefs-ключом | ✔ |
| 66–67 | `Editor/TexturePackerImporter.cs:100,161` | CS0618 `TextureImporter.spritesheet` (API удалён) | переписано на официальный `ISpriteEditorDataProvider` (`GetSpriteRects/SetSpriteRects/Apply`); сохранение pivot/alignment/border по имени и перенос `spriteID` — ссылки на спрайты стабильны | ✔ |

## 3. Прочее (не предупреждения компилятора)
- Шейдер `Sprites/Clipped` — дублированное ключевое слово `OFF` (см. Unity_Migration_Work) — сообщение не является warning-строкой Console; оставлено вне задачи.
- Рантайм-предупреждения Play Mode — см. раздел 4.

## 4. Предупреждения Play Mode (выявлены и устранены)

### 4.1 «The referenced script on this Behaviour is missing!» (десятки повторов)
- **Причина:** префабы/сцена содержали компоненты, чьи скрипты были **намеренно удалены** пользователем:
  NGUI/NData (коммит `f3d1fb1` «удалили нгуи», 2026-11-20) и `LightRendererOld` (коммит `9fd866c`
  «remove unused LightRendererOld», 2026-11-22). Компоненты с отсутствующим скриптом ничего не выполняли.
- **Места:** Boom.prefab/PointLight (2), gui/RedLbl.prefab (4), Prefubs/gui/bagItem.prefab (11),
  Resources/gui/guiMain.prefab (1), Resources/units/Player.prefab (2), MainSceneLight.unity (4) — всего 24.
- **Исправление:** `GameObjectUtility.RemoveMonoBehavioursWithMissingScript` по иерархии (batch-зонд),
  сохранение префабов/сцены; контрольный аудит — 0 потерянных скриптов. Поведение игры сохранено.
- **Статус:** ✔ устранено (24/24).

### 4.2 NotImplementedException каждый кадр в Play Mode (Console — как Error)
- **Обнаружено:** финальным 2-цикловым прогоном (`play_final.log`) после правки CS0108 интерфейсов
  `IInfinityTileMap`/`IInfinityTileMapLight` (см. §1.3 #29–30): у `EmptyTileMesh` было два члена `layerData` —
  публичное свойство (возвращало `LayerData.LightLayer`, работавший путь через `IInfinityTileMap`) и явный
  `IBaseInfinityTileMap.layerData`, бросавший исключение. После унификации слота рантайм стал вызывать
  бросающую версию из `TileRenderer.Render` каждый кадр.
- **Исправление:** удалён явный бросающий член в `EmptyTileMesh.cs`; единый слот интерфейса возвращает
  `LayerData.LightLayer` — прежняя рабочая семантика сохранена.
- **Статус:** ✔ устранено (контроль: `play_final2.log` — 0 исключений во всех фазах).

## Итог по инвентарю
- Уникальных предупреждений компилятора: **91** (пунктов в таблицах 67 — часть строк-локаций объединены в группы).
- Все относятся к коду проекта или встроенным ассетам проекта → подлежат исправлению (подавление не применялось).
- Рантайм: missing script 24 → 0; NotImplemented спам → 0.

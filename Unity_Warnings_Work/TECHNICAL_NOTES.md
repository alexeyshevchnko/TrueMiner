# TECHNICAL_NOTES — Устранение предупреждений Unity 6000.6.10f1

## 1. Устаревшие API и их точные замены (проверено компилятором 6000.6)

| Устаревший API | Замена | Примечания |
|---|---|---|
| `ParticleSystem.duration` | `ps.main.duration` | только чтение |
| `ParticleSystem.startSize/startSpeed/...` | `ps.main.startSize` (MinMaxCurve) | см. масштабирование ниже |
| `ParticleSystem.enableEmission` | `ps.emission.enabled` | bool |
| `ParticleSystem.playbackSpeed` | `ps.main.simulationSpeed` | |
| `ParticleSystem.emissionRate` | `ps.emission.rateOverTime` | MinMaxCurve |
| `ParticleSystem.loop / playOnAwake / startDelay / startColor / startRotation / startLifetime / gravityModifier` | `ps.main.*` | MinMaxCurve/MinMaxGradient |
| `Renderer.useLightProbes` | `renderer.lightProbeUsage` | false → `LightProbeUsage.Off` |
| `EditorApplication.playmodeStateChanged` | `EditorApplication.playModeStateChanged` | `PlayModeStateChange` |
| `Object.FindObjectOfType<T>()` | `Object.FindAnyObjectByType<T>()` | `FindFirstObjectByType` в 6000.6 тоже устарел (зависит от порядка instance ID) |
| `Object.FindObjectsOfType(Type)` | `Object.FindObjectsByType(Type)` | В 6000.6 **даже перегрузка с FindObjectsSortMode устарела** — нужен оверлоад без сортировки |
| `PrefabUtility.GetPrefabType / PrefabType` | `GetPrefabAssetType / PrefabAssetType` | `PrefabType.Prefab` → `PrefabAssetType.Regular` |
| `[PreferenceItem]` | `[SettingsProvider]` | секция Preferences через `SettingsProvider` (`SettingsScope.User`) |
| `TextureImporter.spritesheet` (удалён) | `ISpriteEditorDataProvider` | официальный переходный API, см. §3 |
| `AppDomain.GetAssemblies()` (UAC0005) | `Type.GetType("Type, Assembly", false)` | рекомендованы также `TypeCache`/`CompilationPipeline` |

## 2. Ключевые особенности 6000.6, обнаруженные при исправлении
- **`ParticleSystem.MinMaxCurve` не имеет свойства `multiplier`**: скаляр кривой называется `curveMultiplier` (ошибка CS1061 при первой попытке замены «в лоб»). Режимы `Curve`/`TwoCurves` масштабируются через `curveMultiplier`, `Constant` — через `constant`, `TwoConstants` — через `constantMin/constantMax`.
- **`PlayModeStateChange` не содержит `ExitedPlayMode`**: члены `EnteredEditMode`, `EnteringEditMode`, `EnteredPlayMode`, `ExitingPlayMode`. Событие «игра остановилась» = `EnteredEditMode`.
- **`FindObjectsByType(Type, FindObjectsSortMode)` уже устарел** в 6000.6 (появилось новое предупреждение при первой замене со `FindObjectsSortMode.None`) — использовать перегрузку без сортировки.
- **UAC-анализатор сериализации** (новый в 6000.x): UAC0005 (GetAssemblies), UAC1001 (поле не Serializable), UAC1009 (неподдерживаемый тип). Все три случая в проекте — давнее «отсутствие» сериализации, сделанное явным через `[NonSerialized]`/переписано; поведения сериализации не менялись (Unity и раньше эти поля не сериализовал, потерь данных Inspector нет).
- Политика: подавление предупреждений (`#pragma warning disable`, NoWarn, фильтры Console) не использовалось.

## 3. TexturePackerImporter: переход на ISpriteEditorDataProvider
`TextureImporter.spritesheet` удалён из API. Проект использует `Items.tpsheet` и `TerrariaTiles.tpsheet` (Assets/Resources), поэтому переписан реальный путь импорта:
- `SpriteDataProviderFactories().Init()` → `GetSpriteEditorDataProviderFromObject(importer)` → `InitSpriteEditorDataProvider()`;
- чтение `GetSpriteRects()` (класс `UnityEditor.SpriteRect`: name/rect/pivot/alignment(энум)/border/spriteID);
- сохранение по имени pivot/alignment/border, как в оригинале;
- **`SpriteRect.spriteID` переносится от существующего спрайта** — GUID спрайтов не меняется, ссылки на спрайты в сценах/префабах остаются валидными (это критично: `Sprite.spriteID` — стабильный идентификатор);
- для новых спрайтов — `GUID.Generate()`;
- запись через `SetSpriteRects()` + `Apply()`.
- `alignment` в старом `SpriteMetaData` был `int`, в `SpriteRect` — `SpriteAlignment` (энум) — значения перенесены напрямую.
- Поведение `[PreferenceItem]` → `[SettingsProvider]`: та же настройка «Always import pivot points», тот же ключ `TPImporter.ImportPivotPoints`.

## 4. Play Mode probe (batch)
Для фиксации рантайм-предупреждений используется временный редакторный скрипт `Assets/Editor/WarningsProbe/TempBatchPlayRunner.cs` (запуск: `-batchmode -openfile? нет — `-executeMethod TempBatchPlayRunner.Run``), который входит в Play Mode (`EditorApplication.isPlaying`), ждёт ~25 с, выходит и завершает редактор. Все сообщения Console попадают в лог. Скрипт удаляется после прогонов (и фиксируется в WORK_LOG). Сцена игры — `Assets/MainScene.unity` (в Build Settings список пуст, игра запускается из отредактированной и последней открытой сцены).

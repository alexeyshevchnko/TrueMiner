# REMAINING_ISSUES — Проблемы, оставшиеся после миграции

## 1. Требуют ручной проверки в Unity Editor (не выполнимы агентом)
| # | Проблема | Почему осталась | Варианты решения |
|---|----------|-----------------|------------------|
| 1.1 | Play Mode не запускался: UI (джойстики), управление персонажем, инвентарь, эффекты не проверены в рантайме | Unity Editor закрыт; пакетный Play для GUI-проекта без графики невоспроизводим; добавлять раннер-скрипт в Assets признано риском | Открыть проект в 6000.6.10f1, после загрузки безопасного режима не должно быть (компиляция чистая), войти в Play Mode, проверить джойстики/персонажа/инвентарь/эффекты; при проблемах — сверить с `compile_batch_2.log` |
| 1.2 | Визуальный осмотр сцен/префабов на потерянные ссылки компонентов | Полная валидация ссылок требует открытого редактора | В Editor: окна Console/Inspector по ключевым сценам; поиск «Missing Script» |

## 2. Предупреждения, потенциально влияющие на функциональность в будущем
| # | Проблема | Детали | Варианты решения |
|---|----------|--------|------------------|
| 2.1 | `Assets/external/TexturePacker/Editor/TexturePackerImporter.cs` — CS0618 `TextureImporter.spritesheet` («Support … removed»), строки 100, 161; `[PreferenceItem]` (строка 36) | Импортер атласов TexturePacker использует удалённый API доступа к спрайт-данным; сейчас компилируется, но фактическая работа при импорте атласов не проверена и может быть неполной | Если проект использует .tpsheet/атласы TexturePacker — переписать на `UnityEditor.U2D.Sprites.ISpriteEditorDataProvider` (официальная замена) или обновить плагин TexturePacker Importer до версии для Unity 6 |
| 2.2 | ~45 × CS0618 (ParticleSystem.*, FindObjectOfType/FindObjectsOfType/FindFirstObjectByType, Renderer.useLightProbes, PrefabUtility.GetPrefabType, EditorApplication.playmodeStateChanged) | Внешние ассеты (CartoonFx, Ultimate Joystick, Tiled Sprite) и собственный код (ParticleScale.cs, DeselectOnClick.cs, BasePoolEffect/BoomEffect, StopPlayingOnCompilation.cs). Компилируются; поведение API сохранено в 6000.6. При следующем мажоре Unity станут ошибками (как случилось с GetInstanceID) | Постепенная миграция на новые API (`main.startSize`, `emission.enabled`, `FindAnyObjectByType`, `lightProbeUsage`, `[SettingsProvider]`, `playModeStateChanged`); начинать с собственного кода, внешние — при обновлении ассетов |
| 2.3 | UAC0005 `Assets/Editor/_AuditFix/RegenProjects.cs(15)` — AppDomain.GetAssemblies() | Редакторный аудитор; предупреждение новое в 6000.6, поведение функции в редакторе не изменено Unity | При фактических ошибках аудита — перейти на `TypeCache`/`CurrentAssemblies.GetLoadedAssemblies()` |

## 3. Прочее (низкий приоритет)
- `Shader error in 'Sprites/Clipped': Keyword 'OFF' is duplicated…` (`Assets/external/Tiled Sprite/Materials/Sprites-Clipped.shader`) — дублирование ключевого слова в нескольких директивах, лишние варианты шейдера; компиляция шейдера не блокируется, функциональность не нарушена. Решение: убрать `OFF` из всех директив, кроме одной (файл внешнего ассета — изменять без необходимости не стал).
- `Curl error 42` в логе Package Manager — разовый обрыв сети; воспроизводимой проблемы нет.
- Стилевые предупреждения CS0414/CS0219/CS0162/CS0108/CS0114/CS0659/CS0661/CS0168 (~43) — вне рамок миграции (существовали в 6000.0.60f1); сознательно не тронуты.

## 4. Ограничения доступа/среды
- Веб-поиск в среде агента недоступен (ошибка ключа API окружения) — сигнатуры API проверялись по локальной установке Unity 6000.6.10f1 (XML-документация движка, исходники встроенных пакетов) и реальным логам компилятора — это более точный источник для конкретной версии.
- Незакоммиченные изменения пользователя (≈600 meta-файлов, манифест пакетов, ProjectSettings, TrueMiner.slnx) оставлены без изменений и без коммита — решать об их коммите пользователю.

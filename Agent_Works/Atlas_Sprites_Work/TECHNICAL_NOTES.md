# TECHNICAL_NOTES — атласы, извлечение спрайтов и настройки импорта

## Инструмент Atlas Outline Transfer (сессия 13)
- Скрипт: `Assets/Editor/AtlasOutlineTransfer.cs` (EditorWindow, меню `Tools → Atlas Outline Transfer`).
- Назначение: перенос существующей чёрной 2-пиксельной обводки блоков между атласами
  строго по координатам (без масштабирования/смещения/автогенерации).
- Обнаружение: непрозрачные пиксели исходного атласа (alpha/255 > 0.03); глубина d от
  прозрачной границы связной области считается послойным BFS (4-связность); обводка =
  чёрные пиксели (допуск по каждому каналу ±(ColorTolerance·255), по умолчанию ±30)
  с d ≤ OutlineThickness (по умолчанию 2). Внутренние чёрные детали (d > thickness)
  строго исключаются и только подсчитываются в отчёте.
- Причина выбора такой схемы: атлас `TerrariaTiles.png` содержит как внешние контуры
  (3814 чёрных пикселей с d ≤ 2), так и внутренние чёрные детали (3611 пикселей с d ≥ 3),
  поэтому «копировать все чёрные» недопустимо. Границы области объективно выводятся из
  альфа-канала; произвольные эвристики (сетки/выравнивание) не применяются.
- Чтение пикселей: `GetPixels32` для Read/Write-текстур; иначе файл с диска
  (`File.ReadAllBytes` + `ImageConversion.LoadImage` во временную RGBA32-текстуру,
  DestroyImmediate после) — атлас и его Import Settings не изменяются.
- Запись: резервная копия целевого файла в `Agent_Works/Atlas_Sprites_Work/backup/`
  (`<имя>_<yyyyMMdd_HHmmss>.backup.<ext>`, никогда не перезаписывается); затем
  несовпадающие пиксели обводки пишутся в буфер, результат `EncodeToPNG`:
  - целевой файл `.png` → обновляется in-place, `AssetDatabase.ImportAsset` (ForceUpdate);
    существующие пиксели вне обводки сохраняются байт в байт; `.meta` не изменяются;
  - иной формат → отдельный `*_outlined.png` рядом (+ предупреждение и отмена при
    существующем файле), чтобы не портить несовместимый контейнер.
- Preview: временная Texture2D (целевые пиксели + наложенная обводка) внутри окна — ассеты не изменены.
- Idempotency: пиксели, уже совпадающие с обводкой, не перезаписываются; повторный
  перенос дописывает только недостающее — накопления лишних изменений нет.
- Ограничения: обоим атласам нужен совпадающий размер (иначе ошибка и блокировка);
  для текстур, недоступных для GetPixels, в формате кроме png/jpg/jpeg требуется
  Read/Write Enabled (иначе инструмент честно откажется, их контенер нельзя безопасно перечитать).

## Атлас TerrariaTiles (перенос из GUI_Agent_Work, дополнение сессии 10)
- Атлас: `Assets/Resources/TerrariaTiles.png` 1024×1024 32bppArgb, spriteMode 2 (Multiple), isReadable=false; используется слоями террейна (LayerData → Resources.Load<Texture>, материал PixelSnap*), НЕ как Sprite (рендер через Unity-меш по UV, см. InfinityTileMapMesh/LoadSprites + SpriteData).
- Источник нарезки: 26 SpriteRect в `TerrariaTiles.png.meta` (spriteSheet.sprites, y — от нижней кромки) ⇔ 26 записей `TerrariaTiles.tpsheet` (TexturePacker, численно идентичны мете — проверено попиксельно). Атлас ПОЛНОСТЬЮ покрыт этими 26 прямоугольниками (0 нетранспарентных пикселей вне их) — «скрытых» спрайтов в текстуре нет.
- Устаревшие имена: fire1..fire7 — дубли из `Items`-атласа (реальные fire-спрайты и их rect'ы — в Items.tpsheet); Tiles_0 в коде для Terrain0 соответствует спрайту `Tiles_light2` (SpriteData); Tiles_6/118/146/147/153/154/193 — имена без rect/без графики (если раньше были биомные автотайлы — их изображения в текущем атласе нет).
- Извлечение: `Assets/Editor/TerrariaTilesSpriteExtractor.cs` — меню `Tools/Terraria Tiles`: Extract Sprites (пропуск существующих; Alt = разрешить перезапись) и Analyze Uncovered Regions; SourceRect-кропирование без потери пикселей (PNG lossless, Format32bppArgb, ориентация!) в `Assets/Resources/TerrariaTiles_Sprites`; Контактный лист сохраняется ВНЕ папки результатов: `Agent_Works/Atlas_Sprites_Work/preview/ContactSheet.png` (константа ContactSheetPath) — в папке спрайтов только отдельные PNG. Атлас и .meta не изменяются.

## Настройка импорта извлечённых спрайтов (дополнение сессии 11)
- Требуемые настройки для `Assets/Resources/TerrariaTiles_Sprites/*.png`: Texture Type = **Sprite (2D and UI)**, Sprite Mode = **Single**, Alpha Is Transparency = включён, размеры/цветовые данные не изменяются, компрессия не форсируется (Default-платформа, mipmap выкл.), Тайл-рендер террейна не трогается (спрайты предназначены для SpriteRenderer/UI Image по выбору пользователя_).
- Механизмы:
  1. **Editor-инструмент** (`TerrariaTilesSpriteExtractor.ApplyImportSettings`): после сохранения PNG вызывает `AssetImporter.GetAtPath<TextureImporter>`, ставит textureType/spriteImportMode/alphaIsTransparency, `SaveAndReimport()` и затем проверяет фактические значения через `TextureImporter` (не только «наличие настроек в коде»). Отдельный пункт меню `Tools/Terraria Tiles/Verify Imported Sprites` перепроверяет и при необходимости восстанавливает настройки. Настройки применяются только к файлам в `TerrariaTiles_Sprites` (путь жёстко ограничен константой OutputDir-проверкой) — AssetPostprocessor не используется, чтобы не затрагивать остальные текстуры проекта.
  2. **Фактическое состояние**: Unity уже импортировала 27 PNG (26 спрайтов + ContactSheet) с textureType 8 (Sprite) и alphaIsTransparency 1; spriteMode Multiple (автосрезка нестандартных областей) переведён в **Single** правкой настроек импорта (те же `.meta`-значения, что пишет Inspector: `spriteMode: 1`); перезапуск импорта произойдёт редактором при следующем фокусе/либо вручную через Verify пункт.
- Оригинальный атлас `TerrariaTiles.png(.meta)` — НЕ изменялся (остался Multiple для террейн-рендера).
- Внимание: SpriteRect-границы извлечённых спрайтов не переносятся — каждый PNG является законченным изображением объекта (Single); pivot по умолчанию Center (0.5,0.5), 100 px/unit.

## Общие правила направления
- Новые задачи по атласам/извлечению/импорту спрайтов документируются ТОЛЬКО здесь (`Agent_Works/Atlas_Sprites_Work`), не в GUI_Agent_Work.
- Оригинальные атласы никогда не изменяются; извлечение — только через известные границы (SpriteRect/tpsheet/прозрачные промежутки с доказательствами), без произвольной нарезки.

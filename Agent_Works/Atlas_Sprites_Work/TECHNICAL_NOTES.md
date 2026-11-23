# TECHNICAL_NOTES — атласы, извлечение спрайтов и настройки импорта

## Инструмент Atlas Outline Transfer (пиксельная маска, сессия 15)
- Скрипт: `Assets/Editor/AtlasOutlineTransfer.cs` (EditorWindow, меню `Tools → Atlas Outline Transfer`).
- Назначение: формирование результата по пиксельной цветовой маске строго по заданным правилам,
  без распознавания контуров, без flood fill, без определения внешних границ, без эвристик
  соседних тайлов, без автоматического замыкания контуров.
- Цвета маски сравниваются строго по RGB (альфа-канал маски не участвует):
  - `#F777F9` (розовый) → результат полностью прозрачный `(0, 0, 0, 0)`;
  - `#000000` (чёрный) → цвет из настройки Outline Color, альфа 255;
  - `#FF0000` (красный) → скопировать пиксель из Source Content Atlas без изменения RGBA;
  - любой другой цвет → предупреждение, без произвольной подмены.
- Размер результата = min(ширина, высота) двух исходных атласов (Source Mask Atlas и
  Source Content Atlas), без масштабирования, только общая область координат.
- Первый проход — формирование результата попиксельно по маске.
- Второй проход — только для координат с маской `#FF0000`: если альфа в Result Atlas < 255,
  найти ближайший полностью непрозрачный пиксель (Alpha = 255) в Source Content Atlas по
  евклидову расстоянию и скопировать его целиком (все 4 канала RGBA). Поиск ограничен
  областью атласа. Если не найден — предупреждение и статистика, пиксель без изменений.
- Исходные атласы только для чтения. Результат только в Result Atlas.
- Перед перезаписью Result Atlas создаётся резервная копия в
  `Agent_Works/Atlas_Sprites_Work/backup/` (никогда не перезаписывается).
- PNG и `.meta` исходных атласов не меняются; импортные настройки исходных текстур не
  меняются без необходимости.
- Кнопки: Analyze / Preview / Transfer. Статистика Analyze: размеры исходных атласов,
  итоговый размер, количество пикселей каждого цвета маски, количество неизвестных цветов,
  скопировано контента, пикселей обводки, заменено прозрачных во втором проходе,
  без непрозрачного источника.

## Инструмент Atlas Outline Transfer (старая контурная реализация, удалена в сессии 15)
- Предыдущий скрипт `Assets/Editor/AtlasOutlineTransfer.cs` содержал обнаружение обводки
  через послойный BFS (глубина от прозрачного края ≤ thickness), flood fill для вычисления
  внешней границы, эвристики соседних тайлов и автоматическое замыкание контуров.
- В сессии 15 эта логика полностью удалена, не оставлена параллельно с новой.
- Новый класс `AtlasOutlineTransferWindow` реализует только два пиксельных прохода.

## Чтение и запись пикселей
- Чтение: `GetPixels32` для Read/Write-текстур; иначе файл с диска
  (`File.ReadAllBytes` + `ImageConversion.LoadImage` во временную RGBA32-текстуру,
  `DestroyImmediate` после) — атлас и его Import Settings не изменяются.
- Запись: результат `EncodeToPNG` пишется только в файл Result Atlas (in-place для PNG);
  затем `AssetDatabase.ImportAsset` с `ForceUpdate`.
- Резервная копия: `<имя>_<yyyyMMdd_HHmmss>.backup.<ext>` в `backup/`, при ошибке копии
  запись отменяется.

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

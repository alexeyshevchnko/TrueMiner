# Добавление нового тайлсета (Tiles_X) — пошаговая инструкция

Инструкция соответствует текущему состоянию кода (проверено по файлам проекта на момент
написания). Позволяет самостоятельно подключить новый тайлсет, например `Tiles_X`, не меняя
алгоритм автотайлинга и не обращаясь к разработчику.

Быстрый шаблон «скопировал — вставил — заменил» находится в конце документа (раздел 8).

---

## 0. Общая картина

Что происходит при добавлении тайлсета:

```
PNG-текстура (Assets/Resources/TerrariaTiles_Sprites/original/Tiles_X.png)
        ↓ Resources.Load (однократно, кэш TilesetTextures)
SpriteData (запись: имя, сдвиги паддинга, схема, размер окна, TypeId, путь к текстуре)
        ↓
TerrariaFraming / другая схема (выбор кадра по соседям)
        ↓
LayerData.TerrainLayer (список типов, которые рисуются в слое земли)
        ↓
Отрисовка (InfinityTileMapMesh), иконки (BlockIcons), дроп (SpriteManager)
```

Совместимость тайлсетов между собой (кто с кем сливается) задаётся **не в коде**, а в
конфигурационном файле `Assets/Resources/TilesetMergeRules.txt`.

Минимальный набор правок для нового тайлсета Terraria-структуры:

| # | Файл | Что добавить | Обязательность |
|---|---|---|---|
| 1 | `Assets/Resources/TerrariaTiles_Sprites/original/Tiles_X.png` (+ `.meta`) | текстура | обязательно |
| 2 | `Assets/Generator/Terrain/TileTypeEnum.cs` | новое значение типа | обязательно |
| 3 | `Assets/Generator/Terrain/TileSham/TerrariaFraming.cs` | объект правил `TerrariaTilesetRules` | обязательно (для Terraria-структуры) |
| 4 | `Assets/Generator/Terrain/SpriteData.cs` | запись `Tiles_X` | обязательно |
| 5 | `Assets/Generator/Terrain/LayerData.cs` | тип в `TerrainLayer.TileIds` | обязательно |
| 6 | `Assets/Resources/TilesetMergeRules.txt` | пары слияния | опционально (нет пары = без слияния) |
| 7 | `Assets/Generator/TestMapGenerator.cs` | тестовый участок | опционально (проверка) |
| 8 | `Assets/src/Bag.cs` | предмет в сумке | опционально (если блок должен строиться игроком) |

---

## 1. Подготовка текстуры

### Куда класть

```
Assets/Resources/TerrariaTiles_Sprites/original/Tiles_X.png
```

Путь внутри `Resources` важен: система загружает текстуры по записи `TexturePath`
относительно `Assets/Resources` (см. `TilesetMergeConfig`/`TilesetTextures` — константа
`TilesetDirectory = "TerrariaTiles_Sprites/original/"`).

### Как называть

Имя файла без расширения (`Tiles_X`) станет «именем тайлсета» в `SpriteData.Name`
и будет использоваться везде (кадры, иконки, дроп). Не меняйте регистр задним числом:
имя чувствительно к регистру при загрузке пути.

### Размеры и раскладка кадров

Требования для тайлсетов **Terraria-структуры** (совместимых с автотайлингом):

- кадр блока — **16×16 px**;
- шаг между кадрами — **18 px** (2 px паддинга между кадрами, вертикальными и
  горизонтальными полосами);
- решатель (`UvProvider`) всегда пробегает сетку **16×16** кадров, начиная от
  левого-верхнего угла листа, со сдвигом `shiftX = 16*x + ShiftPaddingX*floor(x/ShiftPaddingX)`
  (при `ShiftPaddingX = 1` это ровно 18 px). Кадры за пределами листа схема не
  запрашивает — у листа 288×270 реально используется сетка **15×15** (225 кадров),
  сдвиги `y=15` (и `x=15`) уже выходят за лист и не должны попадать в кадры;
- лист должен содержать стандартный набор Terraria-кадров: внутренний блок, края
  (верх/низ/лево/право), внешние и внутренние углы, одиночный блок, «полосы».
  Ориентир — листы `Tiles_0.png` / `Tiles_1.png` / `Tiles_8.png` (288×270 = 15×15 кадров).

Пример для `Tiles_X`: сделайте лист **288×270** и скопируйте раскладку кадров из
`Tiles_0.png` (земля) — тогда автотайлинг заработает без единой правки кода.

Столбцы/строки считаются так: столбец кадра `x` занимает пиксели
`[18*x, 18*x + 16]`, строка `y` — `[18*y, 18*y + 16]` от верхнего края листа.
Количество полезных строк = `высота / 18` (288×270 → 15×15 = 225 кадров; кадры
краёв и внутренних углов должны лежать в тех же ячейках, что у `Tiles_0`).

### Формат и настройки импорта Unity

PNG с альфа-каналом. Настройки импорта (видно в `.meta` эталонного `Tiles_8.png.meta`):

| Параметр | Значение | Почему |
|---|---|---|
| `filterMode` | `0` (Point) | пиксель-арт; Bilinear даёт «мыло» и цветные швы на границах кадров |
| `enableMipMap` | `0` | mip-уровни усредняют прозрачный паддинг в края кадров → полупрозрачные швы |
| `alphaIsTransparency` | `1` | корректный цвет прозрачных пикселей при фильтрации |
| `nPOTScale` | `0` (none) | любые сдвиги размеров ломают сетку кадров |
| `maxTextureSize` | `2048` | листы ≤ 2048 px не ресайзятся |
| `spriteMode` / `textureType` | как у `Tiles_8` (`1` / `8`) | главное — чтобы `Resources.Load<Texture2D>` работал |

После первого импорта Unity создаст `.meta` — проверьте в нём эти значения
(или задайте в Inspector: Filter Mode = Point, Alpha Is Transparency ✓, Mip Maps ✗,
Non-Power of 2 = None).

**Проверка раскладки:** заполненные кадры не должны содержать «мусор» из соседних
кадров. Пример проверки 1:1 — вырезать кадр `(18,18)` размером 16×16 и убедиться,
что это чистый внутренний блок (см. тест в разделе 7).

---

## 2. Добавление записи в SpriteData

Файл: `Assets/Generator/Terrain/SpriteData.cs`.

### Шаг 2.1. Выбор TypeId

Типы задаются в `Assets/Generator/Terrain/TileTypeEnum.cs`. Занятые значения: `0–25`
(`Terrain8 = 25`), служебные отрицательные числа (`-1…-10`) используются жидкостями и
фоном. Свободные значения начинаются с **26**.

Добавьте в конец enum:

```csharp
    TerrainX = 26, // Тайлсет Tiles_X
```

### Шаг 2.2. Запись SpriteData

В `SpriteData.cs` рядом с остальными тайлсетами добавьте:

```csharp
public static SpriteData Tiles_X = new SpriteData(
    "Tiles_X",                          // Name — имя тайлсета (совпадает с именем PNG)
    1,                                  // ShiftPaddingX — сдвиг паддинга по X (Terraria: 1 → шаг 18 px)
    1,                                  // ShiftPaddingY — сдвиг паддинга по Y
    terrainFramingX,                    // Shema — см. раздел 3
    3,                                  // SquareSizeByShema — окно 3×3 для схемы
    (short)TileTypeEnum.TerrainX,       // TypeId — новый тип из enum
    texturePath: TilesetDir + "Tiles_X" // путь к собственной текстуре (без расширения)
);
```

И рядом (в верхней части файла, где другие схемы) — сам объект схемы:

```csharp
private static readonly TerrainTileShema terrainFramingX =
    new TerrainTileShema(TerrariaTilesetRules.TerrainX);
```

Пояснения по параметрам:

- **Name** — ключ тайлсета. По нему ищутся кадры (`GetUV`), строится иконка
  (`BlockIcons`) и дроп (`ItemManager.CreateBlock`). Должен быть уникальным.
- **ShiftPaddingX/Y** — сколько паддинга прибавляется на каждый столбец/строку
  блоков. Для Terraria-листов всегда `1, 1` (шаг 16+2 = 18 px).
- **Shema** — схема автотайлинга (раздел 3).
- **SquareSizeByShema** — размер окна карты (`N×N`), который `TileDataProvider`
  вырезает вокруг тайла для схемы. Terraria-наборы без взаимной проверки: `3`;
  наборы с взаимной проверкой слияния (как земля): `5`.
- **TypeId** — уникальный `short`. Проверьте `TileTypeEnum.cs`, чтобы не пересечься.
- **CountHeight** — количество «высотных» слоёв кадров (для обычных тайлсетов `1`;
  используется жидкостями).
- **TexturePath** — путь к текстуре относительно `Assets/Resources`, без расширения.
  `null` = тайлсет берётся из общего атласа (старое поведение).

---

## 3. Выбор схемы автотайлинга

Файл: `Assets/Generator/Terrain/TileSham/`.

### 3.1. `TerrainTileShema` — тайлсеты Terraria-структуры (рекомендуется)

Используйте, если новый лист повторяет раскладку `Tiles_0`/`Tiles_1`/`Tiles_8`
(288×270, шаг 18 px, кадры: внутренний блок, края, внешние/внутренние углы,
одиночный блок). Схема передаёт 8 соседей в `TerrariaFraming.Solve`, который
построчно повторяет `WorldGen.TileFrame` оригинальной Terraria.

Для нового тайлсета добавьте в `TerrariaFraming.cs` (класс `TerrariaTilesetRules`)
объект правил:

```csharp
/// <summary>
/// Tiles_X: структура листа как у камня. С кем сливается — файл
/// TilesetMergeRules.txt (раздел 4).
/// </summary>
public static readonly TerrariaTilesetRules TerrainX = new TerrariaTilesetRules(
    selfTypes: new[] { (short)TileTypeEnum.TerrainX }, // сам с собой → SlotSelf
    primaryType: (short)TileTypeEnum.TerrainX,         // ключ пары в конфиге совместимости
    mergeTypes: null,                                  // совместимость — в TilesetMergeRules.txt
    mutualMergeCheckTypes: null,                       // взаимная проверка — общий механизм
    mutualMergeCheckRules: null,
    participatesInMergeBlocks: true,                   // рисует merge-кадры (лист содержит их)
    isGrassLike: false);
```

- `participatesInMergeBlocks: true` — лист содержит merge-кадры (переходы к другим
  тайлсетам) и тайл участвует в merge-блоках решателя. Так устроены камень и руда.
- Для «землеподобного» набора, который сам **не** содержит merge-кадров, а полагается
  на встречную проверку соседей, поставьте `false` и `SquareSizeByShema = 5`
  (смотри ниже) — так устроена земля.

`SquareSizeByShema` для `TerrainTileShema`:
- `3` — достаточно, если у набора нет взаимной проверки (тайлсеты с
  `participatesInMergeBlocks = true`: камень, руда);
- `5` — обязательно для наборов с `participatesInMergeBlocks = false` (как земля),
  т.к. взаимная проверка читает окрестность соседа.

### 3.2. `LegacyTerrainTileShema` — старые упрощённые тайлсеты

Используйте, только если лист нарезан под старую раскладку (как `Tiles_coal`,
`Tiles_gold`, `TILES_wood`, `TILES_brick`, `Tiles_kost`, `Tiles_meat`, `Tiles_med`,
`Tiles_stal`): фиксированные кадры на сетке со сдвигами, которые эта схема
знает наизусть (внутренний блок `(1..3,1)`, края, «полосы», углы — см. исходник
`LegacyTerrainTileShema.cs`). `SquareSizeByShema = 3`.

**Важно:** произвольный лист Terraria-структуры подключать к этой схеме нельзя —
она ожидает конкретные позиции кадров старого формата и 4-соседнюю логику без
диагоналей.

### 3.3. Другие схемы

`GrassTerrainTileShema` (трава `Tiles_2`), `JungleShema`, `GrassShema`,
`TerrainBGTileShema` (фон), `StairsShema`, `LiquidTileShema`, древовидные схемы —
специализированные, для новых «землеподобных» тайлсетов не предназначены.

---

## 4. Настройка совместимости тайлсетов

Файл: `Assets/Resources/TilesetMergeRules.txt`. Формат строки:

```
ИмяA, ИмяB, true|false
```

- правило задаётся для пары и **симметрично** (пара хранится как `{min,max}`);
- каждую пару достаточно указать один раз;
- `false` или отсутствие строки = слияния нет (на стыке рисуются краевые кадры);
- если в файле несколько строк одной пары, действует последняя `true`;
- строки с `#` — комментарии;
- если файл отсутствует/повреждён — встроенные дефолты (пары текущего проекта).

Имена в конфиге: понятные алиасы (`Земля` = Terrain0, `Трава` = TerrainGrass,
`Камень` = Stone, `Руда` = Terrain8), точные имена `TileTypeEnum` (`Stone`,
`TerrainX`, …) и имена записей `SpriteData` (`Tiles_X`, …).

Пример для `Tiles_X`:

```
Земля,Tiles_X,true      # руда X сливается с землёй
Камень,Tiles_X,true     # и с камнем
Руда,Tiles_X,false      # с рудой — НЕ сливается (явный запрет)
```

Файл читается один раз при первом использовании; изменения применяются после
перезапуска игры.

---

## 5. Настройка иконки блока

Файл: `Assets/Generator/Terrain/BlockIcons.cs`.

Как сейчас работает иконка блока в сумке:

1. Предмет `BlowBlockItem` возвращает `GetUiIcon() = "tile:" + TileCollection`
   (например `"tile:Tiles_X"`).
2. `GameUi` (ячейки сумки/хотбара и drag-призрак) видит префикс `tile:` и вызывает
   `BlockIcons.GetSprite(имя тайлсета)`.
3. `BlockIcons` берёт запись `SpriteData`, текстуру через `TilesetTextures`,
   и строит иконку из **представляющего кадра**.

Как выбирается кадр: `BlockIcons` создаёт синтетическое окно, где все соседи — тот же
тип (`SquareSizeByShema × SquareSizeByShema`), и вызывает `data.Shema.GetShema(...)`.
Для Terraria-структуры это ветка «внутренний блок» → кадр `(18, 18)` — полный
блок без краевых рамок, хорошо читается в маленькой ячейке. Для `LegacyTerrainTileShema`
— его собственный «центральный» кадр. Координаты считаются тем же `UvProvider`
и той же формулой, что у отрисовки слоёв — параллельной системы координат нет.

Кэширование: спрайт создаётся один раз на тайлсет и хранится в статическом словаре
`BlockIcons`; повторные вызовы бесплатны. `null`-результат тоже кэшируется.

**Про `IconFrameX`/`IconFrameY`: таких полей в проекте нет** (проверено по
`BlockIcons.cs`). Кадр иконки переопределить вручную нельзя — он определяется схемой
тайлсета на «всё-своём» окне. Если в будущем понадобится ручной выбор кадра иконки,
это будет новая функциональность, а не настройка текущей системы.

Обязательных действий для иконки нет: запись в `SpriteData` + текстура в
`TilesetTextures` — и иконка `Tiles_X` в сумке появится автоматически (предмет в
сумке: `BlowBlockItem(0, (short)TileTypeEnum.TerrainX)`).

Ограничения: у записи должна быть непустая `Shema` и загруженная текстура, иначе
`BlockIcons` вернёт `null` (ячейка будет пустой, в Console — предупреждение
`BlockIcons: …`).

---

## 6. Добавление блока в генератор тестовой карты

Файл: `Assets/Generator/TestMapGenerator.cs`, метод `Generate(...)`.

Пример (по образцу участка Tiles_8, раздел после «камня») — большой прямоугольный
выступ + жила:

```csharp
//Tiles_X — тестовый участок: сплошное пятно рядом с землёй и камнем
int xCenterX = map.GetLength(0) / 2;
int oreHalfSpan = 15;      // полширины участка
int oreSlabHeight = 6;     // высота выступа над грунтом
int oreVeinDepth = 30;     // глубина жилы

for (int x = xCenterX - oreHalfSpan; x <= xCenterX + oreHalfSpan && x < map.GetLength(0); x++) {
    if (x < 0) continue;
    int groundY = surface[x].y;

    for (int y = groundY - oreSlabHeight; y < groundY; y++) {
        if (IsValidPoint(new Vector2Int(x, y)) && map[x, y].type == 0) {
            map[x, y].type = (short)TileTypeEnum.TerrainX;
        }
    }
    for (int y = groundY + 1; y <= groundY + oreVeinDepth; y++) {
        if (IsValidPoint(new Vector2Int(x, y)) && map[x, y].type != 0) {
            map[x, y].type = (short)TileTypeEnum.TerrainX;
        }
    }
}
```

Совет: положите участок рядом с каменной жилой и краем карты — сразу будут видны
стыки с землёй, камнем и воздухом.

---

## 7. Чек-лист проверки

После компиляции и запуска (`Assets/Generator/TestMapGenerator` — активный генератор
тестовой сцены):

- [ ] Текстура загружается: в Console есть
      `TilesetTextures: загружена 'TerrariaTiles_Sprites/original/Tiles_X' WxH`.
- [ ] В Console есть `InfinityTileMapMesh[TerrainLayer]: текстур N (из тайлсетов …)`,
      и N увеличился на ваш тайлсет.
- [ ] Спрайты на карте не смещены: внутренние блоки сливаются в сплошную массу,
      края/углы рисуются кадрами листа (нет «мусора» из соседних кадров и мадженты).
- [ ] Автотайлинг работает: выступ/жила имеют правильные края и внутренние углы.
- [ ] Блок сливается с самим собой (массив X — сплошной).
- [ ] Разрешённое слияние работает (например, `Камень,Tiles_X,true` — стык с камнем
      бесшовный).
- [ ] Запрещённое слияние не происходит (`false` или нет пары — между тайлсетами
      видна граница).
- [ ] Иконка в сумке: выберите/добудьте блок — иконка из собственного листа
      (`BlowBlockItem` в `Bag.Init` либо добыча с разрушенного блока).
- [ ] Выпадающий предмет (разрушение блока) — спрайт из собственного тайлсета.
- [ ] В Unity Console нет ошибок `BlockIcons: …`, `TilesetTextures: …`,
      `UvProvider …` (кадр вне листа).

---

## 8. Готовый шаблон (минимум действий)

Шаблон для очередного `Tiles_X` с Terraria-структурой. Замените: `Tiles_X` → имя,
`TerrainX` → имя типа, `26` → свободный TypeId, `terrainFramingX` → имя схемы.

**1. Текстура** — `Assets/Resources/TerrariaTiles_Sprites/original/Tiles_X.png`
(288×270, кадры как в `Tiles_0`, импорт: Point / без mip / alphaIsTransparency / nPOT=none).

**2. `Assets/Generator/Terrain/TileTypeEnum.cs`** — в конец enum:

```csharp
    TerrainX = 26,
```

**3. `Assets/Generator/Terrain/TileSham/TerrariaFraming.cs`** — в класс
`TerrariaTilesetRules` (после `Terrain8`):

```csharp
    /// <summary>Tiles_X: структура листа как у камня.</summary>
    public static readonly TerrariaTilesetRules TerrainX = new TerrariaTilesetRules(
        selfTypes: new[] { (short)TileTypeEnum.TerrainX },
        primaryType: (short)TileTypeEnum.TerrainX,
        mergeTypes: null,                        // совместимость — в TilesetMergeRules.txt
        mutualMergeCheckTypes: null,
        mutualMergeCheckRules: null,
        participatesInMergeBlocks: true,
        isGrassLike: false);
```

**4. `Assets/Generator/Terrain/SpriteData.cs`** — схема и запись:

```csharp
private static readonly TerrainTileShema terrainFramingX =
    new TerrainTileShema(TerrariaTilesetRules.TerrainX);
```

```csharp
public static SpriteData Tiles_X = new SpriteData(
    "Tiles_X", 1, 1, terrainFramingX, 3,
    (short)TileTypeEnum.TerrainX,
    texturePath: TilesetDir + "Tiles_X");
```

**5. `Assets/Generator/Terrain/LayerData.cs`** — в список
`TerrainLayer` (после `Terrain8`):

```csharp
        (short) TileTypeEnum.TerrainX,
```

**6. `Assets/Resources/TilesetMergeRules.txt`** — пары (по желанию):

```
Земля,Tiles_X,true
Камень,Tiles_X,true
```

**7. (опционально)** предмет в сумке — `Assets/src/Bag.cs`, метод `Init`:

```csharp
items[5] = new BlowBlockItem(0, (short)TileTypeEnum.TerrainX) { Count = 100 };
```

**8. (опционально)** тестовый участок — `Assets/Generator/TestMapGenerator.cs`
(пример в разделе 6).

Перезапустите Unity, сгенерируйте тестовую карту, пройдите чек-лист из раздела 7.

---

## Известные особенности архитектуры

- **Номера типов проекта ≠ номера Terraria.** Соответствие — по спрайту
  (`Tiles_0` ↔ dirt, `Tiles_1` ↔ stone), а правила соединения задаются в
  `TerrariaTilesetRules`, не номерами.
- **Совместимость — только в конфиге.** `TerrariaFraming` не содержит пар:
  `ClassifyNeighbor` спрашивает `TilesetMergeConfig.AllowsMerge(primaryType, сосед)`.
  Нет пары — сосед считается границей.
- **Взаимная проверка земли обобщена.** Землеподобный набор
  (`ParticipatesInMergeBlocks = false`) автоматически проверяет любого
  merge-участника (`FindRulesForType`) — список соседей нигде не дублируется.
- **Иконки и дроп** идут через тот же `SpriteData`/`TilesetTextures` — отдельной
  настройки не требуют.
- **Тексты имён предметов** (`GetName`) — в `Assets/src/weapons/BlowBlockItem.cs`;
  для нового типа добавьте там человекочитаемое имя (это про подпись при подборе,
  не про иконку).

# tests — автономный стенд проверки framing

## Запуск

```powershell
dotnet run -c Release
```

Возвращает `0`, если все проверки прошли.

Требования: **.NET SDK 10**, Windows (используется `System.Drawing` / `Microsoft.WindowsDesktop.App`).

## Что внутри

| Файл | Назначение |
|---|---|
| `FramingTest.csproj` | компилирует **файлы игры** по абсолютным путям `D:\projects\TrueMiner\Assets\...` + код тестов; копирует PNG из `assets/` в выходной каталог |
| `Program.cs` | сценарные тесты (41), брутфорс всех комбинаций соседей, фаззинг, детерминизм, производительность |
| `BranchTableTests.cs` | **основной тест**: таблица всех 57 веток `WorldGen.TileFrame` (1.4.4.9) с ожидаемыми кадрами для v=0,1,2 и флагами |
| `Diagnostic.cs` | выгружает в `diagnostic.csv` слоты и кадры каждого камня тестовой карты |
| `FramingSheet.cs` | строит `framing_sheet_stone.png` и `framing_sheet_dirt.png` — атлас 81 комбинации соседей |
| `PreviewGen.cs` | строит тестовую карту и выгружает `terrain_frames.csv` (x;y;type;fx;fy) |
| `DebugRepro.cs` | ручной отладочный вывод для одной конфигурации; **в `Main` не вызывается** |
| `Stubs.cs` | заглушка `TileInfoStructure` и пустое пространство имён `UnityEngine` (нужно для компиляции вне Unity) |
| `render_preview.ps1` | CSV → PNG по оригинальным тайлсетям (визуальная проверка) |
| `assets/` | дампы `Tiles_0` / `Tiles_1` (287×269), вырезанные из `TerrariaTiles.png` |
| `last_test_run.txt` | лог последнего полного прогона |

## Выходные файлы

Все пишутся рядом с `.csproj` (в `bin/<cfg>/<tfm>/../../../`):

`terrain_frames.csv`, `terrain_preview.png`, `zoom_strip.png`,
`framing_sheet_stone.png`, `framing_sheet_dirt.png`, `diagnostic.csv`.

## Рендер картинок

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\render_preview.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\render_preview.ps1 `
  -Zoom 8 -X0 0 -Y0 13 -X1 22 -Y1 22 -Out zoom_strip.png
```

`pwsh` (PowerShell 7) в этой среде отсутствует — используйте `powershell.exe`.
Скрипт сохранён в **UTF‑8 с BOM**, иначе Windows PowerShell 5.1 читает его как ANSI
и падает на русских комментариях.

## Известные особенности

- `FramingSheet` ищет `tiles_0_dump.png` в выходном каталоге (туда их кладёт csproj),
  запасной путь — `tests/assets/`.
- Если переложить тесты в другое место, абсолютные пути в `FramingTest.csproj`
  должны указывать на актуальный путь проекта TrueMiner.
- `DebugRepro` не вызывается намеренно: он печатает отладку и не влияет на результат.

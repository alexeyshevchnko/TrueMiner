# Рендер тестовой карты из кадров, вычисленных решателем.
#
# Вход:  terrain_frames.csv  (формат: x;y;type;fx;fy, где type=1 — камень, type=0 — земля,
#        fx/fy — индексы кадра в сетке 18 px; см. PreviewGen.cs)
#        tiles_0_dump.png / tiles_1_dump.png  (дампы оригинальных спрайтов Tiles_0 / Tiles_1)
# Выход: terrain_preview.png (вся карта), опционально zoom_strip.png (полоса камня)
#
# Запуск:
#   pwsh -File render_preview.ps1
#   pwsh -File render_preview.ps1 -Zoom 8 -X0 0 -Y0 13 -X1 22 -Y1 22
#
# ВАЖНО (грабли GDI+, на которые уже наступали):
#   * арифметику в аргументах New-Object нужно считать заранее и приводить к [int];
#   * Rectangle/Point создавать через -ArgumentList, а не New-Object(Type(...));
#   * InterpolationMode = NearestNeighbor и PixelOffsetMode = Half, иначе тайлы «мылятся».

param(
    [string]$Csv        = "terrain_frames.csv",
    [string]$TilesDirt  = "tiles_0_dump.png",
    [string]$TilesStone = "tiles_1_dump.png",
    [string]$Out        = "terrain_preview.png",
    [int]$Zoom = 2,
    [int]$X0 = 0, [int]$Y0 = 0, [int]$X1 = 0, [int]$Y1 = 0   # X1/Y1 = 0 → вся карта
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$dir = Split-Path -Parent $MyInvocation.MyCommand.Path
function Resolve-Asset([string]$name) {
    foreach ($candidate in @((Join-Path $dir $name), (Join-Path (Join-Path $dir 'assets') $name))) {
        if (Test-Path $candidate) { return (Resolve-Path $candidate).Path }
    }
    throw "Не найден файл: $name"
}

$csvPath = Resolve-Asset $Csv
$dirt    = [System.Drawing.Bitmap]::FromFile((Resolve-Asset $TilesDirt))
$stone   = [System.Drawing.Bitmap]::FromFile((Resolve-Asset $TilesStone))

$rows = Get-Content $csvPath | Select-Object -Skip 1 | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

# Определяем границы, если не заданы
if ($X1 -le 0 -or $Y1 -le 0) {
    $maxX = 0; $maxY = 0
    foreach ($r in $rows) {
        $p = $r.Split(';')
        if ([int]$p[0] -gt $maxX) { $maxX = [int]$p[0] }
        if ([int]$p[1] -gt $maxY) { $maxY = [int]$p[1] }
    }
    $X0 = 0; $Y0 = 0; $X1 = $maxX + 1; $Y1 = $maxY + 1
}

[int]$tilePx = 16
[int]$w = ($X1 - $X0) * $tilePx * $Zoom
[int]$h = ($Y1 - $Y0) * $tilePx * $Zoom
if ($w -le 0 -or $h -le 0) { throw "Пустая область рендера: X0=$X0 X1=$X1 Y0=$Y0 Y1=$Y1" }

$bmp = New-Object -TypeName System.Drawing.Bitmap -ArgumentList $w, $h
$g   = [System.Drawing.Graphics]::FromImage($bmp)
$g.Clear([System.Drawing.Color]::FromArgb(255, 26, 20, 34))   # фон = пустота
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$g.PixelOffsetMode   = [System.Drawing.Drawing2D.PixelOffsetMode]::Half

[int]$stride = 18
[int]$sz = $tilePx * $Zoom
$drawn = 0
foreach ($r in $rows) {
    $p = $r.Split(';')
    [int]$x = $p[0]; [int]$y = $p[1]; [int]$type = $p[2]
    [int]$fx = $p[3]; [int]$fy = $p[4]
    if ($x -lt $X0 -or $x -ge $X1 -or $y -lt $Y0 -or $y -ge $Y1) { continue }

    $sheet = if ($type -eq 1) { $stone } else { $dirt }
    [int]$sx = $fx * $stride
    [int]$sy = $fy * $stride
    if ($sx -lt 0 -or $sy -lt 0 -or ($sx + $tilePx) -gt $sheet.Width -or ($sy + $tilePx) -gt $sheet.Height) {
        Write-Warning "Кадр вне листа: x=$x y=$y frame=($fx,$fy)"
        continue
    }
    [int]$dx = ($x - $X0) * $sz
    [int]$dy = ($y - $Y0) * $sz
    $src = New-Object -TypeName System.Drawing.Rectangle -ArgumentList $sx, $sy, $tilePx, $tilePx
    $dst = New-Object -TypeName System.Drawing.Rectangle -ArgumentList $dx, $dy, $sz, $sz
    $g.DrawImage($sheet, $dst, $src, [System.Drawing.GraphicsUnit]::Pixel)
    $drawn++
}

$g.Dispose()
$outPath = Join-Path $dir $Out
$bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose(); $dirt.Dispose(); $stone.Dispose()
Write-Output "Сохранено: $outPath ($drawn тайлов, область ${X0}..${X1} x ${Y0}..${Y1}, zoom=$Zoom)"

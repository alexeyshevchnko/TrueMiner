# Ключевые факты о типах и таблицах слияния (Terraria 1.4.4.9)

Источники:

- `D:\projects\TrueMiner\Temp\TerrariaOriginalSrc\Terraria.ID\TileID.cs`
- `D:\projects\TrueMiner\Temp\TerrariaOriginalSrc\Terraria\Main.cs`

## TileID.Sets.Dirt / Stone

```csharp
   197: 		public static bool[] Dirt = Factory.CreateBoolSet(0, 668);
    26: 			public static bool[] Stone = Factory.CreateBoolSet(1, 25, 117, 203);
```

Вывод: земля (type 0) попадает в ветку `TileID.Sets.Dirt` (WorldGen.cs:73936),
камень (type 1) — НЕТ, и уходит в ветку `Main.tileMergeDirt` (WorldGen.cs:74139).

## Main.tileMergeDirt[1] = true

```csharp
  7005: 		tileMergeDirt[1] = true;
```

Следствие: камень участвует в merge-блоках, земля — нет.

## SetupTileMerge: BlockMergesWithMergeAllBlock

```csharp
  3339: 	public static void SetupTileMerge()
  3340: 	{
  3341: 		int count = TileID.Count;
  3342: 		tileMerge = new bool[count][];
  3343: 		for (int i = 0; i < tileMerge.Length; i++)
  3344: 		{
  3345: 			tileMerge[i] = new bool[count];
  3346: 		}
  3347: 		for (int j = 0; j < TileID.Count; j++)
  3348: 		{
  3349: 			bool flag = j == 10 || j == 387 || j == 541;
  3350: 			TileID.Sets.BlockMergesWithMergeAllBlock[j] = !flag && tileSolid[j] && !tileSolidTop[j];
  3351: 		}
  3352: 	}
```

Для земли и камня `BlockMergesWithMergeAllBlock` = true (оба solid, не solidTop):
значит весь merge/framing-код для них выполняется.

## Соответствие типов проекта и Terraria

| Проект (`TileTypeEnum`) | Значение | Спрайт | Тип в Terraria |
|---|---|---|---|
| `Terrain0` (земля) | 2 | `Tiles_0` | 0 (Dirt) |
| `Stone` (камень) | 23 | `Tiles_1` | 1 (Stone) |
| `TerrainGrass` (трава) | 5 | `Tiles_2` | 2 (Grass) — своя схема, вне задачи |

Важно: **номера типов в проекте не совпадают с Terraria**. Сопоставление идёт по спрайту
(Tiles_0 ↔ dirt, Tiles_1 ↔ stone), а логика слияния задаётся в `TerrariaTilesetRules`,
а не номерами типов.


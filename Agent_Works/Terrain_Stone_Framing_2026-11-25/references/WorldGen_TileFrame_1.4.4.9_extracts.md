# Извлечения из WorldGen.TileFrame (Terraria 1.4.4.9, декомпиляция)

Источник: `D:\projects\TrueMiner\Temp\TerrariaOriginalSrc\Terraria\WorldGen.cs`

Это дословные фрагменты с номерами строк — по ним получены ВСЕ ожидаемые кадры.
Номера строк относятся именно к этому файлу и могут отличаться в другой декомпиляции.

---

## 1. Классификация 8 соседей и приведение типов (73646–73709)

```csharp
 73646: 							Tile tile2 = Main.tile[i, j - 1];
 73647: 							Tile tile3 = Main.tile[i, j + 1];
 73648: 							Tile tile4 = Main.tile[i - 1, j];
 73649: 							Tile tile5 = Main.tile[i + 1, j];
 73650: 							Tile tile6 = Main.tile[i - 1, j + 1];
 73651: 							Tile tile7 = Main.tile[i + 1, j + 1];
 73652: 							Tile tile8 = Main.tile[i - 1, j - 1];
 73653: 							Tile tile9 = Main.tile[i + 1, j - 1];
 73654: 							int upLeft = -1;
 73655: 							int up = -1;
 73656: 							int upRight = -1;
 73657: 							int left = -1;
 73658: 							int right = -1;
 73659: 							int downLeft = -1;
 73660: 							int down = -1;
 73661: 							int downRight = -1;
 73662: 							if (tile4 != null && tile4.active())
 73663: 							{
 73664: 								left = (Main.tileStone[tile4.type] ? 1 : tile4.type);
 73665: 								if (tile4.slope() == 1 || tile4.slope() == 3)
 73666: 								{
 73667: 									left = -1;
 73668: 								}
 73669: 							}
 73670: 							if (tile5 != null && tile5.active())
 73671: 							{
 73672: 								right = (Main.tileStone[tile5.type] ? 1 : tile5.type);
 73673: 								if (tile5.slope() == 2 || tile5.slope() == 4)
 73674: 								{
 73675: 									right = -1;
 73676: 								}
 73677: 							}
 73678: 							if (tile2 != null && tile2.active())
 73679: 							{
 73680: 								up = (Main.tileStone[tile2.type] ? 1 : tile2.type);
 73681: 								if (tile2.slope() == 3 || tile2.slope() == 4)
 73682: 								{
 73683: 									up = -1;
 73684: 								}
 73685: 							}
 73686: 							if (tile3 != null && tile3.active())
 73687: 							{
 73688: 								down = (Main.tileStone[tile3.type] ? 1 : tile3.type);
 73689: 								if (tile3.slope() == 1 || tile3.slope() == 2)
 73690: 								{
 73691: 									down = -1;
 73692: 								}
 73693: 							}
 73694: 							if (tile8 != null && tile8.active())
 73695: 							{
 73696: 								upLeft = (Main.tileStone[tile8.type] ? 1 : tile8.type);
 73697: 							}
 73698: 							if (tile9 != null && tile9.active())
 73699: 							{
 73700: 								upRight = (Main.tileStone[tile9.type] ? 1 : tile9.type);
 73701: 							}
 73702: 							if (tile6 != null && tile6.active())
 73703: 							{
 73704: 								downLeft = (Main.tileStone[tile6.type] ? 1 : tile6.type);
 73705: 							}
 73706: 							if (tile7 != null && tile7.active())
 73707: 							{
 73708: 								downRight = (Main.tileStone[tile7.type] ? 1 : tile7.type);
 73709: 							}
```

## 2. Сброс WorldGen.merge* и выбор варианта (73892–73905)

```csharp
 73892: 							mergeUp = false;
 73893: 							mergeDown = false;
 73894: 							mergeLeft = false;
 73895: 							mergeRight = false;
 73896: 							int num27 = 0;
 73897: 							if (resetFrame)
 73898: 							{
 73899: 								num27 = genRand.Next(0, 3);
 73900: 								tile.frameNumber((byte)num27);
 73901: 							}
 73902: 							else
 73903: 							{
 73904: 								num27 = tile.frameNumber();
 73905: 							}
```

## 3. Верхняя развилка: ветка TileID.Sets.Dirt (земля) (73924–73956)

```csharp
 73924: 							if (!Main.tileRope[num] && TileID.Sets.BlockMergesWithMergeAllBlock[num])
 73925: 							{
 73926: 								TileMergeAttempt(num, Main.tileBlendAll, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
 73927: 							}
 73928: 							if (Main.tileBlendAll[num])
 73929: 							{
 73930: 								TileMergeAttempt(num, TileID.Sets.BlockMergesWithMergeAllBlock, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
 73931: 							}
 73932: 							if (TileID.Sets.ForcedDirtMerging[num])
 73933: 							{
 73934: 								TileMergeAttempt(num, TileID.Sets.Dirt, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
 73935: 							}
 73936: 							if (TileID.Sets.Dirt[num])
 73937: 							{
 73938: 								if (up > -1 && Main.tileMergeDirt[up])
 73939: 								{
 73940: 									TileFrame(i, j - 1);
 73941: 									if (mergeDown)
 73942: 									{
 73943: 										up = num;
 73944: 									}
 73945: 								}
 73946: 								else if (up == 147)
 73947: 								{
 73948: 									TileFrame(i, j - 1);
 73949: 									if (mergeDown)
 73950: 									{
 73951: 										up = num;
 73952: 									}
 73953: 								}
 73954: 								if (down > -1 && Main.tileMergeDirt[down])
 73955: 								{
 73956: 									TileFrame(i, j + 1);
```

## 4. Ветка Main.tileMergeDirt (камень): земля становится -2 (74139–74150)

```csharp
 74139: 							if (Main.tileMergeDirt[num])
 74140: 							{
 74141: 								TileMergeAttempt(-2, TileID.Sets.Dirt, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
 74142: 								if (num == 1)
 74143: 								{
 74144: 									if ((double)j > Main.rockLayer)
 74145: 									{
 74146: 										TileMergeAttemptFrametest(i, j, num, TileID.Sets.Mud, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
 74147: 									}
 74148: 									TileMergeAttemptFrametest(i, j, num, TileID.Sets.Ash, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
 74149: 								}
 74150: 							}
```

## 5. Guard травяного семейства (74346)

```csharp
 74346: 							if (num == 2 || num == 23 || num == 661 || num == 662 || num == 60 || num == 477 || num == 492 || num == 70 || num == 109 || num == 199 || Main.tileMoss[num] || TileID.Sets.NeedsGrassFraming[num] || TileID.Sets.tileMossBrick[num])
 74347: 							{
 74348: 								flag = true;
 74349: 								TileMergeAttemptWeird(num, -1, Main.tileSolid, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
 74350: 								int num32 = TileID.Sets.NeedsGrassFramingDirt[num];
```

## 6. Вход в merge-блок и нормализация сторон (75556–75581)

```csharp
 75556: 							if (val.X == -1 && val.Y == -1 && (Main.tileMergeDirt[num] || (num > -1 && TileID.Sets.ChecksForMerge[num])))
 75557: 							{
 75558: 								if (!flag)
 75559: 								{
 75560: 									flag = true;
 75561: 									TileMergeAttemptWeird(num, -1, Main.tileSolid, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
 75562: 								}
 75563: 								if (up > -1 && up != num)
 75564: 								{
 75565: 									up = -1;
 75566: 								}
 75567: 								if (down > -1 && down != num)
 75568: 								{
 75569: 									down = -1;
 75570: 								}
 75571: 								if (left > -1 && left != num)
 75572: 								{
 75573: 									left = -1;
 75574: 								}
 75575: 								if (right > -1 && right != num)
 75576: 								{
 75577: 									right = -1;
 75578: 								}
 75579: 								tileMergeCullCache.Cull(ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
 75580: 								if (up != -1 && down != -1 && left != -1 && right != -1)
 75581: 								{
```


## 7. Блок 1: комбинации при всех четырёх сторонах != -1 (75580–75583, 75658–75678, 75862–75886, 75940–75958)

Показаны «голова» цепочки, пример пары сторон, комбинация «все -2» и диагональные слияния.

```csharp
 75580: 								if (up != -1 && down != -1 && left != -1 && right != -1)
 75581: 								{
 75582: 									if (up == -2 && down == num && left == num && right == num)
 75583: 									{
  ...  (75601 down, 75620 left, 75639 right, 75738 left+right, 75758 up+down, 75778/75799/75820/75841 three-side)
 75862: 									else if (up == -2 && down == -2 && left == -2 && right == -2)
 75863: 									{
 75864: 										switch (num27)
 75865: 										{
 75866: 										case 0:
 75867: 											val.X = 108;
 75868: 											val.Y = 198;
 75869: 											break;
 75870: 										case 1:
 75871: 											val.X = 126;
 75872: 											val.Y = 198;
 75873: 											break;
 75874: 										default:
 75875: 											val.X = 144;
 75876: 											val.Y = 198;
 75877: 											break;
 75878: 										}
 75879: 										mergeUp = true;
 75880: 										mergeDown = true;
 75881: 										mergeLeft = true;
 75882: 										mergeRight = true;
 75883: 									}
 75884: 									else if (up == num && down == num && left == num && right == num)
 75885: 									{
 75886: 										if (upLeft == -2)
```

Диагональные слияния (побеждает ПОСЛЕДНЯЯ подошедшая проверка):

```csharp
 75886: 										if (upLeft == -2)
 75887: 										{
 75888: 											switch (num27)
 75889: 											{
 75890: 											case 0:
 75891: 												val.X = 18;
 75892: 												val.Y = 108;
 75893: 												break;
 75894: 											case 1:
 75895: 												val.X = 18;
 75896: 												val.Y = 144;
 75897: 												break;
 75898: 											default:
 75899: 												val.X = 18;
 75900: 												val.Y = 180;
 75901: 												break;
 75902: 											}
 75903: 										}
 75904: 										if (upRight == -2)
 75905: 										{
 75906: 											switch (num27)
  ...  (75922 downLeft, 75940 downRight)
```

## 8. Наш проблемный случай: ветка 76041 (открыта левая сторона, противоположная == num)

```csharp
 76041: 									if (up != -1 && down != -1 && left == -1 && right == num)
 76042: 									{
 76043: 										if (up == -2 && down == num)
 76044: 										{
 76045: 											switch (num27)
 76046: 											{
 76047: 											case 0:
 76048: 												val.X = 72;
 76049: 												val.Y = 144;
 76050: 												break;
 76051: 											case 1:
 76052: 												val.X = 72;
 76053: 												val.Y = 162;
 76054: 												break;
 76055: 											default:
 76056: 												val.X = 72;
 76057: 												val.Y = 180;
 76058: 												break;
 76059: 											}
 76060: 											mergeUp = true;
 76061: 										}
 76062: 										else if (down == -2 && up == num)
 76063: 										{
 76064: 											switch (num27)
 76065: 											{
 76066: 											case 0:
 76067: 												val.X = 72;
 76068: 												val.Y = 90;
 76069: 												break;
 76070: 											case 1:
 76071: 												val.X = 72;
 76072: 												val.Y = 108;
 76073: 												break;
 76074: 											default:
 76075: 												val.X = 72;
 76076: 												val.Y = 126;
 76077: 												break;
 76078: 											}
 76079: 											mergeDown = true;
 76080: 										}
 76081: 									}
```

## 9. Одиночные слияния при трёх открытых сторонах (76327–76403)

```csharp
 76327: 									else if (up == -2 && down == -1 && left == -1 && right == -1)
 76328: 									{
 76329: 										switch (num27)
 76330: 										{
 76331: 										case 0:
 76332: 											val.X = 108;
 76333: 											val.Y = 144;
 76334: 											break;
 76335: 										case 1:
 76336: 											val.X = 108;
 76337: 											val.Y = 162;
 76338: 											break;
 76339: 										default:
 76340: 											val.X = 108;
 76341: 											val.Y = 180;
 76342: 											break;
 76343: 										}
 76344: 										mergeUp = true;
 76345: 									}
 76346: 									else if (up == -1 && down == -2 && left == -1 && right == -1)
  ...  (76346 down, 76365 left, 76384 right)
 76384: 									else if (up == -1 && down == -1 && left == -1 && right == -2)
 76385: 									{
 76386: 										switch (num27)
 76387: 										{
 76388: 										case 0:
 76389: 											val.X = 54;
 76390: 											val.Y = 234;
 76391: 											break;
 76392: 										case 1:
 76393: 											val.X = 72;
 76394: 											val.Y = 234;
 76395: 											break;
 76396: 										default:
 76397: 											val.X = 90;
 76398: 											val.Y = 234;
 76399: 											break;
 76400: 										}
 76401: 										mergeRight = true;
 76402: 									}
 76403: 								}
```

## 10. Generic-цепочка: строгие сравнения с num (76873–76945)

Ветка 76873 — это и есть конфигурация `up=Merge, down=Merge, left=Edge, right=Self`.

```csharp
 76873: 								else if (up != num && down != num && left != num && right == num)
 76874: 								{
 76875: 									switch (num27)
 76876: 									{
 76877: 									case 0:
 76878: 										val.X = 162;
 76879: 										val.Y = 0;
 76880: 										break;
 76881: 									case 1:
 76882: 										val.X = 162;
 76883: 										val.Y = 18;
 76884: 										break;
 76885: 									default:
 76886: 										val.X = 162;
 76887: 										val.Y = 36;
 76888: 										break;
 76889: 									}
 76890: 								}
 76891: 								else if (up != num && down != num && left == num && right != num)
 76892: 								{
 76893: 									switch (num27)
 76894: 									{
 76895: 									case 0:
 76896: 										val.X = 216;
 76897: 										val.Y = 0;
 76898: 										break;
 76899: 									case 1:
 76900: 										val.X = 216;
 76901: 										val.Y = 18;
 76902: 										break;
 76903: 									default:
 76904: 										val.X = 216;
 76905: 										val.Y = 36;
 76906: 										break;
 76907: 									}
 76908: 								}
 76909: 								else if (up != num && down != num && left != num && right != num)
 76910: 								{
 76911: 									switch (num27)
 76912: 									{
 76913: 									case 0:
 76914: 										val.X = 162;
 76915: 										val.Y = 54;
 76916: 										break;
 76917: 									case 1:
 76918: 										val.X = 180;
 76919: 										val.Y = 54;
 76920: 										break;
 76921: 									default:
 76922: 										val.X = 198;
 76923: 										val.Y = 54;
 76924: 										break;
 76925: 									}
 76926: 								}
 76927: 							}
```

## 11. Финальный fallback и применение кадра (76928–76955)

```csharp
 76928: 							if (val.X <= -1 || val.Y <= -1)
 76929: 							{
 76930: 								if (num27 <= 0)
 76931: 								{
 76932: 									val.X = 18;
 76933: 									val.Y = 18;
 76934: 								}
 76935: 								else if (num27 == 1)
 76936: 								{
 76937: 									val.X = 36;
 76938: 									val.Y = 18;
 76939: 								}
 76940: 								if (num27 >= 2)
 76941: 								{
 76942: 									val.X = 54;
 76943: 									val.Y = 18;
 76944: 								}
 76945: 							}
 76946: 							if (Main.tileLargeFrames[num] == 1 && num27 == 3)
 76947: 							{
 76948: 								val.Y += 90;
 76949: 							}
 76950: 							if (Main.tileLargeFrames[num] == 2 && num27 == 3)
 76951: 							{
 76952: 								val.Y += 90;
 76953: 							}
 76954: 							tile.frameX = (short)val.X;
 76955: 							tile.frameY = (short)val.Y;
```


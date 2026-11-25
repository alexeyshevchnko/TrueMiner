# Кросс-проверка по Terraria 1.3.5.3 (вторичный источник)

Файл: `WorldGen_1353.cs` — декомпиляция 1.3.5.3, скачана из сети как **вторичный**
источник для перекрёстной проверки (основной — 1.4.4.9 из `Temp/TerrariaOriginalSrc`).

Имена другие (`index1` = num, `rectangle.X/Y` = val.X/Y), структура веток та же.

## Ветка «открыта левая сторона, противоположная == num» (39564–39604)

```csharp
 39564:                         if (up != -1 && down != -1 && (left == -1 && right == index1))
 39565:                         {
 39566:                           if (up == -2 && down == index1)
 39567:                           {
 39568:                             if (num6 == 0)
 39569:                             {
 39570:                               rectangle.X = (__Null) 72;
 39571:                               rectangle.Y = (__Null) 144;
 39572:                             }
 39573:                             else if (num6 == 1)
 39574:                             {
 39575:                               rectangle.X = (__Null) 72;
 39576:                               rectangle.Y = (__Null) 162;
 39577:                             }
 39578:                             else
 39579:                             {
 39580:                               rectangle.X = (__Null) 72;
 39581:                               rectangle.Y = (__Null) 180;
 39582:                             }
 39583:                             WorldGen.mergeUp = true;
 39584:                           }
 39585:                           else if (down == -2 && up == index1)
 39586:                           {
 39587:                             if (num6 == 0)
 39588:                             {
 39589:                               rectangle.X = (__Null) 72;
 39590:                               rectangle.Y = (__Null) 90;
 39591:                             }
 39592:                             else if (num6 == 1)
 39593:                             {
 39594:                               rectangle.X = (__Null) 72;
 39595:                               rectangle.Y = (__Null) 108;
 39596:                             }
 39597:                             else
 39598:                             {
 39599:                               rectangle.X = (__Null) 72;
 39600:                               rectangle.Y = (__Null) 126;
 39601:                             }
 39602:                             WorldGen.mergeDown = true;
 39603:                           }
 39604:                         }
```

Подветки требуют `up == -2 && down == index1` либо `down == -2 && up == index1`;
при `up == -2 && down == -2` кадр не назначается — как и в 1.4.4.9 (76041–76081).

## Начало generic-цепочки (40072–40092)

```csharp
 40072:                       if (up == index1 && down == index1 && (left == index1 && right == index1))
 40073:                       {
 40074:                         if (upLeft != index1 && upRight != index1)
 40075:                         {
 40076:                           if (num6 == 0)
 40077:                           {
 40078:                             rectangle.X = (__Null) 108;
 40079:                             rectangle.Y = (__Null) 18;
 40080:                           }
 40081:                           else if (num6 == 1)
 40082:                           {
 40083:                             rectangle.X = (__Null) 126;
 40084:                             rectangle.Y = (__Null) 18;
 40085:                           }
 40086:                           else
 40087:                           {
 40088:                             rectangle.X = (__Null) 144;
 40089:                             rectangle.Y = (__Null) 18;
 40090:                           }
 40091:                         }
 40092:                         else if (downLeft != index1 && downRight != index1)
```

Сравнения строгие `== index1` — подтверждает, что `IsConnected(Self||Merge)` в нашей
реализации был ошибкой.


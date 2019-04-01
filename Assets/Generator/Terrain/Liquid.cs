using UnityEngine;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using  System.Linq;
using System.Runtime.CompilerServices;
using Managers;


public class Liquid : ILiquid,IoC.IInitialize {
    private List<Vector2Int> activePointIndex = new List<Vector2Int>();
    private List<Vector2Int> inactivePointIndex = new List<Vector2Int>();

    [IoC.Inject]
    public IScreenUpdateManager ScreenUpdateManager { set; protected get; }

    private MapGenerator mapGen;

    public Liquid(MapGenerator mapGen) {
        Application.runInBackground = true;
        this.mapGen = mapGen; 
    }
     
    public void AddWater(Vector2Int pos) {
        activePointIndex.Add(pos);
        SetID(posCenter, 1);
        SetCount(pos, Settings.WATER_COUNT);
    }


    private int lastUpdateFrame = 0;
    public void Update() {
        if (mapGen.mapType == MapGenerator.MapType.home)
            return;
        //Debug.LogError("Update");
   //     if (Time.frameCount - lastUpdateFrame > 100) {
            UpdateWater();
           // lastUpdateFrame = Time.frameCount;
     //   }
    }

    private void ChackWater(Vector2Int pos, bool isWater) {
        if (mapGen.IsValidPoint(pos))
            mapGen.GetMap()[pos.x, pos.y].ChackWater(isWater);
    }

    private byte GetCount(Vector2Int pos) {
        if (mapGen.IsValidPoint(pos))
            return mapGen.GetMap()[pos.x, pos.y].count;
        return Settings.WATER_COUNT;
    }

    [IoC.Inject]
    public ISwapItemManager SwapItemManager { set; protected get; }

    private void SetCount(Vector2Int pos, byte val) {
        if (mapGen.IsValidPoint(pos)) {
            mapGen.GetMap()[pos.x, pos.y].count = val;
            if (ScreenUpdateManager != null)
                ScreenUpdateManager.RedrawWorldTile3X(1, pos.x, pos.y);

            if (SwapItemManager != null && val != 0) {
                var id = mapGen.GetMap()[pos.x, pos.y].ItemId;
                if (id != 0 && !ItemData.GetById(id).InWater) {
                    SwapItemManager.RemoveItem(pos);
                }
            }

         //   if (mapGen.Map[pos.x, pos.y].count == Settings.WATER_COUNT) {
                //mapGen.Map[pos.x, pos.y].lighted = false;
          //  }
        }
    }
    private void SetInfo(Vector2Int pos, string val) {
        if (mapGen.IsValidPoint(pos))
            mapGen.GetMap()[pos.x, pos.y].info = val;
    }

    TileInfoStructure GetTile(Vector2Int pos) {
        if (mapGen.IsValidPoint(pos))
            return mapGen.GetMap()[pos.x, pos.y];
        else {
            return new TileInfoStructure(0,0) {count = 0,id = 0,layer = 0,randomTile = 0,type = 1};
        }
    }

    private short idd = 0;
    private void SetID(Vector2Int pos, short id) {
        if (id == 1) {
            idd++;
            mapGen.GetMap()[pos.x, pos.y].id = (short)(pos.x + pos.y);
        } else {
            mapGen.GetMap()[pos.x, pos.y].id = id;    
        }
        
    }

    TileInfoStructure GetTile(int x, int y) {
        return GetTile(new Vector2Int(x, y));
    }

   

    private Vector2Int posBottom;
    private Vector2Int posLeft;
    private Vector2Int posRight;
    private Vector2Int posCenter;
    private TileInfoStructure center;
    private TileInfoStructure left;
    private TileInfoStructure right;
    private TileInfoStructure bottom;
    private int maxWaterUpdate = 500;
    public void UpdateWater() {
      //  Debug.Log(string.Format("active = {0}, inactive = {1}", activePointIndex.Count, inactivePointIndex.Count));
        int countUpdate = 0;
        for (int i = activePointIndex.Count - 1; i >= 0; i--) {
           // countUpdate++;
          //  if (Time.frameCount > 100&& countUpdate > maxWaterUpdate)
           //     break;
            
            posCenter = activePointIndex[i];

            if (!ChackPos(posCenter)) {
                activePointIndex.RemoveAt(i);
                SetID(posCenter, 0);

             //   RemoveLastCountDictionary(posCenter);

                continue;
            }

            posBottom = new Vector2Int(posCenter.x, posCenter.y - 1);
            posLeft = new Vector2Int(posCenter.x - 1, posCenter.y);
            posRight = new Vector2Int(posCenter.x + 1, posCenter.y);

            
            center = GetTile(posCenter);
            left = GetTile(posLeft);
            right = GetTile(posRight);
            bottom = GetTile(posBottom);

            WaterRan2();



           
        }

        ClearPourCount();

        for (int i = activePointIndex.Count - 1; i >= 0; i--) {
            Vector2Int posCenter = activePointIndex[i];
            if (GetCount(posCenter) == 0 || !GetTile(posCenter).IsWater()) {
                ChackWater(posCenter,false);
                activePointIndex.RemoveAt(i);
            } else {/*
                posBottom = new Vector2Int(posCenter.x, posCenter.y - 1);
                bottom = GetTile(posBottom);
                
                if (!CanPour(bottom) && GetLastCountDictionary(posCenter) == GetNCount(posCenter)) {
                    activePointIndex.RemoveAt(i);
                    SetID(posCenter, 0);

                    RemoveLastCountDictionary(posCenter);
                    IsACap(posCenter);
                } else {
                    SetLastCountDictionary(posCenter, GetNCount(posCenter));
                }*/
            }
        }

   //     activePointIndex = activePointIndex.OrderByDescending(v => v.y).ThenBy(v => v.x).ToList();
       for (int i = 0; i < activePointIndex.Count; i++) {
       //    Debug.Log(activePointIndex[i]);
        }
        activePointIndex= activePointIndex.OrderByDescending(x => x.y).ToList();
    }



    private Dictionary<string, Dictionary<string, byte>> lastPourCount = new Dictionary<string, Dictionary<string, byte>>();

    private void AddPourCount(Vector2Int from, Vector2Int to, byte count) {
        var fromT =  GetTile(from);
        var toT = GetTile(to);

        string fromId = string.Format("{0}_{1}", from.x, from.y);
        string toId = string.Format("{0}_{1}", to.x, to.y);

        if (fromT.id == 0 || toT.id == 0)
            return;

        if (!lastPourCount.ContainsKey(fromId)) {
            var val = new Dictionary<string, byte>();
            val[toId] = count;
            lastPourCount[fromId] = val;
        } else {
            if (!lastPourCount[fromId].ContainsKey(toId)) {
                lastPourCount[fromId].Add(toId, count);
            } else {
                lastPourCount[fromId][toId] = count;
            }
        }
    }

    private void ClearPourCount() {
        lastPourCount.Clear();
    }

    private bool IsAlreadyBeenPourCount(Vector2Int from, Vector2Int to, byte count) {
        var fromT = GetTile(from);
        var toT = GetTile(to);

        string fromId = string.Format("{0}_{1}", from.x, from.y);
        string toId = string.Format("{0}_{1}", to.x, to.y);

        if (lastPourCount.ContainsKey(toId) && lastPourCount[toId].ContainsKey(fromId)) {
            return lastPourCount[toId][fromId] == count;
        }
        return false;
    }

    private void WaterRan2() {
        //1 проверяем можем ли мы перетеч вниз если да то на половину перетекаем вниз
        if (CanPour(bottom)) {
            var countCenter = GetCount(posCenter);
            Pour(posCenter, posBottom, countCenter);
        } /*else*/ {
            var isOpenToLeft = IsOpenInTheDirection(posCenter, posLeft);
            var isOpenToRight = IsOpenInTheDirection(posCenter, posRight);

            bool isClosedAll = isOpenToLeft == -1 && isOpenToRight == -1;
            bool isOpenAll = isOpenToLeft != -1 && isOpenToRight != -1;

            if (!isOpenAll && !isClosedAll) {
                if (isOpenToLeft != -1 && isOpenToRight == -1) {
                    Pour(posCenter, posLeft, GetCountPour(center, left));
                } else
                if (isOpenToLeft == -1 && isOpenToRight != -1) {
                    Pour(posCenter, posRight, GetCountPour(center, right));
                }
            }

            if (isClosedAll || isOpenAll) {
                int delta = GetParityCountPour(left, center, right);
                if (delta > 0) {
                    Pour(posCenter, posRight, delta);
                    Pour(posCenter, posLeft, delta);
                } else {
                    int countToLeft = GetPotentialInDirection(posCenter, posLeft);
                    int countToRight = GetPotentialInDirection(posCenter, posRight);

                    if (countToLeft > countToRight) {
                        Pour(posCenter, posLeft, GetCountPour(center, left));
                    }else
                    if (countToLeft < countToRight) {
                        Pour(posCenter, posRight, GetCountPour(center, right));
                    }
                }
            }

            //if (isClosedAll) {
                //IsACap(posCenter);
            //}

        
        }
    }

    int GetPointPotential(TileInfoStructure point) {
        if (!point.CanPourWater())
            return 0;

        return Settings.WATER_COUNT - point.count;
    }


    private int lastCount = 0;

    private bool ChackPos(Vector2Int pos) {

        if (!mapGen.IsValidPoint(pos))
            return false;
        

        var D = CanPour(pos.x, pos.y - 1);
        if (D)
            return true;

        var posL = new Vector2Int(pos.x - 1, pos.y);
        var posR = new Vector2Int(pos.x + 1, pos.y);
        var posT = new Vector2Int(pos.x, pos.y + 1);
        var posLL = new Vector2Int(pos.x - 2, pos.y);
        var posRR = new Vector2Int(pos.x + 2, pos.y);
        var L = CanPotentialPour(pos, posL); //CanPour(pos.x - 1, pos.y);
        var R = CanPotentialPour(pos, posR); //CanPour(pos.x + 1, pos.y);
        /*
        if (L && R) {
            if (GetTile(posLL).IsWater()  && GetTile(posRR).IsWater()) {
                if (Mathf.Abs(GetTile(pos).count - GetTile(posL).count) == 1 && Mathf.Abs(GetTile(posLL).count - GetTile(pos).count) <= 1 ) {
                    if (Mathf.Abs(GetTile(pos).count - GetTile(posR).count) == 1 && Mathf.Abs(GetTile(posRR).count - GetTile(pos).count) <= 1) {
                        return false;
                    }
                }
                

            }

            if (GetTile(posL).IsWater() && GetTile(posR).IsWater()) {
                if (GetTile(posL).count == GetTile(posR).count && GetTile(posL).count == GetTile(pos).count) {
                    return false;
                }
            }
        }

        */
        
        
        if (L && R && !GetTile(posT).IsWater()) {
            var isOpenToLeft = IsOpenInTheDirection(pos, posL);
            if (isOpenToLeft == -1) {
                var isOpenToRight = IsOpenInTheDirection(pos, posR);
                bool isClosedAll = isOpenToLeft == -1 && isOpenToRight == -1;
                if (isClosedAll) {
                    float averageValue = GetCountInLine(pos);
                    SetInfo(pos, averageValue.ToString());
                    if (Mathf.Abs(averageValue - GetCount(pos)) < 1f) {
                       // return false;
                       // SetCountInLine(pos, (byte)averageValue);
                        IsACap(pos);
                        return false;
                    }
                }
            }
        } else {
            SetInfo(pos, string.Empty);
        }
        

        if (L)
            return true;

        if (R)
            return true;

        return false;
    }

    private int maxVertSync = 1000;
    int IsOpenInTheDirection(Vector2Int from, Vector2Int to) {
        int countStep = 0;
        int direction = to.x - from.x;
        int currentX = from.x;

        if (CanPour(currentX, to.y - 1)) {
            return 0;
        }

        while (true) {
            countStep++;
            currentX = currentX + direction;
            var tile = GetTile(currentX, to.y);
            if (GetPointPotential(tile) == 0)
                break;

            if (CanPour(currentX, to.y - 1)) 
                return countStep;

            if(countStep> maxVertSync)
                break;
        }
        return -1;
    }

    int GetPotentialInDirection(Vector2Int from, Vector2Int to) {
        int rez = 0;
        int myCount = GetCount(from); 
        int direction = to.x - from.x;
        int currentX = to.x;
        int countStep = 0;
        while (true) {
            
            var tile = GetTile(currentX, to.y);
            
            if (!tile.CanPourWater())
                break;

            if (tile.IsWater() && myCount <= tile.count) {
                break;
            }

            if (tile.IsWater())
                rez += Settings.WATER_COUNT;
            else
                rez += myCount - tile.count;

            countStep++;
            currentX = currentX + direction;

            if (countStep > maxVertSync)
                break;
        }

        return rez;
    }

    //вернёт среднее значение воды на линии
    float GetCountInLine(Vector2Int point) {
        float rez = 0;
        int currentX = point.x - 1;
        int countStepL = 0;
        while (true) {
            var newpos = new Vector2Int(currentX, point.y);
            if (!mapGen.IsValidPoint(newpos)) {
                break;
            }
            var tile = GetTile(newpos);

            //если это препятствие
            if (!tile.IsEmpty())
                break;

            if (tile.IsWater())
                rez += tile.count;

            countStepL++;
            currentX = currentX - 1;

            if (countStepL > maxVertSync)
                break;
        }

        currentX = point.x + 1;
        int countStepR = 0;
        while (true) {
            var newpos = new Vector2Int(currentX, point.y);
            if (!mapGen.IsValidPoint(newpos)) {
                break;
            }
            var tile = GetTile(newpos);

            //если это препятствие
            if (!tile.IsEmpty())
                break;

            if (tile.IsWater())
                rez += tile.count;

            countStepR++;
            currentX = currentX + 1;

            if (countStepR > maxVertSync)
                break;
        }

        float steps = countStepL + countStepR + 1;

        rez += GetCount(point);

        return rez / steps;
        //return rez/(float)countStep;
    }

    //вернёт колл изменённых
    int SetCountInLine(Vector2Int point, byte val) {
        int currentX = point.x - 1;
        int countStepL = 0;
        while (true) {
            var newpos = new Vector2Int(currentX, point.y);
            if (!mapGen.IsValidPoint(newpos)) {
                break;
            }
            var tile = GetTile(newpos);

            //если это препятствие
            if (!tile.IsEmpty())
                break;

            if (tile.IsWater())
                SetCount(newpos, val);
            else {
                ChackWater(newpos, true);
                SetCount(newpos, val);
            }

            countStepL++;
            currentX = currentX - 1;

          //  if (countStepL > maxVertSync)
          //      break;
        }

        currentX = point.x + 1;
        int countStepR = 0;
        while (true) {
            var newpos = new Vector2Int(currentX, point.y);
            if (!mapGen.IsValidPoint(newpos)) {
                break;
            }
            var tile = GetTile(newpos);

            //если это препятствие
            if (!tile.IsEmpty())
                break;

            if (tile.IsWater())
                SetCount(newpos, val);
            else {
                ChackWater(newpos, true);
                SetCount(newpos, val);
            }

            countStepR++;
            currentX = currentX + 1;

         //   if (countStepR > maxVertSync)
         //       break;
        }

        if (GetTile(point).IsWater())
            SetCount(point, val);
        else {
            ChackWater(point, true);
            SetCount(point, val);
        }

        return countStepR + countStepL + 1;
    }

    int GetParityCountPour(TileInfoStructure left, TileInfoStructure center, TileInfoStructure right) {
        int parity = Divide(left.count + right.count + center.count, 3);
        int delta = Divide(center.count - parity, 2);
        return delta;
    }

    bool IsACap(Vector2Int point) {
        List<Vector2Int> line = new List<Vector2Int>();
        bool rez = true;
        int currentX = point.x - 1;
        var countStepL = 0;
        while (true) {
            var newpos = new Vector2Int(currentX, point.y);
            if (!mapGen.IsValidPoint(newpos)) {
                break;
            }
            var tile = GetTile(newpos);
            /*if (!tile.IsWater() || tile.count == 0) {
                return false;
            }*/

            //если это препятствие
            if (!tile.IsEmpty()) {
                break;
            }

            line.Add(newpos);
            currentX = currentX - 1;
            countStepL++;
          //  if (countStepL > maxVertSync)
           //       break;
        }
        countStepL = 0;
        currentX = point.x + 1;
        while (true) {
            var newpos = new Vector2Int(currentX, point.y);
            if (!mapGen.IsValidPoint(newpos)) {
                break;
            }
            var tile = GetTile(newpos);
            /*if (!tile.IsWater() || tile.count == 0) {
                return false;
            }*/

            //если это препятствие
            if (!tile.IsEmpty()) {
                break;
            }
            line.Add(newpos);
            currentX = currentX + 1;
            countStepL++;
          //  if (countStepL > maxVertSync)
          //      break;
        }
        line.Add(point);
        line = line.OrderBy(x => x.x).ToList();

        for (int i = 0; i < line.Count; i++) {
            var tileC = GetTile(line[i]);
            var upPos = new Vector2Int(line[i].x, line[i].y + 1);
            var downPos = new Vector2Int(line[i].x, line[i].y - 1);
            
            
            if (!tileC.IsWater() || tileC.count == 0) {
                return false;
            }
            
            if (mapGen.IsValidPoint(upPos)) {
                var tileU = GetTile(upPos);
                if (tileU.IsWater() /*&& tileU.count != Settings.WATER_COUNT*/) {
                    return false;
                }
            }

            if (mapGen.IsValidPoint(downPos)) {
                var tileD = GetTile(downPos);
                /*if (!tileD.IsWater()) {
                    return false;
                }*/

                if (tileD.IsWater() && tileD.count < Settings.WATER_COUNT) {
                    return false;
                }
            }
        }

         float averageValue = GetCountInLine(point);
        
         //SetInfo(point, averageValue.ToString());
         //if (Mathf.Abs(averageValue - GetCount(point)) < 1.5f) { }
        int newCount = ((byte) averageValue)* line.Count;
        int oldCount = (int) (averageValue*(float) line.Count);
        SetCountInLine(point, (byte)averageValue);
        int lostCount = oldCount - newCount;
        if (lostCount > 0) {
            for (int i = 0; i < line.Count; i++) {
                if (i%2 == 0 && GetCount(line[i]) != Settings.WATER_COUNT) {
                    SetCount(line[i], (byte) (GetCount(line[i]) + 1));
                    lostCount --;
                }
                if(lostCount ==0)
                    break;
            }
        }

        if (lostCount > 0) {
            for (int i = 0; i < line.Count; i++) {
                if (i % 2 != 0 && GetCount(line[i]) != Settings.WATER_COUNT) {
                    SetCount(line[i], (byte)(GetCount(line[i]) + 1));
                    lostCount--;
                }
                if (lostCount == 0)
                    break;
            }
        }
        for (int i = 0; i < line.Count; i++) {
            SetInfo(line[i], "ops");
        }
        /*
        for (int i = 0; i < line.Count; i++) {
            int index = GetActiveIndex(line[i]);
            if (index != -1) {
                activePointIndex.RemoveAt(index);
                SetID(line[i],0);
            }
        }*/
        return rez;
    }

    int GetCountPour(TileInfoStructure from, TileInfoStructure to) {
        if (from.count - to.count > 1)
            return Divide(from.count - to.count, 2);
        return 1;
    }

    bool CanPour(TileInfoStructure tile) {
        return tile.CanPourWater();
    }

    private bool CanPotentialPour(TileInfoStructure from , TileInfoStructure to) {

        if (!CanPour(to))
            return false;
        return from.count > to.count;
    }

    bool CanPotentialPour(Vector2Int from,Vector2Int to) {
        return CanPotentialPour(GetTile(from), GetTile(to));
    }



    bool CanPour(int x, int y) {
        return CanPour(GetTile(x, y));
    }

    public void ChackNeighborInactive(Vector2Int pos) {
        var posT = new Vector2Int(pos.x, pos.y + 1);
        var posL = new Vector2Int(pos.x - 1, pos.y);
        var posR = new Vector2Int(pos.x + 1, pos.y);


        if (GetTile(posT).IsWater() && GetActiveIndex(posT) == -1) {
            activePointIndex.Add(posT);
            SetID(posT, 1);
        }

        if (GetTile(posL).IsWater() && GetActiveIndex(posL) == -1) {
            activePointIndex.Add(posL);
            SetID(posL, 1);
        }

        if (GetTile(posR).IsWater() && GetActiveIndex(posR) == -1) {
            activePointIndex.Add(posR);
            SetID(posR, 1);
        }
    }

    int GetActiveIndex(Vector2Int pos) {

        int rez = -1;

        for (int i = 0; i < activePointIndex.Count; i++) {
            if (activePointIndex[i].x == pos.x && activePointIndex[i].y == pos.y) {
                return i;
            }
        }
        return rez;
    }



    void Pour(Vector2Int fromPoint, Vector2Int toPoint, int count) {
        if(count == 0)
            return;

        if (count < 0) {
            Debug.LogError("count  = " + count );
            return;
        }
        
        var to = GetTile(toPoint);
       

        if (!to.IsWater() && to.CanPourWater()) {
            activePointIndex.Add(toPoint);
            SetID(toPoint,1);
            ChackWater(toPoint, true);
        }

        int summStart = GetCount(fromPoint) + GetCount(toPoint);
       // string doReport = string.Format("do from = {0}  to = {1} ", mapGen.Map[fromPoint.x, fromPoint.y].count, mapGen.Map[toPoint.x, toPoint.y].count);

        int pourCount = 0;
        int rezFromCount = 0;
        int rezToCount = 0;
        int fromCount = GetCount(fromPoint);
        int toCount = GetCount(toPoint);

        
        if (count < fromCount)
            pourCount = count;
        else
            pourCount = fromCount;

        var freeTo = Settings.WATER_COUNT - toCount;

        if (freeTo < pourCount) {
            pourCount = freeTo;
        }
         
        if (pourCount == 0)
            return;

        rezFromCount = fromCount - pourCount;
        rezToCount = toCount + pourCount;
        
        ///
        if (IsAlreadyBeenPourCount(fromPoint, toPoint, (byte)(rezToCount))) {
            var index = GetActiveIndex(fromPoint);
            if (index!=-1) {
                SetID(fromPoint, 0);
                SetInfo(fromPoint, "deltete");
                activePointIndex.RemoveAt(index);
            }

            index = GetActiveIndex(toPoint);
            if (index != -1) {
                SetID(toPoint, 0);
                SetInfo(fromPoint, "deltete");
                activePointIndex.RemoveAt(index);
            }

            return;
        }

      

        
        ///
        AddPourCount(fromPoint, toPoint, (byte)GetCount(fromPoint));

        SetCount(fromPoint, (byte)rezFromCount);
        SetCount(toPoint, (byte)rezToCount);

        int summRezult = GetCount(fromPoint) + GetCount(toPoint);
        if (summStart != summRezult) {
            string posleReport = string.Format("posle from = {0}  to = {1} ", GetCount(fromPoint), GetCount(toPoint));
            Debug.LogError(/*doReport + " \n " +*/ "freeTo = " + freeTo + "count = " + count + " pourCount = " + pourCount + " \n " + posleReport);
        }

        ChackNeighborInactive(fromPoint);
        ChackNeighborInactive(toPoint);
    }

    int Divide(float val, float divider) {
        return Mathf.FloorToInt(val / divider); 
    }

    public void OnInject() {

    }
}
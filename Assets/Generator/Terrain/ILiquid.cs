using UnityEngine;
using System.Collections;

public interface ILiquid {
    void AddWater(Vector2Int pos);
    void Update();

    void UpdateWater();
    void ChackNeighborInactive(Vector2Int pos);
}

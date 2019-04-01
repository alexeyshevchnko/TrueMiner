using UnityEngine;
using System.Collections;

public interface ILightRenderer {
    void Test();
    Vector2Int UpdateScreenMin { get; }
    Vector2Int UpdateScreenMax { get; }

    void CalculateChangeScreen(Vector2Int min, Vector2Int max);
    void Swap(int deltaX, int deltaY);
    void SetScreenRect(Vector2Int start, Vector2Int end);
    void Update();
}

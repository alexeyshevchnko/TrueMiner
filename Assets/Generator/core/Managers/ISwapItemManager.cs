using UnityEngine;
using System.Collections;

public interface ISwapItemManager {
    bool AddItem(short id, Vector2 worldPos, bool useNeighbors = true);
    bool AddItem(short id, int offseX, int offsetY, bool useNeighbors = true);

    void SetStartPos(Vector2Int val);
    void SetSize(int x, int y);
    void Swap(int deltaX, int deltaY);

    int RemoveItem(Vector2 worldPos);
    int RemoveItem(Vector2Int offset);
}

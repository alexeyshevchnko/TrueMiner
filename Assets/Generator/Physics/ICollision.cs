using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ICollision {
    List<TileInfoStructure> GetTileInLine(Vector2 startPos, Vector2 endPos);
    List<Vector2Int> GetPosTileInLine(Vector2 startPos, Vector2 endPos);
    BufferItem Raycast(Vector2 position, Vector2 size, Vector2 ray, bool includingAll = false);
    Vector2 ChackVelocity(Vector2 position, Vector2 velocity, float width, float height);
    Vector2 GetTest();
}

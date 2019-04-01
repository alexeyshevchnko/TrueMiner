using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IMapGenerator {
    void StartApplication();
    void ChangeMap();
    TileInfoStructure[,] GetMap();
    TileInfoStructure GetTile(Vector2Int pos);
    List<Vector2Int> Surface { get; }
    Vector2Int CenterPos { get; }
    Action OnGenerate { get; set; }
    Action OnChangeMap { get; set; }
    bool IsGenerated { get; }
    int SizeX { get; }
    int SizeY { get; }

    int GetSurfaceMaxY { get; }
    int GetSurfaceMinY { get; }
    int GetSurfaceMineralsY { get; }
}

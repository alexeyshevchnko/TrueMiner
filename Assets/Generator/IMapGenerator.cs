using System;
using UnityEngine;
using System.Collections.Generic;
using static MapGenerator;

public interface IMapGenerator
{
    void StartApplication();
    void ChangeMap();
    void Generate(ILiquid liquid);

    MapType mapType { get; set; }
    TileInfoStructure[,] GetMap();
    TileInfoStructure GetTile(Vector2Int pos);

    List<Vector2Int> Surface { get; }
    Vector2Int CenterPos { get; }

    Action OnGenerate { get; set; }
    Action OnChangeMap { get; set; }

    bool IsGenerated { get; }
    int SizeX { get; }
    int SizeY { get; }

    bool IsValidPoint(Vector2Int pos, int maxX = -1, int maxY = -1);

    int GetSurfaceMaxY { get; }
    int GetSurfaceMinY { get; }
    int GetSurfaceMineralsY { get; }

    int GetLenghtDown(Vector2Int startPos, short type);

    void RemoveLineUp(Vector2Int startPos, List<short> typeList);
    void RemoveLineDown(Vector2Int startPos, List<short> typeList);

    void RemoveContactsUp(Vector2Int startPos, List<short> typeList);
    void RemoveContactsDown(Vector2Int startPos, List<short> typeList);
}
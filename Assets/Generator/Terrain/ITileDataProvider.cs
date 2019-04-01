using UnityEngine;
using System.Collections;

public interface ITileDataProvider {
    float TileSize { get; }
    void Init(MonoBehaviour monoBehaviour, float tileScale);
    FrameStructure GetFrameStructure(float x, float y, TileInfoStructure tileStructure, LayerData layerData);
    TileInfoStructure GetTileStructure(float x, float y);
    LightInfoStructure GetLightStructure(float x, float y, TileInfoStructure tileStructure);
    void DamageTile(Vector2Int offset, int hp, bool ignoreTree = true);
    void DamageTile(Vector2 worldPos, int hp);
    void СircleDamageTile(float x, float y, short tileType, byte layer, byte liquidType = 0, int radius = 1, int damag = 0);
    void ChangeTile(float x, float y, short tileType, byte layer, byte liquidType = 0);
    void ChangeTile(Vector2Int offset, short tileType, byte layer, byte liquidType = 0);
    void LoadNext();
    void Update();
    void Test();
    Vector2Int WorldPosToOffsetTile(Vector2 wirldPos);
    Vector2 OffsetTileToWorldPos(Vector2Int offset);
}

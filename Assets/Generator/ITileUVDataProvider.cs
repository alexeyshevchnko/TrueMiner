using UnityEngine;
using System.Collections;

public interface IUvProvider : IBaseUVProvider {
    void LoadTiles(Sprite sprite, long shiftPaddingX = 1, long shiftPaddingY = 1, int countHeight = 1);
}

public interface ILightUvProvider : IBaseUVProvider {
    void LoadTiles(Texture2D texture);
    void LateUpdate();
}

public interface IBaseUVProvider {
    TileUVCoordinate GetUV(string spriteName, long flameX, long fameY, int height = 0);
    TileUVCoordinate uvEmpty { get; }
}

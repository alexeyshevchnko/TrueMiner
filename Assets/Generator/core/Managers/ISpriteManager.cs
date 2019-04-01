using UnityEngine;
using System.Collections;

public interface ISpriteManager {
    void SetTileMap(IInfinityTileMap provider);
    Sprite CreateSprite(string tileName, int flameX = 0, int fameY = 0, int deltaHeight = 0);
   // GameObject CreateBlock(string tileCollection);
}

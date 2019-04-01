using UnityEngine;
using System.Collections;

public interface ITileShema {
    FrameStructure GetShema(short[,] map, TileInfoStructure info);
}

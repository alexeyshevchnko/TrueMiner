using UnityEngine;
using System.Collections;

public interface IOverlayLightShema : ITileShema {
    FrameStructure GetShema(short[,] map, TileInfoStructure info, Vector2Int pos);
}

public class OverlayLightShema : IOverlayLightShema {
    public FrameStructure GetShema(short[,] map, TileInfoStructure info) {

        if (info.type == 0) {
            return FrameStructure.Empty();
        }
        //MapGenerator.surface

        FrameStructure rez = new FrameStructure(1, 1);
        rez.TileCollection = "LightTiles";

        
        return rez;
    }

    public FrameStructure GetShema(short[,] map, TileInfoStructure info, Vector2Int pos) {
        /*if (info.type == 0) {
            return FrameStructure.Empty();
        }*/
        //if (info.IsWater()) {
            if (info.IsWater() || pos.y > MapGenerator.surface[pos.x].y) {
                return FrameStructure.Empty();
            }
        //}

        FrameStructure rez = new FrameStructure(1, 1);
        rez.TileCollection = "LightTiles";


        return rez;
    }
}

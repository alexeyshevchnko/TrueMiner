using UnityEngine;
using System.Collections;

public interface ILightShema : ITileShema {
    FrameStructure GetShema(short[,] map, TileInfoStructure info, Vector2 pos);
    FrameStructure GetShemaStructure(TileInfoStructure[,] map, TileInfoStructure info);
    LightInfoStructure GetLightShemaStructure(TileInfoStructure[,] map, TileInfoStructure info);
}

public class LightShema : ILightShema {
    //[IoC.Inject]
    //public ILightRendererOld LightRendererOld { set; protected get; }
    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }

    private TileInfoStructure currentTileInfo = new TileInfoStructure(0, 0);

    bool ChackMap(short[,] mapTemplate, short[,] map, TileInfoStructure info) {
        // 0 пусто
        // 1 тогоже типа
        // -1 не тогоже типа
        // -2 не рассматриваем
        // 2 не пусто

        for (int x = 0; x < mapTemplate.GetLength(0); x++) {
            for (int y = 0; y < mapTemplate.GetLength(1); y++) {

                if (mapTemplate[y, x] == 2) {
                    if (map[x, y] == 0)
                        return false;
                }

                if (mapTemplate[y, x] == 0) {
                    if (map[x, y] != 0)
                        return false;
                }

                if (mapTemplate[y, x] == 1) {
                    if (map[x, y] != info.type)
                        return false;
                }

                if (mapTemplate[y, x] == -1) {
                    if (map[x, y] == info.type)
                        return false;
                }
            }
        }

        return true;
    }

    public FrameStructure GetShema(short[,] map, TileInfoStructure info) {
        var rez = new FrameStructure(0, 0);
     //   rez.TileCollection = LayerData.LightLayer.TexrureName;
        return rez;
    }

    public FrameStructure GetShema(short[,] map, TileInfoStructure info, Vector2 pos) {
        currentTileInfo = info;
        FrameStructure rez;
        short[,] mapTemplate;
        rez = new FrameStructure(0, 0);
      //  rez.TileCollection = LayerData.LightLayer.TexrureName;
      //  Debug.Log(map[1, 1]);
        if (map[1, 1] != 0) {
            rez.FrameX = Settings.LIGHT_SIZE / 2 - 2;
            rez.FrameY = Settings.LIGHT_SIZE / 2 - 2;
        }


        mapTemplate = new short[,] {
                    {-2, 0,-2},
                    {-2,-2,-2},
                    {-2, 1,-2}
        };
        if (ChackMap(mapTemplate, map, info)) {
           // return GetTop();
        }


        return rez;
    }

    public FrameStructure GetShemaStructure(TileInfoStructure[,] map, TileInfoStructure info) {
        //throw new System.NotImplementedException();
        FrameStructure rez;
        /*
        int shift = 0;//200;
        float radius = Settings.LIGHT_SIZE/2f;//6.0f;

        float min = shift - ((radius) - 1);
        float max = shift + ((radius) - 1);
        
        var alpha = Mathf.Lerp(0, Settings.LIGHT_SIZE/2 , map[1, 1].lightAlpha);

       

        var val = (int)Mathf.Clamp((alpha) + shift, min, max);
       // Debug.LogError(val);
        */
       // Debug.LogError(map[1, 1].lightAlpha);
     //   map[1, 1].lightAlpha = 1;
        //var alpha = (int)Mathf.Lerp(0, 255, map[1, 1].lightAlpha);
        var alpha = 255 - map[1, 1].lightAlpha2;
        //if (alpha != 0)
            //Debug.LogError(alpha);

        if (map[1, 1].type == 0 && map[1, 1].typeBG == 0)
            alpha = 0;
        rez = new FrameStructure(alpha, alpha);
        rez.TileCollection = "LightTiles";//LayerData.LightLayer.TexrureName;
        return rez;
    }

    public LightInfoStructure GetLightShemaStructure(TileInfoStructure[,] map, TileInfoStructure info) {
        if (info.lightInfo == null)
            return new LightInfoStructure() {R = 0, G = 0, B = 0, A = 255};
        return info.lightInfo;
    }
} 

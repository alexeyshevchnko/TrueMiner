using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskManager : IMaskManager
{

    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }
    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }


    public int GetMaskLJamp(Vector2 pos)
    {
        var rez = 0;   
        var offset = TileDataProvider.WorldPosToOffsetTile(new Vector2(pos.x,pos.y));

        for (int i = 1; i <= 7; i++)
        {
            bool isEmpty = MapGenerator.GetTile(new Vector2Int(offset.x, offset.y + i)).IsEmpty();
            bool isEmpty2 = MapGenerator.GetTile(new Vector2Int(offset.x, offset.y + i + 1)).IsEmpty();
            bool isEmpty3 = MapGenerator.GetTile(new Vector2Int(offset.x, offset.y + i + 2)).IsEmpty();

         

            if (isEmpty && isEmpty2 && isEmpty3 )
            {
                break;
            }
            else
            {
                rez++;
            }
        }

        if (rez == 7)
            rez = 0;

        if (rez > 0)
        {
            for (int i = 2; i <= rez+3; i++)
            {
                bool isEmpty_1 = MapGenerator.GetTile(new Vector2Int(offset.x + 1, offset.y + i)).IsEmpty();
                bool isEmpty_2 = MapGenerator.GetTile(new Vector2Int(offset.x + 2, offset.y + i)).IsEmpty();
                if (!isEmpty_1 || !isEmpty_2)
                {
                    return 0;
                }
            }
        }

        return rez;
    }

    public int GetMaskRJamp(Vector2 pos)
    {
        var rez = 0;
        var offset = TileDataProvider.WorldPosToOffsetTile(new Vector2(pos.x+1, pos.y));

        for (int i = 1; i <= 7; i++)
        {
            bool isEmpty = MapGenerator.GetTile(new Vector2Int(offset.x, offset.y + i)).IsEmpty();
            
            bool isEmpty2 = MapGenerator.GetTile(new Vector2Int(offset.x, offset.y + i + 1)).IsEmpty();
            bool isEmpty3 = MapGenerator.GetTile(new Vector2Int(offset.x, offset.y + i + 2)).IsEmpty();
            


            if (isEmpty && isEmpty2 && isEmpty3)
            {
                break;
            } else
            {
                rez++;
            }
        }

        if (rez == 7)
            rez = 0;
        
        if (rez > 0)
        {
            for (int i = 2; i <= rez + 3; i++)
            {
                bool isEmpty_1 = MapGenerator.GetTile(new Vector2Int(offset.x - 1, offset.y + i)).IsEmpty();
                bool isEmpty_2 = MapGenerator.GetTile(new Vector2Int(offset.x - 2, offset.y + i)).IsEmpty();
                if (!isEmpty_1 || !isEmpty_2)
                {
                    return 0;
                }
            }
        }
        
        return rez;
    }
}

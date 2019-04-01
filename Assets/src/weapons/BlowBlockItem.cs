using UnityEngine;
using System.Collections;
using trasharia.weapons;

public class BlowBlockItem : BaseWeaponItem {
    
    public string TileCollection { get; protected set; }
    public int Layer;
    public int Type;

    public BlowBlockItem(int layer, short type) {
        //Debug.LogError(tileCollection);
        //Debug.LogError("BlowBlockItem " + tileCollection + " " + layer + " " + type);

        

        Layer = layer;
        Type = type;
        TileCollection = SpriteData.SpritesByTypeId[type].Name;
    }

    public override string GetMainPrefubPath() {
        return "Items/Block";
    }

    public override string GetUiIcon() {
      //  Debug.LogError("Type = " + Type + " Layer = " + Layer + " colect = " + TileCollection);
        
        switch (Type) {
            case   (short) TileTypeEnum.Terrain0:
                return "ground";
            case (short) TileTypeEnum.TerrainGrass:
                return "green";
            case (short) TileTypeEnum.Coal:
                return "coal";
            case (short) TileTypeEnum.Gold:
                return "gold";

            case (short) TileTypeEnum.TreeBody:
            case (short) TileTypeEnum.Wood:
                return "wood";
            
            case (short) TileTypeEnum.Brick:
                return "brick";
            case (short) TileTypeEnum.Bone:
                return "kost";
            case (short) TileTypeEnum.Meat:
                return "meat";
            case (short) TileTypeEnum.Med:
                return "med";
            case (short) TileTypeEnum.Iron:
                return "stal";

            case (short)TileTypeEnum.Stone:
                return "stone";
        }

        return "ground";
    }

    public override bool IsEqualType(IItem other) {
        if (base.IsEqualType(other)) {
            var block = other as BlowBlockItem;

            if (block != null && block.TileCollection == TileCollection && block.Layer == Layer && block.Type == Type) {
                return true;
            }   
        }
        return false;
    }


    public override bool IsCountKit() {
        return true;
    }


    public override int GetId() {
        return Type + 100;

    }

    public override string GetName() {
        switch (Type) {
            case (short)TileTypeEnum.Coal:
                return "Уголь";
                break;

            case (short)TileTypeEnum.Terrain0:
                return "Грязь";
                break;
            case (short)TileTypeEnum.TerrainGrass:
                return "Трава";
                break;

            case (short)TileTypeEnum.Gold:
                return "Золото";
                break;

            case (short)TileTypeEnum.TreeBody:
            case (short)TileTypeEnum.Wood:
                return "Древесина";
                break;

            case (short)TileTypeEnum.Brick:
                return "Кирпич";
            case (short)TileTypeEnum.Bone:
                return "Кость";
            case (short)TileTypeEnum.Meat:
                return "Плоть";
            case (short)TileTypeEnum.Med:
                return "Медь";
            case (short)TileTypeEnum.Iron:
                return "Железная руда";

            case (short)TileTypeEnum.Stone:
                return "Камень";

        }


        return "блок тип = (" + Type.ToString() +")";
    }
}

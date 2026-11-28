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
        // Иконка блока берётся из СОБСТВЕННОГО тайлсета блока: префикс "tile:" говорит
        // интерфейсу резолвить спрайт через BlockIcons.GetSprite(TileCollection),
        // а не через атлас gui.png. Представляющий кадр выбирает схема тайлсета.
        return "tile:" + TileCollection;
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

            case (short)TileTypeEnum.Terrain0:
                return "Грязь";

            case (short)TileTypeEnum.TerrainGrass:
                return "Трава";

            case (short)TileTypeEnum.Gold:
                return "Золото";

            case (short)TileTypeEnum.TreeBody:
            case (short)TileTypeEnum.Wood:
                return "Древесина";

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

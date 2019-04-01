using System.Collections.Generic;

namespace trasharia.weapons {
    public interface IItem {
        int Count { get; set; }
        string GetUiIcon();
        string GetMainPrefubPath();
        void Use(IPlayerModel model);

        bool IsEqualType(IItem other);
        bool IsCountKit();
        int GetId();

        string GetName();
    }
}
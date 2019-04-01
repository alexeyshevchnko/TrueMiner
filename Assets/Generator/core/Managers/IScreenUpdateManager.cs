using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;

namespace Managers {
    public interface IScreenUpdateManager {
        void Init(int sixeX, int sizeY);
        void RedrawWorldTile(int layer, int worldOffsetX, int worldOffsetY);
        void RedrawWorldTile3X(int layer, int worldOffsetX, int worldOffsetY);
        void EndDrawScreenTile(int layer, int screenOffsetX, int screenOffsetY);
        bool IsUpdateOffset(int x, int y);
        bool IsUpdate(int layerId, int x, int y);

        void SetScreenRect(Vector2Int start, Vector2Int end);

        void SetApplayChanges(bool b);
        bool GetApplayChanges();

        bool GetChangeLight();
        void SetChangeLight(bool b);
        void EndUpdateLight();
        Vector2Int GetLightUptateWorldMin();
        Vector2Int GetLightUptateWorldMax();

        Vector2Int GetLightUptateScreenMin();
        Vector2Int GetLightUptateScreenMax();
        void RedrawFullScreenLight();

        void EndDrawObstacleLightTile(int screenOffsetX, int screenOffsetY);
        bool IsUpdateOffsetObstacleLight(int x, int y);
        void SetChangeObstacleLight(bool b);
        bool GetChangeObstacleLight();

        bool TryAddUpdateLightPoint(int worldOffsetX, int worldOffsetY);

    }
}


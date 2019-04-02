using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shell : MonoBehaviourPhysicItem {
    private TileInfoStructure[,] Map {
        get {
            return MapGenerator.GetMap();
        }
    }

    protected virtual void FixedUpdate() {
        if (Container == null)
            return;

       // Debug.LogError(pysicItem.velocity.magnitude);
        if (pysicItem.velocity.magnitude > 6) {
            var tiles = Collision.Raycast(pysicItem.Position, size, pysicItem.velocity, true);
            for (int i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                if (Map[tile.x, tile.y].IsDecor())
                {
                    TileDataProvider.ChangeTile(tile, 0, 0);
                }
            }
        } else {
            //pysicItem.AddVelocity(-pysicItem.velocity);
        }

        base.FixedUpdate();
    }
}

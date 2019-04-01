using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ITargetManager {
    PlayerController PlayerController { get;}
    void Init();
    void CreateOtherPlayer(int id, Vector3 worldPos);
    PlayerController GetPlayer(int id);
    AiController CreateUnit(int id, Vector2Int pos);
    AiController CreateMinerUnit(int id, Vector2Int pos);

    void DestroyUnit(AiController unit);
    void ExplosionDamage(Vector2 center, float radius, float hp);
    List<ITargetBase> GetUnitsInRange(Vector2 center, float radius);
}

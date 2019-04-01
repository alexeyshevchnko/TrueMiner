using System;
using UnityEngine;
using System.Collections;

public interface IPhysicItem {
    Action<IPhysicItem> OnCollision { get; set; }
    Action OnTerrainCollision { get; set; }
    Vector2 velocity { get; }
    bool Sleep { get; }
    float itemWidth { get; set; }
    float itemHeight { get; set; }
    Vector2 Size { get; }
    Vector2Int[] PhysicsSectors { get; set; }
    MonoBehaviour View { get; }

    void AddMomentum(Vector2 val);
    void AddVelocity(Vector2 val);
    void SetVelocity(Vector2 val);
    float ReboundFactor { get; set; }
    void SetPosition(Vector2 pos);
    Vector2 GetPosition();
    Vector2 GetTopLeftPos();
    Vector2 GetBottomRightPos();
    Vector2 GetTopRightPos();
    Vector2 GetBottomLeftPos();
    Vector2 GetBottomPos();

    Rect GetRect();
    void SetSleep(bool val);

    bool IsCollisionTerrain { get; set; }
    bool IsReactExplosion { get; set; }

    void Update();
    void Destroy();
    void Register();
    void UnRegister();

    void SetMomentVector(Vector2 val);
    void SetGravity(Vector2 val);

    byte[] GetSyncData();
    void Sync(byte[] data);

    void ApplayVelocity(Vector2 velocity);
}

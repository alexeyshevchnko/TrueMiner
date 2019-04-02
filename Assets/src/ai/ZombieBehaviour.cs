﻿using System.Collections;
using System.Collections.Generic;
using trasharia;
using UnityEngine;

public class ZombieBehaviour : IAIBehaviour {
    private float direcr = 1;
    private float speed = 0.1f;
    private float lastJampPosX;
    private int attemptJampCount = 4;
    private float dirJamp = 15;


    private int countTryJamp = 1;
    [IoC.Inject]
    public ICollision Collision { set; protected get; }
    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }
    [IoC.Inject]
    public ITileDataProvider TileDataProvider { set; protected get; }

    [IoC.Inject]
    public ITargetManager TargetManager { set; protected get; }

    [IoC.Inject]
    public ICooldownManager CooldownManager { set; protected get; }

    [IoC.Inject]
    public IRandom Random { set; protected get; }

    public IPhysicItem pysicItem { get; protected set; }

    private ICooldownItem cooldownItem;

    private TileInfoStructure[,] Map {
        get {
            return MapGenerator.GetMap();
        }
    }

    ~ZombieBehaviour() {
        if (pysicItem != null) {
            pysicItem.Destroy();
        }

        if (CooldownManager != null && cooldownItem!= null) {
            CooldownManager.RemoveCooldown(cooldownItem);
        }
    }

    public void Init(IPhysicItem pysicItem, IView view) {
        this.pysicItem = pysicItem;
        UpdateDirect();
    }

    void UpdateDirect() {
        direcr = Mathf.Sign(TargetManager.PlayerController.transform.position.x - pysicItem.Position.x);
        

        cooldownItem = CooldownManager.AddCooldown(1, null, UpdateDirect);
    }
         

    public void Update() {
      
        var dir = new Vector2(speed * direcr, 0);
        pysicItem.AddVelocity(dir);
        var pos = pysicItem.Position;

        var tiles = Collision.Raycast(pos, pysicItem.Size, dir, true);

        //прыжок только нсли есть земля
        var dirDown = new Vector2(0, -1.1f);
        var tilesDown = Collision.Raycast(pos, pysicItem.Size, dirDown, false);
        if (tilesDown.Count > 0) {
            
            bool tryJamp = false;
            for (int i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                if (!Map[tile.x, tile.y].IsEmpty())
                {
                    tryJamp = true;
                    break;
                }
            }

            var t = Random.Range(0, 100);
            if (t < 5) {
                tryJamp = true;
                countTryJamp = Random.Range(1, 7);
            }

            if (tryJamp) {
                if (lastJampPosX != pos.x) {
                    countTryJamp = 1;
                } else {
                    countTryJamp++;
                    if (countTryJamp > attemptJampCount) {
                        countTryJamp = 1;
                       // direcr *= -1;
                    }
                }

                lastJampPosX = pos.x;
                Jamp();

            }
        }
    }

    public void Jamp() {
        pysicItem.AddVelocity(new Vector2(2 * countTryJamp * direcr, dirJamp * countTryJamp));
    }

    public int GetDirection() {
        return (int)direcr;
    }
}

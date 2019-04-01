﻿using System.Collections;
using System.Collections.Generic;
using trasharia.weapons;
using UnityEngine;

public class Axe : MonoBehaviour, IWeapon {

    public Transform view;
    public Transform firePoint;
    public float cooldown;
    public int range;

    [IoC.Inject]
    public ICooldownManager CooldownManager { set; protected get; }
    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }

    [IoC.Inject]
    public ITileDataProvider tileDataProvider { set; protected get; }

    [IoC.Inject]
    public ISwapItemManager SwapItemManager { set; protected get; }

    [IoC.Inject]
    public ICollision Collision { set; protected get; }

    private TileInfoStructure[,] Map {
        get {
            return MapGenerator.GetMap();
        }
    }

    private DamagePointBase damagePoint;
    private IItem itemData;
    void Start() {
        this.Inject();
        damagePoint = firePoint.gameObject.GetComponent<DamagePointBase>();
        if (damagePoint == null) {
            damagePoint = firePoint.gameObject.AddComponent<DamagePointBase>();
        }
        damagePoint.enabled = false;
    }

    public void Init(ref IItem data, PlayerModel playerModel) {
        itemData = data;
    }

    public IItem GetItem() {
        return itemData;
    }

    private bool isFireing = false;
    public void Fire(Vector2 direct) {
        if (isFireing)
            return;
        isFireing = true;
        var tween = TweenRotation.Begin(gameObject, cooldown, Quaternion.identity);
        tween.from = new Vector3(0, 0, 90);
        tween.to = new Vector3(0, 0, -90);
        tween.SetOnFinished(() => {
            OnFinishedFire();
        });

        damagePoint.enabled = true;
    }

    public void FireCycle(Vector2 direct) {
        Fire(direct);
    }

    void OnFinishedFire() {
        isFireing = false;
        transform.localRotation = new Quaternion();
        damagePoint.enabled = false;

        var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        var ray = pos - firePoint.position;
        List<Vector2Int> tilesDown = Collision.Raycast(firePoint.position, new Vector2(1, 1), ray, true);

        // Debug.LogError(tilesDown.Count);





        if (tilesDown.Count > 0) {
            // ищем ближайшею
            int index = 0;
            float minDist = float.MaxValue;
            for (int i = 0; i < tilesDown.Count; i++) {
                //Debug.LogError(Map[tilesDown[i].x, tilesDown[i].y].IsTree());
                if (Map[tilesDown[i].x, tilesDown[i].y].IsTree()) {

                    //if(tilesDown[i].)
                    var newDist =
                        Vector3.Distance(firePoint.position, tileDataProvider.OffsetTileToWorldPos(tilesDown[i]))/
                        tileDataProvider.TileSize;
                    if (newDist < minDist) {
                        minDist = newDist;
                        index = i;
                       
                    }
                }
            }


            if (minDist <= range) {
                tileDataProvider.DamageTile(tilesDown[index], 7, false);

                var removeItemId = SwapItemManager.RemoveItem(pos);
                if (removeItemId != 0) {
                    //сделать предмет
                }
            }

        }
    }

    public void Rotate(Vector2 target) {

    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CooldownManager : ICooldownManager, IoC.IInitialize {

    private int count = 0;

    private readonly Dictionary<int, ICooldownItem> items = new Dictionary<int, ICooldownItem>();
    private readonly Dictionary<int, float> lastTickTimes = new Dictionary<int, float>();

    private float expectedSyncTime = 0;
    private bool isSync = false;
    private readonly DateTime startTime = DateTime.Now;

    public int GetUniqueId() {
        count++;
        return count + 1;
    }

    public ICooldownItem AddInfinityCooldown(Action onTick, float tickInterval = 1) {
        var infinityCooldownItem = new InfinityCooldownItem(GetUniqueId(), 0, onTick, null);
        AddCooldown(infinityCooldownItem);
        return infinityCooldownItem;
    }


    float GetTime() {
        float rez = isSync ? expectedSyncTime : ((float)(DateTime.Now - startTime).TotalSeconds);
        return rez;
    }

    private void AddCooldown(ICooldownItem cooldown) {
        items.Add(cooldown.Id, cooldown);
        lastTickTimes[cooldown.Id] = GetTime();
    }

    public ICooldownItem AddCooldown(float duration, Action onTick, Action onEnd, float elapsedTime = 0, float tickInterval = 1) {
        ICooldownItem item = new CooldownItem(GetUniqueId(), duration, onTick, onEnd, elapsedTime, tickInterval);
        AddCooldown(item);
        return item;
    }


    public void RemoveCooldown(ICooldownItem item) {
        if (item == null)
            return;

        items.Remove(item.Id);
        lastTickTimes.Remove(item.Id);
    }

    public ICooldownItem GetById(int id) {
        if (items.ContainsKey(id))
            return items[id];
        return null;
    }

    public void Update() {
        UpdateOfTime(GetTime());
    }


    public void UpdateOfTime(float currentTime) {
        var listKeys = items.Keys.ToList();
        for (int i = listKeys.Count - 1; i >= 0; i--) {
            int id = listKeys[i];
            float lastTickTime = lastTickTimes[id];
            CooldownItem cooldown = (CooldownItem)items[id];

            float deltaTime = currentTime - lastTickTime;

            if (deltaTime >= cooldown.TickInterval) {
                cooldown.Tick(deltaTime);
                lastTickTimes[id] = currentTime;

                if (cooldown.ChackEnd()) {
                    RemoveCooldown(cooldown);
                }
            }
        }
    }

    public void Sync(DateTime stopTime) {
        isSync = true;
        //1просчитаем кол пропущенных фиксед апдейтов
        var delta = (float)(DateTime.Now - stopTime).TotalSeconds;
        var stopTimeSecond = (float)(stopTime - startTime).TotalSeconds;
        int countLostFrame = Mathf.FloorToInt(delta / Time.fixedDeltaTime);

        for (int i = 0; i < countLostFrame; i++) {
            expectedSyncTime = stopTimeSecond + i * Time.fixedDeltaTime;

            UpdateOfTime(expectedSyncTime);
        }

        isSync = false;
    }



    public void Clear() {
        items.Clear();
        lastTickTimes.Clear();
    }

    public void OnInject() {
    }

    private void OnBeforePlanetLoaded() {
        Clear();
    }
}
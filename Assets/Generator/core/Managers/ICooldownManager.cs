using System;

public interface ICooldownManager {
    int GetUniqueId();
    ICooldownItem AddInfinityCooldown(Action onTick, float tickInterval = 1);
    ICooldownItem AddCooldown(float duration, Action onTick, Action onEnd, float elapsedTime = 0, float tickInterval = 1);
    ICooldownItem GetById(int id);
    void RemoveCooldown(ICooldownItem item);
    void Clear();
}
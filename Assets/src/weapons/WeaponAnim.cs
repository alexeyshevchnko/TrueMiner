using UnityEngine;
using System.Collections;

/// <summary>
/// Без-NGUI замена для TweenRotation "взмаха" оружия/инструмента.
/// Плавно поворачивает объект по Z от 90 к -90 и возвращает в исходное.
/// </summary>
public static class WeaponAnim
{
    public static IEnumerator Swing(Transform t, float duration, System.Action onFinished)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / Mathf.Max(duration, 0.001f));
            float z = Mathf.Lerp(90f, -90f, k);
            t.localRotation = Quaternion.Euler(0f, 0f, z);
            yield return null;
        }
        t.localRotation = Quaternion.identity;
        if (onFinished != null) onFinished();
    }
}
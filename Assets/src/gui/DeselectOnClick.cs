using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Сбрасывает выбор EventSystem после Pointer Click по данной кнопке.
/// Без этого StandaloneInputModule оставляет нажатую мышью/тачем кнопку выбранной
/// (currentSelectedGameObject), и пробел (Submit) продолжает её активировать.
/// Влияет только на элемент, выбранный этим же указателем: клавиатурный выбор,
/// переходы стрелками/Tab и Submit на прочих элементах не затрагиваются.
/// </summary>
public class DeselectOnClick : MonoBehaviour, IPointerClickHandler
{
    private EventSystem cachedEventSystem;

    public void OnPointerClick(PointerEventData eventData)
    {
        var eventSystem = cachedEventSystem;
        if (eventSystem == null)
        {
            // Единственный EventSystem игры; FindFirstObjectByType вместо статического
            // EventSystem.current — надёжнее (current не всегда заполнен до первого Update).
            eventSystem = cachedEventSystem = FindFirstObjectByType<EventSystem>();
        }

        if (eventSystem != null && eventSystem.currentSelectedGameObject == gameObject)
        {
            eventSystem.SetSelectedGameObject(null);
        }
    }
}

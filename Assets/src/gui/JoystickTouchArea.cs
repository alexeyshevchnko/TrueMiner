using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Зона TouchArea (половина экрана) для мобильного управления (GUI_Agent_Work/ISSUE-9).
/// Стоит на канвасе ниже всех игровых канвасов: элементы интерфейса выигрывают
/// raycast, а зона получает касания только в свободных областях экрана.
/// Полученные события пересылаются в целевой Ultimate Joystick с тем же
/// PointerEventData (сохраняется fingerId) — multi-touch и независимость
/// джойстиков обеспечивает сам UltimateJoystick (_pointerId).
/// </summary>
public class JoystickTouchArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private UltimateJoystick target;

    public UltimateJoystick Target
    {
        get { return target; }
        set { target = value; }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (target != null)
            ExecuteEvents.Execute(target.gameObject, eventData, ExecuteEvents.pointerDownHandler);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (target != null)
            ExecuteEvents.Execute(target.gameObject, eventData, ExecuteEvents.dragHandler);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (target != null)
            ExecuteEvents.Execute(target.gameObject, eventData, ExecuteEvents.pointerUpHandler);
    }
}

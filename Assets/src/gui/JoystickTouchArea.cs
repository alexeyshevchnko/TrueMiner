using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Зона TouchArea для мобильного управления (GUI_Agent_Work/ISSUE-9/10).
/// Стоит на канвасе ниже всех игровых канвасов: элементы интерфейса выигрывают
/// raycast, а зона получает касания только в свободных областях экрана.
/// Полученные события пересылаются в целевой Ultimate Joystick с тем же
/// PointerEventData (сохраняется fingerId) — multi-touch и независимость
/// джойстиков обеспечивает сам UltimateJoystick (_pointerId).
///
/// Геометрия: ровно половина ширины экрана (левая 0–50% / правая 50–100%)
/// и нижняя часть экрана высотой TouchAreaHeightPercent (Inspector,
/// Range 10–100, по умолчанию 50 — нижняя половина). Якоря пересчитываются
/// каждый кадр из параметра, поэтому изменение значения в Inspector сразу
/// меняет размер зоны; разрешение/соотношение сторон учитываются якорями.
/// </summary>
public class JoystickTouchArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField, Range(10f, 100f)]
    private float touchAreaHeightPercent = 50f;

    private UltimateJoystick target;
    private RectTransform rect;

    /// <summary>Высота зоны в процентах высоты экрана (10–100).</summary>
    public float TouchAreaHeightPercent
    {
        get { return touchAreaHeightPercent; }
        set
        {
            touchAreaHeightPercent = Mathf.Clamp(value, 10f, 100f);
            ApplyLayout();
        }
    }

    public UltimateJoystick Target
    {
        get { return target; }
        set { target = value; }
    }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        ApplyLayout();
    }

    private void Update()
    {
        // Пересчёт на случай изменения параметра из Inspector в Play Mode.
        ApplyLayout();
    }

    /// <summary>
    /// Половина ширины экрана (сторона задаётся горизонтальными якорями,
    /// настроенными при создании зоны), нижняя часть экрана высотой
    /// touchAreaHeightPercent. Отступы нулевые — без зазора между зонами.
    /// </summary>
    private void ApplyLayout()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();

        float height = Mathf.Clamp(touchAreaHeightPercent, 10f, 100f) / 100f;
        rect.anchorMin = new Vector2(rect.anchorMin.x, 0f);
        rect.anchorMax = new Vector2(rect.anchorMax.x, height);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
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

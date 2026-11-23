using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Зона TouchArea для мобильного управления (Agent_Works/GUI_Agent_Work/ISSUE-9/10).
/// Стоит на канвасе ниже всех игровых канвасов: элементы интерфейса выигрывают
/// raycast, а зона получает касания только в свободных областях экрана.
/// Полученные события пересылаются в целевой Ultimate Joystick с тем же
/// PointerEventData (сохраняется fingerId) — multi-touch и независимость
/// джойстиков обеспечивает сам UltimateJoystick (_pointerId).
///
/// Геометрия: ширина экрана задаётся константой
/// TouchAreaRaycaster.TOUCH_AREA_WIDTH_PERCENT симметрично от краёв
/// (по умолчанию 50 — левая 0–50% / правая 50–100%) и нижняя часть экрана
/// высотой TouchAreaHeightPercent (Inspector, Range 10–100; значение по
/// умолчанию — константа TOUCH_AREA_HEIGHT_PERCENT, 50 — нижняя половина).
/// Якоря пересчитываются
/// каждый кадр из параметра, поэтому изменение значения в Inspector сразу
/// меняет размер зоны; разрешение/соотношение сторон учитываются якорями.
/// </summary>
public class JoystickTouchArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerEnterHandler
{
    // Значение по умолчанию — из константы TOUCH_AREA_HEIGHT_PERCENT
    // (TouchAreaRaycaster): единая точка настройки размеров зон TouchArea.
    // В Inspector зону можно переопределить отдельно, не меняя константу.
    [SerializeField, Range(10f, 100f)]
    private float touchAreaHeightPercent = TouchAreaRaycaster.TOUCH_AREA_HEIGHT_PERCENT;

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Мышь над зоной — это мировая область, а не интерактивный UI: сбрасываем
        // pointerEnter, чтобы EventSystem.IsPointerOverGameObject(-1) возвращал false
        // и прицеливание мышью (KeyboardJoystick) работало в свободных областях экрана.
        // Над кнопками/сумкой pointerEnter остаётся на их графике — играется как UI.
        if (target != null && eventData.pointerId < 0)
            eventData.pointerEnter = null;
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

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Жизненный цикл видимости Ultimate Joystick (см. GUI_Agent_Work/ISSUE-5):
/// - визуальная часть (дочерний объект JoystickSizeFolder) скрыта, пока джойстик не удерживается,
///   и появляется на время касания (dynamicPositioning переносит визуал под палец);
/// - невидимая зона касания (Image с alpha=0) позволяет начать касание в зоне джойстика
///   даже при скрытом визуале — область не привязана к видимой картинке;
/// - пока открыта сумка, зона касания отключает raycastTarget: новые касания не
///   активируют джойстики, а уже удерживаемый палец продолжает управлять (пойнтер
///   остаётся захвачен StandaloneInputModule до отпускания).
/// Multi-touch: UltimateJoystick хранит pointerId коснувшегося пальца (_pointerId) и
/// игнорирует чужие pointerId в OnDrag/OnPointerUp — два джойстика работают одновременно
/// и независимо; этот компонент оперирует только своим джойстиком.
/// Механика управления (радиус, оси, чувствительность) не изменяется.
/// </summary>
public class JoystickTouchVisibility : MonoBehaviour
{
    private const string VisualFolderName = "JoystickSizeFolder";
    private const float TouchAreaSize = 160f; // меньше, чтобы не перекрывать ячейки хотбара/сумки в углах

    private Image touchArea;
    private Transform visuals;
    private string joystickName;

    private void Awake()
    {
        var joystick = GetComponent<UltimateJoystick>();
        joystickName = joystick != null ? joystick.joystickName : name;

        visuals = transform.Find(VisualFolderName);

        // Невидимая зона касания по центру корневого прямоугольника джойстика.
        var area = new GameObject("TouchArea", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        area.transform.SetParent(transform, false);
        var areaRect = (RectTransform)area.transform;
        areaRect.anchorMin = areaRect.anchorMax = areaRect.pivot = new Vector2(0.5f, 0.5f);
        areaRect.sizeDelta = new Vector2(TouchAreaSize, TouchAreaSize);
        areaRect.anchoredPosition = Vector2.zero;
        touchArea = area.GetComponent<Image>();
        touchArea.color = new Color(0f, 0f, 0f, 0f);
        touchArea.raycastTarget = true;

        // До первого Update джойстик уже скрыт (без кадра «мигания» при старте).
        if (visuals != null && !UltimateJoystick.GetJoystickState(joystickName))
            visuals.gameObject.SetActive(false);
    }

    private void Update()
    {
        var touched = UltimateJoystick.GetJoystickState(joystickName);

        // Появление в момент начала касания и скрытие после отпускания.
        if (visuals != null && visuals.gameObject.activeSelf != touched)
            visuals.gameObject.SetActive(touched);

        // Открытая сумка: зона джойстика не должна перехватывать касания её ячеек.
        var bagOpen = RootContext.Instance != null && RootContext.Instance.Player != null &&
                      RootContext.Instance.Player.IsShowBag;
        var shouldRaycast = !bagOpen;
        if (touchArea.raycastTarget != shouldRaycast)
            touchArea.raycastTarget = shouldRaycast;
    }
}

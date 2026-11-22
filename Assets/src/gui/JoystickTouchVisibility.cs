using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Жизненный цикл видимости Ultimate Joystick (GUI_Agent_Work/ISSUE-8/10):
/// визуальная часть (дочерний JoystickSizeFolder) скрыта, пока джойстик не
/// удерживается, и появляется на время касания (dynamicPositioning переносит
/// визуал в точку касания).
/// Источник касаний — полупроекранные зоны TouchArea (JoystickTouchArea +
/// TouchAreaRaycaster): собственная графика джойстика отключена от raycast
/// (raycastTarget = false), поэтому его прямоугольник больше не является
/// областью активации — касание обрабатывается ровно один раз, зоной.
/// Multi-touch: UltimateJoystick хранит pointerId пальца (_pointerId) и
/// игнорирует чужие события — джойстики независимы.
/// Механика управления (радиус, оси, чувствительность) не изменяется.
/// </summary>
public class JoystickTouchVisibility : MonoBehaviour
{
    private const string VisualFolderName = "JoystickSizeFolder";

    private Transform visuals;
    private string joystickName;

    private void Awake()
    {
        var joystick = GetComponent<UltimateJoystick>();
        joystickName = joystick != null ? joystick.joystickName : name;

        visuals = transform.Find(VisualFolderName);

        // Активация джойстика — только через TouchArea: своя графика (Base/Handle/
        // Highlight/Tension) не участвует в raycast, иначе одно касание обрабатывалось
        // бы дважды (зоной и прямоугольником джойстика).
        foreach (var graphic in GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = false;

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
    }
}

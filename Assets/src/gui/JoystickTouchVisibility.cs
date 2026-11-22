using UnityEngine;

/// <summary>
/// Жизненный цикл видимости Ultimate Joystick (GUI_Agent_Work/ISSUE-8):
/// визуальная часть (дочерний JoystickSizeFolder) скрыта, пока джойстик не
/// удерживается, и появляется на время касания (dynamicPositioning переносит
/// визуал в точку касания). Область касания — отдельные полупроекранные зоны
/// TouchArea (см. JoystickTouchArea/TouchAreaRaycaster), поэтому скрытие
/// визуала не влияет на ввод.
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

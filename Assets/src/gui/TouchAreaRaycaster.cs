using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Лучепроектировщик для канваса TouchArea (см. Agent_Works/GUI_Agent_Work/ISSUE-7..9).
/// Отвечает за маршрутизацию касаний:
/// - зоны TouchArea находятся на канвасе с sortingOrder ниже всех игровых канвасов,
///   поэтому любые элементы интерфейса (сумка, ячейки, хотбар, кнопки) всегда
///   выигрывают raycast и TouchArea не блокирует их;
/// - мышь (pointerId -1) игнорируется: на десктопе мышь остаётся прицеливанием/огнём
///   (KeyboardJoystick, Input.GetMouseButton), а зоны — только для сенсорных пальцев;
/// - сенсорный палец, попавший в свободную (не занятую интерфейсом) область своей
///   половины экрана, получает событие соответствующей зоны.
/// Канвас работает вместо GraphicRaycaster: Image зон прозрачны, попадание
/// определяется по прямоугольникам половин экрана.
/// </summary>
public class TouchAreaRaycaster : BaseRaycaster
{
    // Размеры зон TouchArea в процентах экрана — единая точка настройки.
    // Значения читаются при создании зон (GameUi.AttachJoystickTouchVisibility —
    // ширина) и в JoystickTouchArea (высота зон). Изменяются вручную здесь,
    // поиск по коду не требуется.
    // - Ширина: зоны симметричны от краёв экрана; при значении 50 левая зона
    //   занимает 0–50% ширины, правая — 50–100% (разделение ровно посередине).
    // - Высота: нижняя часть экрана высотой TOUCH_AREA_HEIGHT_PERCENT от низа
    //   (значение по умолчанию в JoystickTouchArea; в Play Mode его можно
    //   переопределить в Inspector каждой зоны без изменения константы).
    public const float TOUCH_AREA_WIDTH_PERCENT = 50f;
    public const float TOUCH_AREA_HEIGHT_PERCENT = 50f;

    private Canvas canvas;
    private Canvas Canvas
    {
        get
        {
            if (canvas == null)
                canvas = GetComponent<Canvas>();
            return canvas;
        }
    }

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        if (eventData == null)
            return;

        // pointerId -1 — мышь: в билдах зоны не реагируют на мышь (десктопное
        // прицеливание/огонь мышью, см. KeyboardJoystick), в редакторе мышь
        // активирует зоны, чтобы можно было тестировать их без сенсорного устройства.
        if (eventData.pointerId < 0 && !Application.isEditor)
            return;

        var screenPoint = eventData.position;
        var canvasTransform = (RectTransform)Canvas.transform;
        for (var i = 0; i < canvasTransform.childCount; i++)
        {
            var areaRect = canvasTransform.GetChild(i) as RectTransform;
            if (areaRect == null || !areaRect.gameObject.activeInHierarchy)
                continue;

            if (!RectTransformUtility.RectangleContainsScreenPoint(areaRect, screenPoint, eventCamera))
                continue;

            resultAppendList.Add(new RaycastResult
            {
                gameObject = areaRect.gameObject,
                module = this,
                index = i,
                depth = 0,
                sortingLayer = Canvas.sortingLayerID,
                sortingOrder = Canvas.sortingOrder
            });
        }
    }

    public override Camera eventCamera
    {
        get { return null; } // Screen Space Overlay
    }

    public override int sortOrderPriority
    {
        get { return Canvas != null ? Canvas.sortingOrder : int.MinValue; }
    }

    public override int renderOrderPriority
    {
        get { return int.MinValue; }
    }
}

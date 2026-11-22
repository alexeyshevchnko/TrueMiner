using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Лучепроектировщик для канваса TouchArea (см. GUI_Agent_Work/ISSUE-7..9).
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

        // pointerId -1 — мышь. Зоны джойстиков реагируют только на пальцы (>= 0).
        if (eventData.pointerId < 0)
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

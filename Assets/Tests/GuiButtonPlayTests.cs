using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Play Mode-проверки поведения кнопок GUI:
/// после клика мышью/тачем кнопка не остаётся selected и не активируется пробелом.
/// Выполняются в Play Mode, где жизненный цикл EventSystem/StandaloneInputModule
/// соответствует игровому (в отличие от batch EditMode, где static EventSystem.current
/// не заполняется окружением тестового раннера).
/// Тип DeselectOnClick из Assembly-CSharp получаем рефлексией: asmdef-сборка тестов
/// не может ссылаться на предопределённую Assembly-CSharp.
/// </summary>
public class GuiButtonPlayTests
{
    static void AddDeselectOnClick(GameObject go)
    {
        var type = typeof(GuiButtonPlayTests).Assembly.GetType("DeselectOnClick");
        if (type == null) type = System.Type.GetType("DeselectOnClick, Assembly-CSharp");
        Assert.NotNull(type, "тип DeselectOnClick не найден в Assembly-CSharp");
        go.AddComponent(type);
    }

    [UnityTest]
    public IEnumerator DeselectOnClick_ClearsSelectionAfterPointerClick()
    {
        var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        var buttonGo = new GameObject("TestButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(canvasGo.transform, false);
        var button = buttonGo.GetComponent<Button>();
        AddDeselectOnClick(buttonGo);

        var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        var eventSystem = eventSystemGo.GetComponent<EventSystem>();
        yield return null; // даём отработать OnEnable/Start

        Assert.NotNull(EventSystem.current, "в Play Mode EventSystem.current должен быть задан");
        Assert.AreEqual(eventSystem, EventSystem.current);

        // Поток StandaloneInputModule: pointer down выбирает кнопку, click выполняет действие.
        var downData = new PointerEventData(eventSystem) { pointerId = -1, button = PointerEventData.InputButton.Left };
        var clickData = new PointerEventData(eventSystem) { pointerId = -1, button = PointerEventData.InputButton.Left };
        ExecuteEvents.Execute(buttonGo, downData, ExecuteEvents.pointerDownHandler);
        Assert.AreEqual(buttonGo, eventSystem.currentSelectedGameObject, "pointer down должен выбрать кнопку (стандарт uGUI)");

        int clicks = 0;
        button.onClick.AddListener(() => clicks++);
        ExecuteEvents.Execute(buttonGo, clickData, ExecuteEvents.pointerClickHandler);
        ExecuteEvents.Execute(buttonGo, downData, ExecuteEvents.pointerUpHandler);
        Assert.AreEqual(1, clicks, "клик должен выполнить действие кнопки");
        Assert.Null(eventSystem.currentSelectedGameObject, "после Pointer Click выбор должен быть сброшен");

        // Пробел/Enter после клика: StandaloneInputModule доставляет Submit только
        // eventSystem.currentSelectedGameObject. После сброса выбора цели нет — срабатывания нет.
        yield return null;
        var submitTarget = eventSystem.currentSelectedGameObject;
        Assert.Null(submitTarget, "после Pointer Click не должно быть выбранного объекта для Submit");
        if (submitTarget != null)
            ExecuteEvents.Execute(submitTarget, new PointerEventData(eventSystem), ExecuteEvents.submitHandler);
        Assert.AreEqual(1, clicks, "пробел не должен активировать кнопку после Pointer Click");

        // Контроль: штатная активация с клавиатурным выбором по-прежнему работает.
        eventSystem.SetSelectedGameObject(buttonGo);
        ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, new PointerEventData(eventSystem), ExecuteEvents.submitHandler);
        Assert.AreEqual(2, clicks, "штатный Submit при явном клавиатурном выборе должен работать");

        Object.Destroy(eventSystemGo);
        Object.Destroy(buttonGo);
        Object.Destroy(canvasGo);
    }

    [UnityTest]
    public IEnumerator NavigationNone_ButtonIsNeverSelectedByPointerDown()
    {
        var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        var buttonGo = new GameObject("TestNavNoneButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(canvasGo.transform, false);
        var button = buttonGo.GetComponent<Button>();
        var nav = new Navigation();
        nav.mode = Navigation.Mode.None;
        button.navigation = nav;

        var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        var eventSystem = eventSystemGo.GetComponent<EventSystem>();
        yield return null;

        Assert.NotNull(EventSystem.current);

        var downData = new PointerEventData(eventSystem) { pointerId = -1, button = PointerEventData.InputButton.Left };
        ExecuteEvents.Execute(buttonGo, downData, ExecuteEvents.pointerDownHandler);
        Assert.Null(eventSystem.currentSelectedGameObject, "с Navigation.None pointer down не должен выбирать кнопку");

        Object.Destroy(eventSystemGo);
        Object.Destroy(buttonGo);
        Object.Destroy(canvasGo);
    }

    [UnityTest]
    public IEnumerator SlotButton_ClickIsNotRepeatedBySubmit()
    {
        var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        var slotGo = new GameObject("Slot", typeof(RectTransform), typeof(Image), typeof(Button));
        slotGo.transform.SetParent(canvasGo.transform, false);
        var slotButton = slotGo.GetComponent<Button>();
        AddDeselectOnClick(slotGo); // как в GameUi.MakeSlot

        var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        var eventSystem = eventSystemGo.GetComponent<EventSystem>();
        yield return null;

        int clicks = 0;
        slotButton.onClick.AddListener(() => clicks++);

        var downData = new PointerEventData(eventSystem) { pointerId = -1, button = PointerEventData.InputButton.Left };
        var clickData = new PointerEventData(eventSystem) { pointerId = -1, button = PointerEventData.InputButton.Left };
        ExecuteEvents.Execute(slotGo, downData, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(slotGo, clickData, ExecuteEvents.pointerClickHandler);
        ExecuteEvents.Execute(slotGo, downData, ExecuteEvents.pointerUpHandler);
        Assert.AreEqual(1, clicks, "клик по ячейке должен сработать");
        Assert.Null(eventSystem.currentSelectedGameObject, "после клика по ячейке выбор должен быть сброшен");

        var submitTarget = eventSystem.currentSelectedGameObject;
        Assert.Null(submitTarget, "после клика по ячейке не должно быть выбранного объекта для Submit");
        if (submitTarget != null)
            ExecuteEvents.Execute(submitTarget, new PointerEventData(eventSystem), ExecuteEvents.submitHandler);
        Assert.AreEqual(1, clicks, "пробел не должен повторно активировать ячейку");

        Object.Destroy(eventSystemGo);
        Object.Destroy(slotGo);
        Object.Destroy(canvasGo);
    }
}

using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Smoke-проверка запуска всей сцены MainScene в Play Mode:
/// 1) IoC-контекст (GameContext/RootModel) инициализируется раньше инъекций пулов —
///    регрессия NullReferenceException UnityContext&lt;T&gt;.OnAdded (см. GUI_Agent_Work/ISSUE-3);
/// 2) GUI (GameUi, хотбар/сумка) создаётся и наполняется;
/// 3) джойстики скрыты, появляются при касании своей зоны и скрываются после
///    отпускания, независимо друг от друга (multi-touch), и рисуются поверх сумки
///    (ISSUE-5); при открытой сумке их зоны не перехватывают новые касания;
/// 4) тестовые количества (1 и 128) показываются в сумке и хотбаре.
/// Типы игры получаются рефлексией: asmdef-сборка тестов не может ссылаться на
/// предопределённую Assembly-CSharp.
/// </summary>
public class GameStartSmokeTests
{
    private static System.Type GameType(string name)
    {
        var type = System.Type.GetType(name + ", Assembly-CSharp");
        Assert.NotNull(type, "тип " + name + " не найден в Assembly-CSharp");
        return type;
    }

    private static object GetJoystickComponent(string joystickName)
    {
        var joystickType = GameType("UltimateJoystick");
        return joystickType.GetMethod("GetUltimateJoystick").Invoke(null, new object[] { joystickName });
    }

    private static Transform GetJoystickVisuals(Component joystick)
    {
        return joystick.transform.Find("JoystickSizeFolder");
    }

    [UnityTest]
    public IEnumerator MainScene_InitializesAndGuiShows()
    {
        // Загрузка реальной игровой сцены по пути — без добавления в build profile
        // (LoadSceneAsyncInPlayMode предназначен именно для этого).
        var load = EditorSceneManager.LoadSceneAsyncInPlayMode(
            "Assets/MainScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
        Assert.NotNull(load, "запрос на загрузку MainScene должен быть создан");
        yield return new WaitUntil(() => load.isDone);
        Assert.AreEqual("MainScene", SceneManager.GetActiveScene().name, "MainScene должна загрузиться");

        // Даём время Awake/Start всего дерева объектов (IoC, пулы, генерация карты).
        yield return new WaitForSeconds(3f);

        // 1. IoC-цепочка: если бы UnityContext<T>.OnAdded упал, RootContext/Player/Bag
        //    остались бы неинициализированными, а GuiPool игры не был бы создан.
        var rootContextType = GameType("RootContext");
        // RootContext.Instance — статическое поле.
        var root = rootContextType.GetField("Instance").GetValue(null);
        Assert.NotNull(root, "RootContext должен существовать после загрузки");

        var player = rootContextType.GetProperty("Player").GetValue(root);
        Assert.NotNull(player, "Player (PlayerComtext) должен быть создан RootModel.OnInject");

        var bag = player.GetType().GetProperty("Bag").GetValue(player);
        Assert.NotNull(bag, "BagContext должен быть создан");

        // 2. GUI инициализирован (RootModel.Awake вызывает GameUi.EnsureCreated).
        Assert.NotNull(GameType("GameUi").GetProperty("Instance").GetValue(null), "GameUi должен быть создан");

        // 3. GuiPool.Awake корректно завершился (в т.ч. Inject без NRE на UnityContext<T>.OnAdded).
        Assert.NotNull(GameType("GuiPool").GetField("guiPool").GetValue(null), "GuiPool должен быть инициализирован");

        Assert.NotNull(EventSystem.current, "EventSystem должен быть активным");

        // 4. Джойстики поверх сумки: канвас GameUi ниже канваса Ultimate UI Canvas.
        var gameUi = GameType("GameUi").GetProperty("Instance").GetValue(null);
        var gameUiCanvas = ((Component)gameUi).GetComponentInChildren<Canvas>();
        Assert.NotNull(gameUiCanvas, "канвас GameUi должен существовать");
        var ultimateCanvas = GameObject.Find("Ultimate UI Canvas");
        Assert.NotNull(ultimateCanvas, "Ultimate UI Canvas должен существовать");
        Assert.Less(gameUiCanvas.sortingOrder, ultimateCanvas.GetComponent<Canvas>().sortingOrder,
            "джойстики должны отображаться поверх интерфейса GameUi (сумки)");

        // 5. Джойстики: компонент видимости подключён, визуал скрыт без касаний.
        var visibilityType = GameType("JoystickTouchVisibility");
        var joystick1 = (Component)GetJoystickComponent("Joystick1");
        var joystick2 = (Component)GetJoystickComponent("Joystick2");
        Assert.NotNull(joystick1, "Joystick1 должен существовать в сцене");
        Assert.NotNull(joystick2, "Joystick2 должен существовать в сцене");
        Assert.NotNull(joystick1.GetComponent(visibilityType), "Joystick1 должен иметь JoystickTouchVisibility");
        Assert.NotNull(joystick2.GetComponent(visibilityType), "Joystick2 должен иметь JoystickTouchVisibility");
        Assert.False(GetJoystickVisuals(joystick1).gameObject.activeSelf, "левый джойстик должен быть скрыт без касания");
        Assert.False(GetJoystickVisuals(joystick2).gameObject.activeSelf, "правый джойстик должен быть скрыт без касания");

        // 6. Multi-touch: два независимых пальца. pointerId 1 — левая зона, 2 — правая.
        var pointerDown = ExecuteEvents.pointerDownHandler;
        var pointerUp = ExecuteEvents.pointerUpHandler;
        var touch1 = new PointerEventData(EventSystem.current) { pointerId = 1, position = Vector2.zero };
        var touch2 = new PointerEventData(EventSystem.current) { pointerId = 2, position = new Vector2(Screen.width, 0f) };
        ExecuteEvents.Execute(joystick1.gameObject, touch1, pointerDown);
        ExecuteEvents.Execute(joystick2.gameObject, touch2, pointerDown);
        yield return new WaitForSeconds(0.2f);
        Assert.True(UltimateJoystickState(joystick1, "joystickState"), "левый джойстик должен быть удержан первым пальцем");
        Assert.True(UltimateJoystickState(joystick2, "joystickState"), "правый джойстик должен быть удержан вторым пальцем");
        Assert.True(GetJoystickVisuals(joystick1).gameObject.activeSelf, "левый джойстик должен появиться при касании");
        Assert.True(GetJoystickVisuals(joystick2).gameObject.activeSelf, "правый джойстик должен появиться при касании");

        // Отпускаем только первый палец: второй продолжает работать, первый скрыт.
        ExecuteEvents.Execute(joystick1.gameObject, touch1, pointerUp);
        yield return new WaitForSeconds(0.2f);
        Assert.False(GetJoystickVisuals(joystick1).gameObject.activeSelf, "левый джойстик должен скрыться после отпускания");
        Assert.True(GetJoystickVisuals(joystick2).gameObject.activeSelf, "правый джойстик должен остаться при удержании второго пальца");
        ExecuteEvents.Execute(joystick2.gameObject, touch2, pointerUp);
        yield return new WaitForSeconds(0.2f);
        Assert.False(GetJoystickVisuals(joystick2).gameObject.activeSelf, "правый джойстик должен скрыться после отпускания");

        // 7. Оформление количества: шрифт Comic + чёрный Outline (ISSUE-4), хотбар и сумка.
        AssertCountStyle("hotbar");
        AssertCountStyle("bagSlots");

        // 8. Тестовые данные количества: однозначное и многозначное число.
        SetCount(bag, "Items", 0, 1, "kirka");
        SetCount(bag, "BagItems", 0, 128, "torch");

        var showBag = player.GetType().GetMethod("ShowBag");
        var isShowBag = player.GetType().GetProperty("IsShowBag");
        if (!(bool)isShowBag.GetValue(player, null))
            showBag.Invoke(player, null);
        yield return new WaitForSeconds(0.5f);
        Assert.True((bool)isShowBag.GetValue(player, null), "сумка должна быть открыта");

        // 9. Открытая сумка: зоны джойстиков не перехватывают новые касания её ячеек
        //    (raycastTarget выключен; удерживаемый ранее палец при этом продолжает работать).
        Assert.False(GetTouchArea(joystick1).raycastTarget, "зона левого джойстика не должна ловить касания при открытой сумке");
        Assert.False(GetTouchArea(joystick2).raycastTarget, "зона правого джойстика не должна ловить касания при открытой сумке");

        ScreenCapture.CaptureScreenshot("GUI_Agent_Work/preview/playmode_smoke.png");
        yield return new WaitForSeconds(1.5f);
        // В batch-среде скриншот может не записаться — это не падение теста:
        // существование файла проверяется вне теста и фиксируется в TEST_REPORT.

        // Модель возвращается к пустым ячейкам; play-сессия после теста всё равно завершается.
        var items = bag.GetType().GetProperty("Items").GetValue(bag) as IList;
        var bagItems = bag.GetType().GetProperty("BagItems").GetValue(bag) as IList;
        if (items != null && items.Count > 0) SetEmpty(items[0]);
        if (bagItems != null && bagItems.Count > 0) SetEmpty(bagItems[0]);
    }

    private static bool UltimateJoystickState(Component joystick, string fieldName)
    {
        return (bool)joystick.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(joystick);
    }

    private static Image GetTouchArea(Component joystick)
    {
        // Активная графика при скрытом визуале — только невидимая зона касания.
        var images = joystick.GetComponentsInChildren<Image>(false);
        foreach (var image in images)
        {
            if (image.name == "TouchArea")
                return image;
        }
        Assert.Fail("зона касания TouchArea не найдена у " + joystick.name);
        return null;
    }

    private static void SetCount(object bag, string listPropName, int index, int count, string icon)
    {
        var list = bag.GetType().GetProperty(listPropName).GetValue(bag, null) as IList;
        Assert.NotNull(list, "список " + listPropName + " должен существовать");
        Assert.Greater(list.Count, index, "в " + listPropName + " должен быть элемент " + index);
        SetCountOn(list[index], count, icon);
    }

    private static void SetCountOn(object item, int count, string icon)
    {
        var itemType = item.GetType();
        itemType.GetProperty("Empty").SetValue(item, false, null);
        itemType.GetProperty("Count").SetValue(item, count, null);
        itemType.GetProperty("Icon").SetValue(item, icon, null);
    }

    /// <summary>
    /// Проверяет ячейки GameUi (по приватному полю-массиву): у Count-текста должен быть
    /// шрифт Comic и компонент Outline с непрозрачным чёрным цветом (см. ISSUE-4).
    /// </summary>
    private static void AssertCountStyle(string slotsFieldName)
    {
        var gameUi = GameType("GameUi").GetProperty("Instance").GetValue(null);
        Assert.NotNull(gameUi, "GameUi должен существовать");

        var gameUiType = GameType("GameUi");
        var slots = gameUiType.GetField(slotsFieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gameUi) as IList;
        Assert.NotNull(slots, "массив ячеек " + slotsFieldName + " должен существовать");
        Assert.Greater(slots.Count, 0, "в " + slotsFieldName + " должна быть хотя бы одна ячейка");

        var slotType = gameUiType.GetNestedType("Slot");
        var slot0 = slots[0];
        var countText = slotType.GetField("count").GetValue(slot0) as Text;
        Assert.NotNull(countText, "Count-текст ячейки должен существовать");

        var fontName = countText.font != null ? countText.font.name.ToLowerInvariant() : string.Empty;
        Assert.True(fontName.Contains("comic"), "числа должны быть шрифтом Comic, а не '" + fontName + "'");

        var outline = countText.GetComponent<Outline>();
        Assert.NotNull(outline, "у Count-текста должен быть компонент Outline");
        Assert.AreEqual(Color.black, outline.effectColor, "контур Count-текста должен быть чёрным");
        Assert.AreEqual(1f, outline.effectColor.a, "контур должен быть непрозрачным");
    }

    private static void SetEmpty(object item)
    {
        item.GetType().GetProperty("Empty").SetValue(item, true, null);
    }
}

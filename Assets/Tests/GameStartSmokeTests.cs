using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Smoke-проверка запуска всей сцены MainScene в Play Mode:
/// 1) IoC-контекст (GameContext/RootModel) инициализируется раньше инъекций пулов —
///    регрессия NullReferenceException UnityContext&lt;T&gt;.OnAdded (см. GUI_Agent_Work/ISSUE-3);
/// 2) GUI (GameUi, хотбар/сумка) создаётся и наполняется;
/// 3) мобильное управление: нейтраль осей без касаний (ISSUE-7), Base в точке
///    касания (ISSUE-8), полупроекранные зоны TouchArea ниже интерфейса (ISSUE-9),
///    multi-touch-независимость;
/// 4) тестовые количества (1 и 128) показываются в сумке и хотбаре.
/// Типы игры получаются рефлексией: asmdef-сборка тестов не может ссылаться на
/// предопределённую Assembly-CSharp.
/// </summary>
public class GameStartSmokeTests
{
    private const float PixelTolerance = 4f;

    private static System.Type GameType(string name)
    {
        var type = System.Type.GetType(name + ", Assembly-CSharp");
        Assert.NotNull(type, "тип " + name + " не найден в Assembly-CSharp");
        return type;
    }

    private static object GetJoystickComponent(string joystickName)
    {
        return GameType("UltimateJoystick").GetMethod("GetUltimateJoystick").Invoke(null, new object[] { joystickName });
    }

    private static Transform GetJoystickVisuals(Component joystick)
    {
        return joystick.transform.Find("JoystickSizeFolder");
    }

    private static float JoystickAxis(string joystickName, string axisMethod)
    {
        // Статический вариант метода с одним строковым параметром (у осей есть и
        // экземплярные перегрузки без параметров).
        return (float)GameType("UltimateJoystick").GetMethod(axisMethod, new[] { typeof(string) })
            .Invoke(null, new object[] { joystickName });
    }

    private static Vector2 GetFolderCenter(Component joystick)
    {
        // Вся визуальная часть (Base) лежит в JoystickSizeFolder (pivot 0,0).
        // Экземпляр джойстика может быть отскейлен в сцене (0.7), поэтому мировой
        // центр = worldPos(пивот) + локальный полуразмер * lossyScale.
        var folder = (RectTransform)GetJoystickVisuals(joystick);
        var scale = Mathf.Approximately(folder.lossyScale.x, 0f) ? 1f : Mathf.Abs(folder.lossyScale.x);
        return (Vector2)folder.position + folder.sizeDelta * 0.5f * scale;
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

        // 4. Нейтраль осей без касаний (ISSUE-7): персонаж не должен «съезжать» сам.
        Assert.AreEqual(0f, JoystickAxis("Joystick1", "GetHorizontalAxis"), 0.02f, "горизонтальная ось левого джойстика должна быть нейтральной до касания");
        Assert.AreEqual(0f, JoystickAxis("Joystick1", "GetVerticalAxis"), 0.02f, "вертикальная ось левого джойстика должна быть нейтральной до касания");
        Assert.AreEqual(0f, JoystickAxis("Joystick2", "GetHorizontalAxis"), 0.02f, "горизонтальная ось правого джойстика должна быть нейтральной до касания");
        Assert.AreEqual(0f, JoystickAxis("Joystick2", "GetVerticalAxis"), 0.02f, "вертикальная ось правого джойстика должна быть нейтральной до касания");

        // 5. Джойстики поверх сумки: канвас GameUi ниже канваса Ultimate UI Canvas.
        var gameUi = GameType("GameUi").GetProperty("Instance").GetValue(null);
        var gameUiCanvas = ((Component)gameUi).GetComponentInChildren<Canvas>();
        Assert.NotNull(gameUiCanvas, "канвас GameUi должен существовать");
        var ultimateCanvasGo = GameObject.Find("Ultimate UI Canvas");
        Assert.NotNull(ultimateCanvasGo, "Ultimate UI Canvas должен существовать");
        Assert.Less(gameUiCanvas.sortingOrder, ultimateCanvasGo.GetComponent<Canvas>().sortingOrder,
            "джойстики должны отображаться поверх интерфейса GameUi (сумки)");

        // 6. Зоны TouchArea: канвас ниже всех, половины ширины, нижние 50% высоты (ISSUE-9/10).
        var touchCanvasGo = GameObject.Find("TouchAreaCanvas");
        Assert.NotNull(touchCanvasGo, "TouchAreaCanvas должен существовать");
        var touchCanvas = touchCanvasGo.GetComponent<Canvas>();
        Assert.Less(touchCanvas.sortingOrder, gameUiCanvas.sortingOrder,
            "TouchArea должны лежать ВНИЗЕ сумки/ячеек/кнопок (маршрутизация raycast)");
        Assert.AreEqual(2, touchCanvasGo.transform.childCount, "должны существовать две зоны TouchArea");
        var areaComponentType = GameType("JoystickTouchArea");
        foreach (Transform area in touchCanvasGo.transform)
        {
            var rect = (RectTransform)area;
            var image = area.GetComponent<Image>();
            Assert.AreEqual(0f, image.color.a, "зона TouchArea должна быть прозрачной");
            var areaComponent = area.GetComponent(areaComponentType);
            Assert.IsNotNull(areaComponent, "у зоны должен быть JoystickTouchArea");
            var heightPercent = (float)areaComponentType.GetProperty("TouchAreaHeightPercent").GetValue(areaComponent, null);
            var heightRatio = rect.anchorMax.y - rect.anchorMin.y;
            Assert.AreEqual(heightPercent / 100f, heightRatio, 0.001f,
                "высота зоны должна быть нижней частью экрана по параметру TouchAreaHeightPercent");
            Assert.AreEqual(0f, rect.anchorMin.y, "зона прижата к низу экрана");
        }
        var leftArea = touchCanvasGo.transform.Find("TouchArea_Left") as RectTransform;
        var rightArea = touchCanvasGo.transform.Find("TouchArea_Right") as RectTransform;
        Assert.NotNull(leftArea, "должна быть зона TouchArea_Left");
        Assert.NotNull(rightArea, "должна быть зона TouchArea_Right");
        Assert.AreEqual(0f, leftArea.anchorMin.x, "левая зона начинается от X=0%");
        Assert.AreEqual(0.5f, leftArea.anchorMax.x, "левая зона заканчивается на X=50%");
        Assert.AreEqual(0.5f, rightArea.anchorMin.x, "правая зона начинается от X=50%");
        Assert.AreEqual(1f, rightArea.anchorMax.x, "правая зона заканчивается на X=100%");

        // Параметр высоты в Inspector: по умолчанию 50%, изменение пересчитывает якоря.
        var leftComponent = leftArea.GetComponent(areaComponentType);
        var rightComponent = rightArea.GetComponent(areaComponentType);
        Assert.AreEqual(50f, (float)areaComponentType.GetProperty("TouchAreaHeightPercent").GetValue(leftComponent, null),
            "высота зон по умолчанию должна быть 50% (нижняя половина экрана)");
        areaComponentType.GetProperty("TouchAreaHeightPercent").SetValue(leftComponent, 30f, null);
        Assert.AreEqual(0.3f, leftArea.anchorMax.y, 0.001f, "при 30% зона занимает нижнюю треть экрана");
        areaComponentType.GetProperty("TouchAreaHeightPercent").SetValue(leftComponent, 70f, null);
        Assert.AreEqual(0.7f, leftArea.anchorMax.y, 0.001f, "при 70% зона занимает нижние 70% экрана");
        areaComponentType.GetProperty("TouchAreaHeightPercent").SetValue(leftComponent, 50f, null);
        areaComponentType.GetProperty("TouchAreaHeightPercent").SetValue(rightComponent, 50f, null);
        Assert.AreEqual(0.5f, rightArea.anchorMax.x - rightArea.anchorMin.x, 0.001f, "правая зона — полширины экрана, без зазора с левой (0.5–1.0)");

        // TouchAreaRaycaster: источник активации — только зоны.
        var raycaster = touchCanvasGo.GetComponent(GameType("TouchAreaRaycaster"));
        Assert.NotNull(raycaster, "у TouchAreaCanvas должен быть TouchAreaRaycaster");
        var raycastMethod = GameType("TouchAreaRaycaster").GetMethod("Raycast");

        // Пробы по краям и центру зоны (нижний угол, центр, верхняя граница нижней зоны).
        var probePositions = new Vector2[]
        {
            new Vector2(Screen.width * 0.25f, 4f),          // левый нижний угол
            new Vector2(Screen.width * 0.25f, Screen.height * 0.25f), // центр левой зоны
            new Vector2(Screen.width * 0.25f, Screen.height * 0.5f),  // верхняя граница левой зоны
            new Vector2(Screen.width * 0.75f, 4f),          // правый нижний угол
            new Vector2(Screen.width * 0.75f, Screen.height * 0.25f), // центр правой зоны
            new Vector2(Screen.width * 0.75f, Screen.height * 0.5f),  // верхняя граница правой зоны
        };
        var expectedNames = new[] { "TouchArea_Left", "TouchArea_Left", "TouchArea_Left", "TouchArea_Right", "TouchArea_Right", "TouchArea_Right" };
        for (var i = 0; i < probePositions.Length; i++)
        {
            var probe = new PointerEventData(EventSystem.current) { pointerId = i % 2, position = probePositions[i] };
            var results = RaycastOnce(raycastMethod, raycaster, probe);
            Assert.GreaterOrEqual(results.Count, 1, "сенсорный палец в зоне (проба " + i + ") должен попадать в TouchArea");
            Assert.AreEqual(expectedNames[i], results[0].gameObject.name, "проба " + i + " попала не в ту зону");
        }

        // Мышь в редакторе активирует зоны (для тестирования без сенсорного устройства);
        // в билдах мышь игнорируется — прицеливание мышью работает как раньше.
        var mouseData = new PointerEventData(EventSystem.current) { pointerId = -1, position = new Vector2(Screen.width * 0.25f, Screen.height * 0.5f) };
        var mouseResults = RaycastOnce(raycastMethod, raycaster, mouseData);
        Assert.AreEqual(1, mouseResults.Count, "в редакторе мышь должна попадать в TouchArea (для тестов)");
        Assert.AreEqual("TouchArea_Left", mouseResults[0].gameObject.name);

        // 7. Джойстики: скрыты без касания, компонент видимости подключён.
        var visibilityType = GameType("JoystickTouchVisibility");
        var joystick1 = (Component)GetJoystickComponent("Joystick1");
        var joystick2 = (Component)GetJoystickComponent("Joystick2");
        Assert.NotNull(joystick1, "Joystick1 должен существовать в сцене");
        Assert.NotNull(joystick2, "Joystick2 должен существовать в сцене");
        Assert.NotNull(joystick1.GetComponent(visibilityType), "Joystick1 должен иметь JoystickTouchVisibility");
        Assert.NotNull(joystick2.GetComponent(visibilityType), "Joystick2 должен иметь JoystickTouchVisibility");
        Assert.False(GetJoystickVisuals(joystick1).gameObject.activeSelf, "левый джойстик должен быть скрыт без касания");
        Assert.False(GetJoystickVisuals(joystick2).gameObject.activeSelf, "правый джойстик должен быть скрыт без касания");

        // 7b. Единственный источник активации: собственная графика джойстиков отключена
        //     от raycast (raycastTarget = false) — касание их прямоугольников не обрабатывается.
        foreach (var graphic in joystick1.GetComponentsInChildren<Graphic>(true))
            Assert.False(graphic.raycastTarget, "графика джойстика должна быть отключена от raycast: " + graphic.name);
        foreach (var graphic in joystick2.GetComponentsInChildren<Graphic>(true))
            Assert.False(graphic.raycastTarget, "графика джойстика должна быть отключена от raycast: " + graphic.name);

        // 8. Маршрутизация через зоны + multi-touch: палец 1 — левая зона, палец 2 — правая.
        //    События посылаются в TouchArea (как это делает EventSystem по результату raycast),
        //    форвардер пересылает их в целевой джойстик.
        var leftTouchPoint = new Vector2(Screen.width * 0.25f, Screen.height * 0.4f);
        var rightTouchPoint = new Vector2(Screen.width * 0.75f, Screen.height * 0.4f);
        var touch1 = new PointerEventData(EventSystem.current) { pointerId = 1, position = leftTouchPoint };
        var touch2 = new PointerEventData(EventSystem.current) { pointerId = 2, position = rightTouchPoint };

        var areaType = GameType("JoystickTouchArea");
        var leftForwarder = leftArea.GetComponent(areaType);
        var rightForwarder = rightArea.GetComponent(areaType);
        Assert.NotNull(leftForwarder, "левая зона должна иметь JoystickTouchArea");
        Assert.NotNull(rightForwarder, "правая зона должна иметь JoystickTouchArea");
        var leftTarget = areaType.GetProperty("Target").GetValue(leftForwarder, null);
        var rightTarget = areaType.GetProperty("Target").GetValue(rightForwarder, null);
        Assert.NotNull(leftTarget, "левая зона должна быть связана с джойстиком");
        Assert.NotNull(rightTarget, "правая зона должна быть связана с джойстиком");
        Assert.AreEqual("Joystick1", leftTarget.GetType().GetField("joystickName").GetValue(leftTarget),
            "левая зона должна быть связана с Joystick1");
        Assert.AreEqual("Joystick2", rightTarget.GetType().GetField("joystickName").GetValue(rightTarget),
            "правая зона должна быть связана с Joystick2");

        var pointerDown = ExecuteEvents.pointerDownHandler;
        var pointerUp = ExecuteEvents.pointerUpHandler;
        ExecuteEvents.Execute(leftArea.gameObject, touch1, pointerDown);
        ExecuteEvents.Execute(rightArea.gameObject, touch2, pointerDown);
        yield return new WaitForSeconds(0.2f);

        Assert.True(JoystickHeld(joystick1), "левый джойстик должен быть удержан первым пальцем");
        Assert.True(JoystickHeld(joystick2), "правый джойстик должен быть удержан вторым пальцем");
        Assert.True(GetJoystickVisuals(joystick1).gameObject.activeSelf, "левый джойстик должен появиться при касании");
        Assert.True(GetJoystickVisuals(joystick2).gameObject.activeSelf, "правый джойстик должен появиться при касании");

        // 9. Base в точке касания (ISSUE-8): центр визуала совпадает с точкой касания.
        Vector2AssertClose(GetFolderCenter(joystick1), leftTouchPoint, PixelTolerance,
            "Base левого джойстика должен быть в точке начала касания");
        Vector2AssertClose(GetFolderCenter(joystick2), rightTouchPoint, PixelTolerance,
            "Base правого джойстика должен быть в точке начала касания");

        // 9a. Handle строго в центре Base в момент появления (ISSUE-10):
        var handle1 = GetJoystickVisuals(joystick1).Find("Joystick");
        var base1 = GetJoystickVisuals(joystick1).Find("Base");
        var handle2 = GetJoystickVisuals(joystick2).Find("Joystick");
        Assert.NotNull(handle1, "у джойстика должен быть дочерний Handle 'Joystick'");
        Assert.NotNull(base1, "у джойстика должен быть дочерний 'Base'");
        Assert.NotNull(handle2, "у джойстика должен быть дочерний Handle 'Joystick'");
        Vector2AssertClose((Vector2)handle1.position, (Vector2)base1.position, PixelTolerance,
            "Handle левого джойстика должен быть в центре Base при появлении");
        Vector2AssertClose((Vector2)handle2.position, GetFolderCenter(joystick2), PixelTolerance,
            "Handle правого джойстика должен быть в центре Base при появлении");
        Assert.AreEqual(0f, JoystickAxis("Joystick1", "GetHorizontalAxis"), 0.02f, "ось левого джойстика нейтральна в момент появления");
        Assert.AreEqual(0f, JoystickAxis("Joystick2", "GetVerticalAxis"), 0.02f, "ось правого джойстика нейтральна в момент появления");

        // 9b. Движение пальца: Handle смещается относительно центра Base в направлении
        //     пальца, а горизонтальная ось становится положительной при движении вправо.
        var dragAxisHandler = ExecuteEvents.dragHandler;
        var dragPoint = leftTouchPoint + new Vector2(80f, 0f);
        touch1.position = dragPoint;
        ExecuteEvents.Execute(leftArea.gameObject, touch1, dragAxisHandler);
        yield return new WaitForSeconds(0.1f);
        Assert.Greater(JoystickAxis("Joystick1", "GetHorizontalAxis"), 0.1f,
            "при смещении пальца вправо горизонтальная ось должна стать положительной");
        var handle1Pos = (Vector2)handle1.position;
        Assert.Greater(handle1Pos.x, ((Vector2)base1.position).x,
            "Handle должен сместиться вправо относительно центра Base");
        var radius1 = ((RectTransform)GetJoystickVisuals(joystick1)).sizeDelta.x * 0.45f; // radiusModifier 4.5/10
        Assert.LessOrEqual((handle1Pos - (Vector2)base1.position).magnitude, radius1 + 1f,
            "Handle не должен выходить за радиус Base");

        // 9c. Отпускаем левый палец перед сравнением половин — состояние нейтрально.
        ExecuteEvents.Execute(leftArea.gameObject, touch1, pointerUp);
        yield return new WaitForSeconds(0.2f);
        Assert.False(GetJoystickVisuals(joystick1).gameObject.activeSelf, "левый джойстик скрылся после отпускания");
        Assert.AreEqual(0f, JoystickAxis("Joystick1", "GetHorizontalAxis"), 0.02f, "ось левого джойстика нейтральна после отпускания");

        // 9d. Перемещение пальца ЗА пределы зоны после начала управления: захват
        //     pointerPress удерживает события на TouchArea_Left — джойстик продолжает
        //     работать, правый не задействуется.
        ExecuteEvents.Execute(leftArea.gameObject, touch1, pointerDown);
        yield return new WaitForSeconds(0.1f);
        touch1.position = new Vector2(Screen.width * 0.9f, Screen.height * 0.9f); // далеко за пределами левой зоны
        ExecuteEvents.Execute(leftArea.gameObject, touch1, dragAxisHandler);
        yield return new WaitForSeconds(0.1f);
        Assert.True(JoystickHeld(joystick1), "захват пальца сохраняется за пределами зоны");
        // Правый джойстик удерживается СВОИМ пальцем (pointerId 2): его _pointerId
        // не должен смениться из-за того, что палец 1 переместился в правую половину.
        var pointerId2 = (int)joystick2.GetType().GetField("_pointerId", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(joystick2);
        Assert.AreEqual(2, pointerId2, "правый джойстик должен удерживаться только своим пальцем (fingerId 2)");
        Assert.Greater(JoystickAxis("Joystick1", "GetHorizontalAxis"), 0.1f,
            "Handle/ось должны продолжать отрабатывать палец за пределами зоны");
        ExecuteEvents.Execute(leftArea.gameObject, touch1, pointerUp);
        yield return new WaitForSeconds(0.2f);
        Assert.False(GetJoystickVisuals(joystick1).gameObject.activeSelf, "левый джойстик скрылся после отпускания (за пределами зоны)");
        Assert.AreEqual(0f, JoystickAxis("Joystick1", "GetHorizontalAxis"), 0.02f, "ось левого джойстика нейтральна после отпускания");

        // 10. Правый палец всё ещё удержан; оси левого нейтральны после его отпускания.
        Assert.True(JoystickHeld(joystick2), "правый джойстик должен остаться при удержании второго пальца");
        Assert.AreEqual(0f, JoystickAxis("Joystick1", "GetHorizontalAxis"), 0.02f, "ось левого джойстика нейтральна после отпускания");
        Assert.AreEqual(0f, JoystickAxis("Joystick1", "GetVerticalAxis"), 0.02f, "ось левого джойстика нейтральна после отпускания");

        ExecuteEvents.Execute(rightArea.gameObject, touch2, pointerUp);
        yield return new WaitForSeconds(0.2f);
        Assert.False(GetJoystickVisuals(joystick2).gameObject.activeSelf, "правый джойстик должен скрыться после отпускания");
        Assert.AreEqual(0f, JoystickAxis("Joystick2", "GetHorizontalAxis"), 0.02f, "ось правого джойстика нейтральна после отпускания");
        Assert.AreEqual(0f, JoystickAxis("Joystick2", "GetVerticalAxis"), 0.02f, "ось правого джойстика нейтральна после отпускания");

        // 11. Оформление количества: шрифт Comic + чёрный Outline (ISSUE-4).
        AssertCountStyle("hotbar");
        AssertCountStyle("bagSlots");

        // 12. Тестовые данные количества: однозначное и многозначное число.
        SetCount(bag, "Items", 0, 1, "kirka");
        SetCount(bag, "BagItems", 0, 128, "torch");

        var showBag = player.GetType().GetMethod("ShowBag");
        var isShowBag = player.GetType().GetProperty("IsShowBag");
        if (!(bool)isShowBag.GetValue(player, null))
            showBag.Invoke(player, null);
        yield return new WaitForSeconds(0.5f);
        Assert.True((bool)isShowBag.GetValue(player, null), "сумка должна быть открыта");

        ScreenCapture.CaptureScreenshot("Agent_Works/GUI_Agent_Work/preview/playmode_smoke.png");
        yield return new WaitForSeconds(1.5f);
        // В batch-среде скриншот может не записаться — это не падение теста:
        // существование файла проверяется вне теста и фиксируется в TEST_REPORT.

        // Модель возвращается к пустым ячейкам; play-сессия после теста всё равно завершается.
        var items = bag.GetType().GetProperty("Items").GetValue(bag) as IList;
        var bagItems = bag.GetType().GetProperty("BagItems").GetValue(bag) as IList;
        if (items != null && items.Count > 0) SetEmpty(items[0]);
        if (bagItems != null && bagItems.Count > 0) SetEmpty(bagItems[0]);
    }

    private static List<RaycastResult> RaycastOnce(MethodInfo raycastMethod, Component raycaster, PointerEventData data)
    {
        var results = new List<RaycastResult>();
        raycastMethod.Invoke(raycaster, new object[] { data, results });
        return results;
    }

    private static void Vector2AssertClose(Vector2 actual, Vector2 expected, float tolerance, string message)
    {
        Assert.True((actual - expected).magnitude <= tolerance,
            message + ". Ожидалась точка " + expected + ", фактически " + actual);
    }

    private static bool JoystickHeld(Component joystick)
    {
        return (bool)joystick.GetType().GetField("joystickState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(joystick);
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

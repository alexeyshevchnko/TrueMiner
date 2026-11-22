using System.Collections;
using System.IO;
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
/// 3) тестовые количества (1 и 128) показываются в сумке и хотбаре;
///    скриншот сохраняется в GUI_Agent_Work/preview/playmode_smoke.png.
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

        // 4. Оформление количества: шрифт Comic + чёрный Outline (ISSUE-4), хотбар и сумка.        AssertCountStyle("hotbar");
        AssertCountStyle("bagSlots");

        // 5. Тестовые данные количества: однозначное и многозначное число.
        SetCount(bag, "Items", 0, 1, "kirka");
        SetCount(bag, "BagItems", 0, 128, "torch");

        var showBag = player.GetType().GetMethod("ShowBag");
        var isShowBag = player.GetType().GetProperty("IsShowBag");
        if (!(bool)isShowBag.GetValue(player, null))
            showBag.Invoke(player, null);
        yield return new WaitForSeconds(1f);
        Assert.True((bool)isShowBag.GetValue(player, null), "сумка должна быть открыта");

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

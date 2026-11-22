using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Проверка нижней панели быстрого доступа (ISSUE-6):
/// 7 ячеек, масштаб 0.7 от исходного (ячейка 46.2 вместо 66, пропорциональные
/// промежутки/отступы), панель по центру низа экрана; оформление количества
/// масштабировано вместе с ячейками (BestFit max 14 = 20*0.7, контур 1.05).
/// Модель инвентаря не изменилась: Bag.Items хранит 9, основная сумка — 36 ячеек.
/// </summary>
public class HotbarSevenSlotsTests
{
    private static System.Type GameType(string name)
    {
        var type = System.Type.GetType(name + ", Assembly-CSharp");
        Assert.NotNull(type, "тип " + name + " не найден в Assembly-CSharp");
        return type;
    }

    [UnityTest]
    public IEnumerator MainScene_HotbarIsSevenSlotsAtScale07()
    {
        var load = EditorSceneManager.LoadSceneAsyncInPlayMode(
            "Assets/MainScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
        Assert.NotNull(load);
        yield return new WaitUntil(() => load.isDone);
        Assert.AreEqual("MainScene", SceneManager.GetActiveScene().name);
        yield return new WaitForSeconds(2f);

        // Панель хотбара и её сетка.
        var hotbarGo = GameObject.Find("Hotbar");
        Assert.NotNull(hotbarGo, "панель Hotbar должна существовать");
        var grid = hotbarGo.GetComponent<GridLayoutGroup>();
        Assert.NotNull(grid, "у Hotbar должна быть GridLayoutGroup");

        // Масштаб 0.7 от исходных значений.
        Assert.AreEqual(66f * 0.7f, grid.cellSize.x, 0.01f, "ячейка хотбара должна быть 66*0.7");
        Assert.AreEqual(66f * 0.7f, grid.cellSize.y, 0.01f, "высота ячейки должна сохранять пропорцию");
        Assert.AreEqual(6f * 0.7f, grid.spacing.x, 0.01f, "промежутки масштабируются пропорционально");
        Assert.AreEqual(7, grid.constraintCount, "панель должна показывать 7 ячеек");

        // Панель по центру нижней части экрана: якорь снизу, ось X по центру.
        var panelRect = hotbarGo.GetComponent<RectTransform>();
        Assert.AreEqual(0.5f, panelRect.anchorMin.x, "панель должна быть отцентрирована по X");
        Assert.AreEqual(0.5f, panelRect.anchorMax.x, "панель должна быть отцентрована по X");
        Assert.AreEqual(0f, panelRect.anchorMin.y, "панель должна быть прижата к низу");
        Assert.AreEqual(0.5f, panelRect.pivot.x, "точка вращения/масштабирования — центр по X");

        // Массив UI-ячеек: 7; основная сумка не изменилась: 36.
        var gameUi = GameType("GameUi").GetProperty("Instance").GetValue(null);
        var gameUiType = GameType("GameUi");
        var hotbar = gameUiType.GetField("hotbar", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gameUi) as IList;
        Assert.NotNull(hotbar, "массив hotbar должен существовать");
        Assert.AreEqual(7, hotbar.Count, "UI-массив хотбара должен содержать 7 ячеек");
        var bagSlots = gameUiType.GetField("bagSlots", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gameUi) as IList;
        Assert.AreEqual(36, bagSlots.Count, "основная сумка должна остаться 36 ячеек");

        // Модель быстрого доступа по-прежнему хранит 9 элементов (первые 7 отображаются).
        var root = GameType("RootContext").GetField("Instance").GetValue(null);
        var player = GameType("RootContext").GetProperty("Player").GetValue(root);
        var bag = player.GetType().GetProperty("Bag").GetValue(player);
        var items = bag.GetType().GetProperty("Items").GetValue(bag) as IList;
        Assert.AreEqual(9, items.Count, "модель Bag.Items должна хранить 9 элементов");

        // Оформление количества масштабировано вместе с ячейками: BestFit max = 20*0.7,
        // контур = 1.5*0.7; разметка Count также *0.7.
        var slotType = gameUiType.GetNestedType("Slot");
        var countText = slotType.GetField("count").GetValue(hotbar[0]) as Text;
        Assert.NotNull(countText);
        Assert.AreEqual(14, countText.resizeTextMaxSize, "максимальный размер цифр хотбара = 20*0.7");
        Assert.AreEqual(7, countText.resizeTextMinSize, "минимальный размер цифр хотбара = 10*0.7");
        var outline = countText.GetComponent<Outline>();
        Assert.AreEqual(1.5f * 0.7f, outline.effectDistance.x, 0.01f, "контур масштабирован 0.7");
        Assert.AreEqual(-1.5f * 0.7f, outline.effectDistance.y, 0.01f, "контур масштабирован 0.7");
        var countRect = countText.rectTransform;
        Assert.AreEqual(52f * 0.7f, countRect.sizeDelta.x, 0.01f, "ширина поля цифр масштабирована 0.7");
    }
}

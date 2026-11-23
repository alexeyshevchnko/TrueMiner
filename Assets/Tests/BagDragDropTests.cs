using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// PlayMode-проверка Drag &amp; Drop предметов сумки и масштаба интерфейса 0.7 (ISSUE-13).
/// События указателя имитируются через ExecuteEvents (как в touch-тестах):
/// PointerDown → Drag (потом порога pixelDragThreshold) → PointerUp.
/// Модель и визуус GameUi получаются рефлексией (см. комментарий в GameStartSmokeTests).
/// </summary>
public class BagDragDropTests
{
    private static System.Type GameType(string name)
    {
        var type = System.Type.GetType(name + ", Assembly-CSharp");
        Assert.NotNull(type, "тип " + name + " не найден в Assembly-CSharp");
        return type;
    }

    private object player;
    private object bagModel; // Bag (модель, слоты 1–45)
    private object gameUi;
    private System.Type slotType;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        var load = EditorSceneManager.LoadSceneAsyncInPlayMode(
            "Assets/MainScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
        Assert.NotNull(load, "запрос на загрузку MainScene должен быть создан");
        yield return new WaitUntil(() => load.isDone);
        yield return new WaitForSeconds(3f);

        var root = GameType("RootContext").GetField("Instance").GetValue(null);
        Assert.NotNull(root, "RootContext должен существовать после загрузки");
        player = GameType("RootContext").GetProperty("Player").GetValue(root);
        Assert.NotNull(player, "Player должен существовать");
        bagModel = player.GetType().GetProperty("Bag") == null ? null
            : GetPlayerBagModel();
        gameUi = GameType("GameUi").GetProperty("Instance").GetValue(null);
        Assert.NotNull(gameUi, "GameUi должен быть создан");
        slotType = GameType("GameUi").GetProperty("Instance").GetValue(null)
            .GetType().GetNestedType("Slot");
        Assert.NotNull(slotType, "GameUi.Slot должен существовать");

        ОткрытьСумку();
    }

    object GetPlayerBagModel()
    {
        var targetManager = player.GetType().GetProperty("TargetManager").GetValue(player);
        var pc = targetManager.GetType().GetProperty("PlayerController").GetValue(targetManager);
        var model = pc.GetType().GetMethod("GetModel").Invoke(pc, null);
        return model.GetType().GetMethod("GetBag").Invoke(model, null);
    }

    void ОткрытьСумку()
    {
        player.GetType().GetMethod("ShowBag").Invoke(player, null);
        Assert.True((bool)player.GetType().GetProperty("IsShowBag").GetValue(player), "сумка должна быть открыта");
    }

    GameObject[] GetSlots(string fieldName)
    {
        var arr = gameUi.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gameUi);
        var list = (System.Collections.IList)arr;
        var result = new GameObject[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            var bg = slotType.GetField("bg").GetValue(list[i]) as Image;
            result[i] = bg.gameObject;
        }
        return result;
    }

    object GetModelItem(int key)
    {
        var dict = bagModel.GetType().GetField("items").GetValue(bagModel) as System.Collections.IDictionary;
        return dict.Contains(key) ? dict[key] : null;
    }

    object GetViewContext(string fieldName, int i)
    {
        var arr = gameUi.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gameUi);
        return ((System.Collections.IList)arr)[i];
    }

    Vector2 SlotScreenCenter(int slotIndex)
    {
        var bg = slotType.GetField("bg").GetValue(((System.Collections.IList)gameUi.GetType()
            .GetField("bagSlots", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gameUi))[slotIndex]) as Image;
        var corners = new Vector3[4];
        bg.rectTransform.GetWorldCorners(corners);
        return (Vector2)(corners[0] + corners[2]) * 0.5f;
    }

    void SendDown(GameObject go, PointerEventData ed, Vector2 pos)
    {
        ed.pointerId = 1; // сенсорный палец
        ed.position = pos;
        ExecuteEvents.Execute(go, ed, ExecuteEvents.pointerDownHandler);
    }

    void SendDrag(GameObject go, PointerEventData ed, Vector2 pos)
    {
        ed.position = pos;
        ExecuteEvents.Execute(go, ed, ExecuteEvents.dragHandler);
    }

    void SendUp(GameObject go, PointerEventData ed, Vector2 pos)
    {
        ed.position = pos;
        ExecuteEvents.Execute(go, ed, ExecuteEvents.pointerUpHandler);
    }

    void SendClick(GameObject go, PointerEventData ed, Vector2 pos)
    {
        // Button (IPointerClickHandler) — как реальный StandaloneInputModule при отпускании без переноса
        ed.position = pos;
        ExecuteEvents.Execute(go, ed, ExecuteEvents.pointerClickHandler);
    }

    bool ViewIsEmpty(object ctx) { return (bool)ctx.GetType().GetProperty("Empty").GetValue(ctx); }

    bool SlotHidden(int slotIndex)
    {
        var slot = ((System.Collections.IList)gameUi.GetType().GetField("bagSlots", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gameUi))[slotIndex];
        return (bool)slotType.GetField("dragHidden").GetValue(slot);
    }

    bool GhostActive()
    {
        var ghost = gameUi.GetType().GetField("dragGhost", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gameUi) as Image;
        return ghost != null && ghost.gameObject.activeSelf;
    }

    Image Ghost()
    {
        return gameUi.GetType().GetField("dragGhost", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gameUi) as Image;
    }

    void EnsureItem(object item, int count, string icon)
    {
        var t = item.GetType();
        t.GetProperty("Empty").SetValue(item, false, null);
        t.GetProperty("Count").SetValue(item, count, null);
        t.GetProperty("Icon").SetValue(item, icon, null);
    }

    [UnityTest]
    public IEnumerator BagPanelScaledToSeventyPercent()
    {
        var bagPanel = GameObject.Find("BagPanel");
        Assert.NotNull(bagPanel, "BagPanel должен существовать");
        var grid = bagPanel.GetComponent<GridLayoutGroup>();
        var rt = bagPanel.GetComponent<RectTransform>();

        Assert.AreEqual(66f * 0.7f, grid.cellSize.x, 0.01f, "ячейка сумки должна быть 66*0.7 (масштаб = хотбару)");
        Assert.AreEqual(6f * 0.7f, grid.spacing.x, 0.01f, "промежутки сумки должны быть 6*0.7");
        Assert.AreEqual(8.4f, grid.padding.left, 0.5f, "поля сумки должны быть 12*0.7");
        Assert.AreEqual((9f * 70f + 8f * 6f + 24f) * 0.7f, rt.sizeDelta.x, 0.05f, "ширина панели сумки ×0.7");
        Assert.AreEqual((4f * 70f + 3f * 6f + 24f) * 0.7f, rt.sizeDelta.y, 0.05f, "высота панели сумки ×0.7");
        Assert.True(Mathf.Abs(rt.anchoredPosition.x) < 0.01f, "панель сумки остаётся по центру по X");
        Assert.AreEqual(92f, rt.anchoredPosition.y, 0.01f, "панель сумки остаётся над хотбаром");
        yield return null;
    }

    [UnityTest]
    public IEnumerator DragMovesItemBetweenEmptySlots()
    {
        EnsureItem(GetModelItem(10), 5, "torch"); // bag item slot 10 (первый слот сумки)
        yield return new WaitForSeconds(0.2f);
        Assert.False(ViewIsEmpty(GetViewContext("bagSlots", 0)), "слот 1 сумки должен показать предмет");

        var slots = GetSlots("bagSlots");
        var source = slots[0];
        var target = slots[3];
        var ed = new PointerEventData(EventSystem.current);

        SendDown(source, ed, SlotScreenCenter(0));
        Assert.False(GhostActive(), "призрак не должен появляться до превышения порога перетаскивания");

        // Драг: порог EventSystem уже подразумевается больше (мы подаём dragHandler напрямую)
        SendDrag(source, ed, SlotScreenCenter(0) + new Vector2(20f, 10f));
        Assert.True(GhostActive(), "призрак должен появиться при перетаскивании");
        Assert.True(SlotHidden(0), "исходная ячейка скрывает предмет на время переноса (нет дублирования)");
        Assert.NotNull(Ghost().sprite, "призрак должен показывать иконку предмета");
        Assert.False(Ghost().raycastTarget, "призрак не должен перехватывать raycast");

        SendDrag(source, ed, SlotScreenCenter(3));
        SendUp(source, ed, SlotScreenCenter(3));

        Assert.NotNull(GetModelItem(10 + 3), "предмет должен переместиться в ключ 13");
        Assert.IsNull(GetModelItem(10), "исходный ключ 10 должен опустеть");
        Assert.False(GhostActive(), "призрак должен исчезнуть после переноса");
        Assert.False(SlotHidden(0), "исходная ячейка больше не должна скрывать предмет");
        yield return new WaitForSeconds(0.2f);
        Assert.True(ViewIsEmpty(GetViewContext("bagSlots", 0)), "исходная ячейка визуально пуста");
        Assert.False(ViewIsEmpty(GetViewContext("bagSlots", 3)), "целевая ячейка показывает предмет");
    }

    [UnityTest]
    public IEnumerator DragOutsideSlotRestoresItem()
    {
        EnsureItem(GetModelItem(11), 7, "kirka");
        yield return new WaitForSeconds(0.2f);

        var slots = GetSlots("bagSlots");
        var ed = new PointerEventData(EventSystem.current);
        SendDown(slots[1], ed, SlotScreenCenter(1));
        SendDrag(slots[1], ed, SlotScreenCenter(1) + new Vector2(30f, 0f));
        Assert.True(GhostActive(), "призрак активен при переносе");

        // Отпускание вне всех ячеек (низ экрана)
        SendUp(slots[1], ed, new Vector2(SlotScreenCenter(1).x, 2f));
        Assert.NotNull(GetModelItem(11), "предмет не должен потеряться при отпускании вне ячеек");
        Assert.False(GhostActive(), "призрак скрыт (возврат предмета)");
        Assert.False(SlotHidden(1), "исходная ячейка снова показывает предмет");
    }

    [UnityTest]
    public IEnumerator DragBackToSourceKeepsItem()
    {
        EnsureItem(GetModelItem(12), 3, "bow2");
        yield return new WaitForSeconds(0.2f);

        var slots = GetSlots("bagSlots");
        var ed = new PointerEventData(EventSystem.current);
        SendDown(slots[2], ed, SlotScreenCenter(2));
        SendDrag(slots[2], ed, SlotScreenCenter(2) + new Vector2(40f, 0f));
        SendUp(slots[2], ed, SlotScreenCenter(2)); // в исходную ячейку
        Assert.NotNull(GetModelItem(12), "предмет остаётся в исходной ячейке");
        Assert.False(GhostActive(), "призрак скрыт");
    }

    [UnityTest]
    public IEnumerator DragToOccupiedSlotSwapsWithoutLoss()
    {
        EnsureItem(GetModelItem(10), 1, "torch");
        EnsureItem(GetModelItem(12), 2, "kirka");
        yield return new WaitForSeconds(0.2f);

        var a = GetModelItem(10);
        var b = GetModelItem(12);
        var slots = GetSlots("bagSlots");
        var ed = new PointerEventData(EventSystem.current);
        SendDown(slots[0], ed, SlotScreenCenter(0));
        SendDrag(slots[0], ed, SlotScreenCenter(0) + new Vector2(25f, 0f));
        SendUp(slots[0], ed, SlotScreenCenter(2));
        Assert.AreEqual(b, GetModelItem(10), "второй предмет должен переехать в ключ 10 (обмен, без перезаписи)");
        Assert.AreEqual(a, GetModelItem(12), "первый предмет должен переехать в ключ 12 (обмен, без перезаписи)");
        Assert.False(GhostActive(), "призрак скрыт");
    }

    [UnityTest]
    public IEnumerator ShortClickDoesNotDrag()
    {
        EnsureItem(GetModelItem(10), 1, "torch");
        yield return new WaitForSeconds(0.2f);

        var slots = GetSlots("bagSlots");
        var ed = new PointerEventData(EventSystem.current);
        SendDown(slots[0], ed, SlotScreenCenter(0));
        SendUp(slots[0], ed, SlotScreenCenter(0)); // без драг-событий: короткое нажатие
        SendClick(slots[0], ed, SlotScreenCenter(0));
        Assert.NotNull(GetModelItem(10), "короткое нажатие не перемещает предмет из модели");
        Assert.False(GhostActive(), "призрак не появлялся");
        // Короткое нажатие сохраняет прежнее назначение Click() — предмет «взято»
        // (двухшаговый перенос кликом через PlayerComtext.MoveItem).
        var moveItem = player.GetType().GetProperty("MoveItem").GetValue(player);
        Assert.NotNull(moveItem, "короткое нажатие сохраняет прежнее поведение Click (предмет «взято»)");
        // Восстановление состояния для других тестов.
        player.GetType().GetProperty("MoveItem").SetValue(player, null, null);
        player.GetType().GetProperty("IsMoveItem").SetValue(player, false, null);
        EnsureItem(GetModelItem(10), 1, "torch");
        var ctx = GetViewContext("bagSlots", 0);
        ctx.GetType().GetProperty("Empty").SetValue(ctx, false, null);
    }

    [UnityTest]
    public IEnumerator SecondFingerDoesNotStealDrag()
    {
        EnsureItem(GetModelItem(10), 1, "torch");
        yield return new WaitForSeconds(0.2f);

        var slots = GetSlots("bagSlots");
        var ed1 = new PointerEventData(EventSystem.current);
        var ed2 = new PointerEventData(EventSystem.current);

        SendDown(slots[0], ed1, SlotScreenCenter(0));
        SendDrag(slots[0], ed1, SlotScreenCenter(0) + new Vector2(20f, 0f));
        Assert.True(GhostActive(), "драг первого пальца активен");

        // Второй палец press на другую ячейку: не должен перебить драг первого
        ed2.pointerId = 2;
        ExecuteEvents.Execute(slots[4], ed2, ExecuteEvents.pointerDownHandler);
        SendDrag(slots[0], ed1, SlotScreenCenter(3)); // первый палец продолжает драг
        SendUp(slots[0], ed1, SlotScreenCenter(3));
        Assert.NotNull(GetModelItem(13), "драг первого пальца должен завершиться переносом");
        Assert.False(GhostActive(), "призрак скрыт");
    }
}

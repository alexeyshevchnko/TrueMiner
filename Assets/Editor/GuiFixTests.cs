using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// EditMode-проверки загрузки иконок из атласа Resources/gui (GuiAtlas).
/// Ожидаемые прямоугольники кадров взяты из Resources/gui/gui.txt (TexturePacker,
/// Y сверху); GuiAtlas пересчитывает их в координаты Unity (Y снизу).
/// Поведение кнопок проверяется в Play Mode (Assets/Tests/GuiButtonPlayTests.cs),
/// т.к. только там жизненный цикл EventSystem соответствует игровому.
/// </summary>
public class GuiFixTests
{
    static readonly string[] ItemIconNames =
    {
        // Имена, возвращаемые GetUiIcon() всех предметов (см. Assets/src/weapons):
        "torch", "kirka", "granade1", "bow2", "Axe", "Item1",
        // Блоки BlowBlockItem:
        "ground", "green", "coal", "gold", "wood", "brick", "kost", "meat", "med", "stal", "stone"
    };

    [Test]
    public void Atlas_ItemIconNames_ResolveToSprite()
    {
        foreach (var name in ItemIconNames)
        {
            var sprite = GuiAtlas.Get(name);
            Assert.NotNull(sprite, "иконка '" + name + "' не найдена в атласе gui");
        }
    }

    [Test]
    public void Atlas_LookupIsCaseInsensitive_AndAcceptsPngExtension()
    {
        Assert.NotNull(GuiAtlas.Get("axe"));
        Assert.NotNull(GuiAtlas.Get("Axe.png"));
        Assert.NotNull(GuiAtlas.Get("ITEM1"));
        Assert.Null(GuiAtlas.Get(null));
        Assert.Null(GuiAtlas.Get(""));
    }

    [Test]
    public void Atlas_RectMatchesGuiTxt_WithYFlipped()
    {
        // kirka.png: frame {x:136,y:250,w:32,h:32}; высота атласа 512.
        var kirka = GuiAtlas.Get("kirka");
        Assert.NotNull(kirka);
        Assert.AreEqual(new Rect(136f, 512f - 250f - 32f, 32f, 32f), kirka.rect, "прямоугольник kirka не совпал (Y должен быть перевёрнут)");

        // cell.png: frame {x:36,y:200,w:48,h:48}.
        var cell = GuiAtlas.Get("cell");
        Assert.NotNull(cell);
        Assert.AreEqual(new Rect(36f, 512f - 200f - 48f, 48f, 48f), cell.rect);
    }

    [Test]
    public void Atlas_UnknownIconReturnsNull()
    {
        LogAssert.Expect(LogType.Warning, "GuiAtlas: иконка 'definitely_not_in_atlas' не найдена в атласе gui.txt");
        Assert.Null(GuiAtlas.Get("definitely_not_in_atlas"));
    }

    // ---- DeselectOnClick (логика компонента) ----
    // В batch EditMode static EventSystem.current не заполняется окружением тестового
    // раннера, поэтому здесь проверяется экземплярное поведение (DeselectOnClick
    // находит EventSystem через FindFirstObjectByType, а не через static current).
    // Полный поток с живым StandaloneInputModule проверяется в Play Mode
    // (Assets/Tests/GuiButtonPlayTests.cs).

    private static System.Type DeselectOnClickType()
    {
        var type = System.Type.GetType("DeselectOnClick, Assembly-CSharp");
        Assert.NotNull(type, "тип DeselectOnClick не найден в Assembly-CSharp");
        return type;
    }

    [Test]
    public void DeselectOnClick_PointerClickClearsSelection()
    {
        var buttonGo = new GameObject("TestButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGo.AddComponent(DeselectOnClickType());
        var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        var eventSystem = eventSystemGo.GetComponent<EventSystem>();

        eventSystem.SetSelectedGameObject(buttonGo);
        Assert.AreEqual(buttonGo, eventSystem.currentSelectedGameObject, "кнопка должна быть выбрана перед кликом");

        var clickData = new PointerEventData(eventSystem) { pointerId = -1, button = PointerEventData.InputButton.Left };
        ExecuteEvents.Execute(buttonGo, clickData, ExecuteEvents.pointerClickHandler);

        Assert.Null(eventSystem.currentSelectedGameObject, "после Pointer Click выбор должен быть сброшен");

        Object.DestroyImmediate(buttonGo);
        Object.DestroyImmediate(eventSystemGo);
    }

    [Test]
    public void DeselectOnClick_LeavesOtherSelectionUntouched()
    {
        var selectedGo = new GameObject("OtherSelected", typeof(RectTransform), typeof(Image), typeof(Selectable));
        var buttonGo = new GameObject("TestButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGo.AddComponent(DeselectOnClickType());
        var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        var eventSystem = eventSystemGo.GetComponent<EventSystem>();

        eventSystem.SetSelectedGameObject(selectedGo);
        var clickData = new PointerEventData(eventSystem) { pointerId = -1, button = PointerEventData.InputButton.Left };
        ExecuteEvents.Execute(buttonGo, clickData, ExecuteEvents.pointerClickHandler);

        Assert.AreEqual(selectedGo, eventSystem.currentSelectedGameObject, "чужой выбор клик по другой кнопке не должен трогать");

        Object.DestroyImmediate(selectedGo);
        Object.DestroyImmediate(buttonGo);
        Object.DestroyImmediate(eventSystemGo);
    }
}

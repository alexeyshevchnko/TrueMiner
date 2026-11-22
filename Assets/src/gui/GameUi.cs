using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Программный uGUI: Canvas/EventSystem, HP-бар, всегда-видимый хотбар (9),
/// панель сумки (36) по кнопке/клавише, флаг, FPS.
/// Данные читаются из plain-моделей каждый кадр.
/// </summary>
public class GameUi : MonoBehaviour, IoC.IInitialize
{
    public static GameUi Instance { get; private set; }

    [IoC.Inject]
    public IMapGenerator MapGenerator { set; protected get; }

    private Image hpFill;
    private Text hpText;
    private GameObject bagPanel;
    private Text fpsText;

    private Slot[] hotbar;
    private Slot[] bagSlots;

    private float fpsFrames;
    private float fpsTimer;
    // Last HP text inputs: the label is rebuilt only when one of them changes.
    private int lastHpNow;
    private int lastHpMaxNow;
    private Text lastHpText;

    public class Slot
    {
        public Image bg;
        public Image icon;
        public Text count;
        public BagItemContext context;
    }

    public static void EnsureCreated()
    {
        if (Instance == null)
        {
            new GameObject("GameUi", typeof(GameUi));
        }
    }

    void Awake()
    {
        Instance = this;
        Build();
    }

    void Start()
    {
        AttachJoystickTouchVisibility();
    }

    /// <summary>
    /// Подключает динамическую видимость к существующим джойстикам Ultimate Joystick
    /// (Joystick1 — движение, Joystick2 — прицел) и создаёт зоны TouchArea:
    /// каждая — половина экрана по ширине и вся высота, на канвасе ниже всех
    /// игровых канвасов (routing: интерфейс всегда выигрывает raycast — зоны не
    /// блокируют кнопки/сумку/ячейки). Сами джойстики не пересоздаются; система
    /// управления не изменяется, меняется только отображение и маршрутизация касаний.
    /// </summary>
    void AttachJoystickTouchVisibility()
    {
        const int touchAreaCanvasOrder = -20; // ниже GameUi (-10) и Ultimate UI Canvas (0)

        var touchCanvasGo = new GameObject("TouchAreaCanvas", typeof(RectTransform), typeof(Canvas), typeof(TouchAreaRaycaster));
        var touchCanvas = touchCanvasGo.GetComponent<Canvas>();
        touchCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        touchCanvas.sortingOrder = touchAreaCanvasOrder;
        var touchRect = (RectTransform)touchCanvasGo.transform;
        touchRect.anchorMin = Vector2.zero;
        touchRect.anchorMax = Vector2.one;
        touchRect.offsetMin = Vector2.zero;
        touchRect.offsetMax = Vector2.zero;

        var joystickNames = new[] { "Joystick1", "Joystick2" };
        for (var i = 0; i < joystickNames.Length; i++)
        {
            var joystick = UltimateJoystick.GetUltimateJoystick(joystickNames[i]);
            if (joystick == null)
                continue; // джойстика нет — зона не нужна

            if (joystick.GetComponent<JoystickTouchVisibility>() == null)
                joystick.gameObject.AddComponent<JoystickTouchVisibility>();

            var left = i == 0;
            var area = new GameObject(left ? "TouchArea_Left" : "TouchArea_Right", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            area.transform.SetParent(touchCanvasGo.transform, false);
            var areaRect = (RectTransform)area.transform;
            // Половина экрана по X, вся высота по Y.
            areaRect.anchorMin = new Vector2(left ? 0f : 0.5f, 0f);
            areaRect.anchorMax = new Vector2(left ? 0.5f : 1f, 1f);
            areaRect.offsetMin = Vector2.zero;
            areaRect.offsetMax = Vector2.zero;

            var image = area.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f); // прозрачная зона, применяется TouchAreaRaycaster

            var touchArea = area.AddComponent<JoystickTouchArea>();
            touchArea.Target = joystick;
        }
    }

    public void OnInject()
    {
    }

    void Build()
    {
        var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // Ниже канваса джойстиков (Ultimate UI Canvas, sortingOrder 0): джойстики
        // отображаются поверх сумки и её ячеек (ISSUE-5, задача «джойстики поверх сумки»).
        canvas.sortingOrder = -10;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1024, 768);

        if (EventSystem.current == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).transform.SetParent(transform, false);
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Шрифт Comic (Resources/gui/comic.ttf) — только для цифр количества предметов.
        Font countFont = Resources.Load<Font>("gui/comic");
        if (countFont == null)
        {
            Debug.LogWarning("GameUi: шрифт gui/comic не найден, количество предметов — системным шрифтом");
            countFont = font;
        }

        // HP bar — top-left.
        var hpBg = AddImage(canvasGo.transform, "HpBg", new Color(0f, 0f, 0f, 0.6f));
        SetAnchored(hpBg.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -14f), new Vector2(240f, 28f));
        hpFill = AddImage(hpBg.transform, "HpFill", new Color(0.85f, 0.2f, 0.2f, 1f));
        hpFill.type = Image.Type.Filled;
        hpFill.fillMethod = Image.FillMethod.Horizontal;
        hpFill.fillAmount = 1f;
        Stretch(hpFill.rectTransform);
        hpText = AddText(hpBg.transform, "HpText", font, 18, TextAnchor.MiddleCenter, Color.white);
        Stretch(hpText.rectTransform);

        // Flag button — top-right.
        var flag = AddButton(canvasGo.transform, "Flag", font);
        SetAnchored(flag.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-14f, -14f), new Vector2(110f, 40f));
        flag.onClick.AddListener(() => { if (MapGenerator != null) MapGenerator.ChangeMap(); });

        // Bag button — top-right under flag.
        var bagBtn = AddButton(canvasGo.transform, "Bag", font);
        SetAnchored(bagBtn.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-14f, -60f), new Vector2(110f, 40f));
        bagBtn.onClick.AddListener(ToggleBag);

        // FPS — top-right under bag button.
        fpsText = AddText(canvasGo.transform, "Fps", font, 15, TextAnchor.UpperRight, Color.green);
        SetAnchored(fpsText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-14f, -108f), new Vector2(110f, 20f));

        // Hotbar — always visible, bottom-center (7 slots, масштаб 0.7 от исходного).
        // Модель Bag.Items по-прежнему хранит 9 элементов: панель показывает первые 7,
        // данные 8-го и 9-го слота не удаляются и остаются доступны модели (клавиши 8/9).
        const int hotbarSlots = 7;
        const float hotbarScale = 0.7f;
        float hotbarCell = 66f * hotbarScale;                      // 46.2
        float hotbarSpacing = 6f * hotbarScale;                    // 4.2
        var hotbarPanel = AddImage(canvasGo.transform, "Hotbar", new Color(0f, 0f, 0f, 0.55f));
        // Размеры панели — из той же формулы, что была, умноженной на масштаб:
        // ширина = padding + ячейки*hotbarSlots + промежутки + padding, высота = padding*2 + ячейка.
        float hotbarWidth = 12f * hotbarScale + hotbarSlots * (hotbarCell + hotbarSpacing) + hotbarSpacing + 12f * hotbarScale;
        float hotbarHeight = 6f * hotbarScale + hotbarCell + 6f * hotbarScale;
        SetAnchored(hotbarPanel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(hotbarWidth, hotbarHeight));
        hotbarPanel.rectTransform.pivot = new Vector2(0.5f, 0f);
        var hgrid = hotbarPanel.gameObject.AddComponent<GridLayoutGroup>();
        hgrid.cellSize = new Vector2(hotbarCell, hotbarCell);
        hgrid.spacing = new Vector2(hotbarSpacing, 0f);
        hgrid.padding = new RectOffset(
            Mathf.RoundToInt(12f * hotbarScale), Mathf.RoundToInt(12f * hotbarScale),
            Mathf.RoundToInt(6f * hotbarScale), Mathf.RoundToInt(6f * hotbarScale));
        hgrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        hgrid.constraintCount = hotbarSlots;
        hgrid.childAlignment = TextAnchor.MiddleCenter;

        hotbar = new Slot[hotbarSlots];
        for (int i = 0; i < hotbarSlots; i++)
        {
            hotbar[i] = MakeSlot(hotbarPanel.transform, countFont, hotbarScale);
        }

        // Bag panel — toggled, above the hotbar (36 slots).
        var bp = AddImage(canvasGo.transform, "BagPanel", new Color(0.05f, 0.05f, 0.05f, 0.75f));
        bagPanel = bp.gameObject;
        SetAnchored(bp.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 92f), new Vector2(9f * 70f + 8f * 6f + 24f, 4f * 70f + 3f * 6f + 24f));
        bp.rectTransform.pivot = new Vector2(0.5f, 0f);
        var bgrid = bagPanel.AddComponent<GridLayoutGroup>();
        bgrid.cellSize = new Vector2(66f, 66f);
        bgrid.spacing = new Vector2(6f, 6f);
        bgrid.padding = new RectOffset(12, 12, 12, 12);
        bgrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        bgrid.constraintCount = 9;
        bgrid.childAlignment = TextAnchor.MiddleCenter;

        bagSlots = new Slot[36];
        for (int i = 0; i < 36; i++)
        {
            bagSlots[i] = MakeSlot(bagPanel.transform, countFont, 1f);
        }
        bagPanel.SetActive(false);
    }

    Slot MakeSlot(Transform parent, Font font, float scale)
    {
        var img = AddImage(parent, "Slot", new Color(1f, 1f, 1f, 0.16f));
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        img.gameObject.AddComponent<DeselectOnClick>();

        Slot s = new Slot();
        s.bg = img;

        s.icon = AddImage(img.transform, "Icon", Color.white);
        Stretch(s.icon.rectTransform);

        s.count = AddCountText(img.transform, font, scale);

        btn.onClick.AddListener(() => { if (s.context != null) s.context.Click(); });
        return s;
    }

    void ToggleBag()
    {
        var p = (RootContext.Instance != null) ? RootContext.Instance.Player : null;
        if (p != null && p.TargetManager != null && p.TargetManager.PlayerController != null)
        {
            p.ShowBag();
        }
    }

    void Update()
    {
        var p = (RootContext.Instance != null) ? RootContext.Instance.Player : null;
        if (p != null)
        {
            if (hpFill != null) hpFill.fillAmount = Mathf.Clamp01(p.HpPct);
            if (hpText != null)
            {
                int hpNow = Mathf.RoundToInt(p.HpPct * p.HpMax);
                int hpMaxNow = Mathf.RoundToInt(p.HpMax);
                if (hpNow != lastHpNow || hpMaxNow != lastHpMaxNow || lastHpText == null)
                {
                    lastHpNow = hpNow;
                    lastHpMaxNow = hpMaxNow;
                    if (p.HpMax > 0f) hpText.text = hpNow + "/" + hpMaxNow;
                    else hpText.text = "";
                    lastHpText = hpText;
                }
            }
            if (bagPanel != null && bagPanel.activeSelf != p.IsShowBag) bagPanel.SetActive(p.IsShowBag);
            SyncSlots(hotbar, p.Bag != null ? p.Bag.Items : null);
            // The bag grid is only visible while it is open, so it is refreshed only then.
            if (p.IsShowBag) SyncSlots(bagSlots, p.Bag != null ? p.Bag.BagItems : null);
        }

        // B / Tab открыть-закрыть сумку.
        if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleBag();
        }

        fpsFrames += 1f;
        fpsTimer += Time.unscaledDeltaTime;
        if (fpsTimer >= 0.5f)
        {
            if (fpsText != null) fpsText.text = Mathf.RoundToInt(fpsFrames / fpsTimer) + " fps";
            fpsFrames = 0f;
            fpsTimer = 0f;
        }
    }

    void SyncSlots(Slot[] slots, List<BagItemContext> list)
    {
        if (slots == null) return;
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            var ctx = (list != null && i < list.Count) ? list[i] : null;
            s.context = ctx;
            if (s.count != null) s.count.text = (ctx != null && !ctx.Empty) ? ctx.Count.ToString() : "";
            if (s.icon != null) UpdateIcon(s, ctx);
            if (s.bg != null) s.bg.color = (ctx != null && ctx.BgIcon == "cellSelect") ? new Color(1f, 0.9f, 0.2f, 0.5f) : new Color(1f, 1f, 1f, 0.16f);
        }
    }

    void UpdateIcon(Slot s, BagItemContext ctx)
    {
        var sprite = (ctx != null && !ctx.Empty && !string.IsNullOrEmpty(ctx.Icon))
            ? GuiAtlas.Get(ctx.Icon)
            : null;
        // Переназначаем только при фактической смене, чтобы не пересобирать Graphic каждый кадр.
        if (!ReferenceEquals(s.icon.sprite, sprite)) s.icon.sprite = sprite;
        var color = sprite != null ? Color.white : Color.clear;
        if (s.icon.color != color) s.icon.color = color;
    }

    // ---- helpers ----
    // Текст количества предметов: шрифт Comic + чёрный контур (Outline).
    // BestFit вписывает любое количество цифр в ячейку без обрезания;
    // выравнивание в правом нижнем углу не закрывает центр иконки.
    // scale — множитель оформления (0.7 для хотбара, 1 для сумки).
    static Text AddCountText(Transform parent, Font font, float scale)
    {
        var t = AddText(parent, "Count", font, Mathf.RoundToInt(20f * scale), TextAnchor.LowerRight, Color.white);
        SetAnchored(t.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-4f * scale, 4f * scale), new Vector2(52f * scale, 24f * scale));
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize = Mathf.RoundToInt(10f * scale);
        t.resizeTextMaxSize = Mathf.RoundToInt(20f * scale);
        var outline = t.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 1f);
        outline.effectDistance = new Vector2(1.5f * scale, -1.5f * scale);
        return t;
    }

    static Image AddImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    static Text AddText(Transform parent, string name, Font font, int size, TextAnchor anchor, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = color;
        t.raycastTarget = false;
        return t;
    }

    static Button AddButton(Transform parent, string label, Font font)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(0.2f, 0.4f, 0.8f, 0.85f);
        var txt = AddText(go.transform, "Label", font, 20, TextAnchor.MiddleCenter, Color.white);
        Stretch(txt.rectTransform);
        var btn = go.GetComponent<Button>();
        // Navigation.None: uGUI не делает кнопку selected при pointer down, поэтому после
        // клика мышью/тачем она не остаётся выбранной и пробел (Submit) больше не активирует
        // её. Навигация остальных элементов интерфейса не затронута.
        var nav = new Navigation();
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;
        // Страховка: если выбор каким-то путём всё же установился — сбрасываем после клика.
        btn.gameObject.AddComponent<DeselectOnClick>();
        return btn;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void SetAnchored(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(
            Mathf.Clamp01((anchorMin.x + anchorMax.x) / 2f),
            Mathf.Clamp01((anchorMin.y + anchorMax.y) / 2f));
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
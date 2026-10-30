#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Creates a reusable ProjectSpark Shop prefab with a complete responsive UGUI layout.
    /// Visual/UI only: purchase logic must be connected to the economy service separately.
    /// Requires TextMeshPro and Unity UI packages.
    /// </summary>
    public static class ProjectSparkShopPanelBuilder
    {
        private const string PrefabPath = "Assets/ProjectSpark/GeneratedUI/ProjectSparkShopPanel.prefab";
        private static readonly Color Navy = Hex("#07152B");
        private static readonly Color Panel = Hex("#0C2242");
        private static readonly Color Tile = Hex("#102D50");
        private static readonly Color Edge = Hex("#24659B");
        private static readonly Color Cyan = Hex("#12CFF7");
        private static readonly Color White = Hex("#F1F8FF");
        private static readonly Color Muted = Hex("#9DBCD7");
        private static readonly Color Gold = Hex("#FFC84A");
        private static readonly Color Green = Hex("#2DCB52");

        [MenuItem("Tools/ProjectSpark/UI/Create Complete Shop Panel")]
        public static void CreateShop()
        {
            var canvasGO = NewUI("ProjectSparkShopPanel", null);
            try
            {
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 50;
                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                canvasGO.AddComponent<GraphicRaycaster>();

                var backdrop = Rect("Backdrop", canvasGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                ImageOf(backdrop, new Color(0.005f, 0.015f, 0.04f, 0.82f));
                var window = Rect("ShopWindow", backdrop.transform, new Vector2(0.045f, 0.055f), new Vector2(0.955f, 0.945f), Vector2.zero, Vector2.zero);
                ImageOf(window, Navy);
                AddOutline(window, Edge, new Vector2(3f, -3f));

                var header = Rect("Header", window.transform, new Vector2(0, 0.885f), Vector2.one, new Vector2(16, 0), new Vector2(-16, -12));
                ImageOf(header, Panel);
                Label("Title", header.transform, "◈  PROJECT SPARK  /  SHOP", 42, White, new Vector2(0.015f, 0.12f), new Vector2(0.31f, 0.9f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
                var tabs = new[] { "COMPONENTS", "TOOLS", "KITS", "DECOR" };
                for (int i = 0; i < tabs.Length; i++)
                {
                    float left = 0.315f + i * 0.112f;
                    Button("Tab_" + tabs[i], header.transform, tabs[i], new Vector2(left, 0.20f), new Vector2(left + 0.105f, 0.82f), i == 0 ? Hex("#087BCD") : Tile, White, 20);
                }
                Label("CoinBalance", header.transform, "●  1,250", 27, Gold, new Vector2(0.765f, .12f), new Vector2(.85f, .9f), TextAlignmentOptions.Center, FontStyles.Bold);
                Label("DiamondBalance", header.transform, "◆  25", 25, Cyan, new Vector2(.85f, .12f), new Vector2(.925f, .9f), TextAlignmentOptions.Center, FontStyles.Bold);
                Button("CloseButton", header.transform, "✕", new Vector2(.939f, .18f), new Vector2(.988f, .86f), Hex("#BB3049"), White, 34);

                var body = Rect("Body", window.transform, Vector2.zero, new Vector2(1, .88f), new Vector2(16, 16), new Vector2(-16, 0));

                var sidebar = Rect("Categories", body.transform, new Vector2(0, 0), new Vector2(.16f, 1), Vector2.zero, new Vector2(-8, 0));
                ImageOf(sidebar, Panel);
                AddOutline(sidebar, Edge, new Vector2(1f, -1f));
                string[] categories = { "ALL", "RESISTORS", "CAPACITORS", "DIODES", "TRANSISTORS", "ICs", "SWITCHES", "POWER", "CONNECTORS", "LEDs", "MOTORS", "TOOLS", "OTHER" };
                for (int i = 0; i < categories.Length; i++)
                {
                    float top = .975f - i * .073f;
                    Button("Category_" + categories[i], sidebar.transform, categories[i], new Vector2(.045f, top - .065f), new Vector2(.955f, top), i == 0 ? Hex("#0576BB") : Tile, White, 20);
                }

                var middle = Rect("Catalog", body.transform, new Vector2(.165f, 0), new Vector2(.795f, 1), new Vector2(0, 0), new Vector2(-8, 0));
                ImageOf(middle, Panel);
                AddOutline(middle, Edge, new Vector2(1f, -1f));
                var toolbar = Rect("Toolbar", middle.transform, new Vector2(.012f, .90f), new Vector2(.988f, .985f), Vector2.zero, Vector2.zero);
                ImageOf(toolbar, Navy);
                Label("SearchPlaceholder", toolbar.transform, "⌕   Search components...", 23, Muted, new Vector2(.015f, 0), new Vector2(.51f, 1), TextAlignmentOptions.MidlineLeft);
                Button("TypeFilter", toolbar.transform, "ALL TYPES  ▾", new Vector2(.52f, .10f), new Vector2(.70f, .90f), Tile, White, 18);
                Button("SortButton", toolbar.transform, "SORT: NAME  ▾", new Vector2(.71f, .10f), new Vector2(.93f, .90f), Tile, White, 18);

                var scrollGO = Rect("ProductScroll", middle.transform, new Vector2(.012f, .012f), new Vector2(.988f, .887f), Vector2.zero, Vector2.zero);
                var scroll = scrollGO.gameObject.AddComponent<ScrollRect>();
                scroll.horizontal = false;
                scroll.vertical = true;
                scroll.movementType = ScrollRect.MovementType.Clamped;
                var viewport = Rect("Viewport", scrollGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                ImageOf(viewport, new Color(1,1,1,.002f));
                viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
                var content = Rect("Content", viewport.transform, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero);
                content.pivot = new Vector2(.5f, 1);
                content.sizeDelta = new Vector2(0, 1500);
                var grid = content.gameObject.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(180, 223);
                grid.spacing = new Vector2(13, 14);
                grid.padding = new RectOffset(10, 10, 12, 12);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 4;
                grid.childAlignment = TextAnchor.UpperCenter;
                scroll.viewport = viewport;
                scroll.content = content;

                var goods = new (string name, string band, int price)[] {
                    ("Resistor 100 Ω", "BROWN · BLACK · BROWN", 10),
                    ("Resistor 150 Ω", "BROWN · GREEN · BROWN", 10),
                    ("Resistor 220 Ω", "RED · RED · BROWN", 12),
                    ("Resistor 330 Ω", "ORANGE · ORANGE · BROWN", 12),
                    ("Resistor 470 Ω", "YELLOW · VIOLET · BROWN", 15),
                    ("Resistor 1 kΩ", "BROWN · BLACK · RED", 15),
                    ("Capacitor 100 nF", "CERAMIC CAPACITOR", 8),
                    ("Capacitor 1 µF", "ELECTROLYTIC", 8),
                    ("Capacitor 10 µF", "ELECTROLYTIC", 12),
                    ("Capacitor 100 µF", "ELECTROLYTIC", 12),
                    ("Diode 1N4148", "SIGNAL DIODE", 8),
                    ("Diode 1N4007", "RECTIFIER", 8),
                    ("LED Red", "LIGHT EMITTING DIODE", 10),
                    ("LED Green", "LIGHT EMITTING DIODE", 10),
                    ("LED Blue", "LIGHT EMITTING DIODE", 12),
                    ("Push Button", "MOMENTARY SWITCH", 12),
                    ("Toggle Switch", "SPST SWITCH", 15),
                    ("Potentiometer", "10 kΩ VARIABLE", 20)
                };
                foreach (var g in goods)
                    ProductCard(content, g.name, g.band, g.price);

                var details = Rect("SelectedItemDetails", body.transform, new Vector2(.80f, 0), new Vector2(1, 1), new Vector2(0, 0), Vector2.zero);
                ImageOf(details, Panel);
                AddOutline(details, Edge, new Vector2(1f, -1f));
                Label("SelectedTitle", details.transform, "Resistor 100 Ω", 29, White, new Vector2(.04f, .925f), new Vector2(.96f, .99f), TextAlignmentOptions.Left, FontStyles.Bold);
                var preview = Rect("ItemPreview", details.transform, new Vector2(.045f, .58f), new Vector2(.955f, .914f), Vector2.zero, Vector2.zero);
                ImageOf(preview, Tile);
                AddOutline(preview, Edge, new Vector2(1f, -1f));
                Label("PreviewPlaceholder", preview.transform, "RESISTOR\n100 Ω", 50, Gold, Vector2.zero, Vector2.one, TextAlignmentOptions.Center, FontStyles.Bold);
                Label("Specs", details.transform, "Resistance                  100 Ω\n\nTolerance                    ±5%\n\nMax Power                 0.25 W\n\nColor Bands\nBrown · Black · Brown · Gold", 20, White, new Vector2(.06f, .275f), new Vector2(.94f, .56f), TextAlignmentOptions.TopLeft);
                Label("Description", details.transform, "A fixed resistor for current limiting and voltage division.", 20, Muted, new Vector2(.06f, .17f), new Vector2(.94f, .275f), TextAlignmentOptions.TopLeft);
                Button("DecreaseQuantity", details.transform, "−", new Vector2(.06f, .09f), new Vector2(.29f, .16f), Tile, White, 31);
                Label("Quantity", details.transform, "1", 27, White, new Vector2(.31f, .09f), new Vector2(.69f, .16f), TextAlignmentOptions.Center, FontStyles.Bold);
                Button("IncreaseQuantity", details.transform, "+", new Vector2(.71f, .09f), new Vector2(.94f, .16f), Tile, White, 31);
                Button("BuyButton", details.transform, "BUY FOR 10 COINS", new Vector2(.045f, .008f), new Vector2(.955f, .082f), Green, White, 25);

                EnsureFolder("Assets/ProjectSpark");
                EnsureFolder("Assets/ProjectSpark/GeneratedUI");
                PrefabUtility.SaveAsPrefabAsset(canvasGO, PrefabPath);
                Undo.RegisterCreatedObjectUndo(canvasGO, "Create ProjectSpark Shop Panel");
                Selection.activeGameObject = canvasGO;
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
                Debug.Log("[ProjectSpark] Shop panel created in scene and prefab saved: " + PrefabPath);
            }
            catch (Exception ex)
            {
                UnityEngine.Object.DestroyImmediate(canvasGO);
                Debug.LogException(ex);
                throw;
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static GameObject NewUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = NewUI(name, parent);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return rt;
        }

        private static Image ImageOf(RectTransform rect, Color color)
        {
            var img = rect.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private static void AddOutline(RectTransform rect, Color color, Vector2 offset)
        {
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = offset;
        }

        private static TextMeshProUGUI Label(string name, Transform parent, string value, int size, Color color,
            Vector2 min, Vector2 max, TextAlignmentOptions align, FontStyles style = FontStyles.Normal)
        {
            var r = Rect(name, parent, min, max, Vector2.zero, Vector2.zero);
            var label = r.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = value;
            label.fontSize = size;
            label.color = color;
            label.alignment = align;
            label.fontStyle = style;
            label.enableWordWrapping = true;
            label.raycastTarget = false;
            return label;
        }

        private static Button Button(string name, Transform parent, string value, Vector2 min, Vector2 max, Color color, Color textColor, int fontSize)
        {
            var rect = Rect(name, parent, min, max, Vector2.zero, Vector2.zero);
            ImageOf(rect, color);
            AddOutline(rect, Edge, new Vector2(1, -1));
            var btn = rect.gameObject.AddComponent<Button>();
            var text = Label("Text", rect.transform, value, fontSize, textColor, Vector2.zero, Vector2.one, TextAlignmentOptions.Center, FontStyles.Bold);
            text.margin = new Vector4(5, 2, 5, 2);
            return btn;
        }

        private static void ProductCard(Transform parent, string item, string subtitle, int price)
        {
            var card = NewUI("Item_" + item.Replace(' ', '_'), parent);
            var r = card.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(180, 223);
            ImageOf(r, Tile);
            AddOutline(r, Edge, new Vector2(1, -1));
            var preview = Rect("IconPlaceholder", r, new Vector2(.06f, .36f), new Vector2(.94f, .92f), Vector2.zero, Vector2.zero);
            ImageOf(preview, Navy);
            string glyph = item.StartsWith("Resistor") ? "Ω" : item.StartsWith("LED") ? "●" : item.StartsWith("Capacitor") ? "▣" : "◇";
            Label("Glyph", preview.transform, glyph, 58, item.StartsWith("LED") ? Green : Cyan, Vector2.zero, Vector2.one, TextAlignmentOptions.Center, FontStyles.Bold);
            Label("ItemName", r, item, 19, White, new Vector2(.025f, .235f), new Vector2(.975f, .355f), TextAlignmentOptions.Center, FontStyles.Bold);
            Label("Subtitle", r, subtitle, 11, Muted, new Vector2(.025f, .165f), new Vector2(.975f, .24f), TextAlignmentOptions.Center);
            Label("Price", r, "● " + price, 20, Gold, new Vector2(.06f, .03f), new Vector2(.55f, .15f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            Button("CardBuyButton", r, "+", new Vector2(.69f, .025f), new Vector2(.95f, .165f), Green, White, 26);
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
#endif

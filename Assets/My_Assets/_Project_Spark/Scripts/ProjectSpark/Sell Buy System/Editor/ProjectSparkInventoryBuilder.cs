#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.UI;

namespace ProjectSpark.EditorTools
{
    public static class ProjectSparkInventoryBuilder
    {
        private const string PrefabPath = "Assets/ProjectSpark/GeneratedUI/ProjectSparkInventoryPanel.prefab";
        private static readonly Color Background = RGB(8, 20, 40);
        private static readonly Color Panel = RGB(14, 37, 64);
        private static readonly Color Tile = RGB(20, 55, 86);
        private static readonly Color Cyan = RGB(36, 211, 245);
        private static readonly Color White = RGB(234, 246, 255);
        private static readonly Color Gold = RGB(255, 195, 61);

        [MenuItem("Tools/ProjectSpark/UI/Create Inventory Panel")]
        public static void Create()
        {
            GameObject root = Node("ProjectSparkInventoryPanel", null);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 65;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            root.AddComponent<GraphicRaycaster>();
            GameObject backdrop = Area("Backdrop", root.transform, Vector2.zero, Vector2.one);
            ImageOf(backdrop, new Color(0, 0, .02f, .86f));
            GameObject window = Area("InventoryWindow", backdrop.transform, new Vector2(.10f, .08f), new Vector2(.90f, .92f));
            ImageOf(window, Background);
            Outline frame = window.AddComponent<Outline>(); frame.effectColor = Cyan; frame.effectDistance = new Vector2(2, -2);
            GameObject header = Area("Header", window.transform, new Vector2(0, .87f), Vector2.one);
            ImageOf(header, Panel);
            Text("Title", header.transform, "PROJECT SPARK   /   INVENTORY", 38, White, new Vector2(.025f, .1f), new Vector2(.49f, .95f));
            Text("CoinBalance", header.transform, "COINS  0", 24, Gold, new Vector2(.51f, .1f), new Vector2(.70f, .95f));
            Text("DiamondBalance", header.transform, "DIAMONDS  0", 24, Cyan, new Vector2(.71f, .1f), new Vector2(.91f, .95f));
            Button("CloseButton", header.transform, "X", new Vector2(.93f, .15f), new Vector2(.986f, .85f), RGB(170, 48, 65));
            GameObject body = Area("Body", window.transform, Vector2.zero, new Vector2(1, .86f));
            GameObject search = Area("Search", body.transform, new Vector2(.025f, .91f), new Vector2(.66f, .985f));
            ImageOf(search, Tile);
            TMP_InputField input = search.AddComponent<TMP_InputField>();
            TMP_Text placeholder = Text("Placeholder", search.transform, "SEARCH OWNED COMPONENTS...", 22, new Color(.59f,.76f,.86f), new Vector2(.02f,0), new Vector2(.98f,1));
            TMP_Text typed = Text("Text", search.transform, "", 22, White, new Vector2(.02f,0), new Vector2(.98f,1));
            input.textViewport = search.GetComponent<RectTransform>(); input.textComponent = (TextMeshProUGUI)typed; input.placeholder = placeholder;
            GameObject scrollRoot = Area("ItemScroll", body.transform, new Vector2(.025f, .035f), new Vector2(.665f, .895f));
            ImageOf(scrollRoot, Panel);
            ScrollRect scroll = scrollRoot.AddComponent<ScrollRect>(); scroll.horizontal = false;
            GameObject viewport = Area("Viewport", scrollRoot.transform, Vector2.zero, Vector2.one);
            viewport.AddComponent<RectMask2D>();
            GameObject content = Area("Content", viewport.transform, new Vector2(0, 1), new Vector2(1, 1));
            RectTransform contentRect = content.GetComponent<RectTransform>(); contentRect.pivot = new Vector2(.5f, 1); contentRect.sizeDelta = new Vector2(0, 400);
            GridLayoutGroup grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(205, 228); grid.spacing = new Vector2(15, 15); grid.padding = new RectOffset(18, 18, 18, 18);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 4;
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport.GetComponent<RectTransform>(); scroll.content = contentRect;
            GameObject template = Area("ItemCardTemplate", content.transform, Vector2.zero, Vector2.one);
            ImageOf(template, Tile); Button card = template.AddComponent<Button>(); card.targetGraphic = template.GetComponent<Image>();
            GameObject cardIcon = Area("Icon", template.transform, new Vector2(.19f, .30f), new Vector2(.81f, .95f));
            Image iconImage = ImageOf(cardIcon, Color.white); iconImage.preserveAspect = true; iconImage.enabled = false;
            Text("Name", template.transform, "Resistor 220 Ω", 20, White, new Vector2(.06f,.15f),new Vector2(.94f,.32f));
            Text("Count", template.transform, "OWNED ×1", 17, Gold, new Vector2(.06f,.02f),new Vector2(.94f,.16f));
            template.SetActive(false);
            GameObject details = Area("Details", body.transform, new Vector2(.685f,.035f), new Vector2(.975f,.985f));
            ImageOf(details, Panel);
            Text("SelectedName", details.transform, "SELECT A COMPONENT", 29, White, new Vector2(.06f,.88f),new Vector2(.94f,.98f));
            GameObject preview = Area("SelectedIcon", details.transform,new Vector2(.12f,.46f),new Vector2(.88f,.85f));
            Image previewImage = ImageOf(preview, Color.white); previewImage.preserveAspect = true; previewImage.enabled = false;
            Text("SelectedCount", details.transform, "Owned: 0", 25, Gold, new Vector2(.06f,.37f),new Vector2(.94f,.46f));
            Text("StatusText", details.transform, "Choose a component to deliver.", 19, Cyan, new Vector2(.06f,.19f),new Vector2(.94f,.34f));
            Button("DeliverButton", details.transform, "DELIVER TO SHELF", new Vector2(.06f,.06f),new Vector2(.94f,.17f), RGB(26,166,90));
            root.AddComponent<ProjectSparkInventoryPanel>();
            EnsureFolder("Assets/ProjectSpark/GeneratedUI");
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Selection.activeGameObject = root;
            Undo.RegisterCreatedObjectUndo(root, "Create ProjectSpark Inventory Panel");
            Debug.Log("[ProjectSpark] Inventory generated and prefab saved: " + PrefabPath, root);
        }

        private static GameObject Node(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }
        private static GameObject Area(string name, Transform parent, Vector2 min, Vector2 max)
        {
            GameObject go = Node(name, parent);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = rt.offsetMax = Vector2.zero;
            return go;
        }
        private static Image ImageOf(GameObject go, Color color)
        {
            Image image = go.AddComponent<Image>(); image.color = color; return image;
        }
        private static TMP_Text Text(string name, Transform parent, string value, float size, Color color, Vector2 min, Vector2 max)
        {
            GameObject go = Area(name,parent,min,max);
            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.text = value; label.fontSize = size; label.color = color;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.enableWordWrapping = true;
            label.raycastTarget = false;
            return label;
        }
        private static Button Button(string name, Transform parent, string value, Vector2 min, Vector2 max, Color color)
        {
            GameObject go = Area(name,parent,min,max); Image image = ImageOf(go,color);
            Button button = go.AddComponent<Button>(); button.targetGraphic = image;
            Text(name == "CloseButton" ? "Text" : "Label",go.transform,value,23,White,new Vector2(.06f,.05f),new Vector2(.94f,.95f)).alignment = TextAlignmentOptions.Center;
            return button;
        }
        private static Color RGB(byte r, byte g, byte b) => new Color32(r,g,b,255);
        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\','/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent,Path.GetFileName(folder));
        }
    }
}
#endif

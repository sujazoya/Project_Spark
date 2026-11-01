#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;

namespace ProjectSpark.UI.VisualTeaching.Editor
{
    /// <summary>
    /// One-click additive meter UI. Does not create fake solver data or overwrite
    /// game systems. Probe marker commands operate only on selected GameObjects.
    /// </summary>
    public static class SparkVirtualMultimeterBuilder
    {
        private static readonly Color Dark = new Color(.022f, .070f, .120f, .98f);
        private static readonly Color Tile = new Color(.052f, .145f, .210f, 1f);
        private static readonly Color Screen = new Color(.012f, .105f, .115f, 1f);
        private static readonly Color Accent = new Color(.30f, .89f, .99f, 1f);
        private static readonly Color TextColor = new Color(.93f, .98f, 1f, 1f);
        private static readonly Color Muted = new Color(.65f, .79f, .86f, 1f);
        private static readonly Color ButtonBlue = new Color(.085f, .31f, .44f, 1f);
        private static readonly Color Red = new Color(.95f, .23f, .25f, 1f);
        private static readonly Color Black = new Color(.32f, .42f, .49f, 1f);

        [MenuItem("Tools/Project Spark/Circuit/Create Advanced Virtual Multimeter")]
        public static void Create()
        {
            SparkVirtualMultimeter existing = Object.FindFirstObjectByType<SparkVirtualMultimeter>(FindObjectsInactive.Include);
            if (existing != null)
            {
                UpgradeExisting(existing);
                return;
            }

            var publisher = Object.FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            var nodeFeed = Object.FindFirstObjectByType<SparkMeterVerifiedNodeFeed>(FindObjectsInactive.Include);
            var level = Object.FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);

            GameObject root = new GameObject("SparkVirtualMultimeterCanvas", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(root, "Create Spark Virtual Multimeter");
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 780;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;

            GameObject openGo = UI("OpenMultimeter", root.transform);
            TopRight(openGo.GetComponent<RectTransform>(), -32, -28, 215, 43);
            Button open = AddButton(openGo, "VIRTUAL METER", 14, ButtonBlue);

            GameObject panel = UI("MultimeterPanel", root.transform);
            TopRight(panel.GetComponent<RectTransform>(), -31, -83, 588, 685);
            Image bg = panel.AddComponent<Image>();
            bg.color = Dark;
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(.29f, .77f, .88f, .6f);
            outline.effectDistance = new Vector2(1, -1);

            Label(panel.transform, "Title", "PROJECT SPARK  /  ADVANCED MULTIMETER", 16, Accent,
                19, -15, 512, 25);
            Button close = MakeButton(panel.transform, "Close", "X", 542, -12, 31, 29);
            TMP_Text source = Label(panel.transform, "Source", "SOLVER: WAITING", 12, Muted,
                20, -48, 554, 25);

            Block(panel.transform, "Screen", 20, -80, 548, 142, Screen);
            TMP_Text mode = Label(panel.transform, "MeterMode", "DC VOLTAGE", 13, Accent,
                34, -90, 520, 27);
            TMP_Text digits = Label(panel.transform, "MeasuredValue", "— — —", 36, TextColor,
                33, -121, 520, 53);
            digits.fontStyle = FontStyles.Bold;
            TMP_Text units = Label(panel.transform, "EvidenceType", "Waiting for a verified measurement", 12, Muted,
                35, -183, 518, 30);

            Button volts = MakeButton(panel.transform, "VoltsMode", "DC V", 20, -240, 170, 42);
            Button amps = MakeButton(panel.transform, "AmpsMode", "COMPONENT A", 199, -240, 170, 42);
            Button ohms = MakeButton(panel.transform, "OhmsMode", "MODEL Ω", 378, -240, 190, 42);

            Block(panel.transform, "ProbePanel", 20, -297, 548, 135, Tile);
            TMP_Text redLabel = Label(panel.transform, "RedProbeLabel", "RED  —", 14, Red,
                32, -308, 520, 27);
            TMP_Text blackLabel = Label(panel.transform, "BlackProbeLabel", "BLACK  —", 14, TextColor,
                32, -342, 520, 27);
            Button red = MakeButton(panel.transform, "ArmRed", "PLACE RED PROBE", 31, -381, 169, 38, Red);
            Button black = MakeButton(panel.transform, "ArmBlack", "PLACE BLACK PROBE", 207, -381, 177, 38, Black);
            Button clear = MakeButton(panel.transform, "Clear", "CLEAR", 392, -381, 165, 38);

            TMP_Text guidance = Label(panel.transform, "Guidance", "Click a probe button, then a marked terminal collider.",
                13, TextColor, 24, -446, 540, 56);
            Block(panel.transform, "ComparisonBackground", 20, -509, 548, 81, Tile);
            TMP_Text comparison = Label(panel.transform, "Comparison", "EXPERIMENT  /  Capture A, change circuit, solve, capture B.",
                13, TextColor, 32, -516, 523, 66);
            Button captureA = MakeButton(panel.transform, "CaptureA", "CAPTURE A", 20, -600, 133, 39);
            Button captureB = MakeButton(panel.transform, "CaptureB", "CAPTURE B", 160, -600, 133, 39);
            Button hold = MakeButton(panel.transform, "Hold", "HOLD / LIVE", 300, -600, 129, 39);
            Button reset = MakeButton(panel.transform, "Reset", "RESET PROBES", 436, -600, 132, 39);
            TMP_Text footer = Label(panel.transform, "Footer", "READ ONLY • No injected signals", 12, Muted,
                20, -651, 546, 26);

            var controller = Undo.AddComponent<SparkVirtualMultimeter>(root);
            var so = new SerializedObject(controller);
            Set(so, "publisher", publisher);
            Set(so, "nodeFeed", nodeFeed);
            Set(so, "levelManager", level);
            Set(so, "sceneCamera", Camera.main);
            Set(so, "panel", panel);
            Set(so, "modeText", mode);
            Set(so, "readingText", digits);
            Set(so, "unitText", units);
            Set(so, "redProbeText", redLabel);
            Set(so, "blackProbeText", blackLabel);
            Set(so, "sourceText", source);
            Set(so, "guidanceText", guidance);
            Set(so, "comparisonText", comparison);
            Set(so, "footerText", footer);
            Set(so, "captureAButton", captureA);
            Set(so, "captureBButton", captureB);
            so.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(open.onClick, controller.Toggle);
            UnityEventTools.AddPersistentListener(close.onClick, controller.Hide);
            UnityEventTools.AddPersistentListener(volts.onClick, controller.SetVolts);
            UnityEventTools.AddPersistentListener(amps.onClick, controller.SetAmps);
            UnityEventTools.AddPersistentListener(ohms.onClick, controller.SetOhms);
            UnityEventTools.AddPersistentListener(red.onClick, controller.ArmRedProbe);
            UnityEventTools.AddPersistentListener(black.onClick, controller.ArmBlackProbe);
            UnityEventTools.AddPersistentListener(clear.onClick, controller.ClearProbes);
            UnityEventTools.AddPersistentListener(reset.onClick, controller.ClearProbes);
            UnityEventTools.AddPersistentListener(captureA.onClick, controller.CaptureA);
            UnityEventTools.AddPersistentListener(captureB.onClick, controller.CaptureB);
            UnityEventTools.AddPersistentListener(hold.onClick, controller.ToggleHold);

            panel.SetActive(false);
            UpgradeExisting(controller);
            Selection.activeGameObject = root;
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[Spark Multimeter] UI created. " +
                (publisher == null ? "WARNING: No SparkVerifiedElectricalPublisher found. " : "Publisher assigned. ") +
                "Mark physical terminal colliders using Tools > Project Spark > Circuit > Meter Probes.", root);
        }

        [MenuItem("Tools/Project Spark/Circuit/Upgrade Existing Multimeter to 3 Modes")]
        public static void UpgradeSelectedMeter()
        {
            SparkVirtualMultimeter meter = Object.FindFirstObjectByType<SparkVirtualMultimeter>(FindObjectsInactive.Include);
            if (meter == null)
            {
                Debug.LogWarning("[Spark Multimeter] Create the Advanced Virtual Multimeter first.");
                return;
            }
            UpgradeExisting(meter);
        }

        // Idempotent: augments the existing meter, retains its source references,
        // its original 12 button actions, and its full-size panel.
        private static void UpgradeExisting(SparkVirtualMultimeter meter)
        {
            Transform root = meter.transform;
            Transform fullTransform = root.Find("MultimeterPanel");
            if (fullTransform == null)
            {
                Debug.LogError("[Spark Multimeter] MultimeterPanel is missing. Existing UI not modified.", meter);
                return;
            }

            GameObject expanded = fullTransform.gameObject;
            CanvasGroup expandedGroup = expanded.GetComponent<CanvasGroup>();
            if (expandedGroup == null) expandedGroup = Undo.AddComponent<CanvasGroup>(expanded);

            Transform title = expanded.transform.Find("Title");
            if (title != null)
            {
                var titleLabel = title.GetComponent<TMP_Text>();
                if (titleLabel != null)
                {
                    Undo.RecordObject(titleLabel, "Format Multimeter Title");
                    titleLabel.text = "PROJECT SPARK  /  MULTIMETER";
                }
            }

            Transform minimizeNode = expanded.transform.Find("Minimize");
            if (minimizeNode == null)
            {
                Button minimize = MakeButton(expanded.transform,
                    "Minimize", "SMALL", 445, -12, 87, 29);
                Undo.RegisterCreatedObjectUndo(minimize.gameObject, "Add Multimeter Small Button");
                UnityEventTools.AddPersistentListener(minimize.onClick, meter.Minimize);
            }

            Transform smallNode = root.Find("MultimeterSmallPanel");
            GameObject compact;
            if (smallNode == null)
            {
                compact = UI("MultimeterSmallPanel", root);
                Undo.RegisterCreatedObjectUndo(compact, "Add Compact Multimeter");
                TopRight(compact.GetComponent<RectTransform>(), -31, -83, 420, 288);
                var bg = compact.AddComponent<Image>();
                bg.color = Dark;
                var outline = compact.AddComponent<Outline>();
                outline.effectColor = new Color(.29f, .77f, .88f, .65f);
                outline.effectDistance = new Vector2(1f, -1f);

                Label(compact.transform, "CompactTitle", "SPARK / VIRTUAL METER", 14, Accent,
                    17, -14, 270, 22);
                Button expand = MakeButton(compact.transform, "Expand", "EXPAND", 294, -10, 80, 29);
                Button hide = MakeButton(compact.transform, "Hide", "X", 378, -10, 28, 29);

                Block(compact.transform, "CompactScreen", 16, -48, 388, 143, Screen);
                Label(compact.transform, "CompactMode", "DC VOLTAGE", 12, Accent,
                    28, -56, 356, 25);
                TMP_Text digits = Label(compact.transform, "CompactReading", "— — —", 33,
                    TextColor, 28, -87, 359, 59);
                digits.fontStyle = FontStyles.Bold;
                digits.alignment = TextAlignmentOptions.MidlineLeft;
                digits.enableAutoSizing = true;
                digits.fontSizeMin = 21;
                digits.fontSizeMax = 33;
                Label(compact.transform, "CompactEvidence", "NO VERIFIED READING", 12,
                    Muted, 29, -157, 354, 24);
                Label(compact.transform, "CompactProbes", "R: —    B: —", 12,
                    TextColor, 20, -196, 385, 22);

                Button volt = MakeButton(compact.transform, "CompactVolts", "V", 16, -230, 48, 32);
                Button amps = MakeButton(compact.transform, "CompactAmps", "A", 69, -230, 48, 32);
                Button ohms = MakeButton(compact.transform, "CompactOhms", "Ω", 122, -230, 48, 32);
                Button red = MakeButton(compact.transform, "CompactRed", "RED", 180, -230, 65, 32, Red);
                Button black = MakeButton(compact.transform, "CompactBlack", "BLACK", 250, -230, 68, 32, Black);
                Button hold = MakeButton(compact.transform, "CompactHold", "HOLD", 323, -230, 81, 32);

                UnityEventTools.AddPersistentListener(expand.onClick, meter.ShowExpanded);
                UnityEventTools.AddPersistentListener(hide.onClick, meter.Hide);
                UnityEventTools.AddPersistentListener(volt.onClick, meter.SetVolts);
                UnityEventTools.AddPersistentListener(amps.onClick, meter.SetAmps);
                UnityEventTools.AddPersistentListener(ohms.onClick, meter.SetOhms);
                UnityEventTools.AddPersistentListener(red.onClick, meter.ArmRedProbe);
                UnityEventTools.AddPersistentListener(black.onClick, meter.ArmBlackProbe);
                UnityEventTools.AddPersistentListener(hold.onClick, meter.ToggleHold);

                compact.SetActive(false);
            }
            else
            {
                compact = smallNode.gameObject;
            }

            CanvasGroup compactGroup = compact.GetComponent<CanvasGroup>();
            if (compactGroup == null) compactGroup = Undo.AddComponent<CanvasGroup>(compact);

            Transform openNode = root.Find("OpenMultimeter");
            TMP_Text tabText = openNode != null ? openNode.GetComponentInChildren<TMP_Text>(true) : null;
            var so = new SerializedObject(meter);
            Set(so, "panel", expanded);
            Set(so, "smallPanel", compact);
            Set(so, "smallReadingText", GetText(compact.transform, "CompactReading"));
            Set(so, "smallModeText", GetText(compact.transform, "CompactMode"));
            Set(so, "smallUnitText", GetText(compact.transform, "CompactEvidence"));
            Set(so, "smallProbeText", GetText(compact.transform, "CompactProbes"));
            Set(so, "drawerTabText", tabText);
            Set(so, "expandedCanvasGroup", expandedGroup);
            Set(so, "smallCanvasGroup", compactGroup);
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(meter);
            EditorSceneManager.MarkSceneDirty(meter.gameObject.scene);
            Selection.activeGameObject = meter.gameObject;
            Debug.Log("[Spark Multimeter] Three modes installed: Hidden / Small / Expanded. " +
                "Verified reading transitions enabled. Existing probe and solver setup retained.", meter);
        }

        private static TMP_Text GetText(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            return child != null ? child.GetComponent<TMP_Text>() : null;
        }

        [MenuItem("Tools/Project Spark/Circuit/Meter Probes/Mark Selected as Positive Reference")]
        private static void MarkPositive() { Mark(SparkMeterTerminalEnd.PositiveReference); }

        [MenuItem("Tools/Project Spark/Circuit/Meter Probes/Mark Selected as Negative Reference")]
        private static void MarkNegative() { Mark(SparkMeterTerminalEnd.NegativeReference); }

        [MenuItem("Tools/Project Spark/Circuit/Meter Probes/Mark Selected as Node (No Component Polarity)")]
        private static void MarkNode() { Mark(SparkMeterTerminalEnd.Unspecified); }

        [MenuItem("Tools/Project Spark/Circuit/Meter Probes/Mark Selected as Positive Reference", true)]
        [MenuItem("Tools/Project Spark/Circuit/Meter Probes/Mark Selected as Negative Reference", true)]
        [MenuItem("Tools/Project Spark/Circuit/Meter Probes/Mark Selected as Node (No Component Polarity)", true)]
        private static bool CanMark() => Selection.activeGameObject != null &&
            Selection.activeGameObject.GetComponent<Collider>() != null;

        private static void Mark(SparkMeterTerminalEnd end)
        {
            GameObject go = Selection.activeGameObject;
            if (go == null || go.GetComponent<Collider>() == null)
            {
                Debug.LogWarning("[Spark Multimeter] Select a GameObject with a Collider representing a terminal.");
                return;
            }
            var marker = go.GetComponent<SparkMeterProbeTerminal>();
            if (marker == null) marker = Undo.AddComponent<SparkMeterProbeTerminal>(go);
            var so = new SerializedObject(marker);
            var endField = so.FindProperty("referenceEnd");
            endField.enumValueIndex = (int)end;
            var anchor = go.GetComponentInParent<SparkTeachingAnchor>();
            var component = so.FindProperty("componentAnchorId");
            if (anchor != null && string.IsNullOrWhiteSpace(component.stringValue))
                component.stringValue = anchor.AnchorId;
            var id = so.FindProperty("terminalId");
            if (string.IsNullOrWhiteSpace(id.stringValue))
                id.stringValue = go.name;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(marker);
            Debug.Log("[Spark Multimeter] Terminal marked. Check terminalId and componentAnchorId in Inspector; " +
                "reference polarity must match solver sign convention.", marker);
        }

        private static GameObject UI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void TopRight(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
        }

        private static void TopLeft(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
        }

        private static void Block(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var go = UI(name, parent);
            TopLeft(go.GetComponent<RectTransform>(), x, y, w, h);
            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static TMP_Text Label(Transform parent, string name, string text, int size,
            Color color, float x, float y, float w, float h)
        {
            var go = UI(name, parent);
            TopLeft(go.GetComponent<RectTransform>(), x, y, w, h);
            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.alignment = TextAlignmentOptions.TopLeft;
            label.raycastTarget = false;
            label.enableAutoSizing = false;
            return label;
        }

        private static Button MakeButton(Transform parent, string name, string text, float x, float y,
            float w, float h, Color? color = null)
        {
            var go = UI(name, parent);
            TopLeft(go.GetComponent<RectTransform>(), x, y, w, h);
            return AddButton(go, text, 13, color ?? ButtonBlue);
        }

        private static Button AddButton(GameObject go, string text, int size, Color background)
        {
            var image = go.AddComponent<Image>();
            image.color = background;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var label = Label(go.transform, "Label", text, size, TextColor, 0, 0,
                go.GetComponent<RectTransform>().sizeDelta.x,
                go.GetComponent<RectTransform>().sizeDelta.y);
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return button;
        }

        private static void Set(SerializedObject so, string name, Object value)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property == null) Debug.LogError("[Spark Multimeter] Missing Inspector field: " + name);
            else property.objectReferenceValue = value;
        }
    }
}
#endif

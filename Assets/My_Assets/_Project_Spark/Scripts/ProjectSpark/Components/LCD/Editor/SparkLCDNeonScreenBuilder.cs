#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// One-click world-space LCD UI builder. Operates on a scene instance of LCD.fbx.
    /// Creates only editor-time objects; keeps LCD frame, PCB and wire pins intact.
    /// It hides only screen/character mesh renderers, not their GameObjects.
    /// </summary>
    public static class SparkLCDNeonScreenBuilder
    {
        private const string Menu = "Tools/Project Spark/Electrical/LCD Meter/";
        private const string GeneratedRoot = "Spark_LCD_Neon_UI";
        private static readonly Color Dark = Hex("06181C");
        private static readonly Color Glass = Hex("041F26");
        private static readonly Color Panel = Hex("08282F");
        private static readonly Color Cyan = Hex("25E8F6");
        private static readonly Color SoftCyan = Hex("9BEBF1");
        private static readonly Color Green = Hex("31F39C");
        private static readonly Color Muted = Hex("8AA9B0");
        private static readonly Color Frame = Hex("0D7080");
        private static readonly Color Dim = Hex("0A3641");
        private static readonly Color White = Hex("D9FDFF");

        [MenuItem(Menu + "Build Neon 3D Screen on Selected LCD")]
        private static void Build()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null || EditorUtility.IsPersistent(selected))
            {
                EditorUtility.DisplayDialog("Spark LCD — Neon Screen",
                    "Select an LCD.fbx INSTANCE in the Scene Hierarchy (not the FBX project asset).", "OK");
                return;
            }
            Transform screen = Find(selected.transform, "scr");
            if (screen == null)
            {
                EditorUtility.DisplayDialog("Spark LCD — Neon Screen",
                    "Cannot find a child mesh called 'scr' under the selection. Select the LCD model root.", "OK");
                return;
            }
            MeshFilter mf = screen.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
                EditorUtility.DisplayDialog("Spark LCD — Neon Screen",
                    "The 'scr' object needs a MeshFilter with a readable local mesh bounding box.", "OK");
                return;
            }
            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                EditorUtility.DisplayDialog("Spark LCD — Neon Screen",
                    "Install TMP Essentials first: Window > TextMeshPro > Import TMP Essential Resources.", "OK");
                return;
            }
            Transform existing = screen.Find(GeneratedRoot);
            if (existing != null && !EditorUtility.DisplayDialog("Rebuild neon LCD?",
                    "A generated neon screen already exists. Rebuild its visual hierarchy? " +
                    "This resets manual UI alignment and appearance adjustments.", "Rebuild", "Cancel"))
                return;

            Undo.RegisterFullObjectHierarchyUndo(selected, "Build Spark Neon LCD");
            if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);
            SparkLCDMeter meter = selected.GetComponent<SparkLCDMeter>();
            if (meter == null) meter = Undo.AddComponent<SparkLCDMeter>(selected);
            SparkLCDMeterDisplay display = selected.GetComponent<SparkLCDMeterDisplay>();
            if (display == null) display = Undo.AddComponent<SparkLCDMeterDisplay>(selected);
            if (meter.TerminalA == null || meter.TerminalB == null)
            {
                SparkTerminal a = meter.TerminalA != null ? meter.TerminalA :
                    EnsureTerminal(selected.transform, "LCD_Terminal_A", new Vector3(-0.08f, -0.04f, -0.005f));
                SparkTerminal b = meter.TerminalB != null ? meter.TerminalB :
                    EnsureTerminal(selected.transform, "LCD_Terminal_B", new Vector3(+0.08f, -0.04f, -0.005f));
                Undo.RecordObject(meter, "Assign LCD terminals");
                meter.ConfigureTerminals(a, b);
            }

            // Only the original face and its static characters are hidden.
            SetOriginalMeshVisible(selected.transform, false);
            HideOldText(selected.transform);

            var go = NewObject(GeneratedRoot, screen);
            RectTransform baseRect = go.GetComponent<RectTransform>();
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 150;
            go.AddComponent<GraphicRaycaster>();
            AlignToScreenMesh(baseRect, screen, mf.sharedMesh.bounds);

            // Top-level housing: translucent layered neon glass, no custom shader.
            Flat(baseRect, "LCD Glass", 0, 0, 1600, 420, Dark);
            Flat(baseRect, "Glass Inset", 5, 5, 1590, 410, Glass);
            Flat(baseRect, "Top Neon Rule", 5, 3, 1590, 3, Cyan);
            Flat(baseRect, "Bottom Neon Rule", 5, 414, 1590, 2, Frame);
            for (int i = 0; i < 14; i++)
                Flat(baseRect, "Header Bars " + i, 760 + 12 * i, 38 - (i % 4) * 3,
                    7, 8 + (i % 4) * 3, i < 5 ? Cyan : Hex("134E5C"));

            var title = Text(baseRect, "Brand", "PROJECT  SPARK", 35, 16, 465, 59,
                46, White, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, font);
            title.characterSpacing = 1.5f;
            Text(baseRect, "Header Tag", "UNIVERSAL ELECTRICAL METER", 540, 25, 490, 40,
                24, Muted, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, font);
            TMP_Text headerStatus = Text(baseRect, "Verified Indicator", "● WAITING", 1280, 23, 290, 40,
                23, Green, FontStyles.Bold, TextAlignmentOptions.MidlineRight, font);
            Flat(baseRect, "Header Divider", 20, 73, 1560, 2, Frame);

            // Physically connected A/B terminals on the left of the readout.
            RectTransform wirePanel = FrameBox(baseRect, "Terminal Pair", 18, 86, 228, 255, Frame, Panel, 3);
            Text(wirePanel, "Probe Header", "MEASURE", 15, 10, 198, 32,
                21, SoftCyan, FontStyles.Bold, TextAlignmentOptions.Center, font);
            RectTransform aBox = FrameBox(wirePanel, "Terminal A", 15, 63, 68, 52, Cyan, Dim, 3);
            Text(aBox, "A Symbol", "A", 3, 0, 62, 50, 34, White, FontStyles.Bold, TextAlignmentOptions.Center, font);
            RectTransform bBox = FrameBox(wirePanel, "Terminal B", 15, 174, 68, 52, Green, Dim, 3);
            Text(bBox, "B Symbol", "B", 3, 0, 62, 50, 34, White, FontStyles.Bold, TextAlignmentOptions.Center, font);
            Flat(wirePanel, "Circuit Stroke", 47, 115, 4, 58, Cyan);
            Text(wirePanel, "Lead Polarity", "A (+)\n\nB (-)", 97, 83, 118, 145,
                25, White, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, font);

            // Main numeric readout.
            RectTransform main = FrameBox(baseRect, "Main Measurement", 264, 86, 1016, 204, Frame, Hex("051B22"), 3);
            TMP_Text heading = Text(main, "Measurement Label", "VAB / DC VOLTAGE", 25, 10, 950, 34,
                24, SoftCyan, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, font);
            TMP_Text value = Text(main, "Major Digits", "--.---", 24, 41, 760, 113,
                113, Cyan, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, font);
            value.enableAutoSizing = true;
            value.fontSizeMin = 50;
            value.fontSizeMax = 113;
            TMP_Text unit = Text(main, "Unit", "V", 793, 56, 195, 103,
                84, Cyan, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, font);
            TMP_Text secondary = Text(main, "Secondary Readout", "AWAITING VERIFIED SOLVE", 28, 163, 954, 34,
                26, White, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, font);

            // Four clickable mode selectors (safe display modes only).
            RectTransform modes = FrameBox(baseRect, "Display Modes", 1296, 86, 287, 255, Frame, Panel, 3);
            Text(modes, "Mode Header", "MEASURE MODE", 13, 6, 257, 33,
                20, SoftCyan, FontStyles.Bold, TextAlignmentOptions.Center, font);
            string[] symbols = { "V", "A", "Ω", "W" };
            string[] names = { "VOLTAGE", "CURRENT", "RESISTANCE", "POWER" };
            Image[] highlights = new Image[4];
            Button[] buttons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                RectTransform button = FrameBox(modes, "Mode " + names[i], 12, 43 + i * 51,
                    262, 46, Dim, Dim, 2);
                Image image = button.GetComponent<Image>();
                image.raycastTarget = true;
                highlights[i] = image;
                buttons[i] = button.gameObject.AddComponent<Button>();
                buttons[i].targetGraphic = image;
                var colors = buttons[i].colors;
                colors.highlightedColor = Hex("1DA0A0");
                colors.pressedColor = Green;
                colors.normalColor = Color.white;
                colors.selectedColor = Color.white;
                buttons[i].colors = colors;
                var navigation = buttons[i].navigation;
                navigation.mode = Navigation.Mode.None;
                buttons[i].navigation = navigation;
                Text(button, "Symbol", symbols[i], 10, 3, 54, 40,
                    33, White, FontStyles.Bold, TextAlignmentOptions.Center, font).raycastTarget = false;
                Text(button, "Name", names[i], 70, 7, 179, 32,
                    23, White, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, font).raycastTarget = false;
            }

            // Verified per-component cards. No placeholder fake measurement values.
            TMP_Text current = Card(baseRect, "CURRENT", 264, 302, 324, font);
            TMP_Text resistance = Card(baseRect, "RESISTANCE", 602, 302, 330, font);
            TMP_Text power = Card(baseRect, "POWER", 946, 302, 334, font);
            RectTransform bottom = FrameBox(baseRect, "Status Footer", 264, 375, 1319, 37,
                Frame, Hex("07262C"), 2);
            TMP_Text footer = Text(bottom, "Footer Status", "CONNECT A / B  •  SOLVE CIRCUIT",
                12, 3, 891, 30, 19, SoftCyan,
                FontStyles.Normal, TextAlignmentOptions.MidlineLeft, font);
            TMP_Text modeName = Text(bottom, "Mode Text", "MODE  AUTO",
                903, 3, 320, 30, 18, Green,
                FontStyles.Bold, TextAlignmentOptions.MidlineRight, font);
            RectTransform nextHolder = FrameBox(bottom, "Next Mode Button", 1241, 3, 69, 30,
                Cyan, Dim, 2);
            Image nextImage = nextHolder.GetComponent<Image>();
            nextImage.raycastTarget = true;
            Button nextButton = nextHolder.gameObject.AddComponent<Button>();
            nextButton.targetGraphic = nextImage;
            Text(nextHolder, "Next Symbol", ">", 0, 0, 69, 29, 25,
                White, FontStyles.Bold, TextAlignmentOptions.Center, font);

            // Tiny, transparent data channel. SparkLCDMeterDisplay owns its text.
            TextMeshProUGUI channel = Text(baseRect, "Spark LCD Verified Data Channel", "SPARK LCD / WAIT\nAWAITING SOLVE",
                0, 0, 2, 2, 1, Color.clear,
                FontStyles.Normal, TextAlignmentOptions.Center, font);
            channel.raycastTarget = false;

            SparkElectricalSolver solver = UnityEngine.Object.FindFirstObjectByType<SparkElectricalSolver>();
            SerializedObject displaySO = new SerializedObject(display);
            displaySO.Update();
            SerializedProperty sourceProperty = displaySO.FindProperty("resistor1");
            // Preserve existing level bindings, source selection, and electrical options.
            AssignObject(displaySO, "screenText", channel);
            AssignObject(displaySO, "meter", meter);
            if (displaySO.FindProperty("solver") != null &&
                displaySO.FindProperty("solver").objectReferenceValue == null)
                AssignObject(displaySO, "solver", solver);
            displaySO.ApplyModifiedProperties();

            SparkLCDNeonScreen neon = selected.GetComponent<SparkLCDNeonScreen>();
            if (neon == null) neon = Undo.AddComponent<SparkLCDNeonScreen>(selected);
            SerializedObject neonSO = new SerializedObject(neon);
            neonSO.Update();
            AssignObject(neonSO, "meter", meter);
            AssignObject(neonSO, "verifiedDisplay", display);
            AssignObject(neonSO, "solver", solver);
            AssignObject(neonSO, "sourceText", channel);
            AssignObject(neonSO, "mainValue", value);
            AssignObject(neonSO, "mainUnit", unit);
            AssignObject(neonSO, "mainHeading", heading);
            AssignObject(neonSO, "secondaryLine", secondary);
            AssignObject(neonSO, "headerStatus", headerStatus);
            AssignObject(neonSO, "currentText", current);
            AssignObject(neonSO, "resistanceText", resistance);
            AssignObject(neonSO, "powerText", power);
            AssignObject(neonSO, "footerText", footer);
            AssignObject(neonSO, "modeText", modeName);
            SerializedProperty highlightArray = neonSO.FindProperty("modeHighlights");
            highlightArray.arraySize = 4;
            SerializedProperty buttonArray = neonSO.FindProperty("modeButtons");
            buttonArray.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                highlightArray.GetArrayElementAtIndex(i).objectReferenceValue = highlights[i];
                buttonArray.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
            }
            AssignObject(neonSO, "nextModeButton", nextButton);
            // Mirror the existing analysis Source 1 binding into optional stat cards.
            if (sourceProperty != null)
            {
                SerializedProperty bindingId = sourceProperty.FindPropertyRelative("levelBindingId");
                SerializedProperty slot = sourceProperty.FindPropertyRelative("replacementSlotRoot");
                SerializedProperty direct = sourceProperty.FindPropertyRelative("directComponent");
                if (bindingId != null) neonSO.FindProperty("statsLevelBindingId").stringValue = bindingId.stringValue;
                if (slot != null) AssignObject(neonSO, "statsReplacementSlotRoot", slot.objectReferenceValue);
                if (direct != null) AssignObject(neonSO, "statsDirectComponent", direct.objectReferenceValue);
            }
            neonSO.ApplyModifiedProperties();

            if (PrefabUtility.IsPartOfPrefabInstance(selected))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(meter);
                PrefabUtility.RecordPrefabInstancePropertyModifications(display);
                PrefabUtility.RecordPrefabInstancePropertyModifications(neon);
            }
            EditorUtility.SetDirty(selected);
            Selection.activeGameObject = selected;
            SceneView.RepaintAll();
            string note = "Built a world-space neon LCD screen on 'scr'. Original screen and character meshes hidden, PCB/frame preserved.\n\n" +
                          "If the screen faces backward, use 'Flip Selected Neon Screen' from the same Tools menu.\n" +
                          "Current/Ω/Power cards use the same optional resistor source as your existing display; assign stats source if needed.\n" +
                          "Buttons require the gameplay scene's existing EventSystem.\n" +
                          "The LCD solver definition must already be registered for electrical operation.";
            Debug.Log("[Spark LCD Neon] " + note.Replace("\n", " "), selected);
            EditorUtility.DisplayDialog("Project Spark — Neon LCD", note, "OK");
        }

        [MenuItem(Menu + "Flip Selected Neon Screen")]
        private static void Flip()
        {
            Transform ui = Find(Selection.activeTransform, GeneratedRoot);
            if (ui == null) { EditorUtility.DisplayDialog("Spark LCD", "Select the LCD model root first.", "OK"); return; }
            Undo.RecordObject(ui, "Flip Spark LCD front face");
            ui.localRotation *= Quaternion.Euler(0f, 180f, 0f);
            EditorUtility.SetDirty(ui);
        }

        [MenuItem(Menu + "Restore Original LCD Screen Mesh")]
        private static void Restore()
        {
            Transform target = Selection.activeTransform;
            if (target == null) return;
            Transform scr = Find(target, "scr");
            if (scr == null) return;
            SetOriginalMeshVisible(target, true);
            Transform ui = scr.Find(GeneratedRoot);
            if (ui != null) Undo.DestroyObjectImmediate(ui.gameObject);
            // Restore original text if present; may still need reassigning in MeterDisplay.
            Transform old = Find(target, "Spark_LCD_1602_Text");
            if (old != null)
            {
                var text = old.GetComponent<TMP_Text>();
                if (text != null) { Undo.RecordObject(text, "Restore LCD text"); text.enabled = true; }
            }
            Debug.Log("[Spark LCD Neon] Original LCD geometry restored. If returning to the old text readout, reassign SparkLCDMeterDisplay.Screen Text.", target);
        }

        private static TMP_Text Card(RectTransform parent, string name, float x, float y, float width, TMP_FontAsset font)
        {
            RectTransform box = FrameBox(parent, name + " Card", x, y, width, 62, Frame, Panel, 2);
            Text(box, "Title", name, 14, 4, width - 28, 25, 18,
                SoftCyan, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, font);
            return Text(box, "Reading", "--", 14, 24, width - 28, 35, 31,
                Cyan, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, font);
        }

        private static RectTransform FrameBox(RectTransform parent, string name, float x, float y,
            float width, float height, Color outline, Color fill, float edge)
        {
            RectTransform border = Flat(parent, name, x, y, width, height, outline);
            Flat(border, "Inset", edge, edge, width - edge * 2, height - edge * 2, fill);
            return border;
        }

        private static RectTransform Flat(RectTransform parent, string name, float x, float y,
            float width, float height, Color color)
        {
            var go = NewObject(name, parent);
            RectTransform rt = go.GetComponent<RectTransform>();
            AnchorTopLeft(rt, x, y, width, height);
            Image image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rt;
        }

        private static TextMeshProUGUI Text(RectTransform parent, string name, string content,
            float x, float y, float width, float height, float fontSize, Color color,
            FontStyles style, TextAlignmentOptions align, TMP_FontAsset font)
        {
            var go = NewObject(name, parent);
            RectTransform rt = go.GetComponent<RectTransform>();
            AnchorTopLeft(rt, x, y, width, height);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = align;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;
            tmp.margin = Vector4.zero;
            return tmp;
        }

        private static void AnchorTopLeft(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(w, h);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static GameObject NewObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Create Spark LCD UI " + name);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void AlignToScreenMesh(RectTransform rect, Transform meshTransform, Bounds bounds)
        {
            Vector3 extents = bounds.size;
            int thin = extents.x <= extents.y && extents.x <= extents.z ? 0 :
                       extents.y <= extents.z ? 1 : 2;
            // Choose the face facing the Editor Scene view camera, if available.
            Camera camera = SceneView.lastActiveSceneView != null ?
                SceneView.lastActiveSceneView.camera : Camera.main;
            Vector3 toCamera = camera != null
                ? meshTransform.InverseTransformDirection(camera.transform.position - meshTransform.TransformPoint(bounds.center))
                : Vector3.forward;
            float sign = thin == 0 ? Mathf.Sign(toCamera.x) :
                         thin == 1 ? Mathf.Sign(toCamera.y) : Mathf.Sign(toCamera.z);
            if (Mathf.Approximately(sign, 0f)) sign = 1f;
            Vector3 normal = thin == 0 ? Vector3.right * sign :
                             thin == 1 ? Vector3.up * sign : Vector3.forward * sign;
            Vector3 up = thin == 1 ? Vector3.forward : Vector3.up;
            // LookRotation creates a correct normal and readable world-space axes.
            rect.localRotation = Quaternion.LookRotation(normal, up);
            Vector3 center = bounds.center + normal * (Mathf.Max(extents[thin] * 0.5f, 0f) +
                                                          Mathf.Max(extents.x, extents.y, extents.z) * 0.002f);
            rect.localPosition = center;
            float w = thin == 0 ? extents.z : extents.x;
            float h = thin == 1 ? extents.z : extents.y;
            float scale = Mathf.Min(w * 0.97f / 1600f, h * 0.96f / 420f);
            rect.localScale = Vector3.one * Mathf.Max(0.0000001f, scale);
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * 0.5f;
            rect.sizeDelta = new Vector2(1600f, 420f);
        }

        private static void SetOriginalMeshVisible(Transform root, bool visible)
        {
            foreach (string name in new[] { "scr", "chr" })
            {
                Transform target = Find(root, name);
                if (target == null) continue;
                // Do not disable arbitrary Canvas components in the subtree.
                foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>(true))
                {
                    if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
                    Undo.RecordObject(renderer, "Hide original LCD geometry");
                    renderer.enabled = visible;
                    if (PrefabUtility.IsPartOfPrefabInstance(renderer))
                        PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                }
            }
        }

        private static void HideOldText(Transform root)
        {
            Transform old = Find(root, "Spark_LCD_1602_Text");
            if (old == null) return;
            TMP_Text text = old.GetComponent<TMP_Text>();
            if (text != null)
            {
                Undo.RecordObject(text, "Hide old LCD text");
                text.enabled = false;
            }
        }

        private static SparkTerminal EnsureTerminal(Transform root, string name, Vector3 position)
        {
            Transform existing = root.Find(name);
            GameObject go;
            if (existing == null)
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Undo.RegisterCreatedObjectUndo(go, "Create LCD terminal " + name);
                go.name = name;
                go.transform.SetParent(root, false);
                go.transform.localPosition = position;
                go.transform.localScale = Vector3.one * 0.018f;
            }
            else go = existing.gameObject;
            var terminal = go.GetComponent<SparkTerminal>();
            if (terminal == null) terminal = Undo.AddComponent<SparkTerminal>(go);
            if (go.GetComponent<Collider>() == null) Undo.AddComponent<SphereCollider>(go);
            return terminal;
        }

        private static Transform Find(Transform root, string name)
        {
            if (root == null) return null;
            if (string.Equals(root.name, name, StringComparison.OrdinalIgnoreCase)) return root;
            foreach (Transform child in root)
            {
                Transform result = Find(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private static void AssignObject(SerializedObject so, string prop, UnityEngine.Object value)
        {
            SerializedProperty target = so.FindProperty(prop);
            if (target != null) target.objectReferenceValue = value;
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out Color color);
            return color;
        }
    }
}
#endif

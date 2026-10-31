#if UNITY_EDITOR
using ProjectSpark.UI.VisualTeaching;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching.Editor
{
    /// <summary>
    /// Builds a searchable lesson library plus authored, editable starter ScriptableObjects.
    /// Idempotent for both scene and assets; existing lessons are never overwritten.
    /// </summary>
    public static class SparkAILessonLibraryBuilder
    {
        private const string AssetFolder = "Assets/ProjectSpark/LessonLibraryLessons";
        private static readonly Color Back = new Color(.023f,.069f,.111f,.985f);
        private static readonly Color Tile = new Color(.06f,.14f,.21f,1f);
        private static readonly Color Accent = new Color(.30f,.88f,1f,1f);
        private static readonly Color Ink = new Color(.94f,.97f,1f,1f);
        private static readonly Color Muted = new Color(.67f,.80f,.86f,1f);
        private static readonly Color ButtonColor = new Color(.09f,.28f,.42f,1f);

        private sealed class Definition
        {
            public string id, title, goal, topic, summary, keywords;
            public SparkLibraryTier tier;
            public SparkSchematicKind[] symbols;
            public string[] labels;
            public string[] stepNames;
            public string[] stepBodies;
        }

        [MenuItem("Tools/Project Spark/AI/Create Lesson Library")]
        public static void Create()
        {
            SparkVisualTeachingManager teaching =
                Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            Canvas canvas = teaching != null ? teaching.GetComponentInParent<Canvas>() : null;
            if (canvas == null)
            {
                Debug.LogError("[Spark Lesson Library] Create the AAA Visual Teaching Canvas first.");
                return;
            }

            Transform existingRoot = canvas.transform.Find("SparkAILessonLibraryRoot");
            if (existingRoot != null)
            {
                Selection.activeGameObject = existingRoot.gameObject;
                Debug.Log("[Spark Lesson Library] Existing library selected. No duplicate created.", existingRoot);
                return;
            }

            Definition[] definitions = StarterDefinitions();
            SparkVisualLesson[] assets = CreateStarterAssets(definitions);

            GameObject root = UI("SparkAILessonLibraryRoot", canvas.transform);
            Undo.RegisterCreatedObjectUndo(root, "Create Spark AI Lesson Library");
            Stretch(root.GetComponent<RectTransform>());

            Button opener = MakeButton(root.transform, "OpenLessonLibrary", "LESSON LIBRARY",
                -206, -270, 188, 39, true);

            GameObject card = UI("LessonLibraryCard", root.transform);
            Position(card.GetComponent<RectTransform>(), -797, -65, 775, 715, true);
            Image bg = card.AddComponent<Image>();
            bg.color = Back;
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = Accent;
            outline.effectDistance = new Vector2(1f,-1f);
            MakeText(card.transform, "Heading", "SPARK AI  /  LESSON LIBRARY", 19, Accent,
                20, -16, 662, 30);
            Button close = MakeButton(card.transform, "Close", "X", 713, -13, 37, 31);
            MakeText(card.transform, "Subhead", "AUTHORED ELECTRONICS LESSONS  •  CONCEPTUAL SCHEMATICS", 11,
                Muted, 20, -50, 725, 22);

            TMP_InputField search = MakeInput(card.transform, "SearchInput", "Search lessons, topics, components...",
                19, -80, 736, 39);
            Button all = MakeButton(card.transform, "All", "ALL", 19, -133, 87, 34);
            Button foundation = MakeButton(card.transform, "Foundation", "FOUNDATION", 112, -133, 140, 34);
            Button applied = MakeButton(card.transform, "Applied", "APPLIED", 258, -133, 105, 34);
            Button diagnostics = MakeButton(card.transform, "Diagnostics", "DIAGNOSTICS", 369, -133, 148, 34);
            Button bookmarks = MakeButton(card.transform, "Bookmarks", "BOOKMARKS", 523, -133, 231, 34);
            TMP_Text favoritesText = bookmarks.transform.Find("Label").GetComponent<TMP_Text>();

            MakeBlock(card.transform, "LeftBackground", 19, -181, 342, 434);
            TMP_Text summary = MakeText(card.transform, "LibrarySummary", "0 SHOWN / 0 LESSONS",
                12, Accent, 29, -188, 322, 21);

            GameObject scrollGo = UI("LessonScroll", card.transform);
            Position(scrollGo.GetComponent<RectTransform>(), 26, -218, 328, 387);
            ScrollRect scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 28f;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            GameObject viewportGo = UI("Viewport", scrollGo.transform);
            Stretch(viewportGo.GetComponent<RectTransform>());
            Image viewportImage = viewportGo.AddComponent<Image>();
            viewportImage.color = Tile;
            viewportImage.raycastTarget = true;
            viewportGo.AddComponent<RectMask2D>();

            GameObject contentGo = UI("Content", viewportGo.transform);
            RectTransform contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f,1f);
            contentRect.anchorMax = new Vector2(1f,1f);
            contentRect.pivot = new Vector2(.5f,1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;
            VerticalLayoutGroup layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(4,4,4,4);
            layout.spacing = 5f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            ContentSizeFitter fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewportGo.GetComponent<RectTransform>();
            scroll.content = contentRect;

            GameObject rowTemplate = MakeBlock(contentGo.transform, "LessonRowTemplate", 0, 0, 0, 57);
            LayoutElement rowLayout = rowTemplate.AddComponent<LayoutElement>();
            rowLayout.minHeight = 57;
            rowLayout.preferredHeight = 57;
            Button rowButton = rowTemplate.AddComponent<Button>();
            rowButton.targetGraphic = rowTemplate.GetComponent<Image>();
            TMP_Text rowLabel = MakeText(rowTemplate.transform, "Label", "Lesson row", 13, Ink,
                10, -6, 304, 51);
            rowLabel.enableAutoSizing = false;
            rowLabel.fontSize = 13f;
            rowLabel.richText = true;
            rowTemplate.SetActive(false);

            MakeBlock(card.transform, "DetailBackground", 371, -181, 383, 434);
            TMP_Text selectedTier = MakeText(card.transform, "SelectedTier", "FOUNDATION", 12, Accent,
                385, -192, 351, 22);
            TMP_Text selectedTitle = MakeText(card.transform, "SelectedTitle", "SELECT A LESSON", 22, Ink,
                385, -222, 352, 53);
            TMP_Text selectedSummary = MakeText(card.transform, "SelectedSummary", "Choose an item from the list.",
                14, Muted, 386, -283, 351, 91);
            TMP_Text selectedSteps = MakeText(card.transform, "SelectedSteps", "", 14, Ink,
                386, -393, 352, 185);
            TMP_Text recommendation = MakeText(card.transform, "Recommendation", "NEXT PATH:",
                12, Accent, 386, -577, 352, 30);

            Button recommend = MakeButton(card.transform, "Recommend", "RECOMMENDED NEXT", 19, -625, 179, 37);
            Button bookmark = MakeButton(card.transform, "Bookmark", "SAVE BOOKMARK", 207, -625, 167, 37);
            TMP_Text bookmarkText = bookmark.transform.Find("Label").GetComponent<TMP_Text>();
            Button board = MakeButton(card.transform, "OpenBoard", "OPEN BOARD", 384, -625, 165, 37);
            Button start = MakeButton(card.transform, "StartLesson", "START LESSON", 559, -625, 195, 37);
            TMP_Text status = MakeText(card.transform, "Status", "Starting a library lesson replaces current whiteboard lesson.",
                11, Muted, 19, -676, 727, 26);

            SparkAILessonLibrary controller = Undo.AddComponent<SparkAILessonLibrary>(root);
            SerializedObject so = new SerializedObject(controller);
            Set(so, "teaching", teaching);
            Set(so, "libraryCard", card);
            Set(so, "searchField", search);
            Set(so, "listContent", contentRect);
            Set(so, "listScroll", scroll);
            Set(so, "rowTemplate", rowTemplate);
            Set(so, "summaryText", summary);
            Set(so, "selectedTitleText", selectedTitle);
            Set(so, "selectedTierText", selectedTier);
            Set(so, "selectedSummaryText", selectedSummary);
            Set(so, "selectedStepsText", selectedSteps);
            Set(so, "statusText", status);
            Set(so, "bookmarkLabel", bookmarkText);
            Set(so, "favoritesLabel", favoritesText);
            Set(so, "recommendationText", recommendation);

            SerializedProperty list = so.FindProperty("entries");
            list.arraySize = definitions.Length;
            for (int i = 0; i < definitions.Length; i++)
            {
                SerializedProperty entry = list.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("id").stringValue = definitions[i].id;
                entry.FindPropertyRelative("tier").enumValueIndex = (int)definitions[i].tier;
                entry.FindPropertyRelative("topic").stringValue = definitions[i].topic;
                entry.FindPropertyRelative("summary").stringValue = definitions[i].summary;
                entry.FindPropertyRelative("keywords").stringValue = definitions[i].keywords;
                entry.FindPropertyRelative("lesson").objectReferenceValue = assets[i];
            }
            so.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(opener.onClick, controller.Toggle);
            UnityEventTools.AddPersistentListener(close.onClick, controller.Hide);
            UnityEventTools.AddPersistentListener(all.onClick, controller.AllLessons);
            UnityEventTools.AddPersistentListener(foundation.onClick, controller.Foundation);
            UnityEventTools.AddPersistentListener(applied.onClick, controller.Applied);
            UnityEventTools.AddPersistentListener(diagnostics.onClick, controller.Diagnostics);
            UnityEventTools.AddPersistentListener(bookmarks.onClick, controller.Bookmarks);
            UnityEventTools.AddPersistentListener(recommend.onClick, controller.SelectRecommended);
            UnityEventTools.AddPersistentListener(bookmark.onClick, controller.ToggleBookmark);
            UnityEventTools.AddPersistentListener(board.onClick, controller.OpenCurrentTeachingBoard);
            UnityEventTools.AddPersistentListener(start.onClick, controller.StartSelectedLesson);

            card.SetActive(false);
            EditorUtility.SetDirty(controller);
            Selection.activeGameObject = root;
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Spark Lesson Library] Created with " + definitions.Length +
                " editable conceptual lessons. Start Lesson is user-triggered only.", root);
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            int slash = folder.LastIndexOf('/');
            if (slash < 0) return;
            string parent = folder.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder.Substring(slash + 1));
        }

        private static SparkVisualLesson[] CreateStarterAssets(Definition[] definitions)
        {
            EnsureFolder(AssetFolder);
            SparkVisualLesson[] assets = new SparkVisualLesson[definitions.Length];
            for (int i = 0; i < definitions.Length; i++)
            {
                Definition d = definitions[i];
                string path = AssetFolder + "/" + d.id + ".asset";
                SparkVisualLesson asset = AssetDatabase.LoadAssetAtPath<SparkVisualLesson>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<SparkVisualLesson>();
                    asset.lessonId = d.id;
                    asset.title = d.title;
                    asset.goal = d.goal;
                    asset.steps = new SparkTeachingStep[d.stepNames.Length];
                    for (int stepIndex = 0; stepIndex < asset.steps.Length; stepIndex++)
                    {
                        SparkTeachingStep step = new SparkTeachingStep();
                        step.title = d.stepNames[stepIndex];
                        step.instruction = d.stepBodies[stepIndex];
                        step.hint = "This is an authored educational example. Compare it to verified measurements in your current level.";
                        step.requiredVerifiedActionId = "";
                        step.advanceWhenVerified = false;
                        step.annotations = System.Array.Empty<SparkVisualCommand>();
                        step.schematic = BuildSchematic(d);
                        asset.steps[stepIndex] = step;
                    }
                    AssetDatabase.CreateAsset(asset, path);
                }
                assets[i] = asset;
            }
            return assets;
        }

        private static SparkSchematicData BuildSchematic(Definition d)
        {
            SparkSchematicData schematic = new SparkSchematicData();
            schematic.caption = "CONCEPTUAL / " + d.title + " (not live solver data)";
            int count = d.symbols.Length;
            schematic.parts = new SparkSchematicPart[count];
            for (int i = 0; i < count; i++)
            {
                schematic.parts[i] = new SparkSchematicPart
                {
                    id = d.id + "_symbol_" + i,
                    kind = d.symbols[i],
                    label = d.labels[i],
                    position = new Vector2((i + 1f) / (count + 1f), .56f),
                    color = Accent
                };
            }
            // No topology or energized path is inferred from symbol ordering.
            schematic.wires = System.Array.Empty<SparkSchematicWire>();
            return schematic;
        }

        private static Definition Define(string id, string title, SparkLibraryTier tier, string topic,
            string goal, string summary, string keywords, SparkSchematicKind[] symbols,
            string[] labels, string[] stepNames, string[] stepBodies)
        {
            return new Definition { id = id, title = title, tier = tier, topic = topic,
                goal = goal, summary = summary, keywords = keywords, symbols = symbols,
                labels = labels, stepNames = stepNames, stepBodies = stepBodies };
        }

        private static Definition[] StarterDefinitions()
        {
            return new[]
            {
                Define("01_circuit_basics", "Circuit Fundamentals", SparkLibraryTier.Foundation, "Core Concepts",
                    "Recognize sources, loads, and complete return paths.",
                    "Learn what a circuit requires before expecting a component to operate.",
                    "battery source current connection wire return path",
                    new [] { SparkSchematicKind.Battery, SparkSchematicKind.Resistor, SparkSchematicKind.LED },
                    new [] { "Source", "Load", "Indicator" },
                    new [] { "Identify the source", "Identify the load", "Check the return path" },
                    new [] { "A source establishes an electrical potential difference when active.",
                        "Loads can dissipate energy; their behavior depends on actual electrical conditions.",
                        "For sustained current, trace an appropriate path from the source through the circuit and back. The symbols shown here are not a verified wiring diagram." }),

                Define("02_ohms_law", "Ohm's Law", SparkLibraryTier.Foundation, "Core Concepts",
                    "Understand voltage, current, and resistance.",
                    "Explore I = V / R and the limits of a simple resistive estimate.",
                    "resistor formula voltage current ohm resistance",
                    new [] { SparkSchematicKind.Battery, SparkSchematicKind.Resistor },
                    new [] { "Voltage", "Resistance" },
                    new [] { "Voltage", "Current", "Resistance" },
                    new [] { "Voltage is a potential difference between two points.",
                        "Current is the rate of electric charge flow. Under Ohmic conditions, I = V/R.",
                        "A larger fixed resistance reduces current at the same voltage in a simple resistive model. This is an estimate, not a measurement of your current level." }),

                Define("03_led_polarity", "LED Polarity", SparkLibraryTier.Foundation, "Components",
                    "Recognize the anode, cathode, and forward-conduction concept.",
                    "Learn why a typical LED needs appropriate polarity and current limiting.",
                    "led diode cathode anode forward reverse polarity",
                    new [] { SparkSchematicKind.Battery, SparkSchematicKind.Resistor, SparkSchematicKind.LED },
                    new [] { "Source", "Limiter", "LED" },
                    new [] { "Identify LED terminals", "Forward bias", "Avoid excessive current" },
                    new [] { "Identify anode and cathode using the component's actual marking or datasheet.",
                        "LEDs usually conduct and emit light under appropriate forward bias, subject to device characteristics.",
                        "Use a suitable current-limiting arrangement. A glowing LED alone does not prove every game objective is met." }),

                Define("04_switches", "Switches & Open Circuits", SparkLibraryTier.Foundation, "Components",
                    "Understand open and closed switch states.",
                    "Compare a disconnected branch with a verified conducting path.",
                    "switch open closed conduction return",
                    new [] { SparkSchematicKind.Battery, SparkSchematicKind.Switch, SparkSchematicKind.LED },
                    new [] { "Source", "Switch", "LED" },
                    new [] { "Open state", "Closed state", "Verify with the solver" },
                    new [] { "An open switch interrupts its designated path.",
                        "A closed switch connects its designated contacts, but this alone does not establish a complete current path.",
                        "Use the circuit solver and objective evaluator to confirm conduction. These are example symbols, not live connections." }),

                Define("05_resistor_bands", "Resistor Color Bands", SparkLibraryTier.Applied, "Components",
                    "Learn the four-band resistance code.",
                    "Read two significant digits, a multiplier, and tolerance from a typical four-band resistor.",
                    "resistor color bands brown black red orange yellow tolerance ohms",
                    new [] { SparkSchematicKind.Resistor },
                    new [] { "Four-band resistor" },
                    new [] { "First two digits", "Multiplier", "Tolerance" },
                    new [] { "The first two bands encode significant digits. For example brown-black gives 10.",
                        "The third band is a multiplier. Brown-black-brown corresponds to 100 ohms; brown-black-orange corresponds to 10 kΩ.",
                        "The fourth band indicates tolerance, e.g. gold represents ±5%. Check the part's actual bands before making claims about its value." }),

                Define("06_current_limits", "Designing for Safe Current", SparkLibraryTier.Applied, "Measurements",
                    "Compare a verified current reading with a configured current-range objective.",
                    "Learn current limiting without assuming which component the evaluator measures.",
                    "milliampere safe current target min max resistor objective",
                    new [] { SparkSchematicKind.Battery, SparkSchematicKind.Resistor, SparkSchematicKind.LED },
                    new [] { "Source", "Resistor", "Load" },
                    new [] { "Read the target", "Read the bound component", "Compare limits" },
                    new [] { "Find the configured minimum and optional maximum current for the level objective.",
                        "Read the solver's measurement at the objective-bound component, not an unrelated selected resistor.",
                        "Compare the measured magnitude and the evaluator's additional requirements. Only the official evaluator determines whether the objective passes." }),

                Define("07_power_heat", "Resistor Power & Heating", SparkLibraryTier.Applied, "Thermal Behavior",
                    "Connect electrical dissipation with thermal limits.",
                    "Understand P = V×I, rating margins, and verified burn-state diagnostics.",
                    "burn heat overload smoke thermal rated watts power",
                    new [] { SparkSchematicKind.Resistor },
                    new [] { "Resistor" },
                    new [] { "Power dissipation", "Rating and margin", "Thermal evidence" },
                    new [] { "Dissipated electrical power can be calculated as P = V×I using consistent signed conventions.",
                        "Compare actual dissipated power with the resistor's specified power rating.",
                        "Do not claim a resistor burned solely from a high reading. Use the verified resistor burn or failure state in gameplay." }),

                Define("08_short_circuits", "Short Circuit Diagnosis", SparkLibraryTier.Diagnostics, "Fault Finding",
                    "Recognize evaluator-confirmed short-circuit conditions.",
                    "Follow safe investigation steps without guessing which wire is responsible.",
                    "short circuit bypass fault wrong wire overload",
                    new [] { SparkSchematicKind.Battery, SparkSchematicKind.Resistor },
                    new [] { "Source", "Load" },
                    new [] { "Read the evaluator", "Inspect the path", "Re-evaluate" },
                    new [] { "First check whether the official level evaluator reports a source or target short.",
                        "Inspect actual connections for unwanted low-resistance bypass paths. These symbols do not identify a faulty wire.",
                        "After correcting wiring with the game's normal controls, obtain fresh solver and evaluator results to verify the repair." }),

                Define("09_systematic_diagnosis", "Systematic Circuit Troubleshooting", SparkLibraryTier.Diagnostics, "Fault Finding",
                    "Use verified evidence to isolate a circuit problem.",
                    "Work from power-source status through return paths to component measurements and target results.",
                    "diagnose troubleshoot led objective failed power path",
                    new [] { SparkSchematicKind.Battery, SparkSchematicKind.Switch, SparkSchematicKind.Resistor, SparkSchematicKind.LED },
                    new [] { "Source", "Control", "Load", "Indicator" },
                    new [] { "Check source and path", "Inspect components", "Verify objectives" },
                    new [] { "Read the level evaluator's source and return-path flags rather than guessing from visual appearance.",
                        "Inspect the correct runtime component bindings and solver measurements. Check failed or overheated components using authoritative states.",
                        "Re-run the actual level evaluator after changes. The visible behavior of a single component does not guarantee full level completion." })
            };
        }

        private static GameObject UI(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Position(RectTransform rect, float x, float y, float w, float h, bool topRight = false)
        {
            rect.anchorMin = rect.anchorMax = topRight ? Vector2.one : new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
        }

        private static GameObject MakeBlock(Transform parent, string name, float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            Image image = go.AddComponent<Image>();
            image.color = Tile;
            image.raycastTarget = false;
            return go;
        }

        private static TMP_Text MakeText(Transform parent, string name, string value, int size,
            Color color, float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.enableAutoSizing = true;
            text.fontSizeMin = 10;
            text.fontSizeMax = size;
            text.richText = false;
            text.raycastTarget = false;
            return text;
        }

        private static Button MakeButton(Transform parent, string name, string label,
            float x, float y, float w, float h, bool topRight = false)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h, topRight);
            Image image = go.AddComponent<Image>();
            image.color = ButtonColor;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            TMP_Text text = MakeText(go.transform, "Label", label, 12, Ink, 0, 0, w, h);
            text.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private static TMP_InputField MakeInput(Transform parent, string name, string placeholder,
            float x, float y, float w, float h)
        {
            GameObject root = UI(name, parent);
            Position(root.GetComponent<RectTransform>(), x, y, w, h);
            Image bg = root.AddComponent<Image>();
            bg.color = Tile;
            TMP_InputField field = root.AddComponent<TMP_InputField>();
            GameObject area = UI("TextArea", root.transform);
            RectTransform areaRect = area.GetComponent<RectTransform>();
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(12f,3f);
            areaRect.offsetMax = new Vector2(-12f,-3f);
            area.AddComponent<RectMask2D>();
            TMP_Text hint = MakeText(area.transform, "Placeholder", placeholder, 14, Muted, 0, 0, w-24, h-6);
            hint.alignment = TextAlignmentOptions.MidlineLeft;
            TMP_Text value = MakeText(area.transform, "Text", "", 14, Ink, 0, 0, w-24, h-6);
            value.alignment = TextAlignmentOptions.MidlineLeft;
            field.textViewport = areaRect;
            field.textComponent = (TextMeshProUGUI)value;
            field.placeholder = (Graphic)hint;
            field.lineType = TMP_InputField.LineType.SingleLine;
            field.characterLimit = 90;
            return field;
        }

        private static void Set(SerializedObject so, string name, Object component)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property == null)
                Debug.LogError("[Spark Lesson Library] Missing serialized field: " + name);
            else
                property.objectReferenceValue = component;
        }
    }
}
#endif

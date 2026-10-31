#if UNITY_EDITOR
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching.Editor
{
    /// <summary>
    /// Additive one-click builder; preserves existing path assets and scene panels.
    /// All new UI objects receive CanvasRenderer to work with UGUI clipping.
    /// </summary>
    public static class SparkAIGuidedLearningPathsBuilder
    {
        private const string PathsFolder = "Assets/ProjectSpark/GuidedLearningPaths";
        private const string LessonsFolder = "Assets/ProjectSpark/LessonLibraryLessons";
        private static readonly Color Back = new Color(.023f, .070f, .113f, .99f);
        private static readonly Color Tile = new Color(.068f, .139f, .203f, 1f);
        private static readonly Color Accent = new Color(.29f, .88f, 1f, 1f);
        private static readonly Color TextColor = new Color(.94f, .98f, 1f, 1f);
        private static readonly Color Muted = new Color(.68f, .80f, .87f, 1f);
        private static readonly Color ButtonColor = new Color(.08f, .29f, .44f, 1f);

        [MenuItem("Tools/Project Spark/AI/Create Guided Learning Paths")]
        public static void Create()
        {
            SparkVisualTeachingManager teaching = Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            LevelGamePlayManager level = Object.FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            SparkAIInteractiveTrainingChallenges training = Object.FindFirstObjectByType<SparkAIInteractiveTrainingChallenges>(FindObjectsInactive.Include);
            SparkAILessonLibrary library = Object.FindFirstObjectByType<SparkAILessonLibrary>(FindObjectsInactive.Include);
            SparkAIUnifiedAssistant assistant = Object.FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            Canvas canvas = teaching != null ? teaching.GetComponentInParent<Canvas>() : null;
            if (canvas == null)
            {
                Debug.LogError("[Spark Paths] Create/open the AAA Visual Teaching Canvas first.");
                return;
            }

            SparkGuidedLearningPathAsset[] courseAssets = CreateCourseAssets();
            Transform existingRoot = canvas.transform.Find("SparkAIGuidedLearningPathsRoot");
            if (existingRoot != null)
            {
                Selection.activeGameObject = existingRoot.gameObject;
                Debug.Log("[Spark Paths] Existing Guided Learning Paths selected. No duplicate created; path assets preserved.", existingRoot);
                return;
            }

            GameObject root = UI("SparkAIGuidedLearningPathsRoot", canvas.transform);
            Undo.RegisterCreatedObjectUndo(root, "Create Spark Guided Learning Paths");
            Stretch(root.GetComponent<RectTransform>());
            Button opener = MakeButton(root.transform, "OpenPaths", "LEARNING PATHS", -204, -322, 186, 41, true);

            GameObject card = UI("GuidedPathsCard", root.transform);
            Position(card.GetComponent<RectTransform>(), -757, -73, 736, 710, true);
            Image background = card.AddComponent<Image>();
            background.color = Back;
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = Accent;
            outline.effectDistance = new Vector2(1f, -1f);
            MakeText(card.transform, "Heading", "SPARK AI  /  GUIDED LEARNING PATHS", 19, Accent, 19, -13, 640, 31);
            Button closeButton = MakeButton(card.transform, "Close", "X", 685, -13, 33, 29);
            MakeText(card.transform, "Caption", "AUTHORED STUDY + VERIFIED TRAINING ACHIEVEMENTS", 12, Muted, 20, -46, 684, 21);

            Button previousCourse = MakeButton(card.transform, "PreviousCourse", "< COURSE", 20, -85, 112, 35);
            Button nextCourse = MakeButton(card.transform, "NextCourse", "COURSE >", 599, -85, 115, 35);
            TMP_Text courseTitle = MakeText(card.transform, "CourseTitle", "CIRCUIT FOUNDATIONS", 21, TextColor, 143, -85, 447, 36);
            TMP_Text courseDescription = MakeText(card.transform, "CourseDescription", "Study and practice", 14, Muted, 22, -130, 685, 54);
            TMP_Text stepCount = MakeText(card.transform, "CourseProgress", "COURSE 1/3    STEP 1/4", 13, Accent, 22, -190, 680, 23);
            MakeBlock(card.transform, "ProgressTrack", 22, -221, 688, 9, new Color(.14f, .23f, .31f, 1f));
            GameObject fillGo = UI("ProgressFill", card.transform);
            Position(fillGo.GetComponent<RectTransform>(), 22, -221, 688, 9);
            Image fill = fillGo.AddComponent<Image>();
            fill.color = Accent;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;
            fill.raycastTarget = false;

            MakeBlock(card.transform, "TimelineBackground", 20, -248, 690, 143);
            TMP_Text stepList = MakeText(card.transform, "Timeline", "COURSE STEPS", 14, TextColor, 33, -259, 665, 126);
            stepList.fontSizeMin = 12;
            MakeBlock(card.transform, "StepBackground", 20, -399, 690, 107);
            TMP_Text stepTitle = MakeText(card.transform, "StepTitle", "STEP", 19, Accent, 33, -408, 660, 26);
            TMP_Text stepDescription = MakeText(card.transform, "StepDescription", "Open a lesson to begin.", 14, TextColor, 33, -443, 660, 62);
            Button previousStep = MakeButton(card.transform, "PreviousStage", "< STEP", 21, -515, 118, 36);
            Button nextStep = MakeButton(card.transform, "NextStage", "STEP >", 146, -515, 119, 36);
            Button start = MakeButton(card.transform, "StartStage", "START LESSON / PRACTICE", 274, -515, 437, 36);
            Button studied = MakeButton(card.transform, "MarkStudied", "MARK STUDIED", 20, -557, 220, 39);
            Button verify = MakeButton(card.transform, "CheckVerified", "CHECK VERIFIED", 249, -557, 222, 39);
            Button openTraining = MakeButton(card.transform, "OpenTraining", "AI TRAINING", 480, -557, 230, 39);
            Button openLibrary = MakeButton(card.transform, "OpenLibrary", "LESSON LIBRARY", 20, -605, 220, 35);
            Button ask = MakeButton(card.transform, "AskSpark", "ASK SPARK", 249, -605, 221, 35);
            Button refresh = MakeButton(card.transform, "Refresh", "REFRESH", 480, -605, 230, 35);
            TMP_Text evidence = MakeText(card.transform, "Evidence", "ACTIVE LEVEL: none", 11, Muted, 21, -648, 684, 35);
            TMP_Text status = MakeText(card.transform, "Status", "Study progress is separate from game progression.", 11, Accent, 21, -687, 690, 19);

            SparkAIGuidedLearningPaths controller = Undo.AddComponent<SparkAIGuidedLearningPaths>(root);
            SerializedObject so = new SerializedObject(controller);
            Set(so, "teaching", teaching);
            Set(so, "training", training);
            Set(so, "library", library);
            Set(so, "assistant", assistant);
            Set(so, "levelManager", level);
            Set(so, "card", card);
            Set(so, "courseTitleText", courseTitle);
            Set(so, "courseDescriptionText", courseDescription);
            Set(so, "stepCountText", stepCount);
            Set(so, "stepTitleText", stepTitle);
            Set(so, "stepDescriptionText", stepDescription);
            Set(so, "stepListText", stepList);
            Set(so, "evidenceText", evidence);
            Set(so, "statusText", status);
            Set(so, "progressFill", fill);
            Set(so, "startButton", start);
            Set(so, "studiedButton", studied);
            Set(so, "verifyButton", verify);
            SerializedProperty pathsProp = so.FindProperty("courses");
            pathsProp.arraySize = courseAssets.Length;
            for (int i = 0; i < courseAssets.Length; i++)
                pathsProp.GetArrayElementAtIndex(i).objectReferenceValue = courseAssets[i];
            so.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(opener.onClick, controller.Toggle);
            UnityEventTools.AddPersistentListener(closeButton.onClick, controller.Hide);
            UnityEventTools.AddPersistentListener(previousCourse.onClick, controller.PreviousCourse);
            UnityEventTools.AddPersistentListener(nextCourse.onClick, controller.NextCourse);
            UnityEventTools.AddPersistentListener(previousStep.onClick, controller.PreviousStage);
            UnityEventTools.AddPersistentListener(nextStep.onClick, controller.NextStage);
            UnityEventTools.AddPersistentListener(start.onClick, controller.StartCurrentStep);
            UnityEventTools.AddPersistentListener(studied.onClick, controller.MarkStudied);
            UnityEventTools.AddPersistentListener(verify.onClick, controller.CheckVerified);
            UnityEventTools.AddPersistentListener(openTraining.onClick, controller.OpenTraining);
            UnityEventTools.AddPersistentListener(openLibrary.onClick, controller.OpenLibrary);
            UnityEventTools.AddPersistentListener(ask.onClick, controller.AskSpark);
            UnityEventTools.AddPersistentListener(refresh.onClick, controller.Refresh);

            card.SetActive(false);
            Selection.activeGameObject = root;
            EditorUtility.SetDirty(controller);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Spark Paths] Created guided paths. Lesson assets are editable. " +
                (training == null ? "AI Training scene panel not found; install it before verifying practice." : "AI Training connected."), root);
        }

        private static SparkGuidedLearningPathAsset[] CreateCourseAssets()
        {
            EnsureFolder(PathsFolder);
            return new[]
            {
                MakeAsset("path_01_foundations", "01 / Circuit Foundations",
                    "Build a mental model for sources, LEDs and switching, then complete a verified LED objective.",
                    Lesson("01_basics", "Understand Circuit Basics", "Identify supply, load and return path.", "01_circuit_basics"),
                    Lesson("02_led", "Understand LED Polarity", "Learn anode, cathode and current limiting.", "03_led_polarity"),
                    Lesson("03_switches", "Understand Open Switches", "Study how contacts affect the circuit path.", "04_switches"),
                    Practice("04_practice", "Verify an LED Objective", "Complete an LED-on challenge in a suitable gameplay level using AI TRAINING.", SparkLevelTarget.TargetType.LEDOn)),
                MakeAsset("path_02_current", "02 / Resistance & Current",
                    "Read resistance and electrical measurements, then verify a current-range objective.",
                    Lesson("01_ohms", "Ohm's Law", "Study the relationship I = V/R under appropriate conditions.", "02_ohms_law"),
                    Lesson("02_bands", "Read Resistor Color Bands", "Practice reading four-band resistance codes.", "05_resistor_bands"),
                    Lesson("03_current", "Understand Current Limits", "Compare configured limits to solver measurements of the bound component.", "06_current_limits"),
                    Practice("04_practice", "Verify Safe Current", "Complete a configured current-range challenge using verified evaluator evidence.", SparkLevelTarget.TargetType.CurrentRange)),
                MakeAsset("path_03_diagnostics", "03 / Safety & Fault Diagnosis",
                    "Study thermal power and troubleshooting, then verify an objective that measures dissipated power.",
                    Lesson("01_heat", "Resistor Power & Heat", "Study rated power, heating and the game's verified burn state.", "07_power_heat"),
                    Lesson("02_shorts", "Investigate Short Circuits", "Use authoritative short flags, not invented wire paths.", "08_short_circuits"),
                    Lesson("03_diagnosis", "Systematic Troubleshooting", "Follow source, return path, component and evaluator evidence.", "09_systematic_diagnosis"),
                    Practice("04_practice", "Verify a Power Objective", "Complete a power-range objective in a suitable level; change the target type in this course asset if needed.", SparkLevelTarget.TargetType.PowerRange))
            };
        }

        private static SparkLearningPathStage Lesson(string id, string title, string description, string lessonId)
        {
            SparkVisualLesson asset = AssetDatabase.LoadAssetAtPath<SparkVisualLesson>(LessonsFolder + "/" + lessonId + ".asset");
            if (asset == null)
                Debug.LogWarning("[Spark Paths] Starter lesson asset not found: " + lessonId + ". Create Lesson Library first, or assign it to the path asset later.");
            return new SparkLearningPathStage { id = id, title = title, description = description,
                kind = SparkLearningPathStageKind.Lesson, lesson = asset };
        }
        private static SparkLearningPathStage Practice(string id, string title, string description, SparkLevelTarget.TargetType type)
        {
            return new SparkLearningPathStage { id = id, title = title, description = description,
                kind = SparkLearningPathStageKind.VerifiedPractice, requiredTargetType = type };
        }
        private static SparkGuidedLearningPathAsset MakeAsset(string id, string title, string description,
            params SparkLearningPathStage[] stages)
        {
            string path = PathsFolder + "/" + id + ".asset";
            SparkGuidedLearningPathAsset existing = AssetDatabase.LoadAssetAtPath<SparkGuidedLearningPathAsset>(path);
            if (existing != null) return existing; // Never overwrite a designer-edited course.
            SparkGuidedLearningPathAsset created = ScriptableObject.CreateInstance<SparkGuidedLearningPathAsset>();
            created.pathId = id;
            created.title = title;
            created.description = description;
            created.stages = stages;
            AssetDatabase.CreateAsset(created, path);
            return created;
        }
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            if (slash < 0) return;
            string parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }
        private static GameObject UI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return go;
        }
        private static void Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }
        private static void Position(RectTransform r, float x, float y, float w, float h, bool topRight = false)
        {
            r.anchorMin = r.anchorMax = topRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = new Vector2(x, y);
            r.sizeDelta = new Vector2(w, h);
        }
        private static GameObject MakeBlock(Transform parent, string name, float x, float y, float w, float h, Color? color = null)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            Image image = go.AddComponent<Image>();
            image.color = color ?? Tile;
            image.raycastTarget = false;
            return go;
        }
        private static TMP_Text MakeText(Transform parent, string name, string value, int size, Color color,
            float x, float y, float w, float h)
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
            text.raycastTarget = false;
            text.richText = false;
            return text;
        }
        private static Button MakeButton(Transform parent, string name, string label, float x, float y,
            float w, float h, bool topRight = false)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h, topRight);
            Image image = go.AddComponent<Image>();
            image.color = ButtonColor;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            TMP_Text labelText = MakeText(go.transform, "Label", label, 13, TextColor, 0, 0, w, h);
            labelText.alignment = TextAlignmentOptions.Center;
            return button;
        }
        private static void Set(SerializedObject obj, string field, Object value)
        {
            SerializedProperty prop = obj.FindProperty(field);
            if (prop != null) prop.objectReferenceValue = value;
            else Debug.LogError("[Spark Paths] Missing runtime serialized field: " + field);
        }
    }
}
#endif

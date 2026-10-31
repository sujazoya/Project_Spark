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
    /// Standalone, additive assessment UI. Never edits existing assessment assets
    /// or duplicates a previously built panel. All UGUI objects have CanvasRenderer.
    /// </summary>
    public static class SparkAILearningPathAssessmentBuilder
    {
        private const string AssetFolder = "Assets/ProjectSpark/LearningPathAssessments";
        private static readonly Color Back = new Color(.025f, .071f, .118f, .985f);
        private static readonly Color Tile = new Color(.065f, .14f, .21f, 1f);
        private static readonly Color Accent = new Color(.28f, .88f, 1f, 1f);
        private static readonly Color Ink = new Color(.95f, .98f, 1f, 1f);
        private static readonly Color Muted = new Color(.72f, .83f, .9f, 1f);
        private static readonly Color ButtonColor = new Color(.085f, .31f, .47f, 1f);

        [MenuItem("Tools/Project Spark/AI/Create Learning Path Assessment")]
        public static void Create()
        {
            var existing = Object.FindFirstObjectByType<SparkAILearningPathAssessment>(FindObjectsInactive.Include);
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                Debug.Log("[Spark Assessment] Existing panel selected; not duplicated.", existing);
                return;
            }

            SparkAIAssessmentBank[] banks = CreateBanks();
            LevelGamePlayManager level = Object.FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            SparkVerifiedElectricalPublisher publisher = Object.FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            SparkAIUnifiedAssistant assistant = Object.FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            MonoBehaviour paths = FindOptionalLearningPaths();

            // A dedicated Canvas avoids depending on another panel being active.
            GameObject canvasGo = new GameObject("SparkAIAssessmentCanvas", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Spark AI Learning Path Assessment");
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 880;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;

            GameObject root = UI("SparkAILearningPathAssessmentRoot", canvasGo.transform);
            Stretch(root.GetComponent<RectTransform>());
            Button opener = MakeButton(root.transform, "OpenAssessment", "AI ASSESSMENT", -239, -392, 224, 42, true);

            GameObject panel = UI("AssessmentPanel", root.transform);
            Position(panel.GetComponent<RectTransform>(), -795, -65, 775, 740, true);
            Image backImage = panel.AddComponent<Image>();
            backImage.color = Back;
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = Accent;
            outline.effectDistance = new Vector2(1f, -1f);

            MakeText(panel.transform, "Header", "SPARK AI / LEARNING PATH ASSESSMENT", 19, Accent, 20, -13, 680, 32);
            Button close = MakeButton(panel.transform, "Close", "X", 720, -15, 35, 32);
            MakeText(panel.transform, "Info", "CONCEPT QUIZZES + EVALUATOR-VERIFIED PRACTICAL EXAMS", 12, Muted, 21, -49, 712, 20);
            Button bankPrev = MakeButton(panel.transform, "PreviousBank", "< COURSE", 20, -82, 116, 36);
            Button bankNext = MakeButton(panel.transform, "NextBank", "COURSE >", 639, -82, 116, 36);
            TMP_Text bankTitle = MakeText(panel.transform, "BankTitle", "FOUNDATION ASSESSMENT", 20, Ink, 148, -82, 480, 35);
            TMP_Text bankDescription = MakeText(panel.transform, "BankDescription", "Learn and verify core electronics concepts.", 13, Muted, 21, -125, 720, 45);
            TMP_Text count = MakeText(panel.transform, "QuestionCount", "BANK 1/3  •  QUESTION 1/4", 12, Accent, 21, -169, 710, 24);
            MakeBlock(panel.transform, "QuestionBackground", 18, -196, 740, 115);
            TMP_Text questionTitle = MakeText(panel.transform, "QuestionTitle", "Question", 18, Accent, 30, -205, 704, 25);
            TMP_Text prompt = MakeText(panel.transform, "QuestionPrompt", "Select your answer.", 16, Ink, 30, -238, 703, 66);

            Button[] choices = new Button[4];
            TMP_Text[] labels = new TMP_Text[4];
            for (int i = 0; i < 4; i++)
            {
                choices[i] = MakeButton(panel.transform, "Choice" + i, "", 21, -323 - i * 39, 731, 34);
                labels[i] = choices[i].GetComponentInChildren<TextMeshProUGUI>();
                if (labels[i] != null)
                {
                    labels[i].alignment = TextAlignmentOptions.MidlineLeft;
                    labels[i].margin = new Vector4(12, 0, 10, 0);
                }
            }

            GameObject numericRoot = UI("NumericRoot", panel.transform);
            Position(numericRoot.GetComponent<RectTransform>(), 21, -326, 732, 55);
            MakeText(numericRoot.transform, "NumericCaption", "NUMBER ONLY (unit is specified in the question)", 12, Muted, 0, 0, 730, 19);
            TMP_InputField numericInput = MakeNumericInput(numericRoot.transform, 0, -24, 730, 35);

            MakeBlock(panel.transform, "ResultBackground", 20, -484, 734, 74);
            TMP_Text result = MakeText(panel.transform, "Result", "Feedback appears after submission.", 13, Ink, 30, -490, 715, 64);
            TMP_Text score = MakeText(panel.transform, "Score", "ANSWERED 0/0  •  BEST —", 13, Accent, 22, -564, 723, 22);
            MakeBlock(panel.transform, "ProgressTrack", 21, -591, 733, 7, new Color(.14f, .23f, .31f, 1f));
            GameObject fillGo = UI("ProgressFill", panel.transform);
            Position(fillGo.GetComponent<RectTransform>(), 21, -591, 733, 7);
            Image fill = fillGo.AddComponent<Image>();
            fill.color = Accent;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;
            fill.raycastTarget = false;

            Button prevQ = MakeButton(panel.transform, "PreviousQuestion", "< Q", 20, -608, 72, 38);
            Button nextQ = MakeButton(panel.transform, "NextQuestion", "Q >", 99, -608, 72, 38);
            Button submit = MakeButton(panel.transform, "Submit", "SUBMIT", 179, -608, 140, 38);
            Button begin = MakeButton(panel.transform, "BeginPractical", "BEGIN PRACTICAL", 179, -608, 197, 38);
            Button verify = MakeButton(panel.transform, "VerifyPractical", "CHECK EVIDENCE", 381, -608, 163, 38);
            Button hint = MakeButton(panel.transform, "Hint", "HINT", 552, -608, 94, 38);
            Button restart = MakeButton(panel.transform, "Restart", "RETRY", 654, -608, 101, 38);
            Button ask = MakeButton(panel.transform, "AskSpark", "ASK SPARK", 21, -654, 197, 37);
            Button pathsButton = MakeButton(panel.transform, "LearningPaths", "LEARNING PATHS", 225, -654, 240, 37);
            Button refresh = MakeButton(panel.transform, "Refresh", "REFRESH", 472, -654, 282, 37);
            TMP_Text status = MakeText(panel.transform, "Status", "No gameplay progress is changed by this assessment.", 12, Muted, 22, -701, 721, 29);

            SparkAILearningPathAssessment runner = Undo.AddComponent<SparkAILearningPathAssessment>(root);
            SerializedObject so = new SerializedObject(runner);
            Set(so, "levelManager", level);
            Set(so, "publisher", publisher);
            Set(so, "assistant", assistant);
            Set(so, "learningPaths", paths);
            Set(so, "panel", panel);
            Set(so, "bankTitleText", bankTitle);
            Set(so, "bankDescriptionText", bankDescription);
            Set(so, "questionCountText", count);
            Set(so, "questionTitleText", questionTitle);
            Set(so, "questionPromptText", prompt);
            Set(so, "resultText", result);
            Set(so, "statusText", status);
            Set(so, "scoreText", score);
            Set(so, "numericInput", numericInput);
            Set(so, "numericRoot", numericRoot);
            Set(so, "progressFill", fill);
            Set(so, "submitButton", submit);
            Set(so, "beginPracticalButton", begin);
            Set(so, "verifyButton", verify);
            SetArray(so, "choiceButtons", choices);
            SetArray(so, "choiceLabels", labels);
            SetArray(so, "banks", banks);
            so.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(opener.onClick, runner.Toggle);
            UnityEventTools.AddPersistentListener(close.onClick, runner.Hide);
            UnityEventTools.AddPersistentListener(bankPrev.onClick, runner.PreviousBank);
            UnityEventTools.AddPersistentListener(bankNext.onClick, runner.NextBank);
            UnityEventTools.AddPersistentListener(prevQ.onClick, runner.PreviousQuestion);
            UnityEventTools.AddPersistentListener(nextQ.onClick, runner.NextQuestion);
            UnityEventTools.AddPersistentListener(choices[0].onClick, runner.Choice0);
            UnityEventTools.AddPersistentListener(choices[1].onClick, runner.Choice1);
            UnityEventTools.AddPersistentListener(choices[2].onClick, runner.Choice2);
            UnityEventTools.AddPersistentListener(choices[3].onClick, runner.Choice3);
            UnityEventTools.AddPersistentListener(submit.onClick, runner.SubmitAnswer);
            UnityEventTools.AddPersistentListener(begin.onClick, runner.BeginPractical);
            UnityEventTools.AddPersistentListener(verify.onClick, runner.VerifyPractical);
            UnityEventTools.AddPersistentListener(hint.onClick, runner.ShowHint);
            UnityEventTools.AddPersistentListener(restart.onClick, runner.RestartBank);
            UnityEventTools.AddPersistentListener(ask.onClick, runner.AskSpark);
            UnityEventTools.AddPersistentListener(pathsButton.onClick, runner.OpenLearningPaths);
            UnityEventTools.AddPersistentListener(refresh.onClick, runner.Refresh);

            panel.SetActive(false);
            Selection.activeGameObject = root;
            EditorUtility.SetDirty(runner);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvasGo.scene);
            AssetDatabase.SaveAssets();
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Include) == null)
                Debug.LogWarning("[Spark Assessment] No EventSystem detected. Add one appropriate for your project input system.", root);
            Debug.Log("[Spark Assessment] Created with 3 editable exam banks. " +
                      (publisher == null ? "Verified publisher missing: connect one before practical exams." : "Verified publisher linked."), root);
        }

        private static MonoBehaviour FindOptionalLearningPaths()
        {
            MonoBehaviour[] components = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (MonoBehaviour component in components)
                if (component != null && component.GetType().Name == "SparkAIGuidedLearningPaths") return component;
            return null;
        }

        private static SparkAIAssessmentBank[] CreateBanks()
        {
            EnsureFolder(AssetFolder);
            return new[]
            {
                Bank("assessment_01_foundations", "path_01_foundations", "01 / Circuit Foundations Exam",
                    "Understand source, LED, switch and current path. Final question requires a new verified LED-on objective.",
                    Choice("f1", "Open circuit", "What happens to current in an ideal series circuit when its only switch is open?",
                        0, "An open series switch breaks the return path.", "An ideal open circuit has no conduction through that series path.",
                        "Current in that path is zero", "Current becomes unlimited", "Current stays unchanged", "The source doubles its voltage"),
                    Choice("f2", "LED polarity", "For a standard LED in forward operation, how should its anode be connected relative to its cathode?",
                        1, "Forward bias means the anode is at a higher potential (within ratings).", "Correct forward bias still requires suitable current limiting.",
                        "Anode must be lower", "Anode generally higher", "Both terminals always equal", "Polarity never matters"),
                    Choice("f3", "Series resistor", "Why is a resistor commonly placed in series with a simple LED circuit?",
                        2, "Think about what limits the current through the LED.", "A series resistor can help establish safe current; its value depends on the supply and device behavior.",
                        "To create infinite voltage", "To short the supply", "To help limit current", "To remove the return path"),
                    Practical("f4", "Practical: LED operation", "In a suitable gameplay level, begin while an LEDOn objective is PENDING, then satisfy it.",
                        SparkLevelTarget.TargetType.LEDOn, "Check the source, wiring, polarity, and series current limit.",
                        "The official level evaluator confirmed the LED-on target after the practical began.")),
                Bank("assessment_02_current", "path_02_current", "02 / Resistance & Current Exam",
                    "Calculate ideal resistor values and verify an objective-bound current range in the actual game.",
                    Numeric("r1", "Ohm's law", "An ideal resistor has 12 V across 1,000 Ω. What is the current in mA? Enter only the number.",
                        12f, .05f, "mA", "I = V / R; multiply amperes by 1000 for mA.", "12 V / 1000 Ω = 0.012 A = 12 mA."),
                    Choice("r2", "Resistance and current", "With the same voltage across an ideal resistor, what happens if resistance increases?",
                        0, "Use I = V / R.", "At constant voltage, greater resistance results in smaller current.",
                        "Current decreases", "Current increases", "Current stays identical", "Voltage automatically becomes zero"),
                    Numeric("r3", "Milliamp calculation", "An ideal 150 Ω resistor has 3 V across it. What current flows in mA? Enter only the number.",
                        20f, .05f, "mA", "3 ÷ 150 = 0.02 A.", "0.02 A is 20 mA."),
                    Choice("r4", "Color bands", "What nominal resistance and tolerance do brown-black-red-gold bands indicate?",
                        1, "Brown=1, black=0, red multiplier ×100, gold tolerance ±5%.", "10 × 100 = 1000 Ω, with ±5% tolerance.",
                        "100 Ω ±10%", "1,000 Ω ±5%", "10 kΩ ±1%", "100 kΩ ±5%"),
                    Practical("r5", "Practical: Current range", "In a level with a pending CurrentRange objective, achieve a new evaluator-confirmed PASS.",
                        SparkLevelTarget.TargetType.CurrentRange, "Compare the configured target's current limits with its bound-component readings.",
                        "The configured current-range objective passed after the attempt began.")),
                Bank("assessment_03_safety", "path_03_diagnostics", "03 / Safety & Diagnosis Exam",
                    "Test resistance heating, power, short-circuit concepts, solver reference signs and verified safe operation.",
                    Numeric("s1", "Power rating", "A resistor dissipates 0.50 W and is rated for 0.25 W. By what factor is it over its rated power?",
                        2f, .01f, "×", "Divide the dissipated power by the rated power.", "0.50 / 0.25 = 2 times its rating."),
                    Choice("s2", "Short-circuit concept", "Why can a wire that bypasses an intended load be dangerous?",
                        1, "Think about the effective resistance seen by the source.", "A low-resistance bypass may cause excessive current, depending on the actual source and circuit.",
                        "It guarantees safe current", "It may create excessive current", "It always increases load resistance", "It proves the load is operating"),
                    Choice("s3", "Signed current", "What does a negative current reading generally indicate in a circuit solver?",
                        2, "A simulator chooses a reference direction.", "Negative current indicates flow opposite the solver's selected positive reference, not necessarily an electrical fault.",
                        "A burned circuit", "Zero current", "Flow opposite reference direction", "Incorrect units"),
                    Numeric("s4", "Resistor power", "An ideal 100 Ω resistor has 5 V across it. What power is dissipated in watts? Enter only the number.",
                        .25f, .005f, "W", "P = V²/R = 25/100.", "P = 5²/100 = 0.25 W."),
                    Practical("s5", "Practical: Power range", "In a level with a pending PowerRange target, satisfy it and obtain a new evaluator PASS.",
                        SparkLevelTarget.TargetType.PowerRange, "Check the objective binding and its configured minimum and maximum power.",
                        "The official level evaluator passed the bound power-range objective after this attempt began."))
            };
        }

        private static SparkAIAssessmentBank Bank(string id, string course, string title, string description,
            params SparkAssessmentItem[] questions)
        {
            string path = AssetFolder + "/" + id + ".asset";
            SparkAIAssessmentBank existing = AssetDatabase.LoadAssetAtPath<SparkAIAssessmentBank>(path);
            if (existing != null) return existing; // Do not overwrite edits.
            var created = ScriptableObject.CreateInstance<SparkAIAssessmentBank>();
            created.bankId = id;
            created.courseId = course;
            created.displayName = title;
            created.description = description;
            created.passingPercent = 70;
            created.items = questions;
            AssetDatabase.CreateAsset(created, path);
            return created;
        }
        private static SparkAssessmentItem Choice(string id, string title, string prompt, int correct,
            string hint, string explanation, params string[] choices)
        {
            return new SparkAssessmentItem { itemId = id, title = title, prompt = prompt,
                kind = SparkAssessmentItemKind.MultipleChoice, choices = choices, correctChoice = correct,
                hint = hint, explanation = explanation };
        }
        private static SparkAssessmentItem Numeric(string id, string title, string prompt, float expected,
            float tolerance, string unit, string hint, string explanation)
        {
            return new SparkAssessmentItem { itemId = id, title = title, prompt = prompt,
                kind = SparkAssessmentItemKind.Numeric, expectedNumber = expected,
                numericTolerance = tolerance, numericUnit = unit,
                hint = hint, explanation = explanation };
        }
        private static SparkAssessmentItem Practical(string id, string title, string prompt,
            SparkLevelTarget.TargetType type, string hint, string explanation)
        {
            return new SparkAssessmentItem { itemId = id, title = title, prompt = prompt,
                kind = SparkAssessmentItemKind.VerifiedPractical, requiredTargetType = type,
                hint = hint, explanation = explanation };
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
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return go;
        }
        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private static void Position(RectTransform rect, float x, float y, float width, float height, bool topRight = false)
        {
            rect.anchorMin = rect.anchorMax = topRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }
        private static void MakeBlock(Transform parent, string name, float x, float y, float w, float h, Color? color = null)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            Image image = go.AddComponent<Image>();
            image.color = color ?? Tile;
            image.raycastTarget = false;
        }
        private static TextMeshProUGUI MakeText(Transform parent, string name, string content,
            int size, Color color, float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.enableAutoSizing = true;
            text.fontSizeMin = 10;
            text.fontSizeMax = size;
            text.raycastTarget = false;
            text.richText = false;
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
            TextMeshProUGUI text = MakeText(go.transform, "Label", label, 13, Ink, 0, 0, w, h);
            text.alignment = TextAlignmentOptions.Center;
            return button;
        }
        private static TMP_InputField MakeNumericInput(Transform parent, float x, float y, float w, float h)
        {
            GameObject go = UI("NumericInput", parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            Image image = go.AddComponent<Image>();
            image.color = Tile;
            TMP_InputField input = go.AddComponent<TMP_InputField>();
            TextMeshProUGUI text = MakeText(go.transform, "InputText", "", 16, Ink, 12, -6, w - 20, h - 8);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            TextMeshProUGUI placeholder = MakeText(go.transform, "Placeholder", "Enter numeric answer using a decimal point", 14, Muted, 12, -6, w - 20, h - 8);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            input.textViewport = go.GetComponent<RectTransform>();
            input.textComponent = text;
            input.placeholder = placeholder;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.contentType = TMP_InputField.ContentType.Standard;
            return input;
        }
        private static void Set(SerializedObject so, string name, Object reference)
        {
            SerializedProperty prop = so.FindProperty(name);
            if (prop == null) Debug.LogError("[Spark Assessment] Missing serialized field: " + name);
            else prop.objectReferenceValue = reference;
        }
        private static void SetArray<T>(SerializedObject so, string name, T[] values) where T : Object
        {
            SerializedProperty prop = so.FindProperty(name);
            if (prop == null) { Debug.LogError("[Spark Assessment] Missing serialized array: " + name); return; }
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
#endif

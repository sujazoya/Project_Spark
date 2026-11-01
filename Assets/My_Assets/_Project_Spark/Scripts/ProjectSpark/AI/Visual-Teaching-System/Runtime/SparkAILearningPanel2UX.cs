using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectSpark.UI
{
    /// <summary>
    /// Additive 2.0 presentation layer for SparkAILearningPanel. No game/solver writes.
    /// Reads evaluator values through optional public properties; never invents progress.
    /// Does not change drawer mode or close any other panel.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAILearningPanel2UX : MonoBehaviour
    {
        [Serializable]
        public sealed class ActionView
        {
            public string id;
            public Button railButton;
            public Button detailedButton;
            public Image railBackground;
            public Image detailedBackground;
        }

        [SerializeField] private SparkAILearningPanel panel;
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private TMP_Text objectiveLabel;
        [SerializeField] private TMP_Text lastToolLabel;
        [SerializeField] private Image objectiveFill;
        [SerializeField] private Button[] shortcutButtons = new Button[0];
        [SerializeField] private string[] shortcutIds = new string[0];
        [SerializeField] private ActionView[] actionViews = new ActionView[0];
        [SerializeField, Range(0.5f, 8f)] private float refreshInterval = 1.5f;

        private const string LastToolKey = "ProjectSpark.AILearningPanel2.LastTool.v1";
        private static readonly Color NormalRail = Hex("102D40");
        private static readonly Color NormalDetailed = Hex("102D40");
        private static readonly Color HighlightRail = Hex("197087");
        private static readonly Color HighlightDetailed = Hex("1B5468");
        private readonly List<Tuple<Button, UnityAction>> listeners =
            new List<Tuple<Button, UnityAction>>();

        private MonoBehaviour levelManager;
        private float nextRefresh;
        private float nextLevelLookup;
        private string lastToolId;
        private string previousStatus = string.Empty;

        private static Color Hex(string value)
        {
            return ColorUtility.TryParseHtmlString("#" + value, out Color result)
                ? result : Color.white;
        }

        private void OnEnable()
        {
            lastToolId = PlayerPrefs.GetString(LastToolKey, string.Empty);
            for (int i = 0; i < actionViews.Length; i++)
            {
                ActionView view = actionViews[i];
                if (view == null || string.IsNullOrEmpty(view.id)) continue;
                string id = view.id;
                Attach(view.railButton, () => RecordTool(id));
                Attach(view.detailedButton, () => RecordTool(id));
            }
            for (int i = 0; i < shortcutButtons.Length && i < shortcutIds.Length; i++)
            {
                Button button = shortcutButtons[i];
                string id = shortcutIds[i];
                if (string.IsNullOrEmpty(id)) continue;
                Attach(button, () => LaunchShortcut(id));
            }
            RefreshSelection();
            nextRefresh = 0f;
            RefreshStatus();
        }

        private void OnDisable()
        {
            for (int i = 0; i < listeners.Count; i++)
                if (listeners[i].Item1 != null)
                    listeners[i].Item1.onClick.RemoveListener(listeners[i].Item2);
            listeners.Clear();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + Mathf.Max(0.5f, refreshInterval);
            RefreshStatus();
        }

        private void Attach(Button button, UnityAction callback)
        {
            if (button == null) return;
            button.onClick.AddListener(callback);
            listeners.Add(new Tuple<Button, UnityAction>(button, callback));
        }

        private void LaunchShortcut(string id)
        {
            // Explicitly use the existing verified launcher, which already handles
            // unavailable modules and their method invocation errors.
            if (panel != null) panel.Execute(id);
            RecordTool(id);
        }

        private void RecordTool(string id)
        {
            lastToolId = id;
            PlayerPrefs.SetString(LastToolKey, id);
            RefreshSelection();
        }

        private void RefreshSelection()
        {
            string toolTitle = "NONE YET";
            for (int i = 0; i < SparkAILearningPanel.Catalog.Length; i++)
                if (SparkAILearningPanel.Catalog[i].id == lastToolId)
                {
                    toolTitle = SparkAILearningPanel.Catalog[i].title;
                    break;
                }
            if (lastToolLabel != null)
            {
                string label = "LAST USED  /  " + toolTitle;
                if (lastToolLabel.text != label) lastToolLabel.text = label;
            }

            for (int i = 0; i < actionViews.Length; i++)
            {
                ActionView row = actionViews[i];
                if (row == null) continue;
                bool selected = row.id == lastToolId;
                SetColor(row.railBackground, selected ? HighlightRail : NormalRail);
                SetColor(row.detailedBackground, selected ? HighlightDetailed : NormalDetailed);
            }
        }

        private static void SetColor(Image image, Color color)
        {
            if (image != null && image.color != color) image.color = color;
        }

        public void RefreshStatus()
        {
            if (panel == null) return;
            // Follow the existing drawer's availability flags. No second
            // reflection scan of every UI module is needed.
            // Track each shortcut independently: a different module can become
            // available without changing the overall ready count.
            for (int i = 0; i < shortcutButtons.Length && i < shortcutIds.Length; i++)
            {
                bool available = false;
                for (int j = 0; j < actionViews.Length; j++)
                {
                    ActionView view = actionViews[j];
                    if (view != null && view.id == shortcutIds[i])
                    {
                        available = view.detailedButton != null &&
                                    view.detailedButton.interactable;
                        break;
                    }
                }
                if (shortcutButtons[i] != null && shortcutButtons[i].interactable != available)
                    shortcutButtons[i].interactable = available;
            }

            // Do not run a full component scan every refresh when a level
            // manager is not installed or a scene has not loaded yet.
            if (levelManager == null && Time.unscaledTime >= nextLevelLookup)
            {
                nextLevelLookup = Time.unscaledTime + 8f;
                LocateLevelManager();
            }
            if (levelManager == null)
            {
                ShowStatus("NO ACTIVE LEVEL", "OBJECTIVES  — / —", 0f);
                return;
            }

            object level = ReadPublic(levelManager, "ActiveLevel");
            if (level is UnityEngine.Object unityLevel && unityLevel == null) level = null;
            if (level == null)
            {
                ShowStatus("NO ACTIVE LEVEL", "OBJECTIVES  — / —", 0f);
                return;
            }

            string title = ReadPublic(level, "DisplayName") as string;
            if (string.IsNullOrWhiteSpace(title)) title = "ACTIVE LEVEL";
            object doneValue = ReadPublic(levelManager, "SatisfiedTargetCount");
            object totalValue = ReadPublic(levelManager, "ActiveTargetCount");
            object stateValue = ReadPublic(levelManager, "RuntimeState");
            int done, total;
            if (!TryInt(doneValue, out done) || !TryInt(totalValue, out total))
            {
                ShowStatus(title, "OBJECTIVES  UNAVAILABLE", 0f);
                return;
            }
            float ratio = total > 0 ? Mathf.Clamp01((float)done / total) : 0f;
            string suffix = stateValue != null ? "  ·  " + stateValue : string.Empty;
            ShowStatus(title + suffix, "OBJECTIVES  " + done + " / " + total, ratio);
        }

        private void ShowStatus(string title, string objectives, float fraction)
        {
            string signature = title + "|" + objectives;
            if (signature != previousStatus)
            {
                if (levelLabel != null) levelLabel.text = title;
                if (objectiveLabel != null) objectiveLabel.text = objectives;
                previousStatus = signature;
            }
            if (objectiveFill != null)
            {
                float clamped = Mathf.Clamp01(fraction);
                if (Mathf.Abs(objectiveFill.fillAmount - clamped) > 0.001f)
                    objectiveFill.fillAmount = clamped;
            }
        }

        private void LocateLevelManager()
        {
            // Optional dependency: keeps this UI compiling even if the gameplay
            // namespace is in an assembly not referenced by this panel's asmdef.
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
                if (behaviours[i] != null &&
                    behaviours[i].GetType().Name == "LevelGamePlayManager")
                {
                    levelManager = behaviours[i];
                    return;
                }
        }

        private static object ReadPublic(object source, string name)
        {
            if (source == null) return null;
            try
            {
                Type type = source.GetType();
                PropertyInfo property = type.GetProperty(name,
                    BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.GetIndexParameters().Length == 0)
                    return property.GetValue(source, null);
                FieldInfo field = type.GetField(name,
                    BindingFlags.Public | BindingFlags.Instance);
                return field != null ? field.GetValue(source) : null;
            }
            catch (Exception) { return null; }
        }

        private static bool TryInt(object value, out int result)
        {
            result = 0;
            if (value is int n) { result = n; return true; }
            return value != null && int.TryParse(value.ToString(), out result);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>World-space recycle buttons for burned SparkResistor components in the active scene.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkBurnedResistorRecyclePrompt : MonoBehaviour
    {
        [Header("Existing scene references")]
        [SerializeField] private SparkObjectShelf shelf;
        [SerializeField] private SparkRecycleBin recycleBin;
        [SerializeField] private Camera viewingCamera;
        [Header("Prompt")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, .25f, 0f);
        [SerializeField, Min(.001f)] private float worldScale = .0025f;
        [SerializeField, Min(.1f)] private float refreshSeconds = .3f;
        [SerializeField] private string buttonText = "RECYCLE BURNED";
        [SerializeField] private Color backgroundColor = new Color(.75f, .2f, .08f, .95f);
        [SerializeField] private Color textColor = Color.white;

        private readonly Dictionary<SparkResistor, Canvas> prompts = new Dictionary<SparkResistor, Canvas>();
        private readonly List<SparkResistor> stale = new List<SparkResistor>();
        private float nextRefresh;
        private Font cachedFont;

        private void Awake() { Resolve(); }
        private void OnEnable() { Resolve(); Refresh(); }
        private void OnDisable() { ClearPrompts(); }
        private void Resolve()
        {
            if (shelf == null) shelf = FindFirstObjectByType<SparkObjectShelf>();
            if (recycleBin == null) recycleBin = FindFirstObjectByType<SparkRecycleBin>();
            if (viewingCamera == null) viewingCamera = Camera.main;
        }
        private void LateUpdate()
        {
            if (Time.unscaledTime >= nextRefresh)
            {
                nextRefresh = Time.unscaledTime + Mathf.Max(.1f, refreshSeconds);
                Refresh();
            }
            Camera cam = viewingCamera != null ? viewingCamera : Camera.main;
            foreach (var entry in prompts)
            {
                if (entry.Key == null || entry.Value == null) continue;
                entry.Value.transform.position = entry.Key.transform.position + worldOffset;
                if (cam != null)
                    entry.Value.transform.rotation = cam.transform.rotation;
            }
        }
        private void Refresh()
        {
            Resolve();
            if (recycleBin == null) return;
            stale.Clear();
            foreach (var entry in prompts)
            {
                SparkResistor item = entry.Key;
                if (item == null || entry.Value == null || !ShouldShow(item)) stale.Add(item);
            }
            foreach (var item in stale)
            {
                if (!prompts.TryGetValue(item, out Canvas canvas)) continue;
                if (canvas != null) Destroy(canvas.gameObject);
                prompts.Remove(item);
            }
            // Discover active-scene resistors, including runtime level clones.
            var items = FindObjectsByType<SparkResistor>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (SparkResistor item in items)
            {
                if (!ShouldShow(item) || prompts.ContainsKey(item)) continue;
                CreatePrompt(item);
            }
        }
        private bool ShouldShow(SparkResistor item)
        {
            if (item == null || !item.gameObject.activeInHierarchy) return false;
            if (recycleBin == null || !recycleBin.IsRecycleBinActive) return false;
            SparkShelfItem shelfItem = item.GetComponentInParent<SparkShelfItem>();
            if (shelfItem != null && shelfItem.IsMoving) return false;
            return item.IsBurned;
        }
        private void CreatePrompt(SparkResistor item)
        {
            GameObject root = new GameObject("RecyclePrompt_" + item.name,
                typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 150;
            RectTransform rect = (RectTransform)root.transform;
            rect.sizeDelta = new Vector2(240f, 58f);
            rect.localScale = Vector3.one * worldScale;
            root.transform.position = item.transform.position + worldOffset;
            GameObject buttonGO = new GameObject("RecycleButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGO.transform.SetParent(root.transform, false);
            RectTransform buttonRect = (RectTransform)buttonGO.transform;
            buttonRect.anchorMin = Vector2.zero;
            buttonRect.anchorMax = Vector2.one;
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            buttonGO.GetComponent<Image>().color = backgroundColor;
            Button button = buttonGO.GetComponent<Button>();
            SparkResistor captured = item;
            button.onClick.AddListener(() =>
            {
                if (recycleBin != null) recycleBin.TryRecycleBurnedSceneResistor(captured);
            });
            GameObject textGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(buttonGO.transform, false);
            RectTransform textRect = (RectTransform)textGO.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Text label = textGO.GetComponent<Text>();
            if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.font = cachedFont;
            label.text = buttonText;
            label.fontSize = 21;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = textColor;
            label.raycastTarget = false;
            prompts.Add(item, canvas);
        }
        private void ClearPrompts()
        {
            foreach (var entry in prompts)
                if (entry.Value != null) Destroy(entry.Value.gameObject);
            prompts.Clear();
        }
    }
}

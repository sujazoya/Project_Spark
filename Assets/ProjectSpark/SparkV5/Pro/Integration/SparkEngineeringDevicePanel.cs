using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>Small, touch-friendly device-operation panel. Only the operator can authorize a native action.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkEngineeringDevicePanel : MonoBehaviour
    {
        private SparkEngineeringDeviceOperator deviceOperator;
        private GameObject canvasRoot;
        private GameObject drawer;
        private TMP_InputField deviceInput;
        private TMP_Text status;
        private static readonly Color Cyan = new Color(.36f,.90f,.98f);
        private static readonly Color Pale = new Color(.91f,.96f,.99f);
        private void Awake() { deviceOperator = GetComponent<SparkEngineeringDeviceOperator>(); }
        private void OnEnable()
        {
            if (!deviceOperator) deviceOperator = GetComponent<SparkEngineeringDeviceOperator>();
            if (deviceOperator) deviceOperator.StatusChanged += UpdateStatus;
        }
        private void OnDisable()
        { if (deviceOperator) deviceOperator.StatusChanged -= UpdateStatus; }
        private void Start() { Build(); UpdateStatus(); }
        private void OnDestroy() { if (canvasRoot) Destroy(canvasRoot); }
        private static RectTransform Rect(string name, Transform parent, Vector2 anchor,
            Vector2 pivot, Vector2 position, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            if (parent) go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }
        private static TMP_Text Text(Transform parent, string name, string value,
            Vector2 position, Vector2 size, int font, Color color)
        {
            var rt = Rect(name, parent, new Vector2(0,1), new Vector2(0,1), position, size);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = value;
            t.fontSize = font;
            t.color = color;
            t.enableWordWrapping = true;
            t.raycastTarget = false;
            return t;
        }
        private static Button Button(Transform parent, string caption, Vector2 position, Vector2 size, Action onClick)
        {
            var rt = Rect(caption + "_Button", parent, new Vector2(0,1), new Vector2(0,1), position, size);
            var image = rt.gameObject.AddComponent<Image>();
            image.color = new Color(.07f,.30f,.41f,.98f);
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => onClick());
            var label = Text(rt, "Caption", caption, Vector2.zero, size, 14, Pale);
            label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            return button;
        }
        private void Build()
        {
            if (canvasRoot || !deviceOperator) return;
            canvasRoot = new GameObject("SPARK_DeviceOperatorV16_Canvas",typeof(RectTransform),
                typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            Canvas canvas = canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 151;
            var scaler = canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920,1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            var tab = Rect("DeviceOperatorToggle", canvasRoot.transform, new Vector2(1,0),
                new Vector2(1,0),new Vector2(-16,16),new Vector2(194,36));
            Image tabBg = tab.gameObject.AddComponent<Image>();
            tabBg.color = new Color(.055f,.20f,.26f,.98f);
            Button tabButton = tab.gameObject.AddComponent<Button>();
            tabButton.targetGraphic = tabBg;
            var tabTitle = Text(tab,"Text","SPARK • DEVICES",Vector2.zero,new Vector2(194,36),15,Cyan);
            tabTitle.alignment = TextAlignmentOptions.Center;
            tabTitle.rectTransform.anchorMin = Vector2.zero;
            tabTitle.rectTransform.anchorMax = Vector2.one;
            tabTitle.rectTransform.offsetMin = Vector2.zero;
            tabTitle.rectTransform.offsetMax = Vector2.zero;
            drawer = Rect("DeviceOperatorDrawer",canvasRoot.transform,new Vector2(1,0),
                new Vector2(1,0),new Vector2(-16,58),new Vector2(476,316)).gameObject;
            drawer.AddComponent<Image>().color = new Color(.012f,.05f,.095f,.985f);
            Text(drawer.transform,"Title","NATIVE DEVICE OPERATOR  /  v16",new Vector2(14,-12),new Vector2(450,29),17,Cyan);
            Text(drawer.transform,"Disclaimer","Assisted mode • one approved action • real state verification",new Vector2(14,-42),new Vector2(450,25),11,Pale);
            var inputRt = Rect("DeviceIdInput",drawer.transform,new Vector2(0,1),new Vector2(0,1),
                new Vector2(14,-79),new Vector2(448,38));
            inputRt.gameObject.AddComponent<Image>().color = new Color(.045f,.13f,.19f,1f);
            deviceInput = inputRt.gameObject.AddComponent<TMP_InputField>();
            var inputText = Text(inputRt,"InputText","",new Vector2(11,-5),new Vector2(425,30),15,Pale);
            inputText.rectTransform.anchorMin = new Vector2(0,0);
            inputText.rectTransform.anchorMax = new Vector2(1,1);
            inputText.rectTransform.offsetMin = new Vector2(11,4);
            inputText.rectTransform.offsetMax = new Vector2(-11,-4);
            deviceInput.textViewport = inputRt;
            deviceInput.textComponent = inputText as TextMeshProUGUI;
            deviceInput.text = "";
            deviceInput.placeholder = Text(inputRt,"Placeholder","Exact device ID from Inspector",new Vector2(12,-7),new Vector2(418,28),13,new Color(.55f,.7f,.75f));
            Button(drawer.transform,"PROPOSE ON",new Vector2(14,-127),new Vector2(143,36),
                () => deviceOperator.Propose(deviceInput.text.Trim(),true));
            Button(drawer.transform,"PROPOSE OFF",new Vector2(166,-127),new Vector2(143,36),
                () => deviceOperator.Propose(deviceInput.text.Trim(),false));
            Button(drawer.transform,"APPROVE",new Vector2(319,-127),new Vector2(143,36),
                () => deviceOperator.Approve());
            Button(drawer.transform,"INSPECT",new Vector2(14,-174),new Vector2(143,34),
                () => deviceOperator.Inspect());
            Button(drawer.transform,"CANCEL",new Vector2(166,-174),new Vector2(143,34),
                () => deviceOperator.Cancel());
            Button(drawer.transform,"CLOSE",new Vector2(319,-174),new Vector2(143,34),
                () => drawer.SetActive(false));
            status = Text(drawer.transform,"Status","Ready",new Vector2(14,-221),new Vector2(448,83),12,Pale);
            tabButton.onClick.AddListener(() => drawer.SetActive(!drawer.activeSelf));
            drawer.SetActive(false);
            if (!EventSystem.current)
            {
                var es = new GameObject("SparkDeviceOperator_EventSystem");
                es.transform.SetParent(canvasRoot.transform,false);
                es.AddComponent<EventSystem>();
                Type newUi = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (newUi != null && typeof(BaseInputModule).IsAssignableFrom(newUi)) es.AddComponent(newUi);
                else es.AddComponent<StandaloneInputModule>();
            }
        }
        private void UpdateStatus()
        { if (status && deviceOperator) status.text = deviceOperator.LastStatus; }
    }
}

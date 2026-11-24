using System;
using ProjectSpark.AAAArchitecture;
using ProjectSpark.UIManagement;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.SparkV5.Pro
{
    [DisallowMultipleComponent]
    public sealed class SparkProLivePanel : MonoBehaviour
    {
        public const string PanelId = "assistant";
        public const int CurrentPrefabVersion = 2;
        [SerializeField] private int prefabVersion;
        public int PrefabVersion => prefabVersion;
        private static readonly Color Cyan = new Color32(113, 237, 255, 255);
        private static readonly Color Panel = new Color32(10, 24, 36, 252);
        private static readonly Color Surface = new Color32(17, 39, 54, 255);
        private static readonly Color Message = new Color32(17, 34, 48, 255);
        private static readonly Color Muted = new Color32(165, 195, 209, 255);
        private static readonly Color Live = new Color32(255, 178, 134, 255);
        private static readonly Color Ink = new Color32(239, 248, 253, 255);

        [SerializeField] RectTransform shell, messages, settings;
        [SerializeField] ScrollRect scroll;
        [SerializeField] TMP_InputField input;
        [SerializeField] TMP_Text transcript, connection, characterStatus, recognition, device, speechToggle, wakeToggle, liveToggle;
        [SerializeField] TMP_Text thresholdValue, silenceValue;
        [SerializeField] Slider sensitivity, silence;
        [SerializeField] Button close, options, send, handsFree, mute, interrupt, clear, cycleDevice, toggleWake, toggleSpeech;
        [SerializeField] SparkProPushToTalkButton talk;
        [SerializeField] Image[] micWave, voiceWave;
        SparkProExecutive executive;
        SparkProVoiceInput microphone;
        SparkUIManager manager;
        GameObject previousFocus;
        readonly float[] outputSamples = new float[256];
        float nextDisplayUpdate;
        Rect monitorArea;
        bool showVisual;

        public bool Visible => gameObject.activeInHierarchy;

        public static SparkProLivePanel Install(SparkProExecutive executive, SparkUIManager manager)
        {
            if (!executive || !manager)
                return null;
            if (manager.Find(PanelId))
                return null;

            var prefab = Resources.Load<SparkProLivePanel>("SparkProLivePanel");
            var panel = prefab ? Instantiate(prefab) : Build();
            panel.Bind(executive, manager);
            manager.RegisterNative(PanelId, "Spark Live", SparkUILayer.Modal, "workshop", true,
                panel.SetVisible, () => panel && panel.Visible);

            var launcher = new GameObject("Spark Live Controls", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            launcher.transform.SetParent(manager.transform, false);
            SetupCanvas(launcher, 30900);
            var bounds = (RectTransform)launcher.transform;
            var launch = MakeButton(bounds, "Open Spark Live", "Spark Live", out _, () => manager.Open(PanelId));
            Anchor((RectTransform)launch.transform, 1, 1, 1, 1, -173, -56, -16, -12);
            if (executive.Hologram)
                executive.Hologram.AttachControls(bounds);
            return panel;
        }

        private void Bind(SparkProExecutive value, SparkUIManager ui)
        {
            executive = value;
            manager = ui;
            microphone = executive.Microphone;
            talk.Bind(microphone);
            if (microphone)
            {
                sensitivity.SetValueWithoutNotify(microphone.Sensitivity);
                silence.SetValueWithoutNotify(microphone.EndSilence);
            }
            close.onClick.AddListener(() => manager.Close(PanelId));
            options.onClick.AddListener(() => settings.gameObject.SetActive(!settings.gameObject.activeSelf));
            send.onClick.AddListener(SubmitText);
            input.onSubmit.AddListener(_ => SubmitText());
            handsFree.onClick.AddListener(ToggleListening);
            mute.onClick.AddListener(() => executive?.MuteMicrophone());
            interrupt.onClick.AddListener(executive.Interrupt);
            clear.onClick.AddListener(executive.ClearConversation);
            cycleDevice.onClick.AddListener(CycleDevice);
            toggleWake.onClick.AddListener(() => microphone?.SetWakePhrase(!microphone.WakePhraseRequired));
            toggleSpeech.onClick.AddListener(() => executive.SetSpeakAnswers(!executive.SpeakAnswers));
            sensitivity.onValueChanged.AddListener(value => microphone?.SetSensitivity(value));
            silence.onValueChanged.AddListener(value => microphone?.SetEndSilence(value));
        }

        private void SetVisible(bool visible)
        {
            if (visible)
            {
                previousFocus = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
                gameObject.SetActive(true);
                Canvas.ForceUpdateCanvases();
                Fit();
                close.Select();
            }
            else if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
                if (previousFocus && previousFocus.activeInHierarchy && EventSystem.current)
                    EventSystem.current.SetSelectedGameObject(previousFocus);
                previousFocus = null;
            }
        }

        private void OnEnable()
        {
            if (!executive)
                return;
            executive.ConversationChanged += RefreshConversation;
            executive.VisualOpened += OnVisualOpened;
            if (microphone)
            {
                microphone.ProviderStatusChanged += OnProviderStatus;
                microphone.SetHealthMonitoring(true);
            }
            settings.gameObject.SetActive(false);
            RefreshConversation();
            RefreshSignals();
        }

        private void OnDisable()
        {
            if (!executive)
                return;
            executive.ConversationChanged -= RefreshConversation;
            executive.VisualOpened -= OnVisualOpened;
            if (microphone)
            {
                microphone.ProviderStatusChanged -= OnProviderStatus;
                microphone.SetHealthMonitoring(false);
            }
            if (showVisual)
                microphone?.Stop();
            else
                executive.EndLiveSession();
            showVisual = false;
        }

        private void OnVisualOpened()
        {
            showVisual = true;
            manager.Close(PanelId);
        }

        private void OnDestroy()
        {
            if (manager && manager.Find(PanelId))
                manager.Unregister(PanelId);
        }

        public void SubmitText()
        {
            if (!executive || string.IsNullOrWhiteSpace(input.text))
                return;
            string question = input.text.Trim();
            input.SetTextWithoutNotify(string.Empty);
            executive.Ask(question);
        }

        private void ToggleListening()
        {
            if (!microphone)
                return;
            if (microphone.ContinuousRequested) microphone.StopHandsFree();
            else microphone.StartHandsFree();
        }

        private void CycleDevice()
        {
            if (!microphone)
                return;
            var devices = Microphone.devices;
            int i = Array.IndexOf(devices, microphone.Device) + 1;
            microphone.SetDevice(i >= devices.Length ? string.Empty : devices[i]);
            RefreshSignals();
        }

        private void Update()
        {
            if (!executive)
                return;
            Fit();
            if (Time.unscaledTime < nextDisplayUpdate)
                return;
            nextDisplayUpdate = Time.unscaledTime + .075f;
            RefreshSignals();
        }

        private void Fit()
        {
            Rect safe = Screen.safeArea;
            if (monitorArea == safe || Screen.width <= 0 || Screen.height <= 0)
                return;
            monitorArea = safe;
            var rect = (RectTransform)transform;
            Vector2 size = rect.rect.size;
            if (size.x <= 0 || size.y <= 0)
            {
                monitorArea = default;
                return;
            }
            Vector2 available = Vector2.Scale(size, new Vector2(safe.width / Screen.width, safe.height / Screen.height));
            shell.sizeDelta = new Vector2(Mathf.Max(320, Mathf.Min(1000, available.x - 20)),
                Mathf.Max(455, Mathf.Min(770, available.y - 20)));
            shell.anchoredPosition = Vector2.Scale(size,
                new Vector2(safe.center.x / Screen.width - .5f, safe.center.y / Screen.height - .5f));
        }

        private void RefreshConversation()
        {
            int index = 0;
            foreach (var turn in executive.History)
            {
                if (index == messages.childCount)
                {
                    var row = SparkAAAUIFactory.Rect("Message", messages);
                    SparkAAAUIFactory.Image(row, Message);
                    SparkAAAUIFactory.Vertical(row, 9, 2);
                    SparkAAAUIFactory.Text("Speaker", row, "", 13, Cyan, 18, true);
                    var body = SparkAAAUIFactory.Text("Text", row, "", 18, Ink);
                    body.alignment = TextAlignmentOptions.TopLeft;
                    body.overflowMode = TextOverflowModes.Overflow;
                }
                var child = messages.GetChild(index++);
                child.gameObject.SetActive(true);
                child.GetChild(0).GetComponent<TMP_Text>().text = turn.Speaker;
                child.GetChild(1).GetComponent<TMP_Text>().text = turn.Text;
            }
            for (int i = index; i < messages.childCount; i++)
                messages.GetChild(i).gameObject.SetActive(false);

            transcript.text = string.IsNullOrWhiteSpace(executive.LastTranscript)
                ? "Hold Talk to speak, or type a question." : "Heard  /  " + executive.LastTranscript;
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 0;
        }

        private void OnProviderStatus(string status) => connection.text = status;

        private void RefreshSignals()
        {
            characterStatus.text = executive.PlaybackActive ? "Speaking" : executive.CharacterState.ToString();
            connection.text = microphone ? microphone.ProviderStatus : "Text mode";
            recognition.text = microphone ? microphone.Status : "Microphone unavailable";
            recognition.color = microphone && microphone.State == SparkVoiceInputState.Listening ? Live : Muted;
            device.text = !microphone || string.IsNullOrEmpty(microphone.Device) ? "System default" : microphone.Device;
            bool working = microphone && microphone.ContinuousRequested;
            liveToggle.text = working ? "End live" : "Live";
            handsFree.interactable = microphone;
            mute.interactable = microphone && microphone.State != SparkVoiceInputState.Off;
            wakeToggle.text = microphone && microphone.WakePhraseRequired ? "Wake phrase on" : "Wake phrase off";
            speechToggle.text = !executive.SpeechOutputAvailable ? "Voice playback unavailable" :
                executive.SpeakAnswers ? "Spoken replies on" : "Spoken replies off";
            if (microphone)
            {
                thresholdValue.text = $"Voice threshold  {microphone.Sensitivity:0.000}";
                silenceValue.text = $"End silence  {microphone.EndSilence:0.00}s";
                PushWave(micWave, Mathf.Clamp01(microphone.InputLevel * 7));
            }
            PushWave(voiceWave, Mathf.Clamp01(executive.SampleVoice(outputSamples) * 9));
        }

        private static void PushWave(Image[] bars, float amplitude)
        {
            for (int i = 0; i < bars.Length; i++)
            {
                var scale = bars[i].rectTransform.localScale;
                scale.y = i + 1 < bars.Length ? bars[i + 1].rectTransform.localScale.y : .12f + amplitude * .88f;
                bars[i].rectTransform.localScale = scale;
            }
        }

        public static SparkProLivePanel Build()
        {
            var root = new GameObject("Spark_Live_Panel", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.SetActive(false);
            SetupCanvas(root, 31520);
            var panel = root.AddComponent<SparkProLivePanel>();
            panel.prefabVersion = CurrentPrefabVersion;
            var background = SparkAAAUIFactory.Image((RectTransform)root.transform, new Color32(1, 9, 16, 212));
            background.raycastTarget = true;
            var shell = panel.shell = SparkAAAUIFactory.Rect("Console", root.transform);
            shell.anchorMin = shell.anchorMax = new Vector2(.5f, .5f);
            SparkAAAUIFactory.Image(shell, Panel);
            panel.monitorArea = default;
            var top = Region(shell, "Header", 16, 0, 16, 0, 1, 1, -72, -6);
            var heading = Label(top, "Spark Live", "Spark Live", 29, Ink, 0, 0, 170, 35, true);
            heading.enableAutoSizing = true;
            heading.fontSizeMin = 15;
            heading.fontSizeMax = 29;
            panel.connection = Label(top, "Connection", "", 15, Cyan, 0, 29, 172, 19);
            panel.characterStatus = Label(top, "Character state", "", 14, Muted, 0, 49, 172, 0);
            panel.options = MakeButton(top, "Settings", "Settings", out _);
            Anchor((RectTransform)panel.options.transform, 1, 0, 1, 1, -158, 16, -66, -16);
            panel.close = MakeButton(top, "Close", "Close", out _);
            Anchor((RectTransform)panel.close.transform, 1, 0, 1, 1, -60, 16, 0, -16);

            var meters = Region(shell, "Voice visualizer", 16, 0, 16, 0, 1, 1, -131, -76);
            BuildWave(meters, "Microphone", Cyan, 0f, .49f, out panel.micWave);
            BuildWave(meters, "Spark playback", new Color32(144, 242, 187, 255), .51f, 1f, out panel.voiceWave);

            var chat = Region(shell, "Conversation", 16, 136, 16, 150);
            panel.scroll = SparkAAAUIFactory.Scroll("Scroll view", chat, out panel.messages);
            SparkAAAUIFactory.Stretch((RectTransform)panel.scroll.transform);
            SparkAAAUIFactory.Vertical(panel.messages, 8, 6);
            panel.scroll.inertia = false;
            var transcript = Region(shell, "Live transcript", 16, 0, 16, 0, 0, 0, 123, 151);
            panel.transcript = Label(transcript, "Transcript", "", 16, Muted, 0, 0, 0, 0);

            var compose = Region(shell, "Question input", 16, 0, 16, 0, 0, 0, 74, 119);
            panel.input = SparkAAAUIFactory.Search(compose);
            panel.input.characterLimit = 1000;
            ((TMP_Text)panel.input.placeholder).text = "Ask about your circuit or lesson";
            Anchor((RectTransform)panel.input.transform, 0, 0, 1, 1, 0, 0, -85, 0);
            panel.send = MakeButton(compose, "Send", "Send", out _);
            Anchor((RectTransform)panel.send.transform, 1, 0, 1, 1, -76, 0, 0, 0);

            var controls = Region(shell, "Controls", 16, 0, 16, 0, 0, 0, 23, 67);
            var talkButton = Control(controls, "Talk", "Hold Talk", 0, out _);
            panel.talk = talkButton.gameObject.AddComponent<SparkProPushToTalkButton>();
            panel.handsFree = Control(controls, "Live", "Live", 1, out panel.liveToggle);
            panel.mute = Control(controls, "Mute", "Mute mic", 2, out _);
            panel.interrupt = Control(controls, "Interrupt", "Interrupt", 3, out _);
            panel.clear = Control(controls, "Clear", "Clear", 4, out _);
            panel.recognition = Label(Region(shell, "Footer", 16, 0, 16, 0, 0, 0, 1, 21),
                "Recognition status", "", 13, Muted, 0, 0, 0, 0);

            panel.settings = Region(shell, "Voice settings", 16, 136, 16, 150);
            var settingsBackground = SparkAAAUIFactory.Image(panel.settings, Surface);
            settingsBackground.raycastTarget = true;
            var settingsScroll = SparkAAAUIFactory.Scroll("Settings scroll", panel.settings, out var settingsRows);
            SparkAAAUIFactory.Stretch((RectTransform)settingsScroll.transform);
            SparkAAAUIFactory.Vertical(settingsRows, 12, 7);
            SparkAAAUIFactory.Text("Microphone heading", settingsRows, "Microphone device", 18, Cyan, 24, true);
            panel.cycleDevice = MakeButton(settingsRows, "Cycle device", "System default", out panel.device);
            SparkAAAUIFactory.Size((RectTransform)panel.cycleDevice.transform, -1, 39);
            panel.thresholdValue = SparkAAAUIFactory.Text("Sensitivity readout", settingsRows, "", 16, Ink, 23);
            panel.sensitivity = SettingSlider(settingsRows, "Sensitivity", .005f, .15f);
            panel.silenceValue = SparkAAAUIFactory.Text("Silence readout", settingsRows, "", 16, Ink, 23);
            panel.silence = SettingSlider(settingsRows, "End silence", .3f, 1.5f);
            panel.toggleWake = MakeButton(settingsRows, "Wake phrase", "Wake phrase off", out panel.wakeToggle);
            SparkAAAUIFactory.Size((RectTransform)panel.toggleWake.transform, -1, 38);
            panel.toggleSpeech = MakeButton(settingsRows, "Spoken replies", "Spoken replies on", out panel.speechToggle);
            SparkAAAUIFactory.Size((RectTransform)panel.toggleSpeech.transform, -1, 38);
            var explanation = SparkAAAUIFactory.Text("Voice mode", settingsRows,
                "Live alternates between listening and speaking. With wake phrase on, begin each voice turn with “Hey Spark”.", 15, Muted);
            explanation.overflowMode = TextOverflowModes.Overflow;
            panel.settings.gameObject.SetActive(false);
            return panel;
        }

        private static Slider SettingSlider(RectTransform parent, string name, float min, float max)
        {
            var rect = SparkAAAUIFactory.Rect(name, parent);
            SparkAAAUIFactory.Size(rect, -1, 27);
            var track = SparkAAAUIFactory.Image(rect, Panel);
            track.raycastTarget = true;
            var trackFill = SparkAAAUIFactory.Rect("Track", rect);
            SparkAAAUIFactory.Stretch(trackFill, 8, 10, 8, 10);
            SparkAAAUIFactory.Image(trackFill, Cyan);
            var handleArea = SparkAAAUIFactory.Rect("Handle area", rect);
            SparkAAAUIFactory.Stretch(handleArea, 9, 3, 9, 3);
            var handle = SparkAAAUIFactory.Rect("Handle", handleArea);
            handle.sizeDelta = new Vector2(15, 19);
            var image = SparkAAAUIFactory.Image(handle, Ink);
            image.raycastTarget = true;
            var slider = rect.gameObject.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.handleRect = handle;
            slider.targetGraphic = image;
            return slider;
        }

        private static void BuildWave(RectTransform parent, string name, Color color, float left, float right, out Image[] bars)
        {
            var box = SparkAAAUIFactory.Rect(name, parent);
            Anchor(box, left, 0, right, 1, 0, 0, 0, 0);
            SparkAAAUIFactory.Image(box, Surface);
            Label(box, "Signal name", name, 14, color, 8, 1, 6, 30);
            bars = new Image[28];
            for (int i = 0; i < bars.Length; i++)
            {
                var band = SparkAAAUIFactory.Rect("Signal", box);
                Anchor(band, (i + .5f) / bars.Length, 0, (i + .5f) / bars.Length, 0, -1.5f, 5, 1.5f, 31);
                bars[i] = SparkAAAUIFactory.Image(band, color);
                band.localScale = new Vector3(1, .12f, 1);
            }
        }

        internal static void SetupCanvas(GameObject root, int order)
        {
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440, 900);
            scaler.matchWidthOrHeight = .55f;
        }

        internal static Button MakeButton(RectTransform parent, string name, string caption, out TMP_Text label, Action clicked = null)
        {
            var button = SparkAAAUIFactory.Button(name, parent, caption, out var text);
            button.image.color = Surface;
            text.fontSize = 16;
            text.enableAutoSizing = true;
            text.fontSizeMin = 12;
            text.fontSizeMax = 16;
            label = text;
            if (clicked != null) button.onClick.AddListener(() => clicked());
            return button;
        }

        private static Button Control(RectTransform row, string name, string caption, int index, out TMP_Text label)
        {
            Button button = MakeButton(row, name, caption, out label);
            Anchor((RectTransform)button.transform, index * .2f, 0, (index + 1) * .2f, 1, 0, 0, -5, 0);
            return button;
        }

        private static TMP_Text Label(RectTransform parent, string name, string caption, int size, Color color,
            float left, float top, float right, float bottom, bool bold = false)
        {
            var label = SparkAAAUIFactory.Text(name, parent, caption, size, color, bold: bold);
            SparkAAAUIFactory.Stretch(label.rectTransform, left, bottom, right, top);
            return label;
        }

        private static RectTransform Region(RectTransform parent, string name, float left, float top, float right, float bottom)
        {
            var rect = SparkAAAUIFactory.Rect(name, parent);
            SparkAAAUIFactory.Stretch(rect, left, bottom, right, top);
            return rect;
        }

        private static RectTransform Region(RectTransform parent, string name, float left, float top, float right,
            float bottom, float minY, float maxY, float low, float high)
        {
            var rect = SparkAAAUIFactory.Rect(name, parent);
            Anchor(rect, 0, minY, 1, maxY, left, bottom + low, -right, high - top);
            return rect;
        }

        internal static void Anchor(RectTransform rect, float x1, float y1, float x2, float y2, float left, float bottom, float right, float top)
        {
            rect.anchorMin = new Vector2(x1, y1);
            rect.anchorMax = new Vector2(x2, y2);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(right, top);
        }
    }
}


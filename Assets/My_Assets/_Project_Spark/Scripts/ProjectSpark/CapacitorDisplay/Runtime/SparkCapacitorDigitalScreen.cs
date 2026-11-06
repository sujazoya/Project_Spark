using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    /// <summary>World-space capacitor display skin. All data comes from SparkCapacitorDigitalDisplay.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkCapacitorDigitalScreen : MonoBehaviour
    {
        public enum DisplayPage { Overview, Voltage, Current, Energy }
        [SerializeField] private SparkCapacitorDigitalDisplay telemetry;
        [SerializeField] private TMP_Text titleText, statusText, valueText, unitText;
        [SerializeField] private TMP_Text voltageText, currentText, capacitanceText, ratedText;
        [SerializeField] private TMP_Text percentageText, energyText, chargeText, detailText;
        [SerializeField] private Image chargeFill;
        [SerializeField] private Image[] historyBars;
        [SerializeField] private Button[] pageButtons;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Button pauseGraphButton;
        [SerializeField, Min(0.05f)] private float refreshInterval = 0.10f;
        [SerializeField, Min(0.05f)] private float graphSampleInterval = 0.20f;
        [SerializeField] private DisplayPage page = DisplayPage.Overview;
        [SerializeField] private bool freezeGraph;
        private float[] history;
        private int historyIndex, historyCount;
        private float nextRefresh, nextSample;
        private bool hooked;
        private static readonly Color Cyan = new Color(.15f,.89f,1f,1f);
        private static readonly Color Green = new Color(.22f,1f,.63f,1f);
        private static readonly Color Dim = new Color(.08f,.25f,.31f,1f);
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private void OnEnable()
        {
            if (telemetry == null) telemetry = GetComponentInParent<SparkCapacitorDigitalDisplay>();
            RegisterButtons();
            nextRefresh = nextSample = 0f;
            RefreshNow();
        }
        private void OnDisable() => UnregisterButtons();
        public void Configure(SparkCapacitorDigitalDisplay source) { telemetry = source; RefreshNow(); }
        private void RegisterButtons()
        {
            if (hooked) return;
            if (pageButtons != null && pageButtons.Length >= 4)
            {
                if (pageButtons[0] != null) pageButtons[0].onClick.AddListener(ShowOverview);
                if (pageButtons[1] != null) pageButtons[1].onClick.AddListener(ShowVoltage);
                if (pageButtons[2] != null) pageButtons[2].onClick.AddListener(ShowCurrent);
                if (pageButtons[3] != null) pageButtons[3].onClick.AddListener(ShowEnergy);
            }
            if (nextPageButton != null) nextPageButton.onClick.AddListener(NextPage);
            if (pauseGraphButton != null) pauseGraphButton.onClick.AddListener(ToggleGraphPause);
            hooked = true;
        }
        private void UnregisterButtons()
        {
            if (!hooked) return;
            if (pageButtons != null && pageButtons.Length >= 4)
            {
                if (pageButtons[0] != null) pageButtons[0].onClick.RemoveListener(ShowOverview);
                if (pageButtons[1] != null) pageButtons[1].onClick.RemoveListener(ShowVoltage);
                if (pageButtons[2] != null) pageButtons[2].onClick.RemoveListener(ShowCurrent);
                if (pageButtons[3] != null) pageButtons[3].onClick.RemoveListener(ShowEnergy);
            }
            if (nextPageButton != null) nextPageButton.onClick.RemoveListener(NextPage);
            if (pauseGraphButton != null) pauseGraphButton.onClick.RemoveListener(ToggleGraphPause);
            hooked = false;
        }
        private void ShowOverview() => SetPage(0);
        private void ShowVoltage() => SetPage(1);
        private void ShowCurrent() => SetPage(2);
        private void ShowEnergy() => SetPage(3);
        private void Update()
        {
            if (Time.unscaledTime >= nextRefresh)
            {
                nextRefresh = Time.unscaledTime + Mathf.Max(.05f, refreshInterval);
                RefreshNow();
            }
            if (Time.unscaledTime >= nextSample)
            {
                nextSample = Time.unscaledTime + Mathf.Max(.05f, graphSampleInterval);
                SampleGraph();
            }
        }
        public void SetPage(int index) { page = (DisplayPage)Mathf.Clamp(index, 0, 3); RefreshNow(); }
        public void NextPage() => SetPage(((int)page + 1) % 4);
        public void ToggleGraphPause() { freezeGraph = !freezeGraph; RefreshNow(); }
        public void ResetGraph()
        {
            historyCount = historyIndex = 0;
            if (history != null) Array.Clear(history, 0, history.Length);
            DrawGraph();
        }
        private void SampleGraph()
        {
            if (freezeGraph || historyBars == null || historyBars.Length == 0 || telemetry == null || !telemetry.HasValidReading) return;
            if (history == null || history.Length != historyBars.Length) { history = new float[historyBars.Length]; historyIndex = historyCount = 0; }
            history[historyIndex] = telemetry.ChargeFraction;
            historyIndex = (historyIndex + 1) % history.Length;
            historyCount = Mathf.Min(historyCount + 1, history.Length);
            DrawGraph();
        }
        private void DrawGraph()
        {
            if (historyBars == null || historyBars.Length == 0) return;
            for (int i = 0; i < historyBars.Length; i++)
            {
                if (historyBars[i] == null) continue;
                bool filled = history != null && i >= historyBars.Length - historyCount;
                int offset = i - (historyBars.Length - historyCount);
                int index = filled ? (historyIndex - historyCount + offset + history.Length * 2) % history.Length : 0;
                float level = filled ? history[index] : 0f;
                var rt = historyBars[i].rectTransform;
                Vector2 size = rt.sizeDelta;
                size.y = Mathf.Max(2f, level * 90f);
                rt.sizeDelta = size;
                historyBars[i].color = filled ? Color.Lerp(Cyan, Green, level) : Dim;
            }
        }
        public void RefreshNow()
        {
            bool valid = telemetry != null && telemetry.HasValidReading;
            Set(titleText, "PROJECT SPARK  /  CAPACITOR ANALYZER");
            Set(statusText, telemetry == null ? "NO SENSOR" : valid ? telemetry.Status.ToString().ToUpperInvariant() : telemetry.Status.ToString().ToUpperInvariant());
            if (!valid)
            {
                Set(valueText, "--.---"); Set(unitText, "");
                Set(voltageText, "--"); Set(currentText, "--"); Set(capacitanceText, telemetry != null && telemetry.Capacitor != null ? Micro(telemetry.CapacitanceFarads) + " uF" : "--");
                Set(ratedText, telemetry != null && telemetry.Capacitor != null ? F(telemetry.RatedVoltage, "0.##") + " V" : "--");
                Set(percentageText, "-- %"); Set(energyText, "--"); Set(chargeText, "--");
                Set(detailText, "AWAITING ACCEPTED ELECTRICAL SOLVE");
                if (chargeFill != null) chargeFill.fillAmount = 0f;
                return;
            }
            float v = telemetry.Voltage;
            float i = telemetry.Current;
            float e = telemetry.StoredEnergyJoules;
            float q = telemetry.ChargeCoulombs;
            float pct = telemetry.ChargeFraction * 100f;
            switch (page)
            {
                case DisplayPage.Current: Set(valueText, F(i*1000f,"0.000")); Set(unitText,"mA"); break;
                case DisplayPage.Energy: Set(valueText, F(e*1000f,"0.000")); Set(unitText,"mJ"); break;
                default: Set(valueText,F(v,"0.000")); Set(unitText,"V"); break;
            }
            Set(voltageText, F(v,"0.000") + " V");
            Set(currentText, F(i*1000f,"0.000") + " mA");
            Set(capacitanceText, Micro(telemetry.CapacitanceFarads) + " uF");
            Set(ratedText, F(telemetry.RatedVoltage,"0.##") + " V");
            Set(percentageText,F(pct,"0.0") + " %");
            Set(energyText,F(e*1000f,"0.000") + " mJ");
            Set(chargeText,F(q*1000f,"0.000") + " mC");
            Set(detailText,"CHARGE % RELATIVE TO " + F(telemetry.TargetChargeVoltage,"0.##") + " V TARGET  •  SOLVER LIVE");
            if (chargeFill != null) chargeFill.fillAmount = telemetry.ChargeFraction;
        }
        private static string F(float x, string format) => x.ToString(format, Inv);
        private static string Micro(float farads) => (farads * 1000000f).ToString("0.##", Inv);
        private static void Set(TMP_Text tmp, string value) { if (tmp != null && tmp.text != value) tmp.text = value; }
    }
}

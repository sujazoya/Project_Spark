using System;
using System.Collections.Generic;
using AAAUI.VFX;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel79Process
    {
        public TextMesh[] pinLabels = Array.Empty<TextMesh>();
        public Button cableViewButton, openFeedbackButton, focusWireButton;

        private SignalWireBuilder wireBuilder;
        private SparkWireRegistry wireRegistry;
        private readonly Dictionary<Renderer, bool> hiddenCableRenderers = new();
        private bool focusCables, choseCableView;
        private int suggestedWire = -1, shownWire = -2, shownHover = -2;
        private Camera interactionCamera;

        private void BindEquipmentWiring()
        {
            wireRegistry = FindFirstObjectByType<SparkWireRegistry>();
            if (cableViewButton && wireRegistry)
            {
                wireRegistry.WiresChanged += RefreshCableView;
                wireRegistry.SelectionChanged += OnCableSelected;
                cableViewButton.interactable = true;
            }
            if (pinLabels.Length == pins.Length)
            {
                interactionCamera = UnityEngine.Camera.main;
                wireBuilder = FindFirstObjectByType<SignalWireBuilder>();
                if (wireBuilder) wireBuilder.TerminalPickerOverride = PickOriginalSocket;
            }
        }

        private void OnDisable()
        {
            if (wireRegistry)
            {
                wireRegistry.WiresChanged -= RefreshCableView;
                wireRegistry.SelectionChanged -= OnCableSelected;
            }
            RestoreCableRenderers();
            if (!wireBuilder || wireBuilder.TerminalPickerOverride != PickOriginalSocket) return;
            if (wireBuilder.IsDrawing) wireBuilder.CancelWire();
            wireBuilder.TerminalPickerOverride = null;
        }

        public void ToggleCableView()
        {
            choseCableView = true;
            focusCables = !focusCables;
            RefreshCableView();
            Log(focusCables ? "Equipment view highlights W037 and the selected cable." : "All installed cables are visible.");
        }

        public void ResetWorkbenchView() => GetComponent<SparkEquipmentWorkbenchView>()?.ResetView();

        public void FocusNextWire()
        {
            var pair = Wires[suggestedWire >= 0 ? suggestedWire : Feedback];
            GetComponent<SparkEquipmentWorkbenchView>()?.FocusPair(pins[pair.a].transform.position, pins[pair.b].transform.position);
        }

        private void OnCableSelected(string wireId) => RefreshCableView();

        private void RestoreCableRenderers()
        {
            foreach (var entry in hiddenCableRenderers)
                if (entry.Key) entry.Key.forceRenderingOff = entry.Value;
            hiddenCableRenderers.Clear();
        }

        private bool IsFeedback(SparkWireRecord record)
        {
            var (a, b) = Wires[Feedback];
            return record.TerminalA == pins[a] && record.TerminalB == pins[b] ||
                record.TerminalA == pins[b] && record.TerminalB == pins[a];
        }

        private void RefreshCableView()
        {
            RestoreCableRenderers();
            if (!focusCables || !wireRegistry) return;
            foreach (var record in wireRegistry.Wires)
            {
                if (!record.Path || !record.TerminalA || !record.TerminalB || record.WireId == wireRegistry.SelectedWireId ||
                    !record.TerminalA.transform.IsChildOf(transform) || !record.TerminalB.transform.IsChildOf(transform) || IsFeedback(record)) continue;
                foreach (var renderer in record.Path.GetComponentsInChildren<Renderer>(true))
                {
                    hiddenCableRenderers[renderer] = renderer.forceRenderingOff;
                    renderer.forceRenderingOff = true;
                }
            }
        }

        private SparkTerminal PickOriginalSocket(Vector2 screenPosition, SparkTerminal start)
        {
            if (!interactionCamera) return null;
            SparkTerminal closest = null;
            float radius = Mathf.Clamp(interactionCamera.pixelHeight * .011f, 6f, 16f);
            float best = radius * radius;
            foreach (var pin in pins)
            {
                if (!pin || pin == start || !pin.isActiveAndEnabled || !pin.Owner ||
                    !pin.Owner.isActiveAndEnabled || !pin.Owner.InteractionsEnabled) continue;
                Vector3 point = interactionCamera.WorldToScreenPoint(pin.transform.position);
                float distance = ((Vector2)point - screenPosition).sqrMagnitude;
                if (point.z <= 0 || distance >= best) continue;
                closest = pin;
                best = distance;
            }
            return closest;
        }

        public void OpenFeedbackWire()
        {
            if (!armed || !wireRegistry) return;
            wireRegistry.RefreshNow();
            foreach (var record in wireRegistry.Wires)
            {
                if (!IsFeedback(record)) continue;
                wireRegistry.RemoveWire(record.WireId);
                return;
            }
        }

        private void PresentEquipmentWiring()
        {
            if (stage >= 1 && !choseCableView)
            {
                choseCableView = true;
                focusCables = true;
                RefreshCableView();
            }
            if (pinLabels.Length == pins.Length)
            {
                int hover = wireBuilder && wireBuilder.HoveredTerminal ? Array.IndexOf(pins, wireBuilder.HoveredTerminal) : -1;
                if (shownWire != suggestedWire || shownHover != hover)
                {
                    var pair = suggestedWire >= 0 ? Wires[suggestedWire] : (-1, -1);
                    for(int i=0;i<pinLabels.Length;i++)
                        if (pinLabels[i]) pinLabels[i].gameObject.SetActive(i == pair.Item1 || i == pair.Item2 || i == hover);
                    shownWire = suggestedWire;
                    shownHover = hover;
                }
            }
            if (openFeedbackButton) openFeedbackButton.interactable = armed && wiredFeedback && wireRegistry;
        }
    }
}

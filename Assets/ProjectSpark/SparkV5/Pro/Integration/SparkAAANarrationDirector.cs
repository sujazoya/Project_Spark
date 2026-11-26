using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    [DisallowMultipleComponent]
    public sealed class SparkAAANarrationDirector : MonoBehaviour
    {
        internal enum Milestone { Carry, Pointer, Flight, Align, Verified, Stopped, Device }

        [SerializeField] private SparkProExecutive executive;
        [SerializeField] private bool publishLiveText = true;
        [SerializeField] private bool enableSpeechWhenAvailable = true;
        [SerializeField, Range(.1f, 5f)] private float stageCooldownSeconds = .4f;
        [SerializeField, Range(1f, 20f)] private float speechStartupGraceSeconds = 12f;
        [SerializeField, Range(1, 8)] private int maxQueuedStages = 4;

        private readonly Queue<string> pending = new Queue<string>(4);
        private readonly HashSet<Milestone> announced = new HashSet<Milestone>();
        private readonly Queue<string> confirmations = new Queue<string>(4);
        private Transform layout;
        private bool awaitingSpeech, heardSpeech, voiceFault;
        private float requestedAt, nextStage;
        private int published, dropped;
        private string lastStage = "Idle";

        public string LastStage => lastStage;
        public int PendingStages => pending.Count + confirmations.Count;
        public int PublishedStages => published;
        public int SuppressedStages => dropped;
        public bool SpeechInTextFallback => voiceFault || !executive || !executive.SpeechOutputAvailable;

        private void Awake()
        {
            if (!executive) executive = GetComponent<SparkProExecutive>();
        }

        private void OnDisable()
        {
            BeginWire();
            confirmations.Clear();
            layout = null;
            awaitingSpeech = heardSpeech = voiceFault = false;
        }

        internal void BeginWire()
        {
            announced.Clear();
            dropped += pending.Count;
            pending.Clear();
        }

        internal void Announce(Milestone milestone, string terminal = null)
        {
            if (!isActiveAndEnabled || !executive || executive.ExamMode) return;
            if (!announced.Add(milestone)) { dropped++; return; }
            switch (milestone)
            {
                case Milestone.Carry:
                    Queue(pending, "I'm carrying the cable to " + Friendly(terminal) + ".");
                    break;
                case Milestone.Pointer:
                    Queue(pending, "I'll use the player Wire Tool for the low socket.");
                    break;
                case Milestone.Flight:
                    Queue(pending, "That terminal is above my standing reach. I'll approach it by flight.");
                    break;
                case Milestone.Align:
                    Queue(pending, "I'm aligning the connector with " + Friendly(terminal) + ".");
                    break;
                case Milestone.Verified:
                    dropped += pending.Count;
                    pending.Clear();
                    Queue(confirmations, "The circuit confirmed the connection to " + Friendly(terminal) + ".");
                    break;
                case Milestone.Stopped:
                    dropped += pending.Count;
                    pending.Clear();
                    Queue(confirmations, "Wiring stopped. Check Spark Live for the reason.");
                    break;
                case Milestone.Device:
                    if (!string.IsNullOrWhiteSpace(terminal)) Queue(confirmations, terminal);
                    break;
            }
        }

        private void Queue(Queue<string> target, string text)
        {
            if (target.Count >= maxQueuedStages) { target.Dequeue(); dropped++; }
            target.Enqueue(text);
        }

        private void Update()
        {
            if (!executive) return;
            var manager = executive.ActiveLevelManager;
            Transform nextLayout = manager && manager.LayoutLoader && manager.LayoutLoader.ActiveLayout
                ? manager.LayoutLoader.ActiveLayout.transform : null;
            if (layout != nextLayout || executive.ExamMode)
            {
                layout = nextLayout;
                BeginWire();
                confirmations.Clear();
                if (executive.ExamMode) return;
            }

            if (awaitingSpeech)
            {
                if (executive.PlaybackActive) { heardSpeech = true; return; }
                if (!heardSpeech && executive.IsProcessing &&
                    Time.unscaledTime - requestedAt < Mathf.Max(12f, speechStartupGraceSeconds)) return;
                // A player interruption can also finish a request before it plays.
                // Resume subsequent narration as soon as an actual clip is heard.
                if (!heardSpeech && executive.IsProcessing) voiceFault = true;
                awaitingSpeech = false;
                nextStage = Time.unscaledTime + stageCooldownSeconds;
            }
            if (executive.PlaybackActive) { voiceFault = false; return; }
            if (!publishLiveText || PendingStages == 0 || executive.IsProcessing || Time.unscaledTime < nextStage) return;
            lastStage = confirmations.Count > 0 ? confirmations.Dequeue() : pending.Dequeue();
            published++;
            awaitingSpeech = executive.PublishScheduledWireNarration(lastStage, enableSpeechWhenAvailable && !voiceFault);
            heardSpeech = false;
            requestedAt = Time.unscaledTime;
            nextStage = requestedAt + stageCooldownSeconds;
        }

        private static string Friendly(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "the next terminal";
            string label = id.Trim();
            label = Regex.Replace(label, @"^R[_-]LTOP[_-]", "the upper left resistor ", RegexOptions.IgnoreCase);
            label = Regex.Replace(label, @"^R[_-]RTOP[_-]", "the upper right resistor ", RegexOptions.IgnoreCase);
            label = Regex.Replace(label, @"^R[_-]LBOTTOM[_-]", "the lower left resistor ", RegexOptions.IgnoreCase);
            label = Regex.Replace(label, @"^R[_-]RBOTTOM[_-]", "the lower right resistor ", RegexOptions.IgnoreCase);
            label = Regex.Replace(label, "SwitchOutput", "the switch output", RegexOptions.IgnoreCase);
            label = Regex.Replace(label, "Register_", "resistor ", RegexOptions.IgnoreCase);
            label = Regex.Replace(label, "Battery_negative", "battery negative", RegexOptions.IgnoreCase);
            label = Regex.Replace(label, "LCD_Terminal", "LCD terminal", RegexOptions.IgnoreCase);
            return Regex.Replace(label.Replace('_', ' ').Replace('-', ' '), @"\s+", " ").Trim();
        }
    }
}

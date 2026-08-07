using System;
using UnityEngine;

namespace ProjectSpark.Hologram
{
    public enum HologramState
    {
        Hidden,
        Scanning,
        Revealing,
        Active,
        Updating
    }

    public sealed class HologramController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private HologramScanner scanner;

        [SerializeField]
        private HologramVisual visual;

        [Header("Timing")]
        [SerializeField, Min(0.01f)]
        private float revealDuration = 0.45f;

        public HologramState State { get; private set; }

        public HologramTarget CurrentTarget { get; private set; }

        public event Action<HologramState> StateChanged;

        private float revealTime;

        private void Awake()
        {
            SetState(HologramState.Hidden);
        }

        private void OnEnable()
        {
            if (scanner != null)
                scanner.TargetFound += HandleTargetFound;
        }

        private void OnDisable()
        {
            if (scanner != null)
                scanner.TargetFound -= HandleTargetFound;
        }

        private void Update()
        {
            if (State != HologramState.Revealing)
                return;

            revealTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(revealTime / revealDuration);

            visual?.SetReveal(progress);

            if (progress >= 1f)
                SetState(HologramState.Active);
        }

        public void Scan(HologramTarget target)
        {
            if (target == null)
                return;

            CurrentTarget = target;
            revealTime = 0f;

            visual?.Show(target);

            SetState(HologramState.Revealing);
        }

        public void Clear()
        {
            CurrentTarget = null;

            visual?.Hide();

            SetState(HologramState.Hidden);
        }

        public void ShowMessage(HologramMessage message)
        {
            visual?.ShowMessage(message);
        }

        private void HandleTargetFound(HologramTarget target)
        {
            Scan(target);
        }

        private void SetState(HologramState state)
        {
            if (State == state)
                return;

            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
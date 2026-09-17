using AAAUI.VFX;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Tools
{
    [DisallowMultipleComponent]
    public sealed class SparkWireTool : SparkTool
    {
        [Header("References")]
        [SerializeField]
        private SignalWireBuilder wireBuilder;

        [Header("Wire")]
        [SerializeField]
        private WirePolarity defaultPolarity =
            WirePolarity.Positive;

        [Header("Behaviour")]
        [SerializeField]
        private bool requireTerminalAtStart = true;

        protected override SparkToolType GetToolType()
        {
            return SparkToolType.Wire;
        }

        protected override void Awake()
        {
            base.Awake();

            if (wireBuilder == null)
            {
                wireBuilder =
                    GetComponentInParent<SignalWireBuilder>();
            }

            if (wireBuilder != null)
            {
                wireBuilder.enabled =
                    true;
            }
            if (wireBuilder != null)
            {
                wireBuilder.SetExternalInputControl(true);
            }
        }

        public override bool CanBegin(
            in SparkToolContext context,
            out string reason)
        {
            if (!base.CanBegin(
                    context,
                    out reason))
            {
                return false;
            }

            if (wireBuilder == null)
            {
                reason =
                    "SignalWireBuilder is not configured.";

                return false;
            }

            if (wireBuilder.IsDrawing)
            {
                reason =
                    "Wire construction is already active.";

                return false;
            }

            if (!context.HasTarget)
            {
                reason =
                    "No wire terminal was hit.";

                return false;
            }

            SparkTerminal terminal =
                GetTerminal(
                    context.TargetHit.Collider);

            if (terminal == null)
            {
                reason =
                    "Hit object does not contain a SparkTerminal.";

                return false;
            }

            if (!terminal.CanAccept(
                    SparkConnectionKind.Wire,
                    out reason))
            {
                return false;
            }

            reason = null;
            return true;
        }

        protected override SparkResult OnBegin(
            SparkToolContext context)
        {
            if (wireBuilder == null)
            {
                return SparkResult.Invalid(
                    "SignalWireBuilder is not configured.");
            }

            SparkTerminal terminal =
                GetTerminal(
                    context.TargetHit.Collider);

            if (terminal == null)
            {
                if (requireTerminalAtStart)
                {
                    return SparkResult.Invalid(
                        "Wire must start from a SparkTerminal.");
                }

                return SparkResult.Invalid(
                    "Wire start terminal is missing.");
            }

            WirePolarity polarity =
                DeterminePolarity(terminal);

            bool started =
                wireBuilder.BeginWire(
                    terminal,
                    polarity);

            if (!started)
            {
                return SparkResult.Invalid(
                    "SignalWireBuilder rejected the wire start.");
            }

            return SparkResult.Success(
                "Wire construction started.");
        }

       protected override SparkResult OnUpdate(
    SparkToolContext context)
{
    if (wireBuilder == null)
    {
        return SparkResult.Invalid(
            "SignalWireBuilder is not configured.");
    }

    if (!wireBuilder.IsDrawing)
    {
        return SparkResult.Unavailable(
            "Wire construction is not active.");
    }

    wireBuilder.UpdateWireFromScreenPosition(
        context.ScreenPosition);

    return SparkResult.Success();
}

        protected override SparkResult OnEnd(
            SparkToolContext context)
        {
            if (wireBuilder == null)
            {
                return SparkResult.Invalid(
                    "SignalWireBuilder is not configured.");
            }

            if (!wireBuilder.IsDrawing)
            {
                return SparkResult.Unavailable(
                    "Wire construction is not active.");
            }

            wireBuilder.EndWire(
                context.ScreenPosition);

            return SparkResult.Success(
                "Wire construction completed.");
        }

        protected override SparkResult OnCancel(
            SparkToolContext context)
        {
            if (wireBuilder == null)
            {
                return SparkResult.Cancelled(
                    "SignalWireBuilder is not configured.");
            }

            if (wireBuilder.IsDrawing)
            {
                wireBuilder.CancelWire();
            }

            return SparkResult.Cancelled(
                "Wire construction cancelled.");
        }

        private SparkTerminal GetTerminal(
            Collider collider)
        {
            if (collider == null)
                return null;

            return collider.GetComponentInParent<SparkTerminal>();
        }

        private WirePolarity DeterminePolarity(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return defaultPolarity;

            SparkElectricalComponent component =
                terminal.GetComponentInParent<
                    SparkElectricalComponent>();

            if (component is SparkPowerSupply supply)
            {
                SparkTerminal[] terminals =
                    component.GetComponentsInChildren<
                        SparkTerminal>(
                            true);

                if (terminals.Length > 0 &&
                    terminals[0] == terminal)
                {
                    return WirePolarity.Positive;
                }

                if (terminals.Length > 1 &&
                    terminals[1] == terminal)
                {
                    return WirePolarity.Negative;
                }
            }

            if (terminal.Polarity ==
                SparkTerminalPolarity.Positive)
            {
                return WirePolarity.Positive;
            }

            if (terminal.Polarity ==
                SparkTerminalPolarity.Negative)
            {
                return WirePolarity.Negative;
            }

            return defaultPolarity;
        }

        private Vector3 GetWireWorldPosition(
            SparkToolContext context)
        {
            if (!context.HasCamera)
            {
                return wireBuilder.CurrentEnd;
            }

            if (wireBuilder.StartTerminal == null)
            {
                return wireBuilder.CurrentEnd;
            }

            Plane plane =
                new Plane(
                    context.Camera.transform.forward,
                    wireBuilder.StartTerminal.transform.position);

            if (plane.Raycast(
                    context.PointerRay,
                    out float distance))
            {
                return context.PointerRay.GetPoint(
                    distance);
            }

            return wireBuilder.CurrentEnd;
        }

        private void OnDisable()
        {
            if (wireBuilder != null &&
                wireBuilder.IsDrawing)
            {
                wireBuilder.CancelWire();
            }
        }
    }
}
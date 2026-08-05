using UnityEngine;
using AAAUI.VFX;

namespace AAAUI
{
    [DisallowMultipleComponent]
    public sealed class CircuitTerminal : MonoBehaviour
    {
        [SerializeField]
        private WirePolarity polarity;

        [SerializeField]
        private SignalWireBuilder wireBuilder;

        public WirePolarity Polarity =>
            polarity;

        public void BeginWire()
        {
            Debug.Log(
                $"[TERMINAL] START: {name} | Polarity: {polarity}"
            );

            if (wireBuilder == null)
            {
                Debug.LogError(
                    $"[TERMINAL] {name}: WireBuilder is NULL"
                );
                return;
            }

            wireBuilder.BeginWire(
                transform.position,
                polarity,
                this
            );
        }

        public void ReceiveConnection(
            CircuitTerminal other)
        {
            if (other == null)
                return;

            Debug.Log(
                $"{name} connected to {other.name}"
            );

            Level1CircuitChecker checker =
                FindFirstObjectByType<Level1CircuitChecker>();

            if (checker != null)
            {
                checker.RegisterConnection(
                    this,
                    other
                );
            }
        }

        private void OnMouseDown()
        {
            BeginWire();
        }
    }
}
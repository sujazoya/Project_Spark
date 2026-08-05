using AAAUI.VFX;
using System.Collections.Generic;
using UnityEngine;

namespace AAAUI
{
    [DisallowMultipleComponent]
    public sealed class CircuitWireSystem : MonoBehaviour
    {
        [Header("Wire")]
        [SerializeField]
        private SignalPathMesh wirePrefab;

        [SerializeField]
        private Transform wireRoot;

        [Header("Materials")]
        [SerializeField]
        private Material positiveWireMaterial;

        [SerializeField]
        private Material negativeWireMaterial;

        private readonly List<GameObject> wires =
            new List<GameObject>();

        private SignalPathMesh currentWire;

        public void BeginWire(
            Vector3 startPosition,
            WirePolarity polarity)
        {
            if (wirePrefab == null)
                return;

            SignalPathMesh wire =
                Instantiate(
                    wirePrefab,
                    wireRoot
                );

            wire.transform.position =
                Vector3.zero;

            wire.SetMaterial(
                polarity == WirePolarity.Positive
                    ? positiveWireMaterial
                    : negativeWireMaterial
            );

            /*wire.BeginPath(
                startPosition
            );*/

            currentWire =
                wire;

            wires.Add(
                wire.gameObject
            );
        }

        public void ClearWires()
        {
            for (int i = wires.Count - 1;
                 i >= 0;
                 i--)
            {
                if (wires[i] != null)
                {
                    Destroy(
                        wires[i]
                    );
                }
            }

            wires.Clear();

            currentWire = null;
        }
    }
}
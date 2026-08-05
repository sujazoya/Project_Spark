// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/MagneticSnapSolver.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public static class MagneticSnapSolver
    {
        public static SnapPoint FindNearest(
            Vector3 worldPosition,
            float radius,
            LayerMask mask)
        {
            Collider[] hits =
                Physics.OverlapSphere(
                    worldPosition,
                    radius,
                    mask);

            SnapPoint best = null;

            float bestDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                SnapPoint snap =
                    hit.GetComponent<SnapPoint>();

                if (snap == null)
                    continue;

                float distance =
                    Vector3.Distance(
                        worldPosition,
                        snap.Position);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = snap;
                }
            }

            return best;
        }
    }
}
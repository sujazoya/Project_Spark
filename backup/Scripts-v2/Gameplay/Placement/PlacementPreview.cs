using UnityEngine;

namespace ProjectSpark.Gameplay.Placement
{
    public sealed class PlacementPreview
    {
        public void UpdateTransform(
            PlacementSession session)
        {
            if (session.Preview == null)
                return;

            session.Preview.transform.SetPositionAndRotation(
                session.Position,
                session.Rotation);
        }
    }
}

// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/WireDragHandler.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [RequireComponent(typeof(WireController))]
    public sealed class WireDragHandler : MonoBehaviour
    {
        WireController wire;

        void Awake()
        {
            wire = GetComponent<WireController>();
        }

        public void BeginDrag()
        {
            wire.Disconnect();
            wire.SetDragging(true);
        }

        public void UpdateDrag(
            Vector3 position)
        {
            wire.EndTransform.position = position;
        }

        public void EndDrag(
            SnapPoint snap)
        {
            wire.SetDragging(false);

            if (snap == null)
                return;

            if (!ConnectionRules.CanConnect(
                wire.StartConnector,
                snap.Connector))
                return;

            wire.Connect(snap);
        }
    }
}
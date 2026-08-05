// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/WireInteractionManager.cs

using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireInteractionManager : MonoBehaviour
    {
        [SerializeField]
        UnityEngine.Camera gameplayCamera;

        [SerializeField]
        LayerMask connectorMask;

        [SerializeField]
        LayerMask snapMask;

        [SerializeField]
        float snapRadius = .03f;

        WireDragHandler dragging;

        Plane plane;

        void Awake()
        {
            if (gameplayCamera == null)
                gameplayCamera = UnityEngine.Camera.main;

            plane =
                new Plane(
                    Vector3.up,
                    Vector3.zero);
        }

        void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                BeginDrag();

            if (dragging != null)
                UpdateDrag();

            if (Mouse.current.leftButton.wasReleasedThisFrame)
                EndDrag();
        }

        void BeginDrag()
        {
            Ray ray =
                gameplayCamera.ScreenPointToRay(
                    Mouse.current.position.ReadValue());

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    100f,
                    connectorMask))
                return;

            dragging =
                hit.collider.GetComponentInParent<WireDragHandler>();

            if (dragging != null)
                dragging.BeginDrag();
        }

        void UpdateDrag()
        {
            Ray ray =
                gameplayCamera.ScreenPointToRay(
                    Mouse.current.position.ReadValue());

            if (!plane.Raycast(ray, out float enter))
                return;

            dragging.UpdateDrag(
                ray.GetPoint(enter));
        }

        void EndDrag()
        {
            SnapPoint snap =
                MagneticSnapSolver.FindNearest(
                    dragging.transform.position,
                    snapRadius,
                    snapMask);

            dragging.EndDrag(snap);

            dragging = null;
        }
    }
}
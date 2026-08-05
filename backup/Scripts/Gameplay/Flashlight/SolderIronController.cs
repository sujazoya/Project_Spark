// ============================================================================
// SolderIronController.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class SolderIronController : MonoBehaviour
    {
        [SerializeField]
        float radius = .008f;

        void Update()
        {
            if (!UnityEngine.Input.GetMouseButton(0))
                return;

            Ray ray =
                UnityEngine.Camera.main.ScreenPointToRay(
                    UnityEngine.Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit))
                return;

            SolderJoint joint =
                hit.collider.GetComponent<SolderJoint>();

            if (joint != null)
                joint.Heat();
        }
    }
}
// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Level01/BoardInteraction.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Level01
{
    public sealed class BoardInteraction : MonoBehaviour
    {
        [SerializeField]
        LayerMask interactMask;

        UnityEngine.Camera cam;

        void Awake()
        {
            cam = UnityEngine.Camera.main;
        }

        void Update()
        {
            if (cam == null)
                return;

            Ray ray =
                cam.ScreenPointToRay(
                    UnityEngine.Input.mousePosition);

            if (Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    100f,
                    interactMask))
            {
                Debug.DrawLine(
                    ray.origin,
                    hit.point,
                    Color.green);
            }
        }
    }
}
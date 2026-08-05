// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Level01/CameraController.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Level01
{
    public sealed class CameraController : MonoBehaviour
    {
        [SerializeField]
        Transform target;

        [SerializeField]
        Vector3 offset =
            new(0f, 1.7f, -2.6f);

        [SerializeField]
        float smooth = 6f;

        void LateUpdate()
        {
            Vector3 desired =
                target.position + offset;

            transform.position =
                Vector3.Lerp(
                    transform.position,
                    desired,
                    Time.deltaTime * smooth);

            transform.LookAt(target);
        }
    }
}
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DefaultExecutionOrder(9001)]
    public sealed class SparkLevel87MountFollower : MonoBehaviour
    {
        [SerializeField] private Transform carrier;
        [SerializeField] private Vector3 socketPosition;
        [SerializeField] private Quaternion socketRotation;

        public void Configure(Transform movingEquipment)
        {
            carrier=movingEquipment;
            socketPosition=carrier.InverseTransformPoint(transform.position);
            socketRotation=Quaternion.Inverse(carrier.rotation)*transform.rotation;
        }

        private void LateUpdate()
        {
            if(carrier)transform.SetPositionAndRotation(carrier.TransformPoint(socketPosition),carrier.rotation*socketRotation);
        }
    }
}

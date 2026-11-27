using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    [DisallowMultipleComponent, RequireComponent(typeof(SparkTerminal))]
    public sealed class SparkNativeConnectorContact : MonoBehaviour
    {
        [SerializeField] private SparkNativeConnectorProfile profile;
        [Tooltip("Its position must match the native terminal. Its forward points inward, along the measured insertion axis.")]
        [SerializeField] private Transform contactFrame;

        internal bool TryResolve(out SparkNativeConnectorProfile motion, out Vector3 axis)
        {
            motion = profile;
            axis = default;
            if (!isActiveAndEnabled || !profile || !contactFrame ||
                !contactFrame.gameObject.activeInHierarchy || !contactFrame.IsChildOf(transform) ||
                Vector3.Distance(contactFrame.position, transform.position) > profile.ContactTolerance)
                return false;
            axis = contactFrame.forward;
            return true;
        }
    }
}

using UnityEngine;

namespace ProjectSpark.Presentation.Electronics
{
    public sealed class WireView
        : MonoBehaviour
    {
        [SerializeField]
        LineRenderer line;

        public void SetPoints(
            Vector3 a,
            Vector3 b)
        {
            line.positionCount=2;

            line.SetPosition(0,a);

            line.SetPosition(1,b);
        }
    }
}

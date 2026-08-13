using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerBeamTest : MonoBehaviour
    {
        [SerializeField] private ScannerBeamController beam;

        private void Update()
        {
            if (beam == null)
                return;

            if (Input.GetKeyDown(KeyCode.Space))
                beam.Activate();

            if (Input.GetKeyUp(KeyCode.Space))
                beam.Deactivate();
        }
    }
}
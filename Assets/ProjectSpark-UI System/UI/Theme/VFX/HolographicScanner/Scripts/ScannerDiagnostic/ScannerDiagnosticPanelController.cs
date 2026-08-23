using UnityEngine;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerDiagnosticPanelController
        : MonoBehaviour
    {
        [SerializeField]
        private ScannerDiagnosticPanel panel;

        private ScannerComponentTarget currentTarget;

        public ScannerComponentTarget CurrentTarget =>
            currentTarget;

        public void ShowComponent(
            ScannerComponentTarget target,
            ScannerDiagnosticData data)
        {
            if (target == null ||
                panel == null)
            {
                return;
            }

            currentTarget =
                target;

            panel.Show(
                data,
                target.transform);
                transform.localScale=new Vector3(0.01f,0.01f,0.01f);
        }

        public void Hide()
        {
            currentTarget =
                null;

            if (panel != null)
                panel.Hide();
        }
    }
}
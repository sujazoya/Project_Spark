using UnityEngine;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerDiagnosticPanelController
        : MonoBehaviour
    {
        [SerializeField]
        private ScannerDiagnosticPanel panel;

        [SerializeField]
        private Vector3 visibleScale = Vector3.one;

        private ScannerComponentTarget currentTarget;

        public ScannerComponentTarget CurrentTarget
        {
            get
            {
                return currentTarget;
            }
        }

        public bool IsVisible
        {
            get
            {
                return panel != null &&
                       panel.IsVisible;
            }
        }
        void OnEnable()
        {
            if (panel != null)
            panel.transform.localScale =
                visibleScale;
        }

        public void ShowComponent(
            ScannerComponentTarget target,
            ScannerDiagnosticData data)
        {
            if (target == null)
            {
                return;
            }

            if (panel == null)
            {
                Debug.LogError(
                    "ScannerDiagnosticPanelController: " +
                    "ScannerDiagnosticPanel reference is missing.",
                    this);

                return;
            }

            currentTarget = target;

            transform.localScale =
                visibleScale;

           panel.Show(
            data,
            target.transform,
            target.DiagnosticPanelOffset);
        }

        public void Hide()
        {
            currentTarget = null;

            if (panel != null)
            {
                panel.Hide();
            }
        }

        public void ResetPanel()
        {
            currentTarget = null;

            if (panel != null)
            {
                panel.HideImmediate();
            }

            transform.localScale =
                visibleScale;
        }
    }
}
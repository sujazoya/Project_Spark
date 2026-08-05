using ProjectSpark.UI.Core;
using UnityEngine;

namespace ProjectSpark.UI.Gameplay
{
    public sealed class GameplayHUD :
        MonoBehaviour
    {
        [Header("Root")]

        [SerializeField]
        private GameObject hudRoot;

        [Header("Always Available")]

        [SerializeField]
        private GameObject missionHeader;

        [SerializeField]
        private GameObject objectiveTracker;

        [SerializeField]
        private GameObject repairMonitor;

        [Header("Contextual")]

        [SerializeField]
        private GameObject hintPanel;

        [SerializeField]
        private GameObject interactionPrompt;

        [SerializeField]
        private GameObject toolBar;

        [SerializeField]
        private GameObject componentInspector;

        [SerializeField]
        private GameObject diagnosticsPanel;

        [SerializeField]
        private GameObject measurementPanel;

        public GameplayHUDContext CurrentContext
        {
            get;
            private set;
        }

        private void Awake()
        {
            SetContext(
                GameplayHUDContext.Normal);
        }

        public void Show()
        {
            if (hudRoot != null)
            {
                hudRoot.SetActive(true);
            }
        }

        public void Hide()
        {
            if (hudRoot != null)
            {
                hudRoot.SetActive(false);
            }
        }

        public void SetContext(
            GameplayHUDContext context)
        {
            CurrentContext = context;

            ApplyContext(context);
        }

        private void ApplyContext(
            GameplayHUDContext context)
        {
            SetVisible(
                missionHeader,
                true);

            SetVisible(
                objectiveTracker,
                true);

            SetVisible(
                repairMonitor,
                true);

            SetVisible(
                hintPanel,
                false);

            SetVisible(
                interactionPrompt,
                false);

            SetVisible(
                toolBar,
                false);

            SetVisible(
                componentInspector,
                false);

            SetVisible(
                diagnosticsPanel,
                false);

            SetVisible(
                measurementPanel,
                false);

            switch (context)
            {
                case GameplayHUDContext.Normal:
                    ApplyNormalContext();
                    break;

                case GameplayHUDContext.Inspection:
                    ApplyInspectionContext();
                    break;

                case GameplayHUDContext.Diagnostics:
                    ApplyDiagnosticsContext();
                    break;

                case GameplayHUDContext.Repair:
                    ApplyRepairContext();
                    break;
            }
        }

        private void ApplyNormalContext()
        {
            SetVisible(
                toolBar,
                true);
        }

        private void ApplyInspectionContext()
        {
            SetVisible(
                componentInspector,
                true);

            SetVisible(
                interactionPrompt,
                true);
        }

        private void ApplyDiagnosticsContext()
        {
            SetVisible(
                componentInspector,
                true);

            SetVisible(
                diagnosticsPanel,
                true);

            SetVisible(
                measurementPanel,
                true);
        }

        private void ApplyRepairContext()
        {
            SetVisible(
                toolBar,
                true);

            SetVisible(
                componentInspector,
                true);

            SetVisible(
                interactionPrompt,
                true);
        }

        private void SetVisible(
            GameObject target,
            bool visible)
        {
            if (target != null)
            {
                target.SetActive(visible);
            }
        }
    }
}
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
using ProjectSpark.UI.Core;

UIManager.Instance.OpenModal(
    new UIModalRequest
    {
        Type = UIModalType.Confirmation,
        Title = "EXIT PROJECT SPARK?",
        Message = "Your current progress may be lost.",
        PrimaryText = "EXIT",
        SecondaryText = "CANCEL",
        ShowSecondaryButton = true,

        PrimaryAction = ExitGame,
        SecondaryAction = null
    });
*/

namespace ProjectSpark.UI.Core
{
    public enum UIModalType
    {
        Information,
        Confirmation,
        Warning,
        Error
    }

    [Serializable]
    public sealed class UIModalRequest
    {
        public UIModalType Type;

        public string Title;

        [TextArea(2, 6)]
        public string Message;

        public string PrimaryText = "OK";

        public string SecondaryText = "CANCEL";

        public Action PrimaryAction;

        public Action SecondaryAction;

        public bool ShowSecondaryButton = true;
    }

    /// <summary>
    /// Central reusable modal controller.
    /// </summary>
    public sealed class ModalManager : MonoBehaviour
    {
        [Header("Root")]

        [SerializeField]
        private GameObject modalRoot;

        [Header("Content")]

        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text messageText;

        [Header("Buttons")]

        [SerializeField]
        private Button primaryButton;

        [SerializeField]
        private TMP_Text primaryButtonText;

        [SerializeField]
        private Button secondaryButton;

        [SerializeField]
        private TMP_Text secondaryButtonText;

        private Action primaryAction;
        private Action secondaryAction;

        public bool IsOpen { get; private set; }

        public void Initialize()
        {
            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }

            if (primaryButton != null)
            {
                primaryButton.onClick.RemoveAllListeners();
                primaryButton.onClick.AddListener(
                    HandlePrimaryPressed);
            }

            if (secondaryButton != null)
            {
                secondaryButton.onClick.RemoveAllListeners();
                secondaryButton.onClick.AddListener(
                    HandleSecondaryPressed);
            }
        }

        public void Show(UIModalRequest request)
        {
            if (request == null)
            {
                Debug.LogError(
                    "Cannot show a null modal request.",
                    this);

                return;
            }

            if (titleText != null)
            {
                titleText.text = request.Title;
            }

            if (messageText != null)
            {
                messageText.text = request.Message;
            }

            if (primaryButtonText != null)
            {
                primaryButtonText.text =
                    string.IsNullOrWhiteSpace(
                        request.PrimaryText)
                        ? "OK"
                        : request.PrimaryText;
            }

            if (secondaryButtonText != null)
            {
                secondaryButtonText.text =
                    string.IsNullOrWhiteSpace(
                        request.SecondaryText)
                        ? "CANCEL"
                        : request.SecondaryText;
            }

            primaryAction = request.PrimaryAction;
            secondaryAction = request.SecondaryAction;

            if (secondaryButton != null)
            {
                secondaryButton.gameObject.SetActive(
                    request.ShowSecondaryButton);
            }

            IsOpen = true;

            if (modalRoot != null)
            {
                modalRoot.SetActive(true);
            }
        }

        public void Close()
        {
            IsOpen = false;

            primaryAction = null;
            secondaryAction = null;

            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
        }

        private void HandlePrimaryPressed()
        {
            Action action = primaryAction;

            Close();

            action?.Invoke();
        }

        private void HandleSecondaryPressed()
        {
            Action action = secondaryAction;

            Close();

            action?.Invoke();
        }
    }
}
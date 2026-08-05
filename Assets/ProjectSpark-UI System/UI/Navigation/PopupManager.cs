using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.Navigation
{
    /// <summary>
    /// Manages popup UI independently from full-screen navigation.
    /// </summary>
    public sealed class PopupManager :
        MonoBehaviour
    {
        private readonly Dictionary<
            string,
            PopupBase> popups =
            new();

        private readonly Stack<
            PopupBase> openStack =
            new();

        public bool HasOpenPopup =>
            openStack.Count > 0;

        public PopupBase CurrentPopup =>
            openStack.Count > 0
                ? openStack.Peek()
                : null;

        public void Register(
            PopupBase popup)
        {
            if (popup == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(
                popup.PopupId))
            {
                Debug.LogWarning(
                    "Cannot register popup " +
                    "without an ID.",
                    popup);

                return;
            }

            if (popups.ContainsKey(
                popup.PopupId))
            {
                Debug.LogWarning(
                    $"Duplicate popup ID: " +
                    $"{popup.PopupId}",
                    popup);

                return;
            }

            popups.Add(
                popup.PopupId,
                popup);

            popup.Initialize();
        }

        public bool Open(
            string popupId)
        {
            if (!popups.TryGetValue(
                popupId,
                out PopupBase popup))
            {
                Debug.LogWarning(
                    $"Popup '{popupId}' " +
                    "was not registered.",
                    this);

                return false;
            }

            if (openStack.Contains(
                popup))
            {
                return false;
            }

            popup.Open();

            openStack.Push(
                popup);

            return true;
        }

        public bool CloseCurrent()
        {
            if (!HasOpenPopup)
            {
                return false;
            }

            PopupBase popup =
                openStack.Pop();

            if (popup != null)
            {
                popup.Close();
            }

            return true;
        }

        public void CloseAll()
        {
            while (openStack.Count > 0)
            {
                PopupBase popup =
                    openStack.Pop();

                if (popup != null)
                {
                    popup.Close();
                }
            }
        }

        public bool TryGet(
            string popupId,
            out PopupBase popup)
        {
            return popups.TryGetValue(
                popupId,
                out popup);
        }
    }
}
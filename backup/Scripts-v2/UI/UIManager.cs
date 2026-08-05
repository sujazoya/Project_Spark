using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI
{
    public sealed class UIManager
        : MonoBehaviour
    {
        [SerializeField]
        private List<UIScreen> screens;

        private readonly Dictionary<string, UIScreen>
            lookup = new();

        private void Awake()
        {
            foreach (var screen in screens)
            {
                lookup[screen.name] = screen;
            }
        }

        private void OnEnable()
        {
            UIEvents.OpenScreen += Open;
            UIEvents.CloseScreen += Close;
        }

        private void OnDisable()
        {
            UIEvents.OpenScreen -= Open;
            UIEvents.CloseScreen -= Close;
        }

        public void Open(string id)
        {
            if (lookup.TryGetValue(id, out var screen))
                screen.Open();
        }

        public void Close(string id)
        {
            if (lookup.TryGetValue(id, out var screen))
                screen.Close();
        }
    }
}

using System;

namespace ProjectSpark.UI
{
    public static class UIEvents
    {
        public static event Action<string>
            OpenScreen;

        public static event Action<string>
            CloseScreen;

        public static void RaiseOpen(
            string screen)
        {
            OpenScreen?.Invoke(screen);
        }

        public static void RaiseClose(
            string screen)
        {
            CloseScreen?.Invoke(screen);
        }
    }
}

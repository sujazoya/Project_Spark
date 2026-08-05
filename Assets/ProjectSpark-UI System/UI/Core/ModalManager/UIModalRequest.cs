using System;

namespace ProjectSpark.UI.Feedback
{
    public sealed class UIModalRequest
    {
        public UIModalType Type;

        public string Title;

        public string Message;

        public string PrimaryText;

        public string SecondaryText;

        public bool ShowSecondaryButton;

        public Action PrimaryAction;

        public Action SecondaryAction;

        public Action Closed;
    }
}
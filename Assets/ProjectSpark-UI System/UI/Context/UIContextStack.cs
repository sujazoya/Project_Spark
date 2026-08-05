using System.Collections.Generic;

namespace ProjectSpark.UI.Context
{
    public sealed class UIContextStack
    {
        private readonly Stack<UIContext> contexts =
            new();

        public UIContext Current =>
            contexts.Count > 0
                ? contexts.Peek()
                : UIContext.None;

        public void Push(
            UIContext context)
        {
            if (context == UIContext.None)
            {
                return;
            }

            contexts.Push(context);
        }

        public UIContext Pop()
        {
            if (contexts.Count == 0)
            {
                return UIContext.None;
            }

            return contexts.Pop();
        }

        public bool Contains(
            UIContext context)
        {
            return contexts.Contains(context);
        }

        public void Clear()
        {
            contexts.Clear();
        }
    }
}
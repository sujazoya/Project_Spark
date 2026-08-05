using System.Collections.Generic;

namespace ProjectSpark.UI.Input
{
    public sealed class UIInputContextStack
    {
        private readonly Stack<
            UIInputContext>
            contexts =
            new Stack<UIInputContext>();

        public UIInputContext Current
        {
            get
            {
                if (contexts.Count == 0)
                {
                    return
                        UIInputContext.Gameplay;
                }

                return contexts.Peek();
            }
        }

        public void Push(
            UIInputContext context)
        {
            contexts.Push(context);
        }

        public UIInputContext Pop()
        {
            if (contexts.Count == 0)
            {
                return
                    UIInputContext.Gameplay;
            }

            return contexts.Pop();
        }

        public void Clear()
        {
            contexts.Clear();
        }
    }
}
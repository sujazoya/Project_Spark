using System.Collections.Generic;

namespace ProjectSpark.UI
{
    public sealed class UIStack
    {
        private readonly Stack<UIScreen>
            stack = new();

        public void Push(UIScreen screen)
        {
            stack.Push(screen);

            screen.Open();
        }

        public void Pop()
        {
            if (stack.Count == 0)
                return;

            stack.Pop().Close();
        }
    }
}

using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Input
{
    public sealed class InputRouter
    {
        private readonly List<IInputReceiver> _receivers = new();

        public void Register(IInputReceiver receiver)
        {
            if (!_receivers.Contains(receiver))
                _receivers.Add(receiver);
        }

        public void Unregister(IInputReceiver receiver)
        {
            _receivers.Remove(receiver);
        }

        public void Dispatch(PointerState pointer)
        {
            foreach (var receiver in _receivers)
                receiver.ReceiveInput(pointer);
        }
    }
}

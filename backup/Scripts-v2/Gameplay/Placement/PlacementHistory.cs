using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Placement
{
    public sealed class PlacementHistory
    {
        private readonly Stack<GameObject> _history =
            new();

        public void Record(GameObject obj)
        {
            _history.Push(obj);
        }

        public GameObject Undo()
        {
            if (_history.Count == 0)
                return null;

            return _history.Pop();
        }
    }
}

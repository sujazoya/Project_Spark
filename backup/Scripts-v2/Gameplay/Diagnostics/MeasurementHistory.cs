using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class MeasurementHistory
    {
        private readonly List<MeasurementResult>
            history = new();

        public IReadOnlyList<MeasurementResult>
            Results => history;

        public void Add(
            MeasurementResult result)
        {
            history.Add(result);

            if (history.Count > 100)
                history.RemoveAt(0);
        }

        public void Clear()
        {
            history.Clear();
        }
    }
}

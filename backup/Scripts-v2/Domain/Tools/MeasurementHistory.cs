using ProjectSpark.Gameplay.Diagnostics;
using System.Collections.Generic;

namespace ProjectSpark.Domain.Tools
{
    public sealed class MeasurementHistory
    {
        private readonly List<MeasurementResult> history = new();

        public IReadOnlyList<MeasurementResult> Results => history;

        public void Record(MeasurementResult result)
        {
            history.Add(result);
        }

        public void Clear()
        {
            history.Clear();
        }
    }
}
using System.Collections.Generic;

namespace ProjectSpark.Core.Performance
{
    public interface IScheduledUpdate
    {
        void ScheduledUpdate();
    }

    public sealed class UpdateScheduler
    {
        private readonly List<
            IScheduledUpdate>
            updates =
                new();

        public void Register(
            IScheduledUpdate update)
        {
            updates.Add(update);
        }

        public void Tick()
        {
            foreach(var update
                in updates)
            {
                update.ScheduledUpdate();
            }
        }
    }
}

using ProjectSpark.Core.Managers;

namespace ProjectSpark.Core.Bootstrap
{
    /// <summary>
    /// Holds all global runtime objects.
    /// </summary>
    public sealed class ProjectContext
    {
        public ManagerRegistry Managers { get; }

        public GameLifetime Lifetime { get; set; }

        public ProjectContext()
        {
            Managers = new ManagerRegistry();

            Lifetime = GameLifetime.None;
        }
    }
}

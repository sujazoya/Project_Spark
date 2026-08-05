using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Workshop
{
    public sealed class SkillTree
    {
        private readonly HashSet<string>
            skills = new();

        public void Learn(
            string skillId)
        {
            skills.Add(skillId);
        }

        public bool HasSkill(
            string skillId)
        {
            return skills.Contains(skillId);
        }
    }
}

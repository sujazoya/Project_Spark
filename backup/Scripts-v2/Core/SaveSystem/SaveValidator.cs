namespace ProjectSpark.Core.SaveSystem
{
    public sealed class SaveValidator
    {
        public bool Validate(
            SaveGame save)
        {
            if(save == null)
                return false;

            if(save.Version <= 0)
                return false;

            return true;
        }
    }
}

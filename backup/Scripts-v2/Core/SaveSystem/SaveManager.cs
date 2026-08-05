using UnityEngine;

namespace ProjectSpark.Core.SaveSystem
{
    public sealed class SaveManager
        : MonoBehaviour
    {
        private readonly SerializationService
            serializer =
                new();

        private readonly SaveValidator
            validator =
                new();

        public bool Save(
            SaveGame save)
        {
            if(!validator.Validate(save))
                return false;

            string json =
                serializer.Serialize(save);

            PlayerPrefs.SetString(
                "ProjectSpark_Save",
                json);

            PlayerPrefs.Save();

            SaveEvents.RaiseSaved();

            return true;
        }

        public SaveGame Load()
        {
            if(!PlayerPrefs.HasKey(
                "ProjectSpark_Save"))
                return null;

            SaveGame save =
                serializer.Deserialize(
                    PlayerPrefs.GetString(
                        "ProjectSpark_Save"));

            if(!validator.Validate(save))
                return null;

            SaveEvents.RaiseLoaded();

            return save;
        }
    }
}

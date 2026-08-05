using Newtonsoft.Json;

namespace ProjectSpark.Core.SaveSystem
{
    public sealed class SerializationService
    {
        public string Serialize(SaveGame save)
        {
            return JsonConvert.SerializeObject(save, Formatting.Indented);
        }

        public SaveGame Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<SaveGame>(json);
        }
    }
}
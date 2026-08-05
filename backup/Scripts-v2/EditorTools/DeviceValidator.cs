#if UNITY_EDITOR

using ProjectSpark.Gameplay.Devices;

namespace ProjectSpark.EditorTools
{
    public sealed class DeviceValidator
    {
        public bool Validate(
            DeviceDefinition definition)
        {
            if (definition == null)
                return false;

            if (string.IsNullOrWhiteSpace(
                definition.DeviceId))
                return false;

            return definition.Prefab != null;
        }
    }
}
#endif
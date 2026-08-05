#if UNITY_EDITOR

using UnityEditor;

namespace ProjectSpark.EditorTools
{
    public sealed class DeviceExporter
    {
        public void Export()
        {
            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();
        }
    }
}
#endif
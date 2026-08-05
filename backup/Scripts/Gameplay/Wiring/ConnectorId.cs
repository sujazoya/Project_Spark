// ============================================================================
// ConnectorId.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [DisallowMultipleComponent]
    public sealed class ConnectorId : MonoBehaviour
    {
        [SerializeField]
        private string id;

        public string Id => id;

#if UNITY_EDITOR
        [ContextMenu("Generate Id")]
        void Generate()
        {
            id = System.Guid.NewGuid().ToString();
        }
#endif
    }
}
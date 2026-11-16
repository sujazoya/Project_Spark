using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Stable, human-readable ID attached directly to each completed wire.
    /// IDs are assigned by SparkWireRegistry at runtime; this component does
    /// not own, replace, or duplicate the circuit's connection ID.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkWireIdentity : MonoBehaviour
    {
        [SerializeField] private string wireId;
        public string WireId => wireId;

        internal void Assign(string id)
        {
            wireId = id;
        }
    }
}

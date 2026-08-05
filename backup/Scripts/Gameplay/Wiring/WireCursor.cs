// ============================================================================
// WireCursor.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireCursor : MonoBehaviour
    {
        [SerializeField]
        Texture2D normal;

        [SerializeField]
        Texture2D grab;

        [SerializeField]
        Texture2D connect;

        public void Normal()
        {
            Cursor.SetCursor(
                normal,
                Vector2.zero,
                CursorMode.Auto);
        }

        public void Grab()
        {
            Cursor.SetCursor(
                grab,
                Vector2.zero,
                CursorMode.Auto);
        }

        public void Connect()
        {
            Cursor.SetCursor(
                connect,
                Vector2.zero,
                CursorMode.Auto);
        }
    }
}
using UnityEngine;

namespace ProjectSpark.Hologram
{
    public enum HologramMessageType
    {
        Info,
        Success,
        Warning,
        Error,
        Scan,
        Connection,
        Signal
    }

    [System.Serializable]
    public struct HologramMessage
    {
        public HologramMessageType type;

        [TextArea(1, 4)]
        public string text;

        public float duration;

        public HologramMessage(
            HologramMessageType type,
            string text,
            float duration = 2f)
        {
            this.type = type;
            this.text = text;
            this.duration = duration;
        }
    }
}
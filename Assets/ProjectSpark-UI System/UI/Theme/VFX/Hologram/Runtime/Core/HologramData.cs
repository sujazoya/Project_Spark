using System;
using UnityEngine;

namespace ProjectSpark.Hologram
{
    [Serializable]
    public sealed class HologramData
    {
        [Header("Identity")]
        public string title;
        public string type;
        public string id;

        [Header("Information")]
        [TextArea(2, 8)]
        public string description;

        [Header("Status")]
        public string status;
        public string value;

        [Header("Electrical")]
        public string polarity;
        public string voltage;
        public string current;
        public string signal;

        [Header("Messages")]
        [TextArea(1, 4)]
        public string message;

        public static HologramData Create(
            string title,
            string type,
            string description)
        {
            return new HologramData
            {
                title = title,
                type = type,
                description = description,
                status = "UNKNOWN"
            };
        }
    }
}
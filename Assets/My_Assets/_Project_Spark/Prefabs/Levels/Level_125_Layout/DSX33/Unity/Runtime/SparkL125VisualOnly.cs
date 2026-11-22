using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level125
{
    [DisallowMultipleComponent]
    public sealed class SparkL125VisualOnly : MonoBehaviour
    {
        [TextArea(2, 4)] public string notice =
            "VISUAL ONLY / NOT CERTIFIABLE. Artwork cannot prove RF electronics and transmitter electrical continuity or safety, or game objective completion.";
        public bool IsAuthoritative => false;
    }
}

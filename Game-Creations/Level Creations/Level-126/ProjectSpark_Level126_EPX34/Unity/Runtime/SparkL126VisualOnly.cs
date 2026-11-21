using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level126
{
    [DisallowMultipleComponent]
    public sealed class SparkL126VisualOnly : MonoBehaviour
    {
        [TextArea(2, 4)] public string notice =
            "VISUAL ONLY / NOT CERTIFIABLE. Artwork cannot prove power electronics and load electrical continuity or safety, or game objective completion.";
        public bool IsAuthoritative => false;
    }
}

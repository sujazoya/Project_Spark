using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level127
{
    [DisallowMultipleComponent]
    public sealed class SparkL127VisualOnly : MonoBehaviour
    {
        [TextArea(2, 4)] public string notice =
            "VISUAL ONLY / NOT CERTIFIABLE. Artwork cannot prove thermal-control electrical continuity or safety, or game objective completion.";
        public bool IsAuthoritative => false;
    }
}

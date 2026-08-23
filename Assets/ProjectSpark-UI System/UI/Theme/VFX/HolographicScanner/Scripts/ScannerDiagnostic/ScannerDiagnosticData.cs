using System;
using UnityEngine;

namespace ProjectSpark.Scanner
{
    [Serializable]
    public struct ScannerDiagnosticData
    {
        public string componentId;
        public string componentName;
        public string componentType;

        public string primaryValueLabel;
        public string primaryValue;

        public string secondaryValueLabel;
        public string secondaryValue;

        public string tertiaryValueLabel;
        public string tertiaryValue;

        public string quaternaryValueLabel;
        public string quaternaryValue;

        public string status;

        public bool fault;
        [Range(0f, 1f)]
        public float severity;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level125
{
    [DisallowMultipleComponent]
    public sealed class SparkL125IntegrationAnchor : MonoBehaviour
    {
        [SerializeField] private string requiredProjectService;
        public string RequiredService => requiredProjectService;
        public bool IsBound => false;
        public void Configure(string service) { requiredProjectService = service; }
    }
}

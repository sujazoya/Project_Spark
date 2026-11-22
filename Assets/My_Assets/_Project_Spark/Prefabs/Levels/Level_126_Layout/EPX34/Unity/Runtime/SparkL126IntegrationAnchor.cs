using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level126
{
    [DisallowMultipleComponent]
    public sealed class SparkL126IntegrationAnchor : MonoBehaviour
    {
        [SerializeField] private string requiredProjectService;
        public string RequiredService => requiredProjectService;
        public bool IsBound => false;
        public void Configure(string service) { requiredProjectService = service; }
    }
}

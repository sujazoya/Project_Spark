using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level126
{
    [DisallowMultipleComponent]
    public sealed class SparkL126BlockedTask : MonoBehaviour
    {
        [SerializeField] private SparkL126TaskType type;
        [SerializeField] private string taskId;
        [SerializeField] private string domain;
        [SerializeField] private string blocker = "UNASSIGNED net/pads or 44-layer via span";
        public string TaskId => taskId;
        public bool CanCertify => false;
        public void Configure(SparkL126TaskType taskType, string identifier, string taskDomain)
        {
            type = taskType;
            taskId = identifier;
            domain = taskDomain;
        }
    }
}

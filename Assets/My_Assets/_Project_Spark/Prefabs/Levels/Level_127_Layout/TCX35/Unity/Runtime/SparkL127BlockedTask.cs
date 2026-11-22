using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level127
{
    [DisallowMultipleComponent]
    public sealed class SparkL127BlockedTask : MonoBehaviour
    {
        [SerializeField] private SparkL127TaskType type;
        [SerializeField] private string taskId;
        [SerializeField] private string domain;
        [SerializeField] private string blocker = "UNASSIGNED net/pads or 46-layer via span";
        public string TaskId => taskId;
        public bool CanCertify => false;
        public void Configure(SparkL127TaskType taskType, string identifier, string taskDomain)
        {
            type = taskType;
            taskId = identifier;
            domain = taskDomain;
        }
    }
}

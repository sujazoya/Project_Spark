using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level125
{
    [DisallowMultipleComponent]
    public sealed class SparkL125BlockedTask : MonoBehaviour
    {
        [SerializeField] private SparkL125TaskType type;
        [SerializeField] private string taskId;
        [SerializeField] private string domain;
        [SerializeField] private string blocker = "UNASSIGNED net/pads or 42-layer via span";
        public string TaskId => taskId;
        public bool CanCertify => false;
        public void Configure(SparkL125TaskType taskType, string identifier, string taskDomain)
        {
            type = taskType;
            taskId = identifier;
            domain = taskDomain;
        }
    }
}

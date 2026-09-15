using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkToolController : MonoBehaviour
    {
        [SerializeField] private List<SparkTool> tools = new();
        private readonly Dictionary<SparkToolType, SparkTool> map = new();

        public SparkTool ActiveTool { get; private set; }
        public SparkToolType ActiveToolType { get; private set; } = SparkToolType.Select;
        public event Action<SparkTool, SparkTool> ActiveToolChanged;

        private void Awake()
        {
            map.Clear();
            for (int i = 0; i < tools.Count; i++)
            {
                var tool = tools[i]; if (tool == null) continue;
                if (map.ContainsKey(tool.ToolType))
                { Debug.LogError($"Duplicate Spark tool: {tool.ToolType}", this); continue; }
                map.Add(tool.ToolType, tool);
            }
            TrySetTool(ActiveToolType, out _);
        }

        public bool TrySetTool(SparkToolType type, out string reason)
        {
            if (ActiveTool != null && ActiveTool.IsBusy)
            { reason = "Active tool has an interaction session."; return false; }
            if (!map.TryGetValue(type, out var next) || next == null)
            { reason = $"Tool '{type}' is not configured."; return false; }
            var previous = ActiveTool; ActiveTool = next; ActiveToolType = type;
            if (previous != ActiveTool) ActiveToolChanged?.Invoke(previous, ActiveTool);
            reason = null; return true;
        }
    }
}

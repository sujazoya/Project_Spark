using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Scanner
{
    public enum ScannerStage
    {
        Idle,
        Acquire,
        Scan,
        Analyze,
        Result
    }

    public enum ScannerResult
    {
        Unknown,
        Pass,
        Fault
    }

    public enum ScannerFaultType
    {
        None,
        OpenCircuit,
        ShortCircuit,
        MissingConnection,
        WrongPolarity,
        OverVoltage,
        UnderVoltage,
        ComponentValueMismatch,
        DeadNode
    }

    [Serializable]
    public struct ScannerComponentData
    {
        public int id;
        public string reference;
        public string displayName;
        public string value;
        public Vector3 worldPosition;
        public bool powered;
    }

    [Serializable]
    public struct ScannerConnectionData
    {
        public int id;
        public int componentA;
        public int componentB;
        public bool electricallyClosed;
        public float voltage;
        public Vector3 worldStart;
        public Vector3 worldEnd;
    }

    [Serializable]
    public struct ScannerNodeVoltage
    {
        public int nodeId;
        public float voltage;
        public Vector3 worldPosition;
    }

    [Serializable]
    public struct ScannerFault
    {
        public ScannerFaultType type;
        public int componentId;
        public string code;
        public string title;
        public string detail;
        public Vector3 worldPosition;
        public float measuredValue;
        public float expectedValue;
    }

    public sealed class ScannerCapture
    {
        public readonly List<ScannerComponentData> Components = new();
        public readonly List<ScannerConnectionData> Connections = new();
        public readonly List<ScannerNodeVoltage> Voltages = new();

        public void Clear()
        {
            Components.Clear();
            Connections.Clear();
            Voltages.Clear();
        }

        public ScannerComponentData FindComponent(int id)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i].id == id)
                    return Components[i];
            }

            return default;
        }
    }
}

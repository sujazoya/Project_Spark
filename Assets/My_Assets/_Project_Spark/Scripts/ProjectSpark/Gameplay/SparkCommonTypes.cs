using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public enum SparkToolType
    {
        Select,
        Inspect,
        Move,
        Rotate,
        Wire,
        Measure,
        Probe,
        Scan
    }

    public enum SparkInteractionType
    {
        None,
        Select,
        Inspect,
        Move,
        Rotate,
        Connect,
        Disconnect,
        Measure,
        Probe,
        Scan,
        Activate,
        Deactivate,
        Configure,
        Toggle
    }

    public enum SparkResultCode
    {
        Success,
        Rejected,
        Invalid,
        Blocked,
        Unavailable,
        Fault,
        Cancelled
    }

    public readonly struct SparkResult
    {
        public SparkResultCode Code { get; }
        public string Message { get; }
        public bool Succeeded => Code == SparkResultCode.Success;

        public SparkResult(SparkResultCode code, string message = null)
        {
            Code = code;
            Message = message;
        }

        public static SparkResult Success(string message = null) => new(SparkResultCode.Success, message);
        public static SparkResult Rejected(string message) => new(SparkResultCode.Rejected, message);
        public static SparkResult Invalid(string message) => new(SparkResultCode.Invalid, message);
        public static SparkResult Blocked(string message) => new(SparkResultCode.Blocked, message);
        public static SparkResult Unavailable(string message) => new(SparkResultCode.Unavailable, message);
        public static SparkResult Fault(string message) => new(SparkResultCode.Fault, message);
        public static SparkResult Cancelled(string message = null) => new(SparkResultCode.Cancelled, message);
    }

    public readonly struct SparkTargetHit
    {
        public SparkElectronicObject Target { get; }
        public Collider Collider { get; }
        public Vector3 Point { get; }
        public Vector3 Normal { get; }
        public float Distance { get; }
        public bool Valid => Target != null && Collider != null;

        public SparkTargetHit(SparkElectronicObject target, Collider collider, Vector3 point, Vector3 normal, float distance)
        {
            Target = target; Collider = collider; Point = point; Normal = normal; Distance = distance;
        }
    }

    public readonly struct SparkInteractionContext
    {
        public MonoBehaviour Interactor { get; }
        public SparkElectronicObject Target { get; }
        public SparkInteractionType InteractionType { get; }
        public SparkToolType ToolType { get; }
        public Vector3 HitPosition { get; }
        public Vector3 HitNormal { get; }
        public Vector2 ScreenPosition { get; }
        public GameObject HitObject { get; }

        public SparkInteractionContext(MonoBehaviour interactor, SparkElectronicObject target,
            SparkInteractionType interactionType, SparkToolType toolType,
            Vector3 hitPosition, Vector3 hitNormal, Vector2 screenPosition, GameObject hitObject)
        {
            Interactor = interactor; Target = target; InteractionType = interactionType;
            ToolType = toolType; HitPosition = hitPosition; HitNormal = hitNormal;
            ScreenPosition = screenPosition; HitObject = hitObject;
        }
    }
}

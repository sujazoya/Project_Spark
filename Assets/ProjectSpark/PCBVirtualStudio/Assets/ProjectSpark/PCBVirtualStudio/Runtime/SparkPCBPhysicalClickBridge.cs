using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.PCBVirtualStudio
{
    /// <summary>
    /// OPTIONAL: for existing mesh-based PCBs. Add this + SparkVirtualPCBTarget + a Collider
    /// to component/pad/trace objects. Put a PhysicsRaycaster on the gameplay Camera.
    /// The instruments remain world-space UI; interaction hits the real component surface.
    /// </summary>
    public sealed class SparkPCBPhysicalClickBridge : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private SparkVirtualPCBWorkstation workstation;
        [SerializeField] private SparkVirtualPCBTarget target;

        public void Bind(SparkVirtualPCBWorkstation owner, SparkVirtualPCBTarget boardTarget)
        {
            workstation = owner;
            target = boardTarget;
            if (target != null) target.SetWorkstation(workstation);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (workstation == null) workstation = GetComponentInParent<SparkVirtualPCBWorkstation>();
            if (target == null) target = GetComponent<SparkVirtualPCBTarget>();
            if (workstation == null || target == null) return;
            RaycastResult ray = eventData.pointerCurrentRaycast;
            Vector3 contact = ray.worldPosition == Vector3.zero ? transform.position : ray.worldPosition;
            workstation.UseSelectedToolOn(target, contact);
        }
    }
}

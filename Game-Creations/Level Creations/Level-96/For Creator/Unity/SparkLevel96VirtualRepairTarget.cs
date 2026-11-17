using UnityEngine;
namespace ProjectSpark.Level96.BoardOnly
{
 [DisallowMultipleComponent]
 public sealed class SparkLevel96VirtualRepairTarget : MonoBehaviour
 {
  public string componentRef="U1";
  public string targetKind="PCB_Component";
  public bool visuallyRepaired;
  // Deliberately no objective or electrical-solver mutation here.
  public void PlayRepairVisualOnly(ParticleSystem contactEffect) {
   if(contactEffect) { contactEffect.transform.position=transform.position; contactEffect.Play(); }
   visuallyRepaired=true;
  }
 }
}

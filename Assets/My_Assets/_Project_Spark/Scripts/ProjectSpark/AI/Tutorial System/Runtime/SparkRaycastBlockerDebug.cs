
using UnityEngine;

public class SparkRaycastBlockerDebug : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float maxDistance = 1000f;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Camera cam = targetCamera != null
            ? targetCamera
            : Camera.main;

        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(
            Input.mousePosition);

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            maxDistance,
            ~0,
            QueryTriggerInteraction.Collide);

        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance));

        Debug.Log(
            "========== RAYCAST HIT ORDER ==========");

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i].collider;

            Debug.Log(
                $"{i + 1}. {col.name} " +
                $"| Layer: {LayerMask.LayerToName(col.gameObject.layer)} " +
                $"| Distance: {hits[i].distance:F3} " +
                $"| Collider: {col.GetType().Name}",
                col.gameObject);
        }
    }
}

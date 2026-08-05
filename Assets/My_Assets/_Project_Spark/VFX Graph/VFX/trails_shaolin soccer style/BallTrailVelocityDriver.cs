using UnityEngine;
using UnityEngine.VFX;

public class BallTrailVelocityDriver : MonoBehaviour
{
    public VisualEffect vfx;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Vector3 velocity = rb.linearVelocity;

        vfx.SetVector3("BallVelocity", velocity);
        vfx.SetFloat("Speed", velocity.magnitude);
    }
}

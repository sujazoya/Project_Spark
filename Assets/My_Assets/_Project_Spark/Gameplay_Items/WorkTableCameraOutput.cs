using UnityEngine;

[RequireComponent(typeof(Camera))]
public class WorkTableCameraOutput : MonoBehaviour
{
    public RenderTexture renderTexture;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();

        if (renderTexture != null)
        {
            cam.targetTexture = renderTexture;
        }
        cam.enabled = true;
    }
}
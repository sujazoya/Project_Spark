using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ProjectSpark.Gameplay
{
    [DefaultExecutionOrder(9000)]
    public sealed class SparkLevel85ArenaView : MonoBehaviour
    {
        private Transform arena;
        private Camera camera;
        private ProjectSpark.Cameras.SparkGameCameraRig sharedRig;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private bool originalOrthographic;
        private float originalSize,yaw,pitch=48,zoom=1.21f;
        private Vector2 pan;

        public void Initialize(Transform anchor)
        {
            arena=anchor;
            camera=Camera.main;
            if(camera==null)return;
            sharedRig=camera.GetComponent<ProjectSpark.Cameras.SparkGameCameraRig>();
            originalPosition=camera.transform.position;
            originalRotation=camera.transform.rotation;
            originalOrthographic=camera.orthographic;
            originalSize=camera.orthographicSize;
        }

        public void UseOverview(){yaw=0;pitch=48;zoom=1.21f;pan=Vector2.zero;}
        public void UseSocketView(){yaw=0;pitch=27;zoom=.77f;pan=new Vector2(0,-.27f);}

        public void UseDockView(){yaw=18;pitch=25;zoom=.60f;pan=new Vector2(.18f,.09f);}

        private void LateUpdate()
        {
            if (camera == null || arena == null || (sharedRig && sharedRig.OwnsCamera) ||
                AAAUI.VFX.SignalWireBuilder.AnyWireDrawing) return;
            var mouse=Mouse.current;
            bool overUI=EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject();
            if (Application.isFocused && mouse != null && !overUI &&
                (!sharedRig || !sharedRig.IsPointerOverViewControls(mouse.position.ReadValue())))
            {
                Vector2 delta=mouse.delta.ReadValue();
                zoom=Mathf.Clamp(zoom-mouse.scroll.ReadValue().y*.00058f,.22f,1.64f);
                if(mouse.rightButton.isPressed)
                {
                    yaw=Mathf.Clamp(yaw+delta.x*.24f,-165,165);
                    pitch=Mathf.Clamp(pitch-delta.y*.19f,12,77);
                }
                if(mouse.middleButton.isPressed)
                {
                    pan.x=Mathf.Clamp(pan.x-delta.x*.0017f*zoom,-1.36f,1.36f);
                    pan.y=Mathf.Clamp(pan.y-delta.y*.0017f*zoom,-.83f,.83f);
                }
            }
            Vector3 target=arena.TransformPoint(new Vector3(pan.x,.73f,pan.y));
            Vector3 offset=arena.TransformVector(Quaternion.Euler(pitch,yaw,0)*new Vector3(0,0,-3.8f));
            camera.transform.SetPositionAndRotation(target+offset,Quaternion.LookRotation(-offset,arena.up));
            camera.orthographic=true;
            camera.orthographicSize=arena.TransformVector(Vector3.up*zoom).magnitude;
        }

        private void OnDisable()
        {
            if(camera==null)return;
            camera.transform.SetPositionAndRotation(originalPosition,originalRotation);
            camera.orthographic=originalOrthographic;
            camera.orthographicSize=originalSize;
        }
    }
}

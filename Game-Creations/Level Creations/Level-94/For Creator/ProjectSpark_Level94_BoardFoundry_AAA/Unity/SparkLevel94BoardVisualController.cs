using UnityEngine;
namespace ProjectSpark.Level94
{
    /// <summary>Visual-only toggles for the board's inspection modes.
    /// No authority over gameplay, component placement, copper nets, voltage or objective success.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel94BoardVisualController : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private bool showTop = true;
        [SerializeField] private bool showBottom = true;
        [SerializeField] private bool showPlacedComponentPreview = true;
        [SerializeField, Range(.5f,1.5f)] private float brightness = 1.0f;
        private Transform FindUnderRoot(string n)
        {
            if(visualRoot==null)return null;
            foreach(var t in visualRoot.GetComponentsInChildren<Transform>(true))
                if(t.name==n)return t;
            return null;
        }
        public void SetVisualLayers(bool top,bool bottom)
        {showTop=top;showBottom=bottom;ApplyVisualState();}
        public void ShowAssembledBoardPreview(bool visible)
        {showPlacedComponentPreview=visible;ApplyVisualState();}
        [ContextMenu("Refresh Visual-Only Board Display")]
        public void ApplyVisualState()
        {
            var a=FindUnderRoot("L94_TopGreenSoldermask"); if(a!=null)a.gameObject.SetActive(showTop);
            a=FindUnderRoot("L94_TopSMDGoldPads");if(a!=null)a.gameObject.SetActive(showTop);
            a=FindUnderRoot("L94_BottomGreenSoldermask");if(a!=null)a.gameObject.SetActive(showBottom);
            a=FindUnderRoot("L94_BottomCopperPadSet");if(a!=null)a.gameObject.SetActive(showBottom);
            a=FindUnderRoot("02_Components_27");if(a!=null)a.gameObject.SetActive(showPlacedComponentPreview);
        }
        private void OnValidate(){brightness=Mathf.Clamp(brightness,.5f,1.5f);}
    }
}

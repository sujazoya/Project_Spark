using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSpark.AAAArchitecture;
using ProjectSpark.Gameplay;
namespace ProjectSpark.SparkV5.Pro
{
    // Displays only registered static mesh geometry; never instantiates gameplay components, colliders or scripts.
    [DisallowMultipleComponent]
    public sealed class SparkProHologram : MonoBehaviour
    {
        [Serializable] public sealed class TeachingModel {public string id;public GameObject model;public string description;}
        [SerializeField] Transform projectionAnchor;
        [SerializeField] TeachingModel[] library=Array.Empty<TeachingModel>();
        [SerializeField] Material overrideMaterial;
        [SerializeField,Range(.15f,2)] float displaySize=.65f;
        [SerializeField] bool rotate=true;
        [SerializeField,Range(0,90)] float rotationSpeed=18;
        public string CurrentId {get;private set;}="";
        Transform displayed;
        SparkProTeachingLibrary generatedLibrary;
        readonly List<Renderer> parts = new List<Renderer>();
        readonly List<Vector3> terminalPositions = new List<Vector3>();
        readonly List<string> terminalNames = new List<string>();
        Vector3 homeScale;
        LineRenderer terminalMarker;
        Material markerMaterial;
        MaterialPropertyBlock highlight;
        int part = -1, terminal = -1;
        bool isolated;
        float zoom = 1;
        RectTransform controls;
        TMP_Text selectedPart;
        Button terminalButton, isolateButton;

        private void Awake()
        {
            generatedLibrary = Resources.Load<SparkProTeachingLibrary>("SparkProTeachingLibrary");
        }

        private IEnumerable<TeachingModel> Models()
        {
            foreach (var model in library) yield return model;
            if (generatedLibrary && generatedLibrary.models != null)
                foreach (var model in generatedLibrary.models)
                    yield return model;
        }
        public string DescribeLibrary(){var names=new List<string>();foreach(var item in Models())if(item!=null&&item.model&&!string.IsNullOrWhiteSpace(item.id))names.Add(item.id);return names.Count==0?"No authored teaching models are registered.":string.Join(", ",names);}
        public bool Show(string id,out string description){description="";if(string.IsNullOrWhiteSpace(id))return false;TeachingModel record=null;foreach(var item in Models())if(item!=null&&item.model&&string.Equals(item.id,id.Trim(),StringComparison.OrdinalIgnoreCase)){record=item;break;}if(record==null)return false;
            Clear();var root=new GameObject("SparkTeachingProjection");displayed=root.transform;displayed.SetParent(projectionAnchor?projectionAnchor:transform,false);var meshes=record.model.GetComponentsInChildren<MeshFilter>(true);Bounds bound=default;bool any=false;
            foreach(var mesh in meshes){if(!mesh.sharedMesh)continue;var src=mesh.GetComponent<MeshRenderer>();if(!src)continue;var go=new GameObject(mesh.name);go.transform.SetParent(displayed,false);Matrix4x4 matrix=record.model.transform.worldToLocalMatrix*mesh.transform.localToWorldMatrix;go.transform.localPosition=matrix.GetColumn(3);go.transform.localRotation=matrix.rotation;go.transform.localScale=matrix.lossyScale;go.AddComponent<MeshFilter>().sharedMesh=mesh.sharedMesh;var dst=go.AddComponent<MeshRenderer>();var mats=src.sharedMaterials;if(overrideMaterial){for(int i=0;i<mats.Length;i++)mats[i]=overrideMaterial;}dst.sharedMaterials=mats;dst.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;dst.receiveShadows=false;parts.Add(dst);if(!any){bound=dst.bounds;any=true;}else bound.Encapsulate(dst.bounds);}
            if(!any){Clear();return false;}
            // Normalize visual size from the copied geometry; no inference about electrical scale.
            float size=Mathf.Max(bound.size.x,bound.size.y,bound.size.z);if(size<.00001f){Clear();return false;}Vector3 center=displayed.InverseTransformPoint(bound.center);foreach(Transform child in displayed)child.localPosition-=center;displayed.localScale*=displaySize/size;
            homeScale = displayed.localScale;
            foreach (var pin in record.model.GetComponentsInChildren<SparkTerminal>(true))
            {
                terminalPositions.Add(record.model.transform.InverseTransformPoint(pin.transform.position) - center);
                terminalNames.Add(pin.name);
            }
            CurrentId=record.id;
            RefreshControls();
            description=string.IsNullOrWhiteSpace(record.description)?"Showing the registered teaching model "+record.id+".":record.description;return true;}
        public void Clear()
        {
            if (displayed)
            {
                displayed.gameObject.SetActive(false);
                Destroy(displayed.gameObject);
            }
            displayed = null;
            terminalMarker = null;
            CurrentId = "";
            parts.Clear();
            terminalPositions.Clear();
            terminalNames.Clear();
            part = terminal = -1;
            isolated = false;
            zoom = 1;
            RefreshControls();
        }

        public void Zoom(float multiplier)
        {
            if (!displayed) return;
            zoom = Mathf.Clamp(zoom * multiplier, .6f, 2.2f);
            displayed.localScale = homeScale * zoom;
        }

        public void ResetView()
        {
            if (!displayed) return;
            displayed.localRotation = Quaternion.identity;
            zoom = 1;
            displayed.localScale = homeScale;
            part = terminal = -1;
            isolated = false;
            if (terminalMarker) terminalMarker.enabled = false;
            UpdateParts();
        }

        public void HighlightNextPart()
        {
            if (parts.Count == 0) return;
            part = (part + 1) % parts.Count;
            UpdateParts();
        }

        public void ToggleIsolation()
        {
            if (parts.Count == 0) return;
            if (part < 0) part = 0;
            isolated = !isolated;
            UpdateParts();
        }

        private void UpdateParts()
        {
            highlight ??= new MaterialPropertyBlock();
            var color = new Color(.26f, .98f, 1f);
            highlight.SetColor("_BaseColor", color);
            highlight.SetColor("_Color", color);
            highlight.SetColor("_EmissionColor", color * .6f);
            for (int i = 0; i < parts.Count; i++)
            {
                parts[i].enabled = !isolated || i == part;
                parts[i].SetPropertyBlock(i == part ? highlight : null);
            }
            RefreshControls();
        }

        public void HighlightNextTerminal()
        {
            if (!displayed || terminalPositions.Count == 0) return;
            terminal = (terminal + 1) % terminalPositions.Count;
            if (!terminalMarker)
            {
                var node = new GameObject("Teaching terminal marker", typeof(LineRenderer));
                node.transform.SetParent(displayed, false);
                terminalMarker = node.GetComponent<LineRenderer>();
                terminalMarker.useWorldSpace = false;
                terminalMarker.loop = true;
                terminalMarker.positionCount = 24;
                terminalMarker.widthMultiplier = .005f;
                terminalMarker.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                if (!markerMaterial)
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                    if (shader)
                    {
                        markerMaterial = new Material(shader) { color = Color.cyan };
                        if (markerMaterial.HasProperty("_BaseColor"))
                            markerMaterial.SetColor("_BaseColor", Color.cyan);
                    }
                }
                if (markerMaterial) terminalMarker.sharedMaterial = markerMaterial;
            }
            float radius = .028f / Mathf.Max(.00001f, Mathf.Abs(displayed.lossyScale.x));
            for (int i = 0; i < 24; i++)
            {
                float angle = i * Mathf.PI * 2 / 24;
                terminalMarker.SetPosition(i, terminalPositions[terminal] + radius * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0));
            }
            terminalMarker.enabled = true;
            RefreshControls();
        }

        internal void AttachControls(RectTransform canvas)
        {
            if (controls) return;
            controls = SparkAAAUIFactory.Rect("Teaching projection controls", canvas);
            SparkProLivePanel.Anchor(controls, .5f, 0, .5f, 0, -300, 20, 300, 102);
            SparkAAAUIFactory.Image(controls, new Color32(11, 27, 39, 247));
            SparkAAAUIFactory.Vertical(controls, 6, 3);
            selectedPart = SparkAAAUIFactory.Text("Teaching selection", controls, "", 16, SparkAAAUIFactory.Ink, 25, true);
            var row = SparkAAAUIFactory.Rect("Controls", controls);
            SparkAAAUIFactory.Size(row, -1, 37);
            SparkAAAUIFactory.Horizontal(row, 0, 4);
            SparkProLivePanel.MakeButton(row, "Rotate", "Rotate", out _, () => SetRotation(!rotate));
            SparkProLivePanel.MakeButton(row, "Zoom out", "Zoom -", out _, () => Zoom(.8f));
            SparkProLivePanel.MakeButton(row, "Zoom in", "Zoom +", out _, () => Zoom(1.25f));
            SparkProLivePanel.MakeButton(row, "Highlight", "Part", out _, HighlightNextPart);
            terminalButton = SparkProLivePanel.MakeButton(row, "Terminals", "Terminal", out _, HighlightNextTerminal);
            isolateButton = SparkProLivePanel.MakeButton(row, "Isolate", "Isolate", out _, ToggleIsolation);
            SparkProLivePanel.MakeButton(row, "Reset", "Reset", out _, ResetView);
            SparkProLivePanel.MakeButton(row, "Close", "Close", out _, Clear);
            RefreshControls();
        }

        private void RefreshControls()
        {
            if (!controls) return;
            controls.gameObject.SetActive(displayed);
            if (!displayed) return;
            selectedPart.text = terminal >= 0 ? CurrentId + "  /  " + terminalNames[terminal] :
                part >= 0 ? CurrentId + "  /  " + parts[part].name : CurrentId + "  /  Teaching model";
            terminalButton.interactable = terminalPositions.Count > 0;
            isolateButton.interactable = parts.Count > 1;
        }
        public void SetRotation(bool enabled){rotate=enabled;}
        void Update(){if(displayed&&rotate)displayed.Rotate(Vector3.up,rotationSpeed*Time.deltaTime,Space.Self);}
        void OnDisable(){Clear();}
        void OnDestroy()
        {
            if (markerMaterial) Destroy(markerMaterial);
            if (controls) Destroy(controls.gameObject);
        }
    }
}

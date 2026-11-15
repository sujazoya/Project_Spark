#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.EditorTools
{
 public static class SparkLevel83MaterialUpgradeEditor
 {
  const string Menu="Tools/Project Spark/Levels/83/Create & Apply HD Materials To Selected Art";
  const string Folder="Assets/ProjectSpark/Level83/Materials";
  static readonly Dictionary<string,Color> Palette = new Dictionary<string,Color>() {
            {"navy", new Color(0.050980f, 0.113725f, 0.184314f, 1.000000f)},
            {"steel", new Color(0.278431f, 0.396078f, 0.486275f, 1.000000f)},
            {"light", new Color(0.588235f, 0.701961f, 0.764706f, 1.000000f)},
            {"silver", new Color(0.760784f, 0.811765f, 0.839216f, 1.000000f)},
            {"dark", new Color(0.113725f, 0.188235f, 0.247059f, 1.000000f)},
            {"black", new Color(0.047059f, 0.086275f, 0.125490f, 1.000000f)},
            {"cyan", new Color(0.152941f, 0.843137f, 0.964706f, 1.000000f)},
            {"blue", new Color(0.105882f, 0.462745f, 0.835294f, 1.000000f)},
            {"ice", new Color(0.650980f, 0.909804f, 0.976471f, 1.000000f)},
            {"copper", new Color(0.866667f, 0.505882f, 0.243137f, 1.000000f)},
            {"amber", new Color(0.988235f, 0.701961f, 0.266667f, 1.000000f)},
            {"red", new Color(0.933333f, 0.282353f, 0.309804f, 1.000000f)},
            {"green", new Color(0.196078f, 0.878431f, 0.584314f, 1.000000f)},
            {"white", new Color(0.898039f, 0.941176f, 0.960784f, 1.000000f)},
            {"glass", new Color(0.333333f, 0.698039f, 0.870588f, 0.364706f)},
            {"frost", new Color(0.803922f, 0.898039f, 0.956863f, 1.000000f)},
            {"rubber", new Color(0.176471f, 0.188235f, 0.203922f, 1.000000f)},
            {"gold", new Color(0.913725f, 0.737255f, 0.337255f, 1.000000f)},
            {"violet", new Color(0.650980f, 0.360784f, 0.949020f, 1.000000f)},
            {"plasma", new Color(0.682353f, 0.352941f, 0.949020f, 0.627451f)},
            {"purpleglass", new Color(0.588235f, 0.356863f, 0.901961f, 0.450980f)},
            {"teal", new Color(0.247059f, 0.839216f, 0.792157f, 1.000000f)},
            {"ceramic", new Color(0.878431f, 0.870588f, 0.827451f, 1.000000f)},
            {"iridescent", new Color(0.400000f, 0.772549f, 0.937255f, 1.000000f)},
            {"warning", new Color(1.000000f, 0.439216f, 0.262745f, 1.000000f)},
            {"pad", new Color(0.235294f, 0.250980f, 0.301961f, 1.000000f)},
            {"padgroove", new Color(0.101961f, 0.137255f, 0.184314f, 1.000000f)},
            {"slurry", new Color(0.266667f, 0.800000f, 0.933333f, 0.615686f)},
            {"granite", new Color(0.341176f, 0.403922f, 0.443137f, 1.000000f)},
            {"uv", new Color(0.752941f, 0.470588f, 1.000000f, 0.588235f)},
            {"resist", new Color(0.941176f, 0.447059f, 0.713725f, 1.000000f)},
            {"warm", new Color(1.000000f, 0.760784f, 0.396078f, 1.000000f)},
            {"laser", new Color(0.333333f, 0.972549f, 0.956863f, 1.000000f)},
            {"circuit", new Color(0.482353f, 0.996078f, 0.819608f, 1.000000f)},
            {"titanium", new Color(0.556863f, 0.611765f, 0.690196f, 1.000000f)},
            {"solar", new Color(0.101961f, 0.247059f, 0.631373f, 1.000000f)},
            {"solarblue", new Color(0.101961f, 0.431373f, 0.760784f, 1.000000f)},
            {"solarcell", new Color(0.074510f, 0.141176f, 0.380392f, 1.000000f)},
            {"sun", new Color(0.984314f, 0.760784f, 0.309804f, 1.000000f)},
            {"space", new Color(0.050980f, 0.101961f, 0.211765f, 1.000000f)},
            {"battery", new Color(0.129412f, 0.815686f, 0.592157f, 1.000000f)},
            {"batteryblue", new Color(0.121569f, 0.568627f, 0.960784f, 1.000000f)},
            {"shadow", new Color(0.372549f, 0.325490f, 0.792157f, 1.000000f)},
            {"hologram", new Color(0.196078f, 0.780392f, 0.980392f, 0.509804f)},
            {"orbitgold", new Color(0.956863f, 0.666667f, 0.258824f, 1.000000f)},
            {"orbitviolet", new Color(0.490196f, 0.298039f, 0.835294f, 1.000000f)},
            {"metallic", new Color(0.392157f, 0.498039f, 0.607843f, 1.000000f)},
            {"powerrack", new Color(0.113725f, 0.188235f, 0.298039f, 1.000000f)},
            {"safety", new Color(0.984314f, 0.298039f, 0.250980f, 1.000000f)},
            {"emissive", new Color(0.235294f, 0.960784f, 0.945098f, 1.000000f)},
            {"palegold", new Color(0.964706f, 0.811765f, 0.494118f, 1.000000f)},
            {"cellglass", new Color(0.172549f, 0.325490f, 0.619608f, 0.784314f)},
            {"vacuum", new Color(0.247059f, 0.352941f, 0.545098f, 1.000000f)},
  };
  static void EnsureFolders(){
    if(!AssetDatabase.IsValidFolder("Assets/ProjectSpark"))AssetDatabase.CreateFolder("Assets","ProjectSpark");
    if(!AssetDatabase.IsValidFolder("Assets/ProjectSpark/Level83"))AssetDatabase.CreateFolder("Assets/ProjectSpark","Level83");
    if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/ProjectSpark/Level83","Materials");
  }
  static string MapSet(string key){
    if(key=="solar"||key=="solarblue"||key=="solarcell"||key=="cellglass") return "SolarCells";
    if(key=="gold"||key=="palegold"||key=="orbitgold"||key=="sun"||key=="amber")return "GoldContact";
    if(key=="copper") return "CopperBus";
    if(key=="battery"||key=="batteryblue"||key=="green")return "BatteryCells";
    if(key=="metallic"||key=="steel"||key=="silver"||key=="light"||key=="titanium")return "BrushedTitanium";
    if(key=="glass"||key=="frost"||key=="hologram"||key=="ice")return "OpticalGlass";
    if(key=="navy"||key=="space"||key=="shadow"||key=="powerrack")return "SpacecraftAlloy";
    if(key=="black"||key=="dark"||key=="rubber")return "BlackCeramic";
    if(key=="teal"||key=="circuit")return "ElectronicsPCB";
    if(key=="violet"||key=="purpleglass"||key=="orbitviolet")return "OrbitDisplay";
    if(key=="warm"||key=="orange"||key=="red"||key=="safety")return "SafetyCoating";
    return "InstrumentPanel";
  }
  static Material GetOrCreate(string key,Color col){
    string path=Folder+"/PS_"+key+".mat";
    Material m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m!=null)return m;
    Shader s=Shader.Find("Universal Render Pipeline/Lit");
    if(s==null)s=Shader.Find("HDRP/Lit");if(s==null)s=Shader.Find("Standard");
    if(s==null)throw new InvalidOperationException("No compatible Lit shader");
    m=new Material(s);m.name="PS_"+key;
    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",col);
    if(m.HasProperty("_Color"))m.SetColor("_Color",col);
    bool metal=key=="silver"||key=="copper"||key=="gold"||key=="titanium"||key=="metallic"||key=="steel";
    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metal?0.85f:0.12f);
    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",0.64f);
    string pre="Assets/ProjectSpark/Level83/Textures/"+MapSet(key);
    Texture2D colorMap=AssetDatabase.LoadAssetAtPath<Texture2D>(pre+"_Albedo.png");
    if(colorMap!=null){if(m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",colorMap);if(m.HasProperty("_MainTex"))m.SetTexture("_MainTex",colorMap);}
    string norm=pre+"_Normal.png";
    TextureImporter imp=AssetImporter.GetAtPath(norm) as TextureImporter;
    if(imp!=null && imp.textureType!=TextureImporterType.NormalMap){imp.textureType=TextureImporterType.NormalMap;imp.SaveAndReimport();}
    Texture2D n=AssetDatabase.LoadAssetAtPath<Texture2D>(norm);
    if(n!=null){if(m.HasProperty("_BumpMap"))m.SetTexture("_BumpMap",n);if(m.HasProperty("_NormalMap"))m.SetTexture("_NormalMap",n);m.EnableKeyword("_NORMALMAP");}
    if(col.a<0.99f){if(m.HasProperty("_Surface"))m.SetFloat("_Surface",1);if(m.HasProperty("_Mode"))m.SetFloat("_Mode",3);m.renderQueue=3000;}
    AssetDatabase.CreateAsset(m,path);return m;
  }
  [MenuItem(Menu)]public static void Apply(){
    GameObject root=Selection.activeGameObject;
    if(root==null){EditorUtility.DisplayDialog("Level 83 Materials","Select L83_HD_VISUAL_ART","OK");return;}
    EnsureFolders();var lookup=new Dictionary<string,Material>();
    foreach(var p in Palette)lookup[p.Key]=GetOrCreate(p.Key,p.Value);
    int changed=0;
    foreach(Renderer r in root.GetComponentsInChildren<Renderer>(true)){
      Material[] assigned=r.sharedMaterials;bool found=false;
      for(int i=0;i<assigned.Length;i++){
        if(assigned[i]==null)continue;
        string key=assigned[i].name;
        if(key.StartsWith("PS_",StringComparison.OrdinalIgnoreCase))key=key.Substring(3);
        int suffix=key.IndexOf(" (Instance)",StringComparison.Ordinal);
        if(suffix>=0)key=key.Substring(0,suffix);
        Material target;
        if(!lookup.TryGetValue(key.ToLowerInvariant(),out target))continue;
        assigned[i]=target;found=true;
      }
      if(found){Undo.RecordObject(r,"Level 83 Preview Material Assignment");r.sharedMaterials=assigned;changed++;}
    }
    AssetDatabase.SaveAssets();Debug.Log("[Level 83] Assigned preview materials to "+changed+" renderers. Packed roughness/metal/AO maps need shader-specific mapping.");
  }
  [MenuItem(Menu,true)]static bool ValidSelection()=>Selection.activeGameObject!=null;
 }
}
#endif

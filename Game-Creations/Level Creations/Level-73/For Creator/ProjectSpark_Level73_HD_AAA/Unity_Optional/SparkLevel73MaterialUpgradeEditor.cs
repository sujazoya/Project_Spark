#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace ProjectSpark.EditorTools
{
    /// <summary>Editable palette material preview for Level 73. Does not create PBR maps.</summary>
    public static class SparkLevel73MaterialUpgradeEditor
    {
        private const string Folder="Assets/ProjectSpark/Level73/Materials";
        private static readonly string[] Names={"navy","steel","light","silver","dark","black","cyan","blue","ice","copper","amber","red","green","white","glass","frost","rubber","gold","violet","plasma","purpleglass","teal","ceramic","iridescent","warning","pad","padgroove","slurry","granite"};
        private static readonly string[] Hex={"#0D1D2F","#47657C","#96B3C3","#C2CFD6","#1D303F","#0C1620","#27D7F6","#1B76D5","#A6E8F9","#DD813E","#FCB344","#EE484F","#32E095","#E5F0F5","#55B2DE","#CDE5F4","#2D3034","#E9BC56","#A65CF2","#AE5AF2","#965BE6","#3FD6CA","#E0DED3","#66C5EF","#FF7043","#3C404D","#1A232F","#44CCEE","#576771"};
        [MenuItem("Tools/Project Spark/Levels/73/Create & Apply HD Materials To Selected Art")]
        public static void Apply()
        {
            if(Selection.activeGameObject==null)
            {
                EditorUtility.DisplayDialog("Level 73 materials","Select L73_HD_VISUAL_ART or its parent.","OK");return;
            }
            EnsureFolder(Folder);
            Shader sh=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
            if(sh==null){ Debug.LogError("[L73] URP/HDRP/Standard shader not found.");return; }
            Material[] mats=new Material[Names.Length];
            for(int i=0;i<Names.Length;i++)
            {
                string path=Folder+"/PS_"+Names[i]+".mat";
                Material m=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(m==null){m=new Material(sh){name="PS_"+Names[i]};AssetDatabase.CreateAsset(m,path);}
                Undo.RecordObject(m,"Update Level 73 material");
                Color c;
                if(!ColorUtility.TryParseHtmlString(Hex[i],out c)) c=Color.gray;
                bool transparent=Names[i]=="glass" || Names[i]=="slurry" || Names[i]=="plasma" || Names[i]=="purpleglass";
                if(transparent) c.a=Names[i]=="glass"?.32f:.62f;
                if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",c);
                if(m.HasProperty("_Color"))m.SetColor("_Color",c);
                m.color=c;
                bool metal=Names[i]=="steel" || Names[i]=="light" || Names[i]=="silver" || Names[i]=="gold" || Names[i]=="copper";
                if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metal?.84f:.12f);
                if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",metal?.81f:.63f);
                if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",metal?.81f:.63f);
                if(transparent)
                {
                    if(m.HasProperty("_Surface"))m.SetFloat("_Surface",1f);
                    if(m.HasProperty("_Mode"))m.SetFloat("_Mode",3f);
                    m.SetOverrideTag("RenderType","Transparent");
                    m.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);
                    m.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);
                    m.SetInt("_ZWrite",0);
                    m.EnableKeyword("_ALPHABLEND_ON");
                    m.renderQueue=(int)RenderQueue.Transparent;
                }
                mats[i]=m; EditorUtility.SetDirty(m);
            }
            int changed=0;
            foreach(Renderer r in Selection.activeGameObject.GetComponentsInChildren<Renderer>(true))
            {
                Material[] old=r.sharedMaterials;Material[] next=(Material[])old.Clone();bool different=false;
                for(int j=0;j<old.Length;j++)
                {
                    if(old[j]==null)continue;
                    for(int i=0;i<Names.Length;i++)
                        if(old[j].name.IndexOf("PS_"+Names[i],StringComparison.OrdinalIgnoreCase)>=0)
                        { if(next[j]!=mats[i]){next[j]=mats[i];different=true;}break; }
                }
                if(different){Undo.RecordObject(r,"Apply Level 73 material");r.sharedMaterials=next;changed++;}
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[L73 Materials] "+Names.Length+" material palettes created/updated; "+changed+" renderers changed. Adjust transparency for your render pipeline.");
        }
        private static void EnsureFolder(string path)
        {
            string[] a=path.Split('/');string cur=a[0];
            for(int i=1;i<a.Length;i++){string next=cur+"/"+a[i];if(!AssetDatabase.IsValidFolder(next))AssetDatabase.CreateFolder(cur,a[i]);cur=next;}
        }
    }
}
#endif

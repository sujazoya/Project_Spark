using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.AAAArchitecture
{
    public static class SparkShowcaseArchitect
    {
        const string Menu="Tools/Project Spark/AAA Showcase V6/";
        const string Source="Assets/ProjectSpark/AAAArchitecture/Editor/ShowcaseArt/";
        const string Root="Assets/ProjectSpark/AAAArchitecture/Generated/Resources/SparkShowcase/";
        static readonly string[] Roles={"Shell","Header","Rail","Brief","Chamber","Tab","ActiveTab","GalleryCard","Index","Notice","Objective","Action","Selection"};
        [MenuItem(Menu+"01 Build showcase presentation")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            Directory.CreateDirectory(Root+"Skin"); AssetDatabase.Refresh();
            foreach(var role in Roles)
            {
                var source=AssetDatabase.LoadAssetAtPath<TextAsset>(Source+role+".bytes");
                if(!source)throw new InvalidOperationException("Missing V6 artwork: "+role);
            }
            foreach(var role in Roles)
            {
                var path=Root+"Skin/"+role+".png";
                if(AssetDatabase.LoadMainAssetAtPath(path) && !(AssetDatabase.LoadMainAssetAtPath(path) is Texture2D))throw new InvalidOperationException("Occupied path: "+path);
                var bytes=AssetDatabase.LoadAssetAtPath<TextAsset>(Source+role+".bytes").bytes;
                File.WriteAllBytes(path,bytes); AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=200;
                var spriteMeshSettings = new TextureImporterSettings();
                importer.ReadTextureSettings(spriteMeshSettings);
                spriteMeshSettings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(spriteMeshSettings);

                importer.spriteBorder=role=="Shell"||role=="Chamber"?Vector4.zero:new Vector4(32,32,32,32);
                importer.alphaIsTransparency=true; importer.sRGBTexture=true; importer.mipmapEnabled=false; importer.isReadable=false;
                importer.wrapMode=TextureWrapMode.Clamp; importer.filterMode=FilterMode.Bilinear;
                importer.npotScale=TextureImporterNPOTScale.None; importer.maxTextureSize=2048;
                importer.textureCompression=TextureImporterCompression.Uncompressed;
                foreach(var platform in new[]{"Android","iPhone"}) {
                    var p=importer.GetPlatformTextureSettings(platform);p.overridden=true;p.maxTextureSize=role=="Shell"||role=="Chamber"?1024:512;
                    p.format=TextureImporterFormat.ASTC_6x6;importer.SetPlatformTextureSettings(p);
                }
                importer.SaveAndReimport();
            }
            var shader=Shader.Find("ProjectSpark/Preview/Showcase Stage");
            if(!shader)throw new InvalidOperationException("V6 stage shader missing.");
            var material=AssetDatabase.LoadAssetAtPath<Material>(Root+"Stage.mat");
            if(!material){material=new Material(shader);AssetDatabase.CreateAsset(material,Root+"Stage.mat");}
            else {material.shader=shader;EditorUtility.SetDirty(material);}
            AssetDatabase.SaveAssets(); Validate();
        }
        [MenuItem(Menu+"02 Validate showcase assets")]
        public static void Validate()
        {
            int errors=0;
            foreach(var role in Roles) {
                var path=Root+"Skin/"+role+".png";var s=AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if(!s || s.rect.width<256 || s.rect.height<256){errors++;Debug.LogError("[Spark V6] Invalid asset: "+path);}
            }
            var m=AssetDatabase.LoadAssetAtPath<Material>(Root+"Stage.mat");
            if(!m||!m.shader||!m.shader.isSupported){errors++;Debug.LogError("[Spark V6] Stage material unavailable.");}
            if(errors==0)Debug.Log("[Spark V6] 13 UI assets and stage material ready. Open the real campaign in Play Mode.");
        }
    }
}

using UnityEditor;
namespace ProjectSpark.Level32.B850I.Editor {
 public sealed class SparkL32TextureImport : AssetPostprocessor {
  void OnPreprocessTexture() { if(!assetPath.StartsWith("Assets/ProjectSpark/Level32/B850ILightning/Textures/")) return; var t=(TextureImporter)assetImporter; t.maxTextureSize=2048; t.mipmapEnabled=true; t.sRGBTexture=assetPath.EndsWith("_BaseColor.png"); if(assetPath.EndsWith("_Normal.png")) t.textureType=TextureImporterType.NormalMap; if(assetPath.EndsWith("_MetallicSmoothness.png")) t.alphaSource=TextureImporterAlphaSource.FromInput; }
 }
}

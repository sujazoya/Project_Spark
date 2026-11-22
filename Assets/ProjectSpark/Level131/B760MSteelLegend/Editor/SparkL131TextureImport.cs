using UnityEditor;
namespace ProjectSpark.Level131.Editor {
 public sealed class SparkL131TextureImport : AssetPostprocessor {
  void OnPreprocessTexture() { if(!assetPath.StartsWith("Assets/ProjectSpark/Level131/B760MSteelLegend/Textures/")) return; var t=(TextureImporter)assetImporter; t.maxTextureSize=2048; t.mipmapEnabled=true; t.sRGBTexture=assetPath.EndsWith("_BaseColor.png"); if(assetPath.EndsWith("_Normal.png")) t.textureType=TextureImporterType.NormalMap; if(assetPath.EndsWith("_MetallicSmoothness.png")) t.alphaSource=TextureImporterAlphaSource.FromInput; }
 }
}

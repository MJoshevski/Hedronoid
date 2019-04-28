using UnityEditor;
using UnityEngine;

public class SpritesPostprocessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (assetPath.Contains("/Sprites/"))
        {
            var textureImporter = (TextureImporter)assetImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
        }
    }
}
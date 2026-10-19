using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 任务目录下的贴图统一按精灵导入：不做压缩、不生成 mipmap、尺寸不被缩放。
/// 美术不需要碰 Import Settings。
/// </summary>
public class TaskTexturePostprocessor : AssetPostprocessor
{
    /// <summary>规则版本号。改了规则就 +1，然后执行菜单里的“重新导入任务资源”。</summary>
    public const string ImportVersion = "artpractice-v1";

    private const string TaskRoot = "Assets/Tasks/";

    private void OnPreprocessTexture()
    {
        string path = assetPath.Replace('\\', '/');
        if (!path.StartsWith(TaskRoot, StringComparison.Ordinal)) return;

        var importer = assetImporter as TextureImporter;
        if (importer == null) return;

        SpriteAlignment alignment = SpriteAlignment.Center;
        if (path.StartsWith("Assets/Tasks/Task2_Character/", StringComparison.Ordinal))
            alignment = SpriteAlignment.BottomCenter;   // 角色脚底对齐，换帧不跳

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.textureType = TextureImporterType.Sprite;
        settings.spriteMode = (int)SpriteImportMode.Single;
        settings.spriteAlignment = (int)alignment;
        settings.spriteMeshType = SpriteMeshType.FullRect;   // 紧贴的 tight mesh 会裁掉特效外发光
        settings.spritePixelsPerUnit = 100f;
        importer.SetTextureSettings(settings);

        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;   // 不要偷偷把 130px 改成 128px
        importer.maxTextureSize = 4096;
        importer.sRGBTexture = true;

        importer.userData = ImportVersion;
    }

    [MenuItem("Tools/美术课题/重新导入任务资源", false, 12)]
    public static void ReimportAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Tasks" });
        int updated = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            if (importer.userData == ImportVersion && importer.textureType == TextureImporterType.Sprite) continue;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            updated++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"[美术课题] 已重新导入 {updated} 张任务贴图（任务目录下共 {guids.Length} 张）");
    }
}

using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 3D 任务目录下的贴图按后缀自动套导入规则：
///   _Albedo  → sRGB 开，普通贴图
///   _Normal  → Normal Map 类型，sRGB 关
///   _MaskMap → sRGB 关
/// 美术不需要进 Import Settings。
/// </summary>
public class TaskTexturePostprocessor3D : AssetPostprocessor
{
    public const string ImportVersion = "artpractice3d-v1";

    private void OnPreprocessTexture()
    {
        string path = assetPath.Replace('\\', '/');
        if (!path.StartsWith("Assets/Tasks/", StringComparison.Ordinal)) return;

        bool albedo = EndsWith(path, "_Albedo.png");
        bool normal = EndsWith(path, "_Normal.png");
        bool mask = EndsWith(path, "_MaskMap.png");
        if (!albedo && !normal && !mask) return;

        var importer = assetImporter as TextureImporter;
        if (importer == null) return;

        importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
        importer.sRGBTexture = albedo;
        importer.mipmapEnabled = true;
        importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.userData = ImportVersion;
    }

    [MenuItem("Tools/美术课题/重新导入 3D 任务贴图", false, 12)]
    public static void ReimportAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Tasks" });
        int updated = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            if (importer.userData == ImportVersion) continue;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            updated++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"[美术课题] 已重新导入 {updated} 张任务贴图（共 {guids.Length} 张）");
    }

    private static bool EndsWith(string path, string suffix)
    {
        return path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
    }
}

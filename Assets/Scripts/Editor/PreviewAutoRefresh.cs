using System;
using System.Linq;
using ArtPractice.Preview;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 往任务目录里丢 PNG 后自动重新生成预览数据，避免“图换了但预览没变”。
/// 只对 .png 的增删改做出反应：生成过程自己写出的 .anim / .asset / .prefab 不会触发，也就不会循环。
/// </summary>
public class PreviewAutoRefresh : AssetPostprocessor
{
    private static bool _pending;

    [InitializeOnLoadMethod]
    private static void EnsureCatalogOnLoad()
    {
        EditorApplication.delayCall += () =>
        {
            if (Application.isPlaying) return;
            if (AssetDatabase.LoadAssetAtPath<PreviewCatalog>(PreviewGenerator.CatalogAssetPath) == null)
                PreviewGenerator.Generate();
        };
    }

    private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] movedTo, string[] movedFrom)
    {
        if (Application.isPlaying || EditorApplication.isCompiling || _pending) return;

        bool touched =
            imported.Any(IsTaskPng) ||
            deleted.Any(IsTaskPng) ||
            movedTo.Any(IsTaskPng) ||
            movedFrom.Any(IsTaskPng);

        if (!touched) return;

        _pending = true;
        EditorApplication.delayCall += () =>
        {
            _pending = false;
            if (Application.isPlaying) return;
            PreviewGenerator.Generate();
        };
    }

    private static bool IsTaskPng(string path)
    {
        return !string.IsNullOrEmpty(path)
               && path.StartsWith("Assets/Tasks/", StringComparison.Ordinal)
               && path.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
    }
}

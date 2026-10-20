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
        // 生成物不入库，新克隆的工程第一次打开时这里负责把它建出来
        EditorApplication.delayCall += TryEnsureCatalog;
    }

    private static void TryEnsureCatalog()
    {
        if (Application.isPlaying) return;

        // 初次导入 / 编译期间不要碰 AssetDatabase，往后再排一次
        if (EditorApplication.isUpdating || EditorApplication.isCompiling)
        {
            EditorApplication.delayCall += TryEnsureCatalog;
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<PreviewCatalog>(PreviewGenerator.CatalogAssetPath) != null) return;

        try
        {
            PreviewGenerator.Generate();
        }
        catch (Exception e)
        {
            Debug.LogWarning("[美术课题] 自动生成预览数据失败，手动执行 Tools / 美术课题 / 重新生成预览数据："
                             + e.Message);
        }
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

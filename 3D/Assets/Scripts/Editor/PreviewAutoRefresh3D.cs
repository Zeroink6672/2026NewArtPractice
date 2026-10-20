using System;
using System.Linq;
using ArtPractice3D.Preview;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 任务目录里 FBX 变化后自动重建预览数据；只监听 .fbx，生成过程写出的 .asset/.controller 不会触发循环。
/// </summary>
public class PreviewAutoRefresh3D : AssetPostprocessor
{
    private static bool _pending;

    [InitializeOnLoadMethod]
    private static void EnsureDataOnLoad()
    {
        EditorApplication.delayCall += TryEnsureData;
    }

    private static void TryEnsureData()
    {
        if (Application.isPlaying) return;

        if (EditorApplication.isUpdating || EditorApplication.isCompiling)
        {
            EditorApplication.delayCall += TryEnsureData;
            return;
        }

        var existing = AssetDatabase.LoadAssetAtPath<PreviewData3D>(Task4Pipeline.DataAssetPath);
        if (existing != null && existing.schemaVersion == PreviewData3D.CurrentSchema) return;

        try
        {
            Task1Pipeline.Generate();
            Task2Pipeline.Generate();
            Task3Pipeline.Generate();
            Task4Pipeline.Generate();
        }
        catch (Exception e)
        {
            Debug.LogWarning("[美术课题] 3D 自动生成预览数据失败，手动执行 Tools / 美术课题 / 重新生成 3D 预览数据："
                             + e.Message);
        }
    }

    private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] movedTo,
        string[] movedFrom)
    {
        if (Application.isPlaying || EditorApplication.isCompiling || _pending) return;

        bool touched = imported.Any(IsTaskFbx) || deleted.Any(IsTaskFbx) ||
                       movedTo.Any(IsTaskFbx) || movedFrom.Any(IsTaskFbx);

        if (!touched) return;

        _pending = true;
        EditorApplication.delayCall += () =>
        {
            _pending = false;
            if (Application.isPlaying) return;
            Task1Pipeline.Generate();
            Task2Pipeline.Generate();
            Task3Pipeline.Generate();
            Task4Pipeline.Generate();
        };
    }

    private static bool IsTaskFbx(string path)
    {
        return !string.IsNullOrEmpty(path)
               && path.StartsWith("Assets/Tasks/", StringComparison.Ordinal)
               && path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase);
    }
}

using System.Text;
using ArtPractice3D.Preview;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>3D 课题自检：跑完四个课题的生成，再统一报告。</summary>
public static class ArtPractice3DSelfCheck
{
    [MenuItem("Tools/美术课题/自检（3D）", false, 11)]
    public static void Run()
    {
        Task1Pipeline.Generate();
        Task2Pipeline.Generate();
        Task3Pipeline.Generate();
        Task4Pipeline.Generate();

        var data = AssetDatabase.LoadAssetAtPath<PreviewData3D>(Task4Pipeline.DataAssetPath);
        if (data == null)
        {
            Debug.LogError("[美术课题] 3D 自检失败：没有生成预览数据");
            return;
        }

        foreach (var issue in data.issues)
        {
            UnityEngine.Object context = string.IsNullOrEmpty(issue.path)
                ? null
                : AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(issue.path);

            string message = "[" + issue.task + "] " + issue.message;

            if (issue.severity == IssueSeverity3D.Error) Debug.LogError(message, context);
            else if (issue.severity == IssueSeverity3D.Warning) Debug.LogWarning(message, context);
        }

        var sb = new StringBuilder();
        sb.AppendLine("========== 3D 课题自检 ==========");
        sb.AppendLine("Unity " + data.generatedBy);

        sb.AppendLine($"课题一 道具：{CountProps(data)}/{data.task1.props.Count}");
        foreach (var prop in data.task1.props)
            if (prop.present)
                sb.AppendLine($"  ✓ {prop.id,-8} {prop.triangles,5} tris   {prop.maxDimension:F2} m");

        sb.AppendLine($"课题二 材质：{CountMaterials(data)}/{data.task2.materials.Count}");

        if (data.task3.ready)
        {
            sb.AppendLine($"课题三 场景：面数 {data.task3.triangles} · 材质 {data.task3.materials} · " +
                          $"包围盒 {data.task3.size.x:F2}×{data.task3.size.y:F2}×{data.task3.size.z:F2}");
        }
        else
        {
            sb.AppendLine("课题三 场景：还没有 Diorama.fbx");
        }

        sb.AppendLine($"课题四 骨骼动画：{CountClips(data)}/{data.task4.clips.Count} 个 clip");
        foreach (var clip in data.task4.clips)
            sb.AppendLine($"  {(clip.present ? "✓" : "✗")} {clip.name,-9} " +
                          (clip.present ? clip.duration.ToString("F2") + " s" : "缺失"));

        sb.AppendLine($"错误 {data.ErrorCount} · 警告 {data.WarningCount}");
        sb.AppendLine(data.ErrorCount == 0 ? "结果：通过" : "结果：未通过，先修上面的红色错误");

        if (data.ErrorCount > 0) Debug.LogError(sb.ToString());
        else Debug.Log(sb.ToString());
    }

    [MenuItem("Tools/美术课题/打开 3D 预览场景", false, 31)]
    public static void OpenScene()
    {
        const string scenePath = "Assets/Scenes/Preview3D.unity";
        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogError("[美术课题] 找不到 " + scenePath);
            return;
        }

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(scenePath);
    }

    private static int CountProps(PreviewData3D data)
    {
        int n = 0;
        foreach (var prop in data.task1.props)
            if (prop.present) n++;
        return n;
    }

    private static int CountMaterials(PreviewData3D data)
    {
        int n = 0;
        foreach (var material in data.task2.materials)
            if (material.present) n++;
        return n;
    }

    private static int CountClips(PreviewData3D data)
    {
        int n = 0;
        foreach (var clip in data.task4.clips)
            if (clip.present) n++;
        return n;
    }
}

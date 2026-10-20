using System.Text;
using ArtPractice3D.Preview;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>3D 课题自检：跑完课题三、课题四的生成，再统一报告。</summary>
public static class ArtPractice3DSelfCheck
{
    [MenuItem("Tools/美术课题/自检（3D）", false, 11)]
    public static void Run()
    {
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

        sb.AppendLine("课题三 场景：");
        if (data.task3.ready)
        {
            sb.AppendLine($"  面数 {data.task3.triangles} · 材质 {data.task3.materials}");
            sb.AppendLine($"  包围盒 {data.task3.size.x:F2} × {data.task3.size.y:F2} × {data.task3.size.z:F2}");
        }
        else
        {
            sb.AppendLine("  还没有 Diorama.fbx");
        }

        sb.AppendLine($"课题四 骨骼动画：{CountPresent(data)}/{data.task4.clips.Count} 个 clip");
        foreach (var clip in data.task4.clips)
        {
            string state = clip.present ? clip.duration.ToString("F2") + " s" : "缺失";
            sb.AppendLine($"  {(clip.present ? "✓" : "✗")} {clip.name,-9} {state}{(clip.loop ? " · 循环" : "")}");
        }

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

    private static int CountPresent(PreviewData3D data)
    {
        int n = 0;
        foreach (var clip in data.task4.clips)
            if (clip.present) n++;
        return n;
    }
}

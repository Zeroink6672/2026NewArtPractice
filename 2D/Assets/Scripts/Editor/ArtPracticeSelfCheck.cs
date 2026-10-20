using System.Text;
using ArtPractice.Preview;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 一键自检：重新扫描 + 汇总缺件/尺寸/帧数，错误直接点到具体文件。
/// </summary>
public static class ArtPracticeSelfCheck
{
    [MenuItem("Tools/美术课题/自检", false, 11)]
    public static void Run()
    {
        PreviewGenerator.Generate();

        var catalog = AssetDatabase.LoadAssetAtPath<PreviewCatalog>(PreviewGenerator.CatalogAssetPath);
        if (catalog == null)
        {
            Debug.LogError("[美术课题] 自检失败：没能生成预览数据，先看 Console 里的解析错误");
            return;
        }

        foreach (var issue in catalog.issues)
        {
            UnityEngine.Object context = string.IsNullOrEmpty(issue.path)
                ? null
                : AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(issue.path);

            string message = $"[{issue.task}] {issue.message}";

            if (issue.severity == IssueSeverity.Error) Debug.LogError(message, context);
            else if (issue.severity == IssueSeverity.Warning) Debug.LogWarning(message, context);
        }

        var sb = new StringBuilder();
        sb.AppendLine("========== 美术课题自检 ==========");
        sb.AppendLine($"检查时间：{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}    Unity {catalog.generatedBy}");
        sb.AppendLine($"课题一 图标：{CountIcons(catalog)}/{catalog.icons.Count} 张");
        sb.AppendLine($"课题二 角色：{catalog.character.Count} 个状态 / {CountFrames(catalog)} 帧");
        sb.AppendLine($"课题三 特效：{catalog.vfx.Count} 套（其中必需 {CountRequiredVfx(catalog)} 套）");
        sb.AppendLine($"错误 {catalog.ErrorCount} · 警告 {catalog.WarningCount}");
        sb.AppendLine(catalog.ErrorCount == 0 ? "结果：通过" : "结果：未通过，先修上面的红色错误");

        if (catalog.ErrorCount > 0) Debug.LogError(sb.ToString());
        else Debug.Log(sb.ToString());
    }

    [MenuItem("Tools/美术课题/打开预览场景", false, 30)]
    public static void OpenPreviewScene()
    {
        const string scenePath = "Assets/Scenes/Preview.unity";
        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogError("[美术课题] 找不到 " + scenePath);
            return;
        }

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(scenePath);
    }

    private static int CountIcons(PreviewCatalog catalog)
    {
        int n = 0;
        foreach (var icon in catalog.icons)
            if (icon.sprite != null) n++;
        return n;
    }

    private static int CountFrames(PreviewCatalog catalog)
    {
        int n = 0;
        foreach (var entry in catalog.character)
            if (entry.frames != null) n += entry.frames.Length;
        return n;
    }

    private static int CountRequiredVfx(PreviewCatalog catalog)
    {
        int n = 0;
        foreach (var entry in catalog.vfx)
            if (entry.required) n++;
        return n;
    }
}

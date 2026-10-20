using ArtPractice3D.Preview;
using UnityEngine;

public static class Issues3D
{
    public static void Add(PreviewData3D data, string task, IssueSeverity3D severity, string path, string message)
    {
        data.issues.Add(new PreviewIssue3D
        {
            severity = severity,
            task = task,
            path = path,
            message = string.IsNullOrEmpty(message) ? "未知问题" : message
        });
    }
}

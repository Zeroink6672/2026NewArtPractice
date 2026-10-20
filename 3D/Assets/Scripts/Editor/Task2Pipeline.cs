using System;
using System.IO;
using ArtPractice3D.Preview;
using UnityEditor;
using UnityEngine;

[Serializable]
public class MaterialSpec
{
    public string id;
    public string label;
    public bool tileable;
    public string model;
}

[Serializable]
public class MaterialDeliverySpec
{
    public string scanRoot;
    public string filePattern;
    public int textureSize;
}

[Serializable]
public class Task2ContractData
{
    public string taskId;
    public string title;
    public int contractVersion;
    public string summary;
    public MaterialDeliverySpec delivery;
    public MaterialSpec[] materials;
}

/// <summary>课题二：5 套材质的导入、校验与材质生成。</summary>
public static class Task2Pipeline
{
    public const string ContractPath = "Assets/Tasks/Task2_Materials/task.json";
    private const string TaskName = "课题二";

    public static void Generate()
    {
        var data = LoadOrCreate();
        data.issues.RemoveAll(i => i.task == TaskName);
        data.task2 = new Task2Info();

        string error;
        var contract = LoadContract(out error);
        if (contract == null)
        {
            Issues3D.Add(data, TaskName, IssueSeverity3D.Error, "", error);
            Save(data);
            LogSummary(data);
            return;
        }

        string root = contract.delivery.scanRoot;

        foreach (var spec in contract.materials)
        {
            var info = new MaterialInfo
            {
                id = spec.id,
                label = spec.label,
                tileable = spec.tileable
            };

            string albedo = MaterialBuilder3D.TexturePath(root, spec.id, "_Albedo");
            if (!File.Exists(albedo))
            {
                Issues3D.Add(data, TaskName, IssueSeverity3D.Error, albedo, "缺少 " + spec.id + "_Albedo.png");
                data.task2.materials.Add(info);
                continue;
            }

            MaterialBuilder3D.CheckTexture(data, TaskName, albedo, true, false);

            foreach (string suffix in new[] { "_Normal", "_MaskMap" })
            {
                string path = MaterialBuilder3D.TexturePath(root, spec.id, suffix);
                if (File.Exists(path))
                    MaterialBuilder3D.CheckTexture(data, TaskName, path, false, suffix == "_Normal");
                else
                    Issues3D.Add(data, TaskName, IssueSeverity3D.Warning, path, "缺少 " + spec.id + suffix + ".png");
            }

            var material = MaterialBuilder3D.GetOrCreate("Task2", spec.id);
            MaterialBuilder3D.Apply(material, root, spec.id, spec.tileable);

            info.material = material;
            info.present = true;

            if (!spec.tileable && !string.IsNullOrEmpty(spec.model))
            {
                info.model = AssetDatabase.LoadAssetAtPath<GameObject>(spec.model);
                if (info.model == null)
                    Issues3D.Add(data, TaskName, IssueSeverity3D.Warning, spec.model,
                        "找不到指定模型 " + Path.GetFileName(spec.model));
            }

            data.task2.materials.Add(info);
        }

        Save(data);
        LogSummary(data);
    }

    // ------------------------------------------------------------------ 工具

    private static Task2ContractData LoadContract(out string error)
    {
        error = null;
        if (!File.Exists(ContractPath))
        {
            error = "找不到 " + ContractPath;
            return null;
        }

        try
        {
            var data = JsonUtility.FromJson<Task2ContractData>(File.ReadAllText(ContractPath));
            if (data == null || data.delivery == null || data.materials == null)
            {
                error = "task.json 结构不完整";
                return null;
            }
            return data;
        }
        catch (Exception e)
        {
            error = "解析 task.json 失败：" + e.Message;
            return null;
        }
    }

    private static PreviewData3D LoadOrCreate()
    {
        var data = AssetDatabase.LoadAssetAtPath<PreviewData3D>(Task4Pipeline.DataAssetPath);
        if (data == null)
        {
            Directory.CreateDirectory("Assets/Resources/Preview");
            data = ScriptableObject.CreateInstance<PreviewData3D>();
            AssetDatabase.CreateAsset(data, Task4Pipeline.DataAssetPath);
        }
        return data;
    }

    private static void Save(PreviewData3D data)
    {
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void LogSummary(PreviewData3D data)
    {
        var issues = data.issues.FindAll(i => i.task == TaskName);
        int errors = issues.FindAll(i => i.severity == IssueSeverity3D.Error).Count;
        int present = data.task2.materials.FindAll(m => m.present).Count;

        var sb = new System.Text.StringBuilder();
        sb.Append("[美术课题] 3D 课题二 · 材质 ").Append(present).Append('/').Append(data.task2.materials.Count);

        if (errors > 0)
        {
            var first = issues.Find(i => i.severity == IssueSeverity3D.Error);
            sb.Append(" · 错误 ").Append(errors).Append(" · 首个问题：").Append(first.message);
        }
        else
        {
            sb.Append(" · 无错误");
        }

        sb.Append(" · 警告 ").Append(issues.FindAll(i => i.severity == IssueSeverity3D.Warning).Count);
        sb.Append(" · 详情 Tools / 美术课题 / 自检（3D）");

        if (errors > 0) Debug.LogWarning(sb.ToString());
        else Debug.Log(sb.ToString());
    }
}

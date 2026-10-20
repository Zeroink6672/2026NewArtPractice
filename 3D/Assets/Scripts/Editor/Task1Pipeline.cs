using System;
using System.IO;
using ArtPractice3D.Preview;
using UnityEditor;
using UnityEngine;

[Serializable]
public class PropSpec
{
    public string id;
    public string label;
    public int maxTriangles;
    public float maxDimension;
    public float tolerance;
}

[Serializable]
public class PropDeliverySpec
{
    public string scanRoot;
    public string filePattern;
}

[Serializable]
public class Task1ContractData
{
    public string taskId;
    public string title;
    public int contractVersion;
    public string summary;
    public PropDeliverySpec delivery;
    public PropSpec[] props;
}

/// <summary>课题一：5 件道具的导入、统计与校验。</summary>
public static class Task1Pipeline
{
    public const string ContractPath = "Assets/Tasks/Task1_Props/task.json";
    private const string TaskName = "课题一";

    public static void Generate()
    {
        var data = LoadOrCreate();
        data.issues.RemoveAll(i => i.task == TaskName);
        data.task1 = new Task1Info();

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

        foreach (var spec in contract.props)
        {
            var info = new PropInfo { id = spec.id, label = spec.label };
            string fbx = root + "/" + spec.id + ".fbx";

            if (!File.Exists(fbx))
            {
                Issues3D.Add(data, TaskName, IssueSeverity3D.Error, fbx, "缺少 " + spec.id + ".fbx");
                data.task1.props.Add(info);
                continue;
            }

            ConfigureImporter(fbx);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            info.model = model;
            info.present = model != null;

            if (model != null)
            {
                info.triangles = CountTriangles(model);
                Bounds bounds = ComputeBounds(model);
                info.maxDimension = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
                info.hasUv = HasUv(model);

                CheckGeometry(data, spec, info, bounds);
                CheckTextures(data, root, spec, info);
            }

            data.task1.props.Add(info);
        }

        Save(data);
        LogSummary(data);
    }

    // ------------------------------------------------------------------ 校验

    private static void CheckGeometry(PreviewData3D data, PropSpec spec, PropInfo info, Bounds bounds)
    {
        if (info.triangles > spec.maxTriangles * 1.2f)
            Issues3D.Add(data, TaskName, IssueSeverity3D.Error, info.id,
                $"面数 {info.triangles} 超过上限 {spec.maxTriangles} 的 1.2 倍");
        else if (info.triangles > spec.maxTriangles)
            Issues3D.Add(data, TaskName, IssueSeverity3D.Warning, info.id,
                $"面数 {info.triangles} 超过上限 {spec.maxTriangles}");

        float tolerance = 1f + Mathf.Max(0.1f, spec.tolerance);
        if (info.maxDimension > spec.maxDimension * tolerance)
            Issues3D.Add(data, TaskName, IssueSeverity3D.Warning, info.id,
                $"最大边 {info.maxDimension:F2} m，超过参考尺寸 {spec.maxDimension:F2} m");

        if (!info.hasUv)
            Issues3D.Add(data, TaskName, IssueSeverity3D.Error, info.id, "模型没有 UV");

        if (bounds.min.y < -0.02f)
            Issues3D.Add(data, TaskName, IssueSeverity3D.Warning, info.id,
                $"模型最低点 {bounds.min.y:F2}，轴心可能不在底部");
    }

    private static void CheckTextures(PreviewData3D data, string root, PropSpec spec, PropInfo info)
    {
        var material = MaterialBuilder3D.GetOrCreate("Task1", spec.id);
        MaterialBuilder3D.Apply(material, root, spec.id, false);
        info.material = material;

        string albedo = MaterialBuilder3D.TexturePath(root, spec.id, "_Albedo");
        if (!File.Exists(albedo))
        {
            Issues3D.Add(data, TaskName, IssueSeverity3D.Error, albedo, "缺少 " + spec.id + "_Albedo.png");
            return;
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
    }

    // ------------------------------------------------------------------ 导入与统计

    private static void ConfigureImporter(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null) return;

        importer.animationType = ModelImporterAnimationType.None;
        importer.importAnimation = false;
        importer.importCameras = false;
        importer.importLights = false;
        importer.importBlendShapes = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
        importer.globalScale = 1f;
        importer.useFileScale = true;
        importer.isReadable = false;
        importer.SaveAndReimport();
    }

    private static int CountTriangles(GameObject model)
    {
        int total = 0;
        foreach (var filter in model.GetComponentsInChildren<MeshFilter>())
            if (filter.sharedMesh != null)
                total += filter.sharedMesh.triangles.Length / 3;
        return total;
    }

    private static Bounds ComputeBounds(GameObject model)
    {
        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);

        var bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static bool HasUv(GameObject model)
    {
        foreach (var filter in model.GetComponentsInChildren<MeshFilter>())
            if (filter.sharedMesh != null && filter.sharedMesh.uv.Length > 0)
                return true;
        return false;
    }

    // ------------------------------------------------------------------ 工具

    private static Task1ContractData LoadContract(out string error)
    {
        error = null;
        if (!File.Exists(ContractPath))
        {
            error = "找不到 " + ContractPath;
            return null;
        }

        try
        {
            var data = JsonUtility.FromJson<Task1ContractData>(File.ReadAllText(ContractPath));
            if (data == null || data.delivery == null || data.props == null)
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
        data.schemaVersion = PreviewData3D.CurrentSchema;
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void LogSummary(PreviewData3D data)
    {
        var issues = data.issues.FindAll(i => i.task == TaskName);
        int errors = issues.FindAll(i => i.severity == IssueSeverity3D.Error).Count;
        int present = data.task1.props.FindAll(p => p.present).Count;

        var sb = new System.Text.StringBuilder();
        sb.Append("[美术课题] 3D 课题一 · 道具 ").Append(present).Append('/').Append(data.task1.props.Count);

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

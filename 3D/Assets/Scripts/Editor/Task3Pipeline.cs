using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArtPractice3D.Preview;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 课题三：导入学生交的 Diorama.fbx，校验面数/包围盒/材质数，并写入预览数据。
/// </summary>
public static class Task3Pipeline
{
    private const string TaskName = "课题三";

    public static void Generate()
    {
        var data = LoadOrCreate();
        data.issues.RemoveAll(i => i.task == TaskName);
        data.task3 = new Task3Info();

        string error;
        var contract = Task3ContractLoader.Load(out error);
        if (contract == null)
        {
            Add(data, IssueSeverity3D.Error, "", error);
            Save(data);
            LogSummary(data);
            return;
        }

        if (!File.Exists(contract.baseAssets.kit))
            Add(data, IssueSeverity3D.Warning, contract.baseAssets.kit, "找不到 Kit.fbx");
        if (!File.Exists(contract.baseAssets.blockout))
            Add(data, IssueSeverity3D.Warning, contract.baseAssets.blockout, "找不到 Blockout.fbx");

        // 没交作业时，预览里显示白模，让学员至少能看到起点
        data.task3.blockout = AssetDatabase.LoadAssetAtPath<GameObject>(contract.baseAssets.blockout);

        string path = contract.delivery.scanRoot + "/" + contract.delivery.fileName;
        if (!File.Exists(path))
        {
            Add(data, IssueSeverity3D.Error, path, "缺少 " + contract.delivery.fileName);
            Save(data);
            LogSummary(data);
            return;
        }

        ConfigureImporter(path);

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (model == null)
        {
            Add(data, IssueSeverity3D.Error, path, "无法加载模型");
            Save(data);
            LogSummary(data);
            return;
        }

        int triangles = CountTriangles(model);
        Bounds bounds = ComputeBounds(model);
        int materials = CountMaterials(model);

        data.task3.model = model;
        data.task3.triangles = triangles;
        data.task3.materials = materials;
        data.task3.size = bounds.size;
        data.task3.ready = true;

        Validate(contract, triangles, bounds.size, materials, path, data);

        Save(data);
        LogSummary(data);
    }

    // ------------------------------------------------------------------ 导入

    private static void ConfigureImporter(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null) return;

        importer.animationType = ModelImporterAnimationType.None;
        importer.importAnimation = false;
        importer.importCameras = false;
        importer.importLights = false;
        importer.importBlendShapes = false;
        // 导入材质是为了拿到学生嵌在 FBX 里的贴图，预览时再转成 URP/Lit
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        importer.globalScale = 1f;
        importer.useFileScale = true;
        importer.isReadable = false;
        importer.SaveAndReimport();
    }

    // ------------------------------------------------------------------ 统计

    private static int CountTriangles(GameObject model)
    {
        int total = 0;

        foreach (var filter in model.GetComponentsInChildren<MeshFilter>())
            if (filter.sharedMesh != null)
                total += filter.sharedMesh.triangles.Length / 3;

        foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            if (renderer.sharedMesh != null)
                total += renderer.sharedMesh.triangles.Length / 3;

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

    private static int CountMaterials(GameObject model)
    {
        var unique = new HashSet<Material>();
        foreach (var renderer in model.GetComponentsInChildren<Renderer>())
            foreach (var material in renderer.sharedMaterials)
                if (material != null) unique.Add(material);
        return unique.Count;
    }

    // ------------------------------------------------------------------ 校验

    private static void Validate(Task3ContractData contract, int triangles, Vector3 size, int materials,
        string path, PreviewData3D data)
    {
        int maxTriangles = contract.delivery.maxTriangles;
        if (triangles > maxTriangles * 1.2f)
            Add(data, IssueSeverity3D.Error, path, $"面数 {triangles} 超过上限 {maxTriangles} 的 1.2 倍");
        else if (triangles > maxTriangles)
            Add(data, IssueSeverity3D.Warning, path, $"面数 {triangles} 超过上限 {maxTriangles}");

        var max = contract.delivery.maxSize;
        if (size.x > max.x || size.y > max.y || size.z > max.z)
            Add(data, IssueSeverity3D.Warning, path,
                $"包围盒 {size.x:F2}×{size.y:F2}×{size.z:F2} 超过 {max.x}×{max.y}×{max.z}");

        if (materials > contract.delivery.maxTextures * 1.5f)
            Add(data, IssueSeverity3D.Error, path, $"材质 {materials} 个，超过 {contract.delivery.maxTextures} 套的 1.5 倍");
        else if (materials > contract.delivery.maxTextures)
            Add(data, IssueSeverity3D.Warning, path, $"材质 {materials} 个，建议不超过 {contract.delivery.maxTextures} 套");
    }

    // ------------------------------------------------------------------ 工具

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

    private static void Add(PreviewData3D data, IssueSeverity3D severity, string path, string message)
    {
        data.issues.Add(new PreviewIssue3D
        {
            severity = severity,
            task = TaskName,
            path = path,
            message = string.IsNullOrEmpty(message) ? "未知问题" : message
        });
    }

    private static void LogSummary(PreviewData3D data)
    {
        var issues = data.issues.Where(i => i.task == TaskName).ToList();
        int errors = issues.Count(i => i.severity == IssueSeverity3D.Error);

        var sb = new System.Text.StringBuilder();
        sb.Append("[美术课题] 3D 课题三 · ");
        if (data.task3.ready)
        {
            sb.Append($"面数 {data.task3.triangles} · 材质 {data.task3.materials} · ");
            sb.Append($"包围盒 {data.task3.size.x:F2}×{data.task3.size.y:F2}×{data.task3.size.z:F2}");
        }
        else
        {
            sb.Append("还没有可预览的 Diorama.fbx");
        }

        if (errors > 0)
        {
            var first = issues.First(i => i.severity == IssueSeverity3D.Error);
            sb.Append(" · 错误 ").Append(errors).Append(" · 首个问题：").Append(first.message);
        }
        else
        {
            sb.Append(" · 无错误");
        }

        sb.Append(" · 警告 ").Append(issues.Count(i => i.severity == IssueSeverity3D.Warning));
        sb.Append(" · 详情 Tools / 美术课题 / 自检（3D）");

        if (errors > 0) Debug.LogWarning(sb.ToString());
        else Debug.Log(sb.ToString());
    }
}

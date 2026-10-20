using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;

/// <summary>
/// 课题三基础资产：把 CC0 套件拼成一个 Kit.fbx，并用立方体生成 Blockout.fbx。
/// 批处理：-executeMethod Task3BaseSetup.Build
/// </summary>
public static class Task3BaseSetup
{
    private const string BaseDir = "Assets/Tasks/Task3_Diorama/Base";
    private const string SourceDir = BaseDir + "/Source";
    private const string KitPath = BaseDir + "/Kit.fbx";
    private const string BlockoutPath = BaseDir + "/Blockout.fbx";

    [MenuItem("Tools/美术课题/重建课题三基础资产", false, 40)]
    public static void Build()
    {
        BuildKit();
        BuildBlockout();
        AssetDatabase.Refresh();
        AssetDatabase.DeleteAsset(SourceDir);
        AssetDatabase.Refresh();
        Debug.Log("[Task3] 基础资产已生成：" + KitPath + " / " + BlockoutPath);
    }

    private static void BuildKit()
    {
        if (!Directory.Exists(SourceDir))
        {
            Debug.LogError("[Task3] 找不到 " + SourceDir + "，先把 CC0 套件放进去");
            return;
        }

        var files = Directory.GetFiles(SourceDir, "*.fbx", SearchOption.TopDirectoryOnly)
            .OrderBy(f => f)
            .ToArray();

        var root = new GameObject("Kit");
        int index = 0;
        const int columns = 6;
        const float spacing = 3f;

        foreach (string file in files)
        {
            string path = file.Replace('\\', '/');
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.animationType = ModelImporterAnimationType.None;
                importer.importAnimation = false;
                importer.importCameras = false;
                importer.importLights = false;
                importer.globalScale = 1f;
                importer.useFileScale = true;
                importer.SaveAndReimport();
            }

            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) continue;

            var instance = Object.Instantiate(asset);
            instance.name = Path.GetFileNameWithoutExtension(file);
            instance.transform.position = new Vector3(
                (index % columns) * spacing,
                0f,
                -(index / columns) * spacing);
            instance.transform.SetParent(root.transform, true);
            index++;
        }

        var bounds = ComputeBounds(root);
        Debug.Log($"[Task3] Kit: {index} 件，bounds={bounds.size}");

        AssetDatabase.DeleteAsset(KitPath);
        ModelExporter.ExportObject(KitPath, root);
        Object.DestroyImmediate(root);
    }

    private static void BuildBlockout()
    {
        var root = new GameObject("Blockout");

        AddBox(root, "Floor", new Vector3(0f, -0.05f, 0f), new Vector3(4f, 0.1f, 4f));
        AddBox(root, "Wall_Back", new Vector3(0f, 1.5f, 1.9f), new Vector3(4f, 3f, 0.2f));
        AddBox(root, "Wall_Right", new Vector3(1.9f, 1.5f, 1.0f), new Vector3(0.2f, 3f, 2.0f));

        // 左墙留一个 0.8 × 0.6 的窗洞（z 0.2–1.0，y 1.2–1.8）
        AddBox(root, "Wall_Left_Bottom", new Vector3(-1.9f, 0.6f, 0f), new Vector3(0.2f, 1.2f, 4f));
        AddBox(root, "Wall_Left_Top", new Vector3(-1.9f, 2.4f, 0f), new Vector3(0.2f, 1.2f, 4f));
        AddBox(root, "Wall_Left_Front", new Vector3(-1.9f, 1.5f, -0.9f), new Vector3(0.2f, 0.6f, 2.2f));
        AddBox(root, "Wall_Left_Back", new Vector3(-1.9f, 1.5f, 1.5f), new Vector3(0.2f, 0.6f, 1.0f));

        // 右前方的 L 形隔断，把开口做成拐角
        AddBox(root, "Partition", new Vector3(1.45f, 1.5f, 0f), new Vector3(0.9f, 3f, 0.2f));

        // 炼金炉体，占据右后角
        AddBox(root, "Furnace", new Vector3(1.35f, 1.3f, 1.4f), new Vector3(1.0f, 2.6f, 1.0f));

        AddBox(root, "Bench", new Vector3(0f, 0.42f, 0.9f), new Vector3(1.8f, 0.85f, 0.7f));
        AddBox(root, "Shelf", new Vector3(-1.6f, 0.9f, 1.6f), new Vector3(0.5f, 1.8f, 0.4f));
        AddBox(root, "Platform", new Vector3(0.9f, 0.1f, -0.9f), new Vector3(1.4f, 0.2f, 1.4f));

        var bounds = ComputeBounds(root);
        Debug.Log($"[Task3] Blockout: {root.transform.childCount} 个方块，bounds={bounds.size}");

        AssetDatabase.DeleteAsset(BlockoutPath);
        ModelExporter.ExportObject(BlockoutPath, root);
        Object.DestroyImmediate(root);
    }

    [MenuItem("Tools/美术课题/只重建课题三白模", false, 41)]
    public static void BuildBlockoutOnly()
    {
        BuildBlockout();
        AssetDatabase.Refresh();
        Debug.Log("[Task3] 白模已重新生成：" + BlockoutPath);
    }

    private static void AddBox(GameObject parent, string name, Vector3 center, Vector3 size)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = center;
        go.transform.localScale = size;

        var collider = go.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
    }

    private static Bounds ComputeBounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);

        var bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }
}

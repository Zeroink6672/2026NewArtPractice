using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 课题四基础角色：把 Dummy.fbx 导入为 Humanoid，生成材质与预制体，并把映射结果打到 Console。
/// 批处理调用：-executeMethod Task4BaseSetup.Apply
/// </summary>
public static class Task4BaseSetup
{
    private const string BaseDir = "Assets/Tasks/Task4_Character/Base";
    private const string FbxPath = BaseDir + "/Dummy.fbx";
    private const string MatPath = BaseDir + "/Dummy.mat";
    private const string PrefabPath = BaseDir + "/BaseCharacter.prefab";

    public static void Apply()
    {
        var importer = AssetImporter.GetAtPath(FbxPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError("[Task4] 找不到 FBX：" + FbxPath);
            return;
        }

        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importAnimation = false;          // 基础角色不要动画
        importer.importBlendShapes = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.None;  // 材质由我们统一给
        importer.globalScale = 1f;
        importer.useFileScale = true;
        importer.importNormals = ModelImporterNormals.Import;
        importer.isReadable = false;
        importer.SaveAndReimport();

        var assets = AssetDatabase.LoadAllAssetsAtPath(FbxPath);
        var avatar = assets.OfType<Avatar>().FirstOrDefault();
        var mesh = assets.OfType<Mesh>().FirstOrDefault();
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);

        var sb = new StringBuilder();
        sb.AppendLine("[Task4] ===== FBX 导入结果 =====");
        sb.AppendLine("  avatar: " + (avatar == null ? "null" : avatar.name));
        if (avatar != null)
        {
            sb.AppendLine("  isHuman: " + avatar.isHuman);
            sb.AppendLine("  isValid: " + avatar.isValid);
            sb.AppendLine("  humanBoneCount: " + avatar.humanDescription.human.Length);
            sb.AppendLine("  skeletonBoneCount: " + avatar.humanDescription.skeleton.Length);
        }
        if (mesh != null)
            sb.AppendLine("  mesh bounds: " + mesh.bounds.size);
        if (model != null)
            sb.AppendLine("  renderers: " + model.GetComponentsInChildren<Renderer>().Length);
        Debug.Log(sb.ToString());

        var mat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, MatPath);
        }
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", new Color(0.78f, 0.78f, 0.80f, 1f));
        EditorUtility.SetDirty(mat);

        if (model != null)
        {
            var instance = Object.Instantiate(model);
            instance.name = "BaseCharacter";

            var animator = instance.GetComponent<Animator>();
            if (animator == null) animator = instance.AddComponent<Animator>();
            animator.avatar = avatar;
            animator.applyRootMotion = false;

            foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterial = mat;

            PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
            Object.DestroyImmediate(instance);
            Debug.Log("[Task4] 预制体已生成：" + PrefabPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>验证预制体的真实世界尺寸，确认角色是米制、站姿正确。</summary>
    public static void Verify()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[Task4] 找不到预制体：" + PrefabPath);
            return;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        var renderers = instance.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
            Debug.Log($"[Task4] renderer {renderer.name}: size={renderer.bounds.size} center={renderer.bounds.center}");

        Debug.Log($"[Task4] root scale={instance.transform.localScale} rotation={instance.transform.localEulerAngles}");
        Debug.Log($"[Task4] renderer count={renderers.Length}");
        Object.DestroyImmediate(instance);
    }
}

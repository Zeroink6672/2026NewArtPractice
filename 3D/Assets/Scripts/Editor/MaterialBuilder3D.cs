using System.IO;
using ArtPractice3D.Preview;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 从 <id>_Albedo/_Normal/_MaskMap.png 生成 URP/Lit 材质，并做贴图导入规范检查。
/// </summary>
public static class MaterialBuilder3D
{
    private const string MaterialDir = "Assets/Resources/Preview/Materials";

    public static Material GetOrCreate(string prefix, string id)
    {
        Directory.CreateDirectory(MaterialDir);
        string path = MaterialDir + "/" + prefix + "_" + id + ".mat";

        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }
        return material;
    }

    public static string TexturePath(string dir, string id, string suffix)
    {
        return dir + "/" + id + suffix + ".png";
    }

    public static Texture2D Load(string dir, string id, string suffix)
    {
        string path = TexturePath(dir, id, suffix);
        return File.Exists(path) ? AssetDatabase.LoadAssetAtPath<Texture2D>(path) : null;
    }

    public static void Apply(Material material, string dir, string id, bool tiling)
    {
        var albedo = Load(dir, id, "_Albedo");
        var normal = Load(dir, id, "_Normal");
        var mask = Load(dir, id, "_MaskMap");

        if (albedo != null) material.SetTexture("_BaseMap", albedo);

        if (normal != null)
        {
            material.SetTexture("_BumpMap", normal);
            material.EnableKeyword("_NORMALMAP");
        }
        else
        {
            material.DisableKeyword("_NORMALMAP");
        }

        if (mask != null)
        {
            material.SetTexture("_MetallicGlossMap", mask);
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.SetTexture("_OcclusionMap", mask);
            material.EnableKeyword("_OCCLUSIONMAP");
        }
        else
        {
            material.DisableKeyword("_METALLICSPECGLOSSMAP");
            material.DisableKeyword("_OCCLUSIONMAP");
        }

        material.SetTextureScale("_BaseMap", tiling ? new Vector2(2f, 2f) : Vector2.one);
        EditorUtility.SetDirty(material);
    }

    public static void CheckTexture(PreviewData3D data, string task, string path, bool isAlbedo,
        bool isNormal)
    {
        if (!File.Exists(path))
        {
            Issues3D.Add(data, task, IssueSeverity3D.Warning, path, "缺少 " + Path.GetFileName(path));
            return;
        }

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture == null)
        {
            Issues3D.Add(data, task, IssueSeverity3D.Error, path, "贴图无法读取");
            return;
        }

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            if (isNormal && importer.textureType != TextureImporterType.NormalMap)
                Issues3D.Add(data, task, IssueSeverity3D.Warning, path, "法线贴图的导入类型不是 Normal Map");

            if (isAlbedo && !importer.sRGBTexture)
                Issues3D.Add(data, task, IssueSeverity3D.Warning, path, "Albedo 没有开 sRGB");

            if (!isAlbedo && importer.sRGBTexture)
                Issues3D.Add(data, task, IssueSeverity3D.Warning, path, "非 Albedo 贴图不应开 sRGB");
        }

        if (texture.width != texture.height)
            Issues3D.Add(data, task, IssueSeverity3D.Warning, path, "贴图不是正方形");

        if (!IsPowerOfTwo(texture.width) || !IsPowerOfTwo(texture.height))
            Issues3D.Add(data, task, IssueSeverity3D.Warning, path, "贴图尺寸不是 2 的幂");

        if (texture.width < 512 || texture.width > 2048)
            Issues3D.Add(data, task, IssueSeverity3D.Warning, path,
                $"贴图尺寸 {texture.width}，建议 512–2048");
    }

    private static bool IsPowerOfTwo(int value)
    {
        return value > 0 && (value & (value - 1)) == 0;
    }
}

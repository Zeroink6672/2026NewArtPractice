using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 3D 工程一次性初始化：线性色彩空间、创建 URP 管线资产、建预览场景。
/// 批处理调用：
///   Unity.exe -batchmode -nographics -projectPath 3D -executeMethod ProjectSetup3D.Apply -quit
/// </summary>
public static class ProjectSetup3D
{
    private const string SettingsDir = "Assets/Settings";
    private const string RendererPath = SettingsDir + "/UniversalRenderer.asset";
    private const string PipelinePath = SettingsDir + "/URP-Pipeline.asset";
    private const string ScenePath = "Assets/Scenes/Preview3D.unity";

    public static void Apply()
    {
        Directory.CreateDirectory(SettingsDir);
        Directory.CreateDirectory("Assets/Scenes");

        // 3D PBR 必须在伽马/线性里选线性，否则光照是错的
        if (PlayerSettings.colorSpace != ColorSpace.Linear)
            PlayerSettings.colorSpace = ColorSpace.Linear;

        PlayerSettings.companyName = "Chuangyou Workshop";
        PlayerSettings.productName = "2026NewArtPractice3D";

        var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
        if (renderer == null)
        {
            renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer, RendererPath);
        }

        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
        if (pipeline == null)
        {
            pipeline = UniversalRenderPipelineAsset.Create(renderer);
            AssetDatabase.CreateAsset(pipeline, PipelinePath);
        }

        GraphicsSettings.defaultRenderPipeline = pipeline;
        QualitySettings.renderPipeline = pipeline;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.13f, 0.14f, 0.16f, 1f);
        cam.fieldOfView = 35f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 200f;
        camGo.transform.position = new Vector3(0f, 2.4f, -5.2f);
        camGo.transform.LookAt(new Vector3(0f, 1.0f, 0f));
        camGo.AddComponent<AudioListener>();

        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(1f, 0.96f, 0.9f, 1f);
        lightGo.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

        new GameObject("PreviewRoot");

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[3D] 初始化完成：URP 管线资产 + Preview3D 场景");
    }
}

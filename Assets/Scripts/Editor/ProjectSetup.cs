using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 一次性工程初始化：2D 模式、文本序列化、创建 Preview 场景并加入 Build Settings。
/// 批处理调用：
///   Unity.exe -batchmode -nographics -projectPath . -executeMethod ProjectSetup.Apply -quit -logFile &lt;log&gt;
/// </summary>
public static class ProjectSetup
{
    private const string ScenePath = "Assets/Scenes/Preview.unity";

    public static void Apply()
    {
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        EditorSettings.serializationMode = SerializationMode.ForceText;

        PlayerSettings.companyName = "Chuangyou Workshop";
        PlayerSettings.productName = "2026NewArtPractice";

        try
        {
            // 2D 工程用 Gamma，画面颜色和美术在 PS 里看到的一致
            if (PlayerSettings.colorSpace != ColorSpace.Gamma)
            {
                PlayerSettings.colorSpace = ColorSpace.Gamma;
                Debug.Log("[ProjectSetup] colorSpace -> Gamma");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[ProjectSetup] 设置 colorSpace 失败: " + e.Message);
        }

        Directory.CreateDirectory("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.11f, 0.11f, 0.13f, 1f);
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 100f;
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        new GameObject("PreviewRoot");

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ProjectSetup] 初始化完成: " + ScenePath);
    }
}

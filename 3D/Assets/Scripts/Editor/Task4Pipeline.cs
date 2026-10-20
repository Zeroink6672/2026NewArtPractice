using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArtPractice3D.Preview;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// 课题四：扫描 DropHere 下的 FBX，导入为 Humanoid，抽取 clip，校验并生成状态机。
/// 支持两种交付方式：
///   1) 每个动画一个文件（Idle.fbx / Run.fbx / StartRun.fbx）——推荐
///   2) 一个 Animation.fbx 里带三个同名 take——进阶
/// </summary>
public static class Task4Pipeline
{
    public const string DataAssetPath = "Assets/Resources/Preview/PreviewData3D.asset";

    private const string DataDir = "Assets/Resources/Preview";
    private const string ControllerPath = "Assets/Tasks/Task4_Character/Animation.controller";
    private const string TaskName = "课题四";

    [MenuItem("Tools/美术课题/重新生成 3D 预览数据", false, 10)]
    public static void GenerateMenu()
    {
        Generate();
    }

    /// <summary>批处理入口：-executeMethod Task4Pipeline.GenerateFromCommandLine</summary>
    public static void GenerateFromCommandLine()
    {
        Generate();
    }

    public static void Generate()
    {
        Directory.CreateDirectory(DataDir);
        AssetDatabase.Refresh();

        var data = LoadOrCreate();
        data.issues.Clear();
        data.task4 = new Task4Info();
        data.basePrefab = null;
        data.controller = null;
        data.generatedBy = Application.unityVersion;

        string error;
        var contract = Task4ContractLoader.Load(out error);
        if (contract == null)
        {
            Add(data, IssueSeverity3D.Error, "", error);
            Save(data);
            LogSummary(data);
            return;
        }

        var basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(contract.baseCharacter.prefab);
        if (basePrefab == null)
            Add(data, IssueSeverity3D.Error, contract.baseCharacter.prefab, "找不到基础角色 prefab");
        data.basePrefab = basePrefab;

        Avatar baseAvatar = null;
        if (basePrefab != null)
        {
            var animator = basePrefab.GetComponent<Animator>();
            if (animator != null) baseAvatar = animator.avatar;
        }
        if (baseAvatar == null)
            Add(data, IssueSeverity3D.Error, contract.baseCharacter.prefab, "基础角色没有配置 Avatar");

        var clips = CollectClips(contract, baseAvatar, data);
        ValidateClips(contract, clips, data);

        data.controller = BuildController(contract, clips, data);

        foreach (var spec in contract.clips)
        {
            clips.TryGetValue(spec.name, out var clip);
            data.task4.clips.Add(new ClipInfo
            {
                name = spec.name,
                loop = spec.loop,
                present = clip != null,
                duration = clip != null ? clip.length : 0f
            });
        }

        Save(data);
        LogSummary(data);
    }

    // ------------------------------------------------------------------ 扫描与导入

    private static Dictionary<string, AnimationClip> CollectClips(Task4ContractData contract, Avatar baseAvatar,
        PreviewData3D data)
    {
        var result = new Dictionary<string, AnimationClip>(StringComparer.Ordinal);
        string root = Normalize(contract.delivery.scanRoot);

        if (!Directory.Exists(root))
        {
            Add(data, IssueSeverity3D.Error, root, "交付目录不存在：" + root);
            return result;
        }

        string[] files = Directory.GetFiles(root, "*.fbx", SearchOption.TopDirectoryOnly);
        if (files.Length == 0)
        {
            Add(data, IssueSeverity3D.Error, root, "DropHere 里没有任何 .fbx");
            return result;
        }

        foreach (string file in files)
        {
            string path = Normalize(file);
            List<string> matched = ConfigureImporter(path, baseAvatar, contract, data);
            if (matched.Count == 0) continue;

            var imported = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().ToList();

            foreach (string name in matched)
            {
                var clip = imported.FirstOrDefault(c =>
                    string.Equals(c.name, name, StringComparison.OrdinalIgnoreCase));

                if (clip == null)
                {
                    Add(data, IssueSeverity3D.Error, path, $"take 改名后找不到 clip \"{name}\"");
                    continue;
                }

                if (result.ContainsKey(name))
                {
                    Add(data, IssueSeverity3D.Warning, path, $"\"{name}\" 重复出现，已保留先导入的那份");
                    continue;
                }

                result[name] = clip;
            }
        }

        return result;
    }

    private static List<string> ConfigureImporter(string path, Avatar baseAvatar, Task4ContractData contract,
        PreviewData3D data)
    {
        var matched = new List<string>();

        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null)
        {
            Add(data, IssueSeverity3D.Error, path, "读不到导入设置，确认它是 FBX");
            return matched;
        }

        importer.animationType = ModelImporterAnimationType.Human;
        if (baseAvatar != null)
        {
            // 用基础角色的 Avatar，保证重定向结果一致
            importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar = baseAvatar;
        }
        else
        {
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        }

        importer.importAnimation = true;
        importer.importCameras = false;
        importer.importLights = false;
        importer.importBlendShapes = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
        importer.globalScale = 1f;
        importer.useFileScale = true;

        var takes = importer.defaultClipAnimations;
        if (takes == null || takes.Length == 0)
        {
            Add(data, IssueSeverity3D.Error, path, "文件里没有任何动画 take");
            return matched;
        }

        string fileStem = Path.GetFileNameWithoutExtension(path);
        var kept = new List<ModelImporterClipAnimation>();
        var unmatched = new List<string>();

        foreach (var take in takes)
        {
            string canonical = MatchCanonical(take.name, contract.delivery.takeNames);
            if (canonical == null)
            {
                unmatched.Add(take.name);
                continue;
            }

            ApplyTake(take, canonical, contract);
            kept.Add(take);
            matched.Add(canonical);
        }

        // take 名对不上时，退回用文件名识别（Blender 的 Action 名经常是 Dummy RigAction 这种）
        if (kept.Count == 0)
        {
            string stemCanonical = MatchCanonical(fileStem, contract.delivery.takeNames);
            if (stemCanonical != null)
            {
                var take = takes[0];
                unmatched.Remove(take.name);
                ApplyTake(take, stemCanonical, contract);
                kept.Add(take);
                matched.Add(stemCanonical);
            }
        }

        if (kept.Count == 0)
        {
            string takeNames = unmatched.Count > 0 ? string.Join("、", unmatched.ToArray()) : "（无）";
            Add(data, IssueSeverity3D.Error, path,
                $"识别不出动画：take 叫 {takeNames}，文件名是 {fileStem}。" +
                "请把文件命名成 Idle.fbx / Run.fbx / StartRun.fbx，或把 take 改成同名。");
            return matched;
        }

        foreach (string name in unmatched)
            Add(data, IssueSeverity3D.Warning, path, $"多余的动画 take：\"{name}\"，已忽略");

        importer.clipAnimations = kept.ToArray();
        importer.SaveAndReimport();
        return matched;
    }

    // ------------------------------------------------------------------ 校验

    private static void ValidateClips(Task4ContractData contract, Dictionary<string, AnimationClip> clips,
        PreviewData3D data)
    {
        foreach (var spec in contract.clips)
        {
            if (!clips.TryGetValue(spec.name, out var clip)) continue;

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (settings.loopTime != spec.loop)
                Add(data, IssueSeverity3D.Warning, clip.name,
                    $"{spec.name} 的循环设置是 {settings.loopTime}，契约要求 {spec.loop}");

            if (clip.length < spec.minSeconds || clip.length > spec.maxSeconds)
                Add(data, IssueSeverity3D.Warning, clip.name,
                    $"{spec.name} 时长 {clip.length:F2}s，要求 {spec.minSeconds:F1}–{spec.maxSeconds:F1}s");

            if (clip.length <= 0.05f)
                Add(data, IssueSeverity3D.Error, clip.name, $"{spec.name} 几乎是空动画（{clip.length:F2}s）");
        }

        foreach (var spec in contract.clips)
            if (!clips.ContainsKey(spec.name))
                Add(data, IssueSeverity3D.Error, contract.delivery.scanRoot, "缺少 " + spec.name);
    }

    // ------------------------------------------------------------------ 状态机

    private static RuntimeAnimatorController BuildController(Task4ContractData contract,
        Dictionary<string, AnimationClip> clips, PreviewData3D data)
    {
        if (!clips.ContainsKey("Idle"))
        {
            // 没有 Idle 时清掉旧控制器，避免留下引用已删除 clip 的悬空状态机
            AssetDatabase.DeleteAsset(ControllerPath);
            Add(data, IssueSeverity3D.Error, ControllerPath, "没有 Idle，无法生成状态机");
            return null;
        }

        // 生成物，整份重建；路径固定，引用由预览数据持有
        AssetDatabase.DeleteAsset(ControllerPath);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

        var machine = controller.layers[0].stateMachine;

        var idle = machine.AddState("Idle");
        idle.motion = clips["Idle"];
        machine.defaultState = idle;

        AnimatorState run = null;
        AnimatorState startRun = null;

        if (clips.ContainsKey("Run"))
        {
            run = machine.AddState("Run");
            run.motion = clips["Run"];
        }

        // 只有 Run 存在时，过渡状态才有意义；否则会卡死在 StartRun
        if (run != null && clips.ContainsKey("StartRun"))
        {
            startRun = machine.AddState("StartRun");
            startRun.motion = clips["StartRun"];
        }

        if (run != null)
        {
            if (startRun != null)
            {
                var toStart = idle.AddTransition(startRun);
                toStart.hasExitTime = false;
                toStart.duration = 0.05f;
                toStart.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

                var toRun = startRun.AddTransition(run);
                toRun.hasExitTime = true;
                toRun.exitTime = 0.9f;
                toRun.duration = 0.10f;
            }
            else
            {
                var direct = idle.AddTransition(run);
                direct.hasExitTime = false;
                direct.duration = 0.05f;
                direct.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            }

            var backToIdle = run.AddTransition(idle);
            backToIdle.hasExitTime = false;
            backToIdle.duration = 0.10f;
            backToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    // ------------------------------------------------------------------ 工具

    private static string Normalize(string path)
    {
        return string.IsNullOrEmpty(path) ? string.Empty : path.Replace('\\', '/').TrimEnd('/');
    }

    private static string MatchCanonical(string text, string[] wanted)
    {
        if (string.IsNullOrEmpty(text) || wanted == null) return null;

        string name = text;
        int bar = name.LastIndexOf('|');
        if (bar >= 0) name = name.Substring(bar + 1);
        name = name.Trim();

        foreach (var candidate in wanted)
            if (string.Equals(name, candidate, StringComparison.OrdinalIgnoreCase))
                return candidate;

        // 长名字优先，避免 "StartRun.001" 被 "Run" 先匹配走
        foreach (var candidate in wanted.OrderByDescending(x => x.Length))
            if (name.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0)
                return candidate;

        return null;
    }

    private static ClipSpec3D FindSpec(Task4ContractData contract, string name)
    {
        foreach (var spec in contract.clips)
            if (string.Equals(spec.name, name, StringComparison.OrdinalIgnoreCase))
                return spec;
        return null;
    }

    private static void ApplyTake(ModelImporterClipAnimation take, string canonical, Task4ContractData contract)
    {
        var spec = FindSpec(contract, canonical);
        take.name = canonical;
        take.loopTime = spec != null && spec.loop;
        take.loopPose = take.loopTime;
    }

    private static PreviewData3D LoadOrCreate()
    {
        var data = AssetDatabase.LoadAssetAtPath<PreviewData3D>(DataAssetPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<PreviewData3D>();
            AssetDatabase.CreateAsset(data, DataAssetPath);
        }
        return data;
    }

    private static void Save(PreviewData3D data)
    {
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
        var sb = new System.Text.StringBuilder();
        sb.Append("[美术课题] 3D 已扫描 · 角色 ");
        sb.Append(data.task4.clips.Count(c => c.present)).Append('/').Append(data.task4.clips.Count);
        sb.Append(" 个 clip");

        int errors = data.ErrorCount;
        if (errors > 0)
        {
            var first = data.issues.First(i => i.severity == IssueSeverity3D.Error);
            sb.Append(" · 错误 ").Append(errors).Append(" · 首个问题：").Append(first.message);
        }
        else
        {
            sb.Append(" · 无错误");
        }

        sb.Append(" · 警告 ").Append(data.WarningCount);
        sb.Append(" · 详情 Tools / 美术课题 / 自检（3D）");

        if (errors > 0) Debug.LogWarning(sb.ToString());
        else Debug.Log(sb.ToString());
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArtPractice.Preview;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// 扫描 Assets/Tasks 下的交付物 → 生成 PreviewCatalog / AnimationClip / AnimatorController / Character.prefab。
/// 生成物全部走“固定路径原地覆盖”，保证 GUID 不变、场景里的引用不断。
/// </summary>
public static class PreviewGenerator
{
    public const string CatalogAssetPath = "Assets/Resources/Preview/PreviewCatalog.asset";

    private const string CatalogDir = "Assets/Resources/Preview";
    private const string CharRoot = "Assets/Tasks/Task2_Character";
    private const string ClipDir = CharRoot + "/Animations";
    private const string ControllerPath = CharRoot + "/Character.controller";
    private const string PrefabPath = CharRoot + "/Character.prefab";

    private const string Task1Name = "课题一";
    private const string Task2Name = "课题二";
    private const string Task3Name = "课题三";

    [MenuItem("Tools/美术课题/重新生成预览数据", false, 10)]
    public static void GenerateMenu()
    {
        Generate(true);
    }

    /// <summary>批处理入口：-executeMethod PreviewGenerator.GenerateFromCommandLine</summary>
    public static void GenerateFromCommandLine()
    {
        Generate(true);
    }

    public static void Generate(bool verbose)
    {
        var issues = new List<PreviewIssue>();

        Directory.CreateDirectory(CatalogDir);
        Directory.CreateDirectory(ClipDir);
        AssetDatabase.Refresh();

        var catalog = LoadOrCreateCatalog();
        catalog.icons.Clear();
        catalog.character.Clear();
        catalog.vfx.Clear();
        catalog.issues.Clear();
        catalog.characterPrefab = null;

        BuildIcons(catalog, issues);
        BuildCharacter(catalog, issues);
        BuildVfx(catalog, issues);

        catalog.issues.AddRange(issues);
        catalog.generatedBy = Application.unityVersion;

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (verbose) LogSummary(catalog);
    }

    // ------------------------------------------------------------------ 课题一

    private static void BuildIcons(PreviewCatalog catalog, List<PreviewIssue> issues)
    {
        string error;
        var contract = TaskContracts.Load<Task1Contract>(TaskContracts.Task1Path, out error);
        if (contract == null || contract.delivery == null || contract.slots == null)
        {
            Add(issues, IssueSeverity.Error, Task1Name, TaskContracts.Task1Path, error ?? "task.json 结构不完整");
            return;
        }

        string root = Norm(contract.delivery.scanRoot);
        int canvasW = contract.delivery.canvas != null ? contract.delivery.canvas.width : 0;
        int canvasH = contract.delivery.canvas != null ? contract.delivery.canvas.height : 0;

        var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var slot in contract.slots) expected.Add(slot.id + ".png");

        foreach (var slot in contract.slots)
        {
            var entry = new IconEntry { id = slot.id, label = slot.label };
            string path = root + "/" + slot.id + ".png";

            if (!File.Exists(path))
            {
                if (slot.required)
                    Add(issues, IssueSeverity.Error, Task1Name, path, "缺少 " + slot.id + ".png（" + slot.label + "）");
                catalog.icons.Add(entry);
                continue;
            }

            if (!TryReadPngSize(path, out int w, out int h, out string sizeError))
            {
                Add(issues, IssueSeverity.Error, Task1Name, path, sizeError);
            }
            else
            {
                entry.sourceWidth = w;
                entry.sourceHeight = h;
                entry.sizeOk = w == canvasW && h == canvasH;
                if (!entry.sizeOk)
                    Add(issues, IssueSeverity.Error, Task1Name, path, $"尺寸 {w}×{h}，要求 {canvasW}×{canvasH}");
            }

            entry.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (entry.sprite == null)
                Add(issues, IssueSeverity.Error, Task1Name, path, "没有按 Sprite 导入，执行 Tools/美术课题/重新导入任务资源");

            catalog.icons.Add(entry);
        }

        if (Directory.Exists(root))
        {
            foreach (string file in Directory.GetFiles(root, "*.png"))
            {
                string name = Path.GetFileName(file);
                if (!expected.Contains(name))
                    Add(issues, IssueSeverity.Warning, Task1Name, Norm(file), "契约里没有 " + name + "，不会出现在预览里");
            }
        }
    }

    // ------------------------------------------------------------------ 课题二

    private static void BuildCharacter(PreviewCatalog catalog, List<PreviewIssue> issues)
    {
        string error;
        var contract = TaskContracts.Load<Task2Contract>(TaskContracts.Task2Path, out error);
        if (contract == null || contract.delivery == null || contract.states == null)
        {
            Add(issues, IssueSeverity.Error, Task2Name, TaskContracts.Task2Path, error ?? "task.json 结构不完整");
            return;
        }

        float fps = contract.delivery.frameRate > 0 ? contract.delivery.frameRate : 12f;
        string root = Norm(contract.delivery.scanRoot);
        var clips = new Dictionary<string, AnimationClip>(StringComparer.Ordinal);

        foreach (var spec in contract.states)
        {
            string folder = root + "/" + spec.folder;
            List<string> pngs = ListPngs(folder);

            if (pngs.Count == 0)
            {
                if (spec.required)
                    Add(issues, IssueSeverity.Error, Task2Name, folder, "状态 " + spec.name + " 没有任何帧");

                // 帧被删掉了，旧的 clip 也要清掉，否则状态机会指向一份空动画
                string staleClip = ClipDir + "/" + spec.name + ".anim";
                if (File.Exists(staleClip)) AssetDatabase.DeleteAsset(staleClip);
                continue;
            }

            if (pngs.Count < spec.minFrames)
                Add(issues, IssueSeverity.Warning, Task2Name, folder,
                    $"状态 {spec.name} 只有 {pngs.Count} 帧，建议至少 {spec.minFrames} 帧");

            var frames = new List<Sprite>();
            int firstW = 0, firstH = 0;

            foreach (string png in pngs)
            {
                if (TryReadPngSize(png, out int w, out int h, out string sizeError))
                {
                    if (firstW == 0) { firstW = w; firstH = h; }
                    else if (w != firstW || h != firstH)
                        Add(issues, IssueSeverity.Error, Task2Name, png,
                            $"画布 {w}×{h} 与同状态首帧 {firstW}×{firstH} 不一致，播放时角色会跳");
                }
                else
                {
                    Add(issues, IssueSeverity.Error, Task2Name, png, sizeError);
                }

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(png);
                if (sprite != null) frames.Add(sprite);
                else Add(issues, IssueSeverity.Error, Task2Name, png, "没有按 Sprite 导入");
            }

            if (frames.Count == 0) continue;

            var clip = BuildClip(spec.name, frames, fps, spec.loop, issues);
            if (clip != null) clips[spec.name] = clip;

            catalog.character.Add(new CharacterEntry
            {
                state = spec.name,
                loop = spec.loop,
                frameRate = fps,
                minFrames = spec.minFrames,
                frames = frames.ToArray(),
                clip = clip
            });
        }

        var controller = EnsureController(contract, clips, issues);
        catalog.characterPrefab = EnsurePrefab(controller, catalog.character);
    }

    private static AnimationClip BuildClip(string state, List<Sprite> frames, float fps, bool loop, List<PreviewIssue> issues)
    {
        string path = ClipDir + "/" + state + ".anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
        }
        else
        {
            // 原地复用：不能删了重建，否则 GUID 变化会让 Controller 里的引用丢失
            clip.ClearCurves();
        }

        clip.frameRate = fps;
        clip.name = state;

        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keys = new ObjectReferenceKeyframe[frames.Count];
        for (int i = 0; i < frames.Count; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i / fps,
                value = frames[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        settings.startTime = 0f;
        settings.stopTime = frames.Count / fps;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
        return clip;
    }

    private static AnimatorController EnsureController(Task2Contract contract, Dictionary<string, AnimationClip> clips,
        List<PreviewIssue> issues)
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Dead", AnimatorControllerParameterType.Bool);

            var machine = controller.layers[0].stateMachine;
            var states = new Dictionary<string, AnimatorState>(StringComparer.Ordinal);
            foreach (var spec in contract.states)
                states[spec.name] = machine.AddState(spec.name);

            if (!states.ContainsKey("Idle"))
            {
                Add(issues, IssueSeverity.Error, Task2Name, ControllerPath, "状态表里缺少 Idle，无法设置默认状态");
                return controller;
            }

            machine.defaultState = states["Idle"];

            AnimatorState idle = states["Idle"];
            AnimatorState run = SafeState(states, "Run");
            AnimatorState attack = SafeState(states, "Attack");
            AnimatorState hit = SafeState(states, "Hit");
            AnimatorState die = SafeState(states, "Die");

            if (run != null)
            {
                var toRun = idle.AddTransition(run);
                toRun.hasExitTime = false;
                toRun.duration = 0.05f;
                toRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

                var toIdle = run.AddTransition(idle);
                toIdle.hasExitTime = false;
                toIdle.duration = 0.10f;
                toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            }

            if (attack != null)
            {
                var anyToAttack = machine.AddAnyStateTransition(attack);
                anyToAttack.hasExitTime = false;
                anyToAttack.duration = 0.05f;
                anyToAttack.AddCondition(AnimatorConditionMode.IfNot, 0f, "Dead");
                anyToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

                var back = attack.AddTransition(idle);
                back.hasExitTime = true;
                back.exitTime = 0.95f;
                back.duration = 0.10f;
            }

            if (hit != null)
            {
                var anyToHit = machine.AddAnyStateTransition(hit);
                anyToHit.hasExitTime = false;
                anyToHit.duration = 0.03f;
                anyToHit.AddCondition(AnimatorConditionMode.IfNot, 0f, "Dead");
                anyToHit.AddCondition(AnimatorConditionMode.If, 0f, "Hit");

                var back = hit.AddTransition(idle);
                back.hasExitTime = true;
                back.exitTime = 0.95f;
                back.duration = 0.10f;
            }

            if (die != null)
            {
                var anyToDie = machine.AddAnyStateTransition(die);
                anyToDie.hasExitTime = false;
                anyToDie.duration = 0.10f;
                anyToDie.AddCondition(AnimatorConditionMode.If, 0f, "Dead");

                var revive = die.AddTransition(idle);
                revive.hasExitTime = true;
                revive.exitTime = 1f;
                revive.duration = 0.10f;
                revive.AddCondition(AnimatorConditionMode.IfNot, 0f, "Dead");
            }

            Debug.Log("[美术课题] 已创建状态机 " + ControllerPath + "（只会创建一次，之后不会覆盖你的手动调整）");
        }

        // 每次生成都同步一遍 motion：帧数变了 clip 会更新，状态机要跟上
        var machineStates = controller.layers[0].stateMachine.states;
        foreach (var child in machineStates)
        {
            if (clips.TryGetValue(child.state.name, out var clip) && clip != null)
                child.state.motion = clip;
            else
                child.state.motion = null;   // 对应 clip 已删除，避免留下悬空引用
        }

        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static AnimatorState SafeState(Dictionary<string, AnimatorState> states, string name)
    {
        return states.TryGetValue(name, out var state) ? state : null;
    }

    private static GameObject EnsurePrefab(AnimatorController controller, List<CharacterEntry> entries)
    {
        Sprite idle = null;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].state == "Idle" && entries[i].frames != null && entries[i].frames.Length > 0)
            {
                idle = entries[i].frames[0];
                break;
            }
        }

        var go = new GameObject("Task2_Character");
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = idle;
        renderer.sortingOrder = 10;

        var animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    // ------------------------------------------------------------------ 课题三

    private static void BuildVfx(PreviewCatalog catalog, List<PreviewIssue> issues)
    {
        string error;
        var contract = TaskContracts.Load<Task3Contract>(TaskContracts.Task3Path, out error);
        if (contract == null || contract.delivery == null || contract.elements == null || contract.types == null)
        {
            Add(issues, IssueSeverity.Error, Task3Name, TaskContracts.Task3Path, error ?? "task.json 结构不完整");
            return;
        }

        var requiredElements = new HashSet<string>(
            contract.requiredElements ?? new string[0], StringComparer.OrdinalIgnoreCase);
        float fps = contract.delivery.frameRate > 0 ? contract.delivery.frameRate : 24f;
        string root = Norm(contract.delivery.scanRoot);

        foreach (var element in contract.elements)
        {
            foreach (var type in contract.types)
            {
                string id = element.name + "_" + type.name;
                string folder = root + "/" + element.name + "/" + type.name;
                bool required = type.required && requiredElements.Contains(element.name);
                List<string> pngs = ListPngs(folder);

                if (pngs.Count == 0)
                {
                    if (required)
                        Add(issues, IssueSeverity.Error, Task3Name, folder,
                            "缺少 " + id + "（" + element.label + type.label + "）");
                    continue;
                }

                if (pngs.Count < type.minFrames)
                    Add(issues, IssueSeverity.Warning, Task3Name, folder,
                        id + " 只有 " + pngs.Count + " 帧，建议至少 " + type.minFrames + " 帧");

                var frames = new List<Sprite>();
                int firstW = 0, firstH = 0;

                foreach (string png in pngs)
                {
                    if (TryReadPngSize(png, out int w, out int h, out string sizeError))
                    {
                        if (firstW == 0) { firstW = w; firstH = h; }
                        else if (w != firstW || h != firstH)
                            Add(issues, IssueSeverity.Warning, Task3Name, png,
                                $"画布 {w}×{h} 与同组首帧 {firstW}×{firstH} 不一致");
                    }
                    else
                    {
                        Add(issues, IssueSeverity.Error, Task3Name, png, sizeError);
                    }

                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(png);
                    if (sprite != null) frames.Add(sprite);
                    else Add(issues, IssueSeverity.Error, Task3Name, png, "没有按 Sprite 导入");
                }

                if (frames.Count == 0) continue;

                catalog.vfx.Add(new VfxEntry
                {
                    id = id,
                    element = element.name,
                    type = type.name,
                    required = required,
                    loop = type.loop,
                    frameRate = fps,
                    frames = frames.ToArray()
                });
            }
        }
    }

    // ------------------------------------------------------------------ 工具

    private static PreviewCatalog LoadOrCreateCatalog()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<PreviewCatalog>(CatalogAssetPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<PreviewCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
        }
        return catalog;
    }

    private static string Norm(string path)
    {
        return string.IsNullOrEmpty(path) ? string.Empty : path.Replace('\\', '/').TrimEnd('/');
    }

    private static List<string> ListPngs(string folder)
    {
        var list = new List<string>();
        if (!Directory.Exists(folder)) return list;

        foreach (string file in Directory.GetFiles(folder, "*.png", SearchOption.TopDirectoryOnly))
            list.Add(Norm(file));

        // 文件名按数字自然排序，兼容没补零的 _1/_2/_10
        list.Sort(EditorUtility.NaturalCompare);
        return list;
    }

    /// <summary>
    /// 直接读 PNG 文件头拿真实尺寸。不能读 Texture2D.width —— 那是导入后的大小，
    /// maxTextureSize / NPOT 缩放会把它悄悄改掉。
    /// </summary>
    private static bool TryReadPngSize(string path, out int width, out int height, out string error)
    {
        width = 0;
        height = 0;
        error = null;

        try
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var header = new byte[33];
                if (stream.Read(header, 0, header.Length) < 33)
                {
                    error = "文件过短，不是有效的 PNG";
                    return false;
                }

                if (header[0] != 0x89 || header[1] != 0x50 || header[2] != 0x4E || header[3] != 0x47)
                {
                    stream.Position = 0;
                    var head = new byte[64];
                    int read = stream.Read(head, 0, head.Length);
                    string text = System.Text.Encoding.ASCII.GetString(head, 0, read);

                    error = text.StartsWith("version https://git-lfs", StringComparison.Ordinal)
                        ? "这是 Git LFS 指针文件，执行 git lfs pull 后再生成"
                        : "不是有效的 PNG 文件";
                    return false;
                }

                if (header[12] != (byte)'I' || header[13] != (byte)'H' ||
                    header[14] != (byte)'D' || header[15] != (byte)'R')
                {
                    error = "PNG 缺少 IHDR 块";
                    return false;
                }

                width = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
                height = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];
                return width > 0 && height > 0;
            }
        }
        catch (Exception e)
        {
            error = "读取失败：" + e.Message;
            return false;
        }
    }

    private static void Add(List<PreviewIssue> issues, IssueSeverity severity, string task, string path, string message)
    {
        issues.Add(new PreviewIssue
        {
            severity = severity,
            task = task,
            path = path,
            message = string.IsNullOrEmpty(message) ? "未知问题" : message
        });
    }

    private static void LogSummary(PreviewCatalog catalog)
    {
        int iconsOk = catalog.icons.Count(i => i.sprite != null);
        int frameCount = catalog.character.Sum(c => c.frames != null ? c.frames.Length : 0);

        Debug.Log("[美术课题] 预览数据已生成 · " +
                  $"图标 {iconsOk}/{catalog.icons.Count} · " +
                  $"角色 {catalog.character.Count} 个状态 / {frameCount} 帧 · " +
                  $"特效 {catalog.vfx.Count} 套 · " +
                  $"错误 {catalog.ErrorCount} · 警告 {catalog.WarningCount}");
    }
}

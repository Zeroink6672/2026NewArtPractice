using System.Collections.Generic;
using UnityEngine;

namespace ArtPractice3D.Preview
{
    /// <summary>
    /// 3D 预览：进入 Play 自动读取 PreviewData3D。
    /// [1] 道具  [2] 材质  [3] 场景  [4] 骨骼动画
    /// </summary>
    public class Preview3DBootstrap : MonoBehaviour
    {
        private const string ResourcePath = "Preview/PreviewData3D";

        private PreviewData3D _data;
        private Transform _turntable;
        private Camera _cam;

        private int _task = 1;
        private bool _running;
        private float _yaw;

        private readonly List<Transform> _spinners = new List<Transform>();
        private readonly List<Vector3> _labelPositions = new List<Vector3>();
        private readonly List<string> _labels = new List<string>();
        private readonly List<bool> _labelOk = new List<bool>();

        private Animator _animator;

        private GUIStyle _header;
        private GUIStyle _small;
        private GUIStyle _smallError;
        private GUIStyle _bigError;
        private static Material _neutral;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoStart()
        {
            if (FindObjectOfType<Preview3DBootstrap>() != null) return;

            var go = new GameObject("[Preview3D]");
            go.AddComponent<Preview3DBootstrap>();
        }

        private void Awake()
        {
            _data = Resources.Load<PreviewData3D>(ResourcePath);
            _turntable = new GameObject("Turntable").transform;
            _cam = Camera.main;
            Rebuild();
        }

        private void Update()
        {
            if (_task <= 2)
            {
                for (int i = 0; i < _spinners.Count; i++)
                    if (_spinners[i] != null)
                        _spinners[i].Rotate(0f, 28f * Time.deltaTime, 0f, Space.World);
            }
            else
            {
                _yaw += 12f * Time.deltaTime;
                if (_turntable != null)
                    _turntable.rotation = Quaternion.Euler(0f, _yaw, 0f);
            }

            for (int i = 1; i <= 4; i++)
                if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha0 + i))) SwitchTask(i);

            if (_task == 4 && Input.GetKeyDown(KeyCode.Space))
            {
                _running = !_running;
                if (_animator != null) _animator.SetFloat("Speed", _running ? 2f : 0f);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                _running = false;
                _yaw = 0f;
                if (_animator != null) _animator.SetFloat("Speed", 0f);
            }
        }

        private void SwitchTask(int task)
        {
            if (_task == task) return;
            _task = task;
            Rebuild();
        }

        private void Rebuild()
        {
            for (int i = _turntable.childCount - 1; i >= 0; i--)
                Destroy(_turntable.GetChild(i).gameObject);

            _spinners.Clear();
            _labelPositions.Clear();
            _labels.Clear();
            _labelOk.Clear();
            _animator = null;
            _running = false;

            if (_data == null) return;

            switch (_task)
            {
                case 1: BuildProps(); break;
                case 2: BuildMaterials(); break;
                case 3: BuildScene(); break;
                default: BuildCharacter(); break;
            }
        }

        // ------------------------------------------------------------------ 课题一

        private void BuildProps()
        {
            var props = _data.task1.props;
            int visible = 0;
            foreach (var prop in props)
                if (prop.present && prop.model != null) visible++;

            const float spacing = 1.6f;
            float startX = -(visible - 1) * spacing * 0.5f;
            int index = 0;

            foreach (var prop in props)
            {
                if (!prop.present || prop.model == null) continue;

                float x = startX + index * spacing;
                index++;

                var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pedestal.name = "Pedestal_" + prop.id;
                Destroy(pedestal.GetComponent<Collider>());
                pedestal.transform.SetParent(_turntable, false);
                pedestal.transform.localScale = new Vector3(0.9f, 0.7f, 0.9f);
                pedestal.transform.localPosition = new Vector3(x, 0.35f, 0f);
                var pedestalRenderer = pedestal.GetComponent<Renderer>();
                if (pedestalRenderer != null) pedestalRenderer.sharedMaterial = Neutral();

                var instance = Instantiate(prop.model, _turntable);
                instance.name = prop.id;
                ApplyMaterial(instance, prop.material);

                float scale = FitScale(instance, 1.0f);
                instance.transform.localScale = Vector3.one * scale;

                float bottom = BottomOf(instance);
                instance.transform.localPosition = new Vector3(x, 0.7f - bottom, 0f);

                _spinners.Add(instance.transform);
                _labelPositions.Add(new Vector3(x, 0.7f - bottom + 1.1f * scale, 0f));
                _labels.Add(prop.id);
                _labelOk.Add(true);
            }

            FrameCamera(new Vector3(0f, 2.6f, -7.5f), new Vector3(0f, 0.9f, 0f), 40f);
        }

        // ------------------------------------------------------------------ 课题二

        private void BuildMaterials()
        {
            var materials = _data.task2.materials;
            float x = -2.4f;
            float modelX = 1.8f;

            foreach (var info in materials)
            {
                if (!info.present) continue;

                if (info.tileable)
                {
                    var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.name = "Sphere_" + info.id;
                    Destroy(sphere.GetComponent<Collider>());
                    sphere.transform.SetParent(_turntable, false);
                    sphere.transform.localPosition = new Vector3(x, 0.6f, 0f);
                    sphere.transform.localScale = Vector3.one * 1.1f;
                    var renderer = sphere.GetComponent<Renderer>();
                    if (renderer != null) renderer.sharedMaterial = info.material;

                    _spinners.Add(sphere.transform);
                    _labelPositions.Add(new Vector3(x, 1.35f, 0f));
                    _labels.Add(info.id);
                    _labelOk.Add(true);
                    x += 1.3f;
                }
                else if (info.model != null)
                {
                    var instance = Instantiate(info.model, _turntable);
                    instance.name = info.id;
                    ApplyMaterial(instance, info.material);

                    float scale = FitScale(instance, 1.3f);
                    instance.transform.localScale = Vector3.one * scale;
                    float bottom = BottomOf(instance);
                    instance.transform.localPosition = new Vector3(modelX, -bottom, 0f);

                    _spinners.Add(instance.transform);
                    _labelPositions.Add(new Vector3(modelX, 1.6f, 0f));
                    _labels.Add(info.id);
                    _labelOk.Add(true);
                    modelX += 1.6f;
                }
            }

            FrameCamera(new Vector3(0.4f, 2.2f, -8f), new Vector3(0.4f, 0.7f, 0f), 40f);
        }

        // ------------------------------------------------------------------ 课题三

        private void BuildScene()
        {
            if (_data.task3 == null || _data.task3.model == null) return;

            var instance = Instantiate(_data.task3.model, _turntable);
            instance.name = "Diorama";
            instance.transform.localPosition = Vector3.zero;
            ConvertMaterials(instance);

            FrameCamera(new Vector3(0f, 3.2f, -6.5f), new Vector3(0f, 1.2f, 0f), 40f);
        }

        // ------------------------------------------------------------------ 课题四

        private void BuildCharacter()
        {
            if (_data.basePrefab == null) return;

            var instance = Instantiate(_data.basePrefab, _turntable);
            instance.name = "BaseCharacter";

            var renderer = instance.GetComponentInChildren<Renderer>();
            if (renderer != null)
                instance.transform.localPosition = new Vector3(0f, -renderer.bounds.min.y, 0f);

            _animator = instance.GetComponent<Animator>();
            if (_animator != null && _data.controller != null)
                _animator.runtimeAnimatorController = _data.controller;

            FrameCamera(new Vector3(0f, 2.4f, -5.2f), new Vector3(0f, 1.2f, 0f), 35f);
        }

        // ------------------------------------------------------------------ 工具

        private void FrameCamera(Vector3 position, Vector3 target, float fov)
        {
            if (_cam == null) return;
            _cam.transform.position = position;
            _cam.transform.LookAt(target);
            _cam.fieldOfView = fov;
        }

        private static float FitScale(GameObject instance, float target)
        {
            var renderer = instance.GetComponentInChildren<Renderer>();
            if (renderer == null) return 1f;

            Vector3 size = renderer.bounds.size;
            float max = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
            return max <= 0.0001f ? 1f : target / max;
        }

        private static float BottomOf(GameObject instance)
        {
            var renderer = instance.GetComponentInChildren<Renderer>();
            return renderer != null ? renderer.bounds.min.y : 0f;
        }

        private static void ApplyMaterial(GameObject instance, Material material)
        {
            if (material == null) return;
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterial = material;
        }

        private static Material Neutral()
        {
            if (_neutral != null) return _neutral;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return null;

            _neutral = new Material(shader) { name = "PreviewNeutral" };
            _neutral.SetColor("_BaseColor", new Color(0.26f, 0.27f, 0.3f, 1f));
            return _neutral;
        }

        private static void ConvertMaterials(GameObject root)
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) return;

            var cache = new Dictionary<Material, Material>();

            foreach (var renderer in root.GetComponentsInChildren<Renderer>())
            {
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    var source = materials[i];
                    if (source == null) continue;

                    if (!cache.TryGetValue(source, out var converted))
                    {
                        converted = new Material(lit) { name = source.name + " (URP)" };

                        if (source.HasProperty("_MainTex") && source.GetTexture("_MainTex") != null)
                            converted.SetTexture("_BaseMap", source.GetTexture("_MainTex"));

                        if (source.HasProperty("_BumpMap") && source.GetTexture("_BumpMap") != null)
                        {
                            converted.SetTexture("_BumpMap", source.GetTexture("_BumpMap"));
                            converted.EnableKeyword("_NORMALMAP");
                        }

                        if (source.HasProperty("_Metallic"))
                            converted.SetFloat("_Metallic", source.GetFloat("_Metallic"));

                        if (source.HasProperty("_Glossiness"))
                            converted.SetFloat("_Smoothness", source.GetFloat("_Glossiness"));

                        cache[source] = converted;
                    }

                    materials[i] = converted;
                }

                renderer.sharedMaterials = materials;
            }
        }

        // ------------------------------------------------------------------ HUD

        private void OnGUI()
        {
            EnsureStyles();

            if (_data == null)
            {
                GUI.Label(new Rect(24f, 40f, 1400f, 80f),
                    "没有找到 3D 预览数据。\n请执行菜单 Tools / 美术课题 / 重新生成 3D 预览数据，然后重新 Play。",
                    _bigError);
                return;
            }

            GUI.Label(new Rect(20f, 10f, 1600f, 26f),
                "3D 预览    [1] 道具    [2] 材质    [3] 场景    [4] 骨骼动画    [空格] Idle/Run    [R] 重置",
                _header);

            string status = _data.ErrorCount > 0
                ? $"自检：{_data.ErrorCount} 个错误 / {_data.WarningCount} 个警告 —— Tools / 美术课题 / 自检（3D）"
                : $"自检：通过（{_data.WarningCount} 个警告）";
            GUI.Label(new Rect(20f, 34f, 1600f, 26f), status,
                _data.ErrorCount > 0 ? _smallError : _header);

            GUI.Label(new Rect(20f, Screen.height - 40f, 1600f, 24f), BottomLine(), _header);

            for (int i = 0; i < _labelPositions.Count && i < _labels.Count; i++)
            {
                if (_cam == null) break;
                Vector3 screen = _cam.WorldToScreenPoint(_labelPositions[i]);
                if (screen.z < 0f) continue;

                GUI.Label(new Rect(screen.x - 80f, Screen.height - screen.y - 10f, 160f, 20f),
                    _labels[i], _labelOk[i] ? _small : _smallError);
            }
        }

        private string BottomLine()
        {
            switch (_task)
            {
                case 1:
                {
                    int ok = 0;
                    foreach (var prop in _data.task1.props)
                        if (prop.present) ok++;
                    return $"课题一 · 道具 {ok}/{_data.task1.props.Count}";
                }
                case 2:
                {
                    int ok = 0;
                    foreach (var material in _data.task2.materials)
                        if (material.present) ok++;
                    return $"课题二 · 材质 {ok}/{_data.task2.materials.Count}";
                }
                case 3:
                    return _data.task3 != null && _data.task3.ready
                        ? $"课题三 · 面数 {_data.task3.triangles} · 材质 {_data.task3.materials} · " +
                          $"包围盒 {_data.task3.size.x:F2}×{_data.task3.size.y:F2}×{_data.task3.size.z:F2}"
                        : "课题三 · 还没有 Diorama.fbx";
                default:
                    return $"课题四 · clips {CountClips()}/{_data.task4.clips.Count}";
            }
        }

        private int CountClips()
        {
            int n = 0;
            foreach (var clip in _data.task4.clips)
                if (clip.present) n++;
            return n;
        }

        private void EnsureStyles()
        {
            if (_header != null) return;

            _header = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            _header.normal.textColor = new Color(0.85f, 0.87f, 0.9f);

            _small = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.UpperCenter };
            _small.normal.textColor = new Color(0.78f, 0.8f, 0.84f);

            _smallError = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            _smallError.normal.textColor = new Color(1f, 0.45f, 0.4f);

            _bigError = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true };
            _bigError.normal.textColor = new Color(1f, 0.55f, 0.45f);
        }
    }
}

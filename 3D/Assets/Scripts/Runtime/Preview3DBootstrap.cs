using System.Collections.Generic;
using UnityEngine;

namespace ArtPractice3D.Preview
{
    /// <summary>
    /// 3D 预览：进入 Play 自动读取 PreviewData3D。
    /// [3] 场景（课题三） / [4] 骨骼动画（课题四），转台自动旋转。
    /// </summary>
    public class Preview3DBootstrap : MonoBehaviour
    {
        private const string ResourcePath = "Preview/PreviewData3D";

        private PreviewData3D _data;
        private Transform _turntable;
        private GameObject _current;
        private Animator _animator;
        private Camera _cam;

        private int _task = 3;
        private bool _running;
        private float _yaw;

        private GUIStyle _header;
        private GUIStyle _small;
        private GUIStyle _smallError;
        private GUIStyle _bigError;

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
            _yaw += 12f * Time.deltaTime;
            if (_turntable != null)
                _turntable.rotation = Quaternion.Euler(0f, _yaw, 0f);

            if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchTask(3);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchTask(4);

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
            if (_current != null) Destroy(_current);
            _current = null;
            _animator = null;
            _running = false;

            if (_data == null) return;

            if (_task == 3) BuildScene();
            else BuildCharacter();
        }

        private void BuildScene()
        {
            if (_data.task3 == null || _data.task3.model == null) return;

            _current = Instantiate(_data.task3.model, _turntable);
            _current.name = "Diorama";
            _current.transform.localPosition = Vector3.zero;
            ConvertMaterials(_current);

            FrameCamera(new Vector3(0f, 3.2f, -6.5f), new Vector3(0f, 1.2f, 0f), 40f);
        }

        private void BuildCharacter()
        {
            if (_data.basePrefab == null) return;

            _current = Instantiate(_data.basePrefab, _turntable);
            _current.name = "BaseCharacter";

            var renderer = _current.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                float offset = -renderer.bounds.min.y;
                _current.transform.localPosition = new Vector3(0f, offset, 0f);
            }

            _animator = _current.GetComponent<Animator>();
            if (_animator != null && _data.controller != null)
                _animator.runtimeAnimatorController = _data.controller;

            FrameCamera(new Vector3(0f, 2.4f, -5.2f), new Vector3(0f, 1.2f, 0f), 35f);
        }

        private void FrameCamera(Vector3 position, Vector3 target, float fov)
        {
            if (_cam == null) return;
            _cam.transform.position = position;
            _cam.transform.LookAt(target);
            _cam.fieldOfView = fov;
        }

        /// <summary>把 FBX 自带的标准材质换成 URP/Lit，避免整屏粉色。</summary>
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

            GUI.Label(new Rect(20f, 10f, 1400f, 26f),
                "3D 预览      [3] 场景      [4] 骨骼动画      [空格] 切换 Idle/Run      [R] 重置      转台自动旋转",
                _header);

            string status = _data.ErrorCount > 0
                ? $"自检：{_data.ErrorCount} 个错误 / {_data.WarningCount} 个警告 —— Tools / 美术课题 / 自检（3D）"
                : $"自检：通过（{_data.WarningCount} 个警告）";
            GUI.Label(new Rect(20f, 34f, 1400f, 26f), status,
                _data.ErrorCount > 0 ? _smallError : _header);

            if (_task == 3) DrawSceneInfo();
            else DrawClipList();
        }

        private void DrawSceneInfo()
        {
            var info = _data.task3;
            string text = info != null && info.ready
                ? $"课题三 · 面数 {info.triangles} · 材质 {info.materials} · 包围盒 {info.size.x:F2} × {info.size.y:F2} × {info.size.z:F2}"
                : "课题三 · 还没有 Diorama.fbx";

            GUI.Label(new Rect(20f, Screen.height - 40f, 1400f, 24f), text,
                info != null && info.ready ? _header : _smallError);
        }

        private void DrawClipList()
        {
            int present = 0;
            foreach (var clip in _data.task4.clips)
                if (clip.present) present++;

            var clips = _data.task4.clips;
            float y = Screen.height - 24f - clips.Count * 20f - 26f;

            GUI.Label(new Rect(20f, y, 700f, 24f), $"课题四 · clips {present}/{clips.Count}", _header);
            y += 24f;

            foreach (var clip in clips)
            {
                string text = clip.present
                    ? $"✓ {clip.name}    {clip.duration:F2}s{(clip.loop ? "  · 循环" : "")}"
                    : $"✗ {clip.name}    缺失";

                GUI.Label(new Rect(28f, y, 700f, 20f), text, clip.present ? _small : _smallError);
                y += 20f;
            }
        }

        private void EnsureStyles()
        {
            if (_header != null) return;

            _header = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            _header.normal.textColor = new Color(0.85f, 0.87f, 0.9f);

            _small = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            _small.normal.textColor = new Color(0.78f, 0.8f, 0.84f);

            _smallError = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            _smallError.normal.textColor = new Color(1f, 0.45f, 0.4f);

            _bigError = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true };
            _bigError.normal.textColor = new Color(1f, 0.55f, 0.45f);
        }
    }
}

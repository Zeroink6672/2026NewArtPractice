using UnityEngine;

namespace ArtPractice3D.Preview
{
    /// <summary>
    /// 3D 预览：进入 Play 自动读取 PreviewData3D，摆放基础角色并挂上生成的状态机。
    /// 场景里不需要摆任何东西。
    /// </summary>
    public class Preview3DBootstrap : MonoBehaviour
    {
        private const string ResourcePath = "Preview/PreviewData3D";

        private PreviewData3D _data;
        private Transform _turntable;
        private Animator _animator;
        private float _yaw;
        private bool _running;

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

            if (_data == null || _data.basePrefab == null) return;

            var character = Instantiate(_data.basePrefab, _turntable);
            character.name = "BaseCharacter";

            var renderer = character.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                // 素材脚底在原点下方约 0.2，这里对齐地面
                float offset = -renderer.bounds.min.y;
                character.transform.localPosition = new Vector3(0f, offset, 0f);
            }

            _animator = character.GetComponent<Animator>();
            if (_animator != null && _data.controller != null)
                _animator.runtimeAnimatorController = _data.controller;
        }

        private void Update()
        {
            _yaw += 18f * Time.deltaTime;
            if (_turntable != null)
                _turntable.rotation = Quaternion.Euler(0f, _yaw, 0f);

            if (Input.GetKeyDown(KeyCode.Space))
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
                "课题四 · 骨骼动画      [空格] 切换 Idle / Run      [R] 重置      转台自动旋转",
                _header);

            string status = _data.ErrorCount > 0
                ? $"自检：{_data.ErrorCount} 个错误 / {_data.WarningCount} 个警告 —— Tools / 美术课题 / 自检（3D）"
                : $"自检：通过（{_data.WarningCount} 个警告）";
            GUI.Label(new Rect(20f, 34f, 1400f, 26f), status,
                _data.ErrorCount > 0 ? _smallError : _header);

            DrawClipList();
        }

        private void DrawClipList()
        {
            int present = 0;
            foreach (var clip in _data.task4.clips)
                if (clip.present) present++;

            var clips = _data.task4.clips;
            float y = Screen.height - 24f - clips.Count * 20f - 26f;

            GUI.Label(new Rect(20f, y, 700f, 24f),
                $"clips {present}/{clips.Count}", _header);
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

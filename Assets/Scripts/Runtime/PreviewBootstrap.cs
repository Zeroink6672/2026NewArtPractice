using System.Collections.Generic;
using UnityEngine;

namespace ArtPractice.Preview
{
    /// <summary>
    /// 运行时预览。场景里不需要挂任何东西：进入 Play 自动创建，
    /// 内容全部来自 Resources/Preview/PreviewCatalog.asset（由 Editor 菜单生成）。
    /// </summary>
    public class PreviewBootstrap : MonoBehaviour
    {
        private const string CatalogResourcePath = "Preview/PreviewCatalog";

        private PreviewCatalog _catalog;
        private Camera _cam;
        private Transform _root;

        private int _task = 1;
        private int _vfxIndex;

        private readonly List<string> _iconLabels = new List<string>();
        private readonly List<Vector3> _iconPositions = new List<Vector3>();
        private readonly List<bool> _iconSizeOk = new List<bool>();

        private Animator _animator;
        private float _attackTimer = 2.5f;
        private float _hitTimer = 7f;

        private SpriteRenderer _vfxRenderer;
        private Sprite[] _vfxFrames;
        private float _vfxFps = 24f;
        private bool _vfxLoop = true;
        private float _vfxTimer;
        private float _vfxHold;
        private int _vfxFrame;
        private string _vfxTitle = string.Empty;

        private GUIStyle _headerStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _errorStyle;
        private GUIStyle _bigStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoStart()
        {
            if (FindObjectOfType<PreviewBootstrap>() != null) return;

            var go = new GameObject("[PreviewBootstrap]");
            go.AddComponent<PreviewBootstrap>();
        }

        private void Awake()
        {
            _catalog = Resources.Load<PreviewCatalog>(CatalogResourcePath);
            EnsureCamera();
            Rebuild();
        }

        private void EnsureCamera()
        {
            _cam = Camera.main;
            if (_cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                _cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }

            _cam.orthographic = true;
            _cam.orthographicSize = 6f;
            _cam.transform.position = new Vector3(0f, 0f, -10f);
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = new Color(0.11f, 0.11f, 0.13f, 1f);
        }

        private void Rebuild()
        {
            if (_root != null)
            {
                _root.gameObject.SetActive(false);
                Destroy(_root.gameObject);
            }

            _root = new GameObject("PreviewContent").transform;

            _iconLabels.Clear();
            _iconPositions.Clear();
            _iconSizeOk.Clear();
            _animator = null;
            _vfxRenderer = null;
            _vfxFrames = null;
            _vfxFrame = 0;
            _vfxTimer = 0f;
            _vfxHold = 0f;
            _vfxTitle = string.Empty;

            if (_catalog == null) return;

            if (_task == 1) BuildIcons();
            else if (_task == 2) BuildCharacter();
            else BuildVfx();
        }

        private void BuildIcons()
        {
            const int columns = 6;
            const float cell = 1.5f;

            int count = _catalog.icons.Count;
            int rows = Mathf.Max(1, Mathf.CeilToInt(count / (float)columns));
            float startX = -(columns - 1) * cell * 0.5f;
            float startY = (rows - 1) * cell * 0.5f + 0.4f;

            for (int i = 0; i < count; i++)
            {
                IconEntry entry = _catalog.icons[i];
                var position = new Vector3(
                    startX + (i % columns) * cell,
                    startY - (i / columns) * cell,
                    0f);

                var go = new GameObject(entry.id);
                go.transform.SetParent(_root, false);
                go.transform.localPosition = position;

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = entry.sprite;
                renderer.sortingOrder = 10;

                if (entry.sprite != null)
                {
                    Vector3 size = entry.sprite.bounds.size;
                    float fit = 1.05f / Mathf.Max(size.x, size.y, 0.0001f);
                    go.transform.localScale = Vector3.one * fit;
                }

                _iconPositions.Add(position);
                _iconLabels.Add(entry.sprite == null ? "缺 " + entry.id : entry.id);
                _iconSizeOk.Add(entry.sizeOk);
            }
        }

        private void BuildCharacter()
        {
            if (_catalog.characterPrefab == null) return;

            var instance = Instantiate(_catalog.characterPrefab, new Vector3(0f, -3.6f, 0f),
                Quaternion.identity, _root);
            instance.transform.localScale = Vector3.one * 0.5f;

            _animator = instance.GetComponent<Animator>();
            _attackTimer = 2.5f;
            _hitTimer = 7f;
        }

        private void BuildVfx()
        {
            if (_catalog.vfx.Count == 0) return;

            _vfxIndex = Mathf.Clamp(_vfxIndex, 0, _catalog.vfx.Count - 1);
            VfxEntry entry = _catalog.vfx[_vfxIndex];

            var go = new GameObject("Vfx");
            go.transform.SetParent(_root, false);
            go.transform.localPosition = Vector3.zero;

            _vfxRenderer = go.AddComponent<SpriteRenderer>();
            _vfxRenderer.sortingOrder = 10;

            _vfxFrames = entry.frames;
            _vfxFps = entry.frameRate > 0f ? entry.frameRate : 24f;
            _vfxLoop = entry.loop;
            _vfxFrame = 0;
            _vfxTimer = 0f;
            _vfxHold = 0f;
            _vfxTitle = entry.id + (entry.required ? string.Empty : "（加分项）");

            if (_vfxFrames != null && _vfxFrames.Length > 0)
            {
                _vfxRenderer.sprite = _vfxFrames[0];
                Vector3 size = _vfxFrames[0].bounds.size;
                float fit = 4.5f / Mathf.Max(size.x, size.y, 0.0001f);
                go.transform.localScale = Vector3.one * fit;
            }
        }

        private void Update()
        {
            if (_catalog == null) return;

            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchTask(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchTask(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchTask(3);

            if (_task == 3 && _catalog.vfx.Count > 0)
            {
                if (Input.GetKeyDown(KeyCode.Q)) StepVfx(-1);
                if (Input.GetKeyDown(KeyCode.E)) StepVfx(1);
            }

            if (Input.GetKeyDown(KeyCode.R)) Rebuild();

            TickCharacter();
            TickVfx();
        }

        private void SwitchTask(int task)
        {
            if (_task == task) return;
            _task = task;
            _vfxIndex = 0;
            Rebuild();
        }

        private void StepVfx(int delta)
        {
            int count = _catalog.vfx.Count;
            if (count == 0) return;
            _vfxIndex = (_vfxIndex + delta + count) % count;
            Rebuild();
        }

        private void TickCharacter()
        {
            if (_task != 2 || _animator == null) return;

            _animator.SetFloat("Speed", Mathf.Abs(Mathf.Sin(Time.time * 0.8f)) * 2f);

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _animator.SetTrigger("Attack");
                _attackTimer = 4f;
            }

            _hitTimer -= Time.deltaTime;
            if (_hitTimer <= 0f)
            {
                _animator.SetTrigger("Hit");
                _hitTimer = 9f;
            }
        }

        private void TickVfx()
        {
            if (_task != 3 || _vfxRenderer == null || _vfxFrames == null || _vfxFrames.Length == 0) return;

            float frameDuration = 1f / Mathf.Max(1f, _vfxFps);
            _vfxTimer += Time.deltaTime;

            while (_vfxTimer >= frameDuration)
            {
                _vfxTimer -= frameDuration;
                _vfxFrame++;

                if (_vfxFrame >= _vfxFrames.Length)
                {
                    if (_vfxLoop)
                    {
                        _vfxFrame = 0;
                    }
                    else
                    {
                        _vfxFrame = _vfxFrames.Length - 1;
                        _vfxHold += frameDuration;
                        if (_vfxHold >= 0.7f)
                        {
                            _vfxHold = 0f;
                            _vfxFrame = 0;
                        }
                        break;
                    }
                }
            }

            _vfxRenderer.sprite = _vfxFrames[_vfxFrame];
        }

        private void OnGUI()
        {
            EnsureStyles();

            if (_catalog == null)
            {
                GUI.Label(new Rect(24f, 40f, 1400f, 80f),
                    "没有找到预览数据。\n请执行菜单 Tools / 美术课题 / 重新生成预览数据，然后重新 Play。",
                    _bigStyle);
                return;
            }

            string status = _catalog.ErrorCount > 0
                ? $"自检：{_catalog.ErrorCount} 个错误 / {_catalog.WarningCount} 个警告 —— 执行 Tools / 美术课题 / 自检 查看"
                : $"自检：通过（{_catalog.WarningCount} 个警告）";

            GUI.Label(new Rect(20f, 10f, 1400f, 26f),
                "[1] 图标    [2] 角色    [3] 特效        [Q/E] 切换特效        [R] 重播",
                _headerStyle);
            GUI.Label(new Rect(20f, 34f, 1400f, 26f), status,
                _catalog.ErrorCount > 0 ? _errorStyle : _headerStyle);

            if (_task == 1) DrawIconLabels();
            else if (_task == 2) DrawCharacterCaption();
            else DrawVfxCaption();
        }

        private void DrawIconLabels()
        {
            for (int i = 0; i < _iconPositions.Count; i++)
            {
                Vector3 screen = _cam.WorldToScreenPoint(_iconPositions[i]);
                var rect = new Rect(screen.x - 80f, Screen.height - screen.y + 34f, 160f, 20f);
                GUI.Label(rect, _iconLabels[i], _iconSizeOk[i] ? _labelStyle : _errorStyle);
            }
        }

        private void DrawCharacterCaption()
        {
            string text = _catalog.character.Count == 0
                ? "课题二：Idle/Run/Attack/Hit/Die 还没有帧"
                : "课题二：Speed 自动摆动，Attack / Hit 自动触发";

            GUI.Label(new Rect(20f, Screen.height - 40f, 1200f, 24f), text, _headerStyle);
        }

        private void DrawVfxCaption()
        {
            if (_catalog.vfx.Count == 0)
            {
                GUI.Label(new Rect(20f, Screen.height - 40f, 1200f, 24f), "课题三：还没有任何特效帧", _headerStyle);
                return;
            }

            int total = _vfxFrames != null ? _vfxFrames.Length : 0;
            GUI.Label(new Rect(20f, Screen.height - 40f, 1200f, 24f),
                $"{_vfxTitle}    第 {_vfxFrame + 1}/{total} 帧    {_vfxIndex + 1}/{_catalog.vfx.Count}",
                _headerStyle);
        }

        private void EnsureStyles()
        {
            if (_headerStyle != null) return;

            _headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            _headerStyle.normal.textColor = new Color(0.85f, 0.87f, 0.9f);

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.UpperCenter
            };
            _labelStyle.normal.textColor = new Color(0.75f, 0.78f, 0.82f);

            _errorStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.UpperCenter
            };
            _errorStyle.normal.textColor = new Color(1f, 0.45f, 0.4f);

            _bigStyle = new GUIStyle(GUI.skin.label) { fontSize = 18 };
            _bigStyle.normal.textColor = new Color(1f, 0.55f, 0.45f);
            _bigStyle.wordWrap = true;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtPractice.Preview
{
    public enum IssueSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    /// <summary>自检发现问题的一条记录，生成时写入 catalog，运行时只用于显示统计。</summary>
    [Serializable]
    public class PreviewIssue
    {
        public IssueSeverity severity;
        public string task;
        public string path;
        public string message;
    }

    [Serializable]
    public class IconEntry
    {
        public string id;
        public string label;
        public Sprite sprite;
        public int sourceWidth;
        public int sourceHeight;
        public bool sizeOk;
    }

    [Serializable]
    public class CharacterEntry
    {
        public string state;
        public bool loop;
        public float frameRate;
        public int minFrames;
        public Sprite[] frames;
        public AnimationClip clip;
    }

    [Serializable]
    public class VfxEntry
    {
        public string id;
        public string element;
        public string type;
        public bool required;
        public bool loop;
        public float frameRate;
        public Sprite[] frames;
    }

    /// <summary>
    /// 三个课题的预览数据。由 Editor 菜单生成，运行时通过 Resources 读取。
    /// 这个文件是生成物：手改会在下次生成时被覆盖。
    /// </summary>
    public class PreviewCatalog : ScriptableObject
    {
        public string generatedBy;

        public List<IconEntry> icons = new List<IconEntry>();
        public List<CharacterEntry> character = new List<CharacterEntry>();
        public List<VfxEntry> vfx = new List<VfxEntry>();
        public List<PreviewIssue> issues = new List<PreviewIssue>();

        public GameObject characterPrefab;

        public int ErrorCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < issues.Count; i++)
                    if (issues[i].severity == IssueSeverity.Error) n++;
                return n;
            }
        }

        public int WarningCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < issues.Count; i++)
                    if (issues[i].severity == IssueSeverity.Warning) n++;
                return n;
            }
        }
    }
}

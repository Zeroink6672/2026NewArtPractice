using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtPractice3D.Preview
{
    public enum IssueSeverity3D
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    [Serializable]
    public class PreviewIssue3D
    {
        public IssueSeverity3D severity;
        public string task;
        public string path;
        public string message;
    }

    [Serializable]
    public class ClipInfo
    {
        public string name;
        public float duration;
        public bool loop;
        public bool present;
    }

    [Serializable]
    public class Task4Info
    {
        public List<ClipInfo> clips = new List<ClipInfo>();
    }

    /// <summary>
    /// 3D 工程的预览数据。生成物，不入库；打开工程时自动重建。
    /// </summary>
    public class PreviewData3D : ScriptableObject
    {
        public string generatedBy;

        public GameObject basePrefab;
        public RuntimeAnimatorController controller;
        public Task4Info task4 = new Task4Info();

        public List<PreviewIssue3D> issues = new List<PreviewIssue3D>();

        public int ErrorCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < issues.Count; i++)
                    if (issues[i].severity == IssueSeverity3D.Error) n++;
                return n;
            }
        }

        public int WarningCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < issues.Count; i++)
                    if (issues[i].severity == IssueSeverity3D.Warning) n++;
                return n;
            }
        }
    }
}

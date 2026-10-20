using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// task.json 的数据模型与读取。字段名必须和 JSON 完全一致（JsonUtility 按名匹配）。
/// </summary>
[Serializable]
public class CanvasSpec
{
    public int width;
    public int height;
}

[Serializable]
public class DeliverySpec
{
    public string scanRoot;
    public string filePattern;
    public string layout;
    public string fileNameRule;
    public CanvasSpec canvas;
    public string pivot;
    public int pixelsPerUnit;
    public int frameRate;
    public string note;
}

[Serializable]
public class IconSlotSpec
{
    public string id;
    public string label;
    public bool required;
}

[Serializable]
public class StateSpec
{
    public string name;
    public string folder;
    public bool required;
    public bool loop;
    public int minFrames;
    public string description;
}

[Serializable]
public class ElementSpec
{
    public string name;
    public string label;
}

[Serializable]
public class VfxTypeSpec
{
    public string name;
    public string label;
    public bool required;
    public bool loop;
    public int minFrames;
    public string blend;
}

[Serializable]
public class Task1Contract
{
    public string taskId;
    public string title;
    public DeliverySpec delivery;
    public IconSlotSpec[] slots;
}

[Serializable]
public class Task2Contract
{
    public string taskId;
    public string title;
    public DeliverySpec delivery;
    public StateSpec[] states;
}

[Serializable]
public class Task3Contract
{
    public string taskId;
    public string title;
    public DeliverySpec delivery;
    public ElementSpec[] elements;
    public VfxTypeSpec[] types;
    public string[] requiredElements;
}

public static class TaskContracts
{
    public const string Task1Path = "Assets/Tasks/Task1_Icons/task.json";
    public const string Task2Path = "Assets/Tasks/Task2_Character/task.json";
    public const string Task3Path = "Assets/Tasks/Task3_VFX/task.json";

    public static T Load<T>(string assetPath, out string error) where T : class
    {
        error = null;
        if (!File.Exists(assetPath))
        {
            error = "找不到契约文件 " + assetPath;
            return null;
        }

        try
        {
            return JsonUtility.FromJson<T>(File.ReadAllText(assetPath));
        }
        catch (Exception e)
        {
            error = "解析 " + assetPath + " 失败：" + e.Message;
            return null;
        }
    }
}

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[Serializable]
public class BaseCharacterSpec
{
    public string prefab;
    public string source;
    public string license;
    public string note;
}

[Serializable]
public class DeliverySpec3D
{
    public string scanRoot;
    public string filePattern;
    public int frameRate;
    public string rootMotion;
    public string[] takeNames;
    public string note;
}

[Serializable]
public class ClipSpec3D
{
    public string name;
    public bool loop;
    public float minSeconds;
    public float maxSeconds;
    public string note;
}

[Serializable]
public class Task4ContractData
{
    public string taskId;
    public string title;
    public int contractVersion;
    public string summary;
    public BaseCharacterSpec baseCharacter;
    public DeliverySpec3D delivery;
    public ClipSpec3D[] clips;
}

public static class Task4ContractLoader
{
    public const string ContractPath = "Assets/Tasks/Task4_Character/task.json";

    public static Task4ContractData Load(out string error)
    {
        error = null;
        if (!File.Exists(ContractPath))
        {
            error = "找不到契约文件 " + ContractPath;
            return null;
        }

        try
        {
            var data = JsonUtility.FromJson<Task4ContractData>(File.ReadAllText(ContractPath));
            if (data == null || data.delivery == null || data.clips == null)
            {
                error = "task.json 结构不完整";
                return null;
            }
            return data;
        }
        catch (Exception e)
        {
            error = "解析 task.json 失败：" + e.Message;
            return null;
        }
    }
}

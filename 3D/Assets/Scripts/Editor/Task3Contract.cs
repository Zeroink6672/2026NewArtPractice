using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[Serializable]
public class SizeSpec
{
    public float x;
    public float y;
    public float z;
}

[Serializable]
public class BaseAssetsSpec
{
    public string kit;
    public string blockout;
    public string license;
    public string note;
}

[Serializable]
public class SceneDeliverySpec
{
    public string scanRoot;
    public string fileName;
    public int maxTriangles;
    public int maxTextures;
    public SizeSpec maxSize;
}

[Serializable]
public class Task3ContractData
{
    public string taskId;
    public string title;
    public int contractVersion;
    public string summary;
    public BaseAssetsSpec baseAssets;
    public SceneDeliverySpec delivery;
}

public static class Task3ContractLoader
{
    public const string ContractPath = "Assets/Tasks/Task3_Diorama/task.json";

    public static Task3ContractData Load(out string error)
    {
        error = null;
        if (!File.Exists(ContractPath))
        {
            error = "找不到契约文件 " + ContractPath;
            return null;
        }

        try
        {
            var data = JsonUtility.FromJson<Task3ContractData>(File.ReadAllText(ContractPath));
            if (data == null || data.baseAssets == null || data.delivery == null || data.delivery.maxSize == null)
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

public class DataManager
{
    private static DataManager instance;
    private Dictionary<string, object> dataDict;
    private const string SAVE_KEY = "game_wabls";

    public static DataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new DataManager();
            }
            return instance;
        }
    }

    private DataManager()
    {
        LoadData();
    }

    // 加载数据
    private void LoadData()
    {
#if PF_TT && !UNITY_EDITOR
        string jsonData = TTSDK.TT.LoadSaving<string>(SAVE_KEY);
#else
        string jsonData = PlayerPrefs.GetString(SAVE_KEY, "");
#endif

        if (string.IsNullOrEmpty(jsonData))
        {
            dataDict = new Dictionary<string, object>();
        }
        else
        {
            try
            {
                dataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            }
            catch
            {
                dataDict = new Dictionary<string, object>();
            }
        }
    }

    // 保存数据
    private void SaveData()
    {
        string jsonData = JsonConvert.SerializeObject(dataDict);
#if PF_TT && !UNITY_EDITOR
        TTSDK.TT.Save<string>(jsonData, SAVE_KEY);
#else
        PlayerPrefs.SetString(SAVE_KEY, jsonData);
        PlayerPrefs.Save();
#endif

    }

    // 设置单个数据
    public void SetData<T>(string key, T value)
    {
        if (value == null)
        {
            if (dataDict.ContainsKey(key))
            {
                dataDict.Remove(key);
            }
        }
        else
        {
            if (value is IConvertible || value is string)
            {
                dataDict[key] = value;
            }
            else
            {
                dataDict[key] = JsonConvert.SerializeObject(value);
            }
        }
        SaveData();
    }

    // 设置多个数据
    public void SetData(Hashtable data)
    {
        foreach (DictionaryEntry entry in data)
        {
            string key = entry.Key.ToString();
            object value = entry.Value;

            if (value == null)
            {
                if (dataDict.ContainsKey(key))
                {
                    dataDict.Remove(key);
                }
            }
            else
            {
                if (value is IConvertible || value is string)
                {
                    dataDict[key] = value;
                }
                else
                {
                    dataDict[key] = JsonConvert.SerializeObject(value);
                }
            }
        }
        SaveData();
    }

    // 获取数据
    public T GetData<T>(string key, T defaultValue = default)
    {
        if (!dataDict.ContainsKey(key))
        {
            return defaultValue;
        }

        try
        {
            object value = dataDict[key];
            if (value == null)
            {
                return defaultValue;
            }

            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else
            {
                if (value is string strValue)
                {
                    return JsonConvert.DeserializeObject<T>(strValue);
                }
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
            }
        }
        catch
        {
            return defaultValue;
        }
    }

    // 获取原始数据
    public Dictionary<string, object> GetAllData()
    {
        return new(dataDict);
    }

    // 检查键是否存在
    public bool HasKey(string key)
    {
        return dataDict.ContainsKey(key);
    }

    // 删除指定键的数据
    public void DeleteKey(string key)
    {
        if (dataDict.ContainsKey(key))
        {
            dataDict.Remove(key);
            SaveData();
        }
    }

    // 清除所有数据
    public void ClearAll()
    {
#if PF_TT && !UNITY_EDITOR
        TTSDK.TT.ClearAllSavings();
        TTSDK.TT.Save<string>("", SAVE_KEY);
#else
        dataDict.Clear();
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
#endif
    }
}

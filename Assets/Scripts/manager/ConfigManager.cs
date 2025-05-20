using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

internal class ConfigManager
{
  private ConfigManager()
  {
    instance = this;
    config = new();
    // LoadAllConfig();
  }
  private bool isLoaded = false;

  private static ConfigManager instance;

  public static ConfigManager Ins
  {
    get
    {
      instance ??= new ConfigManager();
      return instance;
    }
  }

  private object GetConfig(string name)
  {
    if (config.TryGetValue(name, out object value))
    {
      return value;
    }
    return default;
  }

  public T GetConfig<T>()
  {
    if (config.TryGetValue(typeof(T).Name, out object value))
    {
      return (T)value;
    }
    return default;
  }

  private Dictionary<string, object> config;

  public Dictionary<string, T> GetConfigObj<T>()
  {
    return (Dictionary<string, T>)GetConfig(typeof(T).Name);
  }
  public List<T> GetConfigArr<T>()
  {
    return (List<T>)GetConfig(typeof(T).Name);
  }

  public T GetConfigByid<T>(int id)
  {
    var data = GetConfigObj<T>();
    if (!data.TryGetValue(id.ToString(), out T value))
    {
      return default;
    }
    return value;
  }

  public T GetConfigByid<T>(string id)
  {
    var data = GetConfigObj<T>();
    if (!data.TryGetValue(id, out T value))
    {
      return default;
    }
    return value;
  }

  //根据类型和获取类型获取礼包配置
  public List<ConfigGift> GetGiftConfigByType(int tagType)
  {
    var data = GetConfigObj<ConfigGift>();
    List<ConfigGift> list = new List<ConfigGift>();
    foreach (var item in data)
    {
      if (item.Value.tagType == tagType)
      {
        list.Add(item.Value);
      }
    }
    return list;
  }

  //根据类型和获取类型获取头像配置
  public ConfigProfile GetProfileConfigByTypeAndParam(int type, int param)
  {
    var data = GetConfigObj<ConfigProfile>();
    foreach (var item in data)
    {
      if (item.Value.type == type && item.Value.param == param)
      {
        return item.Value;
      }
    }
    return null;
  }

  //根据类型和获取类型获取头像配置
  public ConfigBackground GetBackgroundConfigByTypeAndParam(int type, int param)
  {
    var data = GetConfigObj<ConfigBackground>();
    foreach (var item in data)
    {
      if (item.Value.type == type && item.Value.param == param)
      {
        return item.Value;
      }
    }
    return null;
  }

  public void LoadAllConfig(TextAsset[] jsonArr)
  {
    if (isLoaded) return;
    isLoaded = true;
    foreach (TextAsset json in jsonArr)
    {
      LoadConfig(json.name, json.text);
    }
  }

  public void LoadAllConfig(Newtonsoft.Json.Linq.JObject data)
  {
    if (isLoaded) return;
    isLoaded = true;
    foreach (var item in data)
    {
      LoadConfig(item.Key, item.Value.ToString());
    }
  }

  private void LoadConfig(string name, string str)
  {
    // Debug.Log("name ===" + name);
    // Debug.Log("str ===" + str);
    // 先创建开放泛型
    Type openType;
    // 再创建具象泛型
    Type target;

    name = "Config" + char.ToUpper(name[0]) + name.Substring(1);
    str = str.Trim();
    if (name == "ConfigCommon")
    {
      config.Add(name, JsonConvert.DeserializeObject<ConfigCommon>(str));
      return;
    }
    if (name == "ConfigTask")
    {
      config.Add(name, JsonConvert.DeserializeObject<ConfigTask>(str));
      return;
    }
    target = Type.GetType(name);
    if (target == null) return;
    if (str[0] == '{')
    {
      openType = typeof(Dictionary<,>);
      target = openType.MakeGenericType(new Type[] { typeof(string), target });
    }
    else if (str[0] == '[')
    {
      openType = typeof(List<>);
      target = openType.MakeGenericType(new Type[] { target });
    }
    config.Add(name, JsonConvert.DeserializeObject(str, target));
    // foreach (var kvp in config)
    // {
    // 	Debug.Log(kvp.Key);
    // 	Debug.Log(kvp.Value);
    // }
  }

}
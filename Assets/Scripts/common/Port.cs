using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
#if PF_WX
using WeChatWASM;
#endif

internal struct Constant
{
    
}

internal struct VIEW_NAME
{
    public const string Set = "SetUI";
    public const string Tip = "TipUI";
    public const string LoadWindow = "LoadWindow";
}

[System.Serializable]
public class OpenDataMessage
{
    // type 用于表明时间类型
    public string type;

    public string shareTicket;

    public int score;
}


internal struct GameData
{
    public static SceneType sceneType = SceneType.Game;
    public static bool isLocal = true;

    public static string version = "1.0.1";

    public static ShareConfig shareConfig;

    public static AdConfig adConfig;

    public static SystemInfo systemInfo = null;

#if PF_WX
    public static WXOpenDataContext openDataContext = null;
#endif
}

enum SceneType
{
    Home,
    Game,
}


internal struct ConstantFun
{

    public static int GetRandom(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public static int RandomWeight(int[] weightArr)
    {
        int max = 0;
        for (int i = weightArr.Length - 1; i >= 0; i--)
        {
            max += weightArr[i];
        }
        float randomValue = UnityEngine.Random.value * max;
        for (int i = weightArr.Length - 1; i >= 0; i--)
        {
            randomValue -= weightArr[i];
            if (randomValue <= 0)
            {
                return i;
            }
        }
        return 0;
    }
    public static T RandomArray<T>(T[] arr)
    {
        return arr[GetRandom(0, arr.Length)];
    }

    public static T RandomArray<T>(List<T> arr)
    {
        return arr[GetRandom(0, arr.Count)];
    }

    public static void ListRandomSort<T>(List<T> list)
    {
        list.Sort((a, b) => UnityEngine.Random.value > 0.5 ? 1 : -1);
    }

    public static void RemoveAllChild(Transform tf)
    {
        while (tf.childCount != 0) UnityEngine.Object.DestroyImmediate(tf.GetChild(0).gameObject);
    }

    private static Action _adAction;
    private static Action<bool> _videoAction = (bool isSuccess) =>
    {
        if (isSuccess)
        {
            _adAction?.Invoke();
        }
        else
        {
            ShowTip("广告未播放完成");
        }
        _adAction = null;
    };


    public static void PlayVideo(int id, Action action)
    {
        _adAction = action;
        AdManager.Ins.ShowAd(id, _videoAction);
    }

    /**获取默认分辨率与真实分辨率 */
    public static float getWindowScaling()
    {
        // 获取默认分辨率与真实分辨率
        float defaultWidth = 750f;
        float defaultHeight = 1334f;
        float currentWidth = Screen.width;
        float currentHeight = Screen.height;
        // 计算缩放比例
        float scale = (currentHeight / currentWidth) * (defaultWidth / defaultHeight);
        return scale;
    }
    public static GameObject _tip;
    public static void ShowTip(string str)
    {
        UIManager.Instance.ShowMsg(str);
    }
}

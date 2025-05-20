using System;
using UnityEngine;


#if PF_WX
using WeChatWASM;
#elif PF_TT
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
#else
#endif

/// <summary>
/// 平台接口基类
/// </summary>
public abstract class PlatformBase
{
    // 分享接口
    public abstract void Share(string title, string imageUrl, Action<bool> callback);

    // 广告接口
    public abstract void ShowAd(string adId, Action<bool> callback);

    // 获取平台名称
    public abstract string GetPlatformName();

    // 平台特定功能检查
    public abstract bool IsFeatureSupported(string featureId);

    // 获取系统信息
    public abstract void GetSystemInfo();

    // 前往侧边栏
    public abstract void goSideBar();

    // 震动
    public abstract void vibrate(int vibrateType = 1);

    public abstract void getEnterOptions();

    public abstract void setUserRecord(int score);
}

/// <summary>
/// 平台管理器 - 单例模式
/// </summary>
public class PlatformManager : MonoBehaviour
{
    private static PlatformManager _instance;
    public static PlatformManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("PlatformManager");
                _instance = go.AddComponent<PlatformManager>();
            }
            return _instance;
        }
    }

    private PlatformBase _platform;
    public PlatformBase CurrentPlatform => _platform;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitPlatform();
    }

    /// <summary>
    /// 获取分享信息（标题和图片URL）
    /// </summary>
    /// <returns>包含标题和图片URL的元组</returns>
    private (string title, string imageUrl) GetShareInfo()
    {
        string title = "";
        string imageUrl = "";
        int titleLen = GameData.shareConfig!.title?.Length ?? 0;
        int imageUrlLen = GameData.shareConfig!.imageUrl?.Length ?? 0;
        if (titleLen > 0)
        {
            int index = UnityEngine.Random.Range(0, titleLen);
            title = GameData.shareConfig.title[index];
            if (imageUrlLen > 0)
            {
                if (imageUrlLen == titleLen)
                {
                    imageUrl = GameData.shareConfig.imageUrl[index];
                }
                else
                {
                    imageUrl = GameData.shareConfig.imageUrl[UnityEngine.Random.Range(0, imageUrlLen)];
                }
            }
        }
        if (!string.IsNullOrEmpty(imageUrl))
        {
            imageUrl = imageUrl.Replace("__URL__", "https://res.wqop2018.com/mp/projects/ndpx/share/");
        }
        // Debug.Log("分享信息：" + title + " " + imageUrl);
        return (title, imageUrl);
    }

    /// <summary>
    /// 初始化当前平台
    /// </summary>
    private void InitPlatform()
    {
#if  PF_WEB || UNITY_EDITOR 
        _platform = new DefaultPlatform();
        Debug.Log("使用默认平台");
#elif PF_WX
        _platform = new WeChatPlatform();
        // (string title, string imageUrl) = GetShareInfo();
        // WX.OnShareAppMessage(new WXShareAppMessageParam()
        // {
        //     title = title,
        //     imageUrl = imageUrl
        // });
        Debug.Log("使用微信小游戏平台");
#elif PF_TT
        _platform = new TiktokPlatform();
        Debug.Log("使用抖音小游戏平台");
#elif PF_KS
        _platform = new KuaishouPlatform();
        Debug.Log("使用快手Native平台");
#elif PF_KS_WASM
        _platform = new KuaishouWasmPlatform();
        Debug.Log("使用快手WASM平台");
#endif
        _platform.GetSystemInfo();
    }

    // 平台封装方法
    public void Share(Action<bool> callback)
    {
        (string title, string imageUrl) = GetShareInfo();
        _platform.Share(title, imageUrl, callback);
    }

    public void ShowAd(string adId, Action<bool> callback)
    {
        _platform.ShowAd(adId, callback);
    }

    public string GetPlatformName()
    {
        return _platform.GetPlatformName();
    }

    public bool IsFeatureSupported(string featureId)
    {
        return _platform.IsFeatureSupported(featureId);
    }


    public void GetSystemInfo()
    {
        _platform.GetSystemInfo();
    }

    public void goSideBar()
    {
        _platform.goSideBar();
    }

    public void vibrate(int vibrateType = 1)
    {
        _platform.vibrate(vibrateType);
    }

    public void getEnterOptions()
    {
        _platform.getEnterOptions();
    }

    public void setUserRecord(int score)
    {
        _platform.setUserRecord(score);
    }
}

/// <summary>
/// 默认平台实现
/// </summary>
public class DefaultPlatform : PlatformBase
{
    public override void Share(string title, string imageUrl, Action<bool> callback)
    {
        Debug.Log("分享成功");
        callback?.Invoke(true);
    }

    public override void ShowAd(string adId, Action<bool> callback)
    {
        callback?.Invoke(true);
    }

    public override string GetPlatformName()
    {
        return "DEV";
    }

    public override bool IsFeatureSupported(string featureId)
    {
        return false;
    }

    public override void GetSystemInfo()
    {
        // throw new NotImplementedException();
        Debug.Log("获取系统信息");
    }

    public override void goSideBar()
    {
    }

    public override void vibrate(int vibrateType = 1)
    {

    }

    public override void getEnterOptions()
    {
        Debug.Log("获取启动参数");
    }

    public override void setUserRecord(int score)
    {
        // throw new NotImplementedException();
    }
}

/// <summary>
/// 微信小游戏平台实现
/// </summary>
#if PF_WX
public class WeChatPlatform : PlatformBase
{

    public override void Share(string title, string imageUrl, Action<bool> callback)
    {

        // Debug.Log("进入微信小游戏分享");
        long shareTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000;

        Action<OnShowListenerResult> shareCallback = null!;

        shareCallback = (res) =>
        {
            Debug.Log("Time.time:" + DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000);
            Debug.Log("shareTime:" + shareTime);
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000 - shareTime >= 3)
            {
                Debug.Log("分享成功");
                callback?.Invoke(true);
            }
            else
            {
                Debug.Log("分享失败");
                callback?.Invoke(false);
            }
            WX.OffShow(shareCallback!);
        };
        WX.OnShow(shareCallback);

        WX.ShareAppMessage(new ShareAppMessageOption
        {
            title = title,
            imageUrl = imageUrl
        });
    }

    public override void ShowAd(string adId, Action<bool> callback)
    {
        // 微信小游戏广告实现
        Debug.Log($"微信小游戏显示广告：{adId}");
        callback?.Invoke(true);
    }

    public override string GetPlatformName()
    {
        return "WX";
    }

    public override bool IsFeatureSupported(string featureId)
    {
        // 根据featureId判断微信小游戏平台是否支持特定功能
        return true;
    }

    public override void GetSystemInfo()
    {
        WX.GetSystemInfo(new GetSystemInfoOption()
        {
            success = (res) =>
            {
                Debug.Log("获取系统信息成功");
                GameData.systemInfo = new SystemInfo()
                {
                    system = res.system,
                    model = res.model,
                    platform = res.platform,
                    version = res.version
                };
            }
        });
    }

    public override void vibrate(int vibrateType = 1)
    {
        if (vibrateType == 1)
        {
            VibrateShortOption result = new VibrateShortOption();
            WX.VibrateShort(result);
        }
        else if (vibrateType == 2)
        {
            VibrateShortOption result = new VibrateShortOption();
            result.type = "heavy";
            WX.VibrateShort(result);
        }
    }

    public override void getEnterOptions()
    {
        Debug.Log("WX获取启动参数(待开发)");
    }

    public override void goSideBar()
    {

    }

    public override void setUserRecord(int score)
    {   
        if (GameData.openDataContext != null)
        {
            OpenDataMessage msgData = new OpenDataMessage();
		    msgData.type = "setUserRecord";
		    msgData.score = UserModel.Ins.levelId;
		    string msg = JsonUtility.ToJson(msgData);
		    GameData.openDataContext.PostMessage(msg);
        }
    }
}
#endif

/// <summary>
/// 抖音小游戏平台实现
/// </summary>
#if PF_TT
public class TiktokPlatform : PlatformBase
{

    public override void Share(string title, string imageUrl, Action<bool> callback)
    {
        TT.ShareAppMessage(new JsonData()
        {
            ["title"] = title,
            ["imageUrl"] = imageUrl
        }, (res) =>
        {
            Debug.Log("分享成功");
            callback?.Invoke(true);
        }, (errCode) =>
        {
            Debug.Log($"分享失败,errCode:{errCode}");
            callback?.Invoke(false);
        }, () =>
        {
            Debug.Log($"分享取消");
            callback?.Invoke(false);
        });
    }

    public override void ShowAd(string adId, Action<bool> callback)
    {
        // 抖音小游戏广告实现
        Debug.Log($"抖音小游戏显示广告：{adId}");
        callback?.Invoke(true);
    }

    public override string GetPlatformName()
    {
        return "TT";
    }

    public override bool IsFeatureSupported(string featureId)
    {
        // 根据featureId判断抖音小游戏平台是否支持特定功能
        return true;
    }

    public override void GetSystemInfo()
    {
        TTSystemInfo res = TT.GetSystemInfo();
        GameData.systemInfo = new SystemInfo()
        {
            system = res.system,
            model = res.model,
            platform = res.platform,
            version = res.ttVersion
        };
    }

    public override void goSideBar()
    {
        var data = new JsonData
        {
            ["scene"] = "sidebar",
        };
        TT.NavigateToScene(data, () =>
        {
            Debug.Log("navigate to scene success");
        }, () =>
        {
            Debug.Log("navigate to scene complete");
        }, (errCode, errMsg) =>
        {
            Debug.Log($"navigate to scene error, errCode:{errCode}, errMsg:{errMsg}");
        });
    }

    public override void vibrate(int vibrateType = 1)
    {
        if (vibrateType == 1)
        {
            TT.Vibrate(new long[] { 100 });

        }
        else if (vibrateType == 2)
        {
            TT.Vibrate(new long[] { 600 });
        }
    }

    public override void getEnterOptions()
    {
        Debug.Log("TT获取启动参数(待开发)");
        TT.GetLaunchOptionsSync();
    }

    public override void setUserRecord(int score)
    {
        // TT.SetUserRecord(score);
    }
}
#endif

/// <summary>
/// 快手Native平台实现
/// </summary>
#if PF_KS
public class KuaishouPlatform : PlatformBase
{
    public override void Share(string title, string imageUrl, Action<bool> callback)
    {
        // 快手Native分享实现
        Debug.Log($"快手Native分享：{title}");
        callback?.Invoke(true);
    }

    public override void ShowAd(string adId, Action<bool> callback)
    {
        // 快手Native广告实现
        Debug.Log($"快手Native显示广告：{adId}");
        callback?.Invoke(true);
    }

    public override string GetPlatformName()
    {
        return "KuaishouNative";
    }

    public override bool IsFeatureSupported(string featureId)
    {
        // 根据featureId判断快手Native平台是否支持特定功能
        return true;
    }

    public override void getEnterOptions()
    {
        Debug.Log("快手Native获取启动参数(待开发)");
    }
}
#endif

/// <summary>
/// 快手WASM平台实现
/// </summary>
#if PF_KS_WASM
public class KuaishouWasmPlatform : PlatformBase
{
    public override void Share(string title, string imageUrl, Action<bool> callback)
    {
        // 快手WASM分享实现
        Debug.Log($"快手WASM分享：{title}");
        callback?.Invoke(true);
    }

    public override void ShowAd(string adId, Action<bool> callback)
    {
        // 快手WASM广告实现
        Debug.Log($"快手WASM显示广告：{adId}");
        callback?.Invoke(true);
    }

    public override string GetPlatformName()
    {
        return "KuaishouWasm";
    }

    public override bool IsFeatureSupported(string featureId)
    {
        // 根据featureId判断快手WASM平台是否支持特定功能
        return true;
    }

    public override void getEnterOptions()
    {
        Debug.Log("快手WASM获取启动参数(待开发)");
    }
}
#endif

internal class ShareConfig
{
    /** 是否打开分享 */
    public bool open;
    /** 分享标题组 */
    public string[] title;
    /** 分享图片组 */
    public string[] imageUrl;
    /** 图片编号 */
    public string[] imageUrlId;
}

internal class AdConfig
{
    /** 是否打开广告 */
    public bool open;
    /** 横幅广告位组 */
    public string[] bannerIds;
    /** 插屏广告位组 */
    public string[] interstitialIds;
    /** 激励广告位组 */
    public string[] rewardedVideoIds;
    /** 原生广告位组 */
    public string[] customIds;
}

internal class SystemInfo
{
    /** 操作系统及版本 */
    public string system;
    /** 设备型号 */
    public string model;
    /** 平台 */
    public string platform;
    /** 版本 */
    public string version;
}




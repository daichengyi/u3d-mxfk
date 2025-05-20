using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZhiSe;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using UnityEngine.ResourceManagement.AsyncOperations;
#if PF_WX
using WeChatWASM;
#elif PF_TT
using TTSDK;
#endif

public class Load : MonoBehaviour
{
    // Start is called before the first frame update
    public Image img_rato;

    //TODO  需要封装一个jsonManager
    public TextAsset[] jsonArr;

    private string goToSceneName = "Start";

    public static LoginData loginData;


    void Start()
    {
        Init();
    }
    private float offsetT = 0;

    void reportTime()
    {
        //数数上报游戏时间
        offsetT = Time.time;
    }

    void Init()
    {
#if PF_WX
        GameData.version = "1.0.4";
#elif PF_WEB
        GameData.version = "1.0.1";
#elif PF_TT
        GameData.version = "1.0.1";
#endif
        // 初始化SDK
        Seeg.Init(new SeegSdkOption()
        {
#if PF_WX || PF_WEB
            gid = "wx_lszjy1",
#elif PF_TT
            gid = "tt_knzmx2",
#endif
            version = GameData.version,
            // 是否由SDK内部自动初始化平台SDK
            // 目前主要针对的是抖音和快手的wasm
            autoInitPF = true,
            // 是否开启数数打点
            // 如果没有运营提到，请务必选择关闭
#if UNITY_EDITOR || PF_WEB
            shushu = false,
#else
            shushu = true,
#endif
        });

#if PF_WX && !UNITY_EDITOR
        WX.PreloadConcurrent(10);
        WX.OnHide((res) =>
        {
            reportTime();
        });
#elif PF_TT 
        TTAppLifeCycle.OnAppHideEvent onAppHide = null;
        onAppHide += () =>
        {
            reportTime();
        };
#endif
        GetConfig();
    }

    void getLogin()
    {
        // 获取用户登录信息
        ZhiSe.LoginOption option = new ZhiSe.LoginOption
        {
            success = (ret) =>
            {
                loginData = ret;
                UserModel.Ins.userId = ret.userId;
                UserModel.Ins.isTest = ret.isTest;
                string[] versionArr = GameData.version.Split('.');
                versionArr[1] = ret.abTest;
                UserModel.Ins.version = string.Join(".", versionArr);
                GetUserData();
                // 登录成功才能上报用户数据
            },
            fail = (code, msg) =>
            {
                Debug.LogFormat("登录失败, 错误码=%s, 错误信息=%s", code, msg);
            },
            complete = () =>
            {
                Debug.Log("登录完成");
            }
        };
        Seeg.OnLoginResult(option);
    }

    void GetConfig()
    {
        // 获取游戏配置
        GetGameConfigOption configOption = new GetGameConfigOption
        {
            success = (ret) =>
            {
                // Debug.Log("游戏配置获取成功=" + (JObject)JObject.Parse(ret));
                JObject config = JObject.Parse(ret);

                JObject mpConfig = (JObject)config["zsmp"];
                // Debug.Log("mpConfig=" + mpConfig);
                foreach (var prop in mpConfig)
                {
                    string _pz = prop.Key;
                    if (_pz == "share")
                    {
                        JObject shareConfig = JObject.Parse(prop.Value.ToString());
                        GameData.shareConfig = shareConfig.ToObject<ShareConfig>();
                        Debug.Log("shareConfig=" + shareConfig);
                    }

                    if (_pz == "ad")
                    {
                        JObject adConfig = JObject.Parse(prop.Value.ToString());
                        GameData.adConfig = adConfig.ToObject<AdConfig>();
                        // Debug.Log("adConfig=" + adConfig);
                    }
                }

                JObject gameConfig = (JObject)config["game"];
                ConfigManager.Ins.LoadAllConfig(gameConfig);
                getLogin();
                _ = LoadScene(goToSceneName);
            },
            fail = (code, msg) =>
            {
                // 获取失败后使用本地配置
                ConfigManager.Ins.LoadAllConfig(jsonArr);
                Debug.LogFormat("游戏配置获取失败, 错误码=%s, 错误信息=%s", code, msg);
                _ = LoadScene(goToSceneName);
            },
            complete = () =>
            {
                Debug.Log("获取配置完成");
            }
        };
        Seeg.GetGameConfig(configOption);
    }

    void GetUserData()
    {
        if (GameData.isLocal)
        {
            var ret = DataManager.Instance.GetAllData();
            string userData = JsonConvert.SerializeObject(ret);
            // Debug.Log("用户数据JSON字符串: " + userData);
            // Debug.Log("游戏用户数据获取成功=" + ret);
            // Debug.LogFormat("游戏用户数据获取成功=%s", ret);
            UserModel.Ins.InitData(ret);
            AdManager.Ins.Init();
        }
        else
        {
            // 获取用户数据
            GetGameDataOption gameDataOption = new GetGameDataOption
            {
                success = (ret) =>
                {
                    UserModel.Ins.InitData(ret);
                    Debug.LogFormat("游戏用户数据获取成功=%s", ret);
                },
                fail = (code, msg) =>
                {
                    // 获取失败后可以考虑重试
                    Debug.LogFormat("游戏用户数据获取失败, 错误码=%s, 错误信息=%s", code, msg);
                },
                complete = () =>
                {
                    Debug.Log("游戏用户数据获取完成");
                    AdManager.Ins.Init();
                }
            };
            Seeg.GetUserData(gameDataOption);
        }
    }

    async Task PreloadAssetAsync()
    {
        await Task.CompletedTask;
    }

    async Task LoadScene(string sceneName)
    {
        await PreloadAssetAsync();
        Debug.Log("预加载资源完成=====");
        maxCurProgess = 0.7f;
        await UIManager.Instance.InitLoading();
        Debug.Log("预加载二级loading完成");
        maxCurProgess = 0.8f;
        var handle = ResourceManager.LoadScene(sceneName);
        while (!handle.IsDone)
        {
            await Task.Yield();
        }
        await handle.Task;
        curProgress = 1f;
        maxCurProgess = 1f;
        img_rato.fillAmount = 1;
        SoundManager.Ins.PlayMusic("bgm");
        _ = UIManager.Instance.PreloadView();
        gameObject.SetActive(false);
        // UIManager.Instance.ShowLoading();
        Debug.Log("加载场景完成--------------");
    }

    float curProgress = 0;

    float maxCurProgess = 0.5f;
    void Update()
    {
        curProgress += Time.deltaTime * 0.001f * 150;
        curProgress = Math.Min(curProgress, maxCurProgess);
        img_rato.fillAmount = curProgress;
    }
}

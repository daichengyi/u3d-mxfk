// 测试或web渠道
//#define PF_WEB
// 微小渠道
//#define PF_WX
// 抖小渠道
// #define PF_TT
// 快小Native渠道
//#define PF_KS
// 快小WASM（WebGL）渠道
//#define PF_KS_WASM

namespace ZhiSe
{
    /// <summary>
    /// SDK初始化参数
    /// </summary>
    public class SeegSdkOption
    {
        /// <summary>
        /// 游戏id
        /// </summary>
        public string gid;
        /// <summary>
        /// 日志等级
        /// </summary>
        public int loggerLevel = 0;
        /// <summary>
        /// 游戏版本号
        /// </summary>
        public string version = "1.0.0";
        /// <summary>
        /// 登录code
        /// </summary>
        public string code = null;
        /// <summary>
        /// 兼容sdk版本
        /// </summary>
        public int fixSdkVer = 0;
        /// <summary>
        /// 自动初始化平台
        /// </summary>
        public bool autoInitPF = false;
        /// <summary>
        /// 是否开启数数
        /// </summary>
        public bool shushu = false;
    }

    public class Seeg : SeegBase
    {
        /// <summary>
        /// 初始化sdk
        /// </summary>
        /// <param name="option">
        ///     gid: 游戏ID
        ///     loggerLevel: 日志等级
        ///     version: 游戏版本号
        ///     code: 登录code
        ///     fixSdkVer: 兼容sdk版本
        ///     autoInitPF: 自动初始化平台
        ///     shushu: 是否开启数数
        /// </param>
        public static void Init(SeegSdkOption option)
        {
            if (option.autoInitPF)
            {
                Bridge.ZS.InitSDK(() =>
                {
                    Init(option.gid, option.loggerLevel, option.version, option.code, option.fixSdkVer, option.shushu);
                });
                return;
            }
            Init(option.gid, option.loggerLevel, option.version, option.code, option.fixSdkVer, option.shushu);
        }

        /// <summary>
        /// 初始化sdk 该已废弃 请使用Seeg.Init(SeegSdkOption option)
        /// </summary>
        public static void Init(string gid, int loggerLevel = 0, string version = "1.0.0", string code = null, int fixSdkVer = 0, bool shushu = false)
        {
            Bridge.GF.zs = new Bridge.CSBridge();
#if UNITY_EDITOR || PF_WEB
            Bridge.GF.zs.Log("===Warning:请注意当前是测试模式===");
#elif PF_WX
            Bridge.GF.zs.env.USER_DATA_PATH = WeChatWASM.WX.env.USER_DATA_PATH;
            if (!gid.StartsWith("wx_"))
            {
                Bridge.GF.zs.Log("===Error:当前平台与gid不对应===");
                return;
            }
#elif PF_TT
            Bridge.GF.zs.env.USER_DATA_PATH = "scfile://user";
            if (!gid.StartsWith("tt_"))
            {
                Bridge.GF.zs.Log("===Error:当前平台与gid不对应===");
                return;
            }
#elif PF_KS || PF_KS_WASM
#if !UNITY_EDITOR
            Bridge.GF.zs.env.USER_DATA_PATH = WeChatWASM.WX.ENV.USER_DATA_PATH;
#endif
            if (!gid.StartsWith("ks_"))
            {
                Bridge.GF.zs.Log("===Error:当前平台与gid不对应===");
                return;
            }
#else
            Bridge.GF.zs.Log("===Error:当前平台暂不支持===");
            return;
#endif
            SdkInit(new SdkOption
            {
                gid = gid,
                loggerLevel = loggerLevel,
                version = version,
                code = code,
                fixSdkVer = fixSdkVer,
                shushu = shushu
            });
#if UNITY_EDITOR
            CheckSdkVersion();
#endif
        }
    }
}

namespace ZhiSe.Bridge
{
    public class FS : FileSystemManager
    {

#if !UNITY_EDITOR && PF_WX
        WeChatWASM.WXFileSystemManager fsMgr = WeChatWASM.WX.GetFileSystemManager();
#elif !UNITY_EDITOR && PF_TT
        TTSDK.TTFileSystemManager fsMgr = TTSDK.TT.GetFileSystemManager();
#elif !UNITY_EDITOR && PF_KS_WASM
        KSWASM.KSFileSystemManager fsMgr = KSWASM.KS.GetFileSystemManager();
#endif

        public bool AccessSync(string path)
        {
#if !UNITY_EDITOR && PF_WX
            var access = fsMgr.AccessSync(path);
            return "access:ok".Equals(access);
#elif !UNITY_EDITOR && PF_TT
            return fsMgr.AccessSync(path);
#elif !UNITY_EDITOR && PF_KS_WASM
            var access = fsMgr.AccessSync(path);
            return "access:ok".Equals(access);
#else
            return false;
#endif
        }

        public void MkdirSync(string dirPath, bool recursive = false)
        {
#if !UNITY_EDITOR && PF_WX
            fsMgr.MkdirSync(dirPath, recursive);
#elif !UNITY_EDITOR && PF_TT
            fsMgr.MkdirSync(dirPath, recursive);
#elif !UNITY_EDITOR && PF_KS_WASM
            fsMgr.MkdirSync(dirPath, recursive);
#endif
        }

        public string ReadFileSync(string filePath, string encoding = "utf8")
        {
#if !UNITY_EDITOR && PF_WX
            return fsMgr.ReadFileSync(filePath, encoding);
#elif !UNITY_EDITOR && PF_TT
            return fsMgr.ReadFileSync(filePath, encoding);
#elif !UNITY_EDITOR && PF_KS_WASM
            return fsMgr.ReadFile(filePath, encoding);
#else
            return null;
#endif
        }

        public void WriteFileSync(string filePath, string data, string encoding)
        {
#if !UNITY_EDITOR && PF_WX
            fsMgr.WriteFileSync(filePath, data, encoding);
#elif !UNITY_EDITOR && PF_TT
            fsMgr.WriteFileSync(filePath, data, encoding);
#elif !UNITY_EDITOR && PF_KS_WASM
            fsMgr.WriteFileSync(filePath, data, encoding);
#endif
        }
    }

    public class ZS
    {
#if PF_TT && !UNITY_EDITOR
        public static TTSDK.VersionType ttVersionType = TTSDK.VersionType.None;
#endif

        public static FS fs = null;

        public static void InitSDK(System.Action action)
        {
#if PF_WX && !UNITY_EDITOR
            WeChatWASM.WX.InitSDK((code)=>
            {
                action?.Invoke();
            });
#elif PF_TT && !UNITY_EDITOR
            TTSDK.TT.InitSDK((code, env) =>
            {
                ttVersionType = env.GetVersionType();
                action?.Invoke();
            });
#elif PF_KS_WASM && !UNITY_EDITOR
             KSWASM.KS.InitSDK((ret) =>
             {
                action?.Invoke();
             }); 
#else
            action?.Invoke();
#endif
        }

        public static SystemInfo GetSystemInfoSync()
        {
            var systemInfo = new SystemInfo();
#if PF_WX && !UNITY_EDITOR
            var info = WeChatWASM.WX.GetSystemInfoSync();
            systemInfo.platform = info.platform;
            systemInfo.brand = info.brand;
            systemInfo.model = info.model;
#elif PF_TT && !UNITY_EDITOR
            var info = TTSDK.TT.GetSystemInfo();
            systemInfo.platform = info.platform;
            systemInfo.brand = info.brand;
            systemInfo.model = info.model;
            systemInfo.version = info.hostVersion;
#elif PF_KS && !UNITY_EDITOR
            systemInfo.platform = "android";
            systemInfo.brand = "";
            systemInfo.model = "";
#elif PF_KS_WASM && !UNITY_EDITOR
            var info = KSWASM.KS.GetSystemInfoSync();
            systemInfo.platform = info.platform;
            systemInfo.brand = info.brand;
            systemInfo.model = info.model;
#else
            systemInfo.platform = "devTools";
            systemInfo.brand = "devTools";
            systemInfo.model = "devTools";
            systemInfo.version = "1.0.0";
#endif
            return systemInfo;
        }

        public static void Login(LoginOption option)
        {
#if PF_WX && !UNITY_EDITOR
            WeChatWASM.WX.Login(new WeChatWASM.LoginOption
            {
                complete = (code) =>
                {
                    option.complete?.Invoke();
                },
                fail = (ret) =>
                {
                    option.fail?.Invoke(System.Convert.ToInt32(ret.errno), ret.errMsg);
                },
                success = (ret) =>
                {
                    option.success?.Invoke(new LoginResult
                    {
                        code = ret.code,
                        pf = "wx",
                        is_old = 1
                    });
                }
            });
#elif PF_TT && !UNITY_EDITOR
            TTSDK.TT.Login((code, anonymousCode, isLogin) =>
            {
                option.success?.Invoke(new LoginResult
                {
                    code = code,
                    pf = "tt",
                    is_old = 1
                });
                option.complete?.Invoke();
            }, (errMsg) =>
            {
                option.fail?.Invoke(-1, errMsg);
                option.complete?.Invoke();
            }, true);
#elif PF_KS && !UNITY_EDITOR
            com.kwai.mini.game.KS.Login((ret) =>
            {
                option.success?.Invoke(new LoginResult
                {
                    code = ret.code,
                    pf = "ks",
                    is_old = 1
                });
                option.complete?.Invoke();
            }, (code, msg) =>
            {
                option.fail?.Invoke(code, msg);
                option.complete?.Invoke();
            });
#elif PF_KS_WASM && !UNITY_EDITOR
            KSWASM.KS.Login((ret) =>
            {
                option.success?.Invoke(new LoginResult
                {
                    code = ret.code,
                    pf = "ks",
                    is_old = 1
                });
                option.complete?.Invoke();
            }, (code, msg) =>
            {
                option.fail?.Invoke(code, msg);
                option.complete?.Invoke();
            });
#else
            var code = GetStorageSync<string>("seeg_login_code");
            if (null == code || "".Equals(code))
            {
                code = $"web_{System.Guid.NewGuid().ToString().Replace("-", "")}";
                SetStorageSync("seeg_login_code", code);
            }
            option.success?.Invoke(new LoginResult
            {
                code = code,
                pf = "zs",
                is_old = 1
            });
            option.complete?.Invoke();
#endif
        }

        public static LaunchOptions GetLaunchOptionsSync()
        {
            var launchOptions = new LaunchOptions();
#if PF_WX && !UNITY_EDITOR
            var launchOption = WeChatWASM.WX.GetLaunchOptionsSync();
            launchOptions.scene = System.Convert.ToString(launchOption.scene);
            launchOptions.query = launchOption.query;
#elif PF_TT && !UNITY_EDITOR
            var launchOption = TTSDK.TT.GetLaunchOptionsSync();
            launchOptions.scene = launchOption.Scene;
            launchOptions.query = launchOption.Query;
#elif PF_KS && !UNITY_EDITOR
            var launchOption = com.kwai.mini.game.KS.GetLaunchOption();
            if (null != launchOption)
            {
                var option = Newtonsoft.Json.Linq.JObject.Parse(launchOption);
                if (option.ContainsKey("scene"))
                {
                    launchOptions.scene = System.Convert.ToString(option["scene"]);
                }
                if (option.ContainsKey("query"))
                {
                    launchOptions.query = option["query"].ToObject<System.Collections.Generic.Dictionary<string, string>>();
                }
            }
#elif PF_KS_WASM && !UNITY_EDITOR
            var launchOption = KSWASM.KS.GetLaunchOptionSync();
            launchOptions.scene = launchOption.from;
            launchOptions.query = launchOption.query;
#else
            launchOptions.scene = "test";
            launchOptions.query = new System.Collections.Generic.Dictionary<string, string>();
#endif
            return launchOptions;
        }

        public static AccountInfo GetAccountInfoSync()
        {
#if PF_WX && !UNITY_EDITOR
            var info = WeChatWASM.WX.GetAccountInfoSync();
            return new AccountInfo
            {
                miniProgram = new MiniProgram
                {
                    appId = info.miniProgram.appId,
                    envVersion = info.miniProgram.envVersion,
                    version = info.miniProgram.version
                }
            };
#elif PF_TT && !UNITY_EDITOR
            var launchOptions = TTSDK.TT.GetLaunchOptionsSync();
            var version = "";
            var appId = "";
            if (null != launchOptions && null != launchOptions.Extra)
            {
                version = launchOptions.Extra["mpVersion"];
                appId = launchOptions.Extra["appId"];
            }
            return new AccountInfo
            {
                miniProgram = new MiniProgram
                {
                    appId = appId,
                    version = version,
                    envVersion = TTSDK.VersionType.Perview == ttVersionType ? "develop" : TTSDK.VersionType.Test == ttVersionType ? "trial" : "release"
                }
            };
#elif PF_KS_WASM && !UNITY_EDITOR
            var systemInfo = KSWASM.KS.GetSystemInfoSync();
            return new AccountInfo
            {
                miniProgram = new MiniProgram
                {
                    appId = systemInfo.host.appId,
                    envVersion = "release",
                    version = "1.0.0"
                }
            };
#else
            return new AccountInfo
            {
                miniProgram = new MiniProgram
                {
                    appId = "",
                    envVersion = "release",
                    version = "1.0.0"
                }
            };
#endif
        }

        public static void ShowModal(ShowModalOption option)
        {
#if PF_WX && !UNITY_EDITOR
            WeChatWASM.WX.ShowModal(new WeChatWASM.ShowModalOption
            {
                title = option.title,
                content = option.content
            });
#else
            Log($"===SeegSDK===\n{option.content}");
#endif
        }

        public static void Log(string msg)
        {
#if PF_KS && !UNITY_EDITOR
            com.kwai.mini.game.KS.Log(msg);
#else
            UnityEngine.Debug.Log(msg);
#endif
        }

        public static void GetFont(System.Action<UnityEngine.Font> action)
        {
            // 获取字体 
#if PF_WX && !UNITY_EDITOR
            WeChatWASM.WX.GetWXFont("https://res.wqop2018.com/mp/sdk/cs/dingtalkjbt.ttf", (font) =>
            {
                action?.Invoke(font);
            });
#elif PF_TT && !UNITY_EDITOR
            TTSDK.TT.GetSystemFont((font) =>
            {
                action?.Invoke(font);
            });
#else
            // 使用系统默认字体
            var font = UnityEngine.Resources.GetBuiltinResource<UnityEngine.Font>("LegacyRuntime.ttf");
            action?.Invoke(font);
#endif
        }

        public static Bridge.FileSystemManager GetFileSystemManager()
        {
            if (null == fs)
            {
                fs = new FS();
            }
            return fs;
        }

        public static void SetStorageSync<T>(string key, T data)
        {
#if !UNITY_EDITOR && PF_WX
            WeChatWASM.WX.StorageSetStringSync(key, Newtonsoft.Json.JsonConvert.ToString(data));
#elif !UNITY_EDITOR && PF_TT
            TTSDK.TT.Save<T>(data, key);
#elif !UNITY_EDITOR && PF_KS_WASM
            KSWASM.KS.StorageSetStringSync(key, Newtonsoft.Json.JsonConvert.ToString(data));
#else
            UnityEngine.PlayerPrefs.SetString(key, Newtonsoft.Json.JsonConvert.ToString(data));
#endif
        }

        public static T GetStorageSync<T>(string key)
        {
#if !UNITY_EDITOR && PF_WX
            if (!WeChatWASM.WX.StorageHasKeySync(key))
            {
                return default;
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(WeChatWASM.WX.StorageGetStringSync(key, ""));
#elif !UNITY_EDITOR && PF_TT
            return TTSDK.TT.LoadSaving<T>(key);
#elif !UNITY_EDITOR && PF_KS_WASM
            if (!KSWASM.KS.StorageHasKeySync(key))
            {
                return default;
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(KSWASM.KS.StorageGetStringSync(key, ""));
#else
            if (!UnityEngine.PlayerPrefs.HasKey(key))
            {
                return default;
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(UnityEngine.PlayerPrefs.GetString(key));
#endif
        }

    }

    public class CSBridge : BridgeBase
    {
        public override SystemInfo GetSystemInfoSync()
        {
            return ZS.GetSystemInfoSync();
        }

        public override void Login(LoginOption option)
        {
            ZS.Login(option);
        }

        public override LaunchOptions GetLaunchOptionsSync()
        {
            return ZS.GetLaunchOptionsSync();
        }

        public override AccountInfo GetAccountInfoSync()
        {
            return ZS.GetAccountInfoSync();
        }

        public override void ShowModal(ShowModalOption option)
        {
            ZS.ShowModal(option);
        }

        public override FileSystemManager GetFileSystemManager()
        {
            return ZS.GetFileSystemManager();
        }

        public override void Log(string msg)
        {
            ZS.Log(msg);
        }

        public override void SetStorageSync<T>(string key, T data)
        {
            ZS.SetStorageSync<T>(key, data);
        }

        public override T GetStorageSync<T>(string key)
        {
            return ZS.GetStorageSync<T>(key);
        }
    }
}
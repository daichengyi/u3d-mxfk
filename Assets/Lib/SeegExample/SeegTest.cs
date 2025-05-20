using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ZhiSe;

public class SeegTest : MonoBehaviour
{
    public Transform listView;
    public Text text;

    void Start()
    {
        // 初始化SDK
        Seeg.Init(new SeegSdkOption
        {
            // 注意gid不是appID
            // 类似于平台+"_"+游戏简称例如，tt_test
            gid = "运营提供的gid",

            // 非必填参数

            // 日志打印等级，建议线上关闭，默认为0
            loggerLevel = 5,
            // 是否由SDK内部自动初始化平台SDK，默认关闭
            // 如果自己没有实现，或不能保证在初始化后初始该SDK，请选择打开
            autoInitPF = true,
            // 是否开启数数打点，默认关闭
            // 如果没有运营提到，请务必选择关闭
            shushu = false
        }); ;

        // 在初始化成功前进行上报
        // 将会在初始化成功后补充上报
        ReportSSEvent();

        // 此方法非必须，仅此处测试需要用到字体
        // 仅测试微小、抖小平台
        // 如果也有获取字体的需要，请自行处理
        ZhiSe.Bridge.ZS.GetFont((font) =>
        {
            this.text.font = font;
            // SDK初始化成功回调
            // 会附带用户信息
            // 其它接口必须在回调之后才会生效
            Seeg.OnLoginResult(new LoginOption
            {
                success = (ret) =>
                {
                    ShowMessage($"初始化成功：{JsonUtility.ToJson(ret)}");
                    InitList(font);
                },
                fail = (errCode, errMsg) =>
                {
                    ShowMessage($"初始化失败：errCode={errCode}, errMsg={errMsg}");
                }
            });
        });
    }

    private void InitList(Font font)
    {
        // 功能列表
        var text = new string[] { "广告上报接口", "获取游戏配置", "上报存档数据", "获取存档数据", "获取所在省份", "上报省份排行榜", "获取省份排行榜", "上报个人排行榜", "获取个人排行榜", "上报用户信息", "上报数数事件" };
        var action = new UnityEngine.Events.UnityAction[] { ReportAd, GetGameConfig, SetUserData, GetUserData, GetUserProvince, AddProvinceRank, GetProvinceRank, AddPersonRank, GetPersonRank, ReportUserInfo, ReportSSEvent };
        for (int i = 0; i < text.Length; i++)
        {
            var button = CreateButton(text[i], action[i], font);
            button.transform.SetParent(listView);
        }
    }

    private void ReportAd()
    {
        // 是否完播
        // 只要展示了，就要上报，没播放玩传false
        // 如果展示都没有成功，则不用上报
        ShowMessage("已开始上报，上报类API不返回结果，可以打开日志查看网络请求信息。");
        Seeg.ReportAd(true);
    }

    private void GetGameConfig()
    {
        Seeg.GetGameConfig(new GetGameConfigOption
        {
            success = (ret) =>
            {
                // 可以序列化后拿到对应配置
                ShowMessage($"获取配置成功：{ret}");
            },
            fail = (errCode, errMsg) =>
            {
                ShowMessage($"获取配置失败：errCode={errCode}, errMsg={errMsg}");
            }
        });
    }

    private void SetUserData()
    {
        ShowMessage("已开始上报，上报类API不返回结果，可以打开日志查看网络请求信息。");
        // 仅需上报本次差异的数据，无需全量上报
        // 注意上报频率不要太高，平均每分钟不超过6次
        // 存储的key不要带"."
        var data = new Dictionary<string, object>
        {
            { "level", 1},
            { "prop", "test"}
        };
        Seeg.SetUserData(data);
    }

    private void GetUserData()
    {
        // 获取用户数据，仅获取登录时的数据
        // 游戏过程中改变的数据，请自行变更
        // 再不退出游戏的情况下，后续调用该接口，都是缓存数据
        Seeg.GetUserData(new GetGameDataOption
        {
            success = (ret) =>
            {
                var sb = new StringBuilder();
                sb.Append("获取存档成功>>>");
                foreach (KeyValuePair<string, object> entry in ret)
                {
                    sb.Append($"{entry.Key}={entry.Value}");
                }
                ShowMessage(sb.ToString());
            },
            fail = (errCode, errMsg) =>
            {
                ShowMessage($"获取存档失败：errCode={errCode}, errMsg={errMsg}");
            }
        });
    }

    private void GetUserProvince()
    {
        Seeg.GetUserProvince(new GetProvinceDataOption
        {
            success = (ret) =>
            {
                ShowMessage($"获取所在省份成功：{JsonUtility.ToJson(ret)}");
            },
            fail = (errCode, errMsg) =>
            {
                ShowMessage($"获取所在省份失败：errCode={errCode}, errMsg={errMsg}");
            }
        });
    }

    private void AddProvinceRank()
    {
        Seeg.ChangeProvinceData(new ProvinceRankOption
        {
            type = "test", // 必传，排行榜的唯一标识
            num = 1, // 默认为1
            timeType = TimeType.ALL // 刷新时间默认为永久
        });
    }

    private void GetProvinceRank()
    {
        Seeg.GetProvinceList(new GetProvinceRankOption
        {
            type = "test", // 必须与传入的异议对应
            timeType = TimeType.ALL, // 必须与传入的异议对应
            success = (ret) =>
            {
                // 注意需要自己排序以及补充没有上传的省份
                // 如果缺失省份，容易被平台打回
                var sb = new StringBuilder();
                sb.Append("获取省份排行榜成功>>>");
                for (int i = 0; i < ret.Count; i++)
                {
                    sb.Append($"\nprovince= {ret[i].province}, 上报数= {ret[i].num}");
                }
                ShowMessage(sb.ToString());
            },
            fail = (errCode, errMsg) =>
            {
                ShowMessage($"获取省份排行榜失败：errCode={errCode}, errMsg={errMsg}");
            }
        });
    }

    private void AddPersonRank()
    {
        Seeg.ChangePersonRankData(new RankOption
        {
            type = "test", // 必填
            num = 1, // 默认1
            valueType = 1, // 1累加num 2覆盖上个num 默认覆盖
            timeType = TimeType.WEEK // 可以指定期限，默认周
        });
    }

    private void GetPersonRank()
    {
        Seeg.GetPersonRankList(new GetRankOption
        {
            type = "test", // 必填
            timeType = TimeType.WEEK, // 必须和上报的一致
            success = (ret) =>
            {
                var sb = new StringBuilder();
                sb.Append("获取个人排行榜成功>>>");
                for (int i = 0; i < ret.Count; i++)
                {
                    sb.Append($"\nuid= {ret[i].uid}, 上报数= {ret[i].num}");
                }
                ShowMessage(sb.ToString());
            },
            fail = (errCode, errMsg) =>
            {
                ShowMessage($"获取个人排行榜失败：errCode={errCode}, errMsg={errMsg}");
            }
        });
    }

    private void ReportUserInfo()
    {
        ShowMessage("已开始上报，上报类API不返回结果，可以打开日志查看网络请求信息。");
        Seeg.ReportUserInfo(new UserInfo
        {
            nickname = "张三",
            avatar = "https://test.com/avatar.png"
        });
    }

    private void ReportSSEvent()
    {
        Seeg.SS.ReportEvent("test", new Dictionary<string, object>
        {
            { "key", "val"}
        });
    }

    private void ShowMessage(string msg)
    {
        Debug.Log(msg);
        this.text.text = msg;
    }

    private GameObject CreateButton(string text, UnityEngine.Events.UnityAction onClick, Font font)
    {
        // 创建一个button
        GameObject button = new GameObject(text);
        button.transform.SetParent(listView.transform);
        button.transform.localPosition = new Vector3(0, 0, 0);
        button.transform.localScale = new Vector3(1, 1, 1);
        button.transform.localRotation = new Quaternion(0, 0, 0, 0);

        // 添加RectTransform并设置宽高
        RectTransform rectTransform = button.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 50); // 设置宽度为200，高度为50

        button.AddComponent<Button>();
        button.GetComponent<Button>().onClick.AddListener(onClick);

        // 添加Image组件作为按钮背景
        Image buttonImage = button.AddComponent<Image>();
        buttonImage.color = Color.white;

        // 创建一个Text节点，并挂在到button上
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(button.transform);
        textObject.transform.localPosition = new Vector3(0, 0, 0);
        textObject.transform.localScale = new Vector3(1, 1, 1);
        textObject.transform.localRotation = new Quaternion(0, 0, 0, 0);

        // 添加RectTransform到文本并使其填充按钮
        RectTransform textRectTransform = textObject.AddComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.sizeDelta = Vector2.zero;

        Text textComponent = textObject.AddComponent<Text>();
        textComponent.text = text;
        textComponent.fontSize = 20;
        textComponent.color = Color.black;
        textComponent.alignment = TextAnchor.MiddleCenter;
        // 设置字体，使用Unity默认字体
        textComponent.font = font;
        return button;
    }
}


// 测试或web渠道
// #define PF_WEB
// 微信渠道
// #define PF_WX
// // 头条渠道
// #define PF_TT
// // 快手渠道
// #define PF_KS
// // 快手渠道WASM（WebGL）版本
// #define PF_KS_WASM


using System;
using UnityEngine;
using ZhiSe;



#if PF_KS_WASM
using Assets.Plugins.KwaiGame;
#elif PF_TT
using TTSDK;
#elif PF_WX
using WeChatWASM;
#endif


class Base
{
    public virtual void ShowSideBar() { }
}

internal class AdManager : Base
{
    private AdManager()
    {
        instance = this;
        CreateAd();
    }
    private static AdManager instance;

    public static AdManager Ins
    {
        get
        {
            instance ??= new AdManager();
            return instance;
        }
    }

    public void Init()
    {

    }


    private void CreateAd()
    {
        // 此方法需要宏里面实现具体的创建广告的代码
        _createAd();
    }

    private int _videoBuryDotId;
    private Action<bool> _adAction;

    private void AdSuccess(bool isSuccess)
    {
        // if (isSuccess) {
        //     UserData.Ins.BuryDotVideo(_videoBuryDotId);
        // }
        // UserData.Ins.ReportAd(isSuccess);
#if !UNITY_EDITOR && !PF_WEB
        Seeg.ReportAd(isSuccess);
#endif
        _adAction?.Invoke(isSuccess);
        _adAction = null;
    }

    public void ShowAd(int id, Action<bool> action)
    {
        _videoBuryDotId = id;
        _adAction = action;

        // 此方法需要宏里面实现具体的显示广告的代码并调用 AdSuccess 方法
        _showAd();
    }
#if PF_WEB  || UNITY_EDITOR
    private void _createAd() { }
    private void _showAd()
    {
        Debug.Log("广告");
        AdSuccess(true);
    }
#elif PF_TT
    private TTRewardedVideoAd ad;
    // private string videoId = "27o2sskvc351mk828n";
    private void _createAd()
    {
        string videoId = GameData.adConfig.rewardedVideoIds[0];
        ad = TT.CreateRewardedVideoAd(videoId, (isSuccess, errCode) =>
        {
            Debug.Log("[广告] 创建广告成功");
        }, (errCode, errMsg) =>
        {
            Debug.Log("[广告] 错误码：" + errCode + " 错误信息：" + errMsg);
        });

        if (ad != null)
        {
            ad.OnClose += RewardAdClose;
            ad.OnError += (errCode, errMsg) =>
            {
                AdSuccess(false);
            };
        }
    }
    private void RewardAdClose(bool isEnded, int count)
    {
        Debug.Log("激励广告关闭" + isEnded);
        AdSuccess(isEnded);
    }

    private void _showAd()
    {
        if (ad != null)
        {
            ad.Show();
        }
        else
        {
            Debug.Log("[广告] 没有创建广告实例！");
            _createAd();
        }
    }

#elif PF_KS_WASM
    private RewardVideoAd ad = null;
    private string videoId = "2300009189_01";

    private void _createAd() {
        ad = KS.CreateRewardedVideoAd(videoId);
        if (ad != null) {
            ad.OnClose(new ADCloseResultCallBack((data) => {
                Debug.Log("[激励广告] onClose : " + JsonUtility.ToJson(data));
                AdSuccess(data.isEnded);
            }));
            ad.OnError(new ADShowResultCallBack((data) => {
                Debug.Log("[激励广告] OnError : " + JsonUtility.ToJson(data));
                AdSuccess(false);
            }));
        }
    }
    private void _showAd() {
        if (ad != null) {
            Debug.Log("[激励广告] 调用广告show方法");
            ad.Show();
        } else {
            Debug.Log("[激励广告] : 没有创建广告实例！");
            _createAd();
        }
    }
#elif PF_WX

    private WXRewardedVideoAd rewardedVideoAd;
    private void _createAd() 
    {
        rewardedVideoAd = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam()
        {
            adUnitId = GameData.adConfig.rewardedVideoIds[0],
            multiton = true
        });
        if(rewardedVideoAd != null)
        {
            rewardedVideoAd.OnClose(RewardAdclose);
            rewardedVideoAd.OnError((WXADErrorResponse res)=>{
                AdSuccess(false);
            });
        }
    }

    private void RewardAdclose(WXRewardedVideoAdOnCloseResponse res)
    {
        if(res != null && res.isEnded)
        {
            AdSuccess(true);
        }else
        {   
            AdSuccess(false);
        }
    }

    private void _showAd() 
    { 
        if (rewardedVideoAd != null) {
            Debug.Log("[激励广告] 调用广告show方法");
            rewardedVideoAd.Show();
        } else {
            Debug.Log("[激励广告] : 没有创建广告实例！");
            _createAd();
        }
    }
#endif

}



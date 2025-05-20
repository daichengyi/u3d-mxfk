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
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ZhiSe;

#if PF_WEB || UNITY_EDITOR

#elif PF_WX
using WeChatWASM;
#endif

internal class RankingManager
{
    private RankingManager()
    {
        instance = this;
    }
    private static RankingManager instance;

    public static RankingManager Ins
    {
        get
        {
            instance ??= new RankingManager();
            return instance;
        }
    }

    #region 公共方法

    /// <summary>
    /// 获取世界排行榜数据
    /// </summary>
    /// <param name="count">获取的数量</param>
    /// <param name="callback">回调函数</param>
    public async Task<List<RankData>> GetWorldRanking(int count = 100)
    {
        return await _getWorldRanking(count);
    }

    private Task<List<RankData>> _getWorldRanking(int count)
    {
        // Debug.Log("[排行榜] 获取世界排行榜数据");
        TaskCompletionSource<List<RankData>> tcs = new TaskCompletionSource<List<RankData>>();
        
        Seeg.GetRankList(new GetRankOption()
        {
            type = "world",
            timeType = TimeType.MONTH,
            limit = count,
            success = (ret) =>
            {
                // Debug.Log("排行榜数据获取成功=" + ret);
                tcs.SetResult(ret);
            },
            fail = (errCode, errMsg) =>
            {
                Debug.LogError($"排行榜数据获取失败: {errCode}, {errMsg}");
                tcs.SetException(new Exception($"获取排行榜失败: {errMsg}"));
            }
        });
        
        return tcs.Task;
    }

    /// <summary>
    /// 提交玩家分数到世界排行榜
    /// </summary>
    /// <param name="score">分数</param>
    /// <param name="callback">回调函数</param>
    public void SubmitScore(int score)
    {
        _submitScore(score);
    }

    private void _submitScore(int score,int count = 100)
    {

        // Debug.Log($"[排行榜] 提交分数: {score}");
        Seeg.ChangeMonthRankData(new RankOption()
        {
            type = "world",
            timeType = TimeType.MONTH,
            num = score,
            limit = count
        });
    }

    #endregion
} 
internal class ConfigLevel
{
    public int level;//关卡
    public int nailNum;//钉子总数
    public int[][] colorNum;//颜色数量
    public int[][] noShowColor;//不展示颜色
    public string modelRes;//关卡模型
    public string picRes;//关卡资源
    public float[][] limitScale;//缩放比例
    public int collecttionID;//收藏品id
    public int[] PuzzlesLower;//铺设区间下线
    public int[] PuzzlesHigher;//铺设区间上限
    public int[] interval;//间隔
    public float[] autoScale;//最大缩放
    public float[] mainPosition;//主界面位置
    public float mainScale;//主界面缩放
    public float isAni;//是否结束之后播放动画
    public int getCoin;//获取金币
    public int basicScore;//基础分数
    public float[][] stagePos;//阶段位置
    public float[][] stageRot;  //阶段旋转
    public float[][] stageSca;//阶段缩放
    public int IsSplit;//是否分层
    public int suitSkin;//官推皮肤
    public int[] lvtype;//关卡类型
    public int situationType;//牌面类型
    public string stageReward;//关卡奖励
}

internal class ConfigCommon
{
    public int taskBoxCount;//任务箱子数量
    public int holeCount;//孔位数量
    public int maxHoleCount;//最大孔位数量
    public int[] levelLoop;//关卡循环
    public int fullPopLv;//满盘关卡
    public int clearFullGuideLv;//清除满盘引导关卡
    public int fullGuideNum;//满盘引导次数
    public int clickTipsLv;//点击提示关卡
    public float[] clickTipsTime;//点击提示时间
    public int clickTipsAvgNum;//点击提示平均数量
    public int reviveCleanNum;
    public int[] fullPopLimit;
    public int[] coinMultiple;
    public int isOpenCoin;
    public int isOpenDailyGift;
    public int dailyGiftAutoPop;
    public int isOpenBackMainGift;
    public int backMainGiftMax;
    public int backMainGiftPopMax;
    public int freeLv;
    public int isOpenProfile;
    public int isOpenStageReward;
    public int isOpenWeeklyQuest;
    public int dailyRewardLvTips;
}

internal class ConfigTask
{
    public string detail;//任务说明
}

internal class ConfigSituation
{
    public int Type;//类型
    public int ID;//盘面id
    public int NumLower;//钉子数量下限
    public int NumHigher;//钉子数量上限
    public string hurtId;//增伤逻辑
}

internal class ConfigHurt
{
    public int lackColorNumLower;//缺失颜色的数量下限
    public int lackColorNumHigher;//缺失颜色的数量上限
    public int holeNumLower;//空孔位下限
    public int holeNumHigher;//空孔位上限
    public int outTaskNumLower;//不能上货的订单数下限
    public int outTaskNumHigher;//不能上货的订单数上限
    public int abletoTaskNum;//能上货的订单数
    public int outEmptyNumLower;//上不了货订单空位数下限
    public int outEmptyNumHigher;//上不了货订单空位数上限
    public int abletoNumLower;//上货能上多少钉子数下限
    public int abletoNumHigher;//上货能上多少钉子数上限
    public int comboLower;//连击数下限
    public int comboHigher;//连击数上限
    public int scoreLower;//用户分数下限
    public int scoreHigher;//用户分数上线
    public int reward_num_Lower;//看广告次数下限
    public int reward_num_Higher;//看广告次数上限
    public int Iopen_box_num;//开了几个盒子
    public string taskID;//执行订单id
    public string supplementtaskID;//保底订单ID
}

internal class ConfigScore
{
    public int ID;//分数事件id
    public string name;//名字
    public string detail;//说明
    public int scoreNum;//分值
}

internal class ConfigCol
{
    public int ID;//资源id
    public string name;//资源名字
    public int type;//类型
    public string name_cn;//中文名字
}

internal class ConfigSkin
{
    public int id;//资源id
    public string modelReS;//模型资源
    public int color;//颜色
    public string res;//资源
    public string name;//名字
    public int isDef;//是否默认
    public int needNum;//需要数量
    public int weight;//权重
    public int firstWeight;//首次权重
}

internal class ConfigCoin
{
    public int id;//id
    public string name;//名字
    public int cost;//消耗
}

internal class ConfigGift
{
    public int id;//id
    public int tagType;//类型
    public string name;//名字
    public int getType;//获取类型
    public int param;//参数
    public string reward;//奖励类型
}

internal class ConfigDailyReward
{
    public int day;//天数
    public string name;//奖励类型
    public string img;//图片
    public string reward;//奖励
}

internal class ConfigProfile
{
    public int id;//id
    public string img;//图片
    public int type;//类型
    public int param;//参数
    public int isDef;//是否默认
}

internal class ConfigBackground
{
    public int id;//id
    public string img;//图片
    public int type;//类型
    public int param;//参数
    public int isDef;//是否默认
}

internal class ConfigWeeklyQuest
{
    public int id;//id
    public string img;//图片
    public string des;//描述
    public int type;//类型
    public int param;//参数
    public int getNum;//获取积分数量
}

internal class ConfigActivity
{
    public int id;//id
    public int type;//类型
    public string name;//名称
    public string des;//描述
    public int openLv;//开启关卡
    public int activityTime;//活动时间
    public string itemRes;//关卡物品资源
    public int[] itemNum;//每关数量
    public string rewardGift;//奖励礼包
}

internal class ConfigActivityUnlock
{
    public int weekID;//周活动id
    public string actID;//活动id
    public string name;//活动名称备注
}
using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.manager;
using ZhiSe;

public class UserModel : SingletonBase<UserModel>
{
    // 是否是新用户
    private bool _isNew = true;

    private long _loginTime = 0;

    // 用户ID
    public string userId;

    // 是否是测试号
    public bool isTest = false;

    public string version;

    /// <summary>
    /// 玩家游戏数据
    /// </summary>
    // 当前金币	
    private int _coin = 0;
    public int coin
    {
        get { return _coin; }
        set { _coin = value; SetUserData("coin", _coin); }
    }

    // 当前关卡
    private int _level = 0;
    public int level
    {
        get { return _level; }
        set { _level = 1; SetUserData("level", _level); }
    }

    // 选择的拼图
    private int _selectedPainting = 0;
    public int selectedPainting
    {
        get { return _selectedPainting; }
        set { _selectedPainting = value; SetUserData("selectedPainting", _selectedPainting); }
    }

    // 道具使用次数
    private int _propNum = 1;
    public int propNum
    {
        get { return _propNum; }
        set { _propNum = value; SetUserData("propNum", _propNum); }
    }

    // 皮肤ID
    private int _skinID = 1;
    public int skinID
    {
        get { return _skinID; }
        set { _skinID = value; SetUserData("skinID", _skinID); }
    }

    public void InitData(Dictionary<string, object> ret)
    {
        string key;
        string value;
        foreach (var item in ret)
        {
            key = item.Key.ToString();
            value = item.Value.ToString();
            if (key.Equals("isNew"))
            {
                _isNew = bool.Parse(value);
            }
            else if (key.Equals("loginTime"))
            {
                _loginTime = long.Parse(value);
            }
            else if (key.Equals("coin"))
            {
                _coin = int.Parse(value);
            }
            else if (key.Equals("level"))
            {
                _level = int.Parse(value);
            }
            else if (key.Equals("selectedPainting"))
            {
                _selectedPainting = int.Parse(value);
            }
            else if (key.Equals("skinID"))
            {
                _skinID = int.Parse(value);
            }
        }

        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        long preTime = _loginTime;

        _loginTime = currentTimestamp;

        Dictionary<string, object> data = new Dictionary<string, object>();

        bool isNewDay = _loginTime / 86400 > preTime / 86400;

        if (_isNew)
        {
            _isNew = false;
            data.Add("loginTime", _loginTime);
            data.Add("isNew", _isNew);
        }
        else
        {
            data.Add("loginTime", _loginTime);
        }

        if (isNewDay)
        {

        }
        SetUserData(data);
    }

    private void SetUserData<T>(string key, T value)
    {

        var hashData = new Hashtable();
        Dictionary<string, object> data = new() {
            { key, value }
        };

        hashData = new(){
            { key, value }
        };
        if (!GameData.isLocal)
        {
            Seeg.SetUserData(data);
        }
        DataManager.Instance.SetData(hashData);
    }

    private void SetUserData(Dictionary<string, object> keyValuePairs)
    {
        var data = new Dictionary<string, object>();
        var hashData = new Hashtable();
        foreach (var pair in keyValuePairs)
        {
            data[pair.Key] = pair.Value;
            hashData[pair.Key] = pair.Value;
        }

        if (!GameData.isLocal)
        {
            Seeg.SetUserData(data);
        }
        DataManager.Instance.SetData(hashData);
    }
}
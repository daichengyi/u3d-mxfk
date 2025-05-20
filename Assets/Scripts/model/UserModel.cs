using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ZhiSe;

internal class UserModel
{

	private static UserModel instance;

	public static UserModel Ins
	{
		get
		{
			instance ??= new UserModel();
			return instance;
		}
	}
	// 是否是新用户
	private bool _isNew = true;
	private long _loginTime = 0;
	// 当前金币	
	private int _coin = 0;
	public int coin
	{
		get { return _coin; }
		set { _coin = value; SetUserData("coin", _coin); }
	}

	// 当前关卡
	private int _levelId = 1;
	public int levelId
	{
		get { return _levelId; }
		set { _levelId = value; SetUserData("levelId", _levelId); }
	}
	// 用户ID
	public string userId;
	// 是否是测试号
	public bool isTest = false;
	public string version;
	// 埋点数据
	private Dictionary<string, object> buryDot = new();

	private UserModel()
	{
		instance = this;
	}

	public void InitData(Dictionary<string, object> ret)
	{
		string key;
		string value;
		foreach (var item in ret)
		{
			key = item.Key.ToString();
			value = item.Value.ToString();
			if (key.StartsWith("dot"))
			{
				string newKey = key.Replace("dot_", "");
				if (newKey.StartsWith("levelId_"))
				{
					buryDot[newKey] = JsonConvert.DeserializeObject<Dictionary<string, object>>(value);
				}
			}
			else if (key.StartsWith("user"))
			{
				if (key.Equals("user_isNew"))
				{
					_isNew = bool.Parse(value);
				}
				else if (key.Equals("user_loginTime"))
				{
					_loginTime = long.Parse(value);
				}
				else if (key.Equals("user_coin"))
				{
					_coin = int.Parse(value);
				}
				else if (key.Equals("user_levelId"))
				{
					_levelId = int.Parse(value);
				}
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
		key = "user_" + key;
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
			data["user_" + pair.Key] = pair.Value;
			hashData["user_" + pair.Key] = pair.Value;
		}

		if (!GameData.isLocal)
		{
			Seeg.SetUserData(data);
		}
		DataManager.Instance.SetData(hashData);
	}

	private void SetDotData(Dictionary<string, object> keyValuePairs)
	{
		var data = new Dictionary<string, object>();
		var hashData = new Hashtable();
		foreach (var pair in keyValuePairs)
		{
			data["dot_" + pair.Key] = pair.Value;
			hashData["dot_" + pair.Key] = pair.Value;
		}
		if (!GameData.isLocal)
		{
			Seeg.SetUserData(data);
		}
		DataManager.Instance.SetData(hashData);
	}
}
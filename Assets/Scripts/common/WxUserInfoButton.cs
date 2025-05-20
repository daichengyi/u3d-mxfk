#if PF_WX
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WeChatWASM;
using ZhiSe;


public class WxUserInfoButton : MonoBehaviour
{
    // Start is called before the first frame update


    private WXUserInfoButton _userInfoButton;
    public UnityEvent onClickEvent = new UnityEvent();
    // Rect screenRect;
    void Start()
    {
        // screenRect = initRect();
        // Rect rect = GetUIScreenRect();
        // Debug.Log("--屏幕坐标和尺寸--" + rect);
        // CreateImageNodeAtScreenPosition(new Vector2(rect.x, rect.y), new Vector2(rect.width, rect.height));
        // Debug.Log("screenRect:" + screenRect);
        // Debug.Log("-----postion:" + GetInitialButtonPosition());
        Button _button = transform.GetComponent<Button>();
        _button.onClick.AddListener(onClick);
#if !UNITY_EDITOR
        if (string.IsNullOrEmpty(Load.loginData.nickname) && _userInfoButton == null)
        {
            Invoke(nameof(createUserInfoButton), 1f);
        }
#endif
    }

    /// <summary>
    /// 根据屏幕坐标在Canvas下创建带有Image组件的节点
    /// </summary>
    /// <param name="screenPosition">屏幕坐标</param>
    /// <param name="size">节点大小</param>
    /// <param name="spriteName">图片资源名称，可选</param>
    /// <returns>创建的GameObject</returns>
    public GameObject CreateImageNodeAtScreenPosition(Vector2 screenPosition, Vector2 size, string spriteName = null)
    {
        // 获取Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("场景中未找到Canvas组件！");
            return null;
        }

        // 创建新的GameObject
        GameObject imageNode = new GameObject("ImageNode");
        imageNode.transform.SetParent(canvas.transform, false);

        // 添加RectTransform组件
        RectTransform rectTransform = imageNode.AddComponent<RectTransform>();
        rectTransform.sizeDelta = size;

        // 添加Image组件
        Image image = imageNode.AddComponent<Image>();

        // 如果提供了精灵名称，尝试加载精灵
        if (!string.IsNullOrEmpty(spriteName))
        {
            Sprite sprite = Resources.Load<Sprite>(spriteName);
            if (sprite != null)
            {
                image.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"未能加载精灵资源: {spriteName}");
            }
        }

        // 将屏幕坐标转换为Canvas中的位置
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPosition
        );

        rectTransform.anchoredPosition = localPosition;

        return imageNode;
    }


    Rect initRect()
    {
        RectTransform rectTransform = transform.GetComponent<RectTransform>();
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);
        Rect screenRect = new Rect(worldCorners[0].x, worldCorners[0].y, worldCorners[2].x - worldCorners[0].x, worldCorners[2].y - worldCorners[0].y);
        return screenRect;
    }


    // 获取初始按钮的长宽
    Vector2 GetInitialButtonSize()
    {
        return transform.GetComponent<RectTransform>().sizeDelta;
    }

    Vector2 GetInitialButtonPosition()
    {
        var button = transform;
        var canvas = button.GetComponentInParent<Canvas>();

        // 获取 Canvas 和按钮的 RectTransform 组件
        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
        RectTransform buttonRectTransform = button.GetComponent<RectTransform>();

        // 计算 Canvas 左上角的局部位置
        Vector2 canvasTopLeftLocalPosition = new Vector2(
            -canvasRectTransform.rect.width * canvasRectTransform.pivot.x,
            canvasRectTransform.rect.height * (1 - canvasRectTransform.pivot.y)
        );

        // 计算按钮左上角的局部位置
        Vector2 buttonTopLeftLocalPosition = new Vector2(
            buttonRectTransform.anchoredPosition.x
                - buttonRectTransform.rect.width * buttonRectTransform.pivot.x,
            buttonRectTransform.anchoredPosition.y
                + buttonRectTransform.rect.height * (1 - buttonRectTransform.pivot.y)
        );

        // 将 Canvas 和按钮左上角的局部位置转换为世界坐标系中的位置
        Vector3 canvasTopLeftWorldPosition = canvasRectTransform.TransformPoint(
            canvasTopLeftLocalPosition
        );
        Vector3 buttonTopLeftWorldPosition = buttonRectTransform.TransformPoint(
            buttonTopLeftLocalPosition
        );

        var x = canvasTopLeftWorldPosition.x - buttonTopLeftWorldPosition.x;
        var y = canvasTopLeftWorldPosition.y - buttonTopLeftWorldPosition.y;
        return new Vector2(x, y);
    }
    void getUserInfo()
    {
        if (!string.IsNullOrEmpty(Load.loginData.nickname))
        {
            onClickEvent?.Invoke();
            return;
        }
        WX.GetSetting(new GetSettingOption()
        {
            success = (GetSettingSuccessCallbackResult res) =>
            {
                Debug.Log("获取用户信息成功，更新用户信息");
                foreach (var setting in res.authSetting)
                {
                    Debug.Log($"权限：{setting.Key}，状态：{setting.Value}");
                }
                if (res.authSetting.ContainsKey("scope.userInfo") && res.authSetting["scope.userInfo"])
                {
                    WX.GetUserInfo(new GetUserInfoOption
                    {
                        success = (res) =>
                        {
                            Load.loginData.nickname = res.userInfo.nickName;
                            Load.loginData.avatar = res.userInfo.avatarUrl;
                            onClickEvent?.Invoke();
                        }
                    });
                }
                else
                {
                    WX.RequirePrivacyAuthorize(new RequirePrivacyAuthorizeOption()
                    {
                        success = (res) =>
                        {
                            Debug.Log("RequirePrivacyAuthorize获取用户信息权限成功");
                            var (x, y, width, height) = GetWeChatUIScreenRect();
                            WXUserInfoButton btn = WX.CreateUserInfoButton(
                                x,
                                y,
                                width,
                                height,
                                "zh_CN",
                                false
                            );
                            btn.OnTap(onTap);
                        },
                        fail = (res) =>
                        {
                            Debug.Log("RequirePrivacyAuthorize获取用户信息权限失败");
                            onClickEvent?.Invoke();
                        }
                    });
                }
            }
        });
    }

    void onTap(WXUserInfoResponse res)
    {
        Debug.Log("点击了用户信息按钮");
        if (res.errCode == 0)
        {
            Debug.Log("获取用户信息成功，更新用户信息");
            Load.loginData.nickname = res.userInfo.nickName;
            Load.loginData.avatar = res.userInfo.avatarUrl;
            Seeg.ReportUserInfo(new ZhiSe.UserInfo()
            {
                nickname = res.userInfo.nickName,
                avatar = res.userInfo.avatarUrl,
                gender = res.userInfo.gender
            });
        }
        _userInfoButton?.Hide();
        onClickEvent?.Invoke();
    }


    void createUserInfoButton()
    {

        var (x, y, width, height) = GetWeChatUIScreenRect();
        Debug.Log($"WXUserInfoButton 位置和大小 - X: {x}, Y: {y}, 宽度: {width}, 高度: {height}");
        _userInfoButton = WX.CreateUserInfoButton(
            x,
            y,
            width,
            height,
            "zh_CN",
            false
        );
        Debug.Log("创建用户信息按钮");
        _userInfoButton.OnTap(onTap);
    }


    /// <summary>
    /// 获取UI元素的屏幕坐标和尺寸
    /// </summary>
    /// <returns>返回包含位置和尺寸的Rect</returns>
    public Rect GetUIScreenRect()
    {
        // 获取Canvas
        Canvas canvas = transform.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("未找到Canvas组件！");
            return Rect.zero;
        }

        // 获取UI元素的四个角坐标
        Vector3[] corners = new Vector3[4];
        transform.GetComponent<RectTransform>().GetWorldCorners(corners);

        // 转换所有角点到屏幕坐标
        Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[0]);
        Vector2 topLeft = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[1]);
        Vector2 topRight = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[2]);
        Vector2 bottomRight = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[3]);

        // 计算尺寸
        float width = Mathf.Abs(topRight.x - topLeft.x);
        float height = Mathf.Abs(topLeft.y - bottomLeft.y);

        float x = topLeft.x;
        float y = Screen.height - topLeft.y;

        Debug.Log("--未转换之前的屏幕坐标和尺寸--" + new Rect(x, y, width, height));
        return new Rect(x, y, width, height);
    }

    /// <summary>
    /// 获取UI元素在微信小游戏中的屏幕坐标和尺寸（考虑像素比）
    /// </summary>
    /// <param name="uiElement">要计算的UI元素</param>
    /// <returns>返回微信小游戏中使用的屏幕坐标和尺寸</returns>
    public (int x, int y, int width, int height) GetWeChatUIScreenRect()
    {
        // 获取系统信息
        var systemInfo = WX.GetSystemInfoSync();
        float pixelRatio = (float)systemInfo.pixelRatio;

        // 获取基础屏幕矩形
        Rect screenRect = GetUIScreenRect();

        // 直接转换，因为都是基于左上角的坐标系
        int x = Mathf.RoundToInt(screenRect.x * pixelRatio - screenRect.width / 2);
        int y = Mathf.RoundToInt(screenRect.y);
        int width = Mathf.RoundToInt(screenRect.width);
        int height = Mathf.RoundToInt(screenRect.height);

        return (x, y, width, height);
    }

    // 使用示例
    public void CreateWeChatButton()
    {
        var (x, y, width, height) = GetWeChatUIScreenRect();
        // 创建微信按钮
        var button = WX.CreateUserInfoButton(
            x,
            y,
            width,
            height,
            "zh_CN",
            false
        );
    }
    void onClick()
    {
#if UNITY_EDITOR
        onClickEvent?.Invoke();
#else
        getUserInfo();
#endif
    }

    void OnEnable()
    {
        _userInfoButton?.Show();
    }
    void OnDestroy()
    {
        _userInfoButton?.Hide();
        _userInfoButton?.Destroy();
    }

    void OnDisable()
    {
        _userInfoButton?.Hide();
    }

}

public class UIScreenPositionCalculator : MonoBehaviour
{

}
#endif
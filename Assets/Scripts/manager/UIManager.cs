using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UIManager();
            }
            return _instance;
        }
    }

    public Transform view;
    public Transform dialog;
    public Transform tips;
    public Transform top;
    public GameObject maskBg;
    public GameObject canvas;

    private GameObject loadWindow;
    private bool isCloseError = false;
    private bool bLoadCommon = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void InitNode()
    {
        canvas = GameObject.Find("Canvas");
        view = GameObject.Find("Canvas/ui/view").transform;
        dialog = GameObject.Find("Canvas/ui/dialog").transform;
        tips = GameObject.Find("Canvas/ui/tips").transform;
        top = GameObject.Find("Canvas/ui/top").transform;
        maskBg = GameObject.Find("Canvas/maskBg");

        view.position = Vector3.zero;
        dialog.position = Vector3.zero;
        tips.position = Vector3.zero;
        top.position = Vector3.zero;
    }

    private void OnDisable()
    {
        // 清理资源
    }

    private void OnDestroy()
    {
        // 清理资源
    }

    /// <summary>
    /// 打开界面
    /// </summary>
    /// <param name="name">界面名字 UIConst.UI_HOME</param>
    /// <param name="type">1是view节点,2是dialog节点,3是tips,4是top</param>
    /// <param name="closeOther">是否关闭节点下其他界面</param>
    /// <param name="callback">回调函数</param>
    /// <param name="parameters">传递的参数</param>
    /// <param name="onlyOne">是否可同时存在多个</param>
    public async Task<GameObject> OpenView(string name, int type = 1, bool closeOther = false, System.Action<GameObject> callback = null, object parameters = null, bool onlyOne = false)
    {
        if (type != VIEW_TYPE.tips && type != VIEW_TYPE.top)
        {
            ShowLoading();
        }

        Transform parent = null;
        switch (type)
        {
            case VIEW_TYPE.view:
                parent = view;
                break;
            case VIEW_TYPE.dialog:
                parent = dialog;
                break;
            case VIEW_TYPE.tips:
                parent = tips;
                break;
            case VIEW_TYPE.top:
                parent = top;
                break;
        }

        if (closeOther)
        {
            foreach (Transform child in parent)
            {
                child.gameObject.SetActive(false);
            }

            foreach (Transform child in dialog)
            {
                child.gameObject.SetActive(false);
            }
        }

        GameObject node = null;
        string sceneName = name;

        if (onlyOne)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform element = parent.GetChild(i);
                if (!element.gameObject.activeSelf && element.name == sceneName)
                {
                    node = element.gameObject;
                    break;
                }
            }
        }
        else
        {
            Transform child = parent.Find(sceneName);
            if (child != null)
            {
                node = child.gameObject;
            }
        }

        if (node == null)
        {
            GameObject prefab = await AsyncLoadView(sceneName);
            node = Instantiate(prefab, parent);
            node.name = sceneName;
        }
        node.SetActive(true);
        int index = parent.childCount - 1;
        index = index < 0 ? 0 : index;
        node.transform.SetSiblingIndex(index);
        HideLoading();
        callback?.Invoke(node);

        BaseView script = node.GetComponent<BaseView>();
        if (script != null)
        {
            script.onShow(parameters);
        }
        return node;
    }

    public void CloseView(string name, bool destroy = false)
    {
        List<GameObject> nodeList = new List<GameObject>();
        name = name.Split('_')[0];
        foreach (Transform element in view)
        {
            if (name == element.name)
            {
                nodeList.Add(element.gameObject);
            }
        }

        foreach (Transform element in dialog)
        {
            if (name == element.name)
            {
                nodeList.Add(element.gameObject);
            }
        }

        foreach (Transform element in tips)
        {
            if (name == element.name)
            {
                nodeList.Add(element.gameObject);
            }
        }

        foreach (Transform element in top)
        {
            if (name == element.name)
            {
                nodeList.Add(element.gameObject);
            }
        }

        if (destroy)
        {
            for (int index = nodeList.Count - 1; index >= 0; index--)
            {
                Destroy(nodeList[index]);
            }
        }
        else
        {
            foreach (GameObject node in nodeList)
            {
                node.SetActive(false);
            }
        }
    }


    /// <summary>
    /// 异步加载预制体
    /// </summary>
    /// <param name="viewName">预制体名称</param>
    /// <param name="callback">回调函数</param>
    /// <returns>预制体</returns>
    public async Task<GameObject> AsyncLoadView(string viewName, System.Action<GameObject> callback = null)
    {
        try
        {
            string fullPath = "Assets/uiPrefab/" + viewName + ".prefab";
            var handle = ResourceManager.LoadAsset<GameObject>(fullPath);
            while (!handle.IsDone)
            {
                await Task.Yield();
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var result = handle.Result;
                callback?.Invoke(result);
                return result;
            }
            else
            {
                Debug.LogError($"加载预制体失败: {viewName}, 状态: {handle.Status}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"加载预制体时发生异常: {viewName}, 错误: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }


    /// <summary>
    /// 异步对场景进行加载
    /// </summary>
    /// <param name="secneName"></param>
    /// <param name="callback"></param>
    /// <param name="updatePercentage"></param>
    public async Task AsyncLoadScene(string sceneName, System.Action callback = null)
    {
        var handle = ResourceManager.LoadScene(sceneName);
        while (!handle.IsDone)
        {
            await Task.Yield();
        }
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            callback?.Invoke();
        }
        else
        {
            Debug.LogError($"场景加载失败: {sceneName}");
        }
    }

    /// <summary>
    /// 加载bundle
    /// </summary>
    // public async Task<AssetBundle> LoadBundle(string name)
    // {
    //     // 这里需要根据Unity的资源加载系统进行调整
    //     // 可以使用Addressables或AssetBundle
    //     AssetBundle bundle = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(b => b.name == name);

    //     if (bundle != null)
    //     {
    //         return bundle;
    //     }

    //     // 异步加载AssetBundle
    //     AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + name);
    //     await request.completed;

    //     if (name == "common")
    //     {
    //         bLoadCommon = true;
    //     }

    //     return request.assetBundle;
    // }

    // public async Task<GameObject> LoadView(AssetBundle bundle, string name)
    // {
    //     AssetBundleRequest request = bundle.LoadAssetAsync<GameObject>(name);
    //     await request.completed;
    //     return request.asset as GameObject;
    // }

    /// <summary>
    /// 切换场景时，清理掉所有UI
    /// </summary>
    public void CleanAllView()
    {
        List<GameObject> nodeList = new List<GameObject>();

        foreach (Transform element in view)
        {
            nodeList.Add(element.gameObject);
        }

        foreach (Transform element in dialog)
        {
            nodeList.Add(element.gameObject);
        }

        foreach (Transform element in tips)
        {
            nodeList.Add(element.gameObject);
        }

        foreach (Transform element in top)
        {
            nodeList.Add(element.gameObject);
        }

        for (int index = nodeList.Count - 1; index >= 0; index--)
        {
            Destroy(nodeList[index]);
        }
    }

    /// <summary>
    /// 预加载界面
    /// </summary>
    public async Task PreloadView()
    {
        List<string> prefabList;
        prefabList = new List<string> { };
        for (int i = 0; i < prefabList.Count; i++)
        {
            string viewName = prefabList[i];
            await AsyncLoadView(viewName);
            Debug.Log("闲时加载界面完毕:" + viewName);
        }
    }

    /// <summary>
    /// 提示消息
    /// </summary>
    public void ShowMsg(string desc, bool withAudio = false)
    {
        _ = OpenView(VIEW_NAME.Tip, 3, false, null, desc, true);
    }

    /// <summary>
    /// 是否失控状态（不可操作）
    /// </summary>
    public void OutControl(bool state = true)
    {
        maskBg.SetActive(state);
    }

    public async Task InitLoading()
    {
        GameObject obj = await AsyncLoadView(VIEW_NAME.LoadWindow);
        loadWindow = Instantiate(obj, top);
        loadWindow.SetActive(false);
    }

    public void ShowLoading()
    {
        if (loadWindow == null)
        {
            _ = OpenView(VIEW_NAME.LoadWindow, 4, false, (node) =>
            {
                loadWindow = node;
                if (isCloseError)
                {
                    isCloseError = false;
                    loadWindow.SetActive(false);
                }
            });
        }
        else
        {
            loadWindow.SetActive(true);
        }
    }

    public void HideLoading()
    {
        if (loadWindow != null)
        {
            isCloseError = false;
            loadWindow.SetActive(false);
        }
        else
        {
            isCloseError = true;
        }
    }
}

// 需要添加的辅助类
public static class VIEW_TYPE
{
    public const int view = 1;
    public const int dialog = 2;
    public const int tips = 3;
    public const int top = 4;
}

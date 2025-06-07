using Assets.Game.Scripts;
using Assets.Scripts.common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class HomeScene : MonoBehaviour
{

    private Dictionary<string, GameObject> _preloadedPrefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, AsyncOperationHandle<GameObject>> _loadingHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();
    private Dictionary<string, List<string>> _folderContents = new Dictionary<string, List<string>>();

    void Start()
    {
        UIManager.Instance.HideLoading();
        SoundManager.Ins.PlayMusic("bgm");
    }

    /// <summary>
    /// 预加载文件夹中的所有预制件
    /// </summary>
    public async Task<bool> PreloadFolderAsync()
    {
        try
        {
            // 获取所有资源位置
            var locationsHandle = Addressables.LoadResourceLocationsAsync("All");
            await locationsHandle.Task;

            if (locationsHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("无法获取资源位置");
                return false;
            }

            var locations = locationsHandle.Result;

            // 按类型分组加载
            var groups = locations.GroupBy(l => l.ResourceType);
            foreach (var group in groups)
            {
                string typeName = group.Key.Name;

                foreach (var location in group)
                {
                    string address = location.PrimaryKey;
                    var handle = Addressables.LoadAssetAsync<object>(address);

                    // 等待加载完成
                    await handle.Task;

                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {

                        Debug.Log($"资源加载成功: {address}");
                    }
                    else
                    {
                        Debug.LogError($"资源加载失败: {address}");
                    }

                    // 释放句柄
                    Addressables.Release(handle);
                    Debug.LogError($"释放句柄:====");
                }
            }

            // 释放位置句柄
            Addressables.Release(locationsHandle);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"预加载过程中发生错误: {e.Message}");
            return false;
        }
    }

    public void onBtnGame()
    {
        UIManager.Instance.ShowLoading();
       GameManager.Instance.EnterMode(GameMode.Feibiao, true);
    }

    public void onBtnSet()
    {
        Addressables.LoadAssetAsync<object>("Assets/Game/Levels/json/lv_0.json");
        Addressables.LoadAssetAsync<object>("Assets/Game/Levels/prefabs/lv_0.prefab");
        //UIManager.Instance.OpenView(VIEW_NAME.SetttingDlg, VIEW_TYPE.dialog);
    }
}

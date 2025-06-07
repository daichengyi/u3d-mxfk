using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

public class ResourceManager
{
    private static bool isInitialized = false;

    // JSON缓存字典
    private static readonly Dictionary<string, (JObject data, DateTime cacheTime)> _jsonCache = new Dictionary<string, (JObject, DateTime)>();
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5); // 缓存时间5分钟

    // 当前关卡加载的资源引用
    private static readonly List<AsyncOperationHandle> _currentLevelHandles = new List<AsyncOperationHandle>();
    private static readonly Dictionary<string, AsyncOperationHandle> _loadedAssets = new Dictionary<string, AsyncOperationHandle>();

    public static async void Initialize(System.Action onComplete = null)
    {
        if (isInitialized)
        {
            onComplete?.Invoke();
            return;
        }

        try
        {
            var initOp = Addressables.InitializeAsync();
            await initOp.Task;
            isInitialized = true;
            onComplete?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Addressables初始化失败: {e.Message}");
        }
    }

    /// <summary>
    /// 加载场景资源
    /// </summary>
    /// <param name="assetAddress">场景资源地址</param>
    /// <param name="callback">加载完成后的回调函数</param>
    /// <returns>异步操作句柄</returns>
    public static AsyncOperationHandle<SceneInstance> LoadScene(string assetAddress, System.Action<SceneInstance> callback = null)
    {
        string fullPath = "Assets/Scene/" + assetAddress + ".unity";
        var handle = Addressables.LoadSceneAsync(fullPath);
        handle.Completed += (operation) =>
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                callback?.Invoke(operation.Result);
            }
            else
            {
                Debug.LogError($"加载场景失败: {fullPath}");
            }
        };
        return handle;
    }

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <param name="assetPath">资源路径</param>
    /// <param name="result">返回数据</param>
    /// <typeparam name="T">类型</typeparam>
    public static AsyncOperationHandle<T> LoadAsset<T>(string assetPath, System.Action<T> result = null, System.Action fail = null)
    {
        // 检查是否已加载该资源
        if (_loadedAssets.TryGetValue(assetPath, out var existingHandle))
        {
            try
            {
                // 因为类型转换问题，我们直接重新加载资源但不增加引用计数
                var loadOp = Addressables.LoadAssetAsync<T>(assetPath);
                loadOp.Completed += (handle) =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        result?.Invoke(handle.Result);
                    }
                    else
                    {
                        fail?.Invoke();
                    }
                };
                return loadOp;
            }
            catch (Exception e)
            {
                Debug.LogError($"复用已加载资源失败: {assetPath}, {e.Message}");
            }
        }

        var newLoadOp = Addressables.LoadAssetAsync<T>(assetPath);
        try
        {
            newLoadOp.Completed += (handle) =>
            {
                try
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                    {
                        // 存储加载的资源引用
                        _loadedAssets[assetPath] = handle;
                        // if (assetPath.Contains("CH/"))
                        // {
                        //     ConfigLevel config = ConfigManager.Ins.GetConfigByid<ConfigLevel>(UserModel.Ins.levelId);
                        // }
                        // 添加到当前关卡资源列表

                        result?.Invoke(handle.Result);
                    }
                    else
                    {
                        Debug.LogError($"加载资源失败: {assetPath}, Status: {handle.Status}");
                        fail?.Invoke();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"加载资源回调异常: {assetPath}, {e.Message}");
                    fail?.Invoke();
                }
            };
        }
        catch (System.Exception e)
        {
            Debug.LogError($"初始化加载异常: {assetPath}, {e.Message}");
            fail?.Invoke();
        }
        return newLoadOp;
    }

    /// <summary>
    /// 异步加载Json文件并返回解析后的数据
    /// </summary>
    /// <param name="jsonPath">Json文件在Addressable中的地址</param>
    /// <param name="useCache">是否使用缓存</param>
    /// <param name="forceRefresh">强制刷新缓存</param>
    /// <returns>解析后的JObject对象,加载失败返回null</returns>
    public static async Task<JObject> LoadJsonAsync(string jsonPath, bool useCache = true, bool forceRefresh = false)
    {
        try
        {
            // 检查缓存
            if (useCache && !forceRefresh && _jsonCache.TryGetValue(jsonPath, out var cachedData))
            {
                if (DateTime.Now - cachedData.cacheTime <= _cacheDuration)
                {
                    return cachedData.data;
                }
                _jsonCache.Remove(jsonPath); // 移除过期缓存
            }

            var handle = LoadAsset<TextAsset>(jsonPath);
            try
            {
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    var jsonText = handle.Result.text;
                    var jsonData = JObject.Parse(jsonText);

                    // 存入缓存
                    if (useCache)
                    {
                        _jsonCache[jsonPath] = (jsonData, DateTime.Now);
                    }

                    return jsonData;
                }

                Debug.LogError($"加载Json文件失败: {jsonPath}, Status: {handle.Status}");
                return null;
            }
            finally
            {
                // 释放资源
                Addressables.Release(handle);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载或解析Json文件失败: {jsonPath}, 错误: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// 清除JSON缓存
    /// </summary>
    /// <param name="jsonPath">指定要清除的JSON文件路径，为null时清除所有缓存</param>
    public static void ClearJsonCache(string jsonPath = null)
    {
        if (string.IsNullOrEmpty(jsonPath))
        {
            _jsonCache.Clear();
            return;
        }
        _jsonCache.Remove(jsonPath);
    }

    /// <summary>
    /// 获取泛型类型的JSON数据
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="jsonPath">JSON文件路径</param>
    /// <param name="useCache">是否使用缓存</param>
    /// <returns>转换后的对象，失败返回default(T)</returns>
    public static async Task<T> LoadJsonAsync<T>(string jsonPath, bool useCache = true)
    {
        try
        {
            var jsonData = await LoadJsonAsync(jsonPath, useCache);
            if (jsonData != null)
            {
                return jsonData.ToObject<T>();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON转换到类型 {typeof(T)} 失败: {jsonPath}, 错误: {e.Message}");
        }
        return default;
    }

    /// <summary>
    /// 预加载JSON文件到缓存
    /// </summary>
    /// <param name="jsonPaths">JSON文件路径列表</param>
    public static async Task PreloadJsonFiles(IEnumerable<string> jsonPaths)
    {
        foreach (var path in jsonPaths)
        {
            await LoadJsonAsync(path, true, true);
        }
    }

    /// <summary>
    /// 释放当前关卡的资源
    /// </summary>
    /// <param name="clearAll">是否清理所有加载的资源，默认为false只清理当前关卡资源</param>
    /// <returns>释放的资源数量</returns>
    public static int ReleaseCurrentLevelResources(bool clearAll = false)
    {
        int releasedCount = 0;

        try
        {
            Debug.Log($"开始释放关卡资源，当前关卡资源数量: {_currentLevelHandles.Count}");

            // 清理当前关卡资源
            foreach (var handle in _currentLevelHandles)
            {
                if (handle.IsValid())
                {
                    Debug.Log("释放资源" + (handle.Result as GameObject).name);
                    Addressables.Release(handle);
                    releasedCount++;
                }
            }

            // 从全局资源字典中移除对应的项
            List<string> keysToRemove = new List<string>();
            foreach (var pair in _loadedAssets)
            {
                if (_currentLevelHandles.Contains(pair.Value))
                {
                    keysToRemove.Add(pair.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _loadedAssets.Remove(key);
            }

            _currentLevelHandles.Clear();

            // 如果需要清理所有资源
            if (clearAll)
            {
                foreach (var handle in _loadedAssets.Values)
                {
                    if (handle.IsValid())
                    {
                        Addressables.Release(handle);
                        releasedCount++;
                    }
                }
                _loadedAssets.Clear();
            }

            // 强制进行垃圾回收
            // Resources.UnloadUnusedAssets();
            GC.Collect();

            Debug.Log($"关卡资源释放完成，共释放 {releasedCount} 个资源");
        }
        catch (Exception ex)
        {
            Debug.LogError($"释放关卡资源时发生异常: {ex.Message}\n{ex.StackTrace}");
        }

        return releasedCount;
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="viewName">预制体名称</param>
    /// <param name="callback">回调函数</param>
    /// <returns>预制体</returns>
    public static async Task<T> AsyncLoadRes<T>(string path, System.Action<T> callback = null)
    {
        try
        {
            string fullPath = "Assets/" + path;
            var handle = ResourceManager.LoadAsset<T>(fullPath);
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
                Debug.LogError($"加载资源失败: {path}, 状态: {handle.Status}");
                return default(T);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"加载资源时发生异常: {path}, 错误: {ex.Message}\n{ex.StackTrace}");
            return default(T);
        }
    }

    public static async Task<int> PreloadLevels()
    {
        // 获取所有资源位置
        var locationsHandle = Addressables.LoadResourceLocationsAsync("levels");
        await locationsHandle.Task;

        if (locationsHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("无法获取资源位置");
            return 0;
        }
        else
        {
            return locationsHandle.Result.Count / 2;
        }
    }

    /// <summary>
    /// 预加载文件夹中的所有预制件
    /// </summary>
    public static async Task<bool> PreloadFolderAsync()
    {
        try
        {
            // 获取所有资源位置
            var locationsHandle = Addressables.LoadResourceLocationsAsync("levels");
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
                    //Addressables.Release(handle);
                    //Debug.LogError($"释放句柄:====");
                }
            }

            // 释放位置句柄
            //Addressables.Release(locationsHandle);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"预加载过程中发生错误: {e.Message}");
            return false;
        }
    }
}

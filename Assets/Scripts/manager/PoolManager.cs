using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag; // 对象池的标识符
        public GameObject prefab; // 预制体
        public int initialPoolSize; // 初始池大小
    }

    private List<Pool> pools; // 存储对象池配置
    public Dictionary<string, List<GameObject>> poolDictionary; // 存储所有对象池的字典

    // 单例实例
    private static PoolManager _instance;
    public static PoolManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PoolManager();
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        pools = new List<Pool>();
        poolDictionary = new Dictionary<string, List<GameObject>>();
        DontDestroyOnLoad(gameObject);
    }

    // 设置对象池配置
    public void SetPools(List<Pool> poolConfigs)
    {
        if (poolConfigs == null || poolConfigs.Count == 0)
        {
            // Debug.Log("Pool configurations are invalid.");
            return;
        }

        pools = poolConfigs;
        // Debug.Log("Pool configurations set successfully.");
    }

    // 添加对象池配置
    public void AddPool(string tag, GameObject prefab, int initialPoolSize)
    {
        if (poolDictionary.ContainsKey(tag))
        {
            Pool pool = pools.Find(p => p.tag == tag);
            pool.prefab = prefab;
            // Debug.Log($"Pool with tag {tag} already exists.");
            return;
        }

        Pool newPool = new Pool
        {
            tag = tag,
            prefab = prefab,
            initialPoolSize = initialPoolSize
        };

        pools.Add(newPool);

        // 初始化新对象池
        List<GameObject> objectPool = new List<GameObject>();
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            objectPool.Add(obj);
            if (tag == "xingxing")
            {
                void OnAnimationComplete(Spine.TrackEntry trackEntry)
                {
                    ReturnObject("xingxing", obj);
                }
                obj.GetComponent<SkeletonGraphic>().AnimationState.Complete += OnAnimationComplete;
            }
        }

        poolDictionary.Add(tag, objectPool);
        // Debug.Log($"Pool with tag {tag} added successfully.");
    }

    // 初始化对象池
    public void InitializePools()
    {
        if (pools == null || pools.Count == 0)
        {
            // Debug.Log("Pool configurations are not set.");
            return;
        }

        if (poolDictionary.Count > 0)
        {
            // Debug.Log("Object pools have already been initialized.");
            return;
        }

        // 初始化所有对象池
        foreach (Pool pool in pools)
        {
            List<GameObject> objectPool = new List<GameObject>();

            for (int i = 0; i < pool.initialPoolSize; i++)
            {
                GameObject obj = Object.Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Add(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }

        // Debug.Log("Object pools initialized successfully.");
    }

    // 从对象池中获取对象
    public GameObject GetObject(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            // Debug.Log($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        if (poolDictionary[tag].Count > 0)
        {
            GameObject obj = poolDictionary[tag][0];
            poolDictionary[tag].RemoveAt(0);
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // 如果池中没有对象，创建一个新的对象
            Pool pool = pools.Find(p => p.tag == tag);
            if (pool != null)
            {
                GameObject obj = Object.Instantiate(pool.prefab);
                if (tag == "xingxing")
                {
                    void OnAnimationComplete(Spine.TrackEntry trackEntry)
                    {
                        ReturnObject("xingxing", obj);
                    }
                    obj.GetComponent<SkeletonGraphic>().AnimationState.Complete += OnAnimationComplete;
                }
                obj.SetActive(true);
                return obj;
            }
            else
            {
                // Debug.Log($"Prefab for tag {tag} not found.");
                return null;
            }
        }
    }

    // 将对象放回对象池
    public void ReturnObject(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            // Debug.Log($"Pool with tag {tag} doesn't exist.");
            return;
        }
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        poolDictionary[tag].Add(obj);
    }

    // 清理对象池
    public void ClearPools()
    {
        if (poolDictionary == null || poolDictionary.Count == 0)
        {
            // Debug.Log("Object pools are not initialized.");
            return;
        }

        foreach (var pool in poolDictionary.Values)
        {
            while (pool.Count > 0)
            {
                GameObject obj = pool[0];
                Object.Destroy(obj);
                pool.RemoveAt(0);
            }
        }

        poolDictionary.Clear();
        pools.Clear();
        // Debug.Log("Object pools cleared.");
    }
}
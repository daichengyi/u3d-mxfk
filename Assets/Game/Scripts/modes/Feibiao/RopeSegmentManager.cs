using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class RopeSegmentManager
    {
        private static RopeSegmentManager instance;
        private Dictionary<int, Queue<GameObject>> segmentPools = new Dictionary<int, Queue<GameObject>>();

        public static RopeSegmentManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RopeSegmentManager();
                }
                return instance;
            }
        }

        public GameObject GetSegment(int type)
        {
            if (!segmentPools.ContainsKey(type))
            {
                segmentPools[type] = new Queue<GameObject>();
            }

            var pool = segmentPools[type];

            if (pool.Count <= 0)
            {
                GameObject segment = new GameObject("RopeSegment");
                SpriteRenderer sprite = segment.AddComponent<SpriteRenderer>();

                // 异步加载精灵
                LoadSegmentSprite(type, sprite);

                pool.Enqueue(segment);
            }

            GameObject ropeSegment = pool.Dequeue();
            ropeSegment.SetActive(true);
            return ropeSegment;
        }

        private async void LoadSegmentSprite(int type, SpriteRenderer sprite)
        {
            string path = $"Assets/Res/segment/{type + 1}";
            var handler =  ResourceManager.LoadAsset<Sprite>(path);
            while (!handler.IsDone)
            {
                await Task.Yield();
            }
            if (handler.Status == AsyncOperationStatus.Succeeded)
            {
                sprite.sprite = handler.Result;
            }
        }

        public void PutSegment(int type, GameObject segment)
        {
            if (segmentPools.ContainsKey(type))
            {
                segment.SetActive(false);
                segmentPools[type].Enqueue(segment);
            }
        }

        public void ClearPool(int type)
        {
            if (segmentPools.ContainsKey(type))
            {
                while (segmentPools[type].Count > 0)
                {
                    GameObject segment = segmentPools[type].Dequeue();
                    if (segment != null)
                    {
                        UnityEngine.Object.Destroy(segment);
                    }
                }
                segmentPools.Remove(type);
            }
        }

        public void ClearAllPools()
        {
            foreach (var pool in segmentPools.Values)
            {
                while (pool.Count > 0)
                {
                    GameObject segment = pool.Dequeue();
                    if(segment != null)
                    {
                        UnityEngine.Object.Destroy(segment);
                    }
                }
            }
            segmentPools.Clear();
        }
    }
}

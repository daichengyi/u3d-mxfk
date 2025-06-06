using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

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

        public  async Task<GameObject> GetSegment(int type)
        {
            if (!segmentPools.ContainsKey(type))
            {
                segmentPools[type] = new Queue<GameObject>();
            }

            var pool = segmentPools[type];

            if (pool.Count <= 0)
            {
                GameObject segment = new GameObject("RopeSegment");
                Image image = segment.AddComponent<Image>();

                segment.layer = LayerMask.NameToLayer("UI");

                // 异步加载精灵
                image.gameObject.SetActive(false);
                string path = $"Res/segment/{type + 1}.png";
                ResourceManager.AsyncLoadRes<Sprite>(path, (spriteFrame) =>
                {
                    image.sprite = spriteFrame;
                    image.SetNativeSize();
                    image.gameObject.SetActive(true);
                });

                return segment;
            }

            GameObject ropeSegment = pool.Dequeue();
            ropeSegment.SetActive(true);
            return ropeSegment;
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
                        Object.Destroy(segment);
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
                    if (segment != null)
                    {
                        Object.Destroy(segment);
                    }
                }
            }
            segmentPools.Clear();
        }
    }
}

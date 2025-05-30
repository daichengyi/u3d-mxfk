using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class RopeTexture : MonoBehaviour
    {
        public int type { get;  set; } = -1;

        [Tooltip("终点X坐标")]
        [SerializeField] private float endX = 0f;

        [Tooltip("终点Y坐标")]
        [SerializeField] private float endY = -300f;

        [Tooltip("段落之间的重叠像素数")]
        [SerializeField] private int overlap = 28;

        [Tooltip("段落之间的间距偏移")]
        [SerializeField] private int offsetY = -2;

        [Tooltip("水平摆动幅度")]
        [SerializeField] public float amplitude = 20f;

        [Tooltip("波浪频率")]
        [SerializeField] private float frequency = 2f;

        [Tooltip("波浪数量")]
        [SerializeField] private float waveCount = 1f;

        private readonly float SEGMENT_SIZE = 32;
        private readonly float TWO_PI = Mathf.PI * 2;
        private List<GameObject> segments = new List<GameObject>();
        private List<Vector3> originalPositions = new List<Vector3>();
        private int segmentCount = 0;
        private float time = 0;
        private List<float> lastAngles = new List<float>();
        private List<Vector3> lastPositions = new List<Vector3>();
        private readonly float SMOOTH_SPEED = 0.3f;
        private bool isMoving = false;
        private float moveStartX = 0;
        private float moveStartY = 0;
        private float moveTargetX = 0;
        private float moveTargetY = 0;
        private float moveTime = 0;
        private float moveElapsed = 0;
        // Use this for initialization
        public void UpdateEndPoint(float x, float y)
        {
            Vector3 endPos = new Vector3(x, y, 0);
            float distance = endPos.magnitude;
            float effectiveSegmentLength = SEGMENT_SIZE - overlap + offsetY;

            int baseSegments = Mathf.CeilToInt(distance / effectiveSegmentLength);
            int waveSegments = Mathf.CeilToInt((amplitude / effectiveSegmentLength) * 2);
            int newSegmentCount = Mathf.Max(5, baseSegments + waveSegments);

            if (newSegmentCount != segmentCount)
            {
                UpdateSegmentCount(newSegmentCount);
            }

            UpdateRopeSegments(x, y);
        }

        private void UpdateSegmentCount(int newCount)
        {
            int diff = newCount - segmentCount;
            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    GameObject segment = RopeSegmentManager.Instance.GetSegment(type);
                    segment.transform.SetParent(transform);
                    segments.Add(segment);
                }
            }
            else if (diff < 0)
            {
                for (int i = 0; i < -diff; i++)
                {
                    GameObject segment = segments[segments.Count - 1];
                    segments.RemoveAt(segments.Count - 1);
                    RopeSegmentManager.Instance.PutSegment(type, segment);
                }
            }
            segmentCount = newCount;
        }

        private void UpdateRopeSegments(float x, float y)
        {
            int len = segments.Count;

            if (originalPositions.Count != len)
            {
                originalPositions = new List<Vector3>(len);
                lastAngles = new List<float>(len);
                lastPositions = new List<Vector3>(len);
                for (int i = 0; i < len; i++)
                {
                    originalPositions.Add(Vector3.zero);
                    lastPositions.Add(Vector3.zero);
                    lastAngles.Add(0);
                }
            }

            for (int i = 0; i < len; i++)
            {
                GameObject segment = segments[i];
                float t = i / (float)(len - 1);

                // 直接使用世界坐标计算目标位置
                Vector3 targetPos = transform.position + new Vector3(t * x, t * y, 0);
                originalPositions[i] = targetPos;

                // 设置段落位置
                segment.transform.position = targetPos;

                // 计算角度
                if (i < len - 1)
                {
                    float nextT = (i + 1) / (float)(len - 1);
                    Vector3 nextPos = transform.position + new Vector3(nextT * x, nextT * y, 0);
                    Vector3 direction = nextPos - targetPos;
                    float newAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;

                    // 平滑过渡角度
                    lastAngles[i] = Mathf.Lerp(lastAngles[i], newAngle, SMOOTH_SPEED);
                    segment.transform.rotation = Quaternion.Euler(0, 0, lastAngles[i]);
                }
                else
                {
                    segment.transform.rotation = Quaternion.Euler(0, 0, lastAngles[i - 1]);
                }
            }
        }

        public void MoveToTarget(float newEndX, float newEndY, float time)
        {
            isMoving = true;
            moveStartX = endX;
            moveStartY = endY;
            moveTargetX = newEndX;
            moveTargetY = newEndY;
            moveTime = time;
            moveElapsed = 0;
        }

        private void Update()
        {
            // 处理移动逻辑
            if (isMoving)
            {
                moveElapsed += Time.deltaTime;
                if (moveElapsed >= moveTime)
                {
                    endX = moveTargetX;
                    endY = moveTargetY;
                    isMoving = false;
                }
                else
                {
                    float progress = moveElapsed / moveTime;
                    endX = Mathf.Lerp(moveStartX, moveTargetX, progress);
                    endY = Mathf.Lerp(moveStartY, moveTargetY, progress);
                }
                UpdateEndPoint(endX, endY);
            }

            // 处理摆动效果
            time += Time.deltaTime;
            int len = segments.Count;
            if (len == 0) return;

            float basePhase = time * frequency;
            float waveStep = TWO_PI * waveCount;

            for (int i = 0; i < len; i++)
            {
                GameObject segment = segments[i];
                if (!segment || !segment.activeSelf) continue;

                float t = i / (float)(len - 1);
                float wavePhase = basePhase + t * waveStep;
                Vector3 originalPos = originalPositions[i];

                float swingAmount = Mathf.Sin(wavePhase) * amplitude * Mathf.Sin(t * Mathf.PI);

                // 直接使用世界坐标更新位置
                segment.transform.position = originalPos + new Vector3(swingAmount, 0, 0);
            }
        }

        public void DestroyByReset()
        {
            Debug.Log("移除 ropeTexture========");
            /*foreach (GameObject segment in segments)
            {
                if (segment != null)
                {
                    RopeSegmentManager.Instance.PutSegment(type, segment);
                }
            }
            //segments.Clear();
            gameObject.transform.DetachChildren();
            Destroy(gameObject);*/
        }
    }
}
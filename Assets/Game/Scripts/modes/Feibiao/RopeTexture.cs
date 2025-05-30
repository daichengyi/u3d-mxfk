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

        private readonly float SEGMENT_SIZE = 32f;
        private readonly float TWO_PI = Mathf.PI * 2f;
        private List<GameObject> segments = new List<GameObject>();
        private List<Vector2> originalPositions = new List<Vector2>();
        private int segmentCount = 0;
        private float time = 0f;
        private List<float> lastAngles = new List<float>();
        private List<Vector2> lastPositions = new List<Vector2>();
        private readonly float SMOOTH_SPEED = 0.3f;
        private bool isMoving = false;
        private float moveStartX = 0f;
        private float moveStartY = 0f;
        private float moveTargetX = 0f;
        private float moveTargetY = 0f;
        private float moveTime = 0f;
        private float moveElapsed = 0f;
        // Use this for initialization
        public void UpdateEndPoint(float x, float y)
        {
            Debug.Log("UpdateEndPoint====");

            Vector2 endPos = new Vector2(x, y);
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
                    if (segment != null)
                    {
                        RopeSegmentManager.Instance.PutSegment(type, segment);
                    }
                }
            }
            segmentCount = newCount;
        }

        private void UpdateRopeSegments(float x, float y)
        {
            int len = segments.Count;

            if (originalPositions.Count != len)
            {
                originalPositions.Clear();
                lastAngles.Clear();
                lastPositions.Clear();
                for (int i = 0; i < len; i++)
                {
                    originalPositions.Add(Vector2.zero);
                    lastPositions.Add(Vector2.zero);
                    lastAngles.Add(0f);
                }
            }

            for (int i = 0; i < len; i++)
            {
                GameObject segment = segments[i];
                float t = i / (float)(len - 1);

                // 计算目标位置
                float targetX = t * x;
                float targetY = t * y;

                // 更新原始位置
                originalPositions[i] = new Vector2(targetX, targetY);

                // 设置段落位置
                segment.transform.localPosition = new Vector3(targetX, targetY, 0f);

                // 计算角度
                if (i < len - 1)
                {
                    float nextT = (i + 1) / (float)(len - 1);
                    float dx = nextT * x - segment.transform.localPosition.x;
                    float dy = nextT * y - segment.transform.localPosition.y;
                    float newAngle = (Mathf.Atan2(dy, dx) * Mathf.Rad2Deg) - 90f;

                    // 平滑过渡角度
                    lastAngles[i] = Mathf.Lerp(lastAngles[i], newAngle, SMOOTH_SPEED);
                    segment.transform.localRotation = Quaternion.Euler(0f, 0f, lastAngles[i]);
                }
                else
                {
                    segment.transform.localRotation = Quaternion.Euler(0f, 0f, lastAngles[i - 1]);
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
            moveElapsed = 0f;
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // 处理移动逻辑
            if (isMoving)
            {
                moveElapsed += dt;
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
            time += dt;
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
                Vector2 originalPos = originalPositions[i];

                float swingAmount = Mathf.Sin(wavePhase) * amplitude * Mathf.Sin(t * Mathf.PI);

                // 在原始位置基础上添加摆动效果
                segment.transform.localPosition = new Vector3(
                    originalPos.x + swingAmount,
                    originalPos.y,
                    0f
                );
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
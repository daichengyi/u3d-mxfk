using UnityEngine;
using DG.Tweening;

namespace Assets.Scripts.common
{
    public class DOTweenAnim : MonoBehaviour
    {
        [Header("呼吸动画设置")]
        [SerializeField] private float breathDuration = 1.5f;    // 一次呼吸的持续时间
        [SerializeField] private float minScale = 0.9f;         // 最小缩放值
        [SerializeField] private float maxScale = 1.1f;         // 最大缩放值
        //[SerializeField] private Ease easeType = Ease.InOutSine; // 缓动类型

        private Sequence breathSequence; // 动画序列
        private Vector3 originalScale;   // 原始缩放值

        private void Start()
        {
            originalScale = transform.localScale;
            StartBreathAnimation();
        }

        private void OnDestroy()
        {
            StopBreathAnimation();
        }

        /// <summary>
        /// 开始呼吸动画
        /// </summary>
        public void StartBreathAnimation()
        {
            StopBreathAnimation();

            // 重置到原始缩放
            transform.localScale = originalScale;

            breathSequence = DOTween.Sequence();
            // 使用单个Tween和Yoyo循环实现平滑过渡
            breathSequence.Append(transform.DOScale(new Vector3(maxScale, maxScale, maxScale), breathDuration)
                .SetEase(Ease.InOutQuad))
                .SetLoops(-1, LoopType.Yoyo);
        }

        /// <summary>
        /// 停止呼吸动画
        /// </summary>
        public void StopBreathAnimation()
        {
            if (breathSequence != null)
            {
                breathSequence.Kill();
                breathSequence = null;
            }
            // 恢复原始缩放
            transform.localScale = originalScale;
        }

        /// <summary>
        /// 设置呼吸动画的频率
        /// </summary>
        /// <param name="duration">一次呼吸的持续时间</param>
        public void SetBreathDuration(float duration)
        {
            if (duration <= 0) return;
            breathDuration = duration;
            StartBreathAnimation(); // 重新开始动画以应用新的持续时间
        }
    }
}
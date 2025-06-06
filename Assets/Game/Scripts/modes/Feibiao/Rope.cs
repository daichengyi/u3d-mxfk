using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class Rope : RopeBase
    {
        private bool _isTouchEnabled = true;
        [HideInInspector]
        public bool isLocked = false;
        private bool isMoving = false;
        [SerializeField]
        private Sprite[] spriteFrames;
        [SerializeField]
        public Image sprite;

        [HideInInspector]
        public HingeJoint2D hingeJoint;

        // Use this for initialization
        void Start()
        {
            _isTouchEnabled = true;
            // 设置初始透明度为0
            sprite.DOFade(0f, 0);

            // 使用DOTween实现淡入效果
            sprite.DOFade(1f, 0.25f).SetDelay(0.45f);
        }

        public bool isTouchEnabled
        {
            get { return _isTouchEnabled; }
            set
            {
                _isTouchEnabled = value;
                // 如果需要实现透明度变化，可以在这里添加
            }
        }

        public void SetLock(bool value)
        {
            isLocked = value;
            sprite.DOFade(value ? 0f : 1f, 0);
        }

        public override void RemoveFromBoard()
        {
            base.RemoveFromBoard();
            isTouchEnabled = false;

            if (hingeJoint != null)
            {
                Destroy(hingeJoint);
                hingeJoint = null;
            }

            Destroy(sprite.GetComponent<Rigidbody2D>());
            sprite.DOFade(1f, 0);
            sprite.transform.rotation = Quaternion.identity;
        }

        public override void SetType(int type, bool isAnimated = true)
        {
            base.SetType(type);
            this.type = type;
            sprite.sprite = spriteFrames[type];
            //sprite.transform.rotation = Quaternion.Euler(0, 0, -transform.parent.parent.rotation.eulerAngles.z);
            isMoving = false;
        }

        public override void MoveStart(Action onComplete)
        {
            base.MoveStart(onComplete);
            sprite.transform.rotation = Quaternion.identity;

            Sequence sequence = DOTween.Sequence();

            // 创建旋转序列
            Sequence rotateSequence = DOTween.Sequence();
            rotateSequence.Append(sprite.transform.DORotate(new Vector3(0, 0, -20), 0.05f))
                         .Append(sprite.transform.DORotate(new Vector3(0, 0, 40), 0.05f))
                         .Append(sprite.transform.DORotate(new Vector3(0, 0, -20), 0.05f));

            // 重复旋转序列两次
            sequence.Join(rotateSequence.SetLoops(2));

            // 添加缩放动画
            sequence.Join(sprite.transform.DOScale(Vector3.zero, 0.3f));

            // 完成后调用回调
            sequence.OnComplete(() => onComplete?.Invoke());

            sequence.Play();
        }

        public void Shake()
        {
            float oldAngle = sprite.transform.rotation.eulerAngles.z;// -transform.parent.parent.rotation.eulerAngles.z;

            Sequence shakeSequence = DOTween.Sequence();
            shakeSequence.Append(sprite.transform.DORotate(new Vector3(0, 0, -10), 0.05f))
                        .Append(sprite.transform.DORotate(new Vector3(0, 0, 20), 0.05f))
                        .Append(sprite.transform.DORotate(new Vector3(0, 0, -oldAngle), 0.05f));

            shakeSequence.Play();
        }

        public override void Dispose()
        {
            base.Dispose();
            transform.SetParent(null);
            Destroy(gameObject);
        }
    }
}
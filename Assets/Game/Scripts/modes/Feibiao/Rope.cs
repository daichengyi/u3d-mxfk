using DG.Tweening;
using System;
using UnityEngine;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class Rope : RopeBase
    {
        private bool _isTouchEnabled = true;
        public bool isLocked = false;
        private bool isMoving = false;

        private Sprite[] spriteFrames;

        public GameObject sprite;

        // Use this for initialization
        void Start()
        {
            _isTouchEnabled = true;
            // 设置初始透明度为0
            SpriteRenderer spriteRenderer = sprite.GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(1f, 1f, 1f, 0f);

            // 使用DOTween实现淡入效果
            spriteRenderer.DOFade(1f, 0.25f).SetDelay(0.45f);
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

        public  void SetLock(bool value)
        {
            isLocked = value;
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, value ? 0f : 1f);
        }

        public void RemoveFromBoard()
        {
            isTouchEnabled = false;

            Destroy(sprite.GetComponent<Rigidbody2D>());
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 1f);

            sprite.transform.rotation = Quaternion.identity;
        }

        public override void SetType(int type, bool isAnimated = true)
        {
            base.SetType(type);
            this.type = type;
            sprite.GetComponent<SpriteRenderer>().sprite = spriteFrames[type];
            sprite.transform.rotation = Quaternion.Euler(0, 0, -transform.parent.parent.rotation.eulerAngles.z);
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
            float oldAngle = -transform.parent.parent.rotation.eulerAngles.z;

            Sequence shakeSequence = DOTween.Sequence();
            shakeSequence.Append(sprite.transform.DORotate(new Vector3(0, 0, -10), 0.05f))
                        .Append(sprite.transform.DORotate(new Vector3(0, 0, 20), 0.05f))
                        .Append(sprite.transform.DORotate(new Vector3(0, 0, -oldAngle), 0.05f));

            shakeSequence.Play();
        }
    }
}
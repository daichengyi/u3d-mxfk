using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class TmpRope : RopeBase
    {
        public bool isAnimated = false;

        private float rotateTime = 0f;

        public override void SetType(int type, bool isAnimated = true)
        {
            base.SetType(type, isAnimated);
            this.type = type;

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                Image sprite = child.GetComponent<Image>();
                if (sprite != null)
                {
                    loadSprite(i * 0.1f, sprite);
                }
            }
            this.isAnimated = isAnimated;
        }

        private async void loadSprite(float time, Image sprite)
        {
            sprite.transform.DOScale(Vector3.zero,0f);
            sprite.gameObject.SetActive(false);
            string path = $"Res/tmp/{type + 1}.png";
            Sprite spriteFrame = await ResourceManager.AsyncLoadRes<Sprite>(path);
            sprite.sprite = spriteFrame;
            sprite.gameObject.SetActive(true);
            //sprite.DOFade(0, 0);
            //sprite.DOFade(1, 0.2f).SetDelay(time);    
            sprite.transform.DOScale(Vector3.one, 0.5f);
            rotateTime = 0.5f;
        }

        public override void MoveStart(Action func)
        {
            base.MoveStart(func);
            Debug.Log("tmp rope moveStart");
            gameObject.transform.DOScale(Vector3.one * 0.2f, 0.3f)
                .OnComplete(() => func?.Invoke());
        }

        private void Update()
        {
            if (rotateTime <= 0) return;
            rotateTime -= Time.deltaTime;
            // 计算每帧旋转角度
            float angle = 360f * Time.deltaTime;
             angle = -angle;

            // 执行旋转
            transform.GetChild(0).Rotate(0, 0, angle);

            if(rotateTime <= 0)
            {
                rotateTime = 0;
                transform.GetChild(0).Rotate(0, 0, 0);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            transform.SetParent(null);
            Destroy(gameObject);
        }
    }
}
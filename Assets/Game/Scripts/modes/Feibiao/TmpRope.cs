using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class TmpRope : RopeBase
    {
        public bool isAnimated = false;

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
            sprite.gameObject.SetActive(false);
            string path = $"Res/tmp/{type + 1}.png";
            Sprite spriteFrame = await ResourceManager.AsyncLoadRes<Sprite>(path);

            sprite.sprite = spriteFrame;
            sprite.gameObject.SetActive(true);
            sprite.DOFade(0, 0);
            sprite.DOFade(1, 0.2f).SetDelay(time);    
        }

        public override void MoveStart(Action func)
        {
            base.MoveStart(func);
            Debug.Log("tmp rope moveStart");
            gameObject.transform.DOScale(Vector3.one * 0.2f, 0.3f)
                .OnComplete(() => func?.Invoke());
        }
    }
}
using DG.Tweening;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

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
                SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    loadSprite(i * 0.1f, spriteRenderer);
                }
            }
            this.isAnimated = isAnimated;
        }

        private async void loadSprite(float time,SpriteRenderer spriteRenderer)
        {
            string path = $"Feibiao/images/tmp/{type + 1}";
            var handler = ResourceManager.LoadAsset<Sprite>(path);
            while (!handler.IsDone)
            {
                await Task.Yield();
            }
            if (handler.Status == AsyncOperationStatus.Succeeded)
            {
                spriteRenderer.sprite = handler.Result;
                Color color = spriteRenderer.color;
                color.a = 0;
                spriteRenderer.color = color;

                DOVirtual.DelayedCall(time, () => {
                    Color color = spriteRenderer.color;
                    color.a = 1;
                    spriteRenderer.DOBlendableColor(color, 0.2f);
                });
            }
        }

        public void RemoveFromBoard() { }

        public override void MoveStart(Action func)
        {
            base.MoveStart(func);
            Debug.Log("tmp rope moveStart");
            gameObject.transform.DOScale(Vector3.one * 0.2f, 0.3f)
                .OnComplete(() => func?.Invoke());
        }
    }
}
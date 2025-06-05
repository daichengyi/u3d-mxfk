using Assets.Scripts.common;
using Assets.Scripts.Events;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class Target : MonoBehaviour
    {
        public int type { get; private set; } = -1;
        public int posIndex { get; set; } = -1;
        public int targetCount { get; private set; } = 3;

        [SerializeField] private Image sprite;
        [SerializeField] private Image capSpr;
        [SerializeField] private Transform holeNode;
        [SerializeField] private Image shadow;

        public int state { get; set; }

        // Use this for initialization
        void Awake()
        {
            type = -1;
            SetSkin(UserModel.Instance.skinID);
            //EventManager.Instance.AddListener("onEventChangeSkinBg", SetSkin);
            EventMng.addEventListener(EventTypes.CHANGE_SKIN_BG, SetSkinHandler);
        }

        private void SetSkinHandler(EventStruct evt)
        {

        }

        private void OnDestroy()
        {
            EventMng.removeEventListener(EventTypes.CHANGE_SKIN_BG, SetSkinHandler);
        }

        public Transform Sub()
        {
            if (targetCount <= 0)
            {
                return null;
            }
            targetCount--;

            Transform target = holeNode.GetChild(targetCount);
            target.gameObject.SetActive(true);
            TargetAnimation(target);

            return target;
        }

        private void TargetAnimation(Transform target, float startOpacity = 0, float endOpacity = 1)
        {
            for (int i = target.childCount - 1; i >= 0; i--)
            {
                Transform child = target.GetChild(i);
                CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = child.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = startOpacity;
                float delayTime = (target.childCount - i) * 0.1f;
                canvasGroup.DOFade(endOpacity, 0.2f).SetDelay(delayTime);
            }
        }

        public async void InitWithType(int tp, Action callback)
        {
            targetCount = 3;
            type = tp;
            state = (int)TargetState.Loading;

            for (int index = 0; index < holeNode.childCount; index++)
            {
                Transform hole = holeNode.GetChild(index);
                hole.gameObject.SetActive(false);
                for (int i = 0; i < hole.childCount; i++)
                {
                    Transform child = hole.GetChild(i);
                    Image childSprite = child.GetComponent<Image>();
                    childSprite.DOFade(0, 0);
                    childSprite.gameObject.SetActive(false);
                    await ResourceManager.AsyncLoadRes<Sprite>(
                        $"Res/order/{type + 1}-3.png", (spriteFrame) =>
                        {
                            childSprite.sprite = spriteFrame;
                            childSprite.gameObject.SetActive(true);
                            childSprite.DOFade(1, 0.2f).SetDelay(i * 0.1f);
                        }
                    );
                }
            }
            sprite.gameObject.SetActive(false);
            await ResourceManager.AsyncLoadRes<Sprite>(
                $"Res/order/{type + 1}.png", (mainSprite) => {
                    sprite.sprite = mainSprite;
                    sprite.gameObject.SetActive(true);
                }
            );
            

            capSpr.gameObject.SetActive(false);
            await ResourceManager.AsyncLoadRes<Sprite>(
                $"Res/order/{type + 1}-2.png", (capSprite) => {
                    capSpr.sprite = capSprite;
                    capSpr.gameObject.SetActive(true);
                    callback?.Invoke();
                }
            );
        }

        public void ShowOne(int index)
        {
            holeNode.GetChild(index).gameObject.SetActive(true);
        }

        public bool IsFinish()
        {
            return targetCount == 0;
        }

        public void ShowFinishAnimation(float delay, Action callback)
        {
            Transform[] children = new Transform[holeNode.childCount];
            for (int i = 0; i < holeNode.childCount; i++)
            {
                children[i] = holeNode.GetChild(i);
            }

            int total = children.Length * children[0].childCount;
            float dt = delay / total;
            int count = 0;

            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                for (int j = 0; j < child.childCount; j++)
                {
                    Transform child2 = child.GetChild(j);
                    CanvasGroup canvasGroup = child2.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = child2.gameObject.AddComponent<CanvasGroup>();
                    }
                    canvasGroup.alpha = 1f;

                    bool isLast = count == children.Length * child.childCount - 1;
                    canvasGroup.DOFade(0f, 0.2f)
                        .SetDelay(count * dt)
                        .OnComplete(() =>
                        {
                            if (isLast)
                            {
                                callback?.Invoke();
                            }
                        });
                    count++;
                }
            }
        }

        public void UpdateView(int n = 3)
        {
            targetCount = n;
            Debug.Log(targetCount);
            for (int i = 0; i < 3; i++)
            {
                Transform target = holeNode.GetChild(i);
                target.gameObject.SetActive(i >= targetCount);
            }
        }

        public async void SetSkin(int index)
        {
            /*shadow.gameObject.SetActive(false);
            string path = $"Res/targetItem/{index}-1.png";
            Sprite spriteFrame = await ResourceManager.AsyncLoadRes<Sprite>(path);
            shadow.sprite = spriteFrame;
            shadow.gameObject.SetActive(true);*/
        }
    }
}
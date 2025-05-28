using Assets.Scripts.common;
using System.Collections;
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
        void Start()
        {
            type = -1;
            ///SetSkin(UserService.Instance.GetSelectedBgSkin());
            //EventManager.Instance.AddListener("onEventChangeSkinBg", SetSkin);
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

                /*LeanTween.alphaCanvas(canvasGroup, endOpacity, 0.2f)
                    .setDelay((target.childCount - i) * 0.1f);*/
            }
        }

        public async void InitWithType(int type)
        {
            targetCount = 3;
            type = type;
            state = (int)TargetState.Loading;

            /*for (int index = 0; index < holeNode.childCount; index++)
            {
                Transform hole = holeNode.GetChild(index);
                hole.gameObject.SetActive(false);

                for (int i = 0; i < hole.childCount; i++)
                {
                    Transform child = hole.GetChild(i);
                    Image childSprite = child.GetComponent<Image>();
                    Sprite spriteFrame = await ResourceLoader.Instance.Load<Sprite>(
                        "Feibiao",
                        $"images/order/{type + 1}-3"
                    );
                    childSprite.sprite = spriteFrame;
                    CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = child.gameObject.AddComponent<CanvasGroup>();
                    }
                    canvasGroup.alpha = 0;
                    LeanTween.alphaCanvas(canvasGroup, 1f, 0.2f)
                        .setDelay(i * 0.1f);
                }
            }

            Sprite mainSprite = await ResourceLoader.Instance.Load<Sprite>(
                "Feibiao",
                $"images/order/{type + 1}"
            );
            sprite.sprite = mainSprite;

            Sprite capSprite = await ResourceLoader.Instance.Load<Sprite>(
                "Feibiao",
                $"images/order/{type + 1}-2"
            );
            capSpr.sprite = capSprite;*/
        }

        public void ShowOne(int index)
        {
            holeNode.GetChild(index).gameObject.SetActive(true);
        }

        public bool IsFinish()
        {
            return targetCount == 0;
        }

        public void ShowFinishAnimation(float delay, System.Action callback)
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
                    /*LeanTween.alphaCanvas(canvasGroup, 0f, 0.2f)
                        .setDelay(count * dt)
                        .setOnComplete(() =>
                        {
                            if (isLast)
                            {
                                callback?.Invoke();
                            }
                        });*/
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
            /*Sprite spriteFrame = await ResourceLoader.Instance.Load<Sprite>(
                "BgSkin",
                $"images/item/{index}-1"
            );
            shadow.sprite = spriteFrame;*/
        }
    }
}
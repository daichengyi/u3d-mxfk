using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonScaleAnimation : MonoBehaviour, IPointerDownHandler
{
    public Vector2 scaleUp = new Vector2(1.2f, 1.2f); // 放大比例
    public Vector2 scaleDown = new Vector2(1f, 1f);   // 缩小比例
    public float animationDuration = 0.2f;            // 动画持续时间
    private Vector3 originalScale;


    private Button button; // 引用Button组件
    void OnEnable()
    {
        // 获取Button组件
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("ButtonScaleAnimation脚本需要附加到带有Button组件的对象上。");
            return;
        }
        if (originalScale == Vector3.zero)
        {
            originalScale = transform.localScale; // 记录初始大小
        }
        else
        {
            transform.localScale = originalScale;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        if (DOTween.IsTweening(transform))
        {
            return;
        }
        SoundManager.Ins.PlaySfx("btn");
        // 放大动画
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOScale(scaleUp, animationDuration).SetEase(Ease.Linear));
        mySequence.Append(transform.DOScale(scaleDown, animationDuration).SetEase(Ease.Linear));
    }
}
using DG.Tweening;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    public GameObject aniObj;

    void OnEnable()
    {
        aniObj.transform.localScale = Vector3.zero;
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(aniObj.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.3f).SetEase(Ease.Linear));
        mySequence.Append(aniObj.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.Linear));
    }

    protected void hide()
    {
        transform.gameObject.SetActive(false);
    }
}

using DG.Tweening;
using UnityEngine;

public class BaseView : MonoBehaviour
{
    public bool isShowAni = true;
    public GameObject aniObj;
    protected object parameters = null;
    [HideInInspector] public bool _canClick;
    public virtual void onShow(object parameters = null)
    {
        this.parameters = parameters;
        if (isShowAni)
        {
            aniObj.transform.localScale = Vector3.zero;
            Sequence mySequence = DOTween.Sequence();
            mySequence.Append(aniObj.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.3f).SetEase(Ease.Linear));
            mySequence.Append(aniObj.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.Linear));
        }
    }


    public T GetParameter<T>()
    {
        return (T)parameters;
    }

    public void hide()
    {
        gameObject.SetActive(false);
    }
}

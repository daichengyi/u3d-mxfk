using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TipUI : BaseView
{
    public TMP_Text lblDesc;
    // Start is called before the first frame update
    void Start()
    {
        // isShowAni = false;
    }

    public override void onShow(object parameters = null)
    {
        ConstantFun._tip = gameObject;
        isShowAni = false;
        base.onShow(parameters);
        string desc = GetParameter<string>();
        lblDesc.text = desc;
        transform.localScale = Vector3.one;
        transform.DOKill();
        transform.DOScale(0, 0.2f).SetDelay(1).OnComplete(() =>
        {
            transform.gameObject.SetActive(false);
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}


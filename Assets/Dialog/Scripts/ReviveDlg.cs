using Assets.Scripts.data;
using System;
using TMPro;
using UnityEngine;

/// <summary>
/// ∏¥ªÓΩÁ√Ê
/// </summary>
public class ReviveDlg : BaseView
{
    [SerializeField] TextMeshProUGUI textGamePro;
    [SerializeField] GameObject tip1;
    [SerializeField] GameObject tip2;

    private Action<bool> action;
    // Start is called before the first frame update
    public override void onShow(object parameters =null)
    {
        base.onShow(parameters);
        ReviveVo reviveVo = parameters as ReviveVo;
        action = reviveVo.action;

        tip1.SetActive(!reviveVo.unlock);
        tip2.SetActive(reviveVo.unlock);
        textGamePro.text = $"{(reviveVo.pro * 100).ToString("F0")}%";
    }

    // Update is called once per frame
    public void clickClose()
    {
        Destroy(gameObject);
        action?.Invoke(false);
    }

    public void clickVideo()
    {
        AdManager.Ins.ShowAd(0, (isSuc) =>
        {
            if (isSuc) {
                action?.Invoke(true);
                Destroy(gameObject);
            }
        });
    }
}

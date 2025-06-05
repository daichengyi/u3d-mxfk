using Assets.Game.Scripts;
using Assets.Game.Scripts.modes.Feibiao;
using Assets.Scripts.common;
using Assets.Scripts.data;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ω·À„- §¿˚\ ß∞‹
/// </summary>
public class ResultDlg : BaseView
{
    [SerializeField] RectTransform rectTransBg;
    [SerializeField] SkeletonGraphic skeGraphic;
    [SerializeField] TextMeshProUGUI textLevel;
    [SerializeField] Transform boradBg;
    [SerializeField] GameObject btnPlay;
    [SerializeField] GameObject win;

    [SerializeField] GameObject fail;
    [SerializeField] Slider sliderFail;
    [SerializeField] TextMeshProUGUI textFailPro;

    private PaintBoard paintBoard;
    // Start is called before the first frame update
    public override void onShow(object parameters)
    {
        //580   550
        base.onShow(parameters);
        OverPageData resultVo = parameters as OverPageData;

        win.SetActive(resultVo.isWin);
        fail.SetActive(!resultVo.isWin);
        
        rectTransBg.sizeDelta = new Vector2(502, resultVo.isWin?580:550);

        textLevel.text = $"Level {resultVo.level}";

        if (!resultVo.isWin)
        {
            sliderFail.value = resultVo.gameProgress;
            textFailPro.text = $"{(resultVo.gameProgress * 100).ToString("F0")}%";
        }
        skeGraphic.initialSkinName = resultVo.isWin ? "shengli" : "shibai";

        DOVirtual.DelayedCall(1f, () => {
            resultVo.paintNode.transform.SetParent(boradBg);
            resultVo.paintNode.transform.localPosition = Vector3.zero;
            resultVo.paintNode.transform.localScale = resultVo.paintNode.transform.localScale;
            paintBoard = resultVo.paintNode.GetComponent<PaintBoard>();
            if (resultVo.isWin)
            {
                paintBoard.JumpToLastStep();
            }
        });
        
    }

    // Update is called once per frame
    public void clickNextLv()
    {
        GameManager.Instance.EnterMode(GameMode.Feibiao);
        Destroy(gameObject);
    }
    public void clickLeft()
    {
        btnPlay.SetActive(!paintBoard.isAutoPlaying);
        paintBoard.PlayPrevStepManually();
    }

    public void clickRight()
    {
        btnPlay.SetActive(!paintBoard.isAutoPlaying);
        paintBoard.PlayNextStepManually();
    }

    public void clickPlay()
    {
        btnPlay.SetActive(!paintBoard.isAutoPlaying);
        paintBoard.AutoPlayClick();
    }


    public void clickHome()
    {
        GameManager.Instance.BackHomePage();
        Destroy(gameObject);
    }

    public void clickReplay()
    {
        GameManager.Instance.EnterMode(GameMode.Feibiao);
        Destroy(gameObject);
    }
}

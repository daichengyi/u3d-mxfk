using Assets.Game.Scripts;
using Assets.Scripts.common;
using UnityEngine;

public class HomeScene : MonoBehaviour
{
    void Start()
    {
        // UIManager.Instance.HideLoading();
        SoundManager.Ins.PlayMusic("bgm");
    }
   
    public void onBtnGame()
    {
        SoundManager.Ins.PlaySfx("click");
        UIManager.Instance.ShowLoading();
       GameManager.Instance.EnterMode(GameMode.Feibiao, true);
    }

    public void onBtnSet()
    {
        SoundManager.Ins.PlaySfx("click");
        UIManager.Instance.OpenView(VIEW_NAME.Set);
    }
}

using Assets.Game.Scripts;
using Assets.Scripts.common;

/// <summary>
/// Ω·À„- §¿˚
/// </summary>
public class WinDlg : UIBase
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void clickNextLv()
    {
        GameManager.Instance.EnterMode(GameMode.Feibiao);
        Destroy(gameObject);
    }
}

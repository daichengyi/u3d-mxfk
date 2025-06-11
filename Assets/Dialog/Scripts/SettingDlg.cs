using Assets.Game.Scripts;
using Assets.Scripts.common;
using Assets.Scripts.config;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 设置弹窗
/// </summary>
public class SettingDlg : BaseView
{
    [SerializeField] Toggle tgMusic;
    [SerializeField] Toggle tgSound;
    [SerializeField] Toggle tgVibrator;

    [SerializeField] GameObject btnReset;
    [SerializeField] GameObject btnBack;
    [SerializeField] GameObject gmNode;

    [SerializeField] TMP_InputField inputFieldLevel;

    private int clickCount;
    // Start is called before the first frame update
    void Start()
    {
        tgMusic.isOn = DataManager.Instance.GetData(ConstVal.Local_Music, 1) == 1;
        tgSound.isOn = DataManager.Instance.GetData(ConstVal.Local_Sound, 1) == 1;
        tgVibrator.isOn = DataManager.Instance.GetData(ConstVal.Local_Vibrator, 1) == 1;

        tgMusic.transform.GetChild(0).GetComponent<Image>().enabled = !tgMusic.isOn;
        tgSound.transform.GetChild(0).GetComponent<Image>().enabled = !tgSound.isOn;
        tgVibrator.transform.GetChild(0).GetComponent<Image>().enabled = !tgVibrator.isOn;

        btnReset.SetActive(GameManager.Instance.IsInGame());
        btnBack.SetActive(GameManager.Instance.IsInGame());

        gmNode.SetActive(Application.isEditor);
    }

    // Update is called once per frame
    public void ClickClose()
    {
        Destroy(gameObject);
    }

    public void ClickMusic()
    {
        tgMusic.transform.GetChild(0).GetComponent<Image>().enabled = !tgMusic.isOn;
        DataManager.Instance.SetData(ConstVal.Local_Music, tgMusic.isOn?1:0);
        SoundManager.Ins.IsMusic = tgMusic.isOn;
    }

    public void ClickSound()
    {
        tgSound.transform.GetChild(0).GetComponent<Image>().enabled = !tgSound.isOn;
        DataManager.Instance.SetData(ConstVal.Local_Sound, tgSound.isOn ? 1 : 0);
        SoundManager.Ins.IsSfx = tgSound.isOn;
    }

    public void ClickVibrator()
    {
        tgVibrator.transform.GetChild(0).GetComponent<Image>().enabled = !tgVibrator.isOn;
        DataManager.Instance.SetData(ConstVal.Local_Vibrator, tgVibrator.isOn ? 1 : 0);
        SoundManager.Ins.IsShake = tgVibrator.isOn;
    }

    public void ClickResetGame()
    {
        GameManager.Instance.EnterMode(GameMode.Feibiao, true);
        Destroy(gameObject);
    }

    public void ClickBackHome()
    {
        GameManager.Instance.BackHomePage();
        Destroy(gameObject);
    }

    #region GM功能

    public void ClickTitle()
    {
        clickCount++;
        if(clickCount > 10)
        {
            gmNode.SetActive(true);
        }
    }

    public void clickClearData()
    {
        Destroy(gameObject);
        DataManager.Instance.ClearAll();
    }
    public void ClickGM()
    {
        int level = int.Parse(inputFieldLevel.text);
        if(level >= 0)
        {
            if(level > ConstVal.ROPE_MAX_LEVEL)
            {
                UIManager.Instance.ShowMsg("超过最大关卡");
                return;
            }
            UserModel.Instance.level = level;
            UserModel.Instance.tempLevel = level;
            GameManager.Instance.EnterMode(GameMode.Feibiao);
            Destroy(gameObject);
        }
    }
    #endregion
}

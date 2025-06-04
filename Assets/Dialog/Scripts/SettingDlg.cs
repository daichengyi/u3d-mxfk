using Assets.Game.Scripts;
using Assets.Scripts.common;
using Assets.Scripts.config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// …Ë÷√µØ¥∞
/// </summary>
public class SettingDlg : UIBase
{
    [SerializeField] Toggle tgMusic;
    [SerializeField] Toggle tgSound;
    [SerializeField] Toggle tgVibrator;

    [SerializeField] GameObject btnReset;
    [SerializeField] GameObject btnBack;


    [SerializeField] TMP_InputField inputFieldLevel;
    // Start is called before the first frame update
    void Start()
    {
        tgMusic.isOn = DataManager.Instance.GetData(ConstVal.Local_Music, 1) == 1;
        tgSound.isOn = DataManager.Instance.GetData(ConstVal.Local_Sound, 1) == 1;
        tgVibrator.isOn = DataManager.Instance.GetData(ConstVal.Local_Vibrator, 1) == 1;

        btnReset.SetActive(GameManager.Instance.IsInGame());
        btnBack.SetActive(GameManager.Instance.IsInGame());
    }

    // Update is called once per frame
    public void ClickClose()
    {
        Destroy(gameObject);
    }

    public void ClickMusic()
    {
        DataManager.Instance.SetData(ConstVal.Local_Music, tgMusic.isOn?1:0);
        SoundManager.Ins.IsMusic = tgMusic.isOn;
    }

    public void ClickSound()
    {
        DataManager.Instance.SetData(ConstVal.Local_Sound, tgSound.isOn ? 1 : 0);
        SoundManager.Ins.IsSfx = tgSound.isOn;
    }

    public void ClickVibrator()
    {
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

    #region GMπ¶ƒ‹
    public void ClickGM()
    {
        int level = int.Parse(inputFieldLevel.text);
        if(level >= 0)
        {
            UserModel.Instance.level = level;
            GameManager.Instance.EnterMode(GameMode.Feibiao);
            Destroy(gameObject);
        }
    }
    #endregion
}

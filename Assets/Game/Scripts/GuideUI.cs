using Assets.Game.Scripts;
using Assets.Scripts.config;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 新手引导
/// </summary>
public class GuideUI : MonoBehaviour
{
    [SerializeField] private GuideMask guideMask;
    [SerializeField] private Transform tip;
    [SerializeField] private TextMeshProUGUI textTip;

    private int step;

    private List<string> guideData = new List<string>();
    private List<string> guidePath = new List<string>();

    // Start is called before the first frame update
    public void init()
    {
        gameObject.SetActive(true);
        step = 0;
        guideData = ConstVal.Guide_Data;
        guidePath = ConstVal.Guide_Path;
        guideMask.Init();
        updateGuide();
    }

    private void updateGuide()
    {
        string nodePath = guidePath[step];
        Transform trans = transform.parent.Find(nodePath);
        guideMask.Play(trans.GetComponent<RectTransform>());

        string desc = guideData[step];
        textTip.text = desc;

        tip.position = new Vector3(trans.position.x + 50, trans.position.y - 120);
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            step++;
            if (step >= guideData.Count)
            {
                gameObject.SetActive(false);
                GameManager.Instance.isGuide = false;
                return;
            }
            updateGuide();
        }
    }
}
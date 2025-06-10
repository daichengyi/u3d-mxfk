using Assets.Game.Scripts;
using Assets.Scripts.common;
using UnityEngine;

public class HomeScene : MonoBehaviour
{
    [SerializeField] Transform conetnt;
    [SerializeField] Transform bottom;

    private int curIndex = 1;

    void Start()
    {
        UIManager.Instance.HideLoading();
        SoundManager.Ins.PlayMusic("bgm");

        loadCollection();
    }

    private async void loadCollection()
    {
        await ResourceManager.AsyncLoadRes<GameObject>("uiPrefab/CollectionView.prefab", (prefab) =>
        {
            GameObject go = Instantiate(prefab);
            go.transform.SetParent(conetnt.GetChild(0));
            go.transform.localPosition = Vector3.zero;
        });
    }

    public void onBtnGame()
    {
        UIManager.Instance.ShowLoading();
        GameManager.Instance.EnterMode(GameMode.Feibiao, true);
    }

    public void onBtnSet()
    {
        UIManager.Instance.OpenView(VIEW_NAME.SetttingDlg, VIEW_TYPE.dialog);
    }

    public void clickBottom(GameObject node)
    {
        int index = node.transform.GetSiblingIndex();

        if (index == curIndex) return;

        if (index == 2)
        {
            UIManager.Instance.ShowMsg("Stay tuned");
            return;
        }

        bottom.GetChild(curIndex).Find("Select").gameObject.SetActive(false);
        conetnt.GetChild(curIndex).gameObject.SetActive(false);

        Transform select = node.transform.Find("Select");
        select.gameObject.SetActive(true);
        conetnt.GetChild(index).gameObject.SetActive(true);

        curIndex = index;
    }
}

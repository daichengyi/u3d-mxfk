using Assets.Game.Scripts.modes.Feibiao;
using UnityEngine;
using UnityEngine.UI;

public class TmpNodeManager : MonoBehaviour
{

    [SerializeField] private Transform tmpsNode;
    [SerializeField] public Transform hideTmpNode;
    [SerializeField] private GameObject tmpPrefab;
    [SerializeField] private RectTransform shadowNode;
    [SerializeField] private RectTransform leftNode;
    [SerializeField] private RectTransform rightNode;

    private const int MAX_TMP = 7;
    public TmpRope[] tmpContents = new TmpRope[MAX_TMP];
    public int activeSlotCount = 5; // 默认前5个槽位激活


    // Start is called before the first frame update
    public bool IsFull()
    {
        for (int i = 0; i < activeSlotCount; i++)
        {
            if (tmpContents[i] == null) return false;
        }
        return true;
    }

    public int GetFreeSlotCount()
    {
        int count = 0;
        for (int i = 0; i < activeSlotCount; i++)
        {
            if (tmpContents[i] == null) count++;
        }
        return count;
    }

    public int GetEmptyIndex()
    {
        for (int i = 0; i < activeSlotCount; i++)
        {
            if (tmpContents[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    public TmpRope PushObjToTemp(Rope operateObj, int index)
    {
        if (index == -1) return null;

        GameObject prefab = Instantiate(tmpPrefab);
        prefab.transform.SetParent(tmpsNode.GetChild(index));

        TmpRope tmpComp = prefab.GetComponent<TmpRope>();
        tmpComp.SetType(operateObj.type);
        tmpContents[index] = tmpComp;
        Debug.Log("放到槽里");
        return tmpComp;
    }

    public void PushToHideTmp(Rope operateObj)
    {
        GameObject prefab = Instantiate(tmpPrefab);
        prefab.transform.SetParent(hideTmpNode);
        TmpRope tmpComp = prefab.GetComponent<TmpRope>();
        tmpComp.SetType(operateObj.type, false);
    }

    public void OnUnlockTmp()
    {
        if (activeSlotCount >= MAX_TMP)
        {
            //UIService.Instance.ShowMessage("无法添加更多槽位");
            return;
        }

        tmpsNode.GetChild(activeSlotCount).gameObject.SetActive(true);
        activeSlotCount++;
        tmpsNode.GetComponent<LayoutGroup>().SetLayoutHorizontal();
        tmpsNode.GetComponent<LayoutGroup>().SetLayoutVertical();

        shadowNode.sizeDelta = new Vector2(tmpsNode.GetComponent<RectTransform>().sizeDelta.x + 100, shadowNode.sizeDelta.y);
        leftNode.anchoredPosition = new Vector2(-tmpsNode.GetComponent<RectTransform>().sizeDelta.x / 2 - 15, leftNode.anchoredPosition.y);
        rightNode.anchoredPosition = new Vector2(tmpsNode.GetComponent<RectTransform>().sizeDelta.x / 2 + 15, rightNode.anchoredPosition.y);
        //SoundManager.Instance.PlaySound("jiesuo");
    }

    public void ClearTmp(System.Action onComplete = null)
    {
        for (int i = 0; i < activeSlotCount; i++)
        {
            if (tmpContents[i] != null)
            {
                tmpContents[i].transform.SetParent(hideTmpNode);
                tmpContents[i] = null;
            }
        }
        onComplete?.Invoke();
    }

    public bool CanClear()
    {
        for (int i = 0; i < activeSlotCount; i++)
        {
            if (tmpContents[i] != null) return true;
        }
        return false;
    }
}

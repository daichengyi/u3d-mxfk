using Assets.Game.Scripts.modes.Feibiao;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public class TmpNodeManager : MonoBehaviour
{

    [SerializeField] private Transform tmpsNode;
    [SerializeField] public Transform hideTmpNode;
    [SerializeField] private GameObject tmpPrefab;
    //[SerializeField] private RectTransform shadowNode;
    //[SerializeField] private RectTransform leftNode;
    //[SerializeField] private RectTransform rightNode;

    private const int MAX_TMP = 7;
    [HideInInspector]
    public TmpRope[] tmpContents = new TmpRope[MAX_TMP];
    [HideInInspector]
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

        GameObject prefab = Instantiate(tmpPrefab, tmpsNode.GetChild(index));
        TmpRope tmpComp = prefab.GetComponent<TmpRope>();
        tmpComp.SetType(operateObj.type);
        tmpContents[index] = tmpComp;
        Debug.Log("放到槽里");
        return tmpComp;
    }

    public void PushToHideTmp(Rope operateObj)
    {
        GameObject prefab = Instantiate(tmpPrefab, hideTmpNode);
        TmpRope tmpComp = prefab.GetComponent<TmpRope>();
        tmpComp.SetType(operateObj.type, false);
    }

    public void OnUnlockTmp()
    {
        if (activeSlotCount >= MAX_TMP)
        {
            UIManager.Instance.ShowMsg("Unable to add more slots");
            return;
        }

        tmpsNode.GetChild(activeSlotCount).gameObject.SetActive(true);
        activeSlotCount++;

        //改版后不需要计算
        /*float width = tmpsNode.GetComponent < RectTransform >().rect.width;
        shadowNode.sizeDelta = new Vector2(width + 100, shadowNode.sizeDelta.y);
        leftNode.localPosition = new Vector2(-shadowNode.sizeDelta.x / 2+5, 0);
        rightNode.localPosition = new Vector2(shadowNode.sizeDelta.x / 2, 0);*/

        SoundManager.Ins.PlaySfx("jiesuo");
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

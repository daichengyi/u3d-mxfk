using Assets.Game.Scripts;
using Assets.Scripts.common.ScrollList;
using Assets.Scripts.config;
using Assets.Scripts.Events;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Home.Scripts.collection
{
    /// <summary>
    /// 收藏部
    /// </summary>
    public class CollectionView : MonoBehaviour
    {
        [SerializeField] ScrollList scrollList;
        [SerializeField] TextMeshProUGUI textProgress;
        [SerializeField] GameObject btnclose;
        // Use this for initialization
        void Start()
        {
            btnclose.SetActive(GameManager.Instance.IsInGame());
            List<CollectionItemData> list = new List<CollectionItemData>();
            int curLv = UserModel.Instance.level;
            int unlockLv = UserModel.Instance.tempLevel;
            for (int i = 0; i <= ConstVal.ROPE_MAX_LEVEL; i++) {
                list.Add(new CollectionItemData() { index = i,isUnlock=( i< unlockLv) });
            }
            scrollList.AddDatas(list);

            textProgress.text = $"Collect progress:{curLv}/{ConstVal.ROPE_MAX_LEVEL+1}";

            EventMng.addEventListener(EventTypes.SELECT_COLLECTION_IDX, onSelectHandler);
        }

        void OnDestroy()
        {
            EventMng.removeEventListener(EventTypes.SELECT_COLLECTION_IDX, onSelectHandler);
        }

        void onSelectHandler(EventStruct evt)
        {
            CollectionItem item = scrollList.IndexGetElement(UserModel.Instance.selectedPainting) as CollectionItem;
            item.SetState();
            UserModel.Instance.selectedPainting = Convert.ToInt32(evt.target);
        }

        public void clickClose()
        {
            Destroy(gameObject);
        }
    }
}
using Assets.Scripts.common.ScrollList;
using Assets.Scripts.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Home.Scripts.collection
{
    internal class CollectionItem:ScrollListElement
    {
        [SerializeField] Image imgIcon;

        [SerializeField] GameObject select;

        [SerializeField] Material matGray;

        private int curIndex = 0;

        public override void UpdateData()
        {
            base.UpdateData();
            if (Data != null) {
                CollectionItemData itemData = Data as CollectionItemData;
                imgIcon.gameObject.SetActive(false);
                ResourceManager.AsyncLoadRes<Sprite>($"Res/collection/{itemData.index}.png", (res) => {
                    imgIcon.sprite = res;
                    imgIcon.gameObject.SetActive(true);
                });
                select.SetActive(UserModel.Instance.selectedPainting == itemData.index);
                curIndex = itemData.index;

                imgIcon.material = itemData.isUnlock?null:matGray;
            }
        }
        public void ClickItem()
        {
            select.SetActive(true);
            EventMng.dispatchEvent(EventTypes.SELECT_COLLECTION_IDX, curIndex);
        }

        public void SetState()
        {
            select.SetActive(false);
        }
    }
}

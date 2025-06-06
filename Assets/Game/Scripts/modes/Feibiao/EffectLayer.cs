using Assets.Scripts.common;
using Assets.Scripts.Events;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class EffectLayer: MonoBehaviour
    {

        [SerializeField] private GameObject touchScrewPrefab;
        [SerializeField] private GameObject hammerPrefab;

        private void Start()
        {
            EventMng.addEventListener(EventTypes.SCREW_REMOVE, playTouchEff);
        }

        private void OnDestroy()
        {
            EventMng.removeEventListener(EventTypes.SCREW_REMOVE, playTouchEff);
        }

        private void playTouchEff(EventStruct es)
        {
            var targetNode = es.target;
            Transform targetTransform = targetNode as Transform;
            if (targetTransform == null) return;

            Vector3 worldPos = targetTransform.parent.TransformPoint(targetTransform.localPosition);
            Vector3 local = transform.InverseTransformPoint(worldPos);

            GameObject eff = Instantiate(touchScrewPrefab);
            eff.transform.SetParent(transform);
            eff.transform.localPosition = local;

            Destroy(eff, 0.25f);
        }

        /// <summary>
        /// 敲玻璃动画
        /// </summary>
        /// <param name="targetNode"></param>
        public void PlayHammerEffect(Transform targetNode)
        {
            Vector3 worldPos = targetNode.parent.TransformPoint(targetNode.localPosition);
            Vector3 local = transform.InverseTransformPoint(worldPos);

            GameObject node = Instantiate(hammerPrefab);
            node.transform.SetParent(transform);
            node.transform.localPosition = local;

            SkeletonGraphic ske = node.GetComponent<SkeletonGraphic>();
            ske.AnimationState.Complete += (TrackEntry trackEntry) =>
            {
                Destroy(node, 0.7f);
            };
        }

        /// <summary>
        /// 难度飙升
        /// </summary>
        /// <param name="level"></param>
        public void ShowLevelupEffect(int level)
        {
            if (GameManager.Instance.currMode.id != GameMode.Feibiao)
            {
                UIManager.Instance.ShowMsg("Special stages are challenging. Proceed with caution");
                return;
            }

            if (level == 0) return;

            SoundManager.Ins.PlaySfx("ding");
            Transform spineNode = transform.Find("nandubiaosheng");
            if (spineNode == null) return;

            SkeletonAnimation spine = spineNode.GetComponent<SkeletonAnimation>();
            if (spine == null) return;

            spineNode.gameObject.SetActive(true);

            string animationName = "animation";
            if (level > 1)
            {
                animationName += (level - 1).ToString();
            }
            if (level > 5)
            {
                animationName = "animation4";
            }

            spine.AnimationState.SetAnimation(0, animationName, false);
            Invoke(nameof(HideSpine), 1.25f);
        }

        private void HideSpine()
        {
            Transform spineNode = transform.Find("nandubiaosheng");
            if (spineNode != null)
            {
                spineNode.gameObject.SetActive(false);
            }
        }
    }
}

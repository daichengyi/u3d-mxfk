using Assets.Scripts.common;
using Assets.Scripts.Events;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class TargetMgr : MonoBehaviour
    {

        private const int MAX_TARGET = 4;
        private const int MAX_TMP = 7;
        private const float TARGET_MOVE_DUR = 0.3f;
        private const int EASY_MAX_COUNT = 4;

        private int unlockCount = 2;
        private int easyCount = 0;

        [SerializeField] private Transform targetsNode;
        [SerializeField] private Transform levelRoot;
        [SerializeField] private GameObject targetPrefab;
        [SerializeField] private GameObject ropePrefab;
        [SerializeField] private TmpNodeManager tmpNodeMgr;
        [SerializeField] private PaintBoard paintBoard;
        [SerializeField] private Image progressBar;
        [SerializeField] private TextMeshProUGUI progressLabel;
        //[SerializeField] private TextMeshProUGUI slotNotEnoughLabel;

        private Feibiao feibiao;
        private List<int> data = new List<int>();
        private List<Target> targets = new List<Target>();
        [HideInInspector] public int totalTargetCount = 0;
        private List<Vector3> targetPos = new List<Vector3>();
        private GamePlay gamePlay;

        private void Start()
        {
            EventMng.addEventListener(EventTypes.BTN_ADD_TMP, OnUnlockTmp);
            EventMng.addEventListener(EventTypes.REMOVE_BOARD, OnRemoveBoard);
            EventMng.addEventListener(EventTypes.BTN_CLEAR_TMP, OnClearTmp);
            EventMng.addEventListener(EventTypes.UNLOCK_2, OnUnlock);
            EventMng.addEventListener(EventTypes.UNLOCK_1, OnUnlock);
            progressBar.fillAmount = 0;
            feibiao = transform.parent.GetComponent<Feibiao>();
        }
        private void OnDestroy()
        {
            EventMng.removeEventListener(EventTypes.BTN_ADD_TMP, OnUnlockTmp);
            EventMng.removeEventListener(EventTypes.REMOVE_BOARD, OnRemoveBoard);
            EventMng.removeEventListener(EventTypes.BTN_CLEAR_TMP, OnClearTmp);
            EventMng.removeEventListener(EventTypes.UNLOCK_2, OnUnlock);
            EventMng.removeEventListener(EventTypes.UNLOCK_1, OnUnlock);
        }

        public void SetData(List<int> newData, GamePlay newGamePlay)
        {
            data = newData;
            totalTargetCount = data.Count;
            Debug.Log($"this.data targetMgr: {string.Join(",", data)}");
            targets.Clear();
            InitTarget();
            gamePlay = newGamePlay;
        }

        public float GetProgress()
        {
            return (float)(totalTargetCount - data.Count) / totalTargetCount;
        }

        private void InitTarget()
        {
            // 获取目标位置
            for (int i = 0; i < targetsNode.childCount; i++)
            {
                targetPos.Add(targetsNode.GetChild(i).position);
            }

            // 初始化目标
            for (int i = 0; i < unlockCount; i++)
            {
                int nextData = GetNextTarget();
                if (nextData >= 0)
                {
                    HandleTargetGeneration(i, nextData);
                }
                else
                {
                    Debug.Log("所有目标已经出完");
                }
            }
        }

        private void HandleTargetGeneration(int index, int type)
        {
            Vector3 position = targetPos[index];
            Vector3 startPos = position + new Vector3(-750, 0, 0);

            GameObject node = Instantiate(targetPrefab, startPos, Quaternion.identity, targetsNode);
            node.transform.SetSiblingIndex(10 - index);

            node.name = $"target{index}";

            Target targetComp = node.GetComponent<Target>();
            targetComp.InitWithType(type, () =>
            {
                AnimateTargetEntry(node, position, targetComp);
            });
            targetComp.posIndex = index;

            targets.Add(targetComp);
        }

        private void AnimateTargetEntry(GameObject node, Vector3 position, Target targetComp)
        {
            node.transform.DOMove(position, TARGET_MOVE_DUR)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    targetComp.state = (int)TargetState.Ready;
                    CheckTemporarySlot();
                    CheckGameOver("HandleTargetGeneration");
                });
        }

        private void HandleRopeAnimation(Vector3[] points, RopeTexture ropeTexture)
        {
            float offsetTime = 0.05f;
            for (int i = 0; i < points.Length; i++)
            {
                float dTime = i * offsetTime;
                Vector2 tmpPoint = points[i];
                // 执行移动
                ropeTexture.transform.DOScaleZ(1,0).SetDelay(dTime).OnComplete(() =>
                {
                    ropeTexture.MoveToTarget(tmpPoint.x, tmpPoint.y, offsetTime);
                });
            }
        }

        private Target FindMatchingTarget(RopeBase obj, bool checkReadyState = true)
        {
            return targets.FirstOrDefault(target =>
                target.targetCount > 0 &&
                target.type == obj.type &&
                (!checkReadyState || target.state == (int)TargetState.Ready));
        }

        /** 添加到木桩*/
        private void FillOrder(RopeBase screwObj, Target targetComp, float amplitude = 20)
        {
            bool ret = targetComp.Sub();
            bool isFinish = targetComp.IsFinish();
            OperateObjAnimation(screwObj, targetComp.gameObject, () =>
            {
                if (isFinish)
                {
                    CheckTargetFinish(targetComp);
                }
                else
                {
                    CheckTemporarySlot();
                }
            }, amplitude);
        }

        private RopeTexture CreateRopeNode(GameObject startNode, int type, float amplitude = 20)
        {
            GameObject ropeNode = Instantiate(ropePrefab, levelRoot);
            ropeNode.transform.SetAsLastSibling();

            RopeTexture ropeController = ropeNode.GetComponent<RopeTexture>();
            ropeController.type = type;
            ropeController.amplitude = amplitude;
            ropeController.UpdateEndPoint(0, 0);

            //Vector3 startLocal = ConvertToNodeSpaceAR(startNode, ropeNode.transform.parent);
            //ropeNode.transform.position = startLocal;
            ropeNode.transform.position = startNode.transform.position;

            return ropeController;
        }

        private void CheckTargetFinish(Target targetComp)
        {
            Debug.Log($"checkTargetFinish {targetComp.targetCount}");
            if (targetComp.IsFinish())
            {
                Debug.Log("finish");
                targets.Remove(targetComp);
                RopeTexture ropeController = CreateRopeNode(targetComp.gameObject, targetComp.type);
                bool isFinished = false;
                var result = paintBoard.DrawWithColor(targetComp.type, (pos, isLast) =>
                {
                    Vector3 tempPos = paintBoard.GetCellPosition(pos.x, pos.y);
                    Vector3 worldPos = paintBoard.gridContainer.TransformPoint(tempPos);
                    ropeController.MoveToTarget(worldPos.x, worldPos.y, 0.04f);
                    SoundManager.Ins.PlaySfx("merge_4");
                    SoundManager.Ins.vibrate();
                    if (isLast)
                    {
                        ropeController.DestroyByReset();
                        if (isFinished)
                        {
                            DOVirtual.DelayedCall(0.5f, () => feibiao.Pass());
                        }
                    }
                });
                isFinished = result.isFinished;
                progressBar.fillAmount = GetProgress();
                progressLabel.text = $"{(GetProgress() * 100).ToString("F0")}%";

                targetComp.ShowFinishAnimation(result.duration, () =>
                {
                    targetComp.transform.DOLocalMoveX(-750, TARGET_MOVE_DUR)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() =>
                        {
                            SetTargetWithStrategy();
                            int nextData = GetNextTarget();
                            if (nextData >= 0)
                            {
                                SoundManager.Ins.PlaySfx("biaobagenghuan2");
                                HandleTargetGeneration(targetComp.posIndex, nextData);
                            }
                            else
                            {
                                CheckGameOver("checkTargetFinish");
                                Debug.Log("所有目标已经出完");
                            }
                            Destroy(targetComp.gameObject);
                        });
                });
            }
        }

        public float GetProgressLimit()
        {
            return feibiao.level <= 2 ? 0.8f : 0.5f;
        }

        private void SetTargetWithStrategy()
        {
            if (GetProgress() < GetProgressLimit() && UnityEngine.Random.value > 0.7f)
            {
                SetTargetWithTouchEnabledMore();
            }
            else
            {
                SetTargetWithTopBoardMore(3);
            }
        }

        private void CheckTemporarySlot()
        {
            for (int i = 0; i < tmpNodeMgr.activeSlotCount; i++)
            {
                TmpRope tmp = tmpNodeMgr.tmpContents[i];
                if (tmp != null)
                {
                    Target targetComp = FindMatchingTarget(tmp);
                    if (!tmp.isAnimated && targetComp != null)
                    {
                        tmpNodeMgr.tmpContents[i] = null;
                        FillOrder(tmp, targetComp, 5);
                        return;
                    }
                }
            }

            for (int k = tmpNodeMgr.hideTmpNode.childCount - 1; k >= 0; k--)
            {
                Transform tmp = tmpNodeMgr.hideTmpNode.GetChild(k);
                TmpRope screwComp = tmp.GetComponent<TmpRope>();
                Target targetComp = FindMatchingTarget(screwComp);
                if (targetComp != null)
                {
                    FillOrder(screwComp, targetComp, 5);
                    return;
                }
            }
        }

        private Target IsAnyMatched()
        {
            for (int i = 0; i < tmpNodeMgr.activeSlotCount; i++)
            {
                TmpRope tmp = tmpNodeMgr.tmpContents[i];
                if (tmp != null)
                {
                    Target targetComp = FindMatchingTarget(tmp, false);
                    if (targetComp != null)
                    {
                        Debug.Log($"isAnyMatched1 {targetComp.type} {tmp.type}");
                        return targetComp;
                    }
                }
            }

            for (int k = tmpNodeMgr.hideTmpNode.childCount - 1; k >= 0; k--)
            {
                Transform tmp = tmpNodeMgr.hideTmpNode.GetChild(k);
                TmpRope screwComp = tmp.GetComponent<TmpRope>();
                Target targetComp = FindMatchingTarget(screwComp, false);
                if (targetComp != null)
                {
                    Debug.Log($"isAnyMatched2 {targetComp.type} {screwComp.type}");
                    return targetComp;
                }
            }

            return null;
        }

        private int GetNextTarget()
        {
            List<int> tmp = targets.Select(t => t.type).ToList();
            for (int i = 0; i < data.Count; i++)
            {
                int element = data[i];
                if (!tmp.Contains(element))
                {
                    data.RemoveAt(i);
                    return element;
                }
            }

            if (data.Count <= 0)
            {
                return -1;
            }
            int result = data[0];
            data.RemoveAt(0);
            return result;
        }

        private int SetTargetWithTmpMore()
        {
            List<int> arr = new List<int>();
            for (int i = 0; i < tmpNodeMgr.activeSlotCount; i++)
            {
                TmpRope tmp = tmpNodeMgr.tmpContents[i];
                if (tmp != null)
                {
                    arr.Add(tmp.type);
                }
            }

            if(arr.Count == 0)
            {
                return -1;
            }
            var result = FindMostFrequentElement(arr.ToArray());
            if (result.Length <= 0)
            {
                return -1;
            }
            int targetType = result[0];
            for (int i = 0; i < data.Count; i++)
            {
                int element = data[i];
                if (element == targetType)
                {
                    SwapArrayElements(data, 0, i);
                    return element;
                }
            }
            return -1;
        }

        private int SetTargetWithTopBoardMore(int layerCount = 2)
        {
            List<int> tmpTypes = new List<int>();
            for (int i = 0; i < tmpNodeMgr.activeSlotCount; i++)
            {
                TmpRope tmp = tmpNodeMgr.tmpContents[i];
                if (tmp != null)
                {
                    tmpTypes.Add(tmp.type);
                }
            }

            List<int> topObjs = gamePlay.GetTopObjs(layerCount);
            List<int> allTypes = new List<int>();
            allTypes.AddRange(tmpTypes);
            allTypes.AddRange(topObjs);

            var result = FindMostFrequentElement(allTypes.ToArray());
            if (result.Length <= 0)
            {
                return -1;
            }
            int targetType = result[0];
            for (int i = 0; i < data.Count; i++)
            {
                int element = data[i];
                if (element == targetType)
                {
                    SwapArrayElements(data, 0, i);
                    return element;
                }
            }
            return -1;
        }

        private int SetTargetWithTopBoardHas(int layerCount = 2)
        {
            List<int> topObjs = gamePlay.GetTopObjs(layerCount);
            if (topObjs.Count <= 0)
            {
                return -1;
            }
            int randomIndex = UnityEngine.Random.Range(0, topObjs.Count);
            int targetType = topObjs[randomIndex];
            for (int i = 0; i < data.Count; i++)
            {
                int element = data[i];
                if (element == targetType)
                {
                    SwapArrayElements(data, 0, i);
                    return element;
                }
            }
            return -1;
        }

        private int SetTargetWithTouchEnabledMore()
        {
            List<int> tmpTypes = new List<int>();
            for (int i = 0; i < tmpNodeMgr.activeSlotCount; i++)
            {
                TmpRope tmp = tmpNodeMgr.tmpContents[i];
                if (tmp != null)
                {
                    tmpTypes.Add(tmp.type);
                }
            }

            List<int> topObjs = gamePlay.GetTouchedObjs();
            List<int> allTypes = new List<int>();
            allTypes.AddRange(tmpTypes);
            allTypes.AddRange(topObjs);

            var result = FindMostFrequentElement(allTypes.ToArray());
            if (result.Length <= 0)
            {
                return -1;
            }
            int targetType = result[0];
            for (int i = 0; i < data.Count; i++)
            {
                int element = data[i];
                if (element == targetType)
                {
                    SwapArrayElements(data, 0, i);
                    return element;
                }
            }
            return -1;
        }

        public void PushObjToTemp(Rope operateObj)
        {
            int index = tmpNodeMgr.GetEmptyIndex();
            if (index == -1)
            {
                return;
            }
            TmpRope tmpComp = tmpNodeMgr.PushObjToTemp(operateObj, index);

            if (tmpNodeMgr.GetFreeSlotCount() == 1)
            {
                ShowSlotNotEnough();
            }
            OperateObjAnimation(operateObj, tmpComp.gameObject, () =>
            {
                tmpComp.isAnimated = false;
                CheckTemporarySlot();
                CheckGameOver("onRemoveScrew");
            });
        }

        private void ShowSlotNotEnough()
        {
            UIManager.Instance.ShowMsg("Only one slot left");
            /*slotNotEnoughLabel.gameObject.transform.DOKill();
            slotNotEnoughLabel.gameObject.SetActive(true);

            CanvasGroup canvasGroup = slotNotEnoughLabel.gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = slotNotEnoughLabel.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0;
            // 创建动画序列
            Sequence sequence = DOTween.Sequence();
            // 1. 渐显（0.2秒）
            sequence.Append(
                canvasGroup.DOFade(1f, 0.2f)
            );
            // 2. 闪烁2次（每次0.5秒）
            for (int i = 0; i < 2; i++)
            {
                sequence.Append(
                    canvasGroup.DOFade(0.3f, 0.5f / 2) // 变暗
                );
                sequence.Append(
                    canvasGroup.DOFade(1f, 0.5f / 2)  // 恢复
                );
            }
            // 3. 0.2秒后渐隐
            sequence.Append(
                canvasGroup.DOFade(0f, 0.2f)
            );
            // 可选：动画结束时销毁节点
            sequence.OnComplete(() =>
            {
                slotNotEnoughLabel.gameObject.SetActive(false);
            });*/
        }

        public bool IsTmpFull()
        {
            return tmpNodeMgr.IsFull();
        }

        private Target IsOrderAnimating()
        {
            foreach (Target element in targets)
            {
                if (element.state == (int)TargetState.Loading)
                {
                    return element;
                }
            }

            foreach (Target element in targets)
            {
                if (element.targetCount == 0)
                {
                    return element;
                }
            }
            return null;
        }

        private void CheckGameOver(string str)
        {
            Debug.Log($"checkGameOver {str}");
            if (!IsTmpFull())
            {
                Debug.Log("还有空槽");
                return;
            }

            if (data.Count > 0 && targets.Count < unlockCount)
            {
                Debug.Log("还有未结束的目标");
                return;
            }

            if (IsOrderAnimating() != null)
            {
                Debug.Log("还有订单动画");
                return;
            }

            if (IsAnyMatched() != null)
            {
                Debug.Log("还有匹配的");
                return;
            }

            feibiao.GameOver();
        }

        public void OnTouchOperateObj(Rope params_)
        {
            if (IsTmpFull())
            {
                UIManager.Instance.ShowMsg("The slot is full and cannot place any more");
                return;
            }
            Target comp = FindMatchingTarget(params_);
            if (comp == null)
            {
                PushObjToTemp(params_);//临时区
            }
            else
            {
                FillOrder(params_, comp);//指定区
            }
        }

        public bool CanUnlock()
        {
            return unlockCount >= MAX_TARGET;
        }

        private void OnUnlock(EventStruct evt)
        {
            Debug.Log("onUnlock");
            if (unlockCount >= MAX_TARGET)
            {
                return;
            }

            int left = MAX_TARGET - unlockCount;
            Transform unlockNode = targetsNode.Find($"unlock_{left}");
            unlockNode.gameObject.SetActive(false);

            SetTargetWithTmpMore();
            SetEasyCountMax();

            int nextData = GetNextTarget();
            if (nextData >= 0)
            {
                HandleTargetGeneration(unlockCount, nextData);
            }
            else
            {
                Debug.Log("所有目标已经出完");
            }

            SoundManager.Ins.PlaySfx("jiesuo");
            unlockCount++;
        }

        private void OnUnlockTmp(EventStruct evt)
        {
            tmpNodeMgr.OnUnlockTmp();
        }

        private void OnRemoveBoard(EventStruct evt)
        {
            Board board = (Board)evt.target;
            board.RemoveHingeJoint();
            Transform objsNode = board.objsNode.transform;
            for (int i = objsNode.childCount - 1; i >= 0; i--)
            {
                Transform element = objsNode.GetChild(i);
                Rope comp = element.GetComponent<Rope>();
                comp.transform.SetParent(null);
                tmpNodeMgr.PushToHideTmp(comp);
            }
            CheckTemporarySlot();
        }

        private void OnClearTmp(EventStruct evt)
        {
            DOTween.To(() => 0, x => { }, 0, 0.5f).OnComplete(() =>
            {
                tmpNodeMgr.ClearTmp(() =>
                {
                    SetEasyCountMax();
                    SoundManager.Ins.PlaySfx("dianji02");
                });
            });
        }

        public bool CanClear()
        {
            return tmpNodeMgr.CanClear();
        }

        private void SetEasyCountMax()
        {
            easyCount = feibiao.level <= 8 ? EASY_MAX_COUNT : 2;
        }

        private void OperateObjAnimation(RopeBase operateObj, GameObject tmpNode, Action callback, float amplitude = 20)
        {
            operateObj.gameObject.transform.DOKill();
            RopeTexture ropeController = CreateRopeNode(operateObj.gameObject, operateObj.type, amplitude);

            Vector3 endPos = tmpNode.transform.position;
            Vector3[] points = new Vector3[]
            {
            new Vector3(endPos.x - 20, endPos.y, 0),
            new Vector3(endPos.x, endPos.y + 20, 0),
            new Vector3(endPos.x, endPos.y - 20, 0),
            new Vector3(endPos.x + 10, endPos.y + 20, 0),
            new Vector3(endPos.x + 10, endPos.y - 20, 0),
            new Vector3(endPos.x + 20, endPos.y + 20, 0),
            new Vector3(endPos.x + 20, endPos.y - 20, 0)
            };

            HandleRopeAnimation(points, ropeController);

            operateObj.RemoveFromBoard();
            operateObj.transform.position = ropeController.transform.position;
            operateObj.transform.SetParent(levelRoot);
            operateObj.transform.SetAsLastSibling();
            operateObj.MoveStart(() =>
            {
                int randomNum = UnityEngine.Random.Range(1, 4);
                SoundManager.Ins.PlaySfx($"merge_{randomNum}");
                ropeController.DestroyByReset();
                //operateObj.transform.SetParent(null);
                callback?.Invoke();
                operateObj.Dispose();
            });
        }

        private static int[] FindMostFrequentElement(int[] array)
        {
            var countMap = array.GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count());

            int maxCount = countMap.Values.Max();
            return countMap.Where(kvp => kvp.Value == maxCount)
                .Select(kvp => kvp.Key)
                .ToArray();
        }

        private static void SwapArrayElements<T>(List<T> arr, int indexA, int indexB)
        {
            T temp = arr[indexA];
            arr[indexA] = arr[indexB];
            arr[indexB] = temp;
        }
    }
}

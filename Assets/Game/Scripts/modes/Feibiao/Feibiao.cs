using Assets.Scripts.common;
using Assets.Scripts.config;
using Assets.Scripts.data;
using Assets.Scripts.Events;
using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Assets.Game.Scripts.modes.Feibiao.PaintBoard;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class Feibiao : MonoBehaviour
    {
        [SerializeField] public TargetMgr targetMgr;
        [SerializeField] private GameUILayer gameUILayer;
        [SerializeField] private EffectLayer effectLayer;
        //[SerializeField] private GameObject xinshouyindaoPrefab;
        //[SerializeField] private GameObject fuhuoNode;
        [SerializeField] private GameObject paintNode;
        [SerializeField] private TextMeshProUGUI levelText;
        //[SerializeField] private TextMeshProUGUI reviveText;

        private GameObject nowGuanqiaNode;
        private GameObject xinshouyindao;
        [HideInInspector] public int level = 1;
        private float lianjitime = 3f;
        private int lianjiNumber = 0;

        // Use this for initialization
        void Start()
        {
            Debug.Log("feibiao - start -------------");
            //Canvas canvas = GetComponent<Canvas>();
            //canvas.fitHeight = UserService.Instance.IsIPAD;
            //canvas.fitWidth = !UserService.Instance.IsIPAD;
            EventMng.addEventListener(EventTypes.BTN_CLEAR_TMP, ShowSaoba);

            SoundManager.Ins.PlayMusic("bgm");
            SetLevel();
            LoadLevel(level);
            string lvName = level < 1 ? "New Guide" : $"Level  {level}";
            levelText.text = lvName;

            /*if (Application.isEditor && Debug.isDebugBuild)
            {
                transform.Find("testNode").gameObject.SetActive(true);
            }*/

            //PlatformService.Instance.StartGameRecording(new GameRecordingData
            //{
            //    Duration = 300,
            //    IsMarkOpen = true
            //});
            //UserService.Instance.SetLastEnterScene("MainPlay");
        }

        private void SetLevel()
        {
            GameMode modeID = GameManager.Instance.currMode.id;
            switch (modeID)
            {
                case GameMode.NewYear:
                    //var newYearData = UserService.Instance.GetNewYearThemeData();
                    //level = newYearData.Level;
                    break;
                case GameMode.ShengXiao:
                    //var shengXiaoData = UserService.Instance.GetShengXiaoThemeData();
                    //level = shengXiaoData.Level;
                    break;
                case GameMode.NeZha:
                    //var neZhaData = UserService.Instance.GetNeZhaThemeData();
                    //level = neZhaData.Level;
                    break;
                case GameMode.NvShen:
                    //var nvShenData = UserService.Instance.GetNvShenThemeData();
                    //level = nvShenData.Level;
                    break;
                case GameMode.Spring:
                    //var springData = UserService.Instance.GetSpringThemeData();
                    //level = springData.Level;
                    break;
                case GameMode.FiveOne:
                    //var fiveOneData = UserService.Instance.GetFiveOneThemeData();
                    //level = fiveOneData.Level;
                    break;
                default:
                    level = UserModel.Instance.level;
                    break;
            }
        }

        private void SetReviveText()
        {
            if (level == 0)
            {
                //reviveText.text = "99.99%人通过此关";
            }
            else if (level <= 100)
            {
                float percent = 99.99f - (level / 100f) * (99.99f - 50f);
                //reviveText.text = $"{percent:F2}%人通过此关";
            }
            else
            {
                //reviveText.text = $"复活 {gameUILayer.freeNumber} 次";
            }
        }

        private IEnumerator LoadingLevel()
        {
            //ReportAnalytics("game_enter");
            effectLayer.ShowLevelupEffect(level);

            yield return null; // 等待一帧
            /*if (level == 0)
            {
                xinshouyindao = Instantiate(xinshouyindaoPrefab);
                Debug.Log("引导");
                    Transform zuishangceng = nowGuanqiaNode.transform.GetChild(nowGuanqiaNode.transform.childCount - 1);
                    Transform qiqiu = targetMgr.TargetsNode.GetChild(0);
                    Transform zancun = transform.Find("tmpNode/tmp_layout");

                    var xinshouyindaoComp = xinshouyindao.GetComponent<Xinshouyindao>();
                    xinshouyindaoComp.NodeArr = new Transform[]
                    {
                    qiqiu,
                    zuishangceng.GetChild(zuishangceng.childCount - 1),
                    zancun,
                    zuishangceng.GetChild(zuishangceng.childCount - 1)
                    };

                    EventManager.Instance.AddListener(EventType.DirectorEvent.XinshouyindaoEnd, (e) =>
                    {
                        if (gameObject && xinshouyindao)
                        {
                            xinshouyindao.transform.SetParent(null);
                        }
                    });

                    EventMng.addEventListener(EventTypes.SCREW_REMOVE, EmitXinshouyindao);

                    xinshouyindaoComp.Str = new string[]
                    {
                    "先观察订单颜色",
                    "点击对应颜色绳子",
                    "匹配不上颜色的绳子会暂时存放在这里",
                    "完成拼图则游戏胜利"
                    };
                    xinshouyindaoComp.Fx = new int[] { 2, 2, 2, 2, 2, 2 };
                    xinshouyindaoComp.Chumo = -999;
                    xinshouyindao.transform.SetParent(transform);
                    xinshouyindao.transform.SetSiblingIndex(13);
                    xinshouyindao.SetActive(true);
                
            }*/
        }

        private void EmitXinshouyindao()
        {
            //EventManager.Instance.Dispatch(EventType.DirectorEvent.Xinshouyindao);
        }

        public void GameOver()
        {
            Debug.Log("over");

            bool can = targetMgr.CanUnlock();
            var data = new ReviveVo { };
            data.unlock = can;
            data.pro = GetProgress();
            data.action = (isRevive) =>
            {
                if (isRevive)
                {
                    Huode();
                }
                else
                {
                    Buxuyao();
                }
            };
            DOVirtual.DelayedCall(1.5f, () =>
            {
                UIManager.Instance.OpenView(VIEW_NAME.ReviveDlg, VIEW_TYPE.dialog, false, null, data);
            });
            UIManager.Instance.ShowMsg("槽位已满!");
        }

        private float GetProgress()
        {
            return targetMgr.GetProgress();
        }

        public void Buxuyao()
        {
            OverPage();
        }

        public void Huode()
        {
            bool can = targetMgr.CanUnlock();

            if (gameUILayer.freeNumber > 0)
            {
                if (!can)
                {
                    EventMng.dispatchEvent(new EventStruct(EventTypes.UNLOCK_2), null);
                }
                else
                {
                    EventMng.dispatchEvent(new EventStruct(EventTypes.BTN_CLEAR_TMP), null);
                }

                foreach (var node in gameUILayer.mianfeiNode)
                {
                    node.SetActive(false);
                }
                gameUILayer.freeNumber--;
                UserModel.Instance.propNum = 0;
                return;
            }

            AdManager.Ins.ShowAd(0, (isSuc) =>
            {
                if (isSuc)
                {
                    if (!can)
                    {
                        EventMng.dispatchEvent(new EventStruct(EventTypes.UNLOCK_2), null);
                    }
                    else
                    {
                        EventMng.dispatchEvent(new EventStruct(EventTypes.BTN_CLEAR_TMP), null);
                    }
                }
            });
        }

        private void OverPage()
        {
            var data = new OverPageData
            {
                level = level,
                gameProgress = GetProgress(),
                paintNode = paintNode
            };
            SoundManager.Ins.PlaySfx("shibai");
            //UIService.Instance.ShowOverPage(new OverPageData { pass = false, data = data });
            UIManager.Instance.OpenView(VIEW_NAME.FailDlg, VIEW_TYPE.dialog);
        }

        public void Pass()
        {
            Debug.Log("pass");

            //ReportAnalytics("game_level_pass");
            UpdateData();

            SoundManager.Ins.PlaySfx("succes");
            UIManager.Instance.OpenView(VIEW_NAME.WinDlg, VIEW_TYPE.dialog);

            /*Action showOverPageA = () =>
            {
                 SoundManager.Ins.PlaySfx("succes");
                UIService.Instance.ShowOverPage(new OverPageParams
                {
                    Pass = true,
                    Data = new OverPageData
                    {
                        Level = level,
                        GameProgress = 1,
                        PaintNode = paintNode,
                        YarnCurrency = GetYarnReward()
                    }
                });
            };

            Action showOverPageB = () =>
            {
                 SoundManager.Ins.PlaySfx("succes");
                UIService.Instance.ShowPage("Feibiao", "GameOverPage", new GameOverPageData
                {
                    PaintNode = paintNode,
                    YarnCurrency = GetYarnReward(),
                    Level = level
                });
            };

            if (GameManager.Instance.CurrentMode.Id != GameMode.Feibiao)
            {
                showOverPageA();
                return;
            }

            Action completeCb = () => showOverPageB();
            bool isUnlocked = DecorateDollManager.Instance.UnlockDoll(new UnlockDollParams
            {
                Type = DollUnlockScene.Zhuxian,
                Num = level,
                IsEqual = true,
                CompleteCallback = completeCb
            });
            Debug.Log($"isUnlocked {isUnlocked}");

            if (!isUnlocked)
            {
                completeCb();
            }*/
        }

        private void UpdateData()
        {
            GameMode modeID = GameManager.Instance.currMode.id;
            switch (modeID)
            {
                case GameMode.NewYear:
                    //var newYearData = UserService.Instance.GetNewYearThemeData();
                    //newYearData.Level++;
                    //UserService.Instance.SetNewYearThemeData(newYearData);
                    break;
                case GameMode.ShengXiao:
                    //var shengXiaoData = UserService.Instance.GetShengXiaoThemeData();
                    //shengXiaoData.Level++;
                    //UserService.Instance.SetShengXiaoThemeData(shengXiaoData);
                    break;
                case GameMode.NeZha:
                    //var neZhaData = UserService.Instance.GetNeZhaThemeData();
                    //neZhaData.Level++;
                    //UserService.Instance.SetNeZhaThemeData(neZhaData);
                    break;
                case GameMode.NvShen:
                    //var nvShenData = UserService.Instance.GetNvShenThemeData();
                    //nvShenData.Level++;
                    //UserService.Instance.SetNvShenThemeData(nvShenData);
                    break;
                case GameMode.Spring:
                    //var springData = UserService.Instance.GetSpringThemeData();
                    //springData.Level++;
                    //UserService.Instance.SetSpringThemeData(springData);
                    break;
                case GameMode.FiveOne:
                    //var fiveOneData = UserService.Instance.GetFiveOneThemeData();
                    //fiveOneData.Level++;
                    //UserService.Instance.SetFiveOneThemeData(fiveOneData);
                    break;
                default:
                    int level = UserModel.Instance.level;
                    if (level > ConstVal.ROPE_MAX_LEVEL)
                    {
                        UserModel.Instance.selectedPainting = ConstVal.ROPE_MAX_LEVEL;
                    }
                    else
                    {
                        UserModel.Instance.selectedPainting = level;
                    }
                    level++;
                    UserModel.Instance.level = level;
                    //PlatformService.Instance.SetUserData(new UserData { Level = feibiaoData.Level });

                    /*int yarn = GetYarnReward();
                    if (yarn > 0)
                    {
                        int yarnData = UserService.Instance.GetYarnCurrency();
                        yarnData += yarn;
                        UserService.Instance.SetYarnCurrency(yarnData);
                    }*/
                    break;
            }
        }

        private int GetYarnReward()
        {
            if (GameManager.Instance.currMode.id != GameMode.Feibiao)
            {
                return 0;
            }
            if (level < 1)
            {
                return 0;
            }
            /*bool isMaxLevel = DecorateRoomManager.Instance.IsMaxLevelId();
            Debug.Log($"isMaxLevel {isMaxLevel}");
            if (isMaxLevel)
            {
                return 0;
            }*/
            return targetMgr.totalTargetCount * 3;
        }

        private void PreloadLevel(int level)
        {
            if (GameManager.Instance.currMode.id != GameMode.Feibiao)
            {
                return;
            }
            try
            {
                string resId = LevelMgr.GetPrefabName(level);
                string bundleName = LevelMgr.GetBundleName(level);
                /*ResourceLoader.Instance.Load<GameObject>(bundleName, $"prefabs/levels/lv_{resId}")
                    .ContinueWith(_ => Debug.Log($"preload success {resId}"));*/
            }
            catch (Exception err)
            {
                Debug.LogError($"preload error {err}");
            }
        }

        public void Addlianji()
        {
            lianjiNumber++;
            Transform text = transform.Find("lianjishu/lianji");
            //LeanTween.cancel(text.gameObject);
            text.gameObject.transform.DOKill();
            text.localScale = Vector3.one;
            text.GetComponent<TextMeshProUGUI>().text = $"x{lianjiNumber}";
            text.gameObject.transform.DOScale(Vector3.one * 1.2f, 0.2f)
                .SetLoops(1, LoopType.Yoyo)
                .OnComplete(() => text.gameObject.transform.DOScale(Vector3.one, 0.1f));

            Transform lianjishu = transform.Find("lianjishu");
            if (lianjishu && lianjiNumber > 1)
            {
                lianjishu.gameObject.SetActive(true);

                /*Slider jindu = lianjishu.Find("pb").GetComponent<Slider>();
                LeanTween.cancel(jindu.gameObject);
                jindu.value = 1;
                LeanTween.value(jindu.gameObject, 1f, 0f, lianjitime)
                    .setOnUpdate((float val) => jindu.value = val);*/
            }
            CancelInvoke(nameof(Congzhilianji));
            Invoke(nameof(Congzhilianji), lianjitime);
        }

        private void ShowSaoba(EventStruct evt)
        {
            Transform saoba = targetMgr.transform.Find("saoba");
            saoba.gameObject.SetActive(true);
            saoba.localPosition = new Vector3(50, 0, 0);
            SoundManager.Ins.PlaySfx("saoba");
            saoba.gameObject.transform.DOLocalMoveX(50, 0.5f)
                .OnComplete(() => saoba.gameObject.SetActive(false));
        }

        private void Congzhilianji()
        {
            lianjiNumber = 0;
            transform.Find("lianjishu").gameObject.SetActive(false);
        }

        public async Task LoadLevel(int rlevel)
        {
            //string resId = LevelMgr.GetPrefabName(rlevel);
            //Debug.Log($"loadLevel {rlevel} {resId}");
            UIManager.Instance.ShowLoading();
            try
            {
                GameObject prefab = await ResourceManager.AsyncLoadRes<GameObject>($"Game/Levels/prefabs/lv_{rlevel}.prefab");
                TextAsset data = await ResourceManager.AsyncLoadRes<TextAsset>($"Game/Levels/json/lv_{rlevel}.json");

                GamePlay gameplay = GetComponent<GamePlay>();
                GameObject prefabNode = Instantiate(prefab, gameplay.levelRoot);

                gameplay.boardLayer = prefabNode.transform;
                GridDataWrapper gridData = JsonConvert.DeserializeObject<GridDataWrapper>(data.text);
                StartCoroutine(gameplay.InitGame(gridData, rlevel));

                nowGuanqiaNode = prefabNode;
                gameUILayer.InitFeibiao(this);

                SceneManager.sceneLoaded += (scene, mode) =>
                {
                    Debug.Log("EVENT_BEFORE_SCENE_LAUNCH");
                    Destroy(prefab);
                    Resources.UnloadAsset(data);
                };

                StartCoroutine(LoadingLevel());
            }
            catch (Exception err)
            {
                Debug.LogError($"loadLevel error {err}");
                //UIService.Instance.ShowMessage("关卡加载失败,请稍后再试");
            }
            finally
            {
                //UIService.Instance.HideLoading();
                UIManager.Instance.HideLoading();
            }
        }

        public void OnEditEnd(string edit)
        {
            if (!int.TryParse(edit, out int lv) || lv < 0)
            {
                //UIService.Instance.ShowMessage("请输入有效的关卡数值");
                return;
            }

            GameMode currentMode = GameManager.Instance.currMode.id;
            switch (currentMode)
            {
                case GameMode.NewYear:
                    //var newYearData = UserService.Instance.GetNewYearThemeData();
                    //newYearData.Level = lv;
                    //UserService.Instance.SetNewYearThemeData(newYearData);
                    break;
                case GameMode.ShengXiao:
                    //var shengXiaoData = UserService.Instance.GetShengXiaoThemeData();
                    //shengXiaoData.Level = lv;
                    //UserService.Instance.SetShengXiaoThemeData(shengXiaoData);
                    break;
                case GameMode.NeZha:
                    //var neZhaData = UserService.Instance.GetNeZhaThemeData();
                    //neZhaData.Level = lv;
                    //UserService.Instance.SetNeZhaThemeData(neZhaData);
                    break;
                case GameMode.NvShen:
                    //var nvShenData = UserService.Instance.GetNvShenThemeData();
                    //nvShenData.Level = lv;
                    //UserService.Instance.SetNvShenThemeData(nvShenData);
                    break;
                case GameMode.Spring:
                    //var springData = UserService.Instance.GetSpringThemeData();
                    //springData.Level = lv;
                    //UserService.Instance.SetSpringThemeData(springData);
                    break;
                case GameMode.FiveOne:
                    //var fiveOneData = UserService.Instance.GetFiveOneThemeData();
                    //fiveOneData.Level = lv;
                    //UserService.Instance.SetFiveOneThemeData(fiveOneData);
                    break;
                default:
                    UserModel.Instance.level = lv;
                    break;
            }
            GameManager.Instance.EnterMode(GameManager.Instance.currMode.id);
        }
    }
}
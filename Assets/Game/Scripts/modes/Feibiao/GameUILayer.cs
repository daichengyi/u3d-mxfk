using Assets.Scripts.common;
using Assets.Scripts.Events;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class GameUILayer: MonoBehaviour
    {
        [SerializeField] private GameObject gamePlayNode;
        [SerializeField] private TextMeshProUGUI layersLabel;
        [SerializeField] private TextMeshProUGUI boardsLabel;
        [SerializeField] private TextMeshProUGUI holesLabel;
        [SerializeField] public int freeNumber = 1;
        [SerializeField] public List<GameObject> mianfeiNode = new List<GameObject>();
        //[SerializeField] private GameObject workBtn;
        //[SerializeField] private GameObject joinGameClubNode;
        //[SerializeField] private SkeletonAnimation skinSke;
        //[SerializeField] private TextMeshProUGUI unlockDollLabel;

        private Feibiao gameScene;
        private GamePlay gamePlay;

        void Start()
        {
            //workBtn.SetActive(GameManager.Instance.currMode.id == GameMode.Feibiao);
            //joinGameClubNode.SetActive(Application.platform == RuntimePlatform.WebGLPlayer);
            LoadSpineAnimation();
        }
        private async void LoadSpineAnimation()
        {
            //skinSke.gameObject.transform.parent.gameObject.SetActive(false);
            if (GameManager.Instance.currMode.id != GameMode.Feibiao)
            {
                return;
            }

            int level = UserModel.Instance.level;
            /*DollConfig dollConfig = await DecorateDollMgr.Ins.GetMainPlayNextUnlockDollConfig(level);
            if (dollConfig == null)
            {
                return;
            }

            int offsetLevel = dollConfig.num - level;
            if (offsetLevel > 0)
            {
                unlockDollLabel.text = (offsetLevel + 1) + "关后可解锁";
            }
            else
            {
                unlockDollLabel.text = "通过本关解锁";
            }

            ResourceLoader.Instance
                .Load<SkeletonDataAsset>("skins", dollConfig.spine)
                .ContinueWith(task =>
                {
                    if (skinSke != null && skinSke.gameObject != null)
                    {
                        skinSke.gameObject.transform.parent.gameObject.SetActive(true);
                        skinSke.skeletonDataAsset = task.Result;
                        skinSke.AnimationState.SetAnimation(0, "idle", true);
                    }
                });*/
        }

        public void Init(GamePlay gamePlay)
        {
            this.gamePlay = gamePlay;
        }

        public void InitFeibiao(Feibiao feibiao)
        {
            InitUI();
            gameScene = feibiao;
            freeNumber = UserModel.Instance.propNum;
            if (freeNumber > 0 && gameScene.level <= 1 && GameManager.Instance.currMode.id == GameMode.Feibiao)
            {
                foreach (var node in mianfeiNode)
                {
                    node.SetActive(true);
                }
            }
            else
            {
                freeNumber = 0;
            }
        }

        private void InitUI()
        {
            if (Application.isEditor)
            {
                layersLabel.gameObject.SetActive(true);
                boardsLabel.gameObject.SetActive(true);
                holesLabel.gameObject.SetActive(true);
            }

            gamePlay = gamePlayNode.GetComponent<GamePlay>();
            layersLabel.text = "总层数：" + gamePlay.boardLayer.transform.childCount;
            int boardsCount = 0;
            int holesCount = 0;

            for (int i = 0; i < gamePlay.boardLayer.transform.childCount; i++)
            {
                Transform boardNodeLayer = gamePlay.boardLayer.transform.GetChild(i);
                boardsCount += boardNodeLayer.childCount;

                for (int j = 0; j < boardNodeLayer.childCount; j++)
                {
                    Board boardComp = boardNodeLayer.GetChild(j).GetComponent<Board>();
                    holesCount += boardComp.holesNode.transform.childCount;
                }
            }

            boardsLabel.text = "总板子数：" + boardsCount;
            holesLabel.text = "总洞数：" + holesCount;
        }

        public void OnProp(GameObject sender)
        {
            Dictionary<string, string> obj = new Dictionary<string, string>
            {
                { "btn_add_tmp", "加槽位" },
                { "btn_remove_board", "移除板子" },
                { "btn_clear_tmp", "清槽位" },
                { "unlock_2", "解锁订单位" },
                { "unlock_1", "解锁订单位" }
            };

            if (sender.name == "btn_clear_tmp" && !gameScene.targetMgr.CanClear())
            {
                UIManager.Instance.ShowMsg("No items required");
                return;
            }

            AdData data = new AdData
            {
                game_name = GameManager.Instance.currMode.explain,
                prop_name = obj[sender.name]
            };


            if (freeNumber > 0)
            {
                
                //EventManager.Instance.Emit(sender.name);
                EventMng.dispatchEvent(new EventStruct(sender.name),null);
                foreach (var node in mianfeiNode)
                {
                    node.SetActive(false);
                }
                freeNumber--;
                UserModel.Instance.propNum = 0;
                return;
            }

            AdManager.Ins.ShowAd(0, (bool isSuc) =>
            {
                EventMng.dispatchEvent(new EventStruct(sender.name), null);
            });
        }

        public void OnPintu()
        {
            //sers().uiSrv.ShowPage("Collection", "CollectionPage");
        }

        public void OnSkin()
        {
            //sers().uiSrv.ShowPage("decorateDoll", "decorate-doll-page");
        }

        public void OnBgSkin()
        {
            //sers().uiSrv.ShowPage("BgSkin", "BgSkinPage");
        }

        public void OnGameClub()
        {
            //sers().uiSrv.ShowPage("JoinGameClub", "JoinGameClubPage");
        }

        public void OnSetBtn()
        {
            UIManager.Instance.OpenView(VIEW_NAME.SetttingDlg, VIEW_TYPE.dialog);
        }
    }

    [Serializable]
    public class AdData
    {
        public string game_name;
        public string prop_name;
        public int? level;
    }
}
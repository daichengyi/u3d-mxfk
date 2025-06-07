using Assets.Game.Scripts.modes.Feibiao;
using Assets.Scripts.common;
using Assets.Scripts.manager;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Game.Scripts
{

    [Serializable]
    public class GameModeProperty
    {
        public GameMode id;
        /// <summary>
        /// 场景名
        /// </summary>
        public string sceneName;
        /// <summary>
        /// 所需资源的bundle名
        /// </summary>
        public string bundleName;
        /// <summary>
        /// 描述
        /// </summary>
        public string explain;
        /// <summary>
        /// 该模式的bundle
        /// </summary>
        public AssetBundle bundle;
        /// <summary>
        /// 视频解锁次数
        /// </summary>
        public int videoCnt;
        /// <summary>
        /// 是否在更多玩法页中 默认不展示
        /// </summary>
        public bool showInMore;
    }
    public class GameManager:SingletonBase<GameManager>
    {
        private GameModeProperty m_CurrMode;
        public GameModeProperty currMode => m_CurrMode;

        public int maxLevel;

        public async Task EnterMode(GameMode mode, bool showLoading = true)
        {
            RopeSegmentManager.Instance.ClearAllPools();
            if (showLoading)
            {
                UIManager.Instance.ShowLoading();
            }

            var currMode = GameModeJson.GetMode(mode);
            Debug.Log($"进入模式耗时---{currMode.explain}");

            m_CurrMode = currMode;

            if (currMode.bundle == null && !string.IsNullOrEmpty(currMode.bundleName))
            {
                Debug.Log($"加载模式所需资源bundle---{currMode.bundleName}");
                try
                {
                    /*currMode.bundle = await AssetBundle.LoadFromFileAsync(currMode.bundleName);
                    await SceneManager.LoadSceneAsync(currMode.sceneName);*/
                    await GoToNeedEnterMode();
                }
                catch (Exception e)
                {
                    Debug.LogError($"进入模式:{currMode.explain}, 加载所需bundle失败 {e.Message}");
                    throw;
                }
            }
            else
            {
                await GoToNeedEnterMode();
            }
        }

        private async Task GoToNeedEnterMode()
        {
            // UIService.Instance.CloseCurrentScenePage();
            //await SceneManager.LoadSceneAsync(m_CurrMode.sceneName);
            ResourceManager.LoadScene(m_CurrMode.sceneName);
            UIManager.Instance.HideLoading();
            Debug.Log($"进入模式耗时---{m_CurrMode.explain}");
        }

        public void BackHomePage()
        {
            m_CurrMode = null;
            //UIService.Instance.CloseCurrentScenePage();
            ResourceManager.LoadScene("Home");
            UIManager.Instance.HideLoading();
            Debug.Log("返回主页");
        }

        public bool IsInGame()
        {
            return m_CurrMode != null;
        }
    }
}

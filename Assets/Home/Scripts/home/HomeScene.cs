using Assets.Game.Scripts;
using Assets.Scripts.common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class HomeScene : MonoBehaviour
{

    private Dictionary<string, GameObject> _preloadedPrefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, AsyncOperationHandle<GameObject>> _loadingHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();
    private Dictionary<string, List<string>> _folderContents = new Dictionary<string, List<string>>();

    void Start()
    {
        UIManager.Instance.HideLoading();
        SoundManager.Ins.PlayMusic("bgm");
    }

    public void onBtnGame()
    {
        UIManager.Instance.ShowLoading();
       GameManager.Instance.EnterMode(GameMode.Feibiao, true);
    }

    public void onBtnSet()
    {
        Addressables.LoadAssetAsync<object>("Assets/Game/Levels/json/lv_0.json");
        Addressables.LoadAssetAsync<object>("Assets/Game/Levels/prefabs/lv_0.prefab");
        //UIManager.Instance.OpenView(VIEW_NAME.SetttingDlg, VIEW_TYPE.dialog);
    }
}

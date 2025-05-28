using Assets.Game.Scripts;
using Assets.Scripts.common;
using Assets.Scripts.Events;
using UnityEngine;

public class HomeScene : MonoBehaviour
{
    void Start()
    {
        // UIManager.Instance.HideLoading();
        EventMng.addEventListener(EventTypes.TEST_EVENT, evtHandle);
    }

    void evtHandle(EventStruct evt) { 
        Debug.Log("==========evt.ToString()");
    }
    void Update()
    {

    }
    public void onBtnGame()
    {
        UIManager.Instance.ShowLoading();
       GameManager.Instance.EnterMode(GameMode.Feibiao, true);
    }

    public void onBtnSet()
    {
        
        EventMng.dispatchEvent(new EventStruct(EventTypes.TEST_EVENT, "str========="), this);
        return;
        UIManager.Instance.OpenView(VIEW_NAME.Set);
    }
}

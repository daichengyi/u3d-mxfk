using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 检查Start场景是否已添加到构建设置
        if (Application.CanStreamedLevelBeLoaded("Start"))
        {
            SceneManager.LoadScene("Start");
        }
        else
        {
            Debug.LogError("Start场景未添加到Build Settings，无法跳转。");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

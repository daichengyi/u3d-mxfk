using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlatformSelect
{

    static List<string> platformList = new List<string>(){
        // 测试或web渠道
        "PF_WEB",
        // 微信渠道
        "PF_WX",
        // 头条渠道
        "PF_TT",
        // 快手渠道
        "PF_KS",
        // 快手渠道WASM（WebGL）版本
        "PF_KS_WASM",
    };

    [MenuItem("平台切换工具/查看当前平台")]
    public static void CheckPlatform()
    {   
        string[] curList;
        PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, out curList);
        string _platformInfo = "Editor";
        foreach (string platform in platformList)
        {
            if (curList.Contains(platform))
            {
                _platformInfo = platform;
                break;
            }
        }
        Debug.Log(_platformInfo);
    }

    [MenuItem("平台切换工具/切换平台/测试")]
    public static void SwitchPlatformTest()
    {
        SwitchPlatform("PF_WEB");
    }  

    [MenuItem("平台切换工具/切换平台/微信")]
    public static void SwitchPlatformWx()
    {
        SwitchPlatform("PF_WX");
    }   

    [MenuItem("平台切换工具/切换平台/头条")]
    public static void SwitchPlatformTT()
    {
        SwitchPlatform("PF_TT");
    }   

    [MenuItem("平台切换工具/切换平台/快手")]
    public static void SwitchPlatformKS()
    {
        SwitchPlatform("PF_KS");
    }   

    [MenuItem("平台切换工具/切换平台/快手WASM")]
    public static void SwitchPlatformKSWASM()
    {
        SwitchPlatform("PF_KS_WASM");
    }

    public static void SwitchPlatform(string platformName)
    {
        string[] curList;
        string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        curList = currentSymbols.Split(';');
        foreach (string platform in platformList)
        {   
            if (curList.Contains(platform))
            {
                currentSymbols = currentSymbols.Replace(";" + platform, "");
            }
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currentSymbols + ";" + platformName);
        Debug.Log("切换平台成功：" + platformName);
    }
}
using UnityEngine;
using System.Collections;

/// <summary>
/// 关卡的各个Field通用脚本
/// </summary>
public class StageField : MonoBehaviour {


    void Start()
    { 
    }

    // 加载结束后 LoadLevelAdditive不会调用OnLevelWasLoaded所以通过Awake处理
    //    void OnLevelWasLoaded(int level)
    void Awake()
    {
        Debug.Log("Stage Loaded");
        // 加载结束时的通知
        GameObject adapter = GameObject.Find("/Adapter");
        if (adapter) adapter.SendMessage("OnLoadedField");
        else Debug.Log("Adapter is not exist!!!");
    }
}

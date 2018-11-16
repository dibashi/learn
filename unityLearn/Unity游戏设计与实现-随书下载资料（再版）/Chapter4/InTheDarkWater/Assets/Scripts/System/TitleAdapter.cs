using UnityEngine;
using System.Collections;

/// <summary>
/// Title场景用的Adapter。
/// 向Root通知场景结束事件
/// </summary>
public class TitleAdapter : MonoBehaviour {

    private GameObject root = null;
    private GameObject ui = null;

    // 加载结束时
//    void OnLevelWasLoaded(int level)
//    {
//        Debug.Log("OnLevelWasLoaded : level=" + level + " - " + Application.loadedLevelName);
    // 加载结束后，由于无法单体调试所以直接Start
    void Start()
    {
        root = GameObject.Find("/Root");
        ui = GameObject.Find("/UI");

        if (root)
        {
            SetHighScore();
            root.BroadcastMessage("OnFadeOut", gameObject);
        }
        else OnIntermissionEnd();
    }

    // 过场显示结束后的处理
    void OnIntermissionEnd()
    {
        // 开始游戏（玩家可以操作乐）
        if(ui) ui.BroadcastMessage("OnStartSwitcher");
    }

    // 场景结束后调用
    void OnSceneEnd()
    {
        // 开始Stage
        if (root) root.SendMessage("OnStartStage");
    }

    private void SetHighScore()
    {
        int highScore = 0;
        SceneSelector selecter = root.GetComponent<SceneSelector>();
        if (selecter) highScore = selecter.HighScore();
        ui.BroadcastMessage("OnAddScore", highScore);
    }
}

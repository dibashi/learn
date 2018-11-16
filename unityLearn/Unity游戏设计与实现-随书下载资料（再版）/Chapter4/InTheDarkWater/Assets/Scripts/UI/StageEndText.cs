using UnityEngine;
using System.Collections;

/// <summary>
/// 关卡结束替换时显示的文字
/// </summary>
public class StageEndText : MonoBehaviour {

    [SerializeField]
    private float backtitleDelay = 3.0f;
    [SerializeField]
    private string gameclearText = "STAGE CLEAR";
    [SerializeField]
    private string gameoverText = "GAME OVER";

    void Start()
    {
    }

    // 游戏清空事件
    void OnGameClear()
    {
        GetComponent<GUIText>().text = gameclearText;
        StartCoroutine("Wait");
    }
    // 游戏结束事件
    void OnGameOver()
    {
        GetComponent<GUIText>().text = gameoverText;
        StartCoroutine("Wait");
    }
    // 重置关卡
    void OnStageReset()
    {
        GetComponent<GUIText>().enabled = false;
    }
    
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(backtitleDelay);
        GetComponent<GUIText>().enabled = true;
        BroadcastMessage("OnStartSwitcher");
    }

}

using UnityEngine;
using System.Collections;

/// <summary>
/// 显示得分
/// </summary>
public class ScoreDisplay : MonoBehaviour {

    [SerializeField]
    private bool offset = true;
    [SerializeField]
    private float offsetPixelY = 0.0f;  // 0表示靠近屏幕边缘
    [SerializeField]
    private int disitSize = 6;

    private int score = 0;

    void Start() 
    {
        // 调整位置
        if (offset)
        {
            float h = (float)Screen.height;
            float yPos = 1.0f - offsetPixelY / h;
            transform.position = new Vector3(0.5f, yPos, 0.0f);
        }
        GetComponent<GUIText>().text = score.ToString("D" + disitSize);
    }

    /// <summary>
    /// [BroadcastMessage]获取得分
    /// </summary>
    /// <param name="value">获得的积分</param>
    void OnAddScore( int value )
    {
        score += value;
        GetComponent<GUIText>().text = score.ToString("D" + disitSize);
        SendMessage("OnStartTextBlink", SendMessageOptions.DontRequireReceiver);
    }

    void OnEndTextBlink()
    {
        GetComponent<GUIText>().enabled = true;
    }

    public int Score() { return score; }
}

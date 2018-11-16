using UnityEngine;
using System.Collections;

/// <summary>
/// 文字闪烁处理
/// </summary>
public class TextBlinker : MonoBehaviour
{
    [SerializeField]
    private bool valid = true;
    [SerializeField]
    private float blinkTime = 0.8f;
    [SerializeField]
    private int num = 5;

    private int count = 0;

    void Start()
    {
    }

    // 开始执行文字闪烁
    void OnStartTextBlink()
    {
        if (GetComponent<GUIText>() == null || !valid) return;
        count = 0;
        GetComponent<GUIText>().enabled = true;
        StartCoroutine("Delay", blinkTime);
    }


    private IEnumerator Delay(float delaytime)
    {
        yield return new WaitForSeconds(delaytime);
        GetComponent<GUIText>().enabled = !GetComponent<GUIText>().enabled;
        count++;
        if (count < num)
        {
            StartCoroutine("Delay", blinkTime);
        }
        else
        {
            // 结束通知
            SendMessage("OnEndTextBlink", SendMessageOptions.DontRequireReceiver);
        }
    }
}

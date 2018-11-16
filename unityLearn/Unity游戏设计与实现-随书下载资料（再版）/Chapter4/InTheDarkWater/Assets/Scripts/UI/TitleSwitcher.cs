using UnityEngine;
using System.Collections;

/// <summary>
/// 用户点击后，向Adapter传递结束场景消息
/// </summary>
public class TitleSwitcher : MonoBehaviour {

    [SerializeField]
    private float waitTime = 3.0f;

    private bool pushed = false;
    private bool fade = false;
   
    void Start()
    {
        GetComponent<GUIText>().enabled = false;
        Color basecolor = GetComponent<GUIText>().material.color;
        GetComponent<GUIText>().material.color = new Color(basecolor.r, basecolor.g, basecolor.b, 0.0f);
    }

    void Update()
    {
        if (!GetComponent<GUIText>().enabled) return;

        if ( !pushed && Input.GetMouseButtonDown(0))
        {
            pushed = true;
            GetComponent<AudioSource>().Play();
            // 传递场景结束消息
            GameObject adapter = GameObject.Find("/Adapter");
            if (adapter) adapter.SendMessage("OnSceneEnd");
            else Debug.Log("adapter is not exist...");
        }
	}

    /// <summary>
    /// 淡入淡出结束时调用
    /// </summary>
    void OnEndTextFade()
    {
        if (!GetComponent<GUIText>().enabled) return;
        StartCoroutine("Delay");
    }

    /// <summary>
    /// 启动切换器
    /// </summary>
    void OnStartSwitcher()
    {
        Debug.Log("OnStartSwitcher");
        GetComponent<GUIText>().enabled = true;
        fade = true;
        SendMessage("OnTextFadeIn");
    }
    /// <summary>
    /// 重置关卡
    /// </summary>
    void OnStageReset()
    {
        GetComponent<GUIText>().enabled = false;
        Color basecolor = GetComponent<GUIText>().material.color;
        GetComponent<GUIText>().material.color = new Color(basecolor.r, basecolor.g, basecolor.b, 0.0f);
        pushed = false;
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(waitTime);
        // FadeIn和FadeOut交替执行
        fade = !fade;
        if (fade) SendMessage("OnTextFadeIn");
        else SendMessage("OnTextFadeOut");
    }

}

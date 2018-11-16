using UnityEngine;
using System.Collections;

/// <summary>
/// 声纳的从外向内，从内向外放大的特效
/// </summary>
public class SonarEffect : MonoBehaviour
{
    [SerializeField]
    private float duration = 1.0f;
    [SerializeField]
    private float delay = 1.0f;

    enum Type {
        Active,
        Passive
    };
    [SerializeField]
    private Type type = Type.Passive;

    private float stepStart = 0.0f;
    private float stepEnd = 1.0f;

    private float rate = 0.0f;
    private float currentTime = 0.0f;
//    private GUITexture texture;
    private Rect baseRect;

    public void Init( Rect rect )
    {
        baseRect = rect;
        switch (type)
        {
            case Type.Active:
                stepStart = 0.0f;
                stepEnd = 1.0f;
                break;
            case Type.Passive:
                stepStart = 1.0f;
                stepEnd = 0.0f;
                break;
        }

        
        //texture = GetComponent<GUITexture>();
        //texture.pixelInset = new Rect(baseRect);
        GetComponent<GUITexture>().enabled = true;
    }
	
	void Update () 
    {
        if (GetComponent<GUITexture>().enabled)
        {
            float time = currentTime / duration;
            if (time <= 1.0f)
            {
                rate = Mathf.SmoothStep(stepStart, stepEnd, time);
//                Debug.Log("alpha=" + rate);
                float w = baseRect.width * rate;
                float h = baseRect.height * rate;
                float a = Mathf.Clamp(1.0f - rate, 0.0f, 0.8f);
                GetComponent<GUITexture>().pixelInset = new Rect(baseRect.center.x - w * 0.5f, baseRect.center.y - h * 0.5f, w, h);
                GetComponent<GUITexture>().color = new Color(GetComponent<GUITexture>().color.r, GetComponent<GUITexture>().color.g, GetComponent<GUITexture>().color.b, a);
                // 更新时间
                currentTime += Time.deltaTime;
            }
            else 
            {
                GetComponent<GUITexture>().enabled = false;
                StartCoroutine("Delay", delay);
            }
        }
    }

    private IEnumerator Delay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        GetComponent<GUITexture>().enabled = true;
        currentTime = 0.0f;
    }

    public Rect Rect() { return GetComponent<GUITexture>().pixelInset; }
    public float Value(){   return rate;    }

}

using UnityEngine;
using System.Collections;

public class SonarCamera : MonoBehaviour {

//    [SerializeField]
    private float radius = 0.0f;

    void Awake()
    {
        // 对齐摄像机和Collider的半径
        radius = GetComponent<Camera>().orthographicSize;
        Debug.Log(Time.time + ": SonarCamera.Awake");
    }

    void Start()
    {

        SphereCollider shereCollider = GetComponent<SphereCollider>();
        if (shereCollider)
        {
            shereCollider.radius = radius;
        }
    }

    // Enter有时候会错过一部分处理
    void OnTriggerEnter(Collider other)
    {
//        Debug.Log("OnTriggerStay:" + other.gameObject.tag + ", " + other.gameObject.name);
        CheckSonarPoint_Enter(other.gameObject);
    }

    // 用Stay来替代
    void OnTriggerStay(Collider other)
    {
//        Debug.Log("OnTriggerStay:" + other.gameObject.tag + ", " + other.gameObject.name);
        CheckSonarPoint_Enter(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
//        Debug.Log("OnTriggerStay:" + other.gameObject.tag + ", " + other.gameObject.name);
        CheckSonarPoint_Exit(other.gameObject);
    }

    private void CheckSonarPoint_Enter(GameObject target)
    {
        if (!target.CompareTag("Sonar")) return;
        ColorFader fader = target.GetComponent<ColorFader>();
        if (fader==null) return;
        if (fader.SonarInside()) return;
        Debug.Log("CheckSonarPoint");
        target.BroadcastMessage("OnSonarInside");
    }

    private void CheckSonarPoint_Exit(GameObject target)
    {
        if (!target.CompareTag("Sonar")) return ;
//        ColorFader fader = target.GetComponent<ColorFader>();
//        if (fader) return fader.SonarInside();
        Debug.Log("CheckSonarPoint_TriggerExit");
        target.SendMessage("OnSonarOutside");
    }

    void OnInstantiatedSonarPoint(GameObject target)
    {
        // 检测是否已经在声纳内
        Vector3 pos = new Vector3(transform.position.x, 0.0f, transform.position.z);
        Vector3 target_pos = new Vector3( target.transform.position.x, 0.0f, target.transform.position.z );
        float dist = Vector3.Distance(pos, target_pos);
        Debug.Log("OnInstantiatedSonarPoint[" + target.transform.parent.gameObject.name + "]: dist=" + dist + ", radius=" + radius);
        if (dist <= radius)
        {
            target.SendMessage("OnSonarInside");
        }
        else {
            target.SendMessage("OnSonarOutside");
        }
    }

    public float Radius() 
    {
        return radius;
    } 

    // 调整显示的位置
	public void SetRect( Rect rect )
    {
        Debug.Log("SetRect:" + rect);
        GetComponent<Camera>().pixelRect = new Rect(rect.x, rect.y, rect.width, rect.height);

        // 将摄像机的显示区域表现在纹理上
        //float r = rect.width * 0.5f;
        //float newWidth = r * Mathf.Pow(2.0f,0.5f);
        //float sub = (rect.width - newWidth)*0.5f;
        //camera.pixelRect = new Rect(rect.x + sub, rect.y + sub, newWidth, newWidth);
        
        //sonarCamera.pixelRect = new Rect( rect );
    }
}

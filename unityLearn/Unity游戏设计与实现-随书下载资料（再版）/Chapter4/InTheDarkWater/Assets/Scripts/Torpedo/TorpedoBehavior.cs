using UnityEngine;
using System.Collections;

/// <summary>
/// 鱼雷的移动
/// </summary>
public class TorpedoBehavior : MonoBehaviour {

    [SerializeField]
    private float speed = 1.0f;

    private bool stop = false;

	void Start () 
    {
	
	}
	
	void Update () 
    {
        // 一般往前移动
        MoveForward();
	}

    /// <summary>
    /// 从Note发来的销毁许可
    /// </summary>
    void OnDestroyLicense()
    {
        // 传递给父对象。通过父对象进行销毁
        transform.parent.SendMessage("OnDestroyChild", gameObject);
    }
    /// <summary>
    /// Hit通知
    /// </summary>
    void OnHit()
    {
        speed = 0.0f;
        stop = true;
    }

    private void MoveForward()
    {
        if (stop) return;
        Vector3 vec = speed * transform.forward.normalized;
        GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + vec * Time.deltaTime);
    }


    public void SetSpeed( float speed_ ) { speed = speed_; }

}

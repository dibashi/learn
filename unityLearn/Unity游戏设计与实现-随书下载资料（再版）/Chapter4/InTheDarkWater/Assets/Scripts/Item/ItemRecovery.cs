using UnityEngine;
using System.Collections;

/// <summary>
/// 根据各个项目设置恢复量
/// </summary>
public class ItemRecovery : MonoBehaviour
{
    [SerializeField]
    private ItemParameter param;

    private float timeStamp = 0.0f; // 用于检测经过时长的时间戳

    void Start()
    {
    }

    void OnStartLifeTimer(ItemParameter param_)
    {
        Debug.Log("OnStartLifeTimer");
        param = param_;
        // 获取当前时间戳
        timeStamp = Time.time;
        StartCoroutine("WaitLifeTimeEnd");
    }

    void OnDestroyObject()
    {
        // 为了保险起见终止协程处理
        StopAllCoroutines();
    }

    private IEnumerator WaitLifeTimeEnd()
    {
        yield return new WaitForSeconds(param.lifeTime);
        // 直到生命周期结束后消失
        Disappear();
    }

    // 生命周期结束后自行Destory
    private void Disappear()
    {
        Debug.Log("Disappear");

        GameObject ui = GameObject.Find("/UI");
        if (ui)
        {
            // 通知Lost事件
            ui.BroadcastMessage("OnLostObject", tag, SendMessageOptions.DontRequireReceiver);
        }
        GameObject parent = gameObject.transform.parent.gameObject;
        if (parent)
        {
            // 向父对象通知Lost事件
            parent.SendMessage("OnLostObject", tag, SendMessageOptions.DontRequireReceiver);
        }

        // 强制销毁
        BroadcastMessage("OnInvalidEffect"); // 令Hit特效失效
        BroadcastMessage("OnHit");  // 事件通知Hit
    }

    /// <summary>
    /// 玩家获得后的恢复
    /// </summary>
    void OnRecovery()
    {
        GameObject ui = GameObject.Find("/UI");
        if (ui)
        {
            // 通知Hit事件
            ui.BroadcastMessage("OnHitObject", tag, SendMessageOptions.DontRequireReceiver);

            float t = (Time.time - timeStamp) / param.lifeTime;
            // 通知得分
            int score = (int)Mathf.Lerp(param.scoreMax, param.scoreMin, t);
            Debug.Log(t + ":ItemScore=" + score);
            ui.BroadcastMessage("OnAddScore",  score );
            // 恢复Air
            int recoveryValue = (int)Mathf.Lerp(param.recoveryMax, param.recoveryMin, t);
            Debug.Log(t + ":ItemAir=" + recoveryValue);
            ui.BroadcastMessage("OnAddAir", recoveryValue);
        }

        GameObject parent = gameObject.transform.parent.gameObject;
        if (parent)
        {
            // 向父对象通知Hit事件
            parent.SendMessage("OnHitObject", tag, SendMessageOptions.DontRequireReceiver);
        }

        // Hit后的处理
        BroadcastMessage("OnHit");  // 通知Hit事件
    }

}

using UnityEngine;
using System.Collections;

/// <summary>
/// 项目的碰撞
/// </summary>
public class ItemCollider : MonoBehaviour
{
    [SerializeField]
    private bool valid = true;  // 为保险起见用标志进行管理

    void Start()
    {
    }

    /// <summary>
    /// 从Note传来的销毁许可
    /// </summary>
//    void OnDestroyObject()
    void OnDestroyLicense()
    {
        // 通过父对象进行销毁
        transform.parent.gameObject.SendMessage("OnDestroyObject", gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        CheckPlayer(collision.gameObject);
    }
    void OnCollisionStay(Collision collider)
    {
        CheckPlayer(collider.gameObject);
    }

    void CheckPlayer(GameObject target)
    {
        if (! valid) return;
        if (!target.CompareTag("Player")) return;
        // 设置Collider无效
        GetComponent<Collider>().enabled = false;
        valid = false;
        // 和玩家碰撞时的效果
        SendMessage("OnRecovery");
    }

}

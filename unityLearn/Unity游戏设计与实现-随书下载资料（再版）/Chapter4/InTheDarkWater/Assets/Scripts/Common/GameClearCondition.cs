using UnityEngine;
using System.Collections;

/// <summary>
/// 清空条件
/// </summary>
public class GameClearCondition : MonoBehaviour
{
    [SerializeField]
    private bool valid = false;
    [SerializeField]
    private int destoryNorma = 0;
    [SerializeField]
    private int hitNorma = 0;

    private GameObject field = null;


    void Start()
    {
        field = GameObject.Find("/Field");
    }

    /// <summary>
    /// 生成时的处理
    /// </summary>
    /// <param name="target"></param>
    void OnInstantiatedChild(GameObject target)
    {
        // 如果有1个被生成则许可
    }

    /// <summary>
    /// 被销毁时的处理
    /// </summary>
    /// <param name="target"></param>
    void OnDestroyObject(GameObject target)
    {
        if (!valid) return;
        if (destoryNorma == 0) return;

        // 
        destoryNorma--;
        if (destoryNorma <= 0) Clear(target.tag);
    }

    /// <summary>
    /// 碰撞时的处理
    /// </summary>
    /// <param name="tag"></param>
    void OnHitObject(string tag)
    {
        if (!valid) return;
        if (hitNorma == 0) return;

        // 碰撞时
        hitNorma--;
        if (hitNorma <= 0) Clear(tag);
    }

    /// <summary>
    /// 失败时的处理
    /// </summary>
    /// <param name="tag"></param>
    void OnLostObject(string tag)
    {
    }

    private void Clear( string tag )
    {
        if (field) field.SendMessage("OnClearCondition", tag);
        valid = false;
    }
}

using UnityEngine;
using System.Collections;

/// <summary>
/// 设置项目的参数
/// </summary>
public class ItemParamSetter: MonoBehaviour
{
    [SerializeField]
    private ItemParameter fromParam = new ItemParameter();
    [SerializeField]
    private ItemParameter toParam = new ItemParameter();
    [SerializeField]
    private float duration = 240.0f;

    private float timeStamp = 0.0f;

    void Start()
    {
        timeStamp = Time.timeSinceLevelLoad;
    }

    void OnInstantiatedChild(GameObject target)
    {
        // 对生成的对象进行设定
        float t = 0;
        if (duration > 0) t = (Time.timeSinceLevelLoad - timeStamp) / duration;
        Debug.Log("ItemParamSetter:" + t);

        ItemParameter param = new ItemParameter();
        param.scoreMax = (int)Mathf.Lerp(fromParam.scoreMax, toParam.scoreMax, t);
        param.scoreMin = (int)Mathf.Lerp(fromParam.scoreMin, toParam.scoreMin, t);
        param.recoveryMax = (int)Mathf.Lerp(fromParam.recoveryMax, toParam.recoveryMax, t);
        param.recoveryMin = (int)Mathf.Lerp(fromParam.recoveryMin, toParam.recoveryMin, t);
        param.lifeTime = Mathf.Lerp(fromParam.lifeTime, toParam.lifeTime, t);

        target.SendMessage("OnStartLifeTimer", param);
    }

}

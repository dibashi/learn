using UnityEngine;
using System.Collections;

/// <summary>
/// 修改敌人参数
/// </summary>
public class EnemyParamSetter : MonoBehaviour
{
    [SerializeField]
    private EnemyParameter fromParam = new EnemyParameter();
    [SerializeField]
    private EnemyParameter toParam = new EnemyParameter();
    [SerializeField]
    private float duration = 240.0f;

    private float timeStamp = 0.0f;

    void Start()
    {
        timeStamp = Time.timeSinceLevelLoad;
    }

    void OnInstantiatedChild(GameObject target)
    {
        // 针对生成的对象进行设定
        float t = 0;
        if(duration > 0) t = (Time.timeSinceLevelLoad - timeStamp) / duration;
//        Debug.Log("EnemyParamSetter: t=" + t);

        EnemyParameter param = new EnemyParameter();
        param.scoreMax = (int)Mathf.Lerp(fromParam.scoreMax, toParam.scoreMax, t);
        param.scoreMin = (int)Mathf.Lerp(fromParam.scoreMin, toParam.scoreMin, t);
        param.cautionWaitMax = Mathf.Lerp(fromParam.cautionWaitMax, toParam.cautionWaitMax, t);
        param.cautionWaitMin = Mathf.Lerp(fromParam.cautionWaitMin, toParam.cautionWaitMin, t);
        param.sonarHitAddCaution = (int)Mathf.Lerp(fromParam.sonarHitAddCaution, toParam.sonarHitAddCaution, t);
        param.sneaking = Mathf.Lerp(fromParam.sneaking, toParam.sneaking, t);
        // 从生成时开始计算
        target.SendMessage("OnStartCautionCount", param);
    }
}

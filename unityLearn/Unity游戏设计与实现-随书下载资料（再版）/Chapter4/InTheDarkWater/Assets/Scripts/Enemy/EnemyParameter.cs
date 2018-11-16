using UnityEngine;

/// <summary>
/// 敌人相关参数
/// </summary>
[System.Serializable]
public class EnemyParameter 
{
    public int scoreMax = 1000; // 击中敌人后得分的最大值
    public int scoreMin = 100;   // 击中敌人后得分的最小值

    public float cautionWaitMax = 1.0f;    // Caution值更新间隔的最大值
    public float cautionWaitMin = 0.01f;   // Caution值更新间隔的最小值

    public float cautionWaitLimit = 10.0f;  // sneaking时Caution更新间隔的限制值

    public float sneaking = 0.5f;  // 根据玩家速度变化的情况调整Caution值更新的间隔


    public int sonarHitAddCaution = 10;  // 显示的时间（超过该时间长度则自动消失）

    public EnemyParameter(){ }
    public EnemyParameter(EnemyParameter param_)
    {
        scoreMax = param_.scoreMax;
        scoreMin = param_.scoreMin;
        cautionWaitMax = param_.cautionWaitMax;
        cautionWaitMin = param_.cautionWaitMin;
        cautionWaitLimit = param_.cautionWaitLimit;
        sonarHitAddCaution = param_.sonarHitAddCaution;
        sneaking = param_.sneaking;
    }
}

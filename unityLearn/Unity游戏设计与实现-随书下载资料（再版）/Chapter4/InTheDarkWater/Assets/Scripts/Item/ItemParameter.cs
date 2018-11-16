/// <summary>
/// 项目的参数
/// </summary>
[System.Serializable]
public class ItemParameter 
{
    public int scoreMax = 1000; // 得分最大值
    public int scoreMin = 100;   // 得分最小值

    public int recoveryMax = 100;    // 恢复量最大值
    public int recoveryMin = 10;     // 恢复量的最小值

    public float lifeTime = 60.0f;  // 显示的时间（超过这个时间长度后会自动消失）


    public ItemParameter() { }
    public ItemParameter(ItemParameter param_)
    {
        scoreMax = param_.scoreMax;
        scoreMin = param_.scoreMin;  
        recoveryMax = param_.recoveryMax;   
        recoveryMin = param_.recoveryMin;
        lifeTime = param_.lifeTime;  

    }

}

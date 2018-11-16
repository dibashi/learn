using UnityEngine;
using System.Collections;

/// <summary>
/// 根据敌人情况对Caution值进行设置
/// </summary>
public class EnemyCaution : MonoBehaviour {

    [SerializeField]
    private float waitForce = 0.02f;

    [SerializeField]
    private int step = 1;

    [SerializeField]   // 用于Debug查看
    private EnemyParameter param = null;
    
    [SerializeField]    // 用于Debug查看
    private int cautionValue = 0;

    private int currentStep = 1;

    [SerializeField]    // 用于Debug查看
    private float waitTime = 1.0f;

    private float count = 0.0f;

    private bool counting = false;
    private bool emergency = false;
    private bool countup = true;
    private CautionUpdater updater = null;
    private PlayerController controller = null;

	void Start () 
    {
        GameObject enemy = GameObject.Find("/Field/Enemies");
        if (enemy) updater = enemy.GetComponent<CautionUpdater>();
        GameObject player = GameObject.Find("/Field/Player");
        if (player) controller = player.GetComponent<PlayerController>();
	}

    void Update()
    {
        if (param == null || !counting) return;

        count += Time.deltaTime;
        if (count >= waitTime) {
            count = 0.0f;
            counting = UpdateCaution();
        }
    }

    void OnStayPlayer( float distRate )
    {
        if (param==null) return;

        if (countup)
        {
            // 距离Player越近，Caution值上升越快
            if (!emergency)
            {
                waitTime = Mathf.Lerp(param.cautionWaitMin, param.cautionWaitMax, distRate);
                // Player的速度越慢，Caution的值上升越困难
                float speedRate = controller.SpeedRate();
                // 一般对waitTime执行Lerp操作
                float sneakingRate = (1.0f - speedRate) * param.sneaking;
                waitTime = Mathf.Lerp(waitTime, param.cautionWaitLimit, sneakingRate);
            }
        }
        else 
        {
            // 计算到达极限后切换为反向
            StartCount(true);
        }
    }

      // 这里表示退出时的处理
    void OnExitPlayer()
    {
        if (param == null || counting) return;
        // 不进行计算直接离开
        waitTime = waitForce;
        emergency = true;
        StartCount(false);
    }

    void OnStartCautionCount(EnemyParameter param_)
    {
        // 参数设置和计算的开始
        Debug.Log("OnStartCautionCount");
        param = param_;
        waitTime = param.cautionWaitMax;
        StartCount(true);
    }

    void OnAddScore()
    {
        if (param == null) return;

        // 传递得分
        GameObject ui = GameObject.Find("/UI");
        if (ui)
        {
            // 游戏对象的命中
            ui.BroadcastMessage("OnHitObject", tag, SendMessageOptions.DontRequireReceiver);
            // 添加得分
            float time = 1.0f - Mathf.InverseLerp(0, 100, cautionValue);
            int scoreValue = (int)Mathf.Lerp(param.scoreMin, param.scoreMax, time);
            ui.BroadcastMessage("OnAddScore", scoreValue);
        }
        GameObject parent = gameObject.transform.parent.gameObject;
        if (parent)
        {
            // 通知父对象命中事件
            parent.SendMessage("OnHitObject", tag, SendMessageOptions.DontRequireReceiver);
        }
        // 自己执行命中判断
        BroadcastMessage("OnHit");
    }

    void OnActiveSonar()
    {
        if (param == null) return;
        Debug.Log("EnemyCaution.OnActiveSonar");
        // 每次声纳命中时，增加Caution值
        cautionValue = Mathf.Clamp(cautionValue + param.sonarHitAddCaution, 0, 100);
        updater.DisplayValue(gameObject, cautionValue);
    }

    private void StartCount(bool countup_)
    {
        count = 0;
        counting = true;
        countup = countup_;
        currentStep = (countup) ? step : (-step);
    }

    private bool UpdateCaution()
    {
        cautionValue = Mathf.Clamp(cautionValue + currentStep, 0, 100);
        // 更新显示
        updater.DisplayValue(gameObject, cautionValue);
        // 条件判断
        if (cautionValue >= 100)
        {
            // 发现Player
            SendMessage("OnEmergency");
            return false;
        }
        else if (cautionValue <= 0)
        {
            // Player消失了
            SendMessage("OnUsual");
            emergency = false;
            StartCount(true);   // 重新计算
        }

        return true;
    }

    void OnEmergency()
    {
        emergency = true;
        if (cautionValue < 100)
        {
            // cuation值未满时的情况
            waitTime = waitForce;
        }
    }

    public int Value(){ return cautionValue; }
}

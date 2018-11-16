using UnityEngine;
using System.Collections;

/// <summary>
/// 敌人的动作
/// </summary>
public class EnemyBehavior : MonoBehaviour
{
    /// <summary>
    /// 速度调整
    /// </summary>
    [System.Serializable]
    public class SpeedValue
    {
        [SerializeField]
        private float usualMax = 5.0f;
        [SerializeField]
        private float emergencyMax = 0.0f;
        [SerializeField]
        private float backwardMax = 10.0f;
        [SerializeField]
        private float current = 1.0f;
        private float max;
        public float Value
        {
            set
            {
                if (max < value) current = max;
                else current = value;
            }
            get { return current; }
        }

        public void Usual() { max = usualMax; }
        public void Emergency(){   max = emergencyMax; }

        public void Stop() { current = 0.0f; }

        /// <summary>
        /// 速度变化
        /// </summary>
        public void Change()
        {
            current += Random.Range(-max, max);
            current = Mathf.Clamp(current, 0.0f, max);
        }
        public void GoBackward()
        {
            current = -backwardMax;
        }
        public void GoFroward()
        {
            current = 0.0f;
            Change();
        }
    };
    [SerializeField]
    private SpeedValue speed = new SpeedValue();

    /// <summary>
    /// 旋转调整
    /// </summary>
    [System.Serializable]
    public class RotationValue
    {
        [SerializeField]
        private float usualMax = 20.0f;
        [SerializeField]
        private float emergencyMax = 10.0f;
        [SerializeField]
        private float swingStep = 20.0f;
        [SerializeField]
        private float blending = 0.8f;
        [SerializeField]
        private float attenuation = 0.2f;

        private Vector3 current = Vector3.zero;
        private float max;
        private float attenuationStart;
        private float attenuationTime;

        public Vector3 Value
        {
            set
            {
                if (value.y < -max) current.y = -max;
                if (value.y > max) current.y = max;
                else current = value;
            }
            get { return current; }
        }

        public void Usual() { max = usualMax; }
        public void Emergency() { max = emergencyMax; }

        public void Stop() { current = Vector3.zero; }

        public void Swing( float value )
        {
            if (value < -swingStep) value = -swingStep;
            if (value > swingStep) value = swingStep;
            current.y = value;
        }

        /// <summary>
        /// 旋转量变化
        /// </summary>
        public void Change()
        {
            float value = Random.Range(-max, max);
            // 旋转量的混合
            current.y = Mathf.Lerp(current.y, current.y + value, blending);
            // 重置衰减
            attenuationStart = current.y;
            attenuationTime = 0.0f;
        }

        /// <summary>
        /// 衰减
        /// </summary>
        /// <param name="time">时间位移量</param>
        /// <returns>衰减中／不衰减</returns>
        public bool Attenuate(float time)
        {
            if (current.y == 0.0f) return false;

            attenuationTime += time;
            current.y = Mathf.SmoothStep(attenuationStart, 0.0f, attenuation * attenuationTime);
            return true;
        }
    };
    [SerializeField]
    private RotationValue rot = new RotationValue();

    [SerializeField]
    private Rect runningArea;   // 移动范围
    [SerializeField]
    private float waitTime = 10.0f;

    [SerializeField]
    private float attackWait = 5.0f;
    [SerializeField]
    private float attackDistance = 100.0f;
        
    enum Mode
    {
        Usual,
        Caution,
        Emergency
    };
    private Mode mode = Mode.Usual;

    private float currentTime;
    private bool valid;

    private bool autoAttack = false;
    private float currentAttackTime;
    private TorpedoGenerator torpedo = null;
    private GameObject player = null;


    void Start()
    {
        // 在开始时处于外侧因此设置朝向中心
        transform.LookAt(Vector3.zero);

        player = GameObject.Find("/Field/Player");
        torpedo = GetComponent<TorpedoGenerator>();
        currentTime = 0.0f;
        valid = true;
        speed.Usual();
        rot.Usual();
    }

    void Update()
    {
       // 旋转的衰减
        if (! rot.Attenuate(Time.deltaTime))
        {
            if (valid)
            {
                // 衰减结束后，进行计算再次移动
                currentTime += Time.deltaTime;
                if (currentTime > waitTime) AutoController();
            }
        }
        // 旋转
        Rotate();
        // 前进
        MoveForward();
    }

    /// <summary>
    /// 游戏结束时
    /// </summary>
    void OnGameOver()
    {
        speed.Stop();
        rot.Stop();
        valid = false;
    }
    /// <summary>
    /// 攻击命中时
    /// </summary>
    void OnHit()
    {
        //Debug.Log("EnemyBehaviour.OnHit:" + name);
        // 设置为无效
        SphereCollider coli = GetComponent<SphereCollider>();
        if (coli) coli.enabled = false;
        // 为保险起见
        StopAllCoroutines();
    }
    /// <summary>
    /// 收到从Note传来的销毁许可
    /// </summary>
    void OnDestroyLicense()
    {
        //Debug.Log("EnemyBehaviour.OnDestroyObject:" + name);
        // 传递消息给父对象。从父对象中删除
        transform.parent.gameObject.SendMessage("OnDestroyObject", gameObject, SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// 发现Player以及攻击状态
    /// </summary>
    void OnEmergency()
    {
        Debug.Log("OnEmergency");
        mode = Mode.Emergency;
        rot.Emergency();
        speed.Emergency();

        if (!autoAttack)
        {
            autoAttack = true;
            Debug.Log("AutoAttack");
            StartCoroutine("AutoAttack");
        }
    }

    /// <summary>
    /// 警戒状态
    /// </summary>
    void OnCaution()
    {
        Debug.Log("OnCaution");
        mode = Mode.Caution;
        rot.Emergency();
        speed.Emergency();
        if (autoAttack)
        {
            autoAttack = false;
            StopCoroutine("AutoAttack");
        }
    }

    /// <summary>
    /// 普通状态
    /// </summary>
    void OnUsual()
    {
        Debug.Log("OnUsual");
        mode = Mode.Usual;
        rot.Usual();
        speed.Usual();
        if(autoAttack) 
        {
            autoAttack = false;
            StopCoroutine("AutoAttack");
        }
    }

    private IEnumerator AutoAttack()
    {
        yield return new WaitForSeconds(attackWait);

        transform.LookAt(player.transform);
        torpedo.Generate();

        StartCoroutine("AutoAttack");
    }


    /// <summary>
    /// 移动的自动更新
    /// </summary>
    private void AutoController()
    {
        currentTime = 0.0f;
        rot.Change();
        speed.Change();
    }

    /// <summary>
    /// 旋转更新
    /// </summary>
    private void Rotate()
    {
        if (!runningArea.Contains(new Vector2(transform.position.x, transform.position.z)))
        {
            // 位于移动区域外因此回旋
            Vector3 aimVec = -transform.position.normalized;
            float angle = Vector3.Angle(transform.forward, aimVec);
            Debug.Log("angle=" + angle + ": (" + transform.position.x + "," +  transform.position.z + ")");
            if (!Mathf.Approximately(angle, 0.0f)) rot.Swing(-angle);
        }

        if (mode == Mode.Emergency)
        {
            // 朝玩家方向
//            Vector3 aimVec = player.transform.position - transform.position;
//            float angle = Vector3.Angle(transform.forward, aimVec);
//            Debug.Log("angle=" + angle + ": (" + transform.position.x + "," + transform.position.z + ")");
//            if (!Mathf.Approximately(angle, 0.0f)) rot.Swing(-angle);
        }

        Quaternion deltaRot = Quaternion.Euler(rot.Value * Time.deltaTime);
        GetComponent<Rigidbody>().MoveRotation(GetComponent<Rigidbody>().rotation * deltaRot);
    }

    /// <summary>
    /// 更新移动
    /// </summary>
    private void MoveForward()
    {
        CheckPlayer();

        Vector3 deltaVec = speed.Value * transform.forward.normalized;
        GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + deltaVec * Time.deltaTime);
    }

    private void CheckPlayer()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        switch (mode)
        {
            case Mode.Emergency:
                if (dist <= attackDistance) speed.GoBackward();
                else speed.GoFroward();
                break;
            case Mode.Usual:
                // 提醒玩家注意
                 if (dist <= attackDistance)
                {
                    SendMessage("OnEmergency");
                }
                break;
            default: break;
        }
    }
}

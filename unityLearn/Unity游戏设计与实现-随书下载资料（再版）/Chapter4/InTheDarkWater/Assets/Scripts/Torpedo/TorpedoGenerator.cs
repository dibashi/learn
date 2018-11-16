using UnityEngine;
using System.Collections;

/// <summary>
/// 生成鱼雷
/// </summary>
public class TorpedoGenerator : MonoBehaviour {

    [SerializeField]
    private GameObject target = null;
    [SerializeField]
    private Vector3 pos = new Vector3();            // 最初的生成位置
    [SerializeField]
    private float coolTime = 3.0f;  // 冷却事件
    [SerializeField]
    private bool sound = false;    // 是否发出声音
    [SerializeField]
    private bool sonar = false;  // 是否显示声纳
    [SerializeField]
    private float speed = 15.0f;
    [SerializeField]
    private TorpedoCollider.OwnerType type = TorpedoCollider.OwnerType.Enemy;
                                        // 拥有者

    private float current;

    private bool valid = true;
    private GameObject parentObj = null;

    void Start()
    {
        // 鱼雷的结点位置
        parentObj = GameObject.Find("/Field/Torpedoes");
    }

    void Update()
    {
        if (!valid)
        {
            // 冷却计时器
            current += Time.deltaTime;
            if (current >= coolTime)
            {
                valid = true;
            }
        }
    }

    public void Generate()
    {
        // 冷却中不会生成
        if (valid == false)
        {
            //Debug.Log("Cool time:" + Time.time);
            return;
        }

        // 求出位置／角度
        Vector3 vec = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        vec += pos.x * transform.right;
        vec += pos.y * transform.up;
        vec += pos.z * transform.forward;
        Quaternion rot = Quaternion.Euler(transform.eulerAngles);
        // 生成
        GameObject newObj = Object.Instantiate(target, vec, rot) as GameObject;
        // 设置父对象
        newObj.transform.parent = parentObj.transform;

        // 设置拥有者对象
        TorpedoCollider torpedoCollider = newObj.GetComponent<TorpedoCollider>();
        if (torpedoCollider) torpedoCollider.SetOwner(type);
        else Debug.LogError("Not exists TorpedoCollider");
        // 设置速度
        TorpedoBehavior torpedoBehavior = newObj.GetComponent<TorpedoBehavior>();
        if (torpedoBehavior) torpedoBehavior.SetSpeed(speed);
        else Debug.LogError("Not exists TorpedoBehavior");
        // 设置声音
        Note note = newObj.GetComponentInChildren<Note>();
        if (note) note.SetEnable(sound);
        else Debug.LogError("Not exists Note");
        // 设置声纳
        parentObj.SendMessage("OnInstantiatedChild", newObj); 
        if (sonar)
        {
            newObj.BroadcastMessage("OnActiveSonar");
        }


        // 开始冷却
        valid = false;
        current = 0.0f;
    }

}

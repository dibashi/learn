using UnityEngine;
using System.Collections;

/// <summary>
/// 鱼雷的碰撞
/// </summary>
public class TorpedoCollider : MonoBehaviour {

    public enum OwnerType {
        Player,
        Enemy
    };
    private OwnerType owner;

    [SerializeField]
    private float delayTime = 2.0f;
    [SerializeField]
    private int damageValue = 1;

    [System.Serializable]
    public class Explosion
    {
        [SerializeField]
        private float force = 100.0f;
        [SerializeField]
        private float upwardsModifier = 0.0f;
        [SerializeField]
        private ForceMode mode = ForceMode.Impulse;

        private float radius = 3.0f;
            
        public void Add(Rigidbody target, Vector3 pos) 
        {
            target.AddExplosionForce(force, pos, radius, upwardsModifier, mode);
        }
        public void SetRadius(float value) { radius = value; }
    };
    [SerializeField]
    Explosion explosion = new Explosion(); 

    private GameObject ui = null;

	void Start () 
    {
        ui = GameObject.Find("/UI");

        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider) explosion.SetRadius( sphereCollider.radius );

        // 发射时和自己有解除，防止误判在发射后数秒之内不执行碰撞检测
        GetComponent<Collider>().enabled = false;
        StartCoroutine("Delay");
	}

    /// <summary>
    /// 游戏结束时
    /// </summary>
    void OnGameOver()
    {
        GetComponent<Collider>().enabled = false;
    }
    /// <summary>
    /// 游戏清空时
    /// </summary>
    void OnGameClear()
    {
        GetComponent<Collider>().enabled = false;
    }

    // 只调用Enter方法可能会遗漏某些处理，所以还调用Stay来执行Collision检测
    void OnCollisionEnter(Collision collision)
    {
        CheckCollision(collision.gameObject);
    }
    void OnCollisionStay(Collision collision)
    {
        CheckCollision(collision.gameObject);
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(delayTime);

        GetComponent<Collider>().enabled = true;
        Debug.Log("Wait EndCoroutine");
    }

    private void CheckCollision(GameObject target)
    {
        bool hit = false;
        hit |= CheckPlayer(target);
        hit |= CheckEnemy(target);
        hit |= CheckTorpedo(target);
        if( hit ) {
            // 和任意对象接触后，调用相应的处理
            BroadcastMessage("OnHit");
            // 使Collider无效
            GetComponent<Collider>().enabled = false;
        }
    }
    /// <summary>
    /// 鱼雷的碰撞
    /// </summary>
    /// <param name="target">检测对象</param>
    /// <returns></returns>
    private bool CheckTorpedo(GameObject target)
    {
        if (target.CompareTag("Torpedo"))
        {
            // 如果和自身相同则表示和其他鱼雷碰撞了
            Debug.Log("CheckTorpedo");
            return true;
        }
        return false;
    }
    /// <summary>
    /// 和玩家的碰撞
    /// </summary>
    /// <param name="target">检测对象</param>
    /// <returns></returns>
    private bool CheckPlayer(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            Debug.Log("CheckPlayer");
            // 施加冲击
            explosion.Add( target.GetComponent<Rigidbody>(), transform.position );
            // 传递Hit事件
            target.BroadcastMessage("OnHit");
            // 损坏通知
            if (ui) ui.BroadcastMessage("OnDamage", damageValue, SendMessageOptions.DontRequireReceiver);
            return true;
        }
        return false;
    }
    /// <summary>
    /// 和敌人的碰撞
    /// </summary>
    /// <param name="target">检测对象</param>
    /// <returns></returns>
    private bool CheckEnemy(GameObject target)
    {
        if (target.CompareTag("Enemy"))
        {
            Debug.Log("CheckEnemy");
            if (owner == OwnerType.Player)
            {
                // 只有自己的鱼雷击中敌人时，才会添加得分
                target.SendMessage("OnAddScore", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                // 向敌人通知Hit事件
                target.BroadcastMessage("OnHit");
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 设置发射鱼雷的所有者对象
    /// </summary>
    /// <param name="type"></param>
    public void SetOwner(OwnerType type) { owner = type; }
    /// <summary>
    /// 设置损坏量。一般没有必要
    /// </summary>
    /// <param name="value"></param>
    public void SetDamageValue(int value) { damageValue = value; }

}

using UnityEngine;
using System.Collections;

public class RandomGenerator : MonoBehaviour {

    [SerializeField]
    private GameObject target;  // 生成对象
    [SerializeField]
    private GenerateParameter param = new GenerateParameter();
    [SerializeField]
    private float	posY = 1.0f;

    private int		counter    = 0;
    private bool	limitCheck = false;
    private bool	ready      = false;

    private ArrayList	childrenArray = new ArrayList();
   
    void Start()
    {
        // 如果有初始化配置则在这里注册（主要用于调试）
        GameObject[] children = GameObject.FindGameObjectsWithTag(target.tag);
        for (int i = 0; i < children.Length; i++ )
        {
            childrenArray.Add(children[i]);
        }

        OnGeneratorStart();
    }

    void Update()
    {
        if (TimingCheck())
        {
            ready = false;
            StartCoroutine("Delay");
        }
    }

    private bool TimingCheck()
    {
        // 还没有准备好
        if (!ready) return false;
        // 一旦数量到达上限，如果endless标记为否将不会继续追加
        if (!param.endless && limitCheck) return false;
        // 检测个数
        return (ChildrenNum() < param.limitNum) ? true : false;
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(param.delayTime);

        Generate();
        ready = true;
    }

    void OnGeneratorStart()
    {
        counter = 0;
        ready = true;
        limitCheck = false;
    }
    void OnGeneratorSuspend()
    {
        ready = false;
    }
    void OnGeneratorResume()
    {
        ready = true;
    }

    /// <summary>
    /// 销毁[ Message ] 对象
    /// </summary>
    void OnDestroyObject( GameObject target )
    {
        // 如果残留在数组中则删除
        childrenArray.Remove(target);
        // 通知减少子对象事件
        SendMessage("OnDestroyChild", target, SendMessageOptions.DontRequireReceiver);
        Destroy(target);
    }

    // 生成对象
    public void Generate()
    {
        RectXZ		rect = param.posXZ;
        Vector3 	pos = new Vector3(rect.x, posY, rect.z);

		if(param.fill) {

            // 随机决定posRange内的一个位置.
            pos.x = Random.Range(-0.5f, 0.5f)*rect.width + rect.x;
            pos.z = Random.Range(-0.5f, 0.5f)*rect.depth + rect.z;

		} else {

            // 随机决定posRange外围上的一个位置.
			float	l = Random.Range(0.0f, rect.width*2.0f + rect.depth*2.0f);

			do {

				if(l < rect.width) {

					pos.x = rect.getXMin() + l;
					pos.z = rect.getZMin();
					break;
				}
				l -= rect.width;

				if(l < rect.depth) {

					pos.x = rect.getXMax();
					pos.z = rect.getZMin() + l;
					break;
				}
				l -= rect.depth;

				if(l < rect.width) {

					pos.x = rect.getXMax() - l;
					pos.z = rect.getZMax();
					break;
				}
				l -= rect.width;

				if(l < rect.depth) {

					pos.x = rect.getXMin();
					pos.z = rect.getZMax() - l;
					break;
				}
				l -= rect.depth;

			} while(false);

        }
		Debug.Log(pos.x + " " + pos.z);

        // 生成实例
        GameObject newChild = Object.Instantiate(target, pos, Quaternion.identity) as GameObject;
        // 将自己作为父对象
        newChild.transform.parent = transform;
	
        // 更新数组
        childrenArray.Add(newChild);

        // 通知子对象增加事件
        SendMessage("OnInstantiatedChild", newChild, SendMessageOptions.DontRequireReceiver);

        counter++;
        if (counter >= param.limitNum)
        {
            limitCheck = true;  // 一旦达到限制数量则设置检测标记
        }
    }

    public int ChildrenNum()
    {
        if (childrenArray != null) return childrenArray.Count;
        return 0;
    }
    public GameObject Target() { return target; }

    // 取得管理的子对象
    public ArrayList Children() { return childrenArray; }

    // 设置生成参数
    public void SetParam(GenerateParameter param_) {  param = param_; }

}


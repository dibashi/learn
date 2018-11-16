using UnityEngine;
using System.Collections;

/// <summary>
/// ランダムな位置にインスタンスを生成.
/// </summary>
public class RandomGenerator : MonoBehaviour {

    [SerializeField]
    private GameObject target = null;  // 生成对象
    [SerializeField]
    private GenerateParameter param = new GenerateParameter();
    [SerializeField]
    private bool relative = false;
    [SerializeField]
    private Rect runningArea = new Rect(-700.0f, -700.0f, 1400.0f, 1400.0f);

    private int counter = 0;

    private bool clear = false;
    private bool limitCheck = false;
    private bool ready = false;

    private ArrayList childrenArray = new ArrayList();
    private ArrayList sonarArray = new ArrayList();

    private GameObject field = null;
    private GameObject player = null;
   
    void Start()
    {
        // 如果有初始化配置则在这里注册（主要用于调试）
        GameObject[] children = GameObject.FindGameObjectsWithTag(target.tag);
        for (int i = 0; i < children.Length; i++ )
        {
            childrenArray.Add(children[i]);
            sonarArray.Add(children[i]);
        }

        field = GameObject.Find("/Field");
        player = GameObject.Find("/Field/Player");
    }

    void Update()
    {
        if (TimingCheck())
        {
            ready = false;
            StartCoroutine("Delay");
//            Generate();
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
        sonarArray.Remove(target);
        // 通知减少子对象事件
        SendMessage("OnDestroyChild", target, SendMessageOptions.DontRequireReceiver);
        if (field) field.SendMessage("OnSwitchCheck", target.tag);
        Destroy(target);
    }

    // 生成对象
    public void Generate()
    {
        Rect rect = param.posXZ;
        float offsetX = 0.0f;
        float offsetZ = 0.0f;
        if (relative)
        {
            offsetX = player.transform.position.x;
            offsetZ = player.transform.position.z;
        }

        Vector3 pos = new Vector3(rect.xMin + offsetX, 0, rect.yMin + offsetZ);
        if (param.fill)
        {
            // 随机决定posRange内的一个位置
            pos.x += rect.width * Random.value;
            pos.z += rect.height * Random.value;
        }
        else {
            // 随机决定posRange外围上的一个位置
            if (Random.Range(0, 2) == 1)
            {
                pos.x += rect.width * Random.value;
                if (Random.Range(0, 2) == 1) pos.z = rect.yMax;
            }
            else
            {
                if (Random.Range(0, 2) == 1) pos.x = rect.xMax;
                pos.z += rect.height * Random.value;
            }
        }

        // 如果在范围外则通过Clamp修正
        pos.x = Mathf.Clamp(pos.x, runningArea.xMin, runningArea.xMax);
        pos.z = Mathf.Clamp(pos.z, runningArea.yMin, runningArea.yMax);

        // 生成实例
        GameObject newChild = Object.Instantiate(target, pos, Quaternion.identity) as GameObject;
        // 将自己作为父对象
        newChild.transform.parent = transform;
        Debug.Log("generated[" + ChildrenNum() + "]=" + newChild.name);

        // 更新数组
        childrenArray.Add(newChild);
        sonarArray.Add(newChild);

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
    public bool Clear() { return clear; }

    // 取得管理的子对象
    public ArrayList Children() { return childrenArray; }
    // 获得声纳对象
    public ArrayList SonarChildren() { return sonarArray; }
    // 设置生成参数
    public void SetParam(GenerateParameter param_) {  param = param_; }

}

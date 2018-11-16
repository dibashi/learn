using UnityEngine;
using System.Collections;

/// <summary>
/// 鱼雷全体管理。鱼雷的自动销毁
/// </summary>
public class TorpedoManager : MonoBehaviour {

    [SerializeField]
    private bool check = true;
    [SerializeField]
    private Rect runningArea = new Rect(-950, -950, 1900, 1900);   // 有效范围（世界坐标）
    [SerializeField]
    private bool relative = false;
    [SerializeField]
    private float delayTime = 2.0f;

    private Rect rect;

    private ArrayList childrenArray = new ArrayList();
    private ArrayList sonarArray = new ArrayList();

    void Start()
    {
        // 开始
        if (check) StartCoroutine("CheckDelay");
    }

    /// <summary>
    /// 生成实例时
    /// </summary>
    /// <param name="target">生成的实例</param>
    void OnInstantiatedChild(GameObject target)
    {
        childrenArray.Add(target);
        sonarArray.Add(target);
    }
    /// <summary>
    /// 销毁实例时
    /// </summary>
    /// <param name="target">销毁对象</param>
    void OnDestroyChild(GameObject target)
    {
        // 如果在列表中则销毁
        Debug.Log("TorpedManager.OnDestroyChild");
        childrenArray.Remove(target);
        sonarArray.Remove(target);

        Destroy(target);
    }

    /// <summary>
    /// 游戏结束时
    /// </summary>
    void OnGameOver()
    {
        StopAllCoroutines();
    }
    /// <summary>
    /// 游戏清空时
    /// </summary>
    void OnGameClear()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// 定期检测鱼雷是否运动出有效区域乐
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckDelay()
    {
        yield return new WaitForSeconds(delayTime);

        if (relative)
        {
            GameObject player = GameObject.Find("/Field/Player");
            if (player) {
                Vector3 pos = player.transform.position;
                rect = new Rect(runningArea.xMin + pos.x, runningArea.yMin + pos.z, runningArea.width, runningArea.height);
            }
        }
        else rect = new Rect(runningArea);

        int i = 0;
        while (i < childrenArray.Count)
        {
            GameObject target = childrenArray[i] as GameObject;
            if (target == null)
            {
                i++;
                continue;
            }

            Vector3 pos = target.transform.position;
            if (rect.Contains(new Vector2(pos.x, pos.z)))
            {
                i++;
            }
            else
            {
                childrenArray.RemoveAt(i);  // 销毁对象
                sonarArray.Remove(target);  // 如果sonar还存在的话销毁它
                Destroy(target);
            }
        }

        // 下一个Delay
        StartCoroutine("CheckDelay");
    }

    public int ChildrenNum()
    {
        if (childrenArray != null) return childrenArray.Count;
        return 0;
    }

    // 获取被管理的子对象
    public ArrayList Children() { return childrenArray; }
    // 获取声纳对象的数组
    public ArrayList SonarChildren() { return sonarArray; }
}

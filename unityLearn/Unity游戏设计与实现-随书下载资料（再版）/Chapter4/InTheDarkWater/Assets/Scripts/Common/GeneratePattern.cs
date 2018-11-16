using UnityEngine;
using System.Collections;

/// <summary>
/// 生成的模式
/// </summary>
public class GeneratePattern : MonoBehaviour {

    // 发生时间
    [SerializeField]
    private float[] timing = new float[] { };
    // 生成状况变化
    [SerializeField]
    private GenerateParameter[] variation = new GenerateParameter[]{ };

    private int current = 0;
    private int validSize;

    private RandomGenerator generator = null;

	void Start () 
    {
        validSize = (timing.Length > variation.Length) ? variation.Length : timing.Length;

        generator = GetComponent<RandomGenerator>();

        // 开始计算
        StartCoroutine("Counter");
    }

    private IEnumerator Counter()
    {
        yield return new WaitForSeconds(timing[current]);

        // 变更参数
        Debug.Log("Change Generate Parameter:time=" + timing[current]);
        if (generator) generator.SetParam(variation[current]);

        current++;
        if (current < validSize)
        {
            // 再次计算
            StartCoroutine("Counter");
        }
    }
}

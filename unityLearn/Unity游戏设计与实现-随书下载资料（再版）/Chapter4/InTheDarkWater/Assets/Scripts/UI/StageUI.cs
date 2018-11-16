using UnityEngine;
using System.Collections;

/// <summary>
/// Stage场景用
/// </summary>
public class StageUI : MonoBehaviour {


	void Awake()
	{
		// 通过LoadLevel剔除Destory対象
        // Destory的判断通过SceneSelector执行
        DontDestroyOnLoad(gameObject);
	}

    // 返回得分
    public int Score()
    {
        GameObject scoreDisp = GameObject.Find("ScoreDisplay");
        if (scoreDisp)
        {
            ScoreDisplay disp = scoreDisp.GetComponent<ScoreDisplay>();
            if (disp) return disp.Score();
        }
        return 0;
    }
}

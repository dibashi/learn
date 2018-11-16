using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 记录游戏内信息（GlobalParam用于Unity在Unity开始执行后持续记录）
//  - 记录／返回HISCORE
// ----------------------------------------------------------------------------
public class GlobalParam : MonoBehaviour {
	
	private int hiScore = 0;
	private static GlobalParam instance = null;
	
	// 返回GlobalParam只生成一次的实例
	// (生成后返回生成的实例)
	public static GlobalParam GetInstance()
	{
		if ( instance == null )
		{
			GameObject globalParam = new GameObject("GlobalParam");
			instance = globalParam.AddComponent<GlobalParam>();
			DontDestroyOnLoad( globalParam );
		}
		return instance;
	}
	
	// 返回HISCORE
	public int GetHiScore()
	{
		return hiScore;
	}
	
	// 记录HISCORE
	public void SetHiScore( int hiScore )
	{
		this.hiScore = hiScore;
	}
}

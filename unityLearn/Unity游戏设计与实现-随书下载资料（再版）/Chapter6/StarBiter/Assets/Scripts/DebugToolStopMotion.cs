using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 调试工具
//  - 通过按键P暂停游戏的进行
// ----------------------------------------------------------------------------
public class DebugToolStopMotion : MonoBehaviour {

	private float origTimeScale;
	
	// 比其他对象更早获取timeScale
	void Awake()
	{
		origTimeScale = Time.timeScale;
	}
	
	void Update () 
	{
		// 点击
		if ( Input.GetKeyDown(KeyCode.P) )
		{
			if ( Time.timeScale != 0 )
			{
				Time.timeScale = 0;
			}
			else
			{
				Time.timeScale = origTimeScale;
			}
		}
	}
}

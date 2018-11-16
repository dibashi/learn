using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// RKeyPressToOpening
//  - 载入开场场景
//  - 使用方法
//    - 将这个脚本添加到主摄像机
//  - 运动规则
//    - 按下R键后载入开场场景
// ---------------------------------------------------------------------------
public class RKeyPressToOpening : MonoBehaviour {

	private string gameSceneName = "opening";	// 游戏场景名

	void Update () {
	
		// 按下按键
		if ( Input.GetKeyDown(KeyCode.R) )
		{
			// 载入游戏场景
			UnityEngine.SceneManagement.SceneManager.LoadScene( gameSceneName );
		}
			
	}
}

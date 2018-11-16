using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// ClickToOpening
//  - 载入开场场景
//  - 使用方法
//    - 将这个脚本添加到主摄像机
//  - 工作流程
//    - 通过点击载入开场场景
// ---------------------------------------------------------------------------
public class ClickToOpening : MonoBehaviour {

	private string gameSceneName = "opening";	// 游戏场景名

	void Update ()
	{
		// 点击
		if ( Input.GetButtonDown("Fire1") ) {
			// 载入游戏场景
			UnityEngine.SceneManagement.SceneManager.LoadScene( gameSceneName );
		}
			
	}
}

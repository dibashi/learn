using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 点击启动游戏
//  - 使用方法
//    - 将这个脚本添加到主摄像机上
//    - 添加点击音效到主摄像机
//  - 工作流程
//    - 通过点击载入游戏场景
// ----------------------------------------------------------------------------
public class ClickToGameStart : MonoBehaviour {
	
	private string gameSceneName = "game";	// 游戏场景名
	private GameObject mainCamera;			// 主摄像机
	
	void Start ()
	{	
		// 获取主摄像机的实例
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
	}
	
	void Update ()
	{	
		// 点击鼠标左键
		if ( Input.GetButtonDown("Fire1") )
		{
			// 按下按键时播放声音
			mainCamera.GetComponent<AudioSource>().Play();
			
			// 载入游戏场景
			UnityEngine.SceneManagement.SceneManager.LoadScene( gameSceneName );
		}
	}
}

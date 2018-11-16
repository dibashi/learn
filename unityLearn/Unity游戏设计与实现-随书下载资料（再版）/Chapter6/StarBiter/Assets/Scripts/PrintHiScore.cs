using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 显示HISCORE
// ----------------------------------------------------------------------------
public class PrintHiScore : MonoBehaviour {

	private int hiScore = 0;
	private GUIText textHiScore;
	
	void Start () 
	{
		// 获取hi-score的实例
		textHiScore = GetComponent<GUIText>();
		
		// 从GlobalParam中获取hi-score
		hiScore = GlobalParam.GetInstance().GetHiScore();		
		
		// 显示初始值
		textHiScore.text = hiScore.ToString();
	}
	
	// ------------------------------------------------------------------------
	// 设置HISCORE
	// ------------------------------------------------------------------------
	public void SetHiScore( int hiScore )
	{
		// 存储
		this.hiScore = hiScore;
	
		// 显示
		textHiScore.text = this.hiScore.ToString();
	}

}

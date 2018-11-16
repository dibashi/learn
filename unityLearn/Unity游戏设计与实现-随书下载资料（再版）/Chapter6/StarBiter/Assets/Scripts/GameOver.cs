using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 游戏结束时的消息显示处理
// ----------------------------------------------------------------------------
public class GameOver : MonoBehaviour {
	
	public GameObject MessageMission;
	public GameObject MessageAccomplished;
	public GameObject MessageAccomplishedLine1;
	public GameObject MessageAccomplishedLine2;
	public GameObject MessageAccomplishedLine3;
	public GameObject MessageGameOver;
	public GameObject MessageHiScore;
	
	private bool isHiScore = false;		// 是否HISCORE？
	private bool defeatedBoss = false;	// 是否击败BOSS？
	private bool isEnable = false;		// 该机能是否有效？
	
	void Update () {
		
		if ( isEnable )
		{
			// 显示通过关卡或者游戏结束消息
			if ( defeatedBoss )
			{
				// 显示通过关卡文字
				Instantiate( MessageMission, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
				Instantiate( MessageAccomplished, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
				Instantiate( MessageAccomplishedLine1, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
				Instantiate( MessageAccomplishedLine2, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
				Instantiate( MessageAccomplishedLine3, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
			}
			else
			{
				// 显示游戏结束文字
				Instantiate( MessageGameOver, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
			}
			
			// 显示HISCORE
			if ( isHiScore )
			{
				// HISCORE的显示处理
				StartCoroutine( WaitAndPrintHiScoreMessage( 0.5f ) );
			}
			
			// 只执行一次显示处理
			isEnable = false;
		}
	}
	
	// ------------------------------------------------------------------------
	// 记录是否为HISCORE
	// ------------------------------------------------------------------------
	public void SetIsHiScore( bool isHiScore )
	{
		this.isHiScore = isHiScore;
	}
	
	// ------------------------------------------------------------------------
	// 记录是否击败了BOSS
	// ------------------------------------------------------------------------
	public void SetDefeatedBoss( bool defeatedBoss )
	{
		this.defeatedBoss = defeatedBoss;
	}
	
	// ------------------------------------------------------------------------
	// 设置消息显示的标记位
	// ------------------------------------------------------------------------
	public void Show()
	{
		isEnable = true;
	}
	
	// ------------------------------------------------------------------------
	// HISCORE时的消息显示
	//  - 指定延迟显示的时间
	// ------------------------------------------------------------------------
	IEnumerator WaitAndPrintHiScoreMessage( float waitForSeconds )
	{
		// 等待一定时间
		yield return new WaitForSeconds( waitForSeconds );

		// 显示HISCORE的消息
		Instantiate( MessageHiScore, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) );
	}
}

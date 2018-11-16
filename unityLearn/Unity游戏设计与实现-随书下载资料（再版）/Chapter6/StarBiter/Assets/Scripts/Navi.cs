using UnityEngine;
using System.Collections;

public class Navi : MonoBehaviour {

	public GameObject	uiCanvas;

	public UnityEngine.UI.Text	uiScoreText;
	public UnityEngine.UI.Text	uiHiScoreText;

	public GameObject	uiMessageGameOver;
	public GameObject	uiMessageAccomplished;
	public GameObject 	uiMessageHiScore;

	public UnityEngine.UI.Image[] 	uiPlayerLeftImages;

	protected int	score = 0;
	protected int	hiScore = 0;

	protected LockSlot		lock_slot;					// 显示锁定槽
	protected LockBonus		lock_bonus;					// 显示锁定奖励槽
	protected PrintMessage	print_message;				// 消息窗口

	// ================================================================ //
	// 继承于MonoBehaviour

	void	Awake()
	{
		this.lock_slot     = this.GetComponent<LockSlot>();
		this.lock_bonus    = this.GetComponent<LockBonus>();
		this.print_message = this.GetComponent<PrintMessage>();

	}

	void	Start()
	{
		// 从全局参数中获取hi-score
		this.SetHiScore(GlobalParam.GetInstance().GetHiScore());
	}

	// ================================================================ //

	// ------------------------------------------------------------------------
	// 累加SCORE
	// ------------------------------------------------------------------------
	public void AddScore( int score )
	{
		// 累加
		this.score += score;
		
		// 显示
		this.uiScoreText.text = this.score.ToString();
	}
	
	// ------------------------------------------------------------------------
	// 返回SCORE
	// ------------------------------------------------------------------------
	public int GetScore()
	{
		return score;
	}

	// ------------------------------------------------------------------------
	// 设定HISCORE
	// ------------------------------------------------------------------------
	public void SetHiScore( int hiScore )
	{
		// 保存
		this.hiScore = hiScore;
	
		// 显示
		uiHiScoreText.text = this.hiScore.ToString();
	}

	// ------------------------------------------------------------------------
	// 设定HISCORE
	// ------------------------------------------------------------------------
	public int GetHiScore()
	{
		return this.hiScore;
	}

	// ================================================================ //

	// 显示"GAME OVER"文字
	public void ShowGameOver()
	{
		this.uiMessageGameOver.SetActive(true);
	}

	// 显示闯关成功（击败BOSS）的提示消息
	public void ShowMisssionAccomplished()
	{
		this.uiMessageAccomplished.SetActive(true);
	}

	// ------------------------------------------------------------------------
	// 记录是否为HISCORE状态
	// ------------------------------------------------------------------------
	public void SetIsHiScore( bool isHiScore )
	{
		if ( isHiScore )
		{
			// HISCORE表示処理.
			StartCoroutine( WaitAndPrintHiScoreMessage( 0.5f ) );
		}
	}

	// ------------------------------------------------------------------------
	// 当HISCORE时显示消息
	//  - 延迟指定的时间后显示
	// ------------------------------------------------------------------------
	IEnumerator WaitAndPrintHiScoreMessage( float waitForSeconds )
	{
		// 等待一定时间
		yield return new WaitForSeconds( waitForSeconds );

		// 显示HISCORE消息
		this.uiMessageHiScore.SetActive(true);
	}

	// ================================================================ //

	// 获取锁定槽对象
	public LockSlot		GetLockSlot()
	{
		return(this.lock_slot);
	}

	// 获取锁定奖励对象
	public LockBonus	GetLockBonus()
	{
		return(this.lock_bonus);
	}

	// ================================================================ //

	// 设置玩家的剩余飞机数量
	public void		SetPlayerLeftCount(int count)
	{
		for(int i = 0;i < this.uiPlayerLeftImages.Length;i++) {

			UnityEngine.UI.Image	left_image = this.uiPlayerLeftImages[i];

			if(i < count) {

				left_image.gameObject.SetActive(true);

			} else {

				left_image.gameObject.SetActive(false);
			}
		}
	}

	// ================================================================ //

	// 获取消息窗口
	public PrintMessage	GetPrintMessage()
	{
		return(this.print_message);
	}

	// ================================================================ //
	//																	//
	// ================================================================ //

	protected static	Navi instance = null;

	public static Navi	get()
	{
		if(Navi.instance == null) {

			GameObject		go = GameObject.Find("GameCanvas");

			if(go != null) {

				Navi.instance = go.GetComponent<Navi>();

				if(Navi.instance == null) {
					Debug.LogError("[Navi] Component not attached.");
				}

			} else {
				Debug.LogError("[Navi] Can't find game object \"GameCanvas\".");
			}
		}

		return(Navi.instance);
	}

	// ================================================================ //
}

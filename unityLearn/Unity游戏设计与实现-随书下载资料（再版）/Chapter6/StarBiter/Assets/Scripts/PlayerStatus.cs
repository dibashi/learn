using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 玩家的状态
// ----------------------------------------------------------------------------
public class PlayerStatus : MonoBehaviour {

	public int playerCount = 3;						// 玩家数量
	
	public GameObject effectBomb;
	public GameObject GameOver;
	
	private enum State
	{
		INITIALIZE,
		INVISIBLE,			// 开始时无敌
		NOWPLAYING,
		STARTDESTRUCTION,	// 销毁玩家
		DESTRUCTION,		// 正在销毁玩家
		WAITING,			// 等待
		RESTART				// 再次开始
	}

	private State programState = State.INITIALIZE;	// 内部处理状态
	private int waitTimeAfterExplosion = 2;			// 播放爆炸动画后的等待时间
	
	private bool isGAMEOVER	= false;				// 游戏结束

	private GameObject scoutingLaser;
	private MeshRenderer invisibleZone;
	
	void Start () 
	{
		// 获取scoutingLaser的实例
		scoutingLaser = GameObject.FindGameObjectWithTag("ScoutingLaser");
		
		// 获取显示无敌的实例的MeshRenderer
		invisibleZone = GameObject.FindGameObjectWithTag("InvisibleZone").GetComponent<MeshRenderer>();
		
		// 获取score的实例
		//printScore = GameObject.FindGameObjectWithTag("Score");
		
		// 游戏开始时，显示玩家
		ShowPlayer();
		
		programState = State.NOWPLAYING;

		//
#if false
		if(dbwin.root().getWindow("プレイヤー.") == null) {
			
			var		window = dbwin.root().createWindow("プレイヤー.");

			window.createButton("自爆")
				.setOnPress(() =>
					{
						programState = State.STARTDESTRUCTION;
					});
		}
#endif
	}
	
	void Update ()
	{
	
		if ( !isGAMEOVER )
		{
			// 确认销毁
			if ( programState == State.STARTDESTRUCTION )
			{
				programState = State.DESTRUCTION;
				
				// 销毁玩家
				DestructPlayer();
			}
	
			// 等待
			if ( programState == State.DESTRUCTION )
			{
				if ( !this.GetComponent<AudioSource>().isPlaying )
				{
					programState = State.WAITING;
					
					// 等待一定的时间
					StartCoroutine("Waiting");
				}
			}
			
			// 再次开始游戏
			if ( programState == State.RESTART )
			{
				programState = State.INITIALIZE;
				
				// 由于游戏重新开始，再次显示玩家
				bool ret = ShowPlayer();
				if ( ret )
				{
					// 再次开始后的一段时间内无敌
					programState = State.INVISIBLE;
					// 显示无敌
					invisibleZone.enabled = true;
					// 设置无敌的解除时间
					StartCoroutine( WaitAndUpdateState( 2f, State.NOWPLAYING ) );
				}
				else
				{
					isGAMEOVER = true;
				}
			}
		}
	}

	// ------------------------------------------------------------------------
	// 玩家的毁坏判断
	// ------------------------------------------------------------------------
	void OnTriggerEnter( Collider collider )
	{
		if ( programState == State.NOWPLAYING ) {
			// 和岩石碰撞
			if ( collider.tag == "Stone" )
			{
				programState = State.STARTDESTRUCTION;
			}
			
			// 和敌机碰撞
			if ( collider.tag == "Enemy" )
			{
				programState = State.STARTDESTRUCTION;
			}
			
			// 和炮弹碰撞
			if ( collider.tag == "EnemyVulcan" )
			{
				programState = State.STARTDESTRUCTION;
			}
			
			// 和激光碰撞
			if ( collider.tag == "EnemyLaser" )
			{
				programState = State.STARTDESTRUCTION;
			}
			
			// 和炮弹发射碰撞
			if ( collider.tag == "EnemyShot" )
			{
				programState = State.STARTDESTRUCTION;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 显示玩家
	// ------------------------------------------------------------------------
	private bool ShowPlayer()
	{
		// 确认玩家数量为0
		if ( playerCount <= 0 )
		{
			// 更新最高得分
			bool isHiScore = SetHiscore();
			
			Navi.get().SetIsHiScore(isHiScore);

			// 显示游戏结束的文字
			Navi.get().ShowGameOver();

			// 设置跳转到开场画面的点击事件为有效
			GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ClickToOpening>().enabled = true;
			
			// 一定时间后返回开场画面
			StartCoroutine( WaitAndCallScene( 5f, "opening" ) );
			
			return false;
		}
		else
		{
		
			// 玩家数量减1
			playerCount--;
			
			// 显示玩家
			Component[] meshRenderers = this.GetComponentsInChildren<MeshRenderer>();
	        foreach ( MeshRenderer meshRenderer in meshRenderers ) {
	            meshRenderer.enabled = true;
	        }
			
			// 显示Player Left
			Navi.get().SetPlayerLeftCount(playerCount);

			//设置玩家状态为生存
			this.GetComponent<PlayerController>().SetIsAlive( true );
			
			return true;
		}
	}
	
	// ------------------------------------------------------------------------
	// 隐藏玩家
	// ------------------------------------------------------------------------
	private void HidePlayer()
	{
		// 隐藏玩家
		Component[] meshRenderers = this.GetComponentsInChildren<MeshRenderer>();
        foreach ( MeshRenderer meshRenderer in meshRenderers ) {
            meshRenderer.enabled = false;
        }
		
		// 设置玩家状态为死亡
		this.GetComponent<PlayerController>().SetIsAlive( true );
	}
	
	// ------------------------------------------------------------------------
	// 销毁玩家
	// ------------------------------------------------------------------------
	private void DestructPlayer()
	{
	
		// 隐藏玩家
		HidePlayer();
		
		// 将玩家控制的相关信息回滚到初始状态
		SendMessage( "Reset" );
		
		// 将索敌激光，锁定相关的信息回滚到初始状态
		scoutingLaser.SendMessage( "Reset" );
		
		// 爆炸
		ShowExplosion();

	}
	
	// ------------------------------------------------------------------------
	// 等待玩家被击毁后的复活
	// ------------------------------------------------------------------------
	IEnumerator Waiting()
	{
		// 等待一定事件
		yield return new WaitForSeconds( waitTimeAfterExplosion );

		// 清空画面
		ClearDisplay();
		
		// 重新开始游戏
		programState = State.RESTART;
	}
	
	// ------------------------------------------------------------------------
	// 显示玩家的爆炸
	// ------------------------------------------------------------------------
	private void ShowExplosion()
	{
		// 是否存在爆炸特效的对象？
		if ( effectBomb )
		{
			// 播放特效
			Instantiate(
				effectBomb,
				Vector3.zero, 
				new Quaternion(0f, 0f, 0f, 0f) );
		}
		
		// 生成爆炸声音
		this.GetComponent<AudioSource>().Play();
		
	}
	
	// ------------------------------------------------------------------------
	// 清空画面
	// ------------------------------------------------------------------------
	private void ClearDisplay()
	{	
		// 销毁所有EnemyMaker生成的敌机
		GameObject[] enemyMakers = GameObject.FindGameObjectsWithTag("EnemyMaker");
		foreach( GameObject enemyMaker in enemyMakers )
		{
			enemyMaker.SendMessage("DestroyEnemys");
		}
		
		// 销毁上面处理中未消除的单独敌机，编队外的敌机
		GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
		foreach( GameObject enemy in enemys )
		{
			enemy.SendMessage("DestroyEnemyEx");
		}
		
		// 销毁敌机的射击
		GameObject[] enemyShots = GameObject.FindGameObjectsWithTag("EnemyShot");
		foreach( GameObject enemyShot in enemyShots )
		{
			Destroy( enemyShot );
		}
		
		// 销毁BOSS的激光
		GameObject[] enemyLasers = GameObject.FindGameObjectsWithTag("EnemyLaser");
		foreach( GameObject enemyLaser in enemyLasers )
		{
			Destroy( enemyLaser );
		}
		
		// 销毁BOSS的Bullet
		GameObject[] enemyVulcans = GameObject.FindGameObjectsWithTag("EnemyVulcan");
		foreach( GameObject enemyVulcan in enemyVulcans )
		{
			Destroy( enemyVulcan );
		}
		
		// 销毁特效
		GameObject[] tmpGameObjects = GameObject.FindGameObjectsWithTag("EffectBomb");
	    for ( int i = 0; i < tmpGameObjects.Length; i++)
		{
			if ( tmpGameObjects[i] != null )
			{
				Destroy( tmpGameObjects[i] );
				tmpGameObjects[i] = null;
			}
	    }
	}
	
	// ------------------------------------------------------------------------
	// 返回是否正在游戏中
	//  - 不含有INVISIBLE
	// ------------------------------------------------------------------------
	public bool GetIsNOWPLAYING()
	{
		if ( programState == State.NOWPLAYING )
		{
			return true;
		}
		return false;
	}
	
	// ------------------------------------------------------------------------
	// 返回玩家是否能操作
	//  - 包含INVISIBLE
	// ------------------------------------------------------------------------
	public bool GetCanPlay()
	{
		if ( programState == State.NOWPLAYING ||
			 programState == State.INVISIBLE )
		{
			return true;
		}
		return false;
	}
	
	// ------------------------------------------------------------------------
	// 经过指定的时间后，修改状态
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUpdateState( float waitForSeconds, State state )
	{
		// 等待指定的时间
		yield return new WaitForSeconds( waitForSeconds );
		
		// 更新状态
		programState = state;
		
		// 不显示无敌状态
		invisibleZone.enabled = false;
	}
	
	// ------------------------------------------------------------------------
	// 经过指定的时间后，读入场景
	// ------------------------------------------------------------------------
	IEnumerator WaitAndCallScene( float waitForSeconds, string sceneName )
	{
		// 等待指定的时间
		yield return new WaitForSeconds( waitForSeconds );
		
		// 载入游戏场景
		UnityEngine.SceneManagement.SceneManager.LoadScene( sceneName );
	}
	
	// ------------------------------------------------------------------------
	// 存储HISCORE
	// ------------------------------------------------------------------------
	private bool SetHiscore()
	{
		// 获取SCORE/HISCORE
		int hiScore = Navi.get().GetHiScore();
		int	score = Navi.get().GetScore();
		
		// 是否超过了HISCORE？
		if ( score > hiScore )
		{
			// 更新最高得分
			Navi.get().SetHiScore(score);

			// 保存到GlobalParam
			GlobalParam.GetInstance().SetHiScore( score );
			
			return true;
		}
		return false;
	}
	
}

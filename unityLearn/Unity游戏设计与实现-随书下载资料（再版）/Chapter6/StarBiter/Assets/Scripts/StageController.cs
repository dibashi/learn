using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// StageController
//  - 关卡控制
//  - 使用方法
//    - 配置添加了这个脚本的游戏对象
//  - 运动规则
//    - 关卡状态如下变化
//      1.  开始
//      2.  和侦察的敌机相遇
//          - 敌机类型: TYPE 01 + TYPE 04
//      3.  第1波攻击
//          - 敌机种类: TYPE 01 + TYPE 01 战斗阵型
//      4.  和侦察的敌机相遇
//          - 敌机种类: TYPE 01 + TYPE 02 + TYPE 04
//      5.  第2波攻击
//          - 敌机种类: TYPE 01 + TYPE 02 + TYPE 02 战斗阵型
//      6.  和侦察的敌机相遇
//          - 敌机种类: TYPE 01 + TYPE 02 + TYPE 03 + TYPE 04
//      7.  第3波攻击
//          - 敌机种类: TYPE 01 + TYPE 02 + TYPE 03 + TYPE 03 战斗阵型
//      8.  短暂的寂静
//      9.  BOSS登场
//      10. BOSS战斗
//      11. 游戏结束
//    - ※战斗阵型: 敌机编队攻击
// ---------------------------------------------------------------------------
public class StageController : MonoBehaviour {

	public GameObject EnemyMakerType01Formation;	// ENEMY MAKER TYPE 01 FORMATION
	public GameObject EnemyMakerType02Formation;	// ENEMY MAKER TYPE 02 FORMATION
	public GameObject EnemyMakerType03Formation;	// ENEMY MAKER TYPE 03 FORMATION
	public GameObject EnemyMakerType01;		// ENEMY MAKER TYPE 01
	public GameObject EnemyMakerType02;		// ENEMY MAKER TYPE 02
	public GameObject EnemyMakerType03;		// ENEMY MAKER TYPE 03
	public GameObject EnemyMakerType04;		// ENEMY MAKER TYPE 04
	public GameObject Boss;					// BOSS
	public GameObject GameOver;
	
	private enum Level
	{
		DEBUG,
		EASY,
		NORMAL,
		HARD
	}
	private Level level = Level.NORMAL;
	
	// 各个关卡的生成数
	//  - 0: TYPE01.
	//  - 1: TYPE01 FORMATION.
	//  - 2: TYPE02.
	//  - 3: TYPE02 FORMATION.
	//  - 4: TYPE03.
	//  - 5: TYPE03 FORMATION.
	private int[,] maxEnemyInSceneByLevel =
	{
		{ 0, 0, 0, 0, 0, 0 },
		{ 1, 1, 1, 1, 1, 1 },
		{ 3, 4, 3, 2, 1, 2 },
		{ 6, 6, 6, 4, 6, 4 },
	};

	private enum EnemyType
	{
		TYPE01,
		TYPE01FORMATION,
		TYPE02,
		TYPE02FORMATION,
		TYPE03,
		TYPE03FORMATION,
	}
	
	// 各个状态
	private enum Stage
	{
		START,								// 开始
		PATROL1,							// 侦察1
		ATTACK1,							// 第1波攻击
		PATROL2,							// 侦察2
		ATTACK2,							// 第2波攻击
		PATROL3,							// 侦察3
		ATTACK3,							// 第3波攻击
		SILENCE,							// 寂静
		BOSS,								// BOSS
		GAMECLEAR,							// 清空游戏
		END
	}
	
	// 关卡场景的各个状态
	private enum State
	{
		INITIALIZE,							// 准备
		NOWPLAYING,							// 游戏中
		END									// 结束
	}
	
	private Stage stage;					// 关卡场景
	private State state;					// 关卡场景的状态
	
	private GameObject enemyMakerType01Formation;	// ENEMY MAKER TYPE 01 FORMATION 实例
	private GameObject enemyMakerType02Formation;	// ENEMY MAKER TYPE 02 FORMATION 实例
	private GameObject enemyMakerType03Formation;	// ENEMY MAKER TYPE 03 FORMATION 实例
	private GameObject enemyMakerType01;	// ENEMY MAKER TYPE 01 实例
	private GameObject enemyMakerType02;	// ENEMY MAKER TYPE 02 实例
	private GameObject enemyMakerType03;	// ENEMY MAKER TYPE 03 实例
	private GameObject enemyMakerType04;	// ENEMY MAKER TYPE 04 实例
	private GameObject boss;				// BOSS 实例
	private PlayerStatus playerStatus;		// 玩家状态的实例
	private PrintMessage printMessage;	// SubScreen的消息区域
	
	private GameObject txtStageController;	// DEBUG
	
	void Start () 
	{
		// --------------------------------------------------------------------
		// 获取各个实例
		// --------------------------------------------------------------------
		
		// 获取PrintMessage的实例
		printMessage = Navi.get().GetPrintMessage();
		
		// 获取玩家状态的实例
		playerStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStatus>();
		
		// --------------------------------------------------------------------
		// 初始值
		// --------------------------------------------------------------------
		
		// 设定关卡场景
		stage = Stage.START;
		state = State.INITIALIZE;
		
		// --------------------------------------------------------------------
		// 生成各个实例
		// --------------------------------------------------------------------

		if ( EnemyMakerType01Formation )
		{
			enemyMakerType01Formation = Instantiate(
				EnemyMakerType01Formation,
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType02Formation )
		{
			enemyMakerType02Formation = Instantiate(
				EnemyMakerType02Formation,
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType03Formation )
		{
			enemyMakerType03Formation = Instantiate( 
				EnemyMakerType03Formation,
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType01 )
		{
			enemyMakerType01 = Instantiate( 
				EnemyMakerType01,
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType02 )
		{
			enemyMakerType02 = Instantiate(
				EnemyMakerType02,
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType03 )
		{
			enemyMakerType03 = Instantiate( 
				EnemyMakerType03, 
				Vector3.zero, 
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType04 )
		{
			enemyMakerType04 = Instantiate( 
				EnemyMakerType04, 
				Vector3.zero, 
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( Boss )
		{
			boss = Instantiate(
				Boss, 
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		
	}
	
	void Update () 
	{
		
		// 更新关卡场景
		UpdateStage();

	}
	
	// ------------------------------------------------------------------------
	// 各个关卡场景处理
	// ------------------------------------------------------------------------
	private void UpdateStage()
	{
		// 只有玩家允许操作使才会继续关卡处理
		if ( playerStatus.GetIsNOWPLAYING() )
		{			
			// 开始
			StageStart();
			
			// 开场结束
			StageStartEnd();
			
			// 侦察(1)
			StagePatrol1Start();
			
			// 侦察(1)结束
			StagePatrol1End();
			
			// 第1波攻击
			StageAttack1Start();
			
			// 第1波攻击结束
			StageAttack1End();
			
			// 侦察(2)
			StagePatrol2Start();
			
			// 侦察(2)结束
			StagePatrol2End();
	
			// 第2波攻击
			StageAttack2Start();
			
			// 第2波攻击结束
			StageAttack2End();
			
			// 侦察(3)
			StagePatrol3Start();
			
			// 侦察(3)结束
			StagePatrol3End();
			
			// 第3波攻击
			StageAttack3Start();
			
			// 第3波攻击结束
			StageAttack3End();
	
			// 短暂的寂静
			SilenceStart();
			
			// 短暂的寂静结束
			SilenceEnd();
			
			// BOSS战 开始
			BossAttackStart();
			
			// BOSS战 结束
			BossAttackEnd();
			
			// GameClear
			GameClearWait();
			
			// GameClear
			GameClearMessage();
		}
	}

	// ------------------------------------------------------------------------
	// 关卡场景 - 开始
	// ------------------------------------------------------------------------
	private void StageStart()
	{
		if ( stage == Stage.START && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// 开始生成TYPE 01 
			if ( enemyMakerType01 )
			{
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE01
					] );
			}
			
			// 设定关卡的结束时间
			StartCoroutine( WaitAndUpdateState( 3f, State.END ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - 开场结束
	// ------------------------------------------------------------------------
	private void StageStartEnd()
	{
		if ( stage == Stage.START && state == State.END )
		{
			// 跳转到下一场景
			stage = Stage.PATROL1;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - 侦察(1)
	// ------------------------------------------------------------------------
	private void StagePatrol1Start()
	{
		if ( stage == Stage.PATROL1 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;	

			// 开始TYPE 04 的生成
			if ( enemyMakerType04 )
			{
				enemyMakerType04.GetComponent<EnemyMaker>().
					SetMaxEnemysInScene( 1 );
				enemyMakerType04.GetComponent<EnemyMaker>().
					SetStage( (int)stage );
			
				// 向子窗体输出消息
				printMessage.SetMessage(" ");
				printMessage.SetMessage("PATROL SHIP IS COMING AHEAD.");
				printMessage.SetMessage(" ");
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - 侦察(1)结束
	// ------------------------------------------------------------------------
	private void StagePatrol1End()
	{
		if ( stage == Stage.PATROL1 && state == State.END )
		{
			// 跳转到下一场景
			stage = Stage.ATTACK1;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - 第1波攻击
	// ------------------------------------------------------------------------
	private void StageAttack1Start()
	{
		if ( stage == Stage.ATTACK1 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
					
			// 开始生成TYPE 01 FORMATION
			if ( enemyMakerType01Formation )
			{
				enemyMakerType01Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE01FORMATION
					] );
				enemyMakerType01Formation.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );
			
				//向子窗体输出消息
				printMessage.SetMessage(" ");
				printMessage.SetMessage("BATTLE STATIONS HAS APPROACHED.");
				printMessage.SetMessage(" ");
			}
			
			// 设定关卡的结束时间
			StartCoroutine( WaitAndUpdateState( 20f, State.END ) );

		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - 第1波攻击结束
	// ------------------------------------------------------------------------
	private void StageAttack1End()
	{
		if ( stage == Stage.ATTACK1 && state == State.END )
		{
			// 停止生成TYPE 01 FORMATION
			if ( enemyMakerType01Formation )
			{
				enemyMakerType01Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}
		
			// 跳转到下一场景
			stage = Stage.PATROL2;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - 侦察(2)
	// ------------------------------------------------------------------------
	private void StagePatrol2Start()
	{
		if ( stage == Stage.PATROL2 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// 开始TYPE 02 的生成
			if ( enemyMakerType02 )
			{
				enemyMakerType02.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE02
					] );
			}
			
			// 开始TYPE 04 的生成
			if ( enemyMakerType04 )
			{
				enemyMakerType04.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 1 );
				enemyMakerType04.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );
				
				// 向子窗体输出消息
				printMessage.SetMessage(" ");
				printMessage.SetMessage("PATROL SHIP IS COMING AHEAD.");
				printMessage.SetMessage(" ");
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - 侦察(2)结束
	// ------------------------------------------------------------------------
	private void StagePatrol2End()
	{
		if ( stage == Stage.PATROL2 && state == State.END )
		{
			// 跳转到下一场景
			stage = Stage.ATTACK2;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - 第2波攻击
	// ------------------------------------------------------------------------
	private void StageAttack2Start()
	{
		if ( stage == Stage.ATTACK2 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// 增加TYPE 01 的生成量
			if ( enemyMakerType01 )
			{
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE01
					] );
			}
			
			// 开始生成TYPE 02 FORMATION
			if ( enemyMakerType02Formation )
			{
				enemyMakerType02Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE02FORMATION
					] );
				enemyMakerType02Formation.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );
				
				// 向子窗体输出消息
				printMessage.SetMessage(" ");
				printMessage.SetMessage("BATTLE STATIONS HAS APPROACHED.");
				printMessage.SetMessage(" ");
			}
			
			// 设定关卡的结束时间
			StartCoroutine( WaitAndUpdateState( 20f, State.END ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - 第2波攻击结束
	// ------------------------------------------------------------------------
	private void StageAttack2End()
	{
		if ( stage == Stage.ATTACK2 && state == State.END )
		{
			// 停止TYPE 02 FORMATION 的生成
			if ( enemyMakerType02Formation )
			{
				enemyMakerType02Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}
			
			// 返回TYPE 01 的生成量
			if ( enemyMakerType01 )
			{
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE01
					] );
				// 设置允许射击
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetCanShoot( true );
			}

			// 跳转到下一场景
			stage = Stage.PATROL3;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - 侦察(3)
	// ------------------------------------------------------------------------
	private void StagePatrol3Start()
	{
		if ( stage == Stage.PATROL3 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// 开始生成TYPE 03 
			if ( enemyMakerType03 )
			{
				enemyMakerType03.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE03
					] );
			}
			
			// 开始生成TYPE 04 
			if ( enemyMakerType04 )
			{
				enemyMakerType04.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 1 );
				enemyMakerType04.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );
			
				// 向子窗体输出消息
				printMessage.SetMessage(" ");
				printMessage.SetMessage("PATROL SHIP IS COMING AHEAD.");
				printMessage.SetMessage(" ");
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - 侦察（3）结束
	// ------------------------------------------------------------------------
	private void StagePatrol3End()
	{
		if ( stage == Stage.PATROL3 && state == State.END )
		{
			// 跳转到下一场景
			stage = Stage.ATTACK3;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - 第3波攻击
	// ------------------------------------------------------------------------
	private void StageAttack3Start()
	{
		if ( stage == Stage.ATTACK3 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// 增加TYPE 01 的生成量
			if ( enemyMakerType01 )
			{
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE01
					] );
			}
			
			// 增加TYPE 02 的生成量
			if ( enemyMakerType02 )
			{
				enemyMakerType02.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE02
					] );
				// 提高速度
				enemyMakerType02.GetComponent<EnemyMaker>()
					.SetAddToSpeed( true );
			}
			
			// 开始生成TYPE 03 FORMATION
			if ( enemyMakerType03Formation )
			{
				enemyMakerType03Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE03FORMATION
					] );
				enemyMakerType03Formation.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );
				
				// 向子窗体输出消息
				printMessage.SetMessage(" ");
				printMessage.SetMessage("BATTLE STATIONS HAS APPROACHED.");
				printMessage.SetMessage(" ");
			}
			
			// 设定关卡的结束时间
			StartCoroutine( WaitAndUpdateState( 20f, State.END ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - 第3波攻击结束
	// ------------------------------------------------------------------------
	private void StageAttack3End()
	{
		if ( stage == Stage.ATTACK3 && state == State.END )
		{
			// 停止TYPE 03 FORMATION 的生成
			if ( enemyMakerType03Formation )
			{
				enemyMakerType03Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}
			
			// 停止TYPE 01 的生成
			if ( enemyMakerType01 )
			{
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}
			
			// 停止TYPE 02 的生成
			if ( enemyMakerType02 )
			{
				enemyMakerType02.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}
			
			// 停止TYPE 03 的生成
			if ( enemyMakerType03 )
			{
				enemyMakerType03.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}
			
			// 跳转到下一场景
			stage = Stage.SILENCE;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - 短暂的寂静
	// ------------------------------------------------------------------------
	private void SilenceStart()
	{
		if ( stage == Stage.SILENCE && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// 向子窗体输出消息
			printMessage.SetMessage(" ");
			printMessage.SetMessage("CAUTION!");
			printMessage.SetMessage("CAUGHT HIGH ENERGY REACTION AHEAD.");
			printMessage.SetMessage(" ");			
			// 设定关卡的结束时间
			StartCoroutine( WaitAndUpdateState( 10f, State.END ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - 结束短暂的寂静
	// ------------------------------------------------------------------------
	private void SilenceEnd()
	{
		if ( stage == Stage.SILENCE && state == State.END )
		{	
			// 跳转到下一场景
			stage = Stage.BOSS;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - BOSS战 开始
	// ------------------------------------------------------------------------
	private void BossAttackStart()
	{
		if ( stage == Stage.BOSS && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// 生成BOSS
			if ( boss )
			{
				boss.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 1 );
				boss.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );
				
				// 向子窗体输出消息
				printMessage.SetMessage(" ");
				printMessage.SetMessage("ACKNOWKEDGED SPIDER-TYPE");
				printMessage.SetMessage("THE LIMITED-WARFARE ATTACK WEAPON.");
				printMessage.SetMessage(" ");
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - BOSS战 结束
	// ------------------------------------------------------------------------
	private void BossAttackEnd()
	{
		if ( stage == Stage.BOSS && state == State.END )
		{
			// 跳转到下一关卡场景
			stage = Stage.GAMECLEAR;
			state = State.INITIALIZE;
		}
	}

	// ------------------------------------------------------------------------
	// 关卡场景 - GameClear.
	// ------------------------------------------------------------------------
	private void GameClearWait()
	{
		if ( stage == Stage.GAMECLEAR && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// 设定BOSS战结束的等待事件
			StartCoroutine( WaitAndUpdateState( 2f, State.END ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 如果SCORE超过了HISCORE则存储
	// ------------------------------------------------------------------------
	private bool SetHiscore()
	{
		// 获取SCORE/HISCORE
		int hiScore = Navi.get().GetHiScore();
		int	score = Navi.get().GetScore();
		
		// 判断是否超过HISCORE？
		if ( score > hiScore )
		{
			// 更新得分记录
			Navi.get().SetHiScore(score);

			// 存储在GlobalParam区域中
			GlobalParam.GetInstance().SetHiScore( score );
			
			return true;
		}
		return false;
	}
	
	// ------------------------------------------------------------------------
	// 关卡场景 - GameClear.
	// ------------------------------------------------------------------------
	private void GameClearMessage()
	{
		if ( stage == Stage.GAMECLEAR && state == State.END )
		{
			stage = Stage.END;
				
			// 更新得分记录
			bool isHiScore = SetHiscore();

			Navi.get().SetIsHiScore(isHiScore);
			
			// 显示游戏结束的文字
			Navi.get().ShowMisssionAccomplished();
			
			// 使转向开场场景的点击事件有效
			GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ClickToOpening>().enabled = true;
			
			// 经过一定时间后返回开场画面
			StartCoroutine( WaitAndCallScene( 5f, "ending" ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 结果指定的时间后调用场景
	// ------------------------------------------------------------------------
	IEnumerator WaitAndCallScene( float waitForSeconds, string sceneName )
	{
		// 等待指定的时间
		yield return new WaitForSeconds( waitForSeconds );
		
		// 载入游戏场景
		UnityEngine.SceneManagement.SceneManager.LoadScene( sceneName );
	}
	
	// ------------------------------------------------------------------------
	// 更新关卡的状态
	// ------------------------------------------------------------------------
	public void SetStateEnd( int stageIndex )
	{
		// 只有在关卡未被其他处理更新时才执行
		if ( stageIndex == (int)stage )
		{
			state = State.END;
		}
	}
	
	// ------------------------------------------------------------------------
	// 等待关卡结束处理
	//  - 结果指定的时间后更新关卡的状态
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUpdateState( float waitForSeconds, State state )
	{
		Stage tmpStage = stage;
		
		// 等待指定的时间
		yield return new WaitForSeconds( waitForSeconds );
		
		// 只有在关卡未改变时处理
		if ( tmpStage == stage )
		{
			// 更新关卡的状态
			this.state = state;
		}
	}
	
	// ------------------------------------------------------------------------
	// DEBUG
	// ------------------------------------------------------------------------
	
	// 返回关卡名称
	public string GetStage()
	{
		return stage.ToString();
	}
	
	// 强制设置关卡
	public void SetStage( string stage )
	{
		
		// 设置关卡
		if ( stage == "START" )   {	this.stage = Stage.START; 	}
		if ( stage == "PATROL1" ) {	this.stage = Stage.PATROL1; }
		if ( stage == "ATTACK1" ) {	this.stage = Stage.ATTACK1; }
		if ( stage == "PATROL2" ) {	this.stage = Stage.PATROL2; }
		if ( stage == "ATTACK2" ) {	this.stage = Stage.ATTACK2; }
		if ( stage == "PATROL3" ) {	this.stage = Stage.PATROL3; }
		if ( stage == "ATTACK3" ) {	this.stage = Stage.ATTACK3; }
		if ( stage == "SILENCE" ) {	this.stage = Stage.SILENCE; }
		if ( stage == "BOSS" ) 	  {	this.stage = Stage.BOSS; 	}
		if ( stage == "GAMECLEAR" ){ this.stage = Stage.GAMECLEAR;}
		
		// 设置关卡状态为初始化
		this.state = State.INITIALIZE;
	}
	
	// 返回关卡的文字
	public string GetLevelText()
	{
		return level.ToString();
	}
	
	// 返回关卡
	public int GetLevel()
	{
		return (int)level;
	}
	
	// 存储关卡数据
	public void SetLevel( int level )
	{
		this.level = (Level)level;
	}
}

using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// BossController
//  - 控制“BOSS”的运动
//  - 使用方法
//    - 附加到BOSS
//  - 运动流程
//    - 在画面上方出现
//    - 回避->攻击前的移动->按攻击的顺序运动（循环）
// ----------------------------------------------------------------------------
public class BossController : MonoBehaviour {
	
	public float startSpeed = 5f;						// BOSS出现时的速度
	public float turnRate = 5f;							// BOSS在距离合适时变换方向的概率
	public float escapeRate = 5f;						// BOSS向左右回避时从当前位置到移动目标运动时的概率
	public float jumpSpeed = 30f;						// BOSS逃向画面外的速度
	public float escapeSpeed = 5f;						// BOSS向左右回避时从当前位置到移动目标运动时的速度
	public float escapeTime = 5f;						// 回避的时间
	public float waitTimeBeforeAttack = 3f;				// 攻击前的等待时间
	public float waitTimeJustBeforeAttack = 0.7f;		// 攻击瞬间的等待时间
	public float escapeSpeedJustBeforeAttack = 4f;		// 攻击前获取距离时的速度
	public float Attack1Time = 7f;						// 攻击1的时间
	public float Attack2Time = 4f;						// 攻击2的时间
	public float Attack3Time = 3f;						// 攻击3的时间
	
	private GameObject player;							// 玩家
	private PlayerStatus playerStatus;					// 玩家状态的实例
	private BattleSpaceController battleSpaceContoller;	// 战斗空间
	private float startPositionZ = -12.0f;				// 出现位置（到玩家（Z＝0）的距离）
	private float distanceToPlayer = 7.0f;				// 和玩家之间的距离
	private float distanceToPlayerJustBeforeAttack = 8.0f;	// 攻击前瞬间和玩家的距离
	private float distanceToPlayerMove1 = 4.0f;			// 和玩家的距离
	private float distanceToPlayerMove2 = 5.0f;			// 和玩家的距离
	private float distanceToPlayerMove3 = 4.0f;			// 和玩家的距离
	
	private BulletMaker bulletMakerLeft;
	private BulletMaker bulletMakerRight;
	private LaserMaker laserMakerLeft;
	private LaserMaker laserMakerRight;
	private ShotMaker shotMaker;
	private EnemyStatusBoss enemyStatusBoss;
	private Animation bossAnimation;
	
	private Vector3 destinationPosition = Vector3.zero;	// 往左右回避的目标位置
	private bool isEscape = false;						// 正在向左右躲避
	
	private enum State
	{
		START,				// BOSS战斗开始
		TOPLAYER,			// 朝向玩家
		ESCAPE1START,		// 开始回避
		ESCAPE1END,			// 回避中～结束
		ESCAPE2START,
		ESCAPE2END,
		ESCAPE3START,
		ESCAPE3END,
		MOVE1START,			// 开始向着攻击位置移动
		MOVE1END,			// 移动中～结束
		MOVE2START,
		MOVE2END,
		MOVE3START,
		MOVE3END,
		JUSTBEFORE1START,	// 攻击前的动作开始
		JUSTBEFORE1END,		// 攻击前的动作～结束
		JUSTBEFORE2START,
		JUSTBEFORE2END,
		JUSTBEFORE3START,
		JUSTBEFORE3END,
		ATTACK1START,		// 攻击开始
		ATTACK1END,			// 攻击中～结束
		ATTACK2START,
		ATTACK2END,
		ATTACK3START,
		ATTACK3END
	}
	private State state = State.TOPLAYER;
	
	void Start () {
	
		// 取得玩家的实例
		player = GameObject.FindGameObjectWithTag("Player");
		
		// 取得玩家状态的实例
		playerStatus = player.GetComponent<PlayerStatus>();
		
		// 取得BOSS的各个部件
		bulletMakerLeft = GameObject.Find("BossVulcanLeft").GetComponent<BulletMaker>();
		bulletMakerRight = GameObject.Find("BossVulcanRight").GetComponent<BulletMaker>();
		laserMakerLeft = GameObject.Find("BossLaserLeft").GetComponent<LaserMaker>();
		laserMakerRight = GameObject.Find("BossLaserRight").GetComponent<LaserMaker>();
		shotMaker = GameObject.Find("BossCore").GetComponent<ShotMaker>();
		enemyStatusBoss = GetComponent<EnemyStatusBoss>();
		
		// 取得动画对象
		bossAnimation = GetComponent<Animation>();
		
		// 取得战斗空间
		battleSpaceContoller = GameObject.FindGameObjectWithTag("BattleSpace").GetComponent<BattleSpaceController>();
		
		// --------------------------------------------------------------------
		// 指定出现的位置
		// --------------------------------------------------------------------
		
		// 首先设置到玩家的位置
		transform.position = player.transform.position;
		transform.rotation = Quaternion.Euler( 0, 180, 0 );
		
		// 调整位置
		transform.Translate ( new Vector3( 0f, 0f, startPositionZ ) );
		
	}
	
	void Update () {
		
		// 确认和玩家之间的距离
		float distance = Vector3.Distance(
			player.transform.position,
			transform.position );
		if ( state == State.TOPLAYER && distance < distanceToPlayer )
		{
			state = State.ESCAPE1START;
			destinationPosition = transform.position;
		}
		
		// 从画面上部出现的处理
		if ( state == State.TOPLAYER )
		{
			transform.position += transform.forward * startSpeed * Time.deltaTime;
		}
		
		// 只在允许玩家操作时处理
		if ( playerStatus.GetIsNOWPLAYING() ) {
		
			// 攻击1
			Attack1();
		
			// 攻击2
			Attack2();
		
			// 攻击3
			Attack3();
			
			// 攻击过程中的处理
			Attacking();
		
			// 取得攻击过程中和玩家间的距离
			SetDistanceToPlayerAtAttack();
			
			// 回避1
			Escape1();

			// 回避2
			Escape2();
			
			// 回避3
			Escape3();
	
			// 向攻击位置移动1
			Move1();

			// 向攻击位置移动2
			Move2();

			// 向攻击位置移动3
			Move3();
			
			// 攻击前的动作1
			MotionJustBeforeAttack1();

			// 攻击前的动作2
			MotionJustBeforeAttack2();

			// 攻击前的动作3
			MotionJustBeforeAttack3();
			
			// 攻击前瞬间获取和玩家之间的距离
			SetDistanceToPlayerJustBefortAttack();
			
			// 获取和玩家间的距离
			SetDistanceToPlayer();
			
			// 回避
			EscapeFromPlayer();
		
		}
	
		// 叠加战斗空间的滚动方向
		transform.position -= battleSpaceContoller.GetAdditionPos();
		
	}

	// ------------------------------------------------------------------------
	// BOSS - 攻击1
	// ------------------------------------------------------------------------
	private void Attack1()
	{
		// 攻击1
		if ( state == State.ATTACK1START )
		{
			// 修改状态
			state = State.ATTACK1END;
			
			// 只有当机体存在时才能攻击
			if ( bulletMakerLeft || bulletMakerRight )
			{
				// 播放动画
				bossAnimation.Play();
					
				// 扫尾攻击
				if ( bulletMakerLeft ) bulletMakerLeft.SetIsFiring();
				if ( bulletMakerRight ) bulletMakerRight.SetIsFiring();					
				
				StartCoroutine( WaitAndUpdateState( Attack1Time, State.ESCAPE2START ) );
			}
			else
			{
				// 如果没有机体则跳到下一次攻击
				state = State.ATTACK2START;
			}
			
		}
	}

	// ------------------------------------------------------------------------
	// BOSS - 攻击2
	// ------------------------------------------------------------------------
	private void Attack2()
	{
		// 攻击2
		if ( state == State.ATTACK2START )
		{
			// 修改状态
			state = State.ATTACK2END;
			
			// 只有机体存在时才允许攻击
			if ( laserMakerLeft || laserMakerRight )
			{
				// 激光攻击
				if ( laserMakerLeft ) laserMakerLeft.SetIsFiring();
				if ( laserMakerRight ) laserMakerRight.SetIsFiring();
				
				StartCoroutine( WaitAndUpdateState( Attack2Time, State.ESCAPE3START ) );
			}
			else
			{
				// 如果没有机体则跳到下一次攻击
				state = State.ATTACK3START;
			}
		}
	}

	// ------------------------------------------------------------------------
	// BOSS - 攻击3
	// ------------------------------------------------------------------------
	private void Attack3()
	{
		// 攻击3
		if ( state == State.ATTACK3START )
		{
			state = State.ATTACK3END;

			// 射击
			if ( shotMaker ) shotMaker.SetIsFiring();
					
			StartCoroutine( WaitAndUpdateState( Attack3Time, State.ESCAPE1START ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 攻击过程中的处理
	// ------------------------------------------------------------------------
	private void Attacking()
	{
		// 攻击结束后
		if ( state == State.ATTACK1END ||
			 state == State.ATTACK2END ||
			 state == State.ATTACK3END )
		{
			// 取得玩家的方向
			Vector3 relativePosition = player.transform.position - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );

			// 获取按指定速度从BOSS现在的方向到目标的方向倾斜后的角度
			float targetRotationAngle = targetRotation.eulerAngles.y;
			float currentRotationAngle = transform.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnRate * Time.deltaTime );
			Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
				
			// 将BOSS的方向朝向玩家
			transform.rotation = tiltedRotation;
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 回避1
	// ------------------------------------------------------------------------
	private void Escape1()
	{
		// 回避1
		if ( state == State.ESCAPE1START )
		{
			state = State.ESCAPE1END;
			
			// 只有机体存在时才执行回避1
			if ( bulletMakerLeft || bulletMakerRight )
			{
				SetEscapeTime();
				StartCoroutine( WaitAndUpdateState( escapeTime, State.MOVE1START ) );
			}
			else
			{
				// 如果没有机体则跳到下一次攻击
				state = State.ESCAPE2START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 回避2
	// ------------------------------------------------------------------------
	private void Escape2()
	{
		// 回避2
		if ( state == State.ESCAPE2START )
		{
			state = State.ESCAPE2END;
			
			// 只有机体存在时才执行回避2
			if ( laserMakerLeft || laserMakerRight )
			{
				SetEscapeTime();
				StartCoroutine( WaitAndUpdateState( escapeTime, State.MOVE2START ) );
			}
			else
			{
				// 如果没有机体则跳到下一次攻击
				state = State.ESCAPE3START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 回避3
	// ------------------------------------------------------------------------
	private void Escape3()
	{
		// 回避3
		if ( state == State.ESCAPE3START )
		{
			state = State.ESCAPE3END;
			SetEscapeTime();
			StartCoroutine( WaitAndUpdateState( escapeTime, State.MOVE3START ) );			
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 向攻击位置移动1
	// ------------------------------------------------------------------------
	private void Move1()
	{
		// 移动1
		if ( state == State.MOVE1START )
		{
			state = State.MOVE1END;
			
			// 只有机体存在时执行移动1
			if ( bulletMakerLeft || bulletMakerRight )
			{
				isEscape = false;
				StartCoroutine( WaitAndUpdateState( waitTimeBeforeAttack, State.JUSTBEFORE1START ) );
			}
			else
			{
				// 如果没有机体则跳到下一次攻击 
				state = State.MOVE2START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 向攻击位置移动2
	// ------------------------------------------------------------------------
	private void Move2()
	{
		// 移动2
		if ( state == State.MOVE2START )
		{
			state = State.MOVE2END;
			
			// 只有机体存在时才执行移动2
			if ( laserMakerLeft || laserMakerRight )
			{
				isEscape = false;
				StartCoroutine( WaitAndUpdateState( waitTimeBeforeAttack, State.JUSTBEFORE2START ) );
			}
			else
			{
				// 如果没有机体则跳到下一次攻击
				state = State.MOVE3START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 向攻击位置移动3
	// ------------------------------------------------------------------------
	private void Move3()
	{
		// 移动3
		if ( state == State.MOVE3START )
		{
			state = State.MOVE3END;
			isEscape = false;
			StartCoroutine( WaitAndUpdateState( waitTimeBeforeAttack, State.JUSTBEFORE3START ) );			
		}
	}
	
	
	// ------------------------------------------------------------------------
	// BOSS - 攻击前的动作1
	// ------------------------------------------------------------------------
	private void MotionJustBeforeAttack1()
	{
		// 攻击前的动作1
		if ( state == State.JUSTBEFORE1START )
		{
			state = State.JUSTBEFORE1END;
			
			// 只有机体存在时才执行攻击前动作1
			if ( bulletMakerLeft || bulletMakerRight )
			{
				isEscape = false;
				StartCoroutine( WaitAndUpdateState( waitTimeJustBeforeAttack, State.ATTACK1START ) );
			}
			else
			{
				// 如果没有机体则跳到下一个攻击前动作
				state = State.JUSTBEFORE2START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 攻击前动作2
	// ------------------------------------------------------------------------
	private void MotionJustBeforeAttack2()
	{
		// 攻击前动作2
		if ( state == State.JUSTBEFORE2START )
		{
			state = State.JUSTBEFORE2END;
			
			// 只有当机体存在时才执行攻击前动作2
			if ( laserMakerLeft || laserMakerRight )
			{
				isEscape = false;
				StartCoroutine( WaitAndUpdateState( waitTimeJustBeforeAttack, State.ATTACK2START ) );
			}
			else
			{
				// 如果没有机体则跳到下一个攻击前动作
				state = State.JUSTBEFORE3START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 攻击前动作3
	// ------------------------------------------------------------------------
	private void MotionJustBeforeAttack3()
	{
		// 攻击前动作3
		if ( state == State.JUSTBEFORE3START )
		{
			isEscape = false;
			state = State.ATTACK3START;
		}
	}
	
	// ------------------------------------------------------------------------
	// 获取和玩家之间的距离
	// ------------------------------------------------------------------------
	private void SetDistanceToPlayer()
	{
		// 获取和玩家之间的距离
		if (
			state == State.MOVE1END ||
			state == State.MOVE2END ||
			state == State.MOVE3END )
		{
			// 获取玩家的方向
			Vector3 relativePosition = player.transform.position - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );

			// 获取按指定速度从BOSS当前的方向往目标方向倾斜后的角度
			float targetRotationAngle = targetRotation.eulerAngles.y;
			float currentRotationAngle = transform.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnRate * Time.deltaTime );
			Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
				
			// 将BOSS的方向朝向玩家
			transform.rotation = tiltedRotation;
			
			// 获取和玩家间的距离
			float tmpDistanceToPlayer = 0;
			if ( state == State.MOVE1END ) { tmpDistanceToPlayer = distanceToPlayerMove1; }
			if ( state == State.MOVE2END ) { tmpDistanceToPlayer = distanceToPlayerMove2; }
			if ( state == State.MOVE3END ) { tmpDistanceToPlayer = distanceToPlayerMove3; }
			float distance5 = Vector3.Distance(
				player.transform.position,
				transform.position );

			if ( distance5 < tmpDistanceToPlayer )
			{
				transform.position -= transform.forward * Time.deltaTime * ( tmpDistanceToPlayer - distance5 ) * 2;
			}
			else
			{
				transform.position += transform.forward * Time.deltaTime * ( distance5 - tmpDistanceToPlayer ) * 2;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 获取攻击前和玩家间的距离
	// ------------------------------------------------------------------------
	private void SetDistanceToPlayerJustBefortAttack()
	{
		// 获取攻击过程中和玩家的距离
		if (
			state == State.JUSTBEFORE1END ||
			state == State.JUSTBEFORE2END ||
			state == State.JUSTBEFORE3END )
		{
			
			// 获取玩家和BOSS的距离
			float distance = Vector3.Distance(
				player.transform.position,
				transform.position );
			
			// 使BOSS朝向玩家
			Vector3 playerRelativePositionByBoss = player.transform.position - transform.position;
			Quaternion playerTargetRotationByBoss = Quaternion.LookRotation( playerRelativePositionByBoss );
			transform.rotation = playerTargetRotationByBoss;

			// 获取和玩家间的距离
			if ( distance < distanceToPlayerJustBeforeAttack )
			{
				transform.position -=
					transform.forward
						* Time.deltaTime
						* ( distanceToPlayerJustBeforeAttack - distance )
						* escapeSpeedJustBeforeAttack;
			}
			else
			{
				transform.position +=
					transform.forward
						* Time.deltaTime
						* ( distance - distanceToPlayerJustBeforeAttack )
						* escapeSpeedJustBeforeAttack;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 获取攻击过程中和玩家间的距离
	// ------------------------------------------------------------------------
	private void SetDistanceToPlayerAtAttack()
	{
		// 获取攻击过程中和玩家间的距离
		if (
			state == State.ATTACK1END ||
			state == State.ATTACK3END )
		{
			
			// 获取玩家和BOSS间的距离
			float distance = Vector3.Distance(
				player.transform.position,
				transform.position );
			
			// 使BOSS朝向玩家
			Vector3 playerRelativePositionByBoss = player.transform.position - transform.position;
			Quaternion playerTargetRotationByBoss = Quaternion.LookRotation( playerRelativePositionByBoss );
			transform.rotation = playerTargetRotationByBoss;
	
			// 获取和玩家间的距离
			if ( distance < distanceToPlayer )
			{
				transform.position -= transform.forward * Time.deltaTime * ( distanceToPlayer - distance ) * 2;
			}
			else
			{
				transform.position += transform.forward * Time.deltaTime * ( distance - distanceToPlayer ) * 2;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 回避
	// ------------------------------------------------------------------------
	private void EscapeFromPlayer()
	{
		// 运动时避开索敌激光
		if (
			state == State.ESCAPE1START ||
			state == State.ESCAPE1END ||
			state == State.ESCAPE2START ||
			state == State.ESCAPE2END ||
			state == State.ESCAPE3START ||
			state == State.ESCAPE3END )
		{
			// BOSS是否正在回避过程中
			if ( !isEscape )
			{
				// 确认和玩家间的距离
				GetDestinationPosition();
			}
			else
			{
				// 获取从BOSS处看到的移动目标的角度
				Vector3 destinationRelativePositionByBoss =
					destinationPosition - transform.position;
				Quaternion destinationTargetRotationByBoss =
					Quaternion.LookRotation( destinationRelativePositionByBoss );

				// 将BOSS的前进方向指向移动目标
				transform.rotation = destinationTargetRotationByBoss;

				// BOSS沿着前进方向前进
				float distanceToDestination = Vector3.Distance(
					destinationPosition,
					transform.position );
				transform.Translate ( new Vector3( 0f, 0f, distanceToDestination * escapeSpeed * Time.deltaTime ) );

				// 求出移动后的位置到移动目标的距离
				distanceToDestination = Vector3.Distance(
					destinationPosition,
					transform.position );
				
				// 到达移动目标时，结束回避行为
				if ( distanceToDestination < 1f )
				{
					isEscape = false;
				}
				
				// 使BOSS朝向玩家的方向
				Vector3 relativePosition = player.transform.position - transform.position;
				Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
				transform.rotation = targetRotation;
			}
		}
	}

	// ------------------------------------------------------------------------
	// 获取回避时的移动目标
	// ------------------------------------------------------------------------
	public void GetDestinationPosition()
	{
		// 玩家的角度
		float playerAngle = player.transform.eulerAngles.y;
		
		// 从玩家位置看到的BOSS角度
		Vector3 bossRelativePositionByPlayer = transform.position - player.transform.position;
		float bossAngleByPlayer = Quaternion.LookRotation( bossRelativePositionByPlayer ).eulerAngles.y;

		// 修正角度
		if ( Mathf.Abs ( playerAngle - bossAngleByPlayer ) > 180f )
		{
			// 如果横跨 0度 <-> 359度，则+360度
			if ( playerAngle < 180f )
			{
				playerAngle += 360f;
			}
			if ( bossAngleByPlayer < 180f )
			{
				bossAngleByPlayer += 360f;
			}
		}
		
		// 如果和玩家的前进方向角度超过一定值则不回避
		if ( Mathf.Abs ( playerAngle - bossAngleByPlayer ) > 45f )
		{
			// ----------------------------------------------------------------
			// 获取和玩家间的距离
			// ----------------------------------------------------------------
			
			// 获取玩家和BOSS间的距离
			float distance = Vector3.Distance(
				player.transform.position,
				transform.position );
			
			// 使BOSS朝向玩家
			Vector3 playerRelativePositionByBoss = player.transform.position - transform.position;
			Quaternion playerTargetRotationByBoss = Quaternion.LookRotation( playerRelativePositionByBoss );
			transform.rotation = playerTargetRotationByBoss;
	
			// 获取和玩家间的距离
			if ( distance < distanceToPlayer )
			{
				transform.position -= transform.forward * Time.deltaTime * ( distanceToPlayer - distance ) * 2;
			}
			else
			{
				transform.position += transform.forward * Time.deltaTime * ( distance - distanceToPlayer ) * 2;
			}

			return;
		}
		
		// ----------------------------------------------------------------
		// 求出回避位置
		// ----------------------------------------------------------------
		
		// 求出以玩家为中心到BOSS的回避位置的角度
		float transformAngle = Random.Range( -45f, 45f );
		if ( transformAngle > 0 )
		{
			transformAngle += 50f;
		}
		else
		{
			transformAngle -= 50f;
		}
		float targetRotationAngle = bossAngleByPlayer;
		targetRotationAngle += transformAngle;
		
		// 设定回避的位置
		destinationPosition =
			Quaternion.AngleAxis( targetRotationAngle, Vector3.up )
				* Vector3.forward * distanceToPlayer;
		
		// 开始回避
		isEscape = true;
		
	}
	
	// ------------------------------------------------------------------------
	// 更新BOSS的状态
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUpdateState( float waitForSeconds, State state )
	{
		// 等待
		yield return new WaitForSeconds( waitForSeconds );
		
		// 更新状态
		this.state = state;
	}
	
	// ------------------------------------------------------------------------
	// 结合life的剩余量减短逃跑的时间
	// ------------------------------------------------------------------------
	private void SetEscapeTime()
	{
		int life = enemyStatusBoss.GetLife();
		if ( life > 5 )
		{
			return;
		}
		switch ( life )
		{
		    case 1:
		        escapeTime = 0.5f;
				waitTimeBeforeAttack = 0.5f;
		        break;
		    case 2:
			case 3: 
		        escapeTime = 1f;
				waitTimeBeforeAttack = 1f;
		        break;
		    case 4:
			case 5: 
		        escapeTime = 2f;
				waitTimeBeforeAttack = 2f;
		        break;
		}
	}
}
using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 管理敌机的状态
// ----------------------------------------------------------------------------
public class EnemyStatus : MonoBehaviour {
	
	public float breakingDistance = 20f;				// 敌机的消灭条件（玩家和敌机的距离）
	public GameObject effectBomb;
	public GameObject effectDamage;
	public int life = 1;								// 生命
	public bool isEnbleByCollisionStone = false;		// 和岩石的碰撞有效（=true）
	public int score = 0;								// 得分
	public string enemyTypeId = "";						// 敌机的（*）-TYPE
	
	private enum State
	{
		INITIALIZE,				// 初始化
		FOLLOWINGLEADER,		// 跟随Leader（如果是BOSS则跟随core）
		ATTACK,					// 攻击
		BREAK,					// 破坏
		DESTROY					// 消灭
	}
	
	private State enemyState = State.INITIALIZE;		// 敌机的状态
	
	protected int lockonBonus = 1;						// 锁定的奖励
	
	private GameObject player;							// 玩家的实例
	private GameObject enemyMaker;						// enemyMaker的实例
	
	private bool isMadeInEnemyMaker = false;			// 通过enemyMaker生成（=true）
	private bool isPlayerBackArea = false;				// 位于玩家的背后（=true）
	private bool isBreakByPlayer = false;				// 被玩家毁坏（=true）
	private bool isBreakByStone = false;				// 被岩石毁坏（=true）
	
	private GameObject txtEnemyStatus;
	private PrintMessage printMessage;					// SubScreen消息区域
	
	private string enemyTypeString = "";				// 敌机的TYPE名
	
	private Vector3 beforePosition;						// 用于爆炸特效：开始移动的位置
	private bool isMoving = false;						// 用于爆炸特效： 是否正在移动？
	private float speed = 0f;							// 用于爆炸特效： 速度
	
	void Start () {

		// 取得玩家的实例
		player = GameObject.FindGameObjectWithTag("Player");
			
		// 获取PrintMessage 的实例

		printMessage = Navi.get().GetPrintMessage();
		
		// 设置敌机的TYPE名
		enemyTypeString = SetEnemyType();
		
		// 追加的初始化处理
		StartSub();
	}
	
	public virtual void StartSub()
	{
		// 派生类的逻辑在这里覆盖
	}
	
	void LateUpdate()
	{
		// 敌机是否在运动（用于爆炸特效）
		if ( enemyState == State.INITIALIZE ||
			 enemyState == State.FOLLOWINGLEADER ||
			 enemyState == State.ATTACK )
		{
			if ( beforePosition != transform.position )
			{
				isMoving = true;
				speed = Vector3.Distance( beforePosition, transform.position );
			}
			else
			{
				isMoving = false;
				speed = 0f;
			}
			beforePosition = transform.position;
		}
	}
	
	void Update ()
	{
		// 判断敌机是否消灭
		IsOverTheDistance();
		
		// 确认敌机的毁坏
		IsBreak();
		
		// 敌机的消灭
		DestroyEnemy();
		
		// 追加的更新处理
		UpdateSub();
	}
	
	public virtual void UpdateSub()
	{
		// 派生类的逻辑在这里覆盖
	}
	
	// ------------------------------------------------------------------------
	// 如果和玩家的距离超过一定值则消除
	// ------------------------------------------------------------------------
	private void IsOverTheDistance()
	{
		if ( enemyState == State.ATTACK )
		{
			float distance = Vector3.Distance(
				player.transform.position,
				transform.position );
			
			if ( distance > breakingDistance )
			{
				enemyState = State.DESTROY;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 敌机的毁坏确认
	// ------------------------------------------------------------------------
	private void IsBreak()
	{
		if ( enemyState == State.BREAK )
		{			
			// 毁坏动画
			if ( effectBomb )
			{
				// 调整角度（如果直接使用rotate的值将导致Particle按意想不到的方向前进）
				Quaternion tmpRotation = Quaternion.Euler(0,transform.rotation.eulerAngles.y / 2,0);
				// 生成爆炸特效
				GameObject tmpGameObject = Instantiate( 
					effectBomb,
					transform.position,
					tmpRotation ) as GameObject;
				// 特效沿着敌机运动的方向移动
				if ( isMoving )
				{
					tmpGameObject.SendMessage( "SetIsMoving", speed );
				}
			}
			
			// 停止敌机
			enemyState = State.DESTROY;
		}
	}
	
	
	protected virtual void DestroyEnemyEx()
	{
		// 销毁敌机
		Destroy( this.gameObject );
	}
	
	// ------------------------------------------------------------------------
	// 销毁敌机
	// ------------------------------------------------------------------------
	private void DestroyEnemy()
	{
		if ( enemyState == State.DESTROY )
		{
			// 如果销毁的对象是编队队长，则取消编队（变为各自单独行动）
			Transform[] children = this.GetComponentsInChildren<Transform>();
      		foreach ( Transform child in children )
			{
    			if ( child.tag == "Enemy" )
				{
					if ( child.GetComponent<EnemyStatus>() )
					{
						// 编队队长之外的其他成员（敌机）的处理
						if ( child.GetComponent<EnemyStatus>().GetIsFollowingLeader() == true )
						{
							// 开始单独行动
							child.SendMessage( "SetIsAttack", true );
							child.transform.parent = null;
						}
					}
				}
			}
			
			// 敌机的生成数量减一
			if ( isMadeInEnemyMaker )
			{
				enemyMaker.SendMessage( "DecreaseEnemyCount", this.GetInstanceID() );
			}
			
			// 追加的销毁时的处理
			DestroyEnemySub();
			
			// 销毁敌机
			Destroy( this.gameObject );
			
		}
	}
	
	public virtual void DestroyEnemySub()
	{
		// 派生类的逻辑在这里覆盖	
	}
	
	// ------------------------------------------------------------------------
	// 设置敌机为消灭状态（毁坏状态将传递给父对象）
	// ------------------------------------------------------------------------
	public void SetIsBreak( bool isBreak )
	{
		
		if ( isBreak )
		{
			if (
				enemyState == State.FOLLOWINGLEADER ||
				enemyState == State.ATTACK )
			{
				if ( life > 0 )
				{
					life--;
				}
				
				if ( life <= 0 )
				{
					// 毁坏敌机
					enemyState = State.BREAK;
					isBreakByPlayer = true;
					
					// 统计得分
					Navi.get().AddScore( score * lockonBonus );

					// 向子窗口输出消息
					printMessage.SetMessage("DESTROYED " + enemyTypeString + " BY LOCK BONUS X " + lockonBonus);
				}
				else
				{
					// 毁坏动画
					if ( effectDamage )
					{
						Instantiate( 
							effectDamage,
							transform.position,
							transform.rotation );
					}
				}
				isBreak = false;
			}
		}
	}
	
	private void SetIsBreakEx( int damage, int lockonBonus )
	{
		if (
			enemyState == State.FOLLOWINGLEADER ||
			enemyState == State.ATTACK )
		{
			if ( life > 0 )
			{
				life -= damage;
			}
			
			if ( life <= 0 )
			{
				// 毁坏敌机
				enemyState = State.BREAK;
				isBreakByPlayer = true;
				
				// 统计得分
				Navi.get().AddScore( score * lockonBonus );
				// 向子窗口输出消息
				printMessage.SetMessage("DESTROYED " + enemyTypeString + " BY LOCK BONUS X " + lockonBonus);
			}
			else
			{
				// 毁坏动画
				if ( effectDamage )
				{
						Instantiate(
							effectDamage,
							transform.position, 
							new Quaternion(0f, 0f, 0f, 0f) );
				}
			}
		}
	}
	
	public void SetLockonBonus( int lockonBonus )
	{
		this.lockonBonus = lockonBonus;
	}
	
	public void SetIsBreakByLaser( int damage )
	{
		SetIsBreakEx( damage, lockonBonus );
	}
	
	public void SetIsBreakByShot( int damage )
	{
		SetIsBreakEx( damage, 1 );
	}
	
	public void SetIsBreakEx2()
	{
		if ( enemyState != State.BREAK )
		{
			life = 0;
			enemyState = State.BREAK;
		}
	}
	
	// ------------------------------------------------------------------------
	// 设置敌机为攻击状态
	// ------------------------------------------------------------------------
	public void SetIsAttack( bool isAttack )
	{
		if ( isAttack )
		{
			if ( enemyState == State.INITIALIZE ||
				 enemyState == State.FOLLOWINGLEADER )
			{
				enemyState = State.ATTACK;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 跟随（编队）队长
	// ------------------------------------------------------------------------
	public void SetIsFollowingLeader( bool isFollowingLeader )
	{
		if ( isFollowingLeader )
		{
			if ( enemyState == State.INITIALIZE )
			{
				enemyState = State.FOLLOWINGLEADER;
			}
		}
	}
	
	public void SetEmenyMaker( GameObject enemyMaker )
	{
		this.enemyMaker = enemyMaker;
		isMadeInEnemyMaker = true;
	}
	public GameObject GetEmenyMaker()
	{
		return enemyMaker;
	}
	
	// ------------------------------------------------------------------------
	// 确认敌机处于攻击状态
	// ------------------------------------------------------------------------
	public bool GetIsAttack()
	{
		if ( enemyState == State.ATTACK )
		{
			return true;
		}
		return false;
	}

	// ------------------------------------------------------------------------
	// 确认敌机处于待机状态
	// ------------------------------------------------------------------------
	public bool GetIsFollowingLeader()
	{
		if ( enemyState == State.FOLLOWINGLEADER )
		{
			return true;
		}
		return false;
	}

	// ------------------------------------------------------------------------
	// 碰撞检测
	// ------------------------------------------------------------------------
	void OnTriggerEnter( Collider collider )
	{
		// 和岩石发生碰撞检测
		if ( isEnbleByCollisionStone )
		{
			if ( collider.tag == "Stone" )
			{
				isBreakByStone = true;
				SetIsBreakEx2();
			}
		}
		
		// 判断是否位于玩家背后
		if ( collider.tag == "PlayerBackArea" )
		{
			isPlayerBackArea = true;
		}

	}
	
	void OnTriggerExit( Collider collider )
	{
		if ( collider.tag == "PlayerBackArea" )
		{
			isPlayerBackArea = false;
		}
	}
	
	public bool GetIsPlayerBackArea()
	{
		return isPlayerBackArea;
	}
	
	public bool GetIsBreakByPlayer()
	{
		return isBreakByPlayer;
	}
	public bool GetIsBreakByStone()
	{
		return isBreakByStone;
	}
	
	private string SetEnemyType()
	{
		if ( enemyTypeId.Length == 0 )
		{
			return "";
		}
		
		string tmpString;
		if ( enemyTypeId.Length == 1  )
		{
			tmpString = enemyTypeId + "-TYPE";
		}
		else
		{
			tmpString = enemyTypeId;
		}
		return tmpString;
	}
	
	public int GetLife()
	{
		return life;
	}
	
}

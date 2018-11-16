using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 控制索敌激光
// ----------------------------------------------------------------------------
public class ScoutingLaser : MonoBehaviour {
	
	public bool isScanOn = false;				// 索敌开始
	
	public GameObject lockonLaser;

	public GameObject lockonSight1;				// 锁定瞄准器1
	public GameObject lockonSight2;				// 锁定瞄准器2
	public GameObject lockonSight3;				// 锁定瞄准器3
	public GameObject lockonSight4;				// 锁定瞄准器4
	public GameObject lockonSight5;				// 锁定瞄准器5
	public GameObject lockonSight6;				// 锁定瞄准器6
	
	public float ScoutingLaserTurnRate = 15f;	// 结合玩家的方向索敌激光旋转的比率
	public float ScoutingLaserFowardPosition = 5.5f;	// 索敌激光的开始位置（玩家看到的前方）
	
	private GameObject[] 	lockonSights;			// 锁定瞄准器1～6
	
	private GameObject		player;					// 玩家
	private GameObject		scoutingLaserMesh;		// 索敌激光的碰撞补间
	private MeshRenderer	scoutingLaserLine;		// 玩家前方方向显示的索敌激光
	private LockBonus		lockBonus;				// LockBonus.
	private LockSlot		lockSlot;				// LockSlot.
	private PrintMessage printMessage;				// SubScreen的消息区域
	
	private int lockonCount = 0;				// 锁定数
	
	private static int MAX_LOCKON_COUNT = 6;	// 锁定最大数
	
	private GameObject[] lockedOnEnemys;		// 锁定的敌机
	private int[] lockedOnEnemyIds;				// 锁定的敌机的实例ID
	private int[] lockonLaserIds;				// 锁定激光的实例ID
	private GameObject[] lockedOnSights;		// 锁定敌机的锁定瞄准器
	
	private float[] lockonLaserStartRotation =	// 锁定激光射出时的角度
	{
		-40f, 40f, -70f, 70f, -100f, 100f
	};
	
	private int	invalidInstanceId = -1;		// 表示lockedOnEnemyIds[] 未被使用的值
	
	void Start () 
	{
		// 锁定敌机
		lockedOnEnemys = new GameObject[MAX_LOCKON_COUNT];
		lockedOnEnemyIds = new int[MAX_LOCKON_COUNT];
		lockonLaserIds = new int[MAX_LOCKON_COUNT];
		lockedOnSights = new GameObject[MAX_LOCKON_COUNT];
		
		// 获取锁定瞄准器的实例
		lockonSights = new GameObject[MAX_LOCKON_COUNT];
		lockonSights[0] = lockonSight1;
		lockonSights[1] = lockonSight2;
		lockonSights[2] = lockonSight3;
		lockonSights[3] = lockonSight4;
		lockonSights[4] = lockonSight5;
		lockonSights[5] = lockonSight6;
		
		// 获取player的实例
		player = GameObject.FindGameObjectWithTag("Player");
		
		// 获取索敌激光的碰撞补间
		scoutingLaserMesh = GameObject.FindGameObjectWithTag("ScoutingLaserMesh");

		// 获取索敌激光前方线的实例
		scoutingLaserLine = GameObject.FindGameObjectWithTag("ScoutingLaserLine").GetComponent<MeshRenderer>();
		
		// 锁定瞄准器的初始化
		this.GetComponent<TrailRenderer>().enabled = isScanOn;
		scoutingLaserLine.enabled = isScanOn;
		
		// 获取LockBonus的实例
		lockBonus = Navi.get().GetLockBonus();

		// 获取LockSlot的实例
		lockSlot = Navi.get().GetLockSlot();
			
		// 获取PrintMessage 的实例
		printMessage = Navi.get().GetPrintMessage();

		// 表示lockedOnEnemyIds[] 未被使用时的值
		// 自身不会被锁定
		invalidInstanceId = this.gameObject.GetInstanceID();

		for(int i = 0;i < lockedOnEnemyIds.Length;i++) {

			lockedOnEnemyIds[i] = invalidInstanceId;
			lockonLaserIds[i] = invalidInstanceId;
		}
		
	}
	
	void Update ()
	{
	
		// 索敌激光停止时，向锁定的敌机发射锁定激光
		if ( isScanOn == false ) 
		{
			StartLockonLaser();
		}
		
		// 设定索敌激光的位置
		UpdateTransformMesh();
	}

	// ------------------------------------------------------------------------
	// 切换索敌激光的有效无效设置
	//  - 此版本索敌激光开始时补需要等待
	// ------------------------------------------------------------------------	
	public void SetIsScanOn( bool isScanOn )
	{		
		// 索敌激光的（有效／无效）切换
		this.isScanOn = isScanOn;
		
		// 索敌激光的（显示／隐藏）切换
		this.GetComponent<TrailRenderer>().enabled = isScanOn;
		scoutingLaserLine.enabled = isScanOn;
		
		// 显示消息
		if ( isScanOn == true )
		{
			StartCoroutine( "SetSearchingMessage" );
		}
		else
		{
			StopCoroutine( "SetSearchingMessage" );
		}
		
		// 索敌激光的音效
		if ( isScanOn == true )
		{
			this.GetComponent<AudioSource>().Play();
		} 
		else
		{
			this.GetComponent<AudioSource>().Stop();
		}
	}
	IEnumerator SetSearchingMessage()
	{
		yield return new WaitForSeconds( 0.5f );
		printMessage.SetMessage("SEARCHING ENEMY...");
	}
	
	// ------------------------------------------------------------------------
	// 索敌激光的位置设定 MeshCollider 
	//  - 总是显示在玩家的前方
	//  - 调整索敌激光的速度（变慢）
	//    - 为了回避改变玩家方向时速度太快将出现的下列问题
	//      - 存在无法到达的位置
	//      - TrailRenderer无法形成一个漂亮的圆
    //  - 为TrailRenderer做调整
	//    - TrailRenderer在Position发生改变前后之间绘制，结合旋转的方向改变位置
	// ------------------------------------------------------------------------
	private void UpdateTransformMesh()
	{
		
		// 获取以指定的速度从现在的方向朝玩家方向倾斜的角度
		float	targetRotationAngle  = player.transform.eulerAngles.y;
		float	currentRotationAngle = transform.eulerAngles.y;

		currentRotationAngle = Mathf.LerpAngle(
			currentRotationAngle,
			targetRotationAngle,
			ScoutingLaserTurnRate * Time.deltaTime );

		Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
		
		// 生成用于碰撞的Mesh
		if ( isScanOn )
		{
			float[] tmpAngle = new float[]{ player.transform.eulerAngles.y, transform.eulerAngles.y };
			scoutingLaserMesh.SendMessage("makeFanShape", tmpAngle);
		}
		else
		{
			scoutingLaserMesh.SendMessage("clearShape");
		}
		
		// 设定角度
		transform.rotation = tiltedRotation;
		
		// 修改位置
		transform.position = new Vector3(
			ScoutingLaserFowardPosition * Mathf.Sin( Mathf.Deg2Rad * currentRotationAngle ),
			0,
			ScoutingLaserFowardPosition * Mathf.Cos( Mathf.Deg2Rad * currentRotationAngle )
		);

	}
	
	// ------------------------------------------------------------------------
	// 锁定处理
	// ------------------------------------------------------------------------
	public void Lockon( Collider collider )
	{
		// 锁定敌机
		if ( collider.gameObject.tag == "Enemy" ) {

			// 如果未锁定则进行锁定
			int targetId = collider.gameObject.GetInstanceID();

			// 累加锁定数量
			// （如果锁定成功将变为true）
			bool isLockon = IncreaseLockonCount( targetId );

			if ( isLockon ) {
				
				// ------------------------------------------------------------
				// 锁定
				// ------------------------------------------------------------
				
				// 决定锁定编号
				int lockonNumber = getLockonNumber();
							
				if ( lockonNumber >= 0 ) {
					
					// 锁定瞄准器的显示位置用于显示被锁定敌机的位置
					Vector3 targetPosition = collider.gameObject.transform.position;
					Quaternion tagetRotation = new Quaternion( 0f, 180f, 0f, 0f );
					
					// 生成锁定瞄准器的实例
					GameObject lockonSight;
					lockonSight = Instantiate( lockonSights[lockonNumber], targetPosition, tagetRotation ) as GameObject;
					lockonSight.SendMessage( "SetLockonEnemy", collider.gameObject );
					
					// 将被锁定的敌机添加到锁定列表
					lockedOnEnemyIds[lockonNumber] = targetId;
					
					// 存储被锁定的对象
					lockedOnEnemys[lockonNumber] = collider.gameObject;
					
					// 存储锁定瞄准器
					lockedOnSights[lockonNumber] = lockonSight;
					
					// 显示消息
					printMessage.SetMessage("LOCKED ON SOME ENEMIES.");
					lockSlot.SetLockCount(lockonCount);
				}
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 更新锁定的数量
	//  - true:累加成功，false:累加失败
	// ------------------------------------------------------------------------
	public bool IncreaseLockonCount( int enemyId )
	{
		// 是否小于最大锁定数
		if ( lockonCount < MAX_LOCKON_COUNT )
		{
			// 是否是未被锁定的敌机
			if ( !IsLockon( enemyId ) )
			{
				// 锁定数量+1.
				lockonCount++;
				return true;
			}
		}
		// 已经锁定
		return false;
	}
	
	// ------------------------------------------------------------------------
	// 返回锁定的空闲领域的索引
	//  - 还有空余: 0以上
	//  - 没有空余: -1
	// ------------------------------------------------------------------------
	private int getLockonNumber()
	{
		// 检测用于管理锁定激光的数组中的空位，并返回该位置的索引
		for( int i = 0; i < lockedOnEnemyIds.Length; i++ )
		{
			if ( lockedOnEnemyIds[i] == invalidInstanceId )
			{
				return i;
			}
		}
		return -1;
	}
	
	// ------------------------------------------------------------------------
	// 减少锁定数量
	// ------------------------------------------------------------------------
	public void DecreaseLockonCount( int instanceId )
	{
		// 减少锁定数量
		if ( lockonCount > 0 ) {
			lockonCount--;
			lockSlot.SetLockCount(lockonCount);		
		}
		
		// 消除已经锁定的信息
		for( int i = 0; i < lockedOnEnemyIds.Length; i++ )
		{
			if ( lockedOnEnemyIds[i] == instanceId )
			{
				if ( lockedOnSights[i] )
				{
					lockedOnSights[i].SendMessage( "Destroy" );
				}
				lockedOnEnemyIds[i] = invalidInstanceId;
				lockedOnEnemys[i] = null;
				lockonLaserIds[i] = invalidInstanceId;
				lockedOnSights[i] = null;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 确认锁定的状况
	// ------------------------------------------------------------------------
	public bool IsLockon( int enemyId )
	{
		// 是否登录到已锁定敌机列表
		int existIndex = System.Array.IndexOf( lockedOnEnemyIds, enemyId );

		if ( existIndex == -1 )
		{
			// 未锁定
			return false;
		}
		// 已锁定
		return true;
	}
	
	// ------------------------------------------------------------------------
	// 发射锁定激光
	// ------------------------------------------------------------------------
	private void StartLockonLaser()
	{
		int countLockon = 0;
		
		// 统计锁定数量
		for( int i = 0; i < MAX_LOCKON_COUNT; i++ )
		{
			if ( lockedOnEnemyIds[i] != invalidInstanceId )
			{
				if ( lockonLaserIds[i] == invalidInstanceId )
				{
					// 锁定奖励
					countLockon++;
					lockBonus.SetLockCount(countLockon);
				}
			}
		}

		for( int i = 0; i < MAX_LOCKON_COUNT; i++ )
		{
			if ( lockedOnEnemyIds[i] != invalidInstanceId )
			{
				// 只在锁定激光未生成时执行
				if ( lockonLaserIds[i] == invalidInstanceId )
				{
				
					// ------------------------------------------------------------
					// 生成锁定激光
					// ------------------------------------------------------------
					
					// 获取玩家的坐标
					Vector3 playerPos = player.transform.position;
					Quaternion playerRot = player.transform.rotation;
					
					// 决定激光的射出角度
					Quaternion startRotation = player.transform.rotation;
					float laserRotationAngle = startRotation.eulerAngles.y;

					laserRotationAngle += lockonLaserStartRotation[i];
					Quaternion tiltedRotation = Quaternion.Euler( 0, laserRotationAngle, 0 );
					playerRot = tiltedRotation;
					
					// 生成锁定激光
					GameObject tmpLockonLaser;
					tmpLockonLaser = Instantiate( lockonLaser, playerPos, playerRot ) as GameObject;
					tmpLockonLaser.SendMessage( "SetLockonBonus", Mathf.Pow (2, countLockon ) );
					tmpLockonLaser.SendMessage( "SetTargetEnemy", lockedOnEnemys[i] );
					lockonLaserIds[i] = tmpLockonLaser.GetInstanceID();
				}
			}
		}
		
		// 消除消息
		if ( countLockon == 0 )
		{
		}

	}
	
	public void Reset()
	{
		// 使索敌激光无效
		this.isScanOn = false;
		
		// 隐藏索敌激光
		this.GetComponent<TrailRenderer>().enabled = false;
		scoutingLaserLine.enabled = false;
		
		// 停止索敌激光音效
		if ( this.GetComponent<AudioSource>().isPlaying )
		{
			this.GetComponent<AudioSource>().Stop();
		} 
		
		// 初始化锁定相关的信息
		for( int i = 0; i < MAX_LOCKON_COUNT; i++ )
		{
			lockedOnEnemys[i] = null;
			lockedOnEnemyIds[i] = invalidInstanceId;
			lockonLaserIds[i] = invalidInstanceId;
			if ( lockedOnSights[i] )
			{
				lockedOnSights[i].SendMessage("Destroy");
			}
			lockedOnSights[i] = null;
		}
		lockonCount = 0;
		
		// 初始化锁定槽的显示
		lockSlot.SetLockCount( lockonCount );
		
		// 初始化锁定奖励的显示
		lockBonus.SetLockCount( lockonCount );
		
	}

}
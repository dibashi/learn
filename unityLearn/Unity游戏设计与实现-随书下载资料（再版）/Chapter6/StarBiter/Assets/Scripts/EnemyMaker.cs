using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyMaker
//  - 按指定的时间间隔生成指定的敌机
//  - 指定场景中敌机的生成上限
// ----------------------------------------------------------------------------
public class EnemyMaker : MonoBehaviour {
	
	public float creationInterval = 5.0f;		// 生成敌机的间隔
	public GameObject enemyGameObject;			// 敌机游戏对象
	
	public int maxEnemysInScene = 6;			// 场景内的生成上限
	public int maxEnemys = 1;					// 生成总数
	
	public bool canShoot = false;				// 能够发射炮弹
	public bool addToSpeed = false;				// 提高速度
	
	public bool isBoss = false;					// 是否是最终的BOSS
	
	private bool isMaking = false;				// 正在生成敌机
	private int enemyCount = 0;					// 当前生成的敌机数量
	private GameObject[] enemyGameObjects;		// 生成的敌机实例
	private int[] enemyIds;						// 生成的敌机实例ID
	
	private PlayerStatus playerStatus;			// 玩家状态的实例
	
	private int destroyedEnemyCount = 0;		// 被击毁的敌机数量
	
	private GameObject stageController;			// 关卡控制器的实例
	
	private int stageIndex = 0;					// 关卡的进度
	
	void Start () {
		
		// 获取关卡控制器的实例
		stageController = GameObject.FindGameObjectWithTag("StageController");
		
		// 获取玩家状态的实例
		playerStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStatus>();

		// 用于记录生成敌机信息的区域
		enemyGameObjects = new GameObject[maxEnemysInScene];
		enemyIds = new int[maxEnemysInScene];

	}
	
	void Update () {
	
		// 只有在允许玩家操作时才执行处理
		if ( playerStatus.GetIsNOWPLAYING() )
		{
		
			// 敌机的生成上限是否都已全部被销毁？
			if ( destroyedEnemyCount == maxEnemys )
			{
				// 关卡结束时向StageController传递消息
				stageController.SendMessage( "SetStateEnd", stageIndex );
				
				// 停止EnemyMaker
				SetMakingStop();
			}
		
			// 还可以生成敌机吗？
			if ( enemyCount < maxEnemysInScene ) {
				
				// 是否正在生成？
				if ( !isMaking ) {
					
					// 正在生成
					isMaking = true;
					
					// 经过指定的时间（CreationInterval）后生成敌机
					StartCoroutine( CreateEnemy() );
				}
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 经过指定的时间（CreationInterval）后生成敌机
	// ------------------------------------------------------------------------
	IEnumerator CreateEnemy()
	{
		// 累加生成数量
		enemyCount++;
		
		// 等待一定间隔
		yield return new WaitForSeconds( creationInterval );
		
		// 生成敌机
		GameObject tmpEnemy = Instantiate(
			enemyGameObject,
			Vector3.zero,
			new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		tmpEnemy.SendMessage("SetEmenyMaker", this.gameObject, SendMessageOptions.DontRequireReceiver );
		
		// 发射设置
		if ( canShoot )
		{
			tmpEnemy.SendMessage( "SetCanShoot", true, SendMessageOptions.DontRequireReceiver );
			// 如果有子对象则全部发送
			Transform[] children = tmpEnemy.GetComponentsInChildren<Transform>();
      		foreach ( Transform child in children )
			{
    			if ( child.tag == "Enemy" )
				{
					child.SendMessage( "SetCanShoot", true, SendMessageOptions.DontRequireReceiver );
				}
			}
		}
		
		// 设定速度
		if ( addToSpeed && tmpEnemy.GetComponent<EnemyType02Controller>() )
		{
			tmpEnemy.SendMessage( "SpeedUp", null, SendMessageOptions.DontRequireReceiver );
		}
		
		// 记录生成的gameObject
	    for ( int i = 0; i < enemyGameObjects.Length; i++)
		{
			if ( enemyGameObjects[i] == null )
			{
				enemyGameObjects[i] = tmpEnemy;
				enemyIds[i] = tmpEnemy.GetInstanceID();
				break;
			}
	    }
		
		// 结束生成的请求
		isMaking = false;

	}
	
	// ------------------------------------------------------------------------
	// 对生成的敌机数量减一
	// ------------------------------------------------------------------------
	public void DecreaseEnemyCount( int instanceId )
	{
		// 减少敌机数量
		if ( enemyCount > 0 ) {
			enemyCount--;
		}
		
		// 消除已经生成的敌机信息
		for( int i = 0; i < enemyIds.Length; i++ )
		{
			if ( enemyIds[i] == instanceId )
			{
				enemyIds[i] = 0;
				enemyGameObjects[i] = null;
			}
		}
		
		// 增加销毁的敌机数量
		destroyedEnemyCount++;
	}
	
	// ------------------------------------------------------------------------
	// 销毁生成的所有敌机
	// ------------------------------------------------------------------------
	public void DestroyEnemys()
	{
		// 如果是最终的BOSS则什么都不做
		if ( isBoss ) { return; }
		
		// 销毁生成的所有敌机
	    for ( int i = 0; i < enemyGameObjects.Length; i++)
		{
			if ( enemyGameObjects[i] != null )
			{
				Destroy( enemyGameObjects[i] );
				enemyGameObjects[i] = null;
				enemyIds[i] = 0;
			}
	    }
		
		// 重置敌机的生成数量
		enemyCount = 0;
	}
	
	// ------------------------------------------------------------------------
	// 设置允许发射炮弹
	// ------------------------------------------------------------------------
	public void SetCanShoot( bool canShoot )
	{
		this.canShoot = canShoot;
	}
	
	// ------------------------------------------------------------------------
	// 提高速度
	// ------------------------------------------------------------------------
	public void SetAddToSpeed( bool addToSpeed )
	{
		this.addToSpeed = addToSpeed;
	}
	
	// ------------------------------------------------------------------------
	// 停止生成敌机
	// ------------------------------------------------------------------------
	private void SetMakingStop()
	{
		maxEnemysInScene = 0;
		destroyedEnemyCount = 0;
	}
	
	// ------------------------------------------------------------------------
	// 设定敌机的生成间隔
	// ------------------------------------------------------------------------
	public void SetCreateInterval( float creationInterval )
	{
		this.creationInterval = creationInterval;
	}
	
	// ------------------------------------------------------------------------
	// 设定场景内的生成上限
	// ------------------------------------------------------------------------
	public void SetMaxEnemysInScene( int maxEnemysInScene )
	{
		this.maxEnemysInScene = maxEnemysInScene;
	}
	
	// ------------------------------------------------------------------------
	// 记录关卡的进度状况
	// ------------------------------------------------------------------------
	public void SetStage( int stageIndex )
	{
		this.stageIndex = stageIndex;
	}

}

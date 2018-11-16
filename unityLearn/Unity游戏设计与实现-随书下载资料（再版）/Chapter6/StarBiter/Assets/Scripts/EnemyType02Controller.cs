using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType02Controller
//  - 控制“M03 敌机模型类型02”的运动（单独or编队的队长）
//  - 使用方法
//    - 将这个脚本添加到对象上
//  - 运动特点
//    - 在玩家前进方向的左上或者右下出现
//    - 直线前进
//    - 编队的情况
//      - 当队长被消灭时将其他队员状态设置为ATTACK
// ----------------------------------------------------------------------------
public class EnemyType02Controller : MonoBehaviour {

	public float speed = 4f;							// 敌机的速度
	public float secondSpeed = 6f;						// 敌机的快速速度
	
	private bool canShoot = false;						// 发射炮弹的条件（true：可以发射）
	
	private GameObject player;							// 玩家
	private BattleSpaceController battleSpaceContoller;	// 战斗空间
	private EnemyStatus enemyStatus;					// 敌机的状态
	
	private float distanceFromPlayerAtStart = 10.5f;		// 开始时和玩家之间的距离
	
	void Start () {
	
		// 获取玩家的实例
		player = GameObject.FindGameObjectWithTag("Player");

		// 获取战斗空间的实例
		battleSpaceContoller = GameObject.FindGameObjectWithTag("BattleSpace").GetComponent<BattleSpaceController>();
		
		// 获取敌机状态的实例
		enemyStatus = this.GetComponent<EnemyStatus>();
		
		// --------------------------------------------------------------------
		// 指定出现的位置
		// --------------------------------------------------------------------
		
		// 计算产生的方向（从玩家角度开始左右偏移45度）
		float playerAngleY = player.transform.rotation.eulerAngles.y;
		float additionalAngle = (float)Random.Range( -45, 45 );
		
		// 设定方向
		transform.rotation = Quaternion.Euler( 0f, playerAngleY + additionalAngle, 0f );
		
		// 设定位置
		transform.position = new Vector3( 0, 0, 0 );
		transform.position = transform.forward * distanceFromPlayerAtStart;
		
		// 使前进方向指向玩家
		Vector3 playerPosition = player.transform.position;
		Vector3 relativePosition = playerPosition - transform.position;
		Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
		transform.rotation = targetRotation;
		
		// 使敌机开始运动
		enemyStatus.SetIsAttack( true );
	}
	
	void Update () {
	
		if ( enemyStatus.GetIsAttack() )
		{
			// 发射炮弹
			Shoot();
			
			// 使敌机前进
			transform.Translate ( new Vector3( 0f, 0f, speed * Time.deltaTime ) );
			
			// 叠加战斗空间的滚动方向
			transform.position -= battleSpaceContoller.GetAdditionPos();
		}
	}
	
	// ------------------------------------------------------------------------
	// 发射炮弹
	// ------------------------------------------------------------------------
	private void Shoot()
	{
		if ( !canShoot ) { return; }
		
		//
		bool isFiring = false;
		if ( this.GetComponent<ShotMaker>() )
		{
			this.GetComponent<ShotMaker>().GetIsFiring();
		}
		
		//
		if ( !isFiring )
		{
			if ( enemyStatus.GetIsPlayerBackArea() )
			{
				if ( this.GetComponent<ShotMaker>() )
				{
					this.GetComponent<ShotMaker>().SetIsFiring();
				}
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 加速
	// ------------------------------------------------------------------------
	public void SpeedUp()
	{
		speed = secondSpeed;
	}
	
	// ------------------------------------------------------------------------
	// 允许发射炮弹
	// ------------------------------------------------------------------------
	public void SetCanShoot( bool canShoot )
	{
		this.canShoot = canShoot;
	}

}

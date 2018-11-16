using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType01Controller
//  - 控制“M02 敌机模型类型 01”的运动（单独or编队的队长）
//  - 使用方法
//    - 将这个脚本添加到对象上
//  - 运动特点
//    - 在玩家前进方向的左上或者右下出现
//    - 朝着玩家中心前进
//    - 编队的情况
//      - 当队长被消灭时将其他队员状态设置为ATTACK
// ----------------------------------------------------------------------------
public class EnemyType01Controller : MonoBehaviour {
	
	public float speed = 2.7f;							// 敌机前进的速度
	public float turnSpeed = 1f;						// 旋转速度
	
	public float startDistanceToShoot = 5f;				// 炮弹射击范围的开始距离
	public float endDistanceToShoot = 8f;				// 炮弹射击范围的结束距离
	
	private bool canShoot = false;						// 发射炮弹的条件（true：可以发射）
	
	private GameObject player;							// 玩家
	private BattleSpaceController battleSpaceContoller;	// 战斗空间
	private EnemyStatus enemyStatus;					// 敌机的状态
	
	private float distanceFromPlayerAtStart = 9.5f;		// 开始时和玩家之间的距离
	
	void Start () {
	
		// 取得玩家的实例
		player = GameObject.FindGameObjectWithTag("Player");

		// 取得战斗空间的实例
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
			// 获取玩家的方向
			Vector3 playerPosition = player.transform.position;
			Vector3 relativePosition = playerPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
				
			// 获取按指定速度从敌机现在的方向朝玩家的方向倾斜后的角度
			float targetRotationAngle;
			targetRotationAngle = targetRotation.eulerAngles.y;
			float currentRotationAngle = transform.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnSpeed * Time.deltaTime );
			Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
			
			// 改变敌机的角度
			transform.rotation = tiltedRotation;
			
			// 使敌机前进
			transform.Translate ( new Vector3( 0f, 0f, speed * Time.deltaTime ) );
			
			// 叠加战斗空间的滚动方向
			transform.position -= battleSpaceContoller.GetAdditionPos();
			
			// 确认炮弹的发射
			if ( canShoot )
			{
				IsFireDistance();
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 攻击对象的距离
	// ------------------------------------------------------------------------
	private void IsFireDistance()
	{
		bool isFiring = false;
		if ( this.GetComponent<ShotMaker>() )
		{
			isFiring = this.GetComponent<ShotMaker>().GetIsFiring();
			if ( !isFiring )
			{
				if ( IsInRange( startDistanceToShoot, endDistanceToShoot ) )
				{
					this.GetComponent<ShotMaker>().SetIsFiring();
				}
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 是否位于范围内
	// ------------------------------------------------------------------------
	private bool IsInRange( float fromDistance, float toDisRance )
	{
		float distance = Vector3.Distance(
			player.transform.position,
			transform.position );
		
		if ( distance >= fromDistance && distance <= toDisRance )
		{
			return true;
		}
		return false;
	}
	
	// ------------------------------------------------------------------------
	// 允许发射炮弹
	// ------------------------------------------------------------------------
	public void SetCanShoot( bool canShoot )
	{
		this.canShoot = canShoot;
	}
}

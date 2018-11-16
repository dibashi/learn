using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType01ChildController
//  - 控制“M02 敌机模型类型01”的运动（除了编队队长之外的成员）
//  - 使用方法
//    - 将添加了这个脚本的对象设置为队长的子对象
//  - 运动特点
//    - 跟随队长
//    - 队长被毁坏时，则采取回避行动
// ----------------------------------------------------------------------------
public class EnemyType01ChildController : MonoBehaviour {
	
	public float speed = 2.7f;							// 敌机前进的速度
	public float turnSpeed = 1f;						// 旋转的速度
	public float escapeSpeed = 3f;						// 回避时的前进速度
	
	public float startDistanceToShoot = 5f;				// 发射炮弹范围的开始距离
	public float endDistanceToShoot = 10f;				// 发射炮弹范围的结束距离

	private bool canShoot = false;						// 炮弹发射条件（true：可以发射）
	
	private GameObject player;							// 玩家
	private BattleSpaceController battleSpaceContoller;	// 战斗空间
	private EnemyStatus enemyStatus;					// 敌机状态

	void Start () {
	
		// 取得玩家的实例
		player = GameObject.FindGameObjectWithTag("Player");

		// 取得战斗空间的实例
		battleSpaceContoller = GameObject.FindGameObjectWithTag("BattleSpace").GetComponent<BattleSpaceController>();
		
		// 取得敌机状态的实例
		enemyStatus = this.GetComponent<EnemyStatus>();
		
		// 跟随队长
		enemyStatus.SetIsFollowingLeader( true );
	}

	void Update () {
	
		if ( enemyStatus.GetIsAttack() )
		{
			// ----------------------------------------------------------------
			// 回避
			// ----------------------------------------------------------------
			
			// 获得玩家的方向
			Vector3 playerPosition = player.transform.position;
			Vector3 relativePosition = playerPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
				
			// 获取按指定速度从敌机现在的方向朝玩家的反方向倾斜后的角度
			float targetRotationAngle = targetRotation.eulerAngles.y - 180;
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

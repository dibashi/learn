using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType02ChildController
//  - 控制“M03 敌机模型 类型02”的运动（除了编队队长外的成员）
//  - 使用方法
//    - 将添加了这个脚本的对象配置为队长的子对象
//  - 运动规则
//    - 跟随队长
//    - 当队长被销毁时，采取回避行动
// ----------------------------------------------------------------------------
public class EnemyType02ChildController : MonoBehaviour {

	public float speed = 4f;							// 敌机的速度
	public float turnSpeed = 1f;						// 敌机变换方向的速度
	public float secondSpeed = 6f;						// 敌机的快速速度
	
	private GameObject player;							// 玩家
	private BattleSpaceController battleSpaceContoller;	// 战斗空间
	private EnemyStatus enemyStatus;					// 敌机的状态

	void Start () {
	
		// 获取玩家的实例
		player = GameObject.FindGameObjectWithTag("Player");

		// 获取战斗空间的实例
		battleSpaceContoller = GameObject.FindGameObjectWithTag("BattleSpace").GetComponent<BattleSpaceController>();
		
		// 获取敌机状态的实例
		enemyStatus = this.GetComponent<EnemyStatus>();
		
		// 跟随队长
		enemyStatus.SetIsFollowingLeader( true );
	}
	
	void Update () {
	
		// 如果队长被消灭，状态变为ATTACK
		if ( enemyStatus.GetIsAttack() )
		{
			// ----------------------------------------------------------------
			// 回避
			// ----------------------------------------------------------------
			
			// 获取玩家的方向
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
			
		}
	}
	
	// ------------------------------------------------------------------------
	// 加速
	// ------------------------------------------------------------------------
	public void SpeedUp()
	{
		speed = secondSpeed;
	}
}

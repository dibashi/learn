using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType03ChildController
//  - 控制“M04 敌机模型 类型03”的运动（除了编队队长外的成员）
//  - 使用方法
//    - 将添加了这个脚本的对象配置为队长的子对象
//  - 运动规则
//    - 跟随队长
//    - 当队长被击毁时，采取回避行动
// ----------------------------------------------------------------------------
public class EnemyType03ChildController : MonoBehaviour {

	public float speed = 6f;							// 敌机的速度
	public float speedUTurn = 6f;						// U转弯时的速度
	public float turnSpeed = 5f;						// 敌机变换方向的速度
	
	private bool canShoot = false;						// 发射炮弹的条件（true：可以发射）
	
	private GameObject player;							// 玩家
	private EnemyStatus enemyStatus;					// 敌机的状态
	
	private float distanceToUTurnPoint = 5.0f;			// U转弯时和玩家之间的距离
	private enum State
	{
		FORWARD,	// 前进
		STAY,		// 停止
		UTURN		// U转弯
	}
	private State state = State.FORWARD;				// 敌机的行动情况
	
	void Start () {
	
		// 获取玩家的实例
		player = GameObject.FindGameObjectWithTag("Player");
		
		// 获取敌机状态的实例
		enemyStatus = this.GetComponent<EnemyStatus>();
		
		// ----------------------------------------------------------------
		// 决定敌机的前进方向
		// ----------------------------------------------------------------
		
		// 获取玩家的方向
		Vector3 playerPosition = player.transform.position;
		Vector3 relativePosition = playerPosition - transform.position;
		Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
		
		// 改变敌机的角度
		transform.rotation = targetRotation;

		// 跟随队长
		enemyStatus.SetIsFollowingLeader( true );

	}
	
	void Update () {
	
		if ( enemyStatus.GetIsFollowingLeader() )
		{
			if ( state == State.UTURN )
			{
				// ----------------------------------------------------------------
				// 决定敌机的前进方向
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
				transform.Translate ( new Vector3( 0f, 0f, speedUTurn * Time.deltaTime ) );
			}
			
			if ( state == State.FORWARD )
			{
				// 使敌机前进
				transform.Translate ( new Vector3( 0f, 0f, speed * Time.deltaTime ) );
				
				// 接近到一定距离时停留一定的时间
				float distance = Vector3.Distance(
					player.transform.position,
					transform.position );
				
				if ( distance < distanceToUTurnPoint )
				{
					state = State.STAY;
					StartCoroutine( WaitAndUTurn( 3f ) );
				}
			}
			
			if ( state == State.STAY )
			{
				// 停止过程中由于rigidbody的关系使碰撞检测有效，运动返回
				// ※采用比Project Settings->Physics 的 Sleep Velocity 更大的值
				transform.Translate ( new Vector3( 0f, 0f, 0.2f ) );
				transform.Translate ( new Vector3( 0f, 0f, -0.2f ) );
			}
		}
		
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
		}
	}
	
	// ------------------------------------------------------------------------
	// 等待U转弯
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUTurn( float waitForSeconds )
	{
		yield return new WaitForSeconds( waitForSeconds );
		state = State.UTURN;
		SetFire();
	}
	
	// ------------------------------------------------------------------------
	// 发射炮弹
	// ------------------------------------------------------------------------
	private void SetFire()
	{
		if ( !canShoot ) return;
		
		bool isFiring = false;
		if ( this.GetComponent<ShotMaker>() )
		{
			this.GetComponent<ShotMaker>().GetIsFiring();
		}
		if ( !isFiring )
		{
			if ( this.GetComponent<ShotMaker>() )
			{
				this.GetComponent<ShotMaker>().SetIsFiring();
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 允许发射炮弹
	// ------------------------------------------------------------------------
	public void SetCanShoot( bool canShoot )
	{
		this.canShoot = canShoot;
	}
}

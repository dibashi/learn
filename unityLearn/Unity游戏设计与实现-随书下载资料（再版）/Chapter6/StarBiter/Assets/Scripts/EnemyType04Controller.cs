using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType04Controller
//  - 控制“敌机模型 类型04”的运动
//  - 使用方法
//    - 通过EnemyMaker每隔一定时间就会生成添加了本控制器的对象
//  - 运动特点（暂定）
//    - 在玩家前进方向的左上或者右下出现
//    - 围绕着玩家
//    - 保持和玩家相同的速度一定时间
//    - 向画面外运动
// ----------------------------------------------------------------------------
public class EnemyType04Controller : MonoBehaviour {

	public float speed = 10f;							// 敌机的速度
	public float turnSpeed = 1.6f;						// 敌机变换方向的速度
	public float followingSpeed = 10f;					// 跟踪速度
	public float uTurnSpeed = 20f;						// 返回速度
	public float distanceFromPlayer = 10f;				// 和玩家保持的一定距离
	public float followingTime = 5f;					// 跟踪时间
	
	private GameObject player;							// 玩家
	private float distanceToPlayer = 10.0f;				// 出现位置到玩家的距离
	private bool isUTurn = false;
	
	private GameObject subScreenMessage;				// SubScreen的消息区域
	
	void Start () {
	
		// 获得玩家
		player = GameObject.FindGameObjectWithTag("Player");
		
		// 获取SubScreenMessage的实例
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");
		
		// --------------------------------------------------------------------
		// 指定出现的位置
		// --------------------------------------------------------------------
		
		// 首先设置为玩家的位置
		transform.position = player.transform.position;
		float tmpAngle = Random.Range( 0f, 360f );
		transform.rotation = Quaternion.Euler( 0, tmpAngle, 0 );
		
		// 调整位置
		transform.Translate ( new Vector3( 0f, 0f, distanceToPlayer ) );
		
		// ----------------------------------------------------------------
		// 决定敌机前进的方向
		// ----------------------------------------------------------------
		
		// 玩家存在时的处理
		if ( player )
		{
			// 获取玩家的方向
			Vector3 playerPosition = player.transform.position;
			Vector3 relativePosition = playerPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
			Quaternion tiltedRotation = Quaternion.Euler( 0, targetRotation.eulerAngles.y + 30f, 0 );
			
			// 改变敌机的角度
			transform.rotation = tiltedRotation;
		}
		
		// 使敌机开始运动
		this.GetComponent<EnemyStatus>().SetIsAttack( true );
		
	}
	
	void Update () {
	
		bool isAttack = this.GetComponent<EnemyStatus>().GetIsAttack();
		if ( isAttack )
		{
			if ( !isUTurn )
			{
				// 使敌机前进
				Forward();
			
				// 是否靠近玩家
				IsNear();
			}
			else
			{
				// 逃跑
				Backward();
			}	
		}
	}
	
	// ------------------------------------------------------------------------
	// 使敌机前进
	// ------------------------------------------------------------------------
	private void Forward()
	{
		// 玩家存在时的处理
		if ( player )
		{
			// 获取玩家的方向
			Vector3 playerPosition = player.transform.position;
			Vector3 relativePosition = playerPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
			
			// 获取按指定速度从敌机现在的方向朝玩家的方向倾斜后的角度
			float targetRotationAngle = targetRotation.eulerAngles.y;
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
	
	private void IsNear()
	{
		float distance = Vector3.Distance(
			player.transform.position,
			transform.position );
		
		if ( distance < distanceFromPlayer )
		{
			// 降低速度（和玩家保持一定的距离）
			speed = followingSpeed;
			
			// 为了在一定时间后采取回避行动，设置计时器
			StartCoroutine("SetTimer");
		}
	}
	
	IEnumerator SetTimer()
	{
		// 等待一定的时间
		yield return new WaitForSeconds( followingTime );
		
		// 开始回避行动
		isUTurn = true;
	}
	
	private void Backward()	
	{
		// 玩家存在时的处理
		if ( player )
		{
			// 获取玩家的方向
			Vector3 playerPosition = player.transform.position;
			Vector3 relativePosition = playerPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
			
			// 获取按指定速度从敌机现在的方向朝玩家的反方向倾斜后的角度
			float targetRotationAngle = targetRotation.eulerAngles.y * - 1;
			float currentRotationAngle = transform.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnSpeed * Time.deltaTime );
			Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
			
			// 改变敌机的角度
			transform.rotation = tiltedRotation;
		
			// 使敌机前进
			transform.Translate ( new Vector3( 0f, 0f, uTurnSpeed * Time.deltaTime ) );
		}
	}
	
	void OnDestroy()
	{
		if ( this.GetComponent<EnemyStatus>() )
		{
			if ( subScreenMessage != null )
			{
				if (
					this.GetComponent<EnemyStatus>().GetIsBreakByPlayer() ||
					this.GetComponent<EnemyStatus>().GetIsBreakByStone() )
				{
					subScreenMessage.SendMessage("SetMessage", " ");
					subScreenMessage.SendMessage("SetMessage", "DESTROYED PATROL SHIP." );
					subScreenMessage.SendMessage("SetMessage", " ");
				}
				else
				{
					subScreenMessage.SendMessage("SetMessage", " ");
					subScreenMessage.SendMessage("SetMessage", "LOST PATROL SHIP..." );
					subScreenMessage.SendMessage("SetMessage", " ");
				}
			}
		}
	}

}

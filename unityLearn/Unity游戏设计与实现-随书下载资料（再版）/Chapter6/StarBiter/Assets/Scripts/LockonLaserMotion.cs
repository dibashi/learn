using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 控制锁定激光的运动
// ----------------------------------------------------------------------------
public class LockonLaserMotion : MonoBehaviour {
	
	public float		laserSpeed = 20f;				// 锁定激光的速度
	public float		turnRate   = 5f;				// 锁定激光的旋转度
	public float		turnRateAcceleration = 18.0f;	// 锁定激光的旋转度的增加量
	public GameObject	targetEnemy;					// 目标敌机
	public float		power = 3;						// 攻击力
	
	private int			lockonBonus = 1;				// 锁定激光发射时的锁定奖励
	
	private int			targetId;						// 目标的实例ID
	private	GameObject	scoutingLaser;					// 索敌激光
	private AudioMaker	audioMaker;						// 锁定激光的音效生成器
	
	private bool		isStart = false;				// 射出锁定激光
	private bool		isClear = false;				// 销毁锁定激光
	
	void Start () 
	{
		// 获取索敌激光的实例
		scoutingLaser = GameObject.FindGameObjectWithTag("ScoutingLaser");
		
		// 获取锁定激光的音效生成器实例
		audioMaker = GameObject.FindGameObjectWithTag("LockonLaserAudioMaker").GetComponent<AudioMaker>();
		
		// 播放音效
		if ( audioMaker )
		{
			audioMaker.Play( this.gameObject );
		}
	}
	
	void Update () 
	{
		// 开始发射锁定激光？
		if ( isStart )
		{
			// 使锁定激光前进
			ForwardLaser();
		
			// 确认目标销毁
			IsDestroyTarget();
		}
	}
	
	// ------------------------------------------------------------------------
	// 使锁定激光前进
	// ------------------------------------------------------------------------
	private void ForwardLaser()
	{
		// 当有敌机时才执行处理
		if ( targetEnemy )
		{
			// 获取敌机的方向
			Vector3		enemyPosition    = targetEnemy.gameObject.transform.position;
			Vector3		relativePosition = enemyPosition - transform.position;
			Quaternion	targetRotation   = Quaternion.LookRotation( relativePosition );
			
			// 获取从锁定激光的当前方向朝敌机按照指定比例旋转后的角度
			float	targetRotationAngle = targetRotation.eulerAngles.y;
			float	currentRotationAngle = transform.eulerAngles.y;

			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnRate * Time.deltaTime );

			Quaternion	tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
			
			// 慢慢增大转弯的角度（防止出现激光围绕着敌机无限循环的状况）
			turnRate += turnRateAcceleration * Time.deltaTime;
			
			// 改变激光的角度
			transform.rotation = tiltedRotation;

			// 激光前进
			transform.Translate ( new Vector3( 0f, 0f, laserSpeed * Time.deltaTime ) );
		
		}
	}
	
	// ------------------------------------------------------------------------
	// 设置敌机目标
	// ------------------------------------------------------------------------
	private void SetTargetEnemy( GameObject targetEnemy )
	{
		this.targetEnemy = targetEnemy;
		this.targetId = targetEnemy.GetInstanceID();
		
		isStart = true;	// 发射锁定激光
	}

	// ------------------------------------------------------------------------
	// 设置发射锁定激光时的锁定奖励
	// ------------------------------------------------------------------------
	private void SetLockonBonus( int lockonBonus )
	{
		this.lockonBonus = lockonBonus;
	}
	
	// ------------------------------------------------------------------------
	// 判断是否击中敌机
	// ------------------------------------------------------------------------
	void OnTriggerEnter( Collider collider )
	{
		
		// 击中敌机时发出销毁敌机的命令
		int colliderId = collider.gameObject.GetInstanceID();
		
		if ( colliderId == targetId )
		{			
			// 销毁敌机的指令
			isClear = true;
			collider.gameObject.SendMessage( "SetLockonBonus", lockonBonus );
			collider.gameObject.SendMessage( "SetIsBreakByLaser", power );
		}
	}
	
	// ------------------------------------------------------------------------
	// 敌机被销毁后的处理
	// ------------------------------------------------------------------------
	private void IsDestroyTarget()
	{
		
		if ( !targetEnemy || isClear )
		{
			// 销毁锁定激光
			Destroy( this.gameObject );
			
			// 减少锁定数量
			scoutingLaser.SendMessage( "DecreaseLockonCount", targetId );
		}
		
	}

}

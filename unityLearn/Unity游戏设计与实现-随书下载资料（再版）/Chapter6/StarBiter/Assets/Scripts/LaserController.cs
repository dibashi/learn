using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 控制从Boss Laser 射出激光的运动
// ----------------------------------------------------------------------------
public class LaserController : MonoBehaviour {

	public float bulletSpeed = 1f;					// 激光的速度
	public float laserSize = 100f;					// 激光的长度
	
	private GameObject target;						// 目标
	private BattleSpaceController battleSpaceContoller;	// 战斗空间
	
	private float breakingDistance = 20f;			// 激光消灭的条件（目标和激光的距离大于指定值）
	private bool isStart = false;					// 发射激光（true：射出）
	
	void Start () {
		
		// 获取战斗空间
		battleSpaceContoller =
			GameObject.FindGameObjectWithTag("BattleSpace")
				.GetComponent<BattleSpaceController>();
		
	}
	
	void Update () {
		
		if ( isStart )
		{
			// 使激光前进
			ForwardBullet();
		
			// 目标销毁后激光也销毁
			IsDestroyTarget();
			
			// 如果激光在范围之外则销毁之
			IsOverTheDistance();
		}
	}
	
	// ------------------------------------------------------------------------
	// 使激光前进
	// ------------------------------------------------------------------------
	private void ForwardBullet()
	{
		// 只有敌机存在时执行处理
		if ( target )
		{
			// 激光前进
			transform.Translate ( new Vector3( 0f, 0f, bulletSpeed * Time.deltaTime ) );
			
			// 叠加战斗空间的滚动方向
			transform.position -= battleSpaceContoller.GetAdditionPos();
			
			// 激光延伸
			if ( transform.localScale.z < laserSize )
			{
				transform.localScale = new Vector3( 
					transform.localScale.x,
					transform.localScale.y,
					transform.localScale.z + ( bulletSpeed * Time.deltaTime * 30 ) );
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 设置目标
	// ------------------------------------------------------------------------
	public void SetTarget( GameObject target )
	{
		this.target = target;
		
		// 指定激光的方向
		SetDirection();
		
		isStart = true;	// 射出激光
	}
	
	// ------------------------------------------------------------------------
	// 将前进方向指向目标
	// ------------------------------------------------------------------------
	private void SetDirection()
	{
		// 目标存在时才执行处理
		if ( target )
		{
			// 获取目标的方向
			Vector3 targetPosition = target.gameObject.transform.position;
			Vector3 relativePosition = targetPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
			
			// 变更激光的角度
			transform.rotation = targetRotation;
		}
	}
	
	// ------------------------------------------------------------------------
	// 自身的销毁处理
	// ------------------------------------------------------------------------
	private void IsDestroyTarget()
	{
		if ( !target )
		{
			// 销毁激光
			Destroy( this.gameObject );
		}
	}
	
	// ------------------------------------------------------------------------
	// 和目标的距离超过一定值则销毁
	// ------------------------------------------------------------------------
	private void IsOverTheDistance()
	{
		if ( target )
		{
			float distance = Vector3.Distance(
				target.transform.position,
				transform.position );
			
			if ( distance > breakingDistance )
			{
				// 销毁激光
				Destroy( this.gameObject );
			}
		}
	}
}

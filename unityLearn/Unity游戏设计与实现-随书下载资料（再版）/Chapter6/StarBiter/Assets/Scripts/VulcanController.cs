using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 控制 Boss vulcan 射出的机关炮弹的运动
// ----------------------------------------------------------------------------
public class VulcanController : MonoBehaviour {

	public float bulletSpeed = 5f;					// 机关炮弹的速度
	
	private GameObject target;						// 目标

	private BattleSpaceController battleSpaceContoller;	// 战斗空间
	
	private float breakingDistance = 20f;			// 机关炮弹的消灭条件（目标和机关炮弹的距离超过一定值）
	private bool isStart = false;					// 机关炮弹（true：射出）
	
	void Start () {
		
		// 获取战斗空间
		battleSpaceContoller = 
			GameObject.FindGameObjectWithTag("BattleSpace")
				.GetComponent<BattleSpaceController>();
		
	}
	
	void Update () {
		
		if ( isStart )
		{
			// 使机关炮弹前进
			ForwardBullet();
		
			// 目标销毁后也将机关炮弹销毁
			IsDestroyTarget();
			
			// 当机关炮弹跑出范围外时销毁
			IsOverTheDistance();
		}
	}
	
	// ------------------------------------------------------------------------
	// 使机关炮弹前进
	// ------------------------------------------------------------------------
	private void ForwardBullet()
	{
		// 当有敌机时才执行处理
		if ( target )
		{
			// 机关炮弹前进、
			transform.Translate ( new Vector3( 0f, 0f, bulletSpeed * Time.deltaTime ) );
			
			// 叠加战斗空间的滚动方向
			transform.position -= battleSpaceContoller.GetAdditionPos();
		}
	}
	
	// ------------------------------------------------------------------------
	// 设置目标
	// ------------------------------------------------------------------------
	public void SetTarget( GameObject target )
	{
		this.target = target;

		isStart = true;	// 射出机关炮弹
	}
	
	// ------------------------------------------------------------------------
	// 使机关炮弹朝着目标方向前进
	// ------------------------------------------------------------------------
	private void SetRotation( float rate )
	{
		// 获取目标的方向
		Vector3 targetPosition = target.gameObject.transform.position;
		Vector3 relativePosition = targetPosition - transform.position;
		Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
		
		// 获取按指定速度从机关炮弹当前方向往敌机方向倾斜后的角度
		float targetRotationAngle = targetRotation.eulerAngles.y;
		float currentRotationAngle = transform.eulerAngles.y;
		currentRotationAngle = Mathf.LerpAngle(
			currentRotationAngle,
			targetRotationAngle,
			rate * Time.deltaTime );
		Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
		
		// 改变激光的角度
		transform.rotation = tiltedRotation;
	}
	
	// ------------------------------------------------------------------------
	// 自身的销毁处理
	// ------------------------------------------------------------------------
	private void IsDestroyTarget()
	{
		if ( !target )
		{
			// 销毁机关炮弹
			Destroy( this.gameObject );
		}
	}
	
	// ------------------------------------------------------------------------
	// 当和目标距离超过一定值后销毁
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
				// 销毁机关炮弹
				Destroy( this.gameObject );
			}
		}
	}
}

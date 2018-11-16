using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 控制Boss Shot 射出的炮弹运动
// ----------------------------------------------------------------------------
public class ShotController : MonoBehaviour {

	public float bulletSpeed = 5f;					// 炮弹的速度
	
	private GameObject target;						// 目标
	private BattleSpaceController battleSpaceContoller;	// 战斗空间
	
	private float breakingDistance = 20f;			// 炮弹销毁的条件（目标和炮弹的距离超过一定值）
	private bool isStart = false;					// 射出炮弹（true：射出）
	
	void Start () {
		
		// 获取战斗空间
		battleSpaceContoller = 
			GameObject.FindGameObjectWithTag("BattleSpace")
				.GetComponent<BattleSpaceController>();
		
	}
	
	void Update () {
		
		if ( isStart )
		{
			// 使炮弹前进
			ForwardBullet();
		
			// 目标销毁后将炮弹也销毁
			IsDestroyTarget();
			
			// 如果炮弹超出范围外将其销毁
			IsOverTheDistance();
		}
	}
	
	// ------------------------------------------------------------------------
	// 使炮弹前进
	// ------------------------------------------------------------------------
	private void ForwardBullet()
	{
		// 当有敌机时才执行处理
		if ( target )
		{
			// 炮弹前进
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
		
		isStart = true;	// 射出炮弹
	}
	
	// ------------------------------------------------------------------------
	// 销毁自身处理
	// ------------------------------------------------------------------------
	private void IsDestroyTarget()
	{
		if ( !target )
		{
			// 销毁炮弹
			Destroy( this.gameObject );
		}
	}
	
	// ------------------------------------------------------------------------
	// 当和目标的距离超过一定值后销毁
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
				// 销毁炮弹
				Destroy( this.gameObject );
			}
		}
	}
}

using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 控制玩家的射击
// ----------------------------------------------------------------------------
public class PlayerShotController : MonoBehaviour {

	public float bulletSpeed = 15f;					// 炮弹的速度
	public int power = 2;							// 攻击力

	private BattleSpaceController battleSpaceContoller;	// 战斗空间
	private GameObject player;							// 玩家的实例
	
	private float breakingDistance = 20f;			// 炮弹销毁的条件（玩家和炮弹的距离超过一定值）

	private bool isClear = false;					// 炮弹销毁
	
	void Start () 
	{
		// 获取战斗空间
		battleSpaceContoller =
			GameObject.FindGameObjectWithTag("BattleSpace")
				.GetComponent<BattleSpaceController>();
		
		// 获取player的实例
		player = GameObject.FindGameObjectWithTag("Player");

	}
	
	void Update ()
	{
		// 使炮弹前进
		ForwardBullet();
	
		// 击中敌机后销毁炮弹
		IsDestroy();
		
		// 如果炮弹跑出范围外将其销毁
		IsOverTheDistance();
	}
	
	// ------------------------------------------------------------------------
	// 使炮弹前进
	// ------------------------------------------------------------------------
	private void ForwardBullet()
	{
		// 炮弹前进
		transform.Translate ( new Vector3( 0f, 0f, bulletSpeed * Time.deltaTime ) );
		
		// 叠加战斗空间的滚动方向
		transform.position -= battleSpaceContoller.GetAdditionPos();
	}
	
	// ------------------------------------------------------------------------
	// 炮弹击中时的处理
	// ------------------------------------------------------------------------
	void OnTriggerEnter( Collider collider )
	{
		if ( collider.tag == "Enemy" )
		{
			// 向敌机发送销毁指令
			isClear = true;
			collider.gameObject.SendMessage( "SetIsBreakByShot", power );
		}
		
		if ( collider.tag == "Stone" )
		{
			// 向岩石发送销毁指令
			isClear = true;
			collider.gameObject.SendMessage( "SetIsBreakByShot", power );
		}
		
	}
		
	
	// ------------------------------------------------------------------------
	// 销毁处理
	// ------------------------------------------------------------------------
	private void IsDestroy()
	{
		
		if ( isClear )
		{
			// 销毁炮弹
			Destroy( this.gameObject );
		}
		
	}
	
	// ------------------------------------------------------------------------
	// 和玩家距离超过一定值后销毁
	// ------------------------------------------------------------------------
	private void IsOverTheDistance()
	{
		float distance = Vector3.Distance(
			player.transform.position,
			transform.position );
		
		if ( distance > breakingDistance )
		{
			// 销毁炮弹
			Destroy( this.gameObject );
		}
	}
}

using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 控制Boss Shot 炮弹的发射
// ----------------------------------------------------------------------------
public class ShotMaker : MonoBehaviour {

	public float fireInterval = 0.1f;		// 发射间隔
	public int numberOfBullets = 10;		// 一次发射炮弹数量
	
	public GameObject Shot;					// 炮弹的Prefab.
	
	private GameObject player;				// 玩家
	
	private int fireCount;					// 发射的次数
	private bool isFiring = false;			// 发射中
	private bool isMakingBullet = false;	// 正在生成炮弹
	
	private float fireAngle = 0;			// 发射角度
	
	void Start () {
	
		// 获取player的实例
		player = GameObject.FindGameObjectWithTag("Player");

	}
	
	void Update () {
		
		// 是否正在发射？
		if ( isFiring )
		{
			// 发射前？
			if ( fireCount == 0 )
			{
				// 计算发射角度
				SetAngle();
			}
			
			// 正在准备发射炮弹？
			if ( !isMakingBullet )
			{
				isMakingBullet = true;
				MakeBullet();
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 生成炮弹
	// ------------------------------------------------------------------------
	private void MakeBullet()
	{
		// 是否指定了炮弹的GameObject？
		if ( Shot )
		{
			// 生成炮弹
			GameObject tmpBullet;
			tmpBullet = Instantiate( Shot, transform.position, Quaternion.Euler( 0, fireAngle, 0 ) ) as GameObject;
			tmpBullet.SendMessage( "SetTarget", player );	
			
			// 累加发射数量
			fireCount++;
			
			// 每发炮弹角度间隔15度
			fireAngle -= 15f;
			
			// 发射指定次数后停止生成炮弹
			if ( fireCount >= numberOfBullets )
			{
				isFiring = false;
			}
			
			// 等待一定时间后再次发射
			StartCoroutine( WaitAndUpdateFlag( fireInterval ) );
	
		}
	}
	
	// ------------------------------------------------------------------------
	// 等待指定的时间后，改变状态
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUpdateFlag( float waitForSeconds )
	{
		// 等待
		yield return new WaitForSeconds( waitForSeconds );
		
		// 更新关卡
		isMakingBullet = false;
	}
	
	// ------------------------------------------------------------------------
	// 算出发射的角度
	// ------------------------------------------------------------------------
	private void SetAngle()
	{
		// 计算发射角度
		Vector3 targetPosition = player.transform.position;
		Vector3 relativePosition = targetPosition - transform.position;
		Quaternion tiltedRotation = Quaternion.LookRotation( relativePosition );
		fireAngle = tiltedRotation.eulerAngles.y + ( numberOfBullets / 2 ) * 15;
	}
	
	// ------------------------------------------------------------------------
	// 开始发射
	// ------------------------------------------------------------------------
	public void SetIsFiring()
	{
		fireCount = 0;
		this.isFiring = true;
	}
	
	// ------------------------------------------------------------------------
	// 返回判断当前是否正在发射
	// ------------------------------------------------------------------------
	public bool GetIsFiring()
	{
		return isFiring;
	}
}

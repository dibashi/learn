using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// Boss Laser 控制激光的发射
// Boss Laser Unit的状态管理
// ----------------------------------------------------------------------------
public class LaserMaker : MonoBehaviour {

	public float fireInterval = 1f;			// 发射间隔
	public int numberOfBullets = 3;			// 一次册发射数量
	
	public GameObject Laser;				// 激光的Prefab.
	
	private GameObject player;				// 玩家的实例
	private PlayerStatus playerStatus;		// 玩家状态的实例
	
	private GameObject shotPosition;		// 激光的发射位置
	
	private int fireCount;					// 发射的次数
	private bool isFiring = false;			// 发射中
	private bool isMakingBullet = false;	// 正在激光生成
	
	private PrintMessage printMessage;		// SubScreen的消息区域
	
	void Start () {
	
		// 获取player的实例
		player = GameObject.FindGameObjectWithTag("Player");
		
		// 获取玩家状态的实例
		playerStatus = player.GetComponent<PlayerStatus>();
		
		// 获取激光的发射位置信息
		shotPosition = GetComponentInChildren<Transform>().Find("ShotPosition").gameObject;
		
		// 获取SubScreenMessage的实例
		printMessage = Navi.get().GetPrintMessage();	
	}
	
	void Update () {
	
		// 是否正在发射？
		if ( isFiring )
		{
			// 是否在准备发射激光？
			if ( !isMakingBullet )
			{
				isMakingBullet = true;
				MakeLaser();
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 生成激光
	//  - 只有当玩家存活时才发射
	// ------------------------------------------------------------------------
	private void MakeLaser()
	{
		// 是否指定了激光的GameObject？
		if ( Laser )
		{
			// 生成激光
			GameObject tmpBullet;
			if ( playerStatus.GetIsNOWPLAYING() )
			{
				tmpBullet = Instantiate( Laser, shotPosition.transform.position, this.transform.rotation ) as GameObject;
				tmpBullet.SendMessage( "SetTarget", player );	
			}
				
			// 累加发射数量
			fireCount++;
			
			// 发射了指定数量后停止生成激光
			if ( fireCount >= numberOfBullets )
			{
				isFiring = false;
			}
			
			// 等待一定时间再进行下次发射
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
	// 开始发射
	// ------------------------------------------------------------------------
	public void SetIsFiring()
	{
		fireCount = 0;
		this.isFiring = true;
	}
	
	// ------------------------------------------------------------------------
	// BOSS Laser Unit被销毁时的处理
	// ------------------------------------------------------------------------
	void OnDestroy()
	{
		if ( this.GetComponent<EnemyStatus>() )
		{
			if (
				this.GetComponent<EnemyStatus>().GetIsBreakByPlayer() ||
				this.GetComponent<EnemyStatus>().GetIsBreakByStone() )
			{
				printMessage.SetMessage(" ");
				printMessage.SetMessage("DESTROYED LASER UNIT.");
				printMessage.SetMessage(" ");
			}
		}
	}
}

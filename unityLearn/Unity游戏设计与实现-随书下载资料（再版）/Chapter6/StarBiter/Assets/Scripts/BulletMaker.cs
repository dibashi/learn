using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 控制Boss Vulcan 机关炮的发射
// Boss Vulcan Unit的状态管理
// ----------------------------------------------------------------------------
public class BulletMaker : MonoBehaviour {
	
	public float fireInterval = 0.3f;		// 发射间隔
	public int numberOfBullets = 15;		// 一次的发射数量
	
	public GameObject Bullet;				// 机关炮弹的Prefab.
	
	private GameObject player;				// 玩家的实例
	private PlayerStatus playerStatus;		// 玩家状态的实例
	
	private GameObject shotPosition;		// 机关炮弹的发射位置
	
	private int fireCount;					// 发射炮弹的次数
	private bool isFiring = false;			// 正在发射炮弹
	private bool isMakingBullet = false;	// 正在生成炮弹
	
	private PrintMessage printMessage;		// SubScreen的消息区域
	
	void Start () {
	
		// 取得player的实例
		player = GameObject.FindGameObjectWithTag("Player");
		
		// 取得玩家状态的实例
		playerStatus = player.GetComponent<PlayerStatus>();
		
		// 取得机关炮弹的发射位置信息
		shotPosition = GetComponentInChildren<Transform>().Find("ShotPosition").gameObject;
		
		// 取得SubScreenMessage的实例
		printMessage = Navi.get().GetPrintMessage();
	}
	
	void Update () {
	
		// 是否正在发射？
		if ( isFiring )
		{
			// 是否正在准备发射机关炮弹？
			if ( !isMakingBullet )
			{
				isMakingBullet = true;
				MakeBullet();
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 生成机关炮弹
	//  - 只有玩家还存活时才会发射
	// ------------------------------------------------------------------------
	private void MakeBullet()
	{
		// 是否已经指定了机关炮弹的GameObject？
		if ( Bullet )
		{
			// 生成机关炮弹
			GameObject tmpBullet;
			if ( playerStatus.GetIsNOWPLAYING() )
			{
				tmpBullet = Instantiate( Bullet, shotPosition.transform.position, this.transform.rotation ) as GameObject;
				tmpBullet.SendMessage( "SetTarget", player );	
			}
			
			// 统计发射数量
			this.fireCount++;
			
			// 发射了指定数量后停止生成机关炮弹
			if ( this.fireCount >= numberOfBullets )
			{
				isFiring = false;
			}
			
			// 等待一定时间再进入下一个发射
			StartCoroutine( WaitAndUpdateFlag( fireInterval ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 等待指定的时候后，改变状态
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUpdateFlag( float waitForSeconds )
	{
		// 等待
		yield return new WaitForSeconds( waitForSeconds );
		
		// 更新stage
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
	// BOSS Vulcan Unit被销毁时的处理
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
				printMessage.SetMessage("DESTROYED VULCAN UNIT.");
				printMessage.SetMessage(" ");
			}
		}
	}
}

using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 玩家的操作／控制
// ----------------------------------------------------------------------------
public class PlayerController : MonoBehaviour {

	private	GameObject mainCamera;					// 主摄像机
	private	GameObject scoutingLaser;				// 索敌激光
	private bool isScanOn = false;					// 索敌模式
	private bool isAlive = false;					// 玩家是否生存
	
	private PlayerStatus playerStatus;				// PlayerStatus实例
	
	void Start () 
	{
		// 获取主摄像机的实例
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		
		// 获取索敌激光的实例
		scoutingLaser = GameObject.FindGameObjectWithTag("ScoutingLaser");
		
		// 获取PlayerStatus实例
		playerStatus = this.gameObject.GetComponent<PlayerStatus>();

	}
	
	void Update ()
	{
		// 玩家是否仍生存？
		if ( isAlive )
		{
			// 玩家状态的实例是否存在？
			if ( playerStatus )
			{
				// 玩家是否能够操作？
				if ( playerStatus.GetCanPlay() == true )
				{
					// 设置玩家的方向朝着鼠标光标方向
					SetPlayerDirection();
					
					// 切换索敌模式
					ChangeScanMode();
				}
			}
		}

	}
	
	// ------------------------------------------------------------------------
	// 玩家的方向朝向鼠标光标方向
	// ------------------------------------------------------------------------
	private void SetPlayerDirection()
	{
		// 求出指向鼠标光标位置的角度
		Vector3 mousePos = GetWorldPotitionFromMouse();
		Vector3 relativePos = mousePos - transform.position;
		Quaternion tmpRotation = Quaternion.LookRotation( relativePos );
		
		// 变更玩家的角度
		transform.rotation = tmpRotation;
		
	}

	// ------------------------------------------------------------------------
	// 切换索敌模式
	// ------------------------------------------------------------------------
	private void ChangeScanMode()
	{
		// 鼠标左键被按下时设置索敌模式为ON
		if ( isScanOn == false ) {
			if ( Input.GetButtonDown("Fire1") ) {
				isScanOn = true;
				scoutingLaser.SendMessage( "SetIsScanOn", isScanOn );
				SendMessage( "SetFireOrder" );
			}
		}
		
		// 松开鼠标左键时设置索敌模式为OFF
		if ( isScanOn == true ) {
			if ( Input.GetButtonUp("Fire1") ) {
				isScanOn = false;
				scoutingLaser.SendMessage( "SetIsScanOn", isScanOn );
			}
		}
	}

	// ------------------------------------------------------------------------
	// 设置玩家的生存状态
	// ------------------------------------------------------------------------
	public void SetIsAlive( bool isAlive )
	{
		this.isAlive = isAlive;
	}
	
	// ------------------------------------------------------------------------
	// 将鼠标的位置变换为3D空间内的世界坐标
	//   - 求出下列二者的交点
	//     1. 穿过鼠标光标和摄像机位置的直线
	//     2. 穿过平面中心的水平面
	// ------------------------------------------------------------------------
	private Vector3	GetWorldPotitionFromMouse()
	{
		Vector3	mousePosition = Input.mousePosition;

		// 穿过平面中心的水平（法线为Y轴。XZ平面）面
		// 以玩家为中心
		Plane plane = new Plane( Vector3.up, new Vector3( 0f, 0f, 0f ) );
		
		// 穿过摄像机位置和鼠标光标的直线
		Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay( mousePosition );

		// 求出上面二者的交点
		float depth;
		
		plane.Raycast( ray, out depth );
		
		Vector3	worldPosition;
		
		worldPosition = ray.origin + ray.direction * depth;
		
		// Y坐标和玩家保持一致
		worldPosition.y = 0;
		
		return worldPosition;
	}
	
	// ------------------------------------------------------------------------
	// 重置所有的值
	// ------------------------------------------------------------------------
	public void Reset()
	{
		// 将索敌激光设置为OFF
		isScanOn = false;
	}
	
}
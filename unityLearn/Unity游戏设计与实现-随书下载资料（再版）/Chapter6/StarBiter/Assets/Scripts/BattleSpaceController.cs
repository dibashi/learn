using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// BattleSpaceController
//  - 控制战斗空间（玩家可以飞行的空间）的滚动
//  - 使用方法
//    - 将这个脚本挂在空的游戏对象上
//    - 配置岩石“M12_asteroid”，交叉网格线“T06_crossharch”作为上述游戏对象的子结点
//  - 运动流程
//    - 战斗空间可以沿着玩家的前进方向和反方向运动
//    - 运动量可以通过其他对象参考算出
//  - 注意点
//    - 在战斗空间的边界附近不放置岩石
//      （边界向另一侧移动的瞬间能看见岩石消失）
// ----------------------------------------------------------------------------
public class BattleSpaceController : MonoBehaviour {
	
	public float scrollSpeed = 3f;					// 结合玩家的前进方向
													// 战斗空间的滚动速度
	
	private Vector3 additionPosition;				// 战斗空间移动的移动量
	private GameObject player;						// 玩家的实例
	
	private float bgX1 = -40f;						// 战斗空间的边界（左端）
	private float bgX2 = 40f;						// 战斗空间的边界（右端）
	private float bgZ1 = -40f;						// 战斗空间的边界（上端）
	private float bgZ2 = 40f;						// 战斗空间的边界（下端）
	
	void Start () {
	
		// 取得玩家的实例
		player = GameObject.FindGameObjectWithTag("Player");
		
	}
	
	void LateUpdate() {
		
		// 滚动战斗空间
		ScrollBattleSpace();
		
	}
	
	// ------------------------------------------------------------------------
	// 滚动战斗空间
	// ------------------------------------------------------------------------
	private void ScrollBattleSpace()
	{
		
		// 取得玩家的方向
		Quaternion playerRotation = player.transform.rotation;

		// 滚动战斗空间（沿着玩家方向的反方向前进）
		additionPosition = playerRotation * Vector3.forward * scrollSpeed * Time.deltaTime;
		transform.position -= additionPosition;
		
		// 战斗空间的循环控制
		IsOutOfWorld();
		
	}
	
	// ------------------------------------------------------------------------
	// 战斗空间的循环控制
	// ------------------------------------------------------------------------
	private void IsOutOfWorld()
	{
		// 超出战斗空间的右端
		if ( transform.position.x < bgX1 )
		{
			// 战斗空间往左端运动
			transform.position = new Vector3(
				bgX2,
				transform.position.y,
				transform.position.z );
		}
		
		// 超出战斗空间的左端
		if ( transform.position.x > bgX2 )
		{
			// 战斗空间往右端运动
			transform.position = new Vector3(
				bgX1,
				transform.position.y,
				transform.position.z );
		}
		
		// 超出战斗空间的上端
		if ( transform.position.z < bgZ1 )
		{
			// 战斗空间往下端运动
			transform.position = new Vector3(
				transform.position.x,
				transform.position.y,
				bgZ2 );
		}
		
		// 超出战斗空间的下端
		if ( transform.position.z > bgZ2 )
		{
			// 战斗空间往上端运动
			transform.position = new Vector3(
				transform.position.x,
				transform.position.y,
				bgZ1 );
		}
	}
	
	// ------------------------------------------------------------------------
	// 返回战斗空间移动的移动量
	// ------------------------------------------------------------------------
	public Vector3 GetAdditionPos()
	{
		return additionPosition;
	}

}

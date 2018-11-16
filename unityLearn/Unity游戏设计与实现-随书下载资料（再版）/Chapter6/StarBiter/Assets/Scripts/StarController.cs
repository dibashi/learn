using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 星星的运动
//  - 仕様.
//    - 将3张星星图片叠在一起，各自指定好速度朝玩家相反方向运动
//  - 使用方法
//    - 将这个脚本设定到任意的游戏对象上
//  - 事先准备
//    - 准备好使用星星纹理贴图的片面（下面称为星板）
//    - 星板纹理的Tiling的x，y都设置为3
//    - 给星板的Star1,Star2,Star3 分别加上标签
//    - 星板沿X轴方向移动并查找移动前相同图像的位置，将其值设置为maxRightPositionX,maxLeftPositionX
//    - 星板沿Z轴方向移动并查找移动前相同图像的位置，将其值设置为maxTopPositionZ,maxBottomPositionZ
//  - 备注
//    - 为了能使星星永远运动，星板的位置会回到指定的坐标
//    - 为了能平滑地实现替换，星板运动时需要确认替换的坐标
// ----------------------------------------------------------------------------
public class StarController : MonoBehaviour {

	public float scrollSpeedStar1 = 0.2f;					// 星星（Star1）的滚动速度
	public float scrollSpeedStar2 = 0.5f;					// 星星（Star2）的滚动速度
	public float scrollSpeedStar3 = 1f;						// 星星（Star3）的滚动速度
	
	private GameObject player;								// 玩家的实例
	
	const int MAX_STARS = 3;								// 星板的数量
	private GameObject[] stars = new GameObject[MAX_STARS];	// 星板的实例
	private float[] scrollSpeed = new float[MAX_STARS];		// 星板的滚动速度
	
	private float maxRightPositionX = -10f;					// 星板的替换位置X
	private float maxLeftPositionX = 10f;					// 星板的替换位置X
	private float maxTopPositionZ = -10f;					// 星板的替换位置Z
	private float maxBottomPositionZ = 10f;					// 星板的替换位置Z
	
	void Start ()
	{
		// 获取玩家的实例
		player = GameObject.FindGameObjectWithTag("Player");
		
		// 获取星板的实例
		GameObject star1 = GameObject.FindGameObjectWithTag("Star1");
		GameObject star2 = GameObject.FindGameObjectWithTag("Star2");
		GameObject star3 = GameObject.FindGameObjectWithTag("Star3");
		
		// 为了方便统一处理，集中到数组
		stars[0] = star1;
		stars[1] = star2;
		stars[2] = star3;
		scrollSpeed[0] = scrollSpeedStar1;
		scrollSpeed[1] = scrollSpeedStar2;
		scrollSpeed[2] = scrollSpeedStar3;
	}
	
	// 确定了玩家的前进方向后的处理
	void LateUpdate() 
	{
		// 滚动星星（以和玩家的前进方向相反运动）
		ScrollStars();
	}
	
	// ------------------------------------------------------------------------
	// 滚动星星（和玩家的前进方向相反运动）
	// ------------------------------------------------------------------------
	private void ScrollStars()
	{
		// 如果Player不存在则处理结束
		if ( !player )
		{
			return;
		}
		
		// 获取玩家的方向
		Quaternion playerRot = player.transform.rotation;

		// 星星按照玩家相反方向前进
		for( int i = 0; i < MAX_STARS; i++ )
		{
			if ( !stars[i] || scrollSpeed[i] == 0 )
			{
				// 当星板的实例或者滚动速度未设置时不执行处理
				continue;
			}
			
			// 滚动星星
			Vector3 additionPos = playerRot * Vector3.forward * scrollSpeed[i] * Time.deltaTime;
			stars[i].transform.position -= additionPos;
			
			// 控制星星的循环滚动
			IsOutOfWorld( stars[i] );
		}
	}
	
	// ------------------------------------------------------------------------
	// 替换星星位置时的循环控制
	// ------------------------------------------------------------------------
	private void IsOutOfWorld( GameObject star )
	{
		if ( star.transform.position.x < maxRightPositionX )
		{
			star.transform.position = new Vector3(
				maxLeftPositionX,
				star.transform.position.y,
				star.transform.position.z );
		}
		if ( star.transform.position.x > maxLeftPositionX )
		{
			star.transform.position = new Vector3(
				maxRightPositionX,
				star.transform.position.y,
				star.transform.position.z );
		}
		if ( star.transform.position.z < maxTopPositionZ )
		{
			star.transform.position = new Vector3(
				star.transform.position.x,
				star.transform.position.y,
				maxBottomPositionZ );
		}
		if ( star.transform.position.z > maxBottomPositionZ )
		{
			star.transform.position = new Vector3(
				star.transform.position.x,
				star.transform.position.y,
				maxTopPositionZ );
		}
	}
}

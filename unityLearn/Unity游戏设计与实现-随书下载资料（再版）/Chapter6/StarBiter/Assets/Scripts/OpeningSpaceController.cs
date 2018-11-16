using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 背景的星空滚动
//  - 只会从上往下滚动
// ----------------------------------------------------------------------------
public class OpeningSpaceController : MonoBehaviour {

	public float scrollSpeedStar1 = 0.2f;			// 星星滚动的速度
	public float scrollSpeedStar2 = 0.5f;			// 星星滚动的速度
	public float scrollSpeedStar3 = 1f;				// 星星滚动的速度
	
	const int MAX_STARS = 3;
	private GameObject[] stars = new GameObject[MAX_STARS];	// 星星
	private float[] scrollSpeed = new float[MAX_STARS];		// 星星的滚动速度
	
	private float bgZ1 = -10f;						// 战斗空间的边界（上端）
	private float bgZ2 = 10f;						// 战斗空间的边界（下端）
	
	private bool isEaseIn = false;					// EaseIn
	private float easeInRate = 0.6f;				// 衰减率
	
	void Start ()
	{
		// 获取星星的实例
		GameObject star1 = GameObject.FindGameObjectWithTag("Star1");
		GameObject star2 = GameObject.FindGameObjectWithTag("Star2");
		GameObject star3 = GameObject.FindGameObjectWithTag("Star3");
		stars[0] = star1;
		stars[1] = star2;
		stars[2] = star3;
		
		// 设定星星的滚动速度
		scrollSpeed[0] = scrollSpeedStar1;
		scrollSpeed[1] = scrollSpeedStar2;
		scrollSpeed[2] = scrollSpeedStar3;
	}

	void LateUpdate()
	{
		// 滚动星星（从上往下）
		Scroll();
	}
	
	// ------------------------------------------------------------------------
	// 滚动星星（从上往下运动）
	// ------------------------------------------------------------------------
	private void Scroll()
	{
		// 星星朝Z轴的负方向前进
		for( int i = 0; i < MAX_STARS; i++ )
		{
			if ( !stars[i] || scrollSpeed[i] == 0 )
			{
				// 未设定星星的游戏对象或者滚动速度时不执行处理
				continue;
			}
			
			// 星星前进
			Vector3 additionPos = new Vector3( 0, 0, 1f )  * scrollSpeed[i] * Time.deltaTime;
			stars[i].transform.position -= additionPos;
			
			// 星星的循环控制
			IsOutOfWorld( stars[i] );
			
			// EaseIn
			if ( isEaseIn )
			{
				scrollSpeed[i] -= scrollSpeed[i] * easeInRate * Time.deltaTime;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 当星星超出了主摄像机的显示区域外时进行循环利用
	// ------------------------------------------------------------------------
	private void IsOutOfWorld( GameObject star )
	{
		
		if ( star.transform.position.z < bgZ1 )
		{
			star.transform.position = new Vector3(
				star.transform.position.x,
				star.transform.position.y,
				bgZ2 );
		}

	}
	
	public void SetEaseIn()
	{
		isEaseIn = true;
	}
	
}

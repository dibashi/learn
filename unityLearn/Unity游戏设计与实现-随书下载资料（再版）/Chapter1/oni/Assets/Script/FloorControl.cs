using UnityEngine;
using System.Collections;

public class FloorControl : MonoBehaviour {

	// 摄像机
	private GameObject		main_camera = null;

	// 初始位置
	private Vector3	initial_position;

	// 地面的宽度（X方向）
	public	const float	WIDTH = 10.0f*4.0f;

	// 地面模型的数量
	public const int		MODEL_NUM = 3;

	// ================================================================ //

	void	Start() 
	{
		// 查找摄像机的实例对象
		this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");

		this.initial_position = this.transform.position;

		this.GetComponent<Renderer>().enabled = SceneControl.IS_DRAW_DEBUG_FLOOR_MODEL;

	}
	
	void	Update()
	{
		// 生成无限循环地面

#if true
		// 简易方法
		// 如果跑出画面之外就移动到玩家的前方（后方）
		// 玩家移动时会存在问题


		// 背景全体（所有的模型并排）的宽度
		//
		float	total_width = FloorControl.WIDTH*FloorControl.MODEL_NUM;

		// 背景的位置
		Vector3	floor_position = this.transform.position;

		// 摄像机的位置
		Vector3	camera_position = this.main_camera.transform.position;

		if(floor_position.x + total_width/2.0f < camera_position.x) {

			// 移动到前面
			floor_position.x += total_width;

			this.transform.position = floor_position;
		}

		if(camera_position.x < floor_position.x - total_width/2.0f) {

			// 移动到后面
			floor_position.x -= total_width;

			this.transform.position = floor_position;
		}
#else
		// 修改后，玩家移动时也不会出问题的方法

		// 背景全体（所有的模型并列）的宽度
		//
		float		total_width = FloorControl.WIDTH*FloorControl.MODEL_NUM;

		Vector3		camera_position = this.main_camera.transform.position;

		float		dist = camera_position.x - this.initial_position.x;

		// 模型出现在total_width 的整数倍位置
		// 用初始位置的距离除以整体背景的宽度，再四舍五入

		int			n = Mathf.RoundToInt(dist/total_width);

		Vector3		position = this.initial_position;

		position.x += n*total_width;

		this.transform.position = position;
#endif
	}
}

using UnityEngine;

// 方块的旋转（交换，颜色改变）
public struct RotateAction {


	public enum TYPE {

		NONE = -1,

		SWAP_UP = 0,		// 交换 从下到上
		SWAP_DOWN,			// 交换 从上到下
		COLOR_CHANGE,		// 颜色改变（围绕中间）

		NUM,
	};

	public bool			is_active;		// 是否执行中？
	public float		timer;			// 经过时间
	public float		rate;			// 经过时间的比率


	public TYPE			type;

	public Block.COLOR_TYPE	target_color;	// 变化后的颜色

	public static float	rotate_time_swap = 0.25f;

	public static float	ROTATE_TIME_SWAP_MIN = 0.1f;
	public static float	ROTATE_TIME_SWAP_MAX = 1.0f;

	// ---------------------------------------------------------------- //

	// 初始化
	public void init()
	{
		this.is_active = false;
		this.timer     = 0.0f;
		this.rate      = 0.0f;
		this.type      = RotateAction.TYPE.NONE;
	}

	// 开始旋转动作
	public void start(RotateAction.TYPE type)
	{
		this.is_active = true;
		this.timer     = 0.0f;
		this.rate      = 0.0f;
		this.type      = type;
	}

	// 执行旋转动作
	public void	execute(StackBlock block)
	{
		float	x_angle = 0.0f;
		float	rotate_time;

		if(this.type == RotateAction.TYPE.COLOR_CHANGE) {

			rotate_time = 0.5f;

		} else {

			rotate_time = RotateAction.rotate_time_swap;
		}

		if(this.is_active) {

			this.timer += Time.deltaTime;

			// 检测是否结束

			if(this.timer > rotate_time) {

				this.timer     = rotate_time;
				this.is_active = false;
			}

			// 旋转的中心

			Vector3		rotation_center = Vector3.zero;
			
			if(this.is_active) {

				switch(this.type) {
	
					case RotateAction.TYPE.SWAP_UP:
					{
						rotation_center.y = -Block.SIZE_Y/2.0f;
					}
					break;
	
					case RotateAction.TYPE.SWAP_DOWN:
					{
						rotation_center.y =  Block.SIZE_Y/2.0f;
					}
					break;

					case RotateAction.TYPE.COLOR_CHANGE:
					{
						rotation_center.y =  0.0f;
					}
					break;
				}

				// 角度

				this.rate = this.timer/rotate_time;

				this.rate = Mathf.Lerp(-Mathf.PI/2.0f, Mathf.PI/2.0f, this.rate);
				this.rate = (Mathf.Sin(this.rate) + 1.0f)/2.0f;
				
				x_angle = Mathf.Lerp(-180.0f, 0.0f, this.rate);
			}

			// 以rotation_center 为中心，进行相对旋转
			block.transform.Translate(rotation_center);
			block.transform.Rotate(Vector3.right, x_angle);
			block.transform.Translate(-rotation_center);
		}
	}
}

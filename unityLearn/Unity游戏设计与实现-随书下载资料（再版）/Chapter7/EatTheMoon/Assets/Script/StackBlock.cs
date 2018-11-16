using UnityEngine;
using System.Collections;

// 下方堆积的方块
public class StackBlock : Block {

	public StackBlockControl	stack_control = null;

	// 状态
	public enum STEP {
		
		NONE = -1,
	
		IDLE = 0,		// 一般
		VANISHING,		// 正在消除的动画（颜色变化）
		VACANT,			// 消除后（因为连锁而消除，变作灰色）
		FALL,			// 落下过程中

		NUM,
	};
	
	public STEP		step;
	public STEP		next_step = STEP.NONE;
	public float	step_timer;

	// 网格上的位置
	public struct PlaceIndex {

		public int		x;
		public int		y;
	};

	public PlaceIndex	place;
	public Vector3		position_offset;
	public Vector3		velocity;

	public RotateAction		swap_action;							// 控制上下替换时的动作
	public RotateAction		color_change_action;					// 控制颜色变换时的动作

	public static float		FALL_START_HEIGHT = 6.5f;

	public static float		OFFSET_REVERT_SPEED = 0.1f*60.0f;		// 偏移向0变化的速度

	public bool		shake_is_active;
	public float	shake_timer;
	public Vector3	shake_offset;

	// ---------------------------------------------------------------- //


	void 	Start()
	{
		this.setColorType(this.color_type);

		this.position_offset = Vector3.zero;

		// 旋转动作的初始化

		this.swap_action.init();
		this.color_change_action.init();

		this.shake_is_active = false;
	}

	// 从from_block 拷贝颜色和位置等信息
	public void	relayFrom(StackBlock from_block)
	{
		this.setColorType(from_block.color_type);

		this.step        = from_block.step;
		this.next_step   = from_block.next_step;
		this.step_timer  = from_block.step_timer;
		this.swap_action = from_block.swap_action;
		this.color_change_action = from_block.color_change_action;

		this.velocity = from_block.velocity;

		// 为了使其全局的位置不发生变化，计算偏移植
		this.position_offset = StackBlockControl.calcIndexedPosition(from_block.place) + from_block.position_offset - StackBlockControl.calcIndexedPosition(this.place);

		//this.position_offset = from_block.transform.position - StackBlockControl.calcIndexedPosition(this.place);
		// 如果按上面这样做，旋转的中心就会受到偏移的影响，这样是不对的
	}

	// 开始方块的上下替换
	static public void		beginSwapAction(StackBlock upper_block, StackBlock under_block)
	{
		Block.COLOR_TYPE	upper_color;
		StackBlock.STEP		upper_step;
		RotateAction		upper_color_change;

		upper_color        = upper_block.color_type;
		upper_step         = upper_block.step;
		upper_color_change = upper_block.color_change_action;

		upper_block.setColorType(under_block.color_type);
		upper_block.step                = under_block.step;
		upper_block.color_change_action = under_block.color_change_action;

		under_block.setColorType(upper_color);
		under_block.step                = upper_step;
		under_block.color_change_action = upper_color_change;

		// 开始旋转
		upper_block.swap_action.start(RotateAction.TYPE.SWAP_UP);
		under_block.swap_action.start(RotateAction.TYPE.SWAP_DOWN);
	}

	void 	Update()
	{
		this.step_timer += Time.deltaTime;

		const float	vanish_time = 1.0f;

		// -------------------------------------------- //
		// 检测是否迁移到下一状态

		switch(this.step) {

			case STEP.VANISHING:
			{
				if(this.step_timer > vanish_time) {

					this.next_step = STEP.VACANT;
				}
			}
			break;

			case STEP.FALL:
			{
				// 落地后结束
				if(this.position_offset.y == 0.0f) {

					this.next_step = STEP.IDLE;
				}
			}
			break;
		}

		// -------------------------------------------- //
		// 状态迁移时的初始化

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
	
				case STEP.VACANT:
				{
					this.setColorType(COLOR_TYPE.GRAY);
				}
				break;

				case STEP.FALL:
				{
					this.velocity = Vector3.zero;
				}
				break;

				case STEP.VANISHING:
				{
					this.shake_start();

					this.stack_control.scene_control.vanish_fx_control.createEffect(this);
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// -------------------------------------------- //
		// 各个状态的执行处理

		switch(this.step) {

			case STEP.VANISHING:
			{
				// 方块的颜色按照
				//
				// 原来的颜色→红→灰色
				//
				// 的顺序变化

				float	rate;

				if(this.step_timer < vanish_time*0.1f) {

					rate = this.step_timer/(vanish_time*0.1f);

				} else if(this.step_timer < vanish_time*0.3f) {

					rate = 1.0f;

				} else if(this.step_timer < vanish_time*0.6f) {

					this.setColorType(COLOR_TYPE.RED);

					rate = (this.step_timer - vanish_time*0.3f)/(vanish_time*0.3f);

				} else {

					rate = 1.0f;
				}

				this.GetComponent<Renderer>().material.SetFloat("_BlendRate", rate);
			}
			break;

		}

		// -------------------------------------------------------------------------------- //
		// 网格上的位置（一般是固定的），旋转时初始化

		this.transform.position = StackBlockControl.calcIndexedPosition(this.place);
		this.transform.rotation = Quaternion.identity;

		// -------------------------------------------- //
		// 滑动（偏移植的补间）

		if(this.step == STEP.FALL) {

			this.velocity.y += -9.8f*Time.deltaTime;

			this.position_offset.y += this.velocity.y*Time.deltaTime;

			if(this.position_offset.y < 0.0f) {

				this.position_offset.y = 0.0f;
			}

			// 为了避免超过下方的方块
			// (处理的顺序不局限于从下到上，不够严谨)
			//
			if(this.place.y < StackBlockControl.BLOCK_NUM_Y - 1) {

				StackBlock	under = this.stack_control.blocks[this.place.x, this.place.y + 1];

				if(this.position_offset.y < under.position_offset.y) {

					this.position_offset.y = under.position_offset.y;
					this.velocity.y        = under.velocity.y;
				}
			}

		} else {

			float	position_offset_prev = this.position_offset.y;

			if(Mathf.Abs(this.position_offset.y) < 0.1f) {

				// 当偏移值足够小就结束
	
				this.position_offset.y = 0.0f;
	
			} else {

				if(this.position_offset.y > 0.0f) {
	
					this.position_offset.y -=  OFFSET_REVERT_SPEED*Time.deltaTime;
					this.position_offset.y = Mathf.Max(0.0f, this.position_offset.y);
	
				} else {
	
					this.position_offset.y -= -OFFSET_REVERT_SPEED*Time.deltaTime;
					this.position_offset.y = Mathf.Min(0.0f, this.position_offset.y);
				}
			}

			// 为了执行上方落下的方块停止时的处理，提前计算出速度
			this.velocity.y = (this.position_offset.y - position_offset_prev)/Time.deltaTime;
		}

		this.transform.Translate(this.position_offset);

		// -------------------------------------------- //
		// 交换动作

		this.swap_action.execute(this);

		// 蛋糕不会旋转
		if(this.isCakeBlock()) {

			this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 0.0f);

			this.transform.rotation = Quaternion.identity;
		}

		// -------------------------------------------- //
		// 改变颜色

		this.color_change_action.execute(this);

		if(this.color_change_action.is_active) {

			// 旋转到一半的时候改变颜色

			if(this.color_change_action.rate > 0.5f) {

				this.setColorType(this.color_change_action.target_color);
			}
		}

		// -------------------------------------------- //
		// 方块消失时的振动

		this.shake_execute();
	}
#if false
	// 鼠标被按下时
	// （使用的时候，请将StackBlockPrefab，BoxCollider设置为有效）
	void 	OnMouseDown()
	{
		// 点击后颜色改变（用于调试）

		if(this.step == STEP.IDLE) {

			/*COLOR_TYPE	color = this.color_type;

			color = (COLOR_TYPE)(((int)color + 1)%Block.NORMAL_COLOR_NUM);

			this.setColorType(color);*/
			/*if(this.color_type == Block.COLOR_TYPE.PINK) {

				this.setColorType(Block.COLOR_TYPE.CYAN);

			} else {

				this.setColorType(Block.COLOR_TYPE.PINK);
			}*/
			this.stack_control.block_feeder.cake.x = this.place.x;
			this.stack_control.block_feeder.cake.is_enable = true;
			this.setColorType(Block.COLOR_TYPE.CAKE0);
		}
	}
#endif
	// 普通的情况
	public void beginIdle(Block.COLOR_TYPE color_type)
	{
		this.setColorType(color_type);
		this.next_step = STEP.IDLE;
	}

	// 开始消除
	public void	beginVanishAction()
	{
		this.next_step = STEP.VANISHING;
	}

	// 开始下落处理
	public void	beginFallAction(Block.COLOR_TYPE color_type)
	{
		this.setColorType(color_type);
		this.setVisible(true);

		this.position_offset.y = FALL_START_HEIGHT;
		this.velocity = Vector3.zero;

		this.next_step = STEP.FALL;
	}

	// 开始改变颜色
	public void	beginColorChangeAction(Block.COLOR_TYPE	color_type)
	{
		this.color_change_action.target_color = color_type;
		this.color_change_action.start(RotateAction.TYPE.COLOR_CHANGE);
	}

	// 是否能执行连锁？
	public bool isConnectable()
	{
		bool	ret;

		ret = false;

		if(this.step == STEP.IDLE || this.next_step == STEP.IDLE) {

			ret = true;
		}

		if(this.color_type < Block.NORMAL_COLOR_FIRST || Block.NORMAL_COLOR_LAST < this.color_type) {

			ret = false;
		}

		// 最下方的部分不在连锁检测的范围内（因为在画面之外）
		if(this.place.y >= StackBlockControl.BLOCK_NUM_Y - 1) {

			ret = false;
		}

		return(ret);
	}

	// 是否为空置的方块？（被连锁消除后）
	public bool isVacant()
	{
		bool	ret;

		do {

			ret = true;

			//

			if((this.step == STEP.VACANT || this.next_step == STEP.VACANT)) {

				break;
			}

			//

			ret = false;

		} while(false);

		return(ret);
	}

	// 是否为空置的方块？（被连锁消除后）
	public bool isCarriable()
	{
		bool	ret;

		do {

			ret = false;

			//

			if((this.step == STEP.VANISHING || this.next_step == STEP.VANISHING)) {

				break;
			}
			if((this.step == STEP.VACANT || this.next_step == STEP.VACANT)) {

				break;
			}

			//

			ret = true;

		} while(false);

		return(ret);
	}

	// 正在交换过程中？
	public bool isNowSwapAction()
	{
		bool	ret = false;

		ret = this.swap_action.is_active;

		return(ret);
	}

	// 落下过程中？
	public bool isNowFallAction()
	{
		bool	ret = (this.step == STEP.FALL || this.next_step == STEP.FALL);

		return(ret);
	}

	// 消除后？
	public bool	isVanishAfter()
	{
		bool	ret;

		do {

			ret = true;

			//

			if((this.step == STEP.VANISHING || this.next_step == STEP.VANISHING)) {

				break;
			}
			if((this.step == STEP.VACANT || this.next_step == STEP.VACANT)) {

				break;
			}

			//

			ret = false;

		} while(false);


		return(ret);
	}
	// 设置为未使用状态
	public void	setUnused()
	{
		this.setColorType(Block.COLOR_TYPE.NONE);
		this.setVisible(false);
	}

	// ---------------------------------------------------------------- //

	// 开始振动
	private void	shake_start()
	{
		this.shake_is_active = true;
		this.shake_timer = 0.0f;
	}

	// 振动控制
	private void	shake_execute()
	{
		if(this.shake_is_active) {

			float	shake_time = 0.5f;

			float	t = this.shake_timer/shake_time;


			//

			float	amplitude = 0.05f*Mathf.Lerp(1.0f, 0.0f, t);

			// 为了避免相邻的方块出现同样的动作行为，
			// 对周期做适当的偏移

			float	t_offset = (float)((this.place.y*StackBlockControl.BLOCK_NUM_X + this.place.x)%(StackBlockControl.BLOCK_NUM_X - 1));

			t_offset /= (float)(StackBlockControl.BLOCK_NUM_X - 2);

			this.shake_offset.x = amplitude*Mathf.Cos(10.0f*(t + t_offset)*2.0f*Mathf.PI);

			//

			Vector3	p = this.transform.position;

			p.x += this.shake_offset.x;

			this.transform.position = p;

			//

			this.shake_timer += Time.deltaTime;

			if(this.shake_timer >= shake_time) {

				this.shake_is_active = false;
			}
		}
	}

}

using UnityEngine;
using System.Collections;

public class CarryBlock : Block {

	public Vector3		position_offset;

	public PlayerControl	player = null;

	// 放下方块时的位置
	public StackBlock.PlaceIndex	place;

	public StackBlock.PlaceIndex	org_place;

	public enum STEP {

		NONE = -1,

		HIDE = 0,				// 不显示
		CARRY_UP,				// 举起中（移动中）
		CARRY,					// 举起中（移动结束）
		DROP_DOWN,				// 放下过程中

		NUM,
	};

	public STEP		step       = STEP.NONE;
	public STEP		next_step  = STEP.NONE;

	public float	step_timer = 0.0f;

	// ---------------------------------------------------------------- //

	public bool	isMoving()
	{
		bool	ret = false;

		switch(this.step) {

			case STEP.CARRY_UP:
			case STEP.DROP_DOWN:
			{
				ret = true;
			}
			break;
		}

		return(ret);
	}

	void 	Start()
	{
		this.position_offset = Vector3.zero;

		this.next_step = STEP.HIDE;
	}
	
	void 	Update()
	{
		this.step_timer += Time.deltaTime;

		// 检测状态迁移

		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.CARRY_UP:
				{
					if(this.position_offset.y == 0.0f) {
	
						this.next_step = STEP.CARRY;
					}
				}
				break;
	
				case STEP.DROP_DOWN:
				{
					if(this.position_offset.y == 0.0f) {
	
						this.player.scene_control.stack_control.endDropBlockAction(this.place.x);
	
						this.next_step = STEP.HIDE;
					}
				}
				break;
			}
		}

		// 状态迁移时的初始化

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.HIDE:
				{
					this.GetComponent<Renderer>().enabled = false;
				}
				break;

				case STEP.CARRY_UP:
				{
					// 从隐藏状态开始时，算出现在的位置
					if(this.step == STEP.HIDE) {

						this.transform.position = StackBlockControl.calcIndexedPosition(this.place);
					}

					Vector3	base_position = this.player.transform.position;

					base_position.y += Block.SIZE_Y;

					this.position_offset = this.transform.position - base_position;
			
					this.setVisible(true);
				}
				break;

				case STEP.DROP_DOWN:
				{
					Vector3	base_position = StackBlockControl.calcIndexedPosition(this.place);

					this.position_offset = this.transform.position - base_position;
				}
				break;
			}

			this.step = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// 各个状态的执行

		Vector3		position = this.transform.position;

		switch(this.step) {

			case STEP.CARRY:
			case STEP.CARRY_UP:
			{
				position.x = this.player.transform.position.x;
				position.y = this.player.transform.position.y + Block.SIZE_Y;
				position.z = 0.0f;
			}
			break;

			case STEP.DROP_DOWN:
			{
				position = StackBlockControl.calcIndexedPosition(this.place);
			}
			break;
		}

		// 对偏移值进行补间

		if(Mathf.Abs(this.position_offset.y) < 0.1f) {

			this.position_offset.y = 0.0f;

		} else {

			const float		speed = 0.2f;

			if(this.position_offset.y > 0.0f) {

				this.position_offset.y -=  speed*(60.0f*Time.deltaTime);

				this.position_offset.y = Mathf.Max(this.position_offset.y, 0.0f);

			} else {

				this.position_offset.y -= -speed*(60.0f*Time.deltaTime);

				this.position_offset.y = Mathf.Min(this.position_offset.y, 0.0f);
			}
		}

		position.y += this.position_offset.y;

		this.transform.position = position;
	}

	// 开始举起的动作
	public void		startCarry(int place_index_x)
	{
		// 放下过程中再举起方块时，执行和落地时一样的处理。
		// 如果不这样的话，最上部分的方块将一直保持隐藏状态
		// （因为在放下过程中直到手里的方块落地之前，最上部分的方块将保持隐藏）
		if(this.step == STEP.DROP_DOWN) {

			this.player.scene_control.stack_control.endDropBlockAction(this.place.x);
		}

		this.place.x = place_index_x;
		this.place.y = StackBlockControl.GROUND_LINE;

		this.org_place = this.place;

		this.next_step = STEP.CARRY_UP;
	}

	// 开始放下
	public void		startDrop(int place_index_x)
	{
		this.place.x = place_index_x;
		this.place.y = StackBlockControl.GROUND_LINE;

		this.next_step = STEP.DROP_DOWN;
	}

	// 隐藏
	// （吃下蛋糕后）
	public void		startHide()
	{
		this.next_step = STEP.HIDE;
	}
}

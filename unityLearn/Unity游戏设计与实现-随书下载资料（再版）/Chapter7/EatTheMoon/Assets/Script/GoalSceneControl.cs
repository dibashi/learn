using UnityEngine;
using System.Collections;

public class GoalSceneControl {

	public SceneControl	scene_control = null;

	public static string	PAT0 =	  ".....*..."
									+ "..*****.."
									+ "....**..."
									+ "...*.*..."
									+ "..*..*...";
	 
	public static string	PAT1 =	  "..*...*.."
									+ "...*.*..."
									+ "....*...."
									+ "...*.**.."
									+ ".**......";

	public static string	PAT2 =	  ".*****.*."
									+ "........*"
									+ ".*****..."
									+ "....*...."
									+ ".***.....";

	public static string	PAT3 =	  "...*....."
									+ "...*....."
									+ "...**...."
									+ "...*.*..."
									+ "...*.....";

//	public static string	PAT4 =	  "...*....."
//									+ ".******.."
//									+ ".*....*.."
//									+ ".....*..."
//									+ "...**....";

	public static string	PAT4 =	  "........."
									+ "..**....."
									+ ".*..*..*."
									+ ".....**.."
									+ ".........";

	public static string	PAT5 =	  "..**.**.."
									+ ".*******."
									+ ".*******."
									+ "..*****.."
									+ "....*....";
/*	public static string	PAT5 =	  ".**...**."
									+ "*..*.*..*"
									+ "........."
									+ "..*...*.."
									+ "...***...";
*/
	/*public static string	PAT5 =	  "...*.*..."
									+ "...*.*..."
									+ "...*.*..."
									+ "........."
									+ "...*.*...";*/

	public string[]	message;

	// ---------------------------------------------------------------- //

	private	int		pat_count;

	private int					color_change_count;
	private Block.COLOR_TYPE	message_color;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		START = 0,				// 开始
		WAIT_SWAP_END,			// 等待交换结束
		WAIT_SWAP_END_AFTER,	// 交换结束后的短暂时间内

		FALL,

		COLOR_CHANGE,

		MESSAGE,
		WAIT,

		NUM,
	};

	public STEP			step;
	public STEP			next_step = STEP.NONE;

	public float	step_timer_prev;
	public float	step_timer;

	public static int	PLAYER_EATING_POSITION = 4;

	public struct Wait {

		public float	duration;
		public STEP		next_step;

	};

	public Wait	wait;

	// ---------------------------------------------------------------- //

	public void		create()
	{
		this.step_timer_prev = 0.0f;
		this.step_timer = 0.0f;

		this.message = new string[6];
		this.message[0] = PAT0;
		this.message[1] = PAT1;
		this.message[2] = PAT2;
		this.message[3] = PAT3;
		this.message[4] = PAT4;
		this.message[5] = PAT5;

		this.pat_count = 0;

		this.message_color = Block.NORMAL_COLOR_FIRST;
	}

	public void		start()
	{
		StackBlockControl	stack_control = this.scene_control.stack_control;

		stack_control.is_connect_check_enable = false;

		GUIControl.get().setDispGoal(true);

		this.step      = STEP.NONE;
		this.next_step = STEP.START;
	}

	public void		execute()
	{
		StackBlockControl	stack_control = this.scene_control.stack_control;
		PlayerControl		player        = this.scene_control.player_control;
		BGControl			bg            = this.scene_control.bg_control;

		this.step_timer_prev = this.step_timer;
		this.step_timer += Time.deltaTime;

		// -------------------------------------------------------- //
		// 检测是否移到下一个状态

		switch(this.step) {

			case STEP.START:
			{
				this.next_step = STEP.WAIT_SWAP_END;
			}
			break;

			case STEP.WAIT_SWAP_END:
			{

				bool	is_has_moving_block = false;
		
				foreach(StackBlock block in stack_control.blocks) {
		
					if(block.step != StackBlock.STEP.IDLE || block.next_step != StackBlock.STEP.NONE) {
		
						is_has_moving_block = true;
						break;
					}

					if(block.position_offset.y != 0.0f) {

						is_has_moving_block = true;
						break;
					}
				}

				if(!is_has_moving_block) {

					this.next_step = STEP.WAIT_SWAP_END_AFTER;
				}
			}
			break;

			case STEP.WAIT_SWAP_END_AFTER:
			{
				if(this.step_timer > 1.0f) {

					this.next_step = STEP.FALL;
				}
			}
			break;

			case STEP.FALL:
			{
				bool	is_has_fall_block = false;
	
				for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {
	
					StackBlock	block = stack_control.blocks[x, StackBlockControl.GROUND_LINE - 1];
	
					if(block.isNowFallAction()) {
	
						is_has_fall_block = true;
						break;
					}
				}

				if(!is_has_fall_block) {

					this.next_step = STEP.COLOR_CHANGE;
				}
			}
			break;

			case STEP.COLOR_CHANGE:
			{
				if(this.step_timer > 1.0f && player.lx == PLAYER_EATING_POSITION) {

					this.begin_wait_step(1.0f, STEP.MESSAGE);
				}
			}
			break;

			case STEP.WAIT:
			{
				if(this.step_timer > this.wait.duration) {

					this.next_step = this.wait.next_step;
				}
			}
			break;
		}

		// -------------------------------------------------------- //
		// 状态迁移时的初始化

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
	
				case STEP.START:
				{
					player.setControlable(false);

				}
				break;

				case STEP.FALL:
				{
					player.dropBlock();

					bg.setHeightRateDirect(1.0f);

					stack_control.is_scroll_enable = false;
					stack_control.fall_request = 0;
					stack_control.blockFallRequest();

					for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {

						stack_control.blocks[x, StackBlockControl.GROUND_LINE - 1].beginColorChangeAction(Block.COLOR_TYPE.CYAN);
					}

					//

					this.pat_count = 0;
				}
				break;

				case STEP.COLOR_CHANGE:
				{
					this.color_change_count = 0;

					player.SetHeight(-1);
				}
				break;

				case STEP.MESSAGE:
				{
					this.message_color = Block.NORMAL_COLOR_FIRST;

					player.beginGoalAct();

					scene_control.playSe(SceneControl.SE.CLEAR);
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer_prev = -1.0f;
			this.step_timer      = 0.0f;
		}

		// -------------------------------------------------------- //
		// 各个状态的执行处理

		switch(this.step) {

			case STEP.COLOR_CHANGE:
			{
				if(this.color_change_count < StackBlockControl.BLOCK_NUM_Y - StackBlockControl.GROUND_LINE) {

					float	delay = 0.05f;

					if(Mathf.FloorToInt(this.step_timer_prev/delay) < Mathf.FloorToInt(this.step_timer/delay)) {
	
						int	y = StackBlockControl.GROUND_LINE + this.color_change_count;
	
						for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {
	
							stack_control.blocks[x, y].beginColorChangeAction(Block.COLOR_TYPE.CYAN);
						}

						this.color_change_count++;
					}
				}

				if(player.lx != PLAYER_EATING_POSITION) {

					float	delay = 0.5f;

					if(this.step_timer > delay*2.0f) {

						if(Mathf.FloorToInt(this.step_timer_prev/delay) < Mathf.FloorToInt(this.step_timer/delay)) {
	
							if(player.lx > PLAYER_EATING_POSITION) {
	
								player.SetLinedPosition(player.lx - 1);
	
							} else {
	
								player.SetLinedPosition(player.lx + 1);
							}
						}
					}
				}
			}
			break;

			case STEP.MESSAGE:
			{
				float	duration = 2.0f;

				if(this.step_timer >= duration) {

					if(Mathf.FloorToInt(this.step_timer_prev/duration) < Mathf.FloorToInt(this.step_timer/duration)) {
	
						do {
	
							this.message_color = Block.getNextNormalColor(this.message_color);
	
						} while(this.message_color == Block.COLOR_TYPE.CYAN);
	
						this.apply_pattern_to_block(this.message[this.pat_count], this.message_color, Block.COLOR_TYPE.CYAN);
	
						this.pat_count = (this.pat_count + 1)%this.message.Length;
					}
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //

	}

	public void		begin_wait_step(float duration, STEP next_step)
	{
		this.wait.duration  = duration;
		this.wait.next_step = next_step;

		this.next_step = STEP.WAIT;
	}
	public void		apply_pattern_to_block(string pat, Block.COLOR_TYPE fore_color, Block.COLOR_TYPE back_color)
	{
		StackBlockControl	stack_control = this.scene_control.stack_control;

		const int	pat_h = 5;

		int	y0 = StackBlockControl.GROUND_LINE;

		Block.COLOR_TYPE	color;

		for(int y = StackBlockControl.GROUND_LINE;y < StackBlockControl.BLOCK_NUM_Y;y++) {

			for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {

				color = back_color;

				if(y0 <= y && y < y0 + pat_h) {

					if(pat[(y - y0)*StackBlockControl.BLOCK_NUM_X + x] == '*') {

						color = fore_color;
					}
				}

				stack_control.blocks[x, y].beginColorChangeAction(color);
			}
		}
	}
}

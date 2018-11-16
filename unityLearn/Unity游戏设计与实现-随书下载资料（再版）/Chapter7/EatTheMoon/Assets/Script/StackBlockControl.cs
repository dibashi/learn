using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 对堆积的方块进行整体控制
public class StackBlockControl {

	public GameObject	StackBlockPrefab = null;

	// 落下的方块列数
	public static int	FALL_LINE_NUM = 3;
	public static int	BLOCK_NUM_X = 9;
	public static int	BLOCK_NUM_Y = 7 + FALL_LINE_NUM;

	// 地面的方块是第几列？
	//
	// 第０至第GROUND_LINE + 1 列还在空中
	// 第GROUND_LINE 至第BLOCK_NUM_Y - 1 列在地面下
	public static int	GROUND_LINE = FALL_LINE_NUM;

	public StackBlock[,]	blocks;

	public	ConnectChecker	connect_checker = null;		// 连锁检测
	public	BlockFeeder		block_feeder = null;		// 决定下次出现的方块颜色
	public 	SceneControl	scene_control = null;

	private bool	is_has_swap_block = false;			// 是否有方块正在进行替换动作？
	private bool	is_swap_end_frame = false;			// 只有当替换动作结束的瞬间为 true.

	public int		fall_request = 0;					// 期待落下的行数（累计的请求数量）
	private int		fall_count = 0;						// 落下过程中的列的数量

	public bool[]	is_color_enable = null;				// 各种颜色的方块是否出现？

	public bool		is_scroll_enable = true;
	public bool		is_connect_check_enable = true;

	// ---------------------------------------------------------------- //

	public struct Combo {

		public bool	is_now_combo;

		// 连锁的次数
		public int	combo_count_last;		// 上一次
		public int	combo_count_current;	// 当前（连锁中）

	};

	public Combo combo;

	public int		eliminate_count;		// 消除了的方块数
	public int		eliminate_to_fall;
	public int		eliminate_to_cake;		// 直到蛋糕出现还需要消除的方块个数

	public const int	ELIMINATE_TO_FALL_INIT = 5;
	public const int	ELIMINATE_TO_CAKE_INIT = 5;
	public const int	ELIMINATE_TO_CAKE_INIT_2ND = 25;
	
	// ---------------------------------------------------------------- //

	public int		score = 0;
	public int		continuous_connect_num = 0;

	// 颜色改变时，改变后的颜色
	private int	change_color_index0 = -1;
	private int	change_color_index1 = -1;

	// ---------------------------------------------------------------- //

	public void	create()
	{
		//

		this.blocks = new StackBlock[BLOCK_NUM_X, BLOCK_NUM_Y];

		for(int y = 0;y < BLOCK_NUM_Y;y++) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				GameObject	game_object = GameObject.Instantiate(this.StackBlockPrefab) as GameObject;

				StackBlock	block = game_object.GetComponent<StackBlock>();

				block.place.x = x;
				block.place.y = y;

				this.blocks[x, y] = block;

				block.setUnused();

				block.stack_control = this;
			}
		}

		//

		this.is_color_enable = new bool[Block.NORMAL_COLOR_NUM];

		for(int i = 0;i < this.is_color_enable.Length;i++) {

			this.is_color_enable[i] = true;
		}

		// 屏蔽粉色
		this.is_color_enable[(int)Block.COLOR_TYPE.PINK] = false;

		//

		this.connect_checker = new ConnectChecker();

		this.connect_checker.stack_control = this;
		this.connect_checker.blocks = this.blocks;
		this.connect_checker.create();

		this.block_feeder = new BlockFeeder();
		this.block_feeder.control = this;
		this.block_feeder.create();

		//

		this.setColorToAllBlock();

		//

		this.combo.is_now_combo        = false;
		this.combo.combo_count_last    = 0;
		this.combo.combo_count_current = 0;

		this.eliminate_count = 0;
		this.eliminate_to_fall = ELIMINATE_TO_FALL_INIT;
		this.eliminate_to_cake = ELIMINATE_TO_CAKE_INIT;


		this.is_scroll_enable = true;
		this.is_connect_check_enable = true;
	}

	// 选择所有的方块颜色
	public void		setColorToAllBlock()
	{
		// places ⋯⋯ 该数组用于存储选择了某种颜色的方块顺序
		//
		// place[0] 最开始选择了颜色的方块（的位置）
		// place[1] 第2个选择了颜色的方块
		// place[2] 第3个选择了颜色的方块
		//            :
		//

		List<StackBlock.PlaceIndex>	places = new List<StackBlock.PlaceIndex>();

		// 暂时从左上开始按顺序排列

		for(int y = GROUND_LINE;y < BLOCK_NUM_Y;y++) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				StackBlock.PlaceIndex	place;

				place.x = x;
				place.y = y;

				places.Add(place);
			}
		}
	#if true
		// 随机打乱顺序
		// 如果将这里注释掉，开始时方块将
		// 从左上开始按顺序排列
		for(int i = 0;i < places.Count - 1;i++) {

			int j = Random.Range(i + 1, places.Count);

			StackBlock.PlaceIndex	tmp = places[i];

			places[i] = places[j];
			places[j] = tmp;
		}
		// 书中的代码是这一段
		/*for(int i = 0;i < places.size() - 1;i++) {

			int j = Random.Range(i + 1, places.size());

			places.swap(i, j);
		}*/
	#endif
		this.block_feeder.connect_arrow_num = 1;

		foreach(StackBlock.PlaceIndex place in places) {

			StackBlock	block = this.blocks[place.x, place.y];

			block.setColorType(this.block_feeder.getNextColorStart(place.x, place.y));
			block.setVisible(true);
		}
	}

	public void		update()
	{

		this.is_swap_end_frame = false;

		// ---------------------------------------------------------------- //
	#if false
		// 按下“0”后，从上方落下方块
		if(Input.GetKeyDown(KeyCode.Keypad0)) {

			this.blockFallRequest();
		}


		// 按下“1”后，颜色改变
		if(Input.GetKeyDown(KeyCode.Keypad1)) {

			this.startColorChange();
		}

		// 按下“2”后，重新开始
		if(Input.GetKeyDown(KeyCode.Keypad2)) {

			for(int y = 0;y < BLOCK_NUM_Y;y++) {
	
				for(int x = 0;x < BLOCK_NUM_X;x++) {

					StackBlock	block = this.blocks[x, y];

					block.setUnused();					
				}
			}

			this.setColorToAllBlock();
		}

		// 按下“8”后，放大招
		if(Input.GetKeyDown(KeyCode.Keypad8)) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {
	
				for(int y = GROUND_LINE - this.fall_count;y < BLOCK_NUM_Y;y++) {

					StackBlock	block = this.blocks[x, y];

					block.beginVanishAction();
				}
			}
		}

		if(Input.GetKeyDown(KeyCode.Keypad9)) {

			//this.CheckConnect();
			//this.block_feeder.beginFeeding();
			
			for(int	x = 0;x < BLOCK_NUM_X;x++) {

				StackBlock	block = this.blocks[x, BLOCK_NUM_Y - 1];

				block.beginIdle(this.block_feeder.getNextColorAppearFromBottom(x));
			}
		}
	#endif

		// ---------------------------------------------------------------- //
		// 如果有空的方块（连锁后变成灰色），开始交换

		for(int x = 0;x < BLOCK_NUM_X;x++) {

			for(int y = GROUND_LINE;y < BLOCK_NUM_Y - 1;y++) {

				StackBlock	upper_block = this.blocks[x, y];
				StackBlock	under_block = this.blocks[x, y + 1];

				do {

					if(!(upper_block.isVacant() && !under_block.isVacant())) {

						break;
					}

					if(upper_block.isNowSwapAction() || under_block.isNowSwapAction()) {

						break;
					}

					//

					StackBlock.beginSwapAction(upper_block, under_block);

					this.scene_control.playSe(SceneControl.SE.SWAP);

				} while(false);

			}
		}

		// ---------------------------------------------------------------- //
		// 如果空的方块到达最下面一列，设置为新的颜色

		for(int x = 0;x < BLOCK_NUM_X;x++) {

			StackBlock	block = this.blocks[x, BLOCK_NUM_Y - 1];

			if(!block.isVacant()) {

				continue;
			}

			if(block.isNowSwapAction()) {

				continue;
			}

			block.beginIdle(this.block_feeder.getNextColorAppearFromBottom(x));
		}


		// ---------------------------------------------------------------- //
		// 检测结束交换瞬间的状态

		// 由于要对“结束的瞬间”进行处理，提前保存前一帧的结果

		bool	is_has_swap_block_prev = this.is_has_swap_block;

		this.is_has_swap_block = false;

		foreach(StackBlock block in this.blocks) {

			if(block.isVanishAfter()) {

				this.is_has_swap_block = true;
				break;
			}
		}

		if(is_has_swap_block_prev && !this.is_has_swap_block) {

			this.is_swap_end_frame = true;
		}

		// ---------------------------------------------------------------- //
		// 方块从上方掉落下来

		do {

			if(this.fall_request <= 0) {

				break;
			}

			if(this.fall_count >= FALL_LINE_NUM) {

				break;
			}

			int		y = GROUND_LINE - 1 - this.fall_count;

			Block.COLOR_TYPE[] colors = this.block_feeder.getNextColorsAppearFromTop(y);

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				StackBlock	block = this.blocks[x, y];

				block.beginFallAction(colors[x]);
			}

			this.fall_count++;
			this.fall_request--;

		} while(false);

		// ---------------------------------------------------------------- //
		// 完全落下后，全体滚动

		this.scroll_control();

		// ---------------------------------------------------------------- //
		// 检测连锁

		if(this.is_swap_end_frame) {
	
			this.CheckConnect();
		}

		// ---------------------------------------------------------------- //
		// 方块从蛋糕上方落下时，蛋糕将往上运动
		// （贴心设计）

		if(this.block_feeder.cake.is_enable) {

			for(int y = StackBlockControl.GROUND_LINE + 1;y < StackBlockControl.BLOCK_NUM_Y;y++) {

				int	x = this.block_feeder.cake.x;

				if(!this.blocks[x, y].isCakeBlock()) {

					continue;
				}

				// 放下的方块在落地前，最上方的方块是不显示的
				// 这个不显示的时间短内忽略跳过
				if(!this.blocks[x, y - 1].isVisible()) {

					continue;
				}

				// 连锁后的方块即时残留着也会被交换
				// （具体来说，因为VanishAction过程中颜色会改变所以必须忽略）
				if(this.blocks[x, y - 1].isVanishAfter()) {

					continue;
				}

				//

				StackBlock.beginSwapAction(this.blocks[x, y - 1], this.blocks[x, y]);
			}
		}
	}

	// 方块落下结束时的滚动控制
	private void	scroll_control()
	{
		do {

			if(this.fall_count <= 0) {

				break;
			}

			//

			bool	is_has_fall_block = false;

			for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {

				StackBlock	block = this.blocks[x, StackBlockControl.GROUND_LINE - 1];

				if(block.isNowFallAction()) {

					is_has_fall_block = true;
					break;
				}
			}

			if(!is_has_fall_block) {

				if(this.is_scroll_enable) {

					// 蛋糕滚动到区域外时的处理
					if(this.block_feeder.cake.is_enable) {

						StackBlock	block = this.blocks[this.block_feeder.cake.x, StackBlockControl.BLOCK_NUM_Y - 1];
	
						if(block.isCakeBlock()) {
	
							this.block_feeder.onDropBlock(block);
						}
					}

					// 中间的方块逐列往下偏移
					for(int y = StackBlockControl.BLOCK_NUM_Y - 1;y >= StackBlockControl.GROUND_LINE;y--) {
		
						for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {
		
							this.blocks[x, y].relayFrom(this.blocks[x, y - 1]);
						}
					}
	
					// 如果有多行方块落下，则逐行向下偏移
					if(this.fall_count >= 2) {
		
						for(int y = StackBlockControl.GROUND_LINE - 1;y > StackBlockControl.GROUND_LINE - 1 - (this.fall_count - 1);y--) {
			
							for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {
			
								this.blocks[x, y].relayFrom(this.blocks[x, y - 1]);
							}
						}
					}
					
					// 如果有多行方块正在落下，最上面一行不显示
					for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {
		
						this.blocks[x, StackBlockControl.GROUND_LINE - 1 - (this.fall_count - 1)].setUnused();
					}
				}

				this.fall_count--;

				this.scene_control.heightGain();

				this.scene_control.playSe(SceneControl.SE.JUMP);
				this.scene_control.playSe(SceneControl.SE.LANDING);
			}

		} while(false);
	}

	// 检测连锁
	public bool	CheckConnect()
	{
		bool	ret = false;

		if(this.is_connect_check_enable) {

			ret = this.check_connect_sub();
		}

		return(ret);
	}

	private bool	check_connect_sub()
	{

		bool	is_connect = false;

		int		connect_num = 0;

		this.connect_checker.clearAll();

		for(int y = GROUND_LINE;y < StackBlockControl.BLOCK_NUM_Y;y++) {

			for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {

				if(!this.blocks[x, y].isConnectable()) {

					continue;
				}

				// 检测同种颜色方块的排列数量

				int connect_block_num = this.connect_checker.checkConnect(x, y);

				// 如果同种颜色方块的排列书小于4则不会消除

				if(connect_block_num < 4) {

					continue;
				}

				connect_num++;

				// 消除连结好的方块

				for(int i = 0;i < connect_block_num;i++) {

					StackBlock.PlaceIndex index = this.connect_checker.connect_block[i];

					this.blocks[index.x, index.y].beginVanishAction();
				}

				//

				this.eliminate_count += connect_block_num;
				is_connect = true;

				//

				this.continuous_connect_num++;
				this.score += this.continuous_connect_num*connect_block_num;
			}
		}

		//

		if(is_connect) {

			if(this.combo.is_now_combo) {

				this.combo.combo_count_current++;

				// 发生连锁后，从上方落下方块
				this.fall_request++;

				this.scene_control.playSe(SceneControl.SE.COMBO);

			} else {

				this.combo.is_now_combo = true;
				this.combo.combo_count_current  = 1;
			}

			this.scene_control.playSe(SceneControl.SE.DROP_CONNECT);

			// 消除的方块达到一定数量后，让蛋糕出现
			//
			do {

				// 蛋糕出现的过程中，或者等待出现的过程中计数器不减小
				if(this.block_feeder.isCakeAppeared()) {

					break;
				}
				if(this.block_feeder.isCakeRequested()) {

					break;
				}

				this.eliminate_to_cake -= connect_num;

				if(this.eliminate_to_cake <= 0) {

					this.block_feeder.requestCake();
					this.eliminate_to_cake = ELIMINATE_TO_CAKE_INIT_2ND;

					// 使方块马上开始落下
					if(this.fall_request == 0) {

						this.fall_request++;
					}
				}

			} while(false);

			this.eliminate_to_fall -= connect_num;

		} else {

			// 连锁结束

			// 即时未发生连锁，当消除的方块达到一定数量后方块也会从上方落下
			if(this.combo.combo_count_current > 1) {

				// 肯定发生了连锁（＝方块落下了），重置残留的个数

				this.eliminate_to_fall = ELIMINATE_TO_FALL_INIT;

			} else {

				// 未发生连锁


				// 消除的方块达到一定数量，新方块从上方落下

				if(this.eliminate_to_fall <= 0) {

					if(this.fall_request == 0) {

						this.fall_request++;
					}

					this.eliminate_to_fall = ELIMINATE_TO_FALL_INIT;
				}
			}

			this.combo.is_now_combo = false;
			this.combo.combo_count_last = this.combo.combo_count_current;
			this.combo.combo_count_current = 0;

			this.continuous_connect_num = 0;
		}

		// 达到了通关标准后，将不再落下方块
		if(SceneControl.MAX_HEIGHT_LEVEL - this.scene_control.height_level < this.fall_request) {

			this.fall_request = SceneControl.MAX_HEIGHT_LEVEL - this.scene_control.height_level;
		}

		return(is_connect);
	}

	// 举起方块时的处理
	public void	pickBlock(int x)
	{
		for(int y = GROUND_LINE;y < BLOCK_NUM_Y - 1;y++) {

			this.blocks[x, y].relayFrom(this.blocks[x, y + 1]);
		} 

		// 为保险起见，将最上方的方块显示设置为ON
		// （因为放下方块后有时候可能会变为不显示）
		this.blocks[x, GROUND_LINE].setVisible(true);

		// 使最下方产生新的方块
		this.blocks[x, BLOCK_NUM_Y - 1].setColorType(this.block_feeder.getNextColorAppearFromBottom(x));

		this.blocks[x, BLOCK_NUM_Y - 1].swap_action.init();
	}

	// 放下方块时的处理
	public void	dropBlock(int x, Block.COLOR_TYPE color, int org_x)
	{
		// 方块（由于玩家的操作）被挤到画面下方时的处理
		this.block_feeder.onDropBlock(this.blocks[x, BLOCK_NUM_Y - 1]);

		// 移动的过程中进行偏移
		for(int y = BLOCK_NUM_Y - 1;y > GROUND_LINE;y--) {

			this.blocks[x, y].relayFrom(this.blocks[x, y - 1]);
		} 

		// 将最上方的方块颜色设置为与放下的方块颜色相同
		this.blocks[x, GROUND_LINE].beginIdle(color);

		// 举起的方块在落地前，都不显示
		this.blocks[x, GROUND_LINE].setVisible(false);
		this.blocks[x, GROUND_LINE].swap_action.is_active = false;
		this.blocks[x, GROUND_LINE].color_change_action.is_active = false;
		this.blocks[x, GROUND_LINE].position_offset.y = this.blocks[x, GROUND_LINE + 1].position_offset.y;

		// 为从上方落下的方块在停止时的处理做准备，对速度进行适当初始化
		this.blocks[x, GROUND_LINE].velocity.y = -StackBlock.OFFSET_REVERT_SPEED;

		if(color == Block.COLOR_TYPE.RED) {

			this.blocks[x, GROUND_LINE].step = StackBlock.STEP.VACANT;
		}
	}

	// 举起的方块在落地时的处理
	//
	public void	endDropBlockAction(int x)
	{
		if(this.is_has_swap_block) {

			// 如果方块正在交换过程中则不会发生连锁

		} else {

			// 连锁检测
			this.CheckConnect();
		}

		// 将位于方块落下位置的的stack方块设置为显示ON
		// （因为被举起的方块不显示了）
		//
		this.blocks[x, GROUND_LINE].setVisible(true);

		this.scene_control.playSe(SceneControl.SE.DROP);
	}


	// 根据位置的索引求出坐标
	public static Vector3	calcIndexedPosition(StackBlock.PlaceIndex place)
	{
		Vector3		position;

		position.x = (-(BLOCK_NUM_X/2.0f - 0.5f) + place.x)*Block.SIZE_X;

		position.y = (-0.5f - (place.y - GROUND_LINE))*Block.SIZE_Y;

		position.z = 0.0f;

		return(position);
	}

	// 吃下蛋糕后的处理
	public void	onEatCake()
	{
		this.block_feeder.onEatCake();

		this.startColorChange();
	}

	// 改变方块的颜色（变为特定的颜色）
	// 吃下蛋糕后的效果
	public void	startColorChange()
	{
		int		color_index = 0;

		var		after_color = new Block.COLOR_TYPE[2];

		// ------------------------------------------------ //
		// 尽量旋转，并且随机旋转两种颜色

		List<int>	candidates = new List<int>();

		for(int i = 0;i < Block.NORMAL_COLOR_NUM;i++) {

			// 和上次颜色相同因此不能作为候选值
			if(i == this.change_color_index0 || i == this.change_color_index1) {

				continue;
			}
			if(!this.is_color_enable[i]) {

				continue;
			}

			//

			candidates.Add(i);
		}

		// 从0 到 N - 1 中随机选取两个不重复的数
		// 选择
		// color0 =  0 到 N - 2 范围内的随机数
		// color1 = color0 到 N - 1 范围内的随机数
		//

		this.change_color_index0 = Random.Range(0, candidates.Count - 1);
		this.change_color_index1 = Random.Range(this.change_color_index0 + 1, candidates.Count);

		this.change_color_index0 = candidates[this.change_color_index0];
		this.change_color_index1 = candidates[this.change_color_index1];

		// ------------------------------------------------ //
		// 改变方块的颜色

		after_color[0] = (Block.COLOR_TYPE)change_color_index0;
		after_color[1] = (Block.COLOR_TYPE)change_color_index1;

		for(int x = 0;x < BLOCK_NUM_X;x++) {

			for(int y = GROUND_LINE - this.fall_count;y < BLOCK_NUM_Y;y++) {

				StackBlock	block = this.blocks[x, y];

				if(block.isVacant()) {

					continue;
				}

				// 如果最开始的颜色和变更后颜色数组中存在相同值则忽略
				if(System.Array.Exists(after_color, c => c == block.color_type)) {

					continue;
				}

				if(!block.isNormalColorBlock()) {

					continue;
				}

				// 开始改变颜色

				Block.COLOR_TYPE	target_color;

				target_color = after_color[color_index%after_color.Length];

				block.beginColorChangeAction(target_color);

				color_index++;
			}
		}
	}

	// 从上方掉落方块（得分动作）
	public void		blockFallRequest()
	{
		this.fall_request++;

		//this.is_fall_request = true;
	}
	
	// 获取第N个有效颜色
	public	Block.COLOR_TYPE	getNthEnableColor(int n)
	{
		Block.COLOR_TYPE	color = Block.COLOR_TYPE.NONE;

		int		sum = 0;

		for(int i = 0;i < (int)Block.COLOR_TYPE.NUM;i++) {

			if(!this.is_color_enable[i]) {

				continue;
			}

			if(sum == n) {

				color = (Block.COLOR_TYPE)i;
				break;
			}

			sum++;
		}

		return(color);
	}

}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 决定下一次出现方块的颜色
public class BlockFeeder {

	public StackBlockControl	control = null;

	protected List<int>					connect_num = null;
	protected List<Block.COLOR_TYPE>	candidates  = null;		// 出现颜色的候选值

	// 将要出现的蛋糕
	public struct Cake {

		public bool				is_enable;
		public int				x;
		public Block.COLOR_TYPE	color_type;		// 颜色种类（目前有两种蛋糕）
	};

	public Cake			cake;				// 将要出现的蛋糕
	protected int		cake_count = 0;		// 出现的蛋糕数量
	protected int		cake_request = 0;

	// ---------------------------------------------------------------- //

	public void	create()
	{
		this.connect_num = new List<int>();

		for(int i = 0;i < Block.NORMAL_COLOR_NUM;i++) {

			this.connect_num.Add(0);
		}

		this.candidates = new List<Block.COLOR_TYPE>();

		this.cake.is_enable  = false;
	}

	// 开始时需要设置同色4方块排列的数量
	public int	connect_arrow_num = 1;

	// 取得下一个方块的颜色（游戏开始时，全部设置后）
	public Block.COLOR_TYPE	getNextColorStart(int x, int y)
	{
#if false
		Block.COLOR_TYPE	color_type;

		color_type = (Block.COLOR_TYPE)Random.Range((int)Block.NORMAL_COLOR_FIRST, (int)Block.NORMAL_COLOR_LAST + 1);

		return(color_type);
#else
		StackBlock[,]		blocks          = this.control.blocks;
		ConnectChecker		connect_checker = this.control.connect_checker;
		Block.COLOR_TYPE	org_color;
		int					sel;

		//

		org_color = blocks[x, y].color_type;

		// 初始化“出现时颜色的候选值列表”
		// （使列表包含所有的颜色）
		this.init_candidates();

		// 设置各个方块的颜色时，探测已经排列了几个同种颜色的方块

		for(int i = 0;i < (int)Block.NORMAL_COLOR_NUM;i++) {

			// 放置第i项颜色的方块
			blocks[x, y].setColorType((Block.COLOR_TYPE)i);

			connect_checker.clearAll();

			// 计算连结数
			this.connect_num[i] = connect_checker.checkConnect(x, y);
		}

		if(this.connect_arrow_num > 0) {

			// 生成好了初始的同色4方块组

			// connect_num[] 中的最大值（最大允许max_num 个同色方块排列）
			int		max_num = this.get_max_connect_num();

			// 如果数量不是max_num 则删除（只保留适当对象作为候补）
			this.erase_candidate_if_not(max_num);

			sel = Random.Range(0, candidates.Count);

			// 如果同色4个方块排列后，初始连结好的4方块组的数量将减少1
			if(this.connect_num[(int)candidates[sel]] >= 4) {

				this.connect_arrow_num--;
			}

		} else {

			// 还没有生成初始的同色4方块组

			// 如果同种颜色已经有4个方块排列好了则从候补值中剔除
			for(int i = candidates.Count - 1;i >= 0;i--) {
	
				if(this.connect_num[(int)candidates[i]] >= 4) {

					candidates.RemoveAt(i);
				}
			}

			if(candidates.Count == 0) {

				this.init_candidates();
				Debug.Log("give up");
			}

			// connect_num[] 中的最大值（最大允许max_num 个同色方块排列）
			int		max_num = this.get_max_connect_num();

			// 如果数量不是max_num 则删除（只保留适当对象作为候补）
			this.erase_candidate_if_not(max_num);

			sel = Random.Range(0, candidates.Count);
		}


		//

		blocks[x, y].setColorType(org_color);

		return((Block.COLOR_TYPE)candidates[sel]);
#endif
	}

	// 初始化候补列表（使列表中包含所有颜色）
	private void	init_candidates()
	{
		this.candidates.Clear();

		for(int i = 0;i < Block.NORMAL_COLOR_NUM;i++) {

			if(!this.control.is_color_enable[i]) {

				continue;
			}

			this.candidates.Add((Block.COLOR_TYPE)i);
		}
	}

	// 挑选出同色方块的排列数量最多的颜色
	private int		get_max_connect_num()
	{
		int		sel = 0;

		for(int i = 1;i < candidates.Count;i++) {

			if(this.connect_num[(int)this.candidates[i]] > this.connect_num[(int)this.candidates[sel]]) {

				sel = i;
			}
		}

		return(this.connect_num[(int)this.candidates[sel]]);
	}

	// 同色排列的方块数量如果不等于 connect_num 则删除
	private void	erase_candidate_if_not(int connect_num)
	{
		for(int i = candidates.Count - 1;i >= 0;i--) {
	
			if(this.connect_num[(int)candidates[i]] != connect_num) {
	
				candidates.RemoveAt(i);
			}
		}
	}

	// 从候补值中剔除指定的颜色
	private void	erase_color_from_candidates(Block.COLOR_TYPE color)
	{
		for(int i = candidates.Count - 1;i >= 0;i--) {

			if(candidates[i] == color) {
				
				candidates.RemoveAt(i);
			}
		}
	}

	// 取得下一个方块的颜色（从画面上方落下的方块）
	public Block.COLOR_TYPE[] getNextColorsAppearFromTop(int y)
	{
		Block.COLOR_TYPE[]	colors = new Block.COLOR_TYPE[StackBlockControl.BLOCK_NUM_X];

		for(int i = 0;i < StackBlockControl.BLOCK_NUM_X;i++) {

			colors[i] = this.get_next_color_appear_from_top_sub(i, y, colors);
		}

		// 如果存在待出现的蛋糕，则让蛋糕出现
		if(this.cake_request > 0) {

			this.cake.is_enable  = true;
			this.cake.x          = Random.Range(0, StackBlockControl.BLOCK_NUM_X);
			this.cake.color_type = (Block.COLOR_TYPE)((int)Block.CAKE_COLOR_FIRST + this.cake_count%2);

			colors[this.cake.x] = this.cake.color_type;

			this.cake_count++;

			this.cake_request = Mathf.Max(this.cake_request - 1, 0);
		}

		return(colors);
	}
	private Block.COLOR_TYPE	get_next_color_appear_from_top_sub(int x, int y, Block.COLOR_TYPE[] colors)
	{
		StackBlock[,]		blocks     = this.control.blocks;
		Block.COLOR_TYPE	color_type = Block.NORMAL_COLOR_FIRST;
		int					sel;

		this.init_candidates();

		// 暂时让左方和下方的方块颜色不同

		this.erase_color_from_candidates(blocks[x, y + 1].color_type);

		if(x > 0) {

			this.erase_color_from_candidates(colors[x - 1]);
		}

		//

		sel = Random.Range(0, candidates.Count);

		color_type = this.candidates[sel];

		return(color_type);
	}

	// 取得下一个方块的颜色（画面下方新出现的方块）
	public Block.COLOR_TYPE	getNextColorAppearFromBottom(int x)
	{
		StackBlock[,]		blocks     = this.control.blocks;
		Block.COLOR_TYPE	color_type = Block.NORMAL_COLOR_FIRST;

		this.init_candidates();

		//

		int		y;

		for(y = (int)StackBlockControl.BLOCK_NUM_Y - 1 - 1;y >= 0;y--) {

			StackBlock	block = blocks[x, y];

			if(block.isConnectable()) {

				break;
			}
		}

		if(y >= 0) {

			Block.COLOR_TYPE	erase_color = blocks[x, y].color_type;

			for(int i = 0;i < candidates.Count;i++) {

				if(candidates[i] == erase_color) {

					candidates.RemoveAt(i);
					break;
				}
			}
		}

		color_type = candidates[Random.Range(0, candidates.Count)];

		return(color_type);
	}

	// ---------------------------------------------------------------- //

	//画面上的方块被按下时（玩家的操作）的处理
	public void	onDropBlock(StackBlock block)
	{
#if true
		do {

			if(!block.isCakeBlock()) {

				break;
			}

			if(!this.cake.is_enable) {

				break;
			}

			//

			this.clearCake();

		} while(false);

#else
		// 蛋糕在画面下方出现（根据玩家操作）时
		// 必须增加蛋糕的出现数量
		// （如果不这样做的话可能会导致蛋糕再也不出现）
		if(block.isCakeBlock()) {

			if(this.cake.is_enable) {

				this.cake.is_enable  = true;
				this.cake.is_active  = false;
				this.cake.x          = block.place.x;
				this.cake.color_type = block.color_type;
			}
		}

		if(this.cake.is_enable && !this.cake.is_active) {

			this.cake.out_count++;
		}
#endif
	}

	// 蛋糕出现
	public void	requestCake()
	{
		if(!this.cake.is_enable) {

			this.cake_request++;
		}
	}

	// 吃了蛋糕
	public void	onEatCake()
	{
		this.clearCake();
	}

	// 蛋糕正在出现？
	public bool	isCakeAppeared()
	{
		return(this.cake.is_enable);
	}

	// 等待蛋糕出现？
	public bool	isCakeRequested()
	{
		return(this.cake_request > 0);
	}

	// 清空蛋糕出现的信息
	public void	clearCake()
	{
		this.cake.is_enable  = false;
		this.cake.x          = -1;
		this.cake.color_type = Block.COLOR_TYPE.NONE;
	}
}

using UnityEngine;
using System.Collections;

public class ConnectChecker {

	public StackBlockControl	stack_control = null;

	public StackBlock[,]	blocks;

	public enum CONNECT_STATUS {

		NONE = -1,

		UNCHECKED = 0,
		CONNECTED,

		NUM,
	};

	public CONNECT_STATUS[,]		connect_status = null;

	public StackBlock.PlaceIndex[]	connect_block;

	// ---------------------------------------------------------------- //

	public void create()
	{
		this.connect_status = new CONNECT_STATUS[StackBlockControl.BLOCK_NUM_X, StackBlockControl.BLOCK_NUM_Y];

		this.connect_block = new StackBlock.PlaceIndex[StackBlockControl.BLOCK_NUM_X*StackBlockControl.BLOCK_NUM_Y];
	}

	public void clearAll()
	{
		for(int y = 0;y < StackBlockControl.BLOCK_NUM_Y;y++) {

			for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {

				this.connect_status[x, y] = CONNECT_STATUS.UNCHECKED;
			}
		}

	}

	// 检测和（x, y）位置有连接的方块
	public int	checkConnect(int x, int y)
	{
		//

		int connect_num = this.check_connect_recurse(x, y, Block.COLOR_TYPE.NONE, 0);

		for(int i = 0;i < connect_num;i++) {

			StackBlock.PlaceIndex index = this.connect_block[i];

			this.connect_status[index.x, index.y] = CONNECT_STATUS.CONNECTED;
		}

		return(connect_num);
	}
	private bool	is_error_printed = false;

	private int		check_connect_recurse(int x, int y, Block.COLOR_TYPE previous_color, int connect_count)
	{
		StackBlock.PlaceIndex	block_index;

		do {

			// 用于防止无限循环的检测
			if(connect_count >= StackBlockControl.BLOCK_NUM_X*StackBlockControl.BLOCK_NUM_Y) {

				if(!this.is_error_printed) {

					Debug.LogError("Suspicious recursive call");
					this.is_error_printed = true;
				}
				break;
			}

			// 非连结对象（在空中，或者正被隐藏）
			if(!this.blocks[x, y].isConnectable()) {

				break;
			}

			// 如果已经和其他方块连结则忽略跳过
			//
			if(this.connect_status[x, y] == CONNECT_STATUS.CONNECTED) {

				break;
			}

			//

			block_index.x = x;
			block_index.y = y;

			// 如果本次已经检测过则忽略跳过
			if(this.is_checked(block_index, connect_count)) {

				break;
			}

			//

			if(previous_color == Block.COLOR_TYPE.NONE) {

				// 开始的第一个

				this.connect_block[0] = block_index;

				connect_count = 1;

			} else {

				// 从第2个开始，检测是否和前一个方块颜色相同

				if(this.blocks[x, y].color_type == previous_color) {
	
					this.connect_block[connect_count] = block_index;

					connect_count++;
				}
			}

			// 如果和前一个颜色相同，则继续检测旁边的方块

			if(previous_color == Block.COLOR_TYPE.NONE || this.blocks[x, y].color_type == previous_color) {
	
				// 左	
				if(x > 0) {
		
					connect_count = this.check_connect_recurse(x - 1, y, this.blocks[x, y].color_type, connect_count);
				}
				// 右
				if(x < StackBlockControl.BLOCK_NUM_X - 1) {
		
					connect_count = this.check_connect_recurse(x + 1, y, this.blocks[x, y].color_type, connect_count);
				}
				// 上
				if(y > 0) {
	
					connect_count = this.check_connect_recurse(x, y - 1, this.blocks[x, y].color_type, connect_count);
				}
				// 下
				if(y < StackBlockControl.BLOCK_NUM_Y - 1) {
	
					connect_count = this.check_connect_recurse(x, y + 1, this.blocks[x, y].color_type, connect_count);
				}
			}

		} while(false);

		return(connect_count);
	}

	// 是否已经检测完成？
	private bool	is_checked(StackBlock.PlaceIndex place, int connect_count)
	{
		bool	is_checked = false;

		for(int i = 0;i < connect_count;i++) {

			if(this.connect_block[i].Equals(place)) {

				is_checked = true;
				break;
			}
		}

		return(is_checked);
	}

}

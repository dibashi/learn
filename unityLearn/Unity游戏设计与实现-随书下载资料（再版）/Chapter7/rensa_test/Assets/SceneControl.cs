using UnityEngine;
using System.Collections;

public class SceneControl : MonoBehaviour {

	public static int	BLOCK_NUM_X = 9;
	public static int	BLOCK_NUM_Y = 5;

	public GameObject	BlockPrefab = null;

	public BlockControl[,]	blocks;

	private bool		toggle_checked = false;

	void	Start()
	{
		// 生成方块，进行配置

		this.blocks = new BlockControl[BLOCK_NUM_X, BLOCK_NUM_Y];

		int		color_index = 0;

		for(int y = 0;y < BLOCK_NUM_Y;y++) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				GameObject game_object = Instantiate(this.BlockPrefab) as GameObject;

				BlockControl	block = game_object.GetComponent<BlockControl>();

				this.blocks[x, y] = block;

				//

				Vector3	position = block.transform.position;

				position.x = -(BLOCK_NUM_X/2.0f - 0.5f) + x;
				position.y = -0.5f                      - y;

				block.transform.position = position;

				block.SetColor((BlockControl.COLOR)color_index);

				//

				color_index = Random.Range(0, (int)BlockControl.COLOR.NORMAL_COLOR_NUM);
			}
		}

		//
	}
	
	void	Update()
	{
		// 按下空格键，则进行连结检测
		if(Input.GetKeyDown(KeyCode.Space)) {

			// 回到方块最初的状态

			for(int y = 0;y < BLOCK_NUM_Y;y++) {
	
				for(int x = 0;x < BLOCK_NUM_X;x++) {
	
					this.blocks[x, y].ToBeVanished(false);	
				}
			}

			// 回到方块最初的状态

			this.ClearConnect();

			if(!this.toggle_checked) {

				// 连结检测
				this.CheckConnect();
			}

			this.toggle_checked = !this.toggle_checked;
		}
	}

	public enum CONNECT_STATUS {

		NONE = -1,

		UNCHECKED = 0,
		CONNECTED,

		NUM,
	};

	public CONNECT_STATUS[,]		connect_status = null;

	public struct BlockIndex {

		public int		x;
		public int		y;
	};

	public BlockIndex[]	connect_block;

	public void	CheckConnect()
	{
		// 将所有的方块设置为“未检测”

		this.connect_status = new CONNECT_STATUS[BLOCK_NUM_X, BLOCK_NUM_Y];

		this.connect_block = new BlockIndex[BLOCK_NUM_X*BLOCK_NUM_Y];

		for(int y = 0;y < BLOCK_NUM_Y;y++) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				this.connect_status[x, y] = CONNECT_STATUS.UNCHECKED;
			}
		}

		//

		for(int y = 0;y < BLOCK_NUM_Y;y++) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				int connect_num = this.check_connect_recurse(x, y, BlockControl.COLOR.NONE, 0);

				// 同种颜色的方块超过4个排列则视为连结
				if(connect_num >= 4) {

					for(int i = 0;i < connect_num;i++) {

						BlockIndex index = this.connect_block[i];

						this.connect_status[index.x, index.y] = CONNECT_STATUS.CONNECTED;

						this.blocks[index.x, index.y].ToBeVanished(true);
					}
				}
			}
		}
	}

	private int		check_connect_recurse(int x, int y, BlockControl.COLOR previous_color, int connect_count)
	{
		BlockIndex	block_index;

		do {

			// 如果已经和其他方块连结则忽略跳过
			//
			if(this.connect_status[x, y] == CONNECT_STATUS.CONNECTED) {

				break;
			}

			//

			block_index.x = x;
			block_index.y = y;

			// 如果本次已经检测过则忽略跳过

			bool	is_checked = false;

			for(int i = 0;i < connect_count;i++) {

				if(this.connect_block[i].Equals(block_index)) {

					is_checked = true;
					break;
				}
			}
			if(is_checked) {

				break;
			}

			//

			if(previous_color == BlockControl.COLOR.NONE) {

				// 开始的第一个

				this.connect_block[0] = block_index;

				connect_count = 1;

			} else {

				// 从第2个开始，检测是否和前一个方块颜色相同

				if(this.blocks[x, y].color == previous_color) {
	
					this.connect_block[connect_count] = block_index;

					connect_count++;
				}
			}

			// 如果和前一个颜色相同，则继续检测旁边的方块

			if(previous_color == BlockControl.COLOR.NONE || this.blocks[x, y].color == previous_color) {

				// 左		
				if(x > 0) {
		
					connect_count = this.check_connect_recurse(x - 1, y, this.blocks[x, y].color, connect_count);
				}
				// 右
				if(x < BLOCK_NUM_X - 1) {
		
					connect_count = this.check_connect_recurse(x + 1, y, this.blocks[x, y].color, connect_count);
				}
				// 上
				if(y > 0) {
		
					connect_count = this.check_connect_recurse(x, y - 1, this.blocks[x, y].color, connect_count);
				}
				// 下
				if(y < BLOCK_NUM_Y - 1) {
		
					connect_count = this.check_connect_recurse(x, y + 1, this.blocks[x, y].color, connect_count);
				}
		
				// 为了让方块按对角线排列时消除
				/*if(x > 0 && y > 0) {
		
					connect_count = this.check_connect_recurse(x - 1, y - 1, this.blocks[x, y].color, connect_count);
				}
				if(x > 0 && y < BLOCK_NUM_Y - 1) {
		
					connect_count = this.check_connect_recurse(x - 1, y + 1, this.blocks[x, y].color, connect_count);
				}
				if(x < BLOCK_NUM_X - 1 && y > 0) {
		
					connect_count = this.check_connect_recurse(x + 1, y - 1, this.blocks[x, y].color, connect_count);
				}
				if(x < BLOCK_NUM_X - 1 && y < BLOCK_NUM_Y - 1) {
		
					connect_count = this.check_connect_recurse(x + 1, y + 1, this.blocks[x, y].color, connect_count);
				}*/
			}
	

		} while(false);

		return(connect_count);
	}

	// 所有的方块恢复到“未连结”的状态
	public void	ClearConnect()
	{
		for(int y = 0;y < BLOCK_NUM_Y;y++) {

			for(int x = 0;x < BLOCK_NUM_X;x++) {

				this.blocks[x, y].ToBeVanished(false);
			}
		}
	}

}

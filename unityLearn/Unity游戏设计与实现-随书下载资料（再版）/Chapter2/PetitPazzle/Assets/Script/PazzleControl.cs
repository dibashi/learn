using UnityEngine;
using System.Collections;

public class PazzleControl : MonoBehaviour {

	public GameControl		game_control = null;

	private int		piece_num;				// 碎片的数量（全部）
	private int		piece_finished_num;		// 完成的碎片数量

	enum STEP {

		NONE = -1,

		PLAY = 0,	// 正在游戏中
		CLEAR,		// 清空游戏

		NUM,
	};

	private STEP	step      = STEP.NONE;
	private STEP	next_step = STEP.NONE;

	private float		step_timer = 0.0f;
	private float		step_timer_prev = 0.0f;

	// -------------------------------------------------------- //

	// 所有的碎片
	private PieceControl[]	all_pieces;

	// 考虑中的碎片。按距离远近排列
	private PieceControl[]	active_pieces;

	// 碎片洗牌的位置（范围）
	private Bounds	shuffle_zone;

	// [degree] 旋转整体拼图的角度
	private float	pazzle_rotation = 37.0f;

	// 设置碎片洗牌的参数。网格的数量（1条边）
	private int		shuffle_grid_num = 1;

	// 显示“完成了！”？
	private bool	is_disp_cleared = false;

	// -------------------------------------------------------- //

	void	Start()
	{

		this.game_control =  GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControl>();

		// 统计碎片的对象数量

		this.piece_num = 0;

		for(int i = 0;i < this.transform.childCount;i++) {

			GameObject	piece = this.transform.GetChild(i).gameObject;

			if(!this.is_piece_object(piece)) {

				continue;
			}

			this.piece_num++;
		}

		//

		this.all_pieces    = new PieceControl[this.piece_num];
		this.active_pieces = new PieceControl[this.piece_num];

		// 向各个碎片添加 PieceControl 组件（脚本“PieceControl.cs”）

		for(int i = 0, n = 0;i < this.transform.childCount;i++) {

			GameObject	piece = this.transform.GetChild(i).gameObject;

			if(!this.is_piece_object(piece)) {

				continue;
			}

			piece.AddComponent<PieceControl>();

			piece.GetComponent<PieceControl>().pazzle_control = this;

			//

			this.all_pieces[n++] = piece.GetComponent<PieceControl>();
		}

		this.piece_finished_num = 0;

		// 设置碎片的绘制顺序
		//
		this.set_height_offset_to_pieces();

		// 设置碎片的绘制顺序
		//
		for(int i = 0;i < this.transform.childCount;i++) {

			GameObject	game_object = this.transform.GetChild(i).gameObject;

			if(this.is_piece_object(game_object)) {

				continue;
			}

			game_object.GetComponent<Renderer>().material.renderQueue = this.GetDrawPriorityBase();
		}

		// 算出碎片洗牌的位置（范围）
		//
		this.calc_shuffle_zone();


		this.is_disp_cleared = false;
	}

	void	Update()
	{
		// ---------------------------------------------------------------- //

		this.step_timer_prev = this.step_timer;

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 检测状态迁移

		switch(this.step) {

			case STEP.NONE:
			{
				this.next_step = STEP.PLAY;
			}
			break;

			case STEP.PLAY:
			{
				// 如果碎片全部都被放置到正解的位置上，清空
				if(this.piece_finished_num >= this.piece_num) {
		
					this.next_step = STEP.CLEAR;
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 迁移时的初始化

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.PLAY:
				{
					// 由于 this.active_pieces = this.all_pieces ，数组的引用都已经被拷贝，
					// 一个一个进行拷贝
					for(int i = 0;i < this.all_pieces.Length;i++) {

						this.active_pieces[i] = this.all_pieces[i];
					}

					this.piece_finished_num = 0;

					this.shuffle_pieces();

					foreach(PieceControl piece in this.active_pieces) {

						piece.Restart();
					}

					// 设置碎片的绘制顺序
					//
					this.set_height_offset_to_pieces();
				}
				break;

				case STEP.CLEAR:
				{
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 执行处理

		switch(this.step) {

			case STEP.CLEAR:
			{
				// 完成时的音乐

				const float	play_se_delay = 0.40f;

				if(this.step_timer_prev < play_se_delay && play_se_delay <= this.step_timer) {

					this.game_control.playSe(GameControl.SE.COMPLETE);
					this.is_disp_cleared = true;
				}
			}
			break;
		}

		PazzleControl.DrawBounds(this.shuffle_zone);
	}

	// “重新开始”按钮被按下时
	public void	beginRetryAction()
	{
		this.next_step = STEP.PLAY;
	}

	// 开始拖动碎片时的处理
	public void		PickPiece(PieceControl piece)
	{
		int	i, j;

		// 将被点击的碎片移动到数组的头部
		//
		// 由于this.pieces[] 按照显示的顺序排列，头部的元素将
		// 被显示在最上方

		for(i = 0;i < this.active_pieces.Length;i++) {

			if(this.active_pieces[i] == null) {

				continue;
			}

			if(this.active_pieces[i].name == piece.name) {

				// 将位于“被点击的碎片”之前的碎片，逐个向后移动
				//
				for(j = i;j > 0;j--) {

					this.active_pieces[j] = this.active_pieces[j - 1];
				}

				// 被点击的碎片回到数组头部
				this.active_pieces[0] = piece;

				break;
			}
		}

		this.set_height_offset_to_pieces();
	}

	// 碎片被放置到正解位置时的处理
	public void		FinishPiece(PieceControl piece)
	{
		int	i, j;

		piece.GetComponent<Renderer>().material.renderQueue = this.GetDrawPriorityFinishedPiece();

		// 将被点击的碎片从数组中剔除

		for(i = 0;i < this.active_pieces.Length;i++) {

			if(this.active_pieces[i] == null) {

				continue;
			}

			if(this.active_pieces[i].name == piece.name) {

				// 将位于“被点击碎片”之后的碎片逐个往前移动
				//
				for(j = i;j < this.active_pieces.Length - 1;j++) {

					this.active_pieces[j] = this.active_pieces[j + 1];
				}

				// 清空数组的末尾
				this.active_pieces[this.active_pieces.Length - 1] = null;

				// “已经得到正解的碎片”的数量 ＋ 1
				this.piece_finished_num++;

				break;
			}
		}

		this.set_height_offset_to_pieces();
	}

	// ---------------------------------------------------------------------------------------- //

	private static float	SHUFFLE_ZONE_OFFSET_X = -5.0f;
	private static float	SHUFFLE_ZONE_OFFSET_Y =  1.0f;
	private static float	SHUFFLE_ZONE_SCALE =  1.1f;

	// 计算处对碎片洗牌的位置（范围）
	private void	calc_shuffle_zone()
	{
		Vector3		center;

		// 分散开碎片的范围中心

		center = Vector3.zero;

		foreach(PieceControl piece in this.all_pieces) {

			center += piece.finished_position;
		}
		center /= (float)this.all_pieces.Length;

		center.x += SHUFFLE_ZONE_OFFSET_X;
		center.z += SHUFFLE_ZONE_OFFSET_Y;

		// 设置碎片的网格数量

		this.shuffle_grid_num = Mathf.CeilToInt(Mathf.Sqrt((float)this.all_pieces.Length));

		// 碎片的边框矩形中最大值 ＝ 1个网格的尺寸

		Bounds	piece_bounds_max = new Bounds(Vector3.zero, Vector3.zero);

		foreach(PieceControl piece in this.all_pieces) {

			Bounds bounds = piece.GetBounds(Vector3.zero);

			piece_bounds_max.Encapsulate(bounds);
		}

		piece_bounds_max.size *= SHUFFLE_ZONE_SCALE;

		this.shuffle_zone = new Bounds(center, piece_bounds_max.size*this.shuffle_grid_num);
	}

	// 对碎片位置进行随机洗牌
	private void	shuffle_pieces()
	{
	#if true
		// 将碎片按照网格顺序排列

		int[]		piece_index = new int[this.shuffle_grid_num*this.shuffle_grid_num];

		for(int i = 0;i < piece_index.Length;i++) {

			if(i < this.all_pieces.Length) {

				piece_index[i] = i;

			} else {

				piece_index[i] = -1;
			}
		}

		// 随机选取两个碎片，交换位置

		for(int i = 0;i < piece_index.Length - 1;i++) {

			int	j = Random.Range(i + 1, piece_index.Length);

			int	temp = piece_index[j];

			piece_index[j] = piece_index[i];

			piece_index[i] = temp;
		}
	
		// 通过位置的索引变换为实际的坐标来进行配置

		Vector3	pitch;

		pitch = this.shuffle_zone.size/(float)this.shuffle_grid_num;

		for(int i = 0;i < piece_index.Length;i++) {

			if(piece_index[i] < 0) {

				continue;
			}

			PieceControl	piece = this.all_pieces[piece_index[i]];

			Vector3	position = piece.finished_position;

			int		ix = i%this.shuffle_grid_num;
			int		iz = i/this.shuffle_grid_num;

			position.x = ix*pitch.x;
			position.z = iz*pitch.z;

			position.x += this.shuffle_zone.center.x - pitch.x*(this.shuffle_grid_num/2.0f - 0.5f);
			position.z += this.shuffle_zone.center.z - pitch.z*(this.shuffle_grid_num/2.0f - 0.5f);

			position.y = piece.finished_position.y;

			piece.start_position = position;
		}

		// 逐步（网格的格子内）随机移动位置

		Vector3		offset_cycle = pitch/2.0f;
		Vector3		offset_add   = pitch/5.0f;

		Vector3		offset = Vector3.zero;

		for(int i = 0;i < piece_index.Length;i++) {

			if(piece_index[i] < 0) {

				continue;
			}

			PieceControl	piece = this.all_pieces[piece_index[i]];

			Vector3	position = piece.start_position;

			position.x += offset.x;
			position.z += offset.z;

			piece.start_position = position;

			//


			offset.x += offset_add.x;

			if(offset.x > offset_cycle.x/2.0f) {

				offset.x -= offset_cycle.x;
			}

			offset.z += offset_add.z;

			if(offset.z > offset_cycle.z/2.0f) {

				offset.z -= offset_cycle.z;
			}
		}

		// 使全体旋转

		foreach(PieceControl piece in this.all_pieces) {

			Vector3		position = piece.start_position;

			position -= this.shuffle_zone.center;

			position = Quaternion.AngleAxis(this.pazzle_rotation, Vector3.up)*position;

			position += this.shuffle_zone.center;

			piece.start_position = position;
		}

		this.pazzle_rotation += 90;

	#else
		// 简单地使用随机数来决定坐标时的情况
		foreach(PieceControl piece in this.all_pieces) {

			Vector3	position;

			Bounds	piece_bounds = piece.GetBounds(Vector3.zero);

			position.x = Random.Range(this.shuffle_zone.min.x - piece_bounds.min.x, this.shuffle_zone.max.x - piece_bounds.max.x);
			position.z = Random.Range(this.shuffle_zone.min.z - piece_bounds.min.z, this.shuffle_zone.max.z - piece_bounds.max.z);

			position.y = piece.finished_position.y;

			piece.start_position = position;
		}
	#endif
	}

	// 碎片的 GameObject？
	private bool is_piece_object(GameObject game_object)
	{
		bool	is_piece = false;

		do {

			// 名字中含有“base”表示底座对象
			if(game_object.name.Contains("base")) {

				continue;
			}

			//

			is_piece = true;

		} while(false);

		return(is_piece);
	}


	// ---------------------------------------------------------------------------------------- //

	// 给所有的碎片设置高度的偏移
	private void	set_height_offset_to_pieces()
	{
		float	offset = 0.01f;

		int		n = 0;

		foreach(PieceControl piece in this.active_pieces) {

			if(piece == null) {

				continue;
			}

			// 指定绘制的顺序
			// pieces 中越前面的碎片＝越靠近上方的碎片被绘制的顺序越晚
			//
			piece.GetComponent<Renderer>().material.renderQueue = this.GetDrawPriorityPiece(n);

			// 为了能够使点击时位于最上方的碎片的 OnMouseDown() 方法被调用，
			// 指定Y轴高度的偏移
			// （不这样处理的话可能会由于绘制优先度的关系而导致下面的碎片响应了点击）

			offset -= 0.01f/this.piece_num;

			piece.SetHeightOffset(offset);

			//

			n++;
		}
	}

	// 获取绘制的优先度（底座）
	private int	GetDrawPriorityBase()
	{
		return(0);
	}

	// 获取绘制的优先度（被放置到正确位置的碎片）
	private int	GetDrawPriorityFinishedPiece()
	{
		int	priority;

		priority = this.GetDrawPriorityBase() + 1;

		return(priority);
	}

	// 获取绘制的优先度（“重新开始”按钮）
	public int	GetDrawPriorityRetryButton()
	{
		int	priority;

		priority = this.GetDrawPriorityFinishedPiece() + 1;

		return(priority);
	}

	// 获取绘制的优先度（被放置到正确位置的碎片）
	private int	GetDrawPriorityPiece(int priority_in_pieces)
	{
		int	priority;

		// 在“重新开始”按钮相应值的基础上，加上1
		priority = this.GetDrawPriorityRetryButton() + 1;

		// 第0号priority_in_pieces 在最上面，renderQueue的值越小则被越先绘制
		priority += this.piece_num - 1 - priority_in_pieces;

		return(priority);
	}

	// ---------------------------------------------------------------------------------------- //

	// 拼图是否完成了？
	public bool	isCleared()
	{
		return(this.step == STEP.CLEAR);
	}

	// 显示“成功了！”？
	public bool	isDispCleard()
	{
		return(this.is_disp_cleared);
	}

	// ---------------------------------------------------------------------------------------- //

	public static void	DrawBounds(Bounds bounds)
	{
		Vector3[]	square = new Vector3[4];

		square[0] = new Vector3(bounds.min.x, 0.0f, bounds.min.z);
		square[1] = new Vector3(bounds.max.x, 0.0f, bounds.min.z);
		square[2] = new Vector3(bounds.max.x, 0.0f, bounds.max.z);
		square[3] = new Vector3(bounds.min.x, 0.0f, bounds.max.z);

		for(int i = 0;i < 4;i++) {

			Debug.DrawLine(square[i], square[(i + 1)%4], Color.white, 0.0f, false);
		}

	}
}

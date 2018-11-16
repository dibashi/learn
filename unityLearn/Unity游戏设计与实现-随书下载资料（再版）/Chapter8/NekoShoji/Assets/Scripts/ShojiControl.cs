using UnityEngine;
using System.Collections;

public class ShojiControl : MonoBehaviour {

	public struct HoleIndex {

		public int	x;
		public int	y;

	};

	// 初始位置X，Y
	public float init_x = 0.0f;
	public float init_y = 2.0f;
	public float init_z = 5.0f;
	
	// 地面的宽度（Z方向）
	public static float WIDTH = 15.0f;
	
	// 和玩家的距离（DIST < WIDTH）
	public float DIST = 10.0f;
	
	// 对象自身的移动距离
	public static float MOVE = 0.1f;
	
	// 对象自身的中间位置
	public Vector3 neutral_position;

	public GameObject paperPrefab;

	public SyoujiPaperControl[,] papers;

	public static float	TILE_SIZE_X = 0.85f;
	public static float	TILE_SIZE_Y = 0.94f;
	public static float	TILE_ORIGIN_X = -0.85f;
	public static float	TILE_ORIGIN_Y =  1.92f;

	public static int	TILE_NUM_X = 3;
	public static int	TILE_NUM_Y = 3;

	public int	paper_num = TILE_NUM_X*TILE_NUM_Y;		// 窗户纸的剩余张数

	// ---------------------------------------------------------------- //

	// Use this for initialization
	void Start () {
	
		// 对象自身的中间位置
		this.neutral_position = this.transform.position;

		this.papers = new SyoujiPaperControl[TILE_NUM_X, TILE_NUM_Y];

		for(int x = 0;x < TILE_NUM_X;x++) {

			for(int y = 0;y < TILE_NUM_Y;y++) {
				
				GameObject	go = Instantiate(this.paperPrefab) as GameObject;

				go.transform.parent = this.transform;

				//

				Vector3	position = go.transform.localPosition;

				position.x = TILE_ORIGIN_X + x*TILE_SIZE_X;
				position.y = TILE_ORIGIN_Y + y*TILE_SIZE_Y;
				position.z = 0.0f;

				go.transform.localPosition = position;

				//

				SyoujiPaperControl	paper_control = go.GetComponent<SyoujiPaperControl>();

				paper_control.shoji_control = this;
				paper_control.hole_index.x = x;
				paper_control.hole_index.y = y;

				this.papers[x, y] = paper_control;
			}
		}

		//

		this.paper_num = this.papers.Length;
	}
	
	// Update is called once per frame
	void Update () {

	}

	// 撞破窗户数时的处理
	public void	onPaperBreak()
	{
		this.paper_num--;

		this.paper_num = Mathf.Max(0, this.paper_num);

		// 重置穿空计数器（穿过已经撞破窗户纸的次数）
		//
		// 1.撞破窗户纸A
		// 2.穿过窗户纸A（增加窗户纸A的穿空数）
		// 3.撞破窗户纸B
		// 4.穿过窗户纸B ←这时候窗户纸A将变成铁板

		for(int x = 0;x < TILE_NUM_X;x++) {

			for(int y = 0;y < TILE_NUM_Y;y++) {

				this.papers[x, y].resetThroughCount();
			}
		}
	}

	// 获取当前窗户纸的剩余张数
	public int		getPaperNum()
	{
		return(this.paper_num);
	}

	// “格子眼”索引是否有效？
	public bool	isValidHoleIndex(HoleIndex hole_index)
	{
		bool	ret = false;

		do {

			ret = false;

			if(hole_index.x < 0 || TILE_NUM_X <= hole_index.x) {

				break;
			}
			if(hole_index.y < 0 || TILE_NUM_Y <= hole_index.y) {

				break;
			}
			
			ret = true;

		} while(false);

		return(ret);
	}

	// 取得最近的“格子眼”
	public HoleIndex	getClosetHole(Vector3 position)
	{
		HoleIndex hole_index;

		position = this.transform.InverseTransformPoint(position);

		hole_index.x = Mathf.RoundToInt((position.x - TILE_ORIGIN_X)/TILE_SIZE_X);
		hole_index.y = Mathf.RoundToInt((position.y - TILE_ORIGIN_Y)/TILE_SIZE_Y);

		return(hole_index);
	}

	// 取得“格子眼”的位置坐标
	public Vector3	getHoleWorldPosition(int hole_pos_x, int hole_pos_y)
	{
		Vector3	position;

		position.x = (float)hole_pos_x*TILE_SIZE_X + TILE_ORIGIN_X;
		position.y = (float)hole_pos_y*TILE_SIZE_Y + TILE_ORIGIN_Y;
		position.z = 0.0f;

		position = this.transform.TransformPoint(position);

		return(position);
	}

}

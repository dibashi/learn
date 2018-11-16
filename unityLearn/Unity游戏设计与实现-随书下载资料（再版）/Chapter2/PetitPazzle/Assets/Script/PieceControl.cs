using UnityEngine;
using System.Collections;

// MeshCollider 是必需的
[RequireComponent(typeof(MeshCollider))]
public class PieceControl : MonoBehaviour {

	// 游戏摄像机
	private	GameObject	game_camera;


	public PazzleControl	pazzle_control = null;

	public GameControl		game_control = null;

	// -------------------------------------------------------- //

	// 拖动时，鼠标光标位置通常位于最初点击处并随碎片一起移动
	// （设置为false后，鼠标光标位置＝碎片中心）
	private static bool	IS_ENABLE_GRAB_OFFSET = true;


	private const float	HEIGHT_OFFSET_BASE = 0.1f;

	private const float	SNAP_SPEED_MIN = 0.01f*60.0f;
	private const float	SNAP_SPEED_MAX = 0.8f*60.0f;

	// -------------------------------------------------------- //

	// 鼠标拖拽的位置
	private	Vector3		grab_offset = Vector3.zero;

	// 是否拖拽中？
	private	bool		is_now_dragging = false;

	// 初始位置＝答案的位置
	public Vector3		finished_position;

	// 开始时的位置（随机分散开的状态）
	public Vector3		start_position;

	public float		height_offset = 0.0f;

	public float		roll = 0.0f;

	// 吸附距离
	//（吸附＝距离正解位置够近时松开按键，碎片被吸附到正解位置的处理）
	static float		SNAP_DISTANCE = 0.5f;

	// 碎片的状态
	enum STEP {

		NONE = -1,

		IDLE = 0,		// 未得到正解
		DRAGING,		// 拖动中
		FINISHED,		// 放置到了正解位置（已无法再被拖动）
		RESTART,		// 重新开始
		SNAPPING,		// 吸附过程中

		NUM,
	};

	// 碎片当前的状态
	private STEP step      = STEP.NONE;

	private STEP next_step = STEP.NONE;

	// 吸附时的目标位置
	private Vector3		snap_target;

	// 吸附后的移动状态
	private STEP		next_step_snap;

	// -------------------------------------------------------- //

	void 	Awake()
	{
		// 记录下初始位置＝正解位置
		this.finished_position = this.transform.position;

		// 初始化
		// 后续将使用移动后的位置来替换
		this.start_position = this.finished_position;
	}

	void	Start()
	{
		// 查找摄像机的实例对象
		this.game_camera = GameObject.FindGameObjectWithTag("MainCamera");

		// 由于Start() 在　PazzleControl.Start() 处理全部结束后被调用
		// 因此 position 表示已经移动的位置
		//this.initial_position = this.transform.position;

		this.game_control =  GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControl>();

	}
	void 	Update()
	{
		Color	color = Color.white;

		// 状态迁移

		switch(this.step) {

			case STEP.NONE:
			{
				this.next_step = STEP.RESTART;
			}
			break;

			case STEP.IDLE:
			{
				if(this.is_now_dragging) {

					// 开始拖动

					this.next_step = STEP.DRAGING;
				}
			}
			break;

			case STEP.DRAGING:
			{
				if(this.is_in_snap_range()) {

					// 进入允许吸附的区域（离正解位置很近）时

					bool	is_snap = false;

					// 松开鼠标按键时开始吸附
					if(!this.is_now_dragging) {

						is_snap = true;
					}
		
					if(is_snap) {

						// 这个碎片已经处理结束

						this.next_step        = STEP.SNAPPING;
						this.snap_target      = this.finished_position;
						this.next_step_snap   = STEP.FINISHED;

						this.game_control.playSe(GameControl.SE.ATTACH);
					}

				} else {
	
					// 处于无法吸附的区域（离正解位置较远）时

					if(!this.is_now_dragging) {

						// 松开按键时

						this.next_step = STEP.IDLE;

						this.game_control.playSe(GameControl.SE.RELEASE);
					}
				}
			}
			break;

			case STEP.SNAPPING:
			{
				// 进入到目标位置后结束

				if((this.transform.position - this.snap_target).magnitude < 0.0001f) {

					this.next_step = this.next_step_snap;
				}
			}
			break;
		}

		// 状态迁移时的初始化处理

		while(this.next_step != STEP.NONE) {

			this.step = this.next_step;

			this.next_step = STEP.NONE;

			switch(this.step) {

				case STEP.IDLE:
				{
					this.SetHeightOffset(this.height_offset);
				}
				break;

				case STEP.RESTART:
				{
					this.transform.position = this.start_position;

					this.SetHeightOffset(this.height_offset);

					this.next_step = STEP.IDLE;
				}
				break;

				case STEP.DRAGING:
				{
					this.begin_dragging();

					// 将拖动开始事件通知给拼图管理类
					this.pazzle_control.PickPiece(this);

					this.game_control.playSe(GameControl.SE.GRAB);
				}
				break;

				case STEP.FINISHED:
				{
					// 靠近正解位置不远处松开
		
					// 吸附到正解位置
					this.transform.position = this.finished_position;
		
					// 通知拼图管理类将这个碎片放置到正解的位置
					this.pazzle_control.FinishPiece(this);

				}
				break;
			}
		}

		// 各个状态的执行处理
		
		this.transform.localScale = Vector3.one;

		switch(this.step) {

			case STEP.DRAGING:
			{
				this.do_dragging();

				// 进入允许吸附的范围（非常靠近正解位置时）后，颜色变亮
				if(this.is_in_snap_range()) {
	
					color *= 1.5f;
				}
	
				this.transform.localScale = Vector3.one*1.1f;
			}
			break;

			case STEP.SNAPPING:
			{
				// 朝目标位置移动

				Vector3	next_position, distance, move;

				// 按Easing方式运动

				distance = this.snap_target - this.transform.position;

				// 下一处位置＝当前位置和目标位置的中间点
				distance *= 0.25f*(60.0f*Time.deltaTime);

				next_position = this.transform.position + distance;

				move = next_position - this.transform.position;

				float	snap_speed_min = PieceControl.SNAP_SPEED_MIN*Time.deltaTime;
				float	snap_speed_max = PieceControl.SNAP_SPEED_MAX*Time.deltaTime;

				if(move.magnitude < snap_speed_min) {

					// 移动量小于一定值则结束
					// 快速向目标位置移动
					// 结束判断在状态迁移检测过程中进行，这里只调整其位置
					// 
					this.transform.position = this.snap_target;

				} else {

					// 如果移动速度过快则进行调整
					if(move.magnitude > snap_speed_max) {

						move *= snap_speed_max/move.magnitude;

						next_position = this.transform.position + move;
					}

					this.transform.position = next_position;
				}
			}
			break;
		}

		this.GetComponent<Renderer>().material.color = color;

		// 绘制出矩形边框
		//
		//PazzleControl.DrawBounds(this.GetBounds(this.transform.position));

		//
	}

	// 拖动开始时的处理
	private void begin_dragging()
	{
		do {

			// 将光标坐标变换为3D空间内的世界坐标

			Vector3 world_position;
	
			if(!this.unproject_mouse_position(out world_position, Input.mousePosition)) {

				break;
			}

			if(PieceControl.IS_ENABLE_GRAB_OFFSET) {

				// 求出偏移值（点击位置距离碎片的中心有多远）
				this.grab_offset = this.transform.position - world_position;
			}

		} while(false);
	}

	// 拖动中的处理
	private void do_dragging()
	{
		do {

			// 将光标坐标变换为3D空间内的世界坐标
			Vector3 world_position;

			if(!this.unproject_mouse_position(out world_position, Input.mousePosition)) {

				break;
			}

			// 加上光标坐标（3D）的偏移值，计算出碎片的中心坐标
			this.transform.position = world_position + this.grab_offset;

		} while(false);
	}

	// 获取碎片的边框矩形
	public Bounds	GetBounds(Vector3 center)
	{
		// 由于 Mesh 不是 Component ，无法执行 GetComponent<Mesh>()

		Bounds	bounds = this.GetComponent<MeshFilter>().mesh.bounds;

		// 设置中心位置
		// （min, max也自动再次计算）
		bounds.center = center;

		return(bounds);
	}

	public void	Restart()
	{
		this.next_step = STEP.RESTART;
	}

	// 按下鼠标按键时
	void 	OnMouseDown()
	{
		this.is_now_dragging = true;
	}

	// 松开鼠标按键时
	void 	OnMouseUp()
	{
		this.is_now_dragging = false;
	}

	// 设置高度的偏移值
	public void	SetHeightOffset(float height_offset)
	{
		Vector3		position = this.transform.position;

		this.height_offset = 0.0f;

		// 只有在放置到正解位置前才有效
		if(this.step != STEP.FINISHED || this.next_step != STEP.FINISHED) {

			this.height_offset = height_offset;
	
			position.y  = this.finished_position.y + PieceControl.HEIGHT_OFFSET_BASE;
			position.y += this.height_offset;
	
			this.transform.position = position;
		}
	}

	// 将鼠标的位置，变换为3D空间内的世界坐标
	//
	// ・穿过鼠标光标和摄像机位置的直线
	// ・ 用于判定是否和地面碰撞的平面
	//　求出↑二者的交点
	//
	public bool		unproject_mouse_position(out Vector3 world_position, Vector3 mouse_position)
	{
		bool	ret;
		float	depth;

		// 通过碎片中心的水平（法线为Y轴，XZ平面）面
		Plane	plane = new Plane(Vector3.up, new Vector3(0.0f, this.transform.position.y, 0.0f));

		// 穿过摄像机位置和鼠标光标位置的直线
		Ray		ray = this.game_camera.GetComponent<Camera>().ScreenPointToRay(mouse_position);

		// 求出上面二者的交点

		if(plane.Raycast(ray, out depth)) {

			world_position = ray.origin + ray.direction*depth;

			ret = true;

		} else {

			world_position = Vector3.zero;

			ret = false;
		}

		return(ret);
	}

	// 允许吸附的位置？（离正解的位置够近的话，会被吸附到正确的位置上）
	private bool	is_in_snap_range()
	{
		bool	ret = false;

		if(Vector3.Distance(this.transform.position, this.finished_position) < PieceControl.SNAP_DISTANCE) {

			ret = true;
		}

		return(ret);
	}
}

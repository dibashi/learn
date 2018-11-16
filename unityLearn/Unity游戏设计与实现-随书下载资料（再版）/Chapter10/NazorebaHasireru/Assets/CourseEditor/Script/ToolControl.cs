using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Linq;

//[ExecuteInEditMode()]
public class ToolControl : MonoBehaviour {

	// 游戏摄像机
	private	GameObject		main_camera;

	private ToolCameraControl	tool_camera = null;
	private GameCameraControl	game_camera = null;
	private CarCamera			car_camera = null;
	private GameControl			game_control = null;

	public GameObject		TunnelPrefab = null;		// 隧道
	public GameObject		TreePrefab = null;			// 树木
	public GameObject[]		BuildingPrefabs;			// 建筑（很多）
	public GameObject		CarPrefab = null;			// 车辆
	public GameObject		JumpSlopePrefab = null;		// 跳台
	public GameObject		StartGatePrefab = null;		// 起点的门
	public GameObject		GoalGatePrefab = null;		// 终点的门

	public Material			material = null;
	public Material			road_material = null;
	public Material			wall_material = null;
	public PhysicMaterial	physic_material = null;

	public GameObject			LineDrawerPrefab = null;
	public LineDrawerControl	line_drawer;

	public RoadCreator		road_creator;
	public TunnelCreator	tunnel_creator;
	public ForestCreator	forest_creator;
	public BuildingArranger	buil_arranger;
	public JumpSlopeCreator	jump_slope_creator;
	public JunctionFinder	junction_finder;			// 探测线的交点
	public ToolGUI			tool_gui;

	public GameObject		car_object = null;

	private GameObject		start_gate = null;			// 起点门（实例）
	private GameObject		goal_gate = null;			// 终点门（实例）

	private GameObject		waku_object = null;

	// 音频片段
	public AudioClip		audio_clip_ignition;		// 引擎发动的音效
	public AudioClip		audio_clip_appear;			// 生成赛道的音效

	public enum STEP {

		NONE = -1,

		EDIT = 0,
		RUN,

		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;

	// ------------------------------------------------------------------------ //

	// Use this for initialization
	void Start ()
	{
		this.tool_gui = this.GetComponent<ToolGUI>();

		// 查找摄像机的实例对象
		this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");

		this.tool_camera = this.main_camera.GetComponent<ToolCameraControl>();
		this.game_camera = this.main_camera.GetComponent<GameCameraControl>();
		this.car_camera  = this.main_camera.GetComponent<CarCamera>();

		this.tool_camera.enabled = true;
		this.game_camera.enabled = false;
		this.car_camera.enabled  = false;

		this.game_control = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControl>();
		this.game_control.tool_control = this;

		//

		this.line_drawer = (Instantiate(this.LineDrawerPrefab) as GameObject).GetComponent<LineDrawerControl>();
		this.line_drawer.root = this;

		this.waku_object = GameObject.Find("waku");

		//

		this.road_creator   = new RoadCreator();
		this.tunnel_creator = new TunnelCreator();
		this.forest_creator = new ForestCreator();
		this.buil_arranger  = new BuildingArranger();
		this.jump_slope_creator = new JumpSlopeCreator();

		this.junction_finder = new JunctionFinder();

		this.tunnel_creator.TunnelPrefab = this.TunnelPrefab;
		this.tunnel_creator.road_creator = this.road_creator;
		this.tunnel_creator.main_camera  = this.main_camera;
		this.tunnel_creator.tool_gui     = this.tool_gui;
		this.tunnel_creator.create();

		this.forest_creator.TreePrefab   = this.TreePrefab;
		this.forest_creator.road_creator = this.road_creator;
		this.forest_creator.main_camera  = this.main_camera;
		this.forest_creator.tool_gui     = this.tool_gui;
		this.forest_creator.create();

		this.buil_arranger.BuildingPrefabs = this.BuildingPrefabs;
		this.buil_arranger.road_creator    = this.road_creator;
		this.buil_arranger.main_camera     = this.main_camera;
		this.buil_arranger.tool_gui        = this.tool_gui;
		this.buil_arranger.create();

		this.jump_slope_creator.JumpSlopePrefab = this.JumpSlopePrefab;
		this.jump_slope_creator.road_creator    = this.road_creator;
		this.jump_slope_creator.main_camera     = this.main_camera;
		this.jump_slope_creator.tool_gui        = this.tool_gui;
		this.jump_slope_creator.create();

		this.junction_finder.create();

		//

		this.game_camera.road_creator = this.road_creator;

		this.step = STEP.EDIT;


	}

	void	Update()
	{
		UIButton[]	buttons = this.tool_gui.buttons;

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 检测是否迁移到下一个状态

		if(this.next_step == STEP.NONE) {

			switch(this.step) {

				case STEP.EDIT:
				{
					// 启动赛车的按钮
					if(buttons[(int)ToolGUI.BUTTON.RUN].trigger_on) {
			
						if(this.road_creator.is_created) {

							this.next_step = STEP.RUN;
						}
					}
				}
				break;

				case STEP.RUN:
				{
					if(buttons[(int)ToolGUI.BUTTON.RUN].trigger_on) {

						// 测试运行结束
	
						this.game_control.stopTestRun();
	
						this.car_object.gameObject.SetActive(false);
	
						this.waku_object.SetActive(true);

						this.tool_gui.onStopTestRun();


						this.next_step = STEP.EDIT;
					}
				}
				break;

			} // switch(this.step)
		}

		// ---------------------------------------------------------------- //
		// 状态迁移时的初始化

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.EDIT:
				{
					this.tool_camera.setEnable(true);
					this.car_camera.setEnable(false);
	
					this.forest_creator.setIsDrawIcon(true);
					this.buil_arranger.setIsDrawIcon(true);

					if(this.tunnel_creator.is_created) {

						this.tunnel_creator.setIsDrawIcon(true);
					}
					if(this.jump_slope_creator.is_created) {

						this.jump_slope_creator.setIsDrawIcon(true);
					}
				}
				break;

				case STEP.RUN:
				{
					// 如果没有赛车则创建
					if(this.car_object == null) {


						RoadCreator.Section	section = this.road_creator.sections[1];
	
						this.car_object = Instantiate(this.CarPrefab) as GameObject;
		
						this.car_object.transform.position = section.center;
						this.car_object.transform.Translate(0.0f, 0.1f, 0.0f);
						this.car_object.transform.rotation = Quaternion.FromToRotation(Vector3.forward, section.direction);
	
						// 摄像机
	
						// 设置目标（可见物体）
						this.car_camera.target = this.car_object.transform;
	
						// 初始化位置，注视点
						this.car_camera.reset();
						this.car_camera.calcPosture();
	
						// 引擎点火音效
	
						this.GetComponent<AudioSource>().PlayOneShot(this.audio_clip_ignition);
					}

					//

					this.tool_camera.setEnable(false);
					this.car_camera.setEnable(true);

					this.tunnel_creator.setIsDrawIcon(false);
					this.forest_creator.setIsDrawIcon(false);
					this.buil_arranger.setIsDrawIcon(false);
					this.jump_slope_creator.setIsDrawIcon(false);
				
					// 开始运行测试

					this.game_control.startTestRun();

					this.car_object.gameObject.SetActive(true);

					this.waku_object.SetActive(false);

					this.tool_gui.onStartTestRun();
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 各个状态的执行处理


		switch(this.step) {

			case STEP.EDIT:
			{
			}
			break;
		}

		// -------------------------------------------------------------------------------------------- //

		if(this.step == STEP.EDIT) {

			//清空按钮
			if(buttons[(int)ToolGUI.BUTTON.NEW].current) {
	
				// 删除上一次处理完成的对象
	
				if(this.line_drawer.isLineDrawed()) {
	
					this.line_drawer.clear();
	
					this.line_drawer.setVisible(true);
				}
	
				if(this.road_creator.is_created) {
	
					this.road_creator.clearOutput();		
				}
	
				this.tunnel_creator.clearOutput();
				this.forest_creator.clearOutput();
				this.buil_arranger.clearOutput();
				this.jump_slope_creator.clearOutput();

				this.tunnel_creator.setIsDrawIcon(false);
				this.forest_creator.setIsDrawIcon(false);
				this.buil_arranger.setIsDrawIcon(false);
				this.jump_slope_creator.setIsDrawIcon(false);

				if(this.car_object != null) {
	
					Destroy(this.car_object);
					this.car_object = null;
				}

				if(this.start_gate != null) {

					Destroy(this.start_gate);
					this.start_gate = null;
				}
				if(this.goal_gate != null) {

					Destroy(this.goal_gate);
					this.goal_gate = null;
				}

				//

				this.game_control.onClearOutput();

				//
	
				this.tool_camera.enabled = true;
				this.game_camera.enabled = false;
			}

			// 载入按钮
			if(buttons[(int)ToolGUI.BUTTON.LOAD].current) {
	
				FileStream    BinaryFile = new FileStream("test-cs.dat", FileMode.Open, FileAccess.Read);
		        BinaryReader  Reader     = new BinaryReader(BinaryFile);
	
				this.line_drawer.loadFromFile(Reader);
	
				Reader.Close();
			}
	
			// 保存按钮
			if(buttons[(int)ToolGUI.BUTTON.SAVE].current) {
	
				FileStream    BinaryFile = new FileStream("test-cs.dat", FileMode.Create, FileAccess.Write);
		        BinaryWriter  Writer     = new BinaryWriter(BinaryFile);
	
				this.line_drawer.saveToFile(Writer);
	
				Writer.Close();
			}
		}

		// -------------------------------------------------------------------------------------------- //

		// 生成道路按钮
		if(buttons[(int)ToolGUI.BUTTON.CREATE_ROAD].current) {

			bool	is_create_road = false;

			do {

				if(!this.line_drawer.isLineDrawed()) {

					break;
				}

				if(this.road_creator.is_created) {

					break;
				}

				is_create_road = true;

			} while(false);

			if(is_create_road) {

				this.create_road();
	
				// 隧道
				// 创建形状时（TunnelCreator.createTunnel()）再次计算
				this.tunnel_creator.place_min = 0.0f;
				this.tunnel_creator.place_max = 0.0f;
	
				// 树林
	
				this.forest_creator.place_max = (float)this.road_creator.sections.Length - 1.0f;
				this.forest_creator.start     = this.road_creator.sections.Length*0.25f;
				this.forest_creator.end       = this.road_creator.sections.Length*0.75f;
	
				this.forest_creator.setIsDrawIcon(true);

				// 建筑街道
	
				this.buil_arranger.place_max = (float)this.road_creator.sections.Length - 1.0f;
				this.buil_arranger.start     = this.road_creator.sections.Length*0.25f;
				this.buil_arranger.end       = this.road_creator.sections.Length*0.75f;
	
				this.buil_arranger.setIsDrawIcon(true);

				this.jump_slope_creator.place_min = 0.0f;
				this.jump_slope_creator.place_max = (float)this.road_creator.sections.Length - 1.0f;

				if(this.road_creator.sections.Length >= 5) {

					// 起点门
	
					RoadCreator.Section	gate_section;
	
					gate_section = this.road_creator.sections[2];
	
					this.start_gate = Instantiate(this.StartGatePrefab) as GameObject;
					this.start_gate.transform.position = gate_section.center;
					this.start_gate.transform.rotation = Quaternion.FromToRotation(Vector3.forward, gate_section.direction);
	
					// 终点门
	
					gate_section = this.road_creator.sections[this.road_creator.sections.Length - 1 - 1];
	
					this.goal_gate = Instantiate(this.GoalGatePrefab) as GameObject;
					this.goal_gate.transform.position = gate_section.center;
					this.goal_gate.transform.rotation = Quaternion.FromToRotation(Vector3.forward, gate_section.direction);
				}

				// 播放音效
				this.GetComponent<AudioSource>().PlayOneShot(this.audio_clip_appear);
			}
		}


		// -------------------------------------------------------------------------------------------- //

		// 创建隧道的按钮
		if(buttons[(int)ToolGUI.BUTTON.TUNNEL_CREATE].trigger_on) {

			do {

				// 如果道路未出现
				if(!this.road_creator.is_created) {
					break;
				}

				// 生成完毕
				if(this.tunnel_creator.is_created) {
					break;
				}

				//

				this.tunnel_creator.createTunnel();

			} while(false);
		}

		// 移动隧道的按钮
		if(this.tunnel_creator.is_created) {

			if(buttons[(int)ToolGUI.BUTTON.TUNNEL_BACKWARD].current) {

				float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.TUNNEL_BACKWARD);

				this.tunnel_creator.setPlace(this.tunnel_creator.place + speed);
			}
			if(buttons[(int)ToolGUI.BUTTON.TUNNEL_FORWARD].current) {

				float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.TUNNEL_FORWARD);

				this.tunnel_creator.setPlace(this.tunnel_creator.place - speed);
			}

		}

		// -------------------------------------------------------------------------------------------- //

		// 创建树林的按钮
		if(buttons[(int)ToolGUI.BUTTON.FOREST_CREATE].current) {

			do {

				// 不允许出现道路未创建的情况
				if(!this.road_creator.is_created) {
					break;
				}

				// 已经生成
				if(this.forest_creator.is_created) {
					break;
				}

				//

				this.forest_creator.createForest();

			} while(false);
		}

		// 树林in 向前按钮
		if(buttons[(int)ToolGUI.BUTTON.FOREST_START_FORWARD].current) {
	
			if(this.road_creator.is_created) {

				if(!this.forest_creator.is_created) {
	
					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.FOREST_START_FORWARD);

					this.forest_creator.setStart(this.forest_creator.start + speed);
				}
			}
		}
		// 树林in 向后按钮
		if(buttons[(int)ToolGUI.BUTTON.FOREST_START_BACKWARD].current) {

			if(this.road_creator.is_created) {

				if(!this.forest_creator.is_created) {
	
					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.FOREST_START_BACKWARD);

					this.forest_creator.setStart(this.forest_creator.start - speed);
				}
			}
		}
		// 树林out 向前按钮
		if(buttons[(int)ToolGUI.BUTTON.FOREST_END_FORWARD].current) {

			if(this.road_creator.is_created) {

				if(!this.forest_creator.is_created) {

					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.FOREST_END_FORWARD);

					this.forest_creator.setEnd(this.forest_creator.end + speed);
				}
			}
		}
		// 树林out 向后按钮
		if(buttons[(int)ToolGUI.BUTTON.FOREST_END_BACKWARD].current) {

			if(this.road_creator.is_created) {

				if(!this.forest_creator.is_created) {

					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.FOREST_END_BACKWARD);

					this.forest_creator.setEnd(this.forest_creator.end - speed);
				}
			}
		}
		
		// -------------------------------------------------------------------------------------------- //

		// 排列建筑的按钮
		if(buttons[(int)ToolGUI.BUTTON.BUIL_CREATE].current) {

			do {

				// 不允许出现道路未创建的情况
				if(!this.road_creator.is_created) {
					break;
				}

				// 已经生成
				if(this.buil_arranger.is_created) {
					break;
				}

				//

				this.buil_arranger.base_offset = 40.0f;

				this.buil_arranger.createBuildings();

			} while(false);
		}
		// 建筑in 向前按钮
		if(buttons[(int)ToolGUI.BUTTON.BUIL_START_FORWARD].current) {

			if(this.road_creator.is_created) {

				if(!this.buil_arranger.is_created) {

					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.BUIL_START_FORWARD);

					this.buil_arranger.setStart(this.buil_arranger.start + speed);
				}
			}
		}
		// 建筑in 向后按钮
		if(buttons[(int)ToolGUI.BUTTON.BUIL_START_BACKWARD].current) {

			if(this.road_creator.is_created) {

				if(!this.buil_arranger.is_created) {
	
					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.BUIL_START_BACKWARD);

					this.buil_arranger.setStart(this.buil_arranger.start - speed);
				}
			}
		}
		// 建筑out 向前按钮
		if(buttons[(int)ToolGUI.BUTTON.BUIL_END_FORWARD].current) {

			if(this.road_creator.is_created) {

				if(!this.buil_arranger.is_created) {
	
					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.BUIL_END_FORWARD);

					this.buil_arranger.setEnd(this.buil_arranger.end + speed);
				}
			}
		}
		// 建筑out 向后按钮
		if(buttons[(int)ToolGUI.BUTTON.BUIL_END_BACKWARD].current) {

			if(this.road_creator.is_created) {

				if(!this.buil_arranger.is_created) {
	
					float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.BUIL_END_BACKWARD);

					this.buil_arranger.setEnd(this.buil_arranger.end - speed);
				}
			}
		}

		// -------------------------------------------------------------------------------------------- //

		// 创建跳台的按钮
		if(buttons[(int)ToolGUI.BUTTON.JUMP_CREATE].current) {

			do {

				// 不允许出现道路未创建的情况
				if(!this.road_creator.is_created) {
					break;
				}

				// 生成完毕
				if(this.jump_slope_creator.is_created) {
					break;
				}

				//

				this.jump_slope_creator.createJumpSlope();

			} while(false);
		}
		// 移动跳台按钮
		if(this.jump_slope_creator.is_created) {

			if(buttons[(int)ToolGUI.BUTTON.JUMP_FORWARD].current) {

				float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.JUMP_FORWARD);

				this.jump_slope_creator.setPlace(this.jump_slope_creator.place + speed);
			}
			if(buttons[(int)ToolGUI.BUTTON.JUMP_BACKWARD].current) {

				float	speed = this.calc_icon_move_speed(ToolGUI.BUTTON.JUMP_BACKWARD);

				this.jump_slope_creator.setPlace(this.jump_slope_creator.place - speed);
			}

		}

		/*if(this.road_creator.is_created) {

			foreach(RoadCreator.Section section in this.road_creator.sections) {

				Debug.DrawLine(section.positions[0], section.positions[1], Color.red, 0.0f, false);
			}
		}*/

		// -------------------------------------------------------------------------------------------- //

		this.forest_creator.execute();
	}

	// "<<" ">>" 按键被按下时，图标的移动速度
	// （越来越快）
	private float	calc_icon_move_speed(ToolGUI.BUTTON button)
	{
		UIButton[]	buttons = this.tool_gui.buttons;

		float	speed_scale;
		float	speed;

		float	pushed_timer = buttons[(int)button].pushed_timer;

		if(pushed_timer < 0.5f) {

			speed_scale = 1.0f;

		} else if(pushed_timer < 1.0f) {

			speed_scale = Mathf.InverseLerp(0.5f, 1.0f, pushed_timer);
			speed_scale = Mathf.Lerp(1.0f, 4.0f, speed_scale);

		} else {

			speed_scale = 4.0f;
		}

		speed = speed_scale*6.0f*Time.deltaTime;

		return(speed);
	}

	// 生成实际的道路
	private void	create_road()
	{
		do {

			if(!this.line_drawer.isLineDrawed()) {
				break;
			}
			if(this.road_creator.is_created) {
				break;
			}

			// ------------------------------------------------ //
			// 检测线条重合部分

			this.junction_finder.positions = this.line_drawer.positions;
			this.junction_finder.findJunction();

			// ------------------------------------------------ //
			// 将各个立体交叉所在位置，用区块划分
			// 因为从立体交叉处通过时，立交桥下的墙壁碰撞将变的无效

			List<int>		junction_points = new List<int>();

			// 标注起点，终点，交叉的位置

			junction_points.Add(0);

			foreach(JunctionFinder.Junction	junction in this.junction_finder.junctions) {

				junction_points.Add(junction.i0);
				junction_points.Add(junction.i1);
			}

			junction_points.Add(this.line_drawer.positions.Count - 1);

			// 按照赛道的距离排序
			junction_points.Sort();

			// 列出方块的分割点
			// 为了使立体交叉的位置处于方块中

			List<int>	split_points = new List<int>();

			split_points.Add(junction_points[0]);

			for(int i = 0;i < junction_points.Count - 1;i++) {

				split_points.Add((int)Mathf.Lerp((float)junction_points[i], (float)junction_points[i + 1], 1.0f/3.0f));
				split_points.Add((int)Mathf.Lerp((float)junction_points[i], (float)junction_points[i + 1], 2.0f/3.0f));
			}

			split_points.Add(junction_points[junction_points.Count - 1]);

			//

			this.road_creator.split_points = split_points.ToArray();

			// ------------------------------------------------ //
			// 在和立交桥发生相交的位置设置高度

			List<RoadCreator.HeightPeg>		pegs = new List<RoadCreator.HeightPeg>();

			// 起点
			pegs.Add(new RoadCreator.HeightPeg(0, 0.0f));

			foreach(JunctionFinder.Junction junction in this.junction_finder.junctions) {

				// 相交的位置
				pegs.Add(new RoadCreator.HeightPeg(junction.i0, 0.0f));
				pegs.Add(new RoadCreator.HeightPeg(junction.i1, 10.0f));
			}

			// 终点
			pegs.Add(new RoadCreator.HeightPeg(this.line_drawer.positions.Count - 1, 0.0f));

			// 对赛道上的距离进行排序
			pegs.Sort((x, y) => x.position - y.position);

			this.road_creator.height_pegs = pegs.ToArray();

			// ------------------------------------------------ //
			//

			this.road_creator.positions       = this.line_drawer.positions;
			this.road_creator.material        = this.material;
			this.road_creator.road_material   = this.road_material;
			this.road_creator.wall_material   = this.wall_material;
			this.road_creator.physic_material = this.physic_material;
			this.road_creator.peak_position   = this.junction_finder.junction.i0;

			this.road_creator.createRoad();

			// 使鼠标绘出的线条不可见
			//
			this.line_drawer.setVisible(false);

		} while(false);
	}

	// 将鼠标的位置变换为3D空间的世界坐标
	//
	// ・穿过鼠标光标和摄像机位置的直线
	// ・用于检测地面碰撞的平面
	//　↑求出上述二者的交点
	//
	public bool		unproject_mouse_position(out Vector3 world_position, Vector3 mouse_position)
	{
		bool	ret;

		// 用于检测地面碰撞的平面
		Plane	plane = new Plane(Vector3.up, Vector3.zero);

		// 穿过摄像机位置和鼠标光标的直线
		Ray		ray = this.main_camera.GetComponent<Camera>().ScreenPointToRay(mouse_position);

		// 求出上面二者的交点

		float	depth;

		if(plane.Raycast(ray, out depth)) {

			world_position = ray.origin + ray.direction*depth;

			ret = true;

		} else {

			world_position = Vector3.zero;

			ret = false;
		}

		return(ret);
	}

}


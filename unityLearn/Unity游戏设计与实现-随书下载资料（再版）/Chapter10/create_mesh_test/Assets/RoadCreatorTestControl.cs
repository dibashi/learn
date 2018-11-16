using UnityEngine;
using System.Collections;

public class RoadCreatorTestControl : MonoBehaviour {

	// 游戏摄像机
	private	GameObject		game_camera;

	public GameObject		BallPrefab = null;

	public Material			material;
	public PhysicMaterial	physic_material = null;

	private Vector3[]	positions;
	private int			position_num = 0;

	private static int	POSITION_NUM_MAX = 100;

	enum STEP {

		NONE = -1,

		IDLE = 0,		// 空闲中
		DRAWING,		// 画线（拖动过程中）
		DRAWED,			// 画线过程结束
		CREATED,		// 生成了道路模型

		NUM,
	};
	
	private STEP	step      = STEP.NONE;
	private STEP	next_step = STEP.NONE;

	private RoadCreator	road_creator;

	// Use this for initialization
	void Start ()
	{
		// 预先探测出摄像机的实例
		this.game_camera = GameObject.FindGameObjectWithTag("MainCamera");

		this.GetComponent<LineRenderer>().SetVertexCount(0);

		this.positions = new Vector3[POSITION_NUM_MAX];

		this.road_creator = new RoadCreator();
	}

	// Update is called once per frame
	void Update ()
	{
		// 检测状态迁移

		switch(this.step) {

			case STEP.NONE:
			{
				this.next_step = STEP.IDLE;
			}
			break;

			case STEP.IDLE:
			{
				if(Input.GetMouseButton(0)) {

					this.next_step = STEP.DRAWING;
				}
			}
			break;

			case STEP.DRAWING:
			{
				if(!Input.GetMouseButton(0)) {

					if(this.position_num >= 2) {

						this.next_step = STEP.DRAWED;

					} else {

						this.next_step = STEP.IDLE;
					}
				}
			}
			break;
		}

		// 状态迁移时的初始化

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.IDLE:
				{
					// 删除上次生成的物体

					this.road_creator.clearOutput();

					this.position_num = 0;

					this.GetComponent<LineRenderer>().SetVertexCount(0);
				}
				break;

				case STEP.CREATED:
				{
					this.road_creator.positions       = this.positions;
					this.road_creator.position_num    = this.position_num;
					this.road_creator.material        = this.material;
					this.road_creator.physic_material = this.physic_material;
		
					this.road_creator.createRoad();
				}
				break;
			}

			this.step = this.next_step;

			this.next_step = STEP.NONE;
		}

		// 各个状态对应的处理

		switch(this.step) {

			case STEP.DRAWING:
			{
				Vector3	position = this.unproject_mouse_position();

				// 检测顶点是否被添加到线上

				bool	is_append_position = false;

				if(this.position_num == 0) {

					// 最开始的一个被无条件添加

					is_append_position = true;

				} else if(this.position_num >= POSITION_NUM_MAX) {

					// 超过最大个数时将无法添加

					is_append_position = false;

				} else {

					// 添加和上次被添加的顶点距离一定间隔的点

					if(Vector3.Distance(this.positions[this.position_num - 1], position) > 0.5f) {

						is_append_position = true;
					}
				}

				//

				if(is_append_position) {

					if(this.position_num > 0) {

						Vector3	distance = position - this.positions[this.position_num - 1];

						distance *= 0.5f/distance.magnitude;

						position = this.positions[this.position_num - 1] + distance;
					}

					this.positions[this.position_num] = position;

					this.position_num++;

					// 重新生成LineRender 

					this.GetComponent<LineRenderer>().SetVertexCount(this.position_num);

					for(int i = 0;i < this.position_num;i++) {
			
						this.GetComponent<LineRenderer>().SetPosition(i, this.positions[i]);
					}
				}
			}
			break;
		}

		dbPrint.setLocate(5, 5);
		dbPrint.print(this.position_num.ToString());

		/*if(is_created) {

			foreach(Section section in this.sections) {

				Debug.DrawLine(section.positions[0], section.positions[1], Color.red, 0.0f, false);
			}
		}*/
	}

	//  按下“create”按钮
	public void onCreateButtonPressed()
	{
		if(this.step == STEP.DRAWED) {

			this.next_step = STEP.CREATED;
		}
	}

	// 按下“clear”按钮
	public void onClearButtonPressed()
	{
		if(this.step == STEP.DRAWED) {

			this.next_step = STEP.IDLE;
		}
	}

	// 按下“ball”按钮
	public void onBallButtonPressed()
	{
		if(this.step == STEP.CREATED) {

			GameObject ball = Instantiate(this.BallPrefab) as GameObject;

			Vector3	ball_position;

			ball_position = (road_creator.sections[0].center + road_creator.sections[1].center)/2.0f + Vector3.up*1.0f;
 
			ball.transform.position = ball_position;
		}
	}


	// 将鼠标的位置变换为3D空间内的世界坐标
	//
	// ・穿过鼠标光标和摄像机所在位置的直线
	// ・穿过道路中心的水平面
	//　↑求出以上两个物体的交点
	//
	private Vector3	unproject_mouse_position()
	{
		Vector3	mouse_position = Input.mousePosition;

		// 穿过道路中心的水平面（以Y轴为法线的XZ平面）
		Plane	plane = new Plane(Vector3.up, new Vector3(0.0f, 0.0f, 0.0f));

		// 穿过摄像机位置和鼠标光标位置的直线
		Ray		ray = this.game_camera.GetComponent<Camera>().ScreenPointToRay(mouse_position);

		// 求出上面两个物体的交点

		float	depth;

		plane.Raycast(ray, out depth);

		Vector3	world_position;

		world_position = ray.origin + ray.direction*depth;

		return(world_position);
	}
}

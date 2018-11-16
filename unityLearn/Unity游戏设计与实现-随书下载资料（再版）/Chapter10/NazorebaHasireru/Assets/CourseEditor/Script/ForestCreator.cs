using UnityEngine;
using System.Collections;

// 排列树木
public class ForestCreator {

	public RoadCreator		road_creator = null;
	public GameObject		main_camera;
	public ToolGUI			tool_gui = null;

	// 输入

	public GameObject		TreePrefab = null;
	public float			start;
	public float			end;

	public float			place_max;

	//

	public bool				is_created = false;

	// ---------------------------------------------------------------- //

	// 根部
	public GameObject	root_r = null;			// 右侧使用
	public GameObject	root_l = null;			// 左侧使用

	// ---------------------------------------------------------------- //

	// 基准线
	public class BaseLine {

		public Vector3[]	points;				// 控制点
		public float		total_distance;
	};

	protected BaseLine	base_line_r;			// 右侧使用
	protected BaseLine	base_line_l;			// 左侧使用

	// ---------------------------------------------------------------- //

	public float	fluc_amplitude   = 10.0f;		// 基准线的弯曲幅度
	public float	fluc_cycle       = 100.0f;		// 基准线的弯曲周期
	public float	base_offset      = 30.0f;		// 道路中心举例基准线的偏移
	public float	base_pitch       = 20.0f;		// 树木之间的间隔（最窄的距离）
	public float	max_pitch_factor = 5.0f;		// 树木之间的间隔（最宽的距离。倍率）

	protected UIIcon	start_icon;
	protected UIIcon	end_icon;

	// ---------------------------------------------------------------- //
	
	public void		create()
	{
		this.start_icon = this.tool_gui.createForestIcon();
		this.end_icon = this.tool_gui.createForestIcon();

		this.start_icon.setVisible(false);
		this.end_icon.setVisible(false);
	}

	public void		execute()
	{
		if(this.is_created) {

			this.draw_base_line(this.base_line_r);
			this.draw_base_line(this.base_line_l);
		}
	}

	// 绘制基准线（用于调试）
	public void		draw_base_line(BaseLine base_line)
	{
		for(int i = 0;i < base_line.points.Length - 1;i++) {

			Debug.DrawLine(base_line.points[i], base_line.points[i + 1], Color.red, 0.0f, false);
		}
	}

	public void		createForest()
	{
		//

		if(this.start > this.end) {

			float	temp = this.start;
			this.start = this.end;
			this.end   = temp;
		}

		// 生成父级对象

		this.root_r = new GameObject();
		this.root_l = new GameObject();

		this.root_r.name = "Trees(right)";
		this.root_l.name = "Trees(left)";

		//

		this.base_line_r = new BaseLine();
		this.base_line_l = new BaseLine();

		this.base_line_r.points = new  Vector3[(int)this.end - (int)this.start + 1];
		this.base_line_l.points = new  Vector3[(int)this.end - (int)this.start + 1];

		// 右侧

		this.create_base_line(this.base_line_r, (int)this.start, (int)this.end,  this.base_offset, this.fluc_amplitude, this.fluc_cycle);
		this.create_tree_on_line(this.root_r, this.base_line_r);

		// 左侧

		this.create_base_line(this.base_line_l, (int)this.start, (int)this.end, -this.base_offset, this.fluc_amplitude, this.fluc_cycle);
		this.create_tree_on_line(this.root_l, this.base_line_l);

		//

		this.is_created = true;
	}

	// 生成基准线
	public void		create_base_line(BaseLine base_line, int start, int end, float base_offset, float fluc_amp, float fluc_cycle)
	{
		int		n = 0;
		float	offset;

		// 道路中心线上的距离
		float	center_distance = 0.0f;
		
		// 道路截面
		RoadCreator.Section[]	sections = this.road_creator.sections;

		// 基准线上的道路距离
		base_line.total_distance = 0.0f;

		for(int i = start;i <= end;i++) {

			// 道路中心线上的距离
			//
			if(i > start) {

				center_distance += (sections[i].center - sections[i - 1].center).magnitude;
			}

			// -------------------------------------------- //
			// 求出和道路垂直交叉方向的偏移

			offset = base_offset;

			// 按照正弦曲线波动

			float	angle = Mathf.Repeat(center_distance, fluc_cycle)/fluc_cycle*Mathf.PI*2.0f;

			offset += fluc_amp*Mathf.Sin(angle);

			// -------------------------------------------- //

			Vector3	point         = sections[i].center;
			Vector3	offset_vector = sections[i].right;

			point += offset*offset_vector;

			base_line.points[n] = point;

			// 基准线上的道路距离
			//
			if(n > 0) {

				base_line.total_distance += Vector3.Distance(base_line.points[n], base_line.points[n - 1]);
			}

			//

			n++;
		}
	}

	public const float	FADE_LENGTH_SCALE = 0.25f;		// 淡入/淡出的长度参数（占整体的比例）

	// 在基准线上生成树木
	public void		create_tree_on_line(GameObject root, BaseLine base_line)
	{
		float		rate;
		float		pitch = 0.0f;

		float		distance_local = 0.0f;
		Vector3		point_previous = base_line.points[0];
		float		current_distance = 0.0f;
		int			instance_count = 0;
		int			instance_num_max;

		// 树木的间隔（最大值）
		float		max_pitch = this.base_pitch*this.max_pitch_factor;

		// 在基准线上排列树木
		foreach(Vector3 point in base_line.points) {

			Vector3	dir      = point - point_previous;		// 区间的方向
			float	distance = dir.magnitude;				// 区间的长度

			// 单位化失败时（大小等于0），变为zero
			dir.Normalize();

			// 区间（两个控制点之间）内允许生成实例的最大数量
			// （防止bug导致无限循环）
			instance_num_max = Mathf.CeilToInt(distance/this.base_pitch) + 2;

			instance_count = 0;

			while(true) {

				// 如果到下一个控制点的距离小于 pitch，则不列出
				// （到下一个区间中操作）
				if(distance - distance_local < pitch) {

					distance_local -= distance;
					break;
				}

				distance_local   += pitch;		// 当前区间内的前进距离
				current_distance += pitch; 		// 基准线的起点开始前进的距离

				GameObject tree = GameObject.Instantiate(this.TreePrefab) as GameObject;
	
				Vector3	position = point_previous + dir*distance_local;
	
				tree.transform.position = position;
				tree.tag = "Tree";

				tree.transform.parent = root.transform;

				// 更新树木的间隔

				float	fade_length = base_line.total_distance*FADE_LENGTH_SCALE;

				if(current_distance < fade_length) {

					// 从起点开始淡入
					// 间隔逐渐变小
					//
					// 距离  [0         ～ fade_length].
					// pitch[max_pitch ～ base_pitch].

					rate = Mathf.InverseLerp(0.0f, fade_length, current_distance);

					pitch = Mathf.Lerp(max_pitch, this.base_pitch, rate);

				} else if(base_line.total_distance - current_distance < fade_length){

					// 朝终点淡出
					// 间隔逐渐变大
					//
					// 距离  [base_line.total_distance - fade_length ～ base_line.total_distance].
					// pitch[base_pitch                             ～ max_pitch].

					rate = Mathf.InverseLerp(base_line.total_distance - fade_length, base_line.total_distance, current_distance);

					pitch = Mathf.Lerp(this.base_pitch, max_pitch, rate);

				} else {

					// 间隔固定

					pitch = this.base_pitch;
				}

				//

				instance_count++;

				if(instance_count >= instance_num_max) {

					break;
				}
			}

			if(instance_count >= instance_num_max) {

				Debug.LogError("error");
				break;
			}

			//

			point_previous = point;
		}
	}

	// ---------------------------------------------------------------- //

	// 删除生成的所有物体
	public void	clearOutput()
	{
		GameObject.Destroy(this.root_r);
		GameObject.Destroy(this.root_l);

		//

		this.is_created = false;
	}

	// 设定树林的开始点
	public void		setStart(float start)
	{
		this.start = start;
		this.start = Mathf.Clamp(this.start, 0.0f, place_max);

		Vector3		start_position = this.road_creator.getPositionAtPlace(this.start);

		//

		Vector3		screen_position = this.main_camera.GetComponent<Camera>().WorldToScreenPoint(start_position);

		screen_position -= new Vector3(Screen.width/2.0f, Screen.height/2.0f, 0.0f);

		this.start_icon.setPosition(screen_position);
	}

	// 设定树林的结束点
	public void		setEnd(float end)
	{
		this.end = end;
		this.end = Mathf.Clamp(this.end, 0.0f, place_max);

		Vector3		end_position = this.road_creator.getPositionAtPlace(this.end);


		//

		Vector3		screen_position = this.main_camera.GetComponent<Camera>().WorldToScreenPoint(end_position);

		screen_position -= new Vector3(Screen.width/2.0f, Screen.height/2.0f, 0.0f);

		this.end_icon.setPosition(screen_position);
	}

	// 设置图标的显示／隐藏
	public void		setIsDrawIcon(bool sw)
	{
		this.start_icon.setVisible(sw);
		this.end_icon.setVisible(sw);

		if(sw) {

			this.setStart(this.start);
			this.setEnd(this.end);
		}
	}

}

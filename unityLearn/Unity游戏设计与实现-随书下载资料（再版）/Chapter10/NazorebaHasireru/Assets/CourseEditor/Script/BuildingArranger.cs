using UnityEngine;
using System.Collections;

public class BuildingArranger {

	public RoadCreator		road_creator = null;
	public GameObject		main_camera;
	public ToolGUI			tool_gui = null;

	// 输入

	public GameObject[]		BuildingPrefabs;
	public float			start;
	public float			end;
	public float			base_offset;

	public float			place_max;

	//


	public bool				is_created = false;
	public bool				is_draw_icon = false;

	public class BaseLine {

		public Vector3[]	points;
		public float		total_distance;
	};

	// 道路的右侧／左侧
	public enum SIDE {

		NONE = -1,

		RIGHT = 0,
		LEFT,

		NUM,
	};

	public Bounds[]			mesh_bounds;

	// ---------------------------------------------------------------- //

	protected UIIcon	start_icon;
	protected UIIcon	end_icon;

	// ---------------------------------------------------------------- //

	public void		create()
	{
		this.start_icon = this.tool_gui.createBuildingIcon();
		this.end_icon = this.tool_gui.createBuildingIcon();

		this.start_icon.setVisible(false);
		this.end_icon.setVisible(false);
	}

	public void		createBuildings()
	{
		if(this.start > this.end) {

			float	temp = this.start;
			this.start = this.end;
			this.end   = temp;
		}

		//

		BaseLine	base_line = new BaseLine();

		base_line.points = new  Vector3[(int)this.end - (int)this.start + 1];

		//

		this.mesh_bounds = new Bounds[this.BuildingPrefabs.Length];

		for(int i = 0;i < this.BuildingPrefabs.Length;i++) {

			GameObject	go = GameObject.Instantiate(this.BuildingPrefabs[i]) as GameObject;

			if(go.GetComponent<MeshFilter>() != null) {

				this.mesh_bounds[i] = go.GetComponent<MeshFilter>().mesh.bounds;
			}

			MeshFilter[]	filters = go.GetComponentsInChildren<MeshFilter>();

			foreach(var filter in filters) {

				this.mesh_bounds[i].Encapsulate(filter.mesh.bounds);
			}

			GameObject.Destroy(go);
		}

		//

		this.create_base_line(base_line, (int)this.start, (int)this.end,  this.base_offset, 0.0f, 400.0f);
		this.create_buildings_on_line(base_line, SIDE.RIGHT);

		//
		
		this.create_base_line(base_line, (int)this.start, (int)this.end, -this.base_offset, 0.0f, 400.0f);
		this.create_buildings_on_line(base_line, SIDE.LEFT);

		//

		this.is_created = true;
	}

	public void		create_base_line(BaseLine base_line, int start, int end, float base_offset, float fluc_amp, float fluc_cycle)
	{
		int		index = 0;
		float	offset;

		base_line.total_distance = 0.0f;

		for(int i = start;i <= end;i++) {

			Vector3	point  = this.road_creator.sections[i].center;

			Vector3	offset_vector = this.road_creator.sections[i].right;

			offset = base_offset;

			offset += (fluc_amp/2.0f)*Mathf.Sin(Mathf.Repeat(base_line.total_distance, fluc_cycle));

			point += offset*offset_vector;

			base_line.points[index] = point;

			if(index > 0) {

				base_line.total_distance += Vector3.Distance(base_line.points[index], base_line.points[index - 1]);
			}

			//

			index++;
		}
	}

	// 在曲线上生成建筑
	public void		create_buildings_on_line(BaseLine base_line, SIDE side)
	{
		float		pitch = 40.0f;

		float		distance_local = 0.0f;
		Vector3		point_previous = base_line.points[0];
		float		current_distance = 0.0f;
		int			model_sel = 0;
		int			model_sel_next = 0;
		int			instance_count;

		// 控制点之间允许生成实例的最大数量
		// （为了防止bug导致无限循环）
		int			instance_num_max = 10;

		pitch = this.mesh_bounds[model_sel].size.z/2.0f;

		// 创建父对象

		GameObject	root = new GameObject();

		root.name = "Buildings";

		foreach(Vector3 point in base_line.points) {

			Vector3	dir      = point - point_previous;
			float	distance = dir.magnitude;
			Vector3	dir_xz;

			// 单位化失败时（大小等于0），变为zero
			dir.Normalize();

			dir_xz = dir;
			dir_xz.y = 0.0f;
			dir_xz.Normalize();

			instance_count = 0;

			while(true) {

				if(distance - distance_local < pitch) {

					distance_local -= distance;
					break;
				}

				distance_local   += pitch;
				current_distance += pitch;

				GameObject	instance = GameObject.Instantiate(this.BuildingPrefabs[model_sel]) as GameObject;
	
				Vector3		position = point_previous + dir*distance_local;
				Quaternion	rotation;

				if(side == SIDE.RIGHT) {

					rotation = Quaternion.FromToRotation(Vector3.back, dir_xz);

				} else {

					rotation = Quaternion.FromToRotation(Vector3.forward, dir_xz);
				}

				instance.transform.position = position;
				instance.transform.rotation = rotation;

				instance.tag = "Building";
				instance.name = "building" + instance_count;

				instance.transform.parent = root.transform;

				//

				float	fade_length = base_line.total_distance*0.25f;

				if(current_distance < fade_length) {

					pitch = Mathf.Lerp(200.0f*(fade_length/800.0f), 40.0f, current_distance/fade_length);

				} else if(base_line.total_distance - current_distance < fade_length){

					pitch = Mathf.Lerp(200.0f*(fade_length/800.0f), 40.0f, (base_line.total_distance - current_distance)/fade_length);

				} else {

					pitch = 40.0f;
				}

				//

				model_sel_next = (model_sel + 1)%this.BuildingPrefabs.Length;

				if(side == SIDE.RIGHT) {

					pitch = this.mesh_bounds[model_sel].size.z;

				} else {

					pitch = this.mesh_bounds[model_sel_next].size.z;
				}

				pitch += 5.0f;

				model_sel = model_sel_next;

				//

				instance_count++;

				if(instance_count >= instance_num_max) {

					break;
				}
			}

			if(instance_count >= instance_num_max) {

				break;
			}

			//

			point_previous = point;
		}
	}

	// ---------------------------------------------------------------- //

	// 清除生成的所有物体
	public void	clearOutput()
	{
		GameObject[]	trees = GameObject.FindGameObjectsWithTag("Building");

		foreach(var tree in trees) {

			GameObject.Destroy(tree);
		}

		//

		this.is_created = false;
	}

	public void		setStart(float start)
	{
		this.start = start;

		this.start = Mathf.Clamp(this.start, 0.0f, place_max);

		Vector3		start_position = this.road_creator.getPositionAtPlace(this.start);
		Vector3		screen_position = this.main_camera.GetComponent<Camera>().WorldToScreenPoint(start_position);

		screen_position -= new Vector3(Screen.width/2.0f, Screen.height/2.0f, 0.0f);

		this.start_icon.setPosition(screen_position);

	}
	public void		setEnd(float end)
	{
		this.end = end;
		this.end = Mathf.Clamp(this.end, 0.0f, place_max);

		//

		Vector3		end_position = this.road_creator.getPositionAtPlace(this.end);
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

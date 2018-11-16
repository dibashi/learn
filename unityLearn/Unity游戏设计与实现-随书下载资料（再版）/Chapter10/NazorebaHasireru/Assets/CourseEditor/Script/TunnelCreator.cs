using UnityEngine;
using System.Collections;

public class TunnelCreator {

	public GameObject		TunnelPrefab = null;
	public GameObject		main_camera;
	public ToolGUI			tool_gui = null;

	public RoadCreator		road_creator = null;

	public GameObject		instance = null;

	public float			place = 0.0f;

	public float			place_min = 0.0f;
	public float			place_max = 0.0f;

	public Vector3[]		vertices_org;			// 原来的形状

	public float			mesh_length;			// Z方向的长度

	public bool				is_created = false;		// 创建隧道？

	// ---------------------------------------------------------------- //

	protected UIIcon	icon;

	// ------------------------------------------------------------------------ //

	public void		create()
	{
		this.icon = this.tool_gui.createTunnelIcon();

		this.icon.setVisible(false);
	}

	// 设置隧道的位置
	public void	setPlace(float place)
	{
		this.place = place;
		this.place = Mathf.Clamp(this.place, this.place_min, this.place_max);

		if(this.is_created) {

			this.modifyShape();

			//

			Vector3		screen_position = this.main_camera.GetComponent<Camera>().WorldToScreenPoint(this.instance.transform.position);

			screen_position -= new Vector3(Screen.width/2.0f, Screen.height/2.0f, 0.0f);

			this.icon.setPosition(screen_position);
		}
	}

	public void modifyShape()
	{
		Mesh mesh = this.instance.GetComponent<MeshFilter>().mesh;

		Vector3[] vertices = mesh.vertices;

		for(int i = 0;i < vertices.Length;i++) {

			vertices[i] = this.vertices_org[i];

			float	z = this.place;

			// 将Z坐标变换到道路中心线上的位置
			// 整数部分⋯⋯ 控制点的索引位置
			// 小数部分⋯⋯ 控制点间的距离比例

			z += vertices[i].z/RoadCreator.PolygonSize.z;

			int		place_i = (int)z; 				// 整数部分⋯⋯ 控制点的索引位置
			float	place_f = z - (float)place_i;	// 小数部分⋯⋯ 控制点间的距离比例

			if(place_i >= this.road_creator.sections.Length - 1) {

				place_i = this.road_creator.sections.Length - 1 - 1;
				place_f = 1.0f;
			}

			RoadCreator.Section		section_prev = this.road_creator.sections[place_i];
			RoadCreator.Section		section_next = this.road_creator.sections[place_i + 1];

			// 旋转使Z轴和道路中心线保持相同方向

			vertices[i].z = 0.0f;
			vertices[i] = Quaternion.LookRotation(section_prev.direction, section_prev.up)*vertices[i];

			// 对前后控制点之间的小数部分进行补间

			vertices[i] += Vector3.Lerp(section_prev.center, section_next.center, place_f);
		}

		//
		{
			int		place_i = (int)place;
			float	place_f = place - (float)place_i;

			RoadCreator.Section		section_prev = this.road_creator.sections[place_i];
			RoadCreator.Section		section_next = this.road_creator.sections[place_i + 1];

			this.instance.transform.position = Vector3.Lerp(section_prev.center, section_next.center, place_f);
			this.instance.transform.rotation = Quaternion.LookRotation(section_prev.direction, section_prev.up);

			for(int i = 0;i < vertices.Length;i++) {

				vertices[i] = this.instance.transform.InverseTransformPoint(vertices[i]);
			}
		}

		//

		mesh.vertices = vertices;
	}
	public void	createTunnel()
	{
		this.instance = GameObject.Instantiate(this.TunnelPrefab) as GameObject;

		Mesh mesh = this.instance.GetComponent<MeshFilter>().mesh;

		this.vertices_org = mesh.vertices;


		this.mesh_length = 0.0f;

		foreach(Vector3 vertex in this.vertices_org) {

			this.mesh_length = Mathf.Max(this.mesh_length, vertex.z);
		}

		this.place_min = 0.0f;
		this.place_max = (float)this.road_creator.sections.Length - 1.0f;
		this.place_max -= this.mesh_length/RoadCreator.PolygonSize.z;

		//

		this.modifyShape();

		//

		this.is_created = true;

		this.setPlace(this.place_min);

		// アイコンを表示する.
		this.setIsDrawIcon(true);
	}

	// 删除生成的所有对象
	public void		clearOutput()
	{
		if(this.is_created) {

			GameObject.Destroy(this.instance);
	
			this.vertices_org = null;
			this.mesh_length = 0.0f;
			this.place = 0.0f;
	
			this.is_created = false;
		}
	}

	// 设置图标的显示／隐藏
	public void		setIsDrawIcon(bool sw)
	{
		this.icon.setVisible(sw);
	}
}

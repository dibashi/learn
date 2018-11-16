using UnityEngine;
using System.Collections;

public class RoadCreator {

	// 输入

	public Vector3[]	positions = null;
	public int			position_num = 0;

	public Material			material = null;
	public PhysicMaterial	physic_material = null;

	// 生成物

	public GameObject	road_mesh = null;
	public GameObject[]	wall_mesh = null;

	// 副产品

	public struct Section {

		public Vector3			center;
		public Vector3			direction;
		public Vector3[]		positions;
	};

	public Section[]	sections;

	//

	public bool			is_created = false;

	private enum WALL_SIDE {

		NONE = -1,

		LEFT = 0,
		RIGHT,

		NUM,
	};

	// 生成道路模型（用于显示的形状，以及碰撞体）
	public void	createRoad()
	{
		// ------------------------------------------------------------ //
		// 生成截面形状

		this.sections = new Section[this.position_num];

		for(int i = 0;i < this.position_num;i++) {

			this.sections[i].positions = new Vector3[2];
		}

		float	height_max = this.position_num*0.1f;

		for(int i = 0;i < this.position_num;i++) {

			this.sections[i].center = this.positions[i];

			this.sections[i].center.y += Mathf.Lerp(height_max, 0.0f, (float)i/(float)(this.position_num - 1));
		}

		for(int i = 0;i < this.position_num;i++) {

			if(i < this.position_num - 1) {

				this.sections[i].direction = this.sections[i + 1].center - this.sections[i].center;

			} else {

				this.sections[i].direction = this.sections[i].center - this.sections[i - 1].center;
			}

			this.sections[i].direction.y = 0.0f;
			this.sections[i].direction.Normalize();

			Vector3	right =  Quaternion.AngleAxis(90.0f, Vector3.up)*this.sections[i].direction;

			float	width = 0.5f;

			this.sections[i].positions[0] = this.sections[i].center - right*width/2.0f;
			this.sections[i].positions[1] = this.sections[i].center + right*width/2.0f;

		}

		// ------------------------------------------------------------ //

		this.road_mesh = new GameObject();
		this.wall_mesh = new GameObject[2];
		this.wall_mesh[0] = new GameObject();
		this.wall_mesh[1] = new GameObject();

		this.road_mesh.name = "Road";
		this.wall_mesh[0].name = "Wall(Left)";
		this.wall_mesh[1].name = "Wall(Right)";
		
		//

		this.create_ground_mesh(this.road_mesh);
		this.create_wall_mesh(this.wall_mesh[0], WALL_SIDE.LEFT);
		this.create_wall_mesh(this.wall_mesh[1], WALL_SIDE.RIGHT);

		this.wall_mesh[0].GetComponent<MeshFilter>().mesh.name += "(Left)";
		this.wall_mesh[1].GetComponent<MeshFilter>().mesh.name += "(Right)";

		// 设置PhysicMaterial

		this.road_mesh.GetComponent<Collider>().material    = this.physic_material;
		this.wall_mesh[0].GetComponent<Collider>().material = this.physic_material;
		this.wall_mesh[1].GetComponent<Collider>().material = this.physic_material;

		// 预先生成内侧多边形（因为要绘制两面）

		this.add_backface_trianbles_to_mesh(this.road_mesh.GetComponent<MeshFilter>().mesh);
		this.add_backface_trianbles_to_mesh(this.wall_mesh[0].GetComponent<MeshFilter>().mesh);
		this.add_backface_trianbles_to_mesh(this.wall_mesh[1].GetComponent<MeshFilter>().mesh);

		//

		this.is_created = true;
	}

	// 删除所有的生成物
	public void clearOutput()
	{
		if(this.is_created) {

			GameObject.Destroy(this.road_mesh);
			GameObject.Destroy(this.wall_mesh[0]);
			GameObject.Destroy(this.wall_mesh[1]);

			this.sections = null;

			this.positions = null;
			this.position_num = 0;
		}
	}


	private void create_wall_mesh(GameObject game_object, WALL_SIDE side)
	{
		game_object.AddComponent<MeshFilter>();
		game_object.AddComponent<MeshRenderer>();
		game_object.AddComponent<MeshCollider>();

		MeshFilter		mesh_filter   = game_object.GetComponent<MeshFilter>();
		MeshCollider	mesh_collider = game_object.GetComponent<MeshCollider>();
		Mesh			mesh          = mesh_filter.mesh;
		MeshRenderer	render        = game_object.GetComponent<MeshRenderer>();

		//

		mesh.Clear();
		mesh.name = "WallMesh";

		Vector3[]	vertices  = new Vector3[position_num*2];
		Vector2[]	uvs       = new Vector2[position_num*2];
		int[]		triangles = new int[(position_num - 1)*2*3];

		float	height = 0.1f;

		if(side == WALL_SIDE.LEFT) {

			for(int i = 0;i < position_num;i++) {

				vertices[i*2 + 0] = this.sections[i].positions[0];
				vertices[i*2 + 1] = this.sections[i].positions[0] + Vector3.up*height;

				uvs[i*2 + 0] = new Vector2(0.0f, (float)i/(float)(this.position_num - 1));
				uvs[i*2 + 1] = new Vector2(1.0f, (float)i/(float)(this.position_num - 1));
			}


		} else {

			for(int i = 0;i < position_num;i++) {

				vertices[i*2 + 0] = this.sections[i].positions[1];
				vertices[i*2 + 1] = this.sections[i].positions[1] + Vector3.up*height;

				uvs[i*2 + 0] = new Vector2(0.0f, (float)i/(float)(this.position_num - 1));
				uvs[i*2 + 1] = new Vector2(1.0f, (float)i/(float)(this.position_num - 1));
			}
		}


		int		position_index = 0;

		if(side == WALL_SIDE.LEFT) {

			for(int i = 0;i < this.position_num - 1;i++) {
	
				triangles[position_index++] = i*2 + 0;
				triangles[position_index++] = i*2 + 1;
				triangles[position_index++] = (i + 1)*2 + 1;
	
				triangles[position_index++] = (i + 1)*2 + 1;
				triangles[position_index++] = (i + 1)*2 + 0;
				triangles[position_index++] = i*2 + 0;
			}

		} else {

			for(int i = 0;i < this.position_num - 1;i++) {
	
				triangles[position_index++] = i*2 + 1;
				triangles[position_index++] = i*2 + 0;
				triangles[position_index++] = (i + 1)*2 + 0;
	
				triangles[position_index++] = (i + 1)*2 + 0;
				triangles[position_index++] = (i + 1)*2 + 1;
				triangles[position_index++] = i*2 + 1;
			}
		}

		//
		// 因为会出现警告信息，所以这里也提前把uv生成（值没有特定要求）

		mesh.vertices  = vertices;
		mesh.uv        = uvs;
		mesh.uv2       = uvs;
		mesh.triangles = triangles;

		mesh.Optimize();
		mesh.RecalculateNormals();

		render.material = this.material;

		if(side == WALL_SIDE.LEFT) {

			render.material.color = Color.green;

		} else {

			render.material.color = Color.blue;
		}

		mesh_collider.sharedMesh = mesh;
		mesh_collider.enabled = true;

		//
	}

	private void create_ground_mesh(GameObject game_object)
	{
		game_object.AddComponent<MeshFilter>();
		game_object.AddComponent<MeshRenderer>();
		game_object.AddComponent<MeshCollider>();

		MeshFilter		mesh_filter   = game_object.GetComponent<MeshFilter>();
		MeshCollider	mesh_collider = game_object.GetComponent<MeshCollider>();
		Mesh			mesh          = mesh_filter.mesh;
		MeshRenderer	render        = game_object.GetComponent<MeshRenderer>();

		//

		mesh.Clear();
		mesh.name = "GroundMesh";

		Vector3[]	vertices  = new Vector3[position_num*2];
		Vector2[]	uvs       = new Vector2[position_num*2];
		int[]		triangles = new int[(position_num - 1)*2*3];

		for(int i = 0;i < position_num;i++) {

			vertices[i*2 + 0] = this.sections[i].positions[0];
			vertices[i*2 + 1] = this.sections[i].positions[1];

			uvs[i*2 + 0] = new Vector2(0.0f, (float)i/(float)(this.position_num - 1));
			uvs[i*2 + 1] = new Vector2(1.0f, (float)i/(float)(this.position_num - 1));
		}

		int		position_index = 0;

		for(int i = 0;i < this.position_num - 1;i++) {

			triangles[position_index++] = i*2 + 1;
			triangles[position_index++] = i*2 + 0;
			triangles[position_index++] = (i + 1)*2 + 0;

			triangles[position_index++] = (i + 1)*2 + 0;
			triangles[position_index++] = (i + 1)*2 + 1;
			triangles[position_index++] = i*2 + 1;
		}

		//
		// 因为会出现警告信息，所以这里也提前把uv生成（值没有特定要求）

		mesh.vertices  = vertices;
		mesh.uv        = uvs;
		mesh.uv2       = uvs;
		mesh.triangles = triangles;

		mesh.Optimize();
		mesh.RecalculateNormals();

		render.material = this.material;
		render.material.color = Color.red;

		mesh_collider.sharedMesh = mesh;
		mesh_collider.enabled = true;

		//
	}

	// 把内侧面追加到mesh
	private void	add_backface_trianbles_to_mesh(Mesh mesh)
	{
		int 	face_num = mesh.triangles.Length/3;

		int[] faces = new int[face_num*3*2];

		for(int i = 0;i < face_num;i++) {

			faces[i*3 + 0] = mesh.triangles[i*3 + 0];
			faces[i*3 + 1] = mesh.triangles[i*3 + 1];
			faces[i*3 + 2] = mesh.triangles[i*3 + 2];
		}

		for(int i = 0;i < face_num;i++) {

			faces[(face_num + i)*3 + 0] = mesh.triangles[i*3 + 2];
			faces[(face_num + i)*3 + 1] = mesh.triangles[i*3 + 1];
			faces[(face_num + i)*3 + 2] = mesh.triangles[i*3 + 0];
		}

		mesh.triangles = faces;
	}
}

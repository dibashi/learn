using UnityEngine;
using System.Collections;

public class DaishaControlParent : MonoBehaviour {

	private Vector3		base_position = Vector3.zero;

	private struct ObjectInfo {

		public GameObject	go;
		public float		angle;

		public Vector3		init_position;
	};

	private	ObjectInfo	u_frame;
	private ObjectInfo	panel;


	// Use this for initialization
	void Start ()
	{
		this.base_position = this.transform.position;

		this.u_frame.go    = this.gameObject.transform.FindChild("u_frame").gameObject;
		this.u_frame.angle = 0.0f;

		this.panel.go    = this.u_frame.go.transform.FindChild("panel").gameObject;
		this.panel.angle = 0.0f;
	}
	
	// Update is called once per frame
	void Update ()
	{
		this.get_input();

		this.transform.position = this.base_position;
		this.u_frame.go.transform.localRotation = Quaternion.AngleAxis(this.u_frame.angle, Vector3.forward);
		this.panel.go.transform.localRotation = Quaternion.AngleAxis(this.panel.angle, Vector3.forward);
	}

	// 获取键盘输入
	private void	get_input()
	{

		float	base_move_speed = 2.0f;

		if(Input.GetKey(KeyCode.LeftArrow)) {

			this.base_position.x -= base_move_speed*Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.RightArrow)) {

			this.base_position.x += base_move_speed*Time.deltaTime;
		}

		//

		if(Input.GetKey(KeyCode.DownArrow)) {

			this.u_frame.angle -= 60.0f*Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.UpArrow)) {

			this.u_frame.angle += 60.0f*Time.deltaTime;
		}

		//

		if(Input.GetKey(KeyCode.Z)) {

			this.panel.angle += 60.0f*Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.X)) {

			this.panel.angle -= 60.0f*Time.deltaTime;
		}
	}	

}

using UnityEngine;
using System.Collections;

public class ToolCameraControl : MonoBehaviour {

	public Vector3		initial_position;
	public Quaternion	initial_rotation;

	// Use this for initialization
	void Start () {

		this.initial_position = this.transform.position;
		this.initial_rotation = this.transform.rotation;
	}
	
	// Update is called once per frame
	void LateUpdate () {
	
		this.transform.position = this.initial_position;
		this.transform.rotation = this.initial_rotation;
	}

	public void	setEnable(bool sw)
	{
		this.enabled = sw;

		if(this.enabled) {

			this.transform.position = this.initial_position;
			this.transform.rotation = this.initial_rotation;
		}
	}
}

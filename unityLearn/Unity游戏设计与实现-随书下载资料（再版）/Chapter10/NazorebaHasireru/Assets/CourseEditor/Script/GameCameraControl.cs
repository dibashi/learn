using UnityEngine;
using System.Collections;

public class GameCameraControl : MonoBehaviour {

	public RoadCreator		road_creator = null;

	public float			place = 0.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update()
	{
		Vector3		eye, interest;
		Quaternion	rotation;

		interest = this.road_creator.getPositionAtPlace(this.place);

		rotation = this.road_creator.getSmoothRotationAtPlace(this.place);

		eye = new Vector3(0.0f, 0.1f, -0.5f);

		eye = rotation*eye;

		eye += interest;

		this.transform.position = eye;
		this.transform.LookAt(interest);

	}
}

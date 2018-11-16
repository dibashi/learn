using UnityEngine;
using System.Collections;

public class ShutterControl : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void	setX(float x)
	{
		Vector3		position = this.transform.localPosition;

		position.x = x;

		this.transform.localPosition = position;
	}
}

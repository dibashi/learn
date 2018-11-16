using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	protected float 	jump_speed = 12.0f;
	public bool			is_landing = false;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if(this.is_landing) {

			if(Input.GetMouseButtonDown(0)) {

				this.is_landing = false;
				this. GetComponent<Rigidbody>().velocity = Vector3.up*this.jump_speed;
			}
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.tag == "Floor") {

			this.is_landing = true;
		}
	}
}

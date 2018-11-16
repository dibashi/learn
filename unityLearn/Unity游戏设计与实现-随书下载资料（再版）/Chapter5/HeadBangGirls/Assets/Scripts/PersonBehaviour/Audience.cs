using UnityEngine;
using System.Collections;

//观众的行为
public class Audience : MonoBehaviour {
	public void Jump(){
		GetComponent<SimpleActionMotor>().Jump();
	}
	// Use this for initialization
	void Start () {
	}
	// Update is called once per frame
	void Update () {
	}
}

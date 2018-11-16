using UnityEngine;
using System.Collections;

public class Launcher : MonoBehaviour {

	public GameObject	ballPrefab;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if(Input.GetMouseButtonDown(1)) {

			Instantiate(this.ballPrefab);	
		}	
	}
}

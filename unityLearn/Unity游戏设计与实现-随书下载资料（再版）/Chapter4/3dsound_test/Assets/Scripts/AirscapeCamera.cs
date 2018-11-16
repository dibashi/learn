using UnityEngine;
using System.Collections;

public class AirscapeCamera : MonoBehaviour {

    [SerializeField]
    private string targetTag;

    private GameObject target;

	// Use this for initialization
	void Start () {
        target = GameObject.FindWithTag(targetTag);
	}
	
	// Update is called once per frame
	void Update () {
        if (target)
        {
            transform.position = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
        }
    }
}

using UnityEngine;
using System.Collections;

public class Effect : MonoBehaviour {

	void Start ()
	{

	}
	
	void Update ()
	{

		// 播放结束后销毁
		if(!this.GetComponentInChildren<ParticleSystem>().isPlaying) {

			Destroy(this.gameObject);
		}
	}
}

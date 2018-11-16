using UnityEngine;
using System.Collections;

public class FruitEffect : MonoBehaviour {

	void Start ()
	{

	}
	
	void Update ()
	{

		// 播放完成后消除
		if(!this.GetComponentInChildren<ParticleSystem>().isPlaying) {

			Destroy(this.gameObject);
		}
	}
}

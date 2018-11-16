using UnityEngine;
using System.Collections;

public class OniStillBodyControl : MonoBehaviour {


	public OniEmitterControl	emitter_control = null;

	void 	Start()
	{
	}
	
	void 	Update()
	{
	}

	void	OnCollisionEnter(Collision other)
	{
		if(other.gameObject.tag == "OniYama") {

			// 和怪物堆接触时发出的声音
			this.emitter_control.PlayHitSound();

			// 这里如果直接播放SE的话会造成各个音效时间间隔较短发生重叠
			// 不容易听清楚，因此通过OniEmitterControl 按适当的间隔发声
		}

		if(other.gameObject.tag == "Floor") {

			// 为了要停止物理计算，采取将rigidbody组件删除的办法
			// 可能不是那么合理，不过Sleep()的话速度会比较慢
			Destroy(this.GetComponent<Rigidbody>());
		}
	}
}

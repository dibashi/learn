using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 销毁播放声音的对象
//  － （因为加入了噪音）播放完毕后销毁
// ----------------------------------------------------------------------------
public class AudioBreaker : MonoBehaviour {
	
	private bool isDestroy = false;
	
	void Update ()
	{
		// 只在销毁时处理
		if ( isDestroy )
		{
			// 确认声音是否播放完毕
			if ( !GetComponent<AudioSource>().isPlaying )
			{
				// 销毁
				Destroy( this.gameObject );
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 开始销毁
	// ------------------------------------------------------------------------
	public void SetDestroy()
	{
		isDestroy = true;
	}
}

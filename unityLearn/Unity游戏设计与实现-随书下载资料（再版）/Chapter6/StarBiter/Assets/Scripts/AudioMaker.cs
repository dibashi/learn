using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 用于控制同时播放多个声音
//  - 设置同时播放的最大数量
//  - 消除声音停止时产生的噪音
// ----------------------------------------------------------------------------
public class AudioMaker : MonoBehaviour {

	public int maxPlayingCount = 0;						// 同时播放的
	public GameObject audioObject;						// 播放的音频
	
	private bool initialized = false;					// 初期化済み.
	private GameObject[] AudioInstances;				// 再生する音のインスタンス.
	
	void Start() 
	{
		// 没有声音则结束
		if ( !audioObject )
		{
			return;
		}
			
		// 准备要播放的声音（准备的数量不超过最多能同时播放的数量）
		AudioInstances = new GameObject[maxPlayingCount];
		for( int i = 0; i < maxPlayingCount; i++ )
		{
			GameObject audioInstance = Instantiate(
				audioObject,
				Vector3.zero,
				new Quaternion( 0, 0, 0, 0 ) ) as GameObject;
			AudioInstances[i] = audioInstance;
		}
		
		// 保持初始化结束的状态
		if ( AudioInstances.Length > 0 )
		{
			initialized = true;
		}
	}
	
	// 播放音频
	public void Play( GameObject target )
	{
		if ( initialized && target )
		{
			bool canPlay = false;
			for( int i = 0; i < maxPlayingCount; i++ )
			{
				AudioSource audioSource = AudioInstances[i].GetComponent<AudioSource>();
				
				// 检测是否为空（未播放的声音）
				if ( !audioSource.isPlaying )
				{
					// 播放声音
					canPlay = true;
					audioSource.Play();
					break;
				}
			}
			// 已经达到了允许同时播放的最大数则无法播放
			if ( !canPlay )
			{
				// 停止一个正在播放的音频，播放一个新的
				
				// ------------------------------------------------------------
				// 噪音对策
				//  - 由于对Audio执行Stop（或者在播放过程中再次Play）后会产生噪音，采取以下对策
				//    1. Mute一个正在播放的声音
				//    2. 生成新的对象
				//    3. 播放声音
				// ------------------------------------------------------------
				
				// Mute，销毁
				AudioInstances[0].GetComponent<AudioSource>().mute = true;
				AudioInstances[0].GetComponent<AudioBreaker>().SetDestroy();
				
				// 将前面的元素删除后，将数组内容往前移动
				for( int i = 0; i < maxPlayingCount - 1; i++ )
				{
					AudioInstances[i] = AudioInstances[i + 1];
				}
				
				// 生成新对象，将其添加为最后的元素，播放声音
				GameObject audioInstance = Instantiate(
					audioObject,
					Vector3.zero,
					new Quaternion( 0, 0, 0, 0 ) ) as GameObject;
				AudioInstances[maxPlayingCount - 1] = audioInstance;
				AudioInstances[maxPlayingCount - 1].GetComponent<AudioSource>().Play();
			}
		}
	}
}

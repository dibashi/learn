using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 播放损坏时的特效和音效
// ----------------------------------------------------------------------------
public class EffectDamage : MonoBehaviour {

	private ParticleSystem[] effects = new ParticleSystem[2];	// 播放的特效
	private int endOfPlayingCount = 0;					// 播放结束的数量
	private int playingCount = 0;						// 开始播放的数量
	
	private AudioMaker damageAudioMaker;				// 损坏音效生成器
	
	void Start ()
	{	
		// 获取损坏音效生成器的实例
		damageAudioMaker =
			GameObject.FindGameObjectWithTag("EffectDamageAudioMaker")
				.GetComponent<AudioMaker>();
		
		// 播放音效
		if ( damageAudioMaker )
		{
			damageAudioMaker.Play( this.gameObject );
		}
		
		// 获取子对象中存在的所有特效对象
		effects = GetComponentsInChildren<ParticleSystem>();
		
		// 播放所有的特效
		for( int i = 0; i < effects.Length; i++ )
		{
			if ( effects[i] )
			{
				effects[i].Play();
				playingCount++;
			}
		}
	}
	
	void Update () 
	{
		// 统计播放结束的特效
		for( int i = 0; i < effects.Length; i++ )
		{
			if ( !effects[i].isPlaying )
			{
				endOfPlayingCount++;
			}
		}
				
		// 全部播放完毕后，销毁对象
		if ( endOfPlayingCount >= playingCount )
		{
			Destroy( this.gameObject );
		}
		
	}
}

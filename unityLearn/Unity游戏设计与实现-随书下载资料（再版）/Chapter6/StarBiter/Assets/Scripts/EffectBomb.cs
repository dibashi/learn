using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 播放爆炸的特效和声音
// ----------------------------------------------------------------------------
public class EffectBomb : MonoBehaviour {
	
	private ParticleSystem[] effects = new ParticleSystem[2];	// 播放的特效
	private int endOfPlayingCount = 0;					// 播放结束的数量
	private int playingCount = 0;						// 开始播放的数量
	
	private AudioMaker bombAudioMaker;					// 爆炸声生成器
	private BattleSpaceController battleSpaceContoller;	// 战斗空间
	
	private float speed = 0f;
	private bool isMoving = false;
	
	void Start () {
		
		// 取得战斗空间的实例
		battleSpaceContoller =
			GameObject.FindGameObjectWithTag("BattleSpace")
				.GetComponent<BattleSpaceController>();
		
		// 取得爆炸声音生成器的实例
		bombAudioMaker = 
			GameObject.FindGameObjectWithTag("EffectBombAudioMaker")
				.GetComponent<AudioMaker>();
		
		// 播放声音
		if ( bombAudioMaker )
		{
			bombAudioMaker.Play( this.gameObject );
		}
		
		// 获取子对象中的特效对象
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
		// 叠加销毁前的运动
		if ( isMoving )
		{
			transform.Translate( transform.forward * speed * Time.deltaTime );
			// 慢慢减缓速度
			if ( speed > 0 )
			{
				speed -= 0.1f;
			}
		}
		
		// 叠加战斗空间的滚动方向
		transform.position -= battleSpaceContoller.GetAdditionPos();
		
		// 统计播放结束的特效
		for( int i = 0; i < effects.Length; i++ )
		{
			if ( !effects[i].isPlaying )
			{
				endOfPlayingCount++;
			}
		}
				
		// 所有播放结束后，销毁对象
		if ( endOfPlayingCount >= playingCount )
		{
			Destroy( this.gameObject );
		}
		
	}
	
	// ------------------------------------------------------------------------
	// 设置特效运动的速度
	//  - 利用销毁前的敌机速度
	// ------------------------------------------------------------------------
	public void SetIsMoving( float speed )
	{
		this.speed = speed * 40f;
		if ( this.speed > 5f )
		{
			this.speed = 5f;
		}
		this.isMoving = true;
	}

}

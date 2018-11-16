
using UnityEngine;


/// <summary>声音播放管理类</summary>
public class SoundManager : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 相关的成员变量／方法

	/// <summary>播放的音频片段</summary>
	public AudioClip[] m_audioClips = null;

	/// <summary>启动方法</summary>
	private void Start()
	{
		// 生成 SE 和 BGM 用的声源
		audioSE  = gameObject.AddComponent< AudioSource >();
		audioBGM = gameObject.AddComponent< AudioSource >();

		// 播放BGM 
		audioBGM.clip = getAudioClip( "rpg_ambience01" );
		audioBGM.loop = true;
		audioBGM.Play();
	}


	//==============================================================================================
	// 公开方法

	/// <summary>播放SE</summary>
	public void playSE( AudioClip clip, bool isLoop = false )
	{
		if ( clip != null )
		{
			audioSE.clip = clip;
			audioSE.loop = isLoop;
			audioSE.Play();
		}
	}

	/// <summary>指定名称播放SE</summary>
	public void playSE( string name, bool isLoop = false )
	{
		AudioClip clip = getAudioClip( name );
		if ( clip != null )
		{
			playSE( clip, isLoop );
		}
	}

	/// <summary>停止播放SE</summary>
	public void stopSE()
	{
		audioSE.Stop();
	}

	/// <summary>通过名称取得音频片段</summary>
	public AudioClip getAudioClip( string name )
	{
		AudioClip audioClip = null;
		foreach ( AudioClip clip in m_audioClips )
		{
			if ( name == clip.name )
			{
				audioClip = clip;
				break;
			}
		}

		return audioClip;
	}

	public static SoundManager	get()
	{
		if(instance == null) {
			
			GameObject	go = GameObject.Find("SoundManager");
			
			if(go == null) {
				
				Debug.Log("Can't find \"SoundManager\" GameObject.");
				
			} else {
				
				instance = go.GetComponent<SoundManager>();
			}
		}
		return(instance);
	}
	protected static SoundManager	instance = null;

	//==============================================================================================
	// 非公开成员变量

	/// <summary>SE 用的声源对象</summary>
	private AudioSource audioSE;

	/// <summary>BGM 用的声源对象</summary>
	private AudioSource audioBGM;
}

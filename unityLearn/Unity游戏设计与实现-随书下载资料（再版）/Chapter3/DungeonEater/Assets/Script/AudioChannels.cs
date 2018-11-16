using UnityEngine;
using System.Collections;

public class AudioChannels : MonoBehaviour {

	private const int	AUDIO_CHANNEL_NUM = 8;

	private struct CHANNEL {

		public AudioSource	channel;
		public float		keyOnTime;
	};
	private CHANNEL[]	m_channels;

	// Use this for initialization
	void Awake () {
		m_channels = new CHANNEL[AUDIO_CHANNEL_NUM];
		for (int i = 0; i < AUDIO_CHANNEL_NUM; i++) {
			m_channels[i].channel = gameObject.AddComponent<AudioSource>();
			m_channels[i].keyOnTime = 0;
		}
		
	}

	public int PlayOneShot(AudioClip clip,float volume  ,float pan ,float pitch = 1.0f)
	{	
		// 同一Clip不能同时播放
		for (int i = 0; i < m_channels.Length ; i++) {
			if (m_channels[i].channel.isPlaying &&
				m_channels[i].channel.clip == clip &&
				m_channels[i].keyOnTime >= Time.time - 0.03f)
				return -1;
		}
		
		int oldest = -1;
		float time = 1000000000.0f;		
		for (int i = 0; i < m_channels.Length ; i++) {
			if (m_channels[i].channel.loop == false &&
				m_channels[i].channel.isPlaying && 
				m_channels[i].keyOnTime < time) {
				oldest = i;
				time =  m_channels[i].keyOnTime;
			}
			if (!m_channels[i].channel.isPlaying) {
				m_channels[i].channel.clip = clip;
				m_channels[i].channel.volume = volume;
				m_channels[i].channel.panStereo = pan;
				m_channels[i].channel.loop = false;
				m_channels[i].channel.pitch = pitch;
				m_channels[i].channel.Play();
				m_channels[i].keyOnTime = Time.time;
				return i;
			}
		}
		
		// 通道未打开时
		if (oldest >= 0) {
			m_channels[oldest].channel.clip = clip;
			m_channels[oldest].channel.volume = volume;
			m_channels[oldest].channel.panStereo = pan;
			m_channels[oldest].channel.loop = false;
			m_channels[oldest].channel.pitch = pitch;
			m_channels[oldest].channel.Play();
			m_channels[oldest].keyOnTime = Time.time;
			return oldest;
		}
		return -1;
		
		
		
//		m_channels[0].pan = pan;
//		m_channels[0].pitch = pitch;
//		m_channels[0].PlayOneShot(clip,volume);
	}
	
	public int PlayLoop(AudioClip clip,float volume  ,float pan,float pitch = 1.0f )
	{
		for (int i = 0; i < m_channels.Length ; i++) {
			if (!m_channels[i].channel.isPlaying) {
				m_channels[i].channel.clip = clip;
				m_channels[i].channel.volume = volume;
				m_channels[i].channel.panStereo = pan;
				m_channels[i].channel.loop = true;
				m_channels[i].channel.pitch = pitch;
				m_channels[i].channel.Play();
				m_channels[i].keyOnTime = Time.time;
				return i;
			}
		}
		return -1;
	}
	
    public void StopAll()
	{
		foreach(CHANNEL channel in m_channels)
			channel.channel.Stop();       
	}
	
	public void Stop(int id)
	{
		if ( id >= 0 && id < m_channels.Length ) {
			m_channels[id].channel.Stop();
		}
	}		

}

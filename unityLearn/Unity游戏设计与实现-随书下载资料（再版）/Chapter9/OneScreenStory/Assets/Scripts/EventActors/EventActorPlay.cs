
using UnityEngine;


/// <summary>play 指令事件Actor</summary>
public class EventActorPlay : EventActor
{
	//==============================================================================================
	// 公有方法

	/// <summary>构造函数</summary>
	public EventActorPlay( AudioClip clip, bool isLoop )
	{
		m_clip   = clip;
		m_isLoop = isLoop;
	}

	/// <summary>生成Actor刚开始时执行的方法</summary>
	public override void start( EventManager evman )
	{
		// 播放音频片段
		evman.getSoundManager().playSE( m_clip, m_isLoop );
	}

	/// <summary>判断执行结束后是否需要等待点击</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 立刻结束
		return false;
	}


	//==============================================================================================
	// 私有变量

	/// <summary>播放的音频片段</summary>
	private AudioClip m_clip;

	/// <summary>是否循环播放</summary>
	private bool m_isLoop;


	//==============================================================================================
	// 静态方法

	/// <summary>生成事件Actor的实例</summary>
	public static EventActorPlay CreateInstance( string[] parameters, GameObject manager, Event ev )
	{
		bool      isValid = false;
		bool      isLoop  = false;
		AudioClip clip    = null;

		// 设置这里创建的Actor的名字（显示错误消息用）
		ev.setCurrentActorName( "EventActorPlay" );

		if ( parameters.Length >= 1 )
		{
			isValid = true;

			if ( parameters.Length >= 2 && parameters[ 1 ].ToLower() == "loop" )
			{
				isLoop = true;
			}

			// 从声音管理器中取得音频片段
			clip = manager.GetComponent< EventManager >().getSoundManager().getAudioClip( parameters[ 0 ] );
			if ( clip == null )
			{
				ev.debugLogError( "Can't find audio clip \"" + parameters[ 0 ] + "\"" );
				isValid = false;
			}
		}

		if ( isValid )
		{
			// 生成Actor
			EventActorPlay actor = new EventActorPlay( clip, isLoop );
			return actor;
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}

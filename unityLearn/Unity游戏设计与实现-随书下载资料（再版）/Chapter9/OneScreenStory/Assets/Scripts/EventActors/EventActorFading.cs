
using UnityEngine;


/// <summary>fadein/fadeout 指令事件Actor</summary>
public class EventActorFading : EventActor
{
	//==============================================================================================
	// 公有方法

	/// <summary>构造函数</summary>
	public EventActorFading( FadeInOutManager manager, bool isFadeIn, float duration )
	{
		m_manager  = manager;
		m_isFadeIn = isFadeIn;
		m_duration = duration;
	}

	/// <summary>生成Actor刚开始时执行的方法</summary>
	public override void start( EventManager evman )
	{
		if ( m_isFadeIn )
		{
			m_manager.fadeIn( m_duration );
		}
		else
		{
			m_manager.fadeOut( m_duration );
		}
	}

	/// <summary>判断Actor必须执行的处理是否结束</summary>
	public override bool isDone()
	{
		return !m_manager.isFading();
	}

	/// <summary>判断执行结束后是否需要等待点击</summary>
	public override bool isWaitClick( EventManager evman )
	{
		return false;
	}


	//==============================================================================================
	// 私有变量

	/// <summary>管理画面淡入淡出的对象</summary>
	private FadeInOutManager m_manager;

	/// <summary>是否淡入</summary>
	private bool m_isFadeIn;

	/// <summary>淡入／淡出过程的秒数</summary>
	private float m_duration;


	//==============================================================================================
	// 静态方法

	/// <summary>生成事件Actor的实例</summary>
	public static EventActorFading CreateInstance( string[] parameters, bool isFadeIn, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
			FadeInOutManager fiom = FadeInOutManager.get();
			float duration;

			if ( fiom != null && float.TryParse( parameters[ 0 ], out duration ) )
			{
				// 生成Actor
				EventActorFading actor = new EventActorFading( fiom, isFadeIn, duration );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}

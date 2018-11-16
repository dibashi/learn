
using System;
using UnityEngine;


/// <summary>message 指令事件Actor</summary>
public class EventActorMessage : EventActor
{
	//==============================================================================================
	// 公有方法

	/// <summary>构造函数</summary>
	public EventActorMessage( string message, BaseObject to, string[] parameters )
	{
		m_message    = message;
		m_to         = to;
		m_parameters = parameters;
	}

	/// <summary>在Actor被销毁前每帧执行的方法</summary>
	public override void execute( EventManager evman )
	{
		if ( !( m_to.updateMessage( m_message, m_parameters ) ) )
		{
			m_isDone = true;
		}
	}

	/// <summary>判断Actor必须执行的处理是否结束</summary>
	public override bool isDone()
	{
		return m_isDone;
	}

	/// <summary>判断执行结束后是否需要等待点击</summary>
	public override bool isWaitClick(EventManager evman)
	{
		// 结束时刻执行消息的目标对象的委托
		return false;
	}


	//==============================================================================================
	// 私有变量

	/// <summary>消息的种类</summary>
	private string m_message;

	/// <summary>消息的目标对象</summary>
	private BaseObject m_to;

	/// <summary>消息参数</summary>
	private string[] m_parameters;

	/// <summary>Actor处理是否结束</summary>
	private bool m_isDone = false;


	//==============================================================================================
	// 静态方法

	/// <summary>生成事件Actor的实例</summary>
	public static EventActorMessage CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 2 )
		{
			// 探测被指定的对象
			BaseObject bo = manager.GetComponent< ObjectManager >().find( parameters[ 0 ] );
			if ( bo != null )
			{
				string[] messageParams = new string[ parameters.Length - 2 ];
				Array.Copy( parameters, 2, messageParams, 0, messageParams.Length );

				// 生成Actor
				EventActorMessage actor = new EventActorMessage( parameters[ 1 ], bo, messageParams );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}

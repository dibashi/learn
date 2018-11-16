
using UnityEngine;


/// <summary>call-event 指令事件Actor</summary>
public class EventActorCallEvent : EventActor
{
	//==============================================================================================
	// 公有方法

	/// <summary>构造函数</summary>
	public EventActorCallEvent( int eventIndex )
	{
		m_eventIndex = eventIndex;
	}

	/// <summary>生成Actor刚开始时执行的方法</summary>
	public override void start( EventManager evman )
	{
		evman.startEvent( m_eventIndex );
	}

	/// <summary>判断执行结束后是否需要等待点击</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 马上结束
		return false;
	}


	//==============================================================================================
	// 私有变量

	/// <summary>事件中调用的事件索引</summary>
	// 实际并不会返回所以跳过
	private int m_eventIndex;


	//==============================================================================================
	// 静态方法

	/// <summary>生成事件Actor的实例</summary>
	public static EventActorCallEvent CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
			int eventIndex = manager.GetComponent< EventManager >().getEventIndexByName( parameters[ 0 ] );

			// 生成Actor
			EventActorCallEvent actor = new EventActorCallEvent( eventIndex );
			return actor;
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}

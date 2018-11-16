
using UnityEngine;


/// <summary>set 指令事件Actor</summary>
public class EventActorSet : EventActor
{
	//==============================================================================================
	// 公有方法

	/// <summary>构造函数</summary>
	public EventActorSet( BaseObject baseObject, string name, string value )
	{
		m_object = baseObject;
		m_name   = name;
		m_value  = value;
	}

	/// <summary>生成Actor刚开始时执行的方法</summary>
	public override void start( EventManager evman )
	{
		// 设置游戏内的参数
		m_object.setVariable( m_name, m_value );
	}

	/// <summary>判断执行结束后是否需要等待点击</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 立刻结束
		return false;
	}


	//==============================================================================================
	// 私有变量

	/// <summary>操作游戏内参数的对象</summary>
	private BaseObject m_object;

	/// <summary>游戏内变量名</summary>
	private string m_name;

	/// <summary>值</summary>
	private string m_value;


	//==============================================================================================
	// 静态方法

	/// <summary>生成事件Actor的实例</summary>
	public static EventActorSet CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 3 )
		{
			// 探测被指定的对象
			BaseObject bo = manager.GetComponent< ObjectManager >().find( parameters[ 0 ] );
			if ( bo != null )
			{
				// 生成Actor
				EventActorSet actor = new EventActorSet( bo, parameters[ 1 ], parameters[ 2 ] );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}

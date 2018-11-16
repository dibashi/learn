
using UnityEngine;


/// <summary>hide/show 指令事件Actor</summary>
public class EventActorVisibility : EventActor
{
	//==============================================================================================
	// 公有方法

	/// <summary>构造函数</summary>
	public EventActorVisibility( BaseObject baseObject, bool doShow )
	{
		m_object = baseObject;
		m_doShow = doShow;
	}

	/// <summary>生成Actor刚开始时执行的方法</summary>
	public override void start( EventManager evman )
	{
		ObjectManager om = evman.GetComponent< ObjectManager >();

		// 播放切换时特效
		EffectManager.get().playAppearEffect(m_object);

		// 显示／不显示
		if ( m_doShow )
		{
			om.activate( m_object );
		}
		else
		{
			om.deactivate( m_object );
		}
	}

	/// <summary>判断执行结束后是否需要等待点击</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 立刻结束
		return false;
	}


	//==============================================================================================
	// 私有变量

	/// <summary>显示／不显示的对象</summary>
	private BaseObject m_object;

	/// <summary>是否显示对象</summary>
	private bool m_doShow;


	//==============================================================================================
	// 静态方法

	/// <summary>生成事件Actor的实例</summary>
	public static EventActorVisibility CreateInstance( string[] parameters, bool doShow, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
			// 探测被指定的对象
			BaseObject bo = manager.GetComponent< ObjectManager >().find( parameters[ 0 ] );
			if ( bo != null )
			{
				// 生成Actor
				EventActorVisibility actor = new EventActorVisibility( bo, doShow );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}

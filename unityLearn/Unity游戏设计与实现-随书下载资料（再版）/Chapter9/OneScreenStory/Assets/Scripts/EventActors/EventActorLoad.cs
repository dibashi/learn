
using UnityEngine;


/// <summary>load 指令事件Actor</summary>
public class EventActorLoad : EventActor
{
	//==============================================================================================
	// 公有方法

	/// <summary>构造函数</summary>
	public EventActorLoad( string[] fileNames )
	{
		m_fileNames = fileNames;
	}

	/// <summary>生成Actor刚开始时执行的方法</summary>
	public override void start( EventManager evman )
	{
		// 事件管理器中指定下一个脚本文件
		// （实际被导入发生在该系列事件结束后）
		evman.setNextScriptFiles( m_fileNames );
	}

	/// <summary>判断执行结束后是否需要等待点击</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 立刻结束
		return false;
	}


	//==============================================================================================
	// 私有变量

	/// <summary>脚本文件名</summary>
	private string[] m_fileNames;


	//==============================================================================================
	// 静态方法

	/// <summary>生成事件Actor的实例</summary>
	public static EventActorLoad CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
			// 生成Actor
			EventActorLoad actor = new EventActorLoad( parameters );
			return actor;
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}

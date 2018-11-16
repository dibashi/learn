
using System;
using UnityEngine;


/// <summary>dialog 指令事件Actor</summary>
public class EventActorDialog : EventActor
{
	//==============================================================================================
	// 公有方法

	/// <summary>构造函数</summary>
	public EventActorDialog( BaseObject baseObject, string text )
	{
		m_object = baseObject;
		m_text   = text;
	}

	/// <summary>生成Actor刚开始时执行的方法</summary>
	public override void start( EventManager evman )
	{
		// 显示对话文本
		TextManager		textman = TextManager.get();

		textman.showDialog( m_object, m_text, 50.0f, 10.0f, 15.0f );
	}

	/// <summary>判断执行结束后是否需要等待点击</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 下一个Actor时选项 choice 时，不需要等待点击
		Event ev = evman.getActiveEvent();
		if ( ev != null && ev.getNextKind() == "choice" )
		{
			return false;
		}

		// 除此之外都需要等待
		return true;
	}


	//==============================================================================================
	// 私有变量

	/// <summary>用于显示对话文本的对象</summary>
	private BaseObject m_object;

	/// <summary>显示的文本</summary>
	private string m_text;


	//==============================================================================================
	// 静态方法

	/// <summary>生成事件Actor的实例</summary>
	public static EventActorDialog CreateInstance( string[] parameters, GameObject manager, Event ev )
	{
		// 设置从这里开始创建的Actor的名字（错误消息中使用）
		ev.setCurrentActorName( "EventActorDialog" );

		if ( parameters.Length >= 2 )
		{
			// 探测指定的对象
			BaseObject bo = manager.GetComponent< ObjectManager >().find( parameters[ 0 ] );
			if ( bo != null )
			{
				// 分割文本
				string[] text = new string[ parameters.Length - 1 ];
				Array.Copy( parameters, 1, text, 0, text.Length );

				// 生成Actor
				EventActorDialog actor = new EventActorDialog( bo, String.Join( "\n", text ) );
				return actor;
			}
			else
			{
				ev.debugLogError( "Can't find BaseObject(" + parameters[ 0 ] + ")" );
				return null;
			}
		}

		ev.debugLogError( "Out of Parameter" );
		return null;
	}
}

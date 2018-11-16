
using System;
using UnityEngine;


/// <summary>text 指令事件Actor</summary>
public class EventActorText : EventActor
{
	//==============================================================================================
	// 公有方法

	/// <summary>构造函数</summary>
	public EventActorText( string text )
	{
		m_text = text;
	}

	/// <summary>生成Actor刚开始时执行的方法</summary>
	public override void start( EventManager evman )
	{
		// 显示提示文本
		TextManager		text_man = TextManager.get();

		text_man.showText(m_text, new Vector2(320.0f + 50.0f, 50.0f + 100.0f), 50.0f, 10.0f, 15.0f);
	}

	/// <summary>判断执行结束后是否需要等待点击</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 下一个Actor为选项 choice 时不需要等待
		Event ev = evman.getActiveEvent();
		if ( ev != null && ev.getNextKind() == "choice" )
		{
			return false;
		}

		// 除此之外需要等待
		return true;
	}


	//==============================================================================================
	// 私有变量

	/// <summary>显示的文本</summary>
	private string m_text;


	//==============================================================================================
	// 静态方法

	/// <summary>生成事件Actor的实例</summary>
	public static EventActorText CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
			// 生成Actor
			EventActorText actor = new EventActorText( String.Join( "\n", parameters ) );
			return actor;
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}

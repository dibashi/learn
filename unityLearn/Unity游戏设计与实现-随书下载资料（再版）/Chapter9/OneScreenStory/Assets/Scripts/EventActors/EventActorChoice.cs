
using System;
using UnityEngine;


/// <summary>choice 指令事件Actor</summary>
public class EventActorChoice : EventActor
{
	//==============================================================================================
	// 公有方法

	/// <summary>构造函数</summary>
	public EventActorChoice( BaseObject baseObject, string name, string[] choices )
	{
		m_object  = baseObject;
		m_name    = name;
		m_choices = choices;
	}

	/// <summary>在Actor被销毁前绘制GUI时执行的方法</summary>
	public override void start( EventManager evman )
	{
		// 显示提示文字
		TextManager		text_man = TextManager.get();

		text_man.createButtons(m_choices);
	}

	/// <summary>在Actor被销毁前每帧都会执行</summary>
	/// 替代UnityEngine.MonoBehaviour.Update()
	public override void execute( EventManager evman )
	{
		TextManager		text_man = TextManager.get();

		do {

			if(text_man.selected_button == "") {
				break;
			}

			int		selected_index = System.Array.IndexOf(m_choices, text_man.selected_button);

			if(selected_index < 0) {
				break;
			}

			// 被点击的选项的索引设置到游戏内参数然后结束
			// （第一个选项为1）
			m_object.setVariable(m_name, (selected_index + 1).ToString());

			text_man.deleteButtons();
			m_isDone = true;

		} while(false);
	}

	/// <summary>判断Actor必须执行的处理是否结束</summary>
	public override bool isDone()
	{
		return m_isDone;
	}

	/// <summary>判断执行结束后是否需要等待点击</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 点击选项过程处理中已经包含了所以这里不需要等待
		return false;
	}


	//==============================================================================================
	// 私有变量

	/// <summary>用于操作游戏内参数的对象</summary>
	private BaseObject m_object;

	/// <summary>游戏内参数名称</summary>
	private string m_name;

	/// <summary>选项一览</summary>
	private string[] m_choices;

	/// <summary>Actor处理是否已经结束</summary>
	private bool m_isDone = false;


	//==============================================================================================
	// 静态方法

	/// <summary>生成事件Actor的实例</summary>
	public static EventActorChoice CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 3 )
		{
			// 探测指定的对象
			BaseObject bo = manager.GetComponent< ObjectManager >().find( parameters[ 0 ] );
			if ( bo != null )
			{
				string[] choices = new string[ parameters.Length - 2 ];
				Array.Copy( parameters, 2, choices, 0, choices.Length );

				// 生成Actor
				EventActorChoice actor = new EventActorChoice( bo, parameters[ 1 ], choices );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}

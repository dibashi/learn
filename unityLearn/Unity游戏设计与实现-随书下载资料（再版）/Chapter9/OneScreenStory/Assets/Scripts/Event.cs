
using System;
using UnityEngine;


/// <summary>事件</summary>
public class Event
{
	//==============================================================================================
	// 内部数据类型

	/// <summary>Actor的执行状态</summary>
	private enum STEP
	{
		NONE = -1,
		EXEC_ACTOR = 0,  // Actor执行中
		WAIT_INPUT,      // 等待点击
		DONE,            // 结束
		NUM
	}


	//==============================================================================================
	// 公开方法

	/// <summary>构造函数</summary>
	public Event( string[] targets, EventCondition[] conditions, string[][] actions, bool isPrologue, bool doContinue, string name )
	{
		Array.Sort( targets );	// 为了后续的比较提前进行排序

		m_targets    = targets;
		m_conditions = conditions;
		m_actions    = actions;
		m_isPrologue = isPrologue;
		m_doContinue = doContinue;
		m_name       = name;
	}

	/// <summary>对事件评估</summary>
	public bool evaluate( string[] contactingObjects, bool isPrologue )
	{
		if ( isPrologue )
		{
			if ( !m_isPrologue ) return false;
		}
		else
		{
			// 发生对象和接触对象的比较
			Array.Sort( contactingObjects );

			if ( m_targets.Length == contactingObjects.Length )
			{
				for ( int i = 0; i < m_targets.Length; ++i )
				{
					// 值为“*”时哪个都行
					if ( m_targets[ i ] == "*" ) continue;

					if ( m_targets[ i ] != contactingObjects[ i ] )
					{
						return false;
					}
				}
			}
			else
			{
				return false;
			}
		}

		// 检测发生条件
		foreach ( EventCondition ec in m_conditions )
		{
			if ( !ec.evaluate() ) return false;
		}

		return true;
	}

	/// <summary>事件开始时的方法</summary>
	public void start()
	{
		m_step           = STEP.NONE;
		m_nextStep       = STEP.EXEC_ACTOR;
		m_currentActor   = null;
		m_nextActorIndex = 0;
	}

	/// <summary>事件的逐帧更新方法</summary>
	public void execute( EventManager evman )
	{
		// ------------------------------------------------------------ //

		switch ( m_step ) {
			case STEP.WAIT_INPUT:
			{
				if ( Input.GetMouseButtonDown( 0 ) )
				{
					m_currentActor = null;
					m_nextStep = STEP.EXEC_ACTOR;
				}
			}
			break;

			case STEP.EXEC_ACTOR:
			{
				if ( m_currentActor.isDone() )
				{
					// 等待输入？
					if ( m_currentActor.isWaitClick( evman ) )
					{
						m_nextStep = STEP.WAIT_INPUT;
					}
					else
					{
						// 如果不是则立刻进入下一个Actor
						m_nextStep = STEP.EXEC_ACTOR;
					}
				}
			}
			break;
		}

		// ------------------------------------------------------------ //

		while ( m_nextStep != STEP.NONE )
		{
			m_step     = m_nextStep;
			m_nextStep = STEP.NONE;

			switch ( m_step )
			{
				case STEP.EXEC_ACTOR:
				{
					m_currentActor = null;

					// 对脚本进行轮询直到找出下一个执行的Actor
					while ( m_nextActorIndex < m_actions.Length )
					{
						m_currentActor = createActor( evman, m_nextActorIndex );

						++m_nextActorIndex;

						if ( m_currentActor != null )
						{
							break;
						}
					}

					if ( m_currentActor != null )
					{
						m_currentActor.start( evman );
					}
					else
					{
						// 如果没有下一个Actor则事件结束
						m_nextStep = STEP.DONE;
					}
				}
				break;
			}
		}

		// ------------------------------------------------------------ //

		switch ( m_step )
		{
			case STEP.EXEC_ACTOR:
			{
				m_currentActor.execute( evman );
			}
			break;
		}
	}

	/// <summary>事件的 GUI 处理方法</summary>
	public void onGUI( EventManager evman )
	{
#if UNITY_EDITOR
		// 显示当前执行中的Actor的文本文件中的行号
		if ( m_currentActor != null )
		{
			if ( m_actionLineNumbers != null )
			{
				string text = "";
				text += m_actionLineNumbers[ m_nextActorIndex - 1 ];
				text += " :";
				text += m_currentActor.ToString();

				//GUI.Label( new Rect( 10, 40, 200, 20 ), text );
			}
		}
#endif

		/*switch ( m_step )
		{
			case STEP.EXEC_ACTOR:
			{
				if ( m_currentActor != null )
				{
					m_currentActor.onGUI( evman );
				}
			}
			break;
		}*/
	}

	/// <summary>事件是否已经结束</summary>
	public bool isDone()
	{
		return m_step == STEP.DONE;
	}

	/// <summary>判断该事件后续的评价／执行是否可能</summary>
	public bool doContinue()
	{
		return m_doContinue;
	}

	/// <summary>取得事件的名称</summary>
	public string getEventName()
	{
		return m_name;
	}

	/// <summary>取得下一次执行的Actor的种类</summary>
	public string getNextKind()
	{
		string kind = "";

		if ( m_nextActorIndex < m_actions.Length )
		{
			kind = m_actions[ m_nextActorIndex ][ 0 ];
			kind = kind.ToLower();
		}

		return kind;
	}

	/// <summary>取得记录该事件的脚本中行号</summary>
	public int getLineNumber()
	{
		return m_lineNumber;
	}

	/// <summary>设定记录该事件的脚本的行号</summary>
	public void setLineNumber( int n )
	{
		m_lineNumber = n;
	}

	/// <summary>设定记录该事件的各个动作的脚本的行号</summary>
	public void setActionLineNumbers( int[] numbers )
	{
		m_actionLineNumbers = numbers;
	}

	/// <summary>设定当前Actor的名字</summary>
	public void setCurrentActorName( string name )
	{
		m_actorName = name;
	}

	/// <summary>输入生成Actor过程中的错误</summary>
	public void debugLogError( string message )
	{
		Debug.LogError( m_actorName + ":" + message + " at " + m_actionLineNumbers[ m_actorIndex ] + "." );
	}

	/// <summary>生成事件Actor</summary>
	public EventActor createActor( EventManager manager, int index )
	{
		string[] action     = m_actions[ index ];
		string   kind       = action[ 0 ];
		string[] parameters = new string[ action.Length - 1 ];
		Array.Copy( action, 1, parameters, 0, parameters.Length );

		m_actorName  = "";
		m_actorIndex = index;
		EventActor actor = null;

		switch ( kind.ToLower() )
		{
		// [evaluate-event]
		// 事件结束时继续执行指定的事件
		case "evaluate-event":
			actor = EventActorEvaluateEvent.CreateInstance( parameters, manager.gameObject );
			break;

		// [set]
		// 将字符串代入游戏内变量
		case "set":
			actor = EventActorSet.CreateInstance( parameters, manager.gameObject );
			break;

		// [move]
		// 移动对象
		case "move":
			actor = EventActorMove.CreateInstance( parameters, manager.gameObject );
			break;

		// [hide]
		// 不显示指定的对象
		case "hide":
			actor = EventActorVisibility.CreateInstance( parameters, false, manager.gameObject );
			break;

		// [show]
		// 显示指定的对象
		case "show":
			actor = EventActorVisibility.CreateInstance( parameters, true, manager.gameObject );
			break;

		// [text]
		// 显示文本
		case "text":
			actor = EventActorText.CreateInstance( parameters, manager.gameObject );
			break;

		// [dialog]
		// 显示对话内容
		case "dialog":
			actor = EventActorDialog.CreateInstance( parameters, manager.gameObject, this );
			break;

		// [choice]
		// 显示选项并根据选择内容将值代入游戏内变量
		case "choice":
			actor = EventActorChoice.CreateInstance( parameters, manager.gameObject );
			break;

		// [play]
		// 播放声音
		case "play":
			actor = EventActorPlay.CreateInstance( parameters, manager.gameObject, this );
			break;

		// [fadeout]
		// 执行淡出
		case "fadeout":
			actor = EventActorFading.CreateInstance( parameters, false, manager.gameObject );
			break;

		// [fadein]
		// 执行淡入
		case "fadein":
			actor = EventActorFading.CreateInstance( parameters, true, manager.gameObject );
			break;

		// [load]
		// 读取脚本替换事件
		case "load":
			// ToDo: load 指令存在时是否中断后续的评价？（现在不中断）
			actor = EventActorLoad.CreateInstance( parameters, manager.gameObject );
			break;

		// [call-event]
		// 执行指令时强行执行其他事件
		case "call-event":
			actor = EventActorCallEvent.CreateInstance( parameters, manager.gameObject );
			break;

		// [message]
		// 发送消息给对象执行特定的处理
		case "message":
			actor = EventActorMessage.CreateInstance( parameters, manager.gameObject );
			break;

		default:
			Debug.LogError( "Invalid command \"" + kind + "\"" );
			break;
		}

		return actor;
	}


	//==============================================================================================
	// 非公开变量

	/// <summary>事件发生对象</summary>
	private string[]         m_targets;

	/// <summary>事件的发生条件</summary>
	private EventCondition[] m_conditions;

	/// <summary>事件动作</summary>
	private string[][]       m_actions;

	/// <summary>开场事件标记</summary>
	private bool             m_isPrologue;

	/// <summary>事件的继续评价标记</summary>
	private bool             m_doContinue;

	/// <summary>事件的名称</summary>
	private string           m_name;

	/// <summary>当前Actor的执行状态</summary>
	private STEP             m_step = STEP.NONE;

	/// <summary>下一次迁移的Actor执行状态</summary>
	private STEP             m_nextStep = STEP.EXEC_ACTOR;

	/// <summary>当前执行中的Actor</summary>
	private EventActor       m_currentActor = null;

	/// <summary>下次执行Actor的索引</summary>
	private int              m_nextActorIndex = 0;

	/// <summary>最后被试着生成的Actor的名称</summary>
	private string           m_actorName = "";

	/// <summary>最后被试着生成的Actor的索引</summary>
	private int              m_actorIndex = 0;

	/// <summary>记录事件的脚本的行号</summary>
	private int              m_lineNumber = 0;

	/// <summary>记录事件各个动作的脚本的行号</summary>
	private int[]            m_actionLineNumbers = null;
}

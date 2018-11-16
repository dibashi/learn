
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/// <summary>游戏内事件管理类</summary>
public class EventManager : MonoBehaviour
{
	//==============================================================================================
	// 内部数据类型

	/// <summary>事件的执行状态</summary>
	private enum STEP
	{
		NONE = -1,
		LOAD_SCRIPT = 0,  // 载入脚本文件
		WAIT_TRIGGER,     // 等待事件触发
		START_EVENT,      // 开始事件
		EXECUTE_EVENT,    // 执行事件
		NUM
	}


	//==============================================================================================
	// MonoBehaviour 相关的方法／成员变量

	public TextAsset[]	m_eventScripts     = new TextAsset[0];

	/// <summary>游戏开始时读取脚本文件名</summary>
	public TextAsset[]	m_firstScriptFiles = new TextAsset[0];


	/// <summary>启动方法</summary>
	private void Start()
	{
		// 未通过检视器指定脚本时则使用主体画面选择的项
		if ( m_firstScriptFiles.Length == 0 )
		{
			m_firstScriptFiles = this.findEventScripts(GlobalParam.getInstance().getStartScriptFiles());
		}

		// 成员初始化
		m_isPrologue = true;
		m_nextStep = STEP.LOAD_SCRIPT;
		m_nextScriptFiles = m_firstScriptFiles;
		m_nextEvaluatingEventIndex = -1;

		// 探测声音管理器
		m_soundManager = GameObject.Find( "SoundManager" ).GetComponent< SoundManager >();
	}

	/// <summary>逐帧更新方法</summary>
	private void Update()
	{
		// ------------------------------------------------------------ //

		if ( m_nextStep == STEP.NONE )
		{
			switch ( m_step )
			{
				case STEP.LOAD_SCRIPT:
				{
					// 脚本文件载入完成
					if ( m_hasCreatedEvents )
					{
						m_isExecuting = false;
						m_isPrologue = true;
						m_activeEvent = null;
						m_activeEventIndex = -1;
						m_nextEvaluatingEventIndex = -1;
						m_nextScriptFiles = null;

						m_nextStep = STEP.WAIT_TRIGGER;
					}
				}
				break;

				case STEP.WAIT_TRIGGER:
				{
					if ( m_isPrologue )
					{
						// プロローグイベントは無条件に発動.
						m_nextStep = STEP.START_EVENT;
					}
					else
					{
						if ( m_contactingObjects.Count > 0 )  // 有对象发生接触
						{
							m_isStartedByContact = true;
							m_nextStep = STEP.START_EVENT;
						}
					}
				}
				break;

				case STEP.START_EVENT:
				{
					// 探测下一个能够执行的事件

					// 从数组中取出
					string[] contactingObjectsArray = m_contactingObjects.ToArray();

					// 从上次执行的下一事件开始进行检索
					int i;
					for ( i = m_activeEventIndex + 1; i < m_events.Length; ++i )
					{
						Event ev = m_events[i];

						if ( ev.evaluate( contactingObjectsArray, m_isPrologue ) )
						{
							break;
						}
					}

					if ( i < m_events.Length )
					{
						// 找到了下一事件

						m_activeEvent      = m_events[ i ];
						m_activeEventIndex = i;
						m_nextStep         = STEP.EXECUTE_EVENT;

						// 事件开始的音效（只针对因接触而产生的事件）
						if ( m_isStartedByContact )
						{
							m_soundManager.playSE( "rpg_system05" );
						}
					}
					else
					{
						// 未找到下一事件

						m_activeEvent      = null;
						m_activeEventIndex = -1;

						// 如果一次性执行完毕，则结束开场事件
						m_isPrologue = false;

						if ( m_nextScriptFiles != null )
						{
							m_nextStep = STEP.LOAD_SCRIPT;
						}
						else
						{
							m_nextStep = STEP.WAIT_TRIGGER;
						}
					}
				}
				break;

				case STEP.EXECUTE_EVENT:
				{
					if ( m_activeEvent.isDone() )
					{
						// 隐藏介绍文字／对话文本
						TextManager.get().hide();

						do {

							// 等待直到松开按键
							if(Input.GetMouseButton(0)) {

								break;
							}

							// 如果指定了继续执行评价事件 (evaluate-event).
							if ( m_nextEvaluatingEventIndex >= 0 )
							{
								Event ev = m_events[ m_nextEvaluatingEventIndex ];
								if ( ev.evaluate( m_contactingObjects.ToArray(), m_isPrologue ) )
								{
									m_activeEvent      = ev;
									m_activeEventIndex = m_nextEvaluatingEventIndex;
									m_nextStep         = STEP.EXECUTE_EVENT;
									break;
								}
							}

							if ( !m_activeEvent.doContinue() ) m_activeEventIndex = m_events.Length;

							m_nextStep = STEP.START_EVENT;

						} while ( false );

						m_nextEvaluatingEventIndex = -1;
					}
				}
				break;
			}
		}

		// ------------------------------------------------------------ //

		while ( m_nextStep != STEP.NONE )
		{
			m_step     = m_nextStep;
			m_nextStep = STEP.NONE;

			switch ( m_step )
			{
				case STEP.LOAD_SCRIPT:
				{
					m_isExecuting = false;
					m_hasCreatedEvents = false;

					createEventsFromFile(m_nextScriptFiles);
				}
				break;

				case STEP.WAIT_TRIGGER:
				{
					m_isExecuting = false;

					// 清空列表
					m_contactingObjects.Clear();
				}
				break;

				case STEP.EXECUTE_EVENT:
				{
					m_isExecuting = true;
					m_isStartedByContact = false;
					m_activeEvent.start();
				}
				break;
			}
		}

		// ------------------------------------------------------------ //

		switch ( m_step )
		{
			case STEP.EXECUTE_EVENT:
			{
				if ( m_activeEvent != null )
				{
					m_activeEvent.execute( this );
				}
			}
			break;
		}
	}

	/// <summary>GUI 处理方法</summary>
	private void OnGUI()
	{
#if UNITY_EDITOR
		if ( m_activeEvent != null )
		{
			//GUI.Label( new Rect( 10, 10, 200, 20 ), m_activeEvent.getLineNumber().ToString() );
		}
#endif //UNITY_EDITOR

		switch ( m_step )
		{
			case STEP.EXECUTE_EVENT:
			{
				if ( m_activeEvent != null )
				{
					m_activeEvent.onGUI( this );
				}
			}
			break;
		}
	}


	//==============================================================================================
	// 公开方法

	/// <summary>添加接触对象</summary>
	public void addContactingObject( BaseObject baseObject )
	{
		string name = baseObject.name;
		if ( !m_contactingObjects.Contains( name ) )
		{
			m_contactingObjects.Add( name );
		}
	}

	/// <summary>是否正在执行事件？</summary>
	public bool isExecutingEvents()
	{
		return m_isExecuting;
	}

	/// <summary>取得正在执行中的事件</summary>
	public Event getActiveEvent()
	{
		return m_activeEvent;
	}

	/// <summary>取得事件的索引</summary>
	public int getEventIndexByName( string eventName )
	{
		return Array.FindIndex( m_events, x => x.getEventName() == eventName );
	}

	/// <summary>设置下次读取的脚本文件.</summary>
	public void		setNextScriptFiles( string[] fileNames )
	{
		m_nextScriptFiles = this.findEventScripts(fileNames);
	}

	/// <summary>设置事件结束后继续执行的评价事件的索引 (evaluate-event).</summary>
	public void setNextEvaluatingEventIndex( int eventIndex )
	{
		m_nextEvaluatingEventIndex = eventIndex;
	}

	/// <summary>强制执行事件（call-event）</summary>
	public void startEvent( int eventIndex )
	{
		m_activeEvent      = m_events[ eventIndex ];
		m_activeEventIndex = eventIndex;
		m_nextStep         = STEP.EXECUTE_EVENT;
	}

	/// <summary>返回声音管理器</summary>
	public SoundManager getSoundManager()
	{
		return m_soundManager;
	}


	//==============================================================================================
	// 非公开方法

	// 通过脚本名查找文本资源
	protected TextAsset[]	findEventScripts(string[] script_names)
	{
		List<TextAsset>		new_scripts = new List<TextAsset>();

		foreach(var script_name in script_names) {

			var	script = System.Array.Find(m_eventScripts, x => x.name == script_name);

			if(script == null) {
				Debug.LogError("Can't find event script \"" + script_name + ".\"");
			} else {
				new_scripts.Add(script);
			}
		}

		return(new_scripts.ToArray());
	}

	/// <summary>通过文件生成事件</summary>
	private void	createEventsFromFile(TextAsset[] texts)
	{
		if(texts.Length > 0 ) {

			// 用于存储所有文件行数据的列表
			List<string>	linesOfAllFiles = new List<string>();

			foreach(var text in texts) {

				// 用于删除空元素的选项
				System.StringSplitOptions	option = System.StringSplitOptions.RemoveEmptyEntries;

				// 以换行符为标志切分为一行一行
				string[]	lines = text.text.Split(new char[]{'\r', '\n'}, option);

				linesOfAllFiles.AddRange(lines);
			}

			// 转换并生成事件数组
			ScriptParser	parser = new ScriptParser();

			m_events = parser.parseAndCreateEvents( linesOfAllFiles.ToArray() );
			Debug.Log( "Created " + m_events.Length.ToString() + " events." );

		} else {

			// 清空事件
			m_events = new Event[0];
		}

		// 事件创建完成
		m_hasCreatedEvents = true;
	}

	// ================================================================ //
	// 实例

	protected static EventManager	instance = null;

	public static EventManager	get()
	{
		if(instance == null) {

			GameObject	go = GameObject.FindGameObjectWithTag("System");

			if(go == null) {

				Debug.Log("Can't find \"System\" GameObject.");

			} else {

				instance = go.GetComponent<EventManager>();
			}
		}
		return(instance);
	}

	//==============================================================================================
	// 非公开变量

	/// <summary>事件生成完成标记</summary>
	private bool m_hasCreatedEvents = false;

	/// <summary>存储事件信息的对象</summary>
	private Event[] m_events = new Event[ 0 ];

	/// <summary>开场事件的评价／执行标记</summary>
	private bool m_isPrologue = false;

	/// <summary>接触对象的列表</summary>
	private List< string > m_contactingObjects = new List< string >();

	/// <summary>事件是否正在执行中</summary>
	private bool m_isExecuting = false;

	/// <summary>当前的状态</summary>
	private STEP m_step = STEP.NONE;

	/// <summary>下一次迁移的状态</summary>
	private STEP m_nextStep = STEP.NONE;

	/// <summary>执行中的事件</summary>
	private Event m_activeEvent = null;

	/// <summary>执行中事件的索引</summary>
	private int m_activeEventIndex = -1;

	/// <summary>下次读取的脚本文件</summary>
	private TextAsset[] m_nextScriptFiles = null;

	/// <summary>根据由于接触启动了事件</summary>
	private bool m_isStartedByContact = false;

	/// <summary>事件接触后继续执行的评价事件的索引 (evaluate-event).</summary>
	private int m_nextEvaluatingEventIndex = -1;

	/// <summary>声音管理器对象</summary>
	private SoundManager m_soundManager = null;
}

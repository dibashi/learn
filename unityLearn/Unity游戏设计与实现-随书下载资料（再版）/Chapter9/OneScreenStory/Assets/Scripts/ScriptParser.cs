
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


/// <summary>脚本文件解析</summary>
public class ScriptParser
{
	//==============================================================================================
	// 公开方法

	/// <summary>解析文件并生成数组</summary>
	public Event[] parseAndCreateEvents( string[] lines )
	{
		bool isInsideOfBlock = false;
		Regex tabSplitter = new Regex( "\\t+" );             // 能通过多个制表符分割
		List< string > commandLines = new List< string >();  // 指令的命令行数据
		List< int > commandLineNumbers = new List< int >();  // 事件指令行数据文件中的行号
		List< Event > events = new List< Event >();

		string eventName = "";
		int    lineCount = 0;
		int    beginLineCount = 0;

		foreach ( string line in lines )
		{
			// 文本文件中的当前行
			lineCount++;

			// 过滤注释
			int index = line.IndexOf( ";;" );	// 因为可能会使用到“//”和“--”所以这个进行替换
			string str = index < 0 ? line : line.Substring( 0, index );

			// 忽略前后的空格
			str = str.Trim();

			// 如果上面的处理遇到了空行
			if ( str.Length < 1 ) continue;

			// 被[]包围着的内容是事件的名称
			if ( str.Length >= 3 )
			{
				if ( str[0] == '[' && str[ str.Length - 1 ] == ']' )
				{
					eventName = str.Substring( 1, str.Length - 2);
				}
			}

			switch ( str.ToLower() )
			{
			// 事件区块开始
			case "begin":
				if ( isInsideOfBlock )
				{
					Debug.LogError( "Unclosed Event ("  + beginLineCount + ")" );
					return new Event[ 0 ];  // begin 的重复错误
				}
				beginLineCount = lineCount;

				isInsideOfBlock = true;				//设置「“Begin” ～ “End” 中」的标记
				break;

			// 事件区块结束
			case "end":
				// 分解指令行数据
				List< string[] > commands = new List< string[] >();
				foreach ( string cl in commandLines )
				{
					string[] tabSplit = tabSplitter.Split( cl );
					commands.Add( tabSplit );
				}
				// 清空指令行数据
				commandLines.Clear();

				// 创建事件并添加到列表
				Event ev = createEvent( commands.ToArray(), eventName, commandLineNumbers.ToArray(), beginLineCount);
				if ( ev != null )
				{
					ev.setLineNumber( beginLineCount );
					events.Add( ev );
				}
				// 初始化事件数据
				commandLineNumbers.Clear();
				eventName = "";

				isInsideOfBlock = false;
				break;

			// 其他情况
			default:
				// 如果「”Begin” ～ “End” 中」，则将当前行添加到命令集合
				if ( isInsideOfBlock )
				{
					commandLines.Add( str );
					commandLineNumbers.Add( lineCount );
				}
				break;
			}
		}

		// Begin 未关闭
		if ( isInsideOfBlock )
		{
			Debug.LogError( "Unclosed Event ("  + beginLineCount + ")" );
		}

		return events.ToArray();
	}


	//==============================================================================================
	// 非公开方法

	/// <summary>从事件的指令数据生成事件</summary>
	private Event createEvent( string[][] commands, string eventName, int[] numbers, int beginLineCount )
	{
		List< string >         targets     = new List< string >();
		List< EventCondition > conditions  = new List< EventCondition >();
		List< string[] >       actions     = new List< string[] >();
		List< int >            lineNumbers = new List< int >();

		DebugManager           debug_manager = GameObject.FindGameObjectWithTag( "DebugManager" ).GetComponent< DebugManager >();

		bool                   isPrologue = false;
		bool                   doContinue = false;

		int                    i = 0;

		foreach ( string[] commandParams in commands )
		{
			switch ( commandParams[ 0 ].ToLower() )
			{
			// 目标对象
			case "target":
				if ( commandParams.Length >= 2 )
				{
					targets.Add( commandParams[ 1 ] );
				}
				else
				{
					Debug.LogError( "Failed to add a target." );
				}
				break;

			// 开场事件
			case "prologue":
				isPrologue = true;
				break;

			// 发生条件
			case "condition":
				if ( commandParams.Length >= 4 )
				{
					// 查找指定的对象
					// ToDo: 目前的处理下找不到隐藏的对象，这里需要改进
					GameObject go = GameObject.Find( commandParams[ 1 ] );

					if(go == null) {

						Debug.LogError( "Can't find object." + " " + commandParams[1]);

					} else {

						BaseObject bo = go.GetComponent<BaseObject>();

						if ( bo != null )
						{
							EventCondition ec = new EventCondition( bo, commandParams[ 2 ], commandParams[ 3 ] );
							conditions.Add( ec );
	
							// （调试用）添加观察的条件变量
							debug_manager.addWatchConditionVariable( commandParams[ 1 ], commandParams[ 2 ] );
						}
						else
						{
							Debug.LogError( "Failed to add a condition." + " " + go.name + " " +  commandParams[2]);
						}
					}
				}
				else
				{
					Debug.LogError( "Failed to add a condition." );
				}
				break;

			// 继续评价
			case "continue":
				doContinue = true;
				break;

			// 其他action（这个阶段仅保持参数）
			default:
				actions.Add( commandParams );
				lineNumbers.Add( numbers[ i ] );
				break;
			}

			++i;
		}

		if ( isPrologue )
		{
			// 开场事件和目标对象没有关系因此清空
			targets.Clear();
		}
		else
		{
			// 如果不是开场事件，目标对象数量最少需要2个
			if ( targets.Count < 2 )
			{
				Debug.LogError( "Failed to create an event." );
				return null;
			}
		}

		if ( actions.Count < 1 )
		{
			// 如果处理的事件数量小于1没有意义
			Debug.LogError( "Failed to create an event at " + beginLineCount + ".");
			return null;
		}

		Event ev = new Event( targets.ToArray(), conditions.ToArray(), actions.ToArray(), isPrologue, doContinue, eventName );
		ev.setActionLineNumbers( lineNumbers.ToArray() );

		return ev;
	}
}

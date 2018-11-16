
using System;
using UnityEngine;


/// <summary>该类用于管理全局变量</summary>
public class GlobalParam : MonoBehaviour
{
	//==============================================================================================
	// 公开方法

	/// <summary>取得游戏开始时读取的脚本文件</summary>
	public string[] getStartScriptFiles()
	{
		return m_startScripts;
	}

	/// <summary>设定游戏开始时读取的脚本文件</summary>
	public void setStartScriptFiles( params string[] files )
	{
		m_startScripts = new string[ files.Length ];
		Array.Copy( files, m_startScripts, files.Length );
	}

	//==============================================================================================
	// 非公开方法

	/// <summary>初始化</summary>
	private void create()
	{
		m_startScripts = new string[2];

		// 不经过 Title 并且未通过实例指定脚本时，
		// 将使用此处指定的值
		//
		m_startScripts[0] = "c01_main";
		m_startScripts[1] = "c01_sub";
		//m_startScripts[0] = "Events/test/test_simple_event.txt";
	}

	//==============================================================================================
	// 非公开成员变量

	/// <summary>游戏开始时读取的脚本文件</summary>
	private string[] m_startScripts;

	/// <summary>Singleton 实例</summary>
	private static GlobalParam m_instance = null;


	//==============================================================================================
	// 静态方法

	/// <summary>取得该类的实例 （Singleton）</summary>
	public static GlobalParam getInstance()
	{
		if ( m_instance == null )
		{
			// 附加该类创建对象
			GameObject go = new GameObject( "GlobalParam" );
			// 附加
			m_instance = go.AddComponent< GlobalParam >();

			m_instance.create();

			// 切换场景时不要销毁该对象
			DontDestroyOnLoad( go );
		}

		return m_instance;
	}
}

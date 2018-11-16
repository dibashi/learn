
using System.Collections.Generic;
using UnityEngine;


/// <summary>用于显示调试文本</summary>
public class DebugPrint : MonoBehaviour
{
	//==============================================================================================
	// 内部数据类型

	/// <summary>文本显示信息</summary>
	private struct TextItem
	{
		public int x;
		public int y;
		public string text;
		public float lifetime;
	}


	//==============================================================================================
	// MonoBehaviour 相关的成员变量／方法

	/// <summary>启动方法</summary>
	private void Start()
	{
		// 清空缓冲区
		clear();
	}

	/// <summary>GUI 处理</summary>
	private void OnGUI()
	{
		// 显示缓冲区中的文本
		foreach ( TextItem item in m_textItems )
		{
			int x = item.x * CHARA_W;
			int y = item.y * CHARA_H;

			GUI.Label( new Rect( x, y, item.text.Length * CHARA_W, CHARA_H ), item.text );
		}

		// 清空缓存
		if ( UnityEngine.Event.current.type == EventType.Repaint )
		{
			clear();
		}
	}


	//==============================================================================================
	// 非公开方法

	/// <summary>清空缓冲区</summary>
	private void clear()
	{
		m_locateX = m_locateY = 0;

		for ( int i = 0; i < m_textItems.Count; ++i )
		{
			TextItem item = m_textItems[ i ];

			if ( item.lifetime >= 0.0f )
			{
				item.lifetime -= Time.deltaTime;
				m_textItems[ i ] = item;  // 回写

				if ( item.lifetime <= 0.0f )
				{
					m_textItems.Remove( m_textItems[ i ] );
				}
			}
		}
	}

	/// <summary>设置显示位置</summary>
	private void setLocatePrivate( int x, int y )
	{
		m_locateX = x;
		m_locateY = y;
	}

	/// <summary>追加文本</summary>
	private void addText( string text, float lifetime = 0.0f )
	{
		TextItem item;
		item.x        = m_locateX;
		item.y        = m_locateY++;
		item.text     = text;
		item.lifetime = lifetime;

		m_textItems.Add( item );
	}


	//==============================================================================================
	// 非公开变量

	/// <summary>横轴显示位置</summary>
	private int m_locateX;

	/// <summary>纵轴显示位置</summary>
	private int m_locateY;

	/// <summary>文本显示信息列表</summary>
	private List< TextItem > m_textItems = new List< TextItem >();

	/// <summary>横向显示位置的单位像素</summary>
	private const int CHARA_W = 20;

	/// <summary>纵向显示位置的单位像素</summary>
	private const int CHARA_H = 20;

	/// <summary>Singleton 实例</summary>
	private static DebugPrint m_instance = null;


	//==============================================================================================
	// 静态方法

	/// <summary>获取该类的实例（Singleton）</summary>
	public static DebugPrint getInstance()
	{
		if ( m_instance == null )
		{
			// 生成该类并附加到对象上
			GameObject go = new GameObject( "DebugPrint" );
			// 附加
			m_instance = go.AddComponent< DebugPrint >();

			// 在场景切换时不销毁该对象
			DontDestroyOnLoad( go );
		}

		return m_instance;
	}

	/// <summary>显示文本</summary>
	public static void print( string text, float lifetime = 0.0f )
	{
		DebugPrint.getInstance().addText( text, lifetime );
	}

	/// <summary>设置显示位置</summary>
	public static void setLocate( int x, int y )
	{
		DebugPrint.getInstance().setLocatePrivate( x, y );
	}
}

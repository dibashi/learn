
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>这个游戏中“游戏对象”的基本结构</summary>
public class BaseObject : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 相关的成员变量／方法

	/// <summary>事件管理器对象</summary>
	protected EventManager m_eventManager = null;

	/// <summary>对话文本的背景颜色</summary>
	/// 不显示对话文字的对象则不需要设定
	public Color m_dialogBackground = new Color32( 0, 128, 255, 160 );

	/// <summary>y 坐标的上端值</summary>
	/// 显示对话文本内容和调整 DraggableObject 等魔术手的显示位置时会使用的值
	public float m_yTop = 0.0f;

	/// <summary>y 坐标的下端值</summary>
	/// 显示对话文本内容和调整 DraggableObject 等魔术手的显示位置时会使用的值
	public float m_yBottom = 0.0f;

#if UNITY_EDITOR
	/// <summary>用于通过检视器来查看游戏内参数值的成员变量</summary>
	public string[] m_debug_variables;
#endif //UNITY_EDITOR

	/// <summary>启动时执行的方法</summary>
	private void Awake()
	{
		m_eventManager = EventManager.get();

		//搜索Terrain 层索引
		m_terrainLayerIndex = LayerMask.NameToLayer( "Terrain" );
	}

	/// <summary>每帧更新的方法</summary>
	private void Update()
	{
		if ( !m_isLandingPrevious && m_isLanding )  // 着陆的瞬间
		{
			foreach ( GameObject co in m_contactingObjects )
			{
				BaseObject bo = co.GetComponent< BaseObject >();
				if ( bo == null ) continue;

				// 向事件管理器通知接触到的对象信息
				m_eventManager.addContactingObject( bo );
			}

			// 向事件管理器通知自己的信息
			m_eventManager.addContactingObject( this );
		}

		m_contactingObjects.Clear();
		m_isLandingPrevious = m_isLanding;
	}

	/// <summary>和对象的接触</summary>
	private void OnTriggerStay( Collider collider )
	{
		// 记录下发生接触的对象
		m_contactingObjects.Add( collider.gameObject );
	}

	/// <summary>和对象的碰撞</summary>
	private void OnCollisionEnter( Collision collision )
	{
		if ( collision.gameObject.layer == m_terrainLayerIndex )
		{
			m_isLanding = true;
		}
	}


	//==============================================================================================
	// 公有方法

	/// <summary>取得游戏内的参数</summary>
	/// 如果指定名称的游戏内变量不存在则返回 null
	public string getVariable( string name )
	{
		// 从字典中取出该名称对应的元素
		string value;
		bool hasData = m_variables.TryGetValue( name, out value );

		return hasData ? value : null;
	}

	/// <summary>设定游戏内参数</summary>
	public void setVariable( string name, string value )
	{
		if ( m_variables.ContainsKey( name ) )
		{
			// 如果已经存在同名的参数 →替换
			m_variables[ name ] = value;
		}
		else
		{
			// 不存在同名的参数 →添加新参数
			m_variables.Add( name, value );
		}

#if UNITY_EDITOR
		// 用编辑器执行时更新检视器的显示
		m_debug_variables = new string[ m_variables.Count ];

		int i = 0;
		foreach ( KeyValuePair< string, string > pair in m_variables )
		{
			m_debug_variables[ i++ ] = pair.Key + " = " + pair.Value;
		}
#endif //UNITY_EDITOR
	}

	/// <summary>清空游戏内参数</summary>
	public bool clearVariable( string name )
	{
		return m_variables.Remove( name );
	}

	/// <summary>清空所有的游戏内变量</summary>
	public void clearAllVariables( bool alsoGlobal )
	{
		if ( alsoGlobal )
		{
			m_variables.Clear();
		}
		else
		{
			// 删除所有不包含globalScopePrefix 的键
			IEnumerable< string > localKeys = from key in m_variables.Keys
			                                  where key.IndexOf( m_globalScopePrefix ) != 0
			                                  select key;
			foreach ( string key in localKeys )
			{
				clearVariable( key );
			}
		}
	}

	/// <summary>返回对话文本的背景颜色</summary>
	public Color getDialogBackgroundColor()
	{
		return m_dialogBackground;
	}

	/// <summary>返回y 坐标的上端值</summary>
	public float getYTop()
	{
		return m_yTop;
	}

	/// <summary>返回y 坐标的下端值</summary>
	public float getYBottom()
	{
		return m_yBottom;
	}

	/// <summary>每帧都处理从Actor来的消息</summary>
	/// 在派生类中重载本方法
	/// 返回值表示下一帧是否仍处理当前的消息
	public virtual bool updateMessage( string message, string[] parameters )
	{
		// 默认什么也不做立即结束
		return false;
	}


	//==============================================================================================
	// 非公有成员变量

	/// <summary>作为Terrain 的对象的层索引</summary>
	protected int m_terrainLayerIndex;

	/// <summary>是否已经着陆</summary>
	protected bool m_isLanding = true;

	/// <summary>前一帧是否已经着陆</summary>
	private bool m_isLandingPrevious = true;

	/// <summary>发生接触的对象</summary>
	private List< GameObject > m_contactingObjects = new List< GameObject >();

	/// <summary>用于保存游戏内参数的字典</summary>
	private Dictionary< string, string > m_variables = new Dictionary< string, string >();

	/// <summary>游戏中全局变量的前缀</summary>
	private const string m_globalScopePrefix = "_global_";
}


using System.Collections.Generic;
using UnityEngine;


/// <summary>对象管理类</summary>
public class ObjectManager : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 相关的函数变量／方法

	/// <summary>游戏开始时设置为非活动的对象</summary>
	/// 这里注册的对象没有必要预先设置为非活动（会自动被设置）
	/// 不执行 活动／非活动 切换的对象没有必要进行注册
	public BaseObject[] m_initialDeactiveObjects;


	/// <summary>启动方法</summary>
	private void Start()
	{
		foreach(BaseObject bo in m_initialDeactiveObjects){

			if(bo != null) {

				deactivate(bo);
			}
		}
	}


	//==============================================================================================
	// 公开方法

	/// <summary>查找对象</summary>
	/// 该方法也可以查找非活动对象
	public BaseObject find( string name )
	{
		// 首先探测状态为活动的对象
		GameObject go = GameObject.Find( name );
		BaseObject bo = ( go != null ) ? go.GetComponent< BaseObject >() : null;

		if ( bo == null )
		{
			// 如果找不到则尝试查找非活动对象
			if ( !m_deactiveObjects.TryGetValue( name, out bo ) )
			{
				// 找不到
				return null;
			}
		}

		// 找到了
		return bo;
	}

	/// <summary>将对象状态设置为非活动</summary>
	/// 返回值表示该对象是否为非活动状态
	public bool deactivate( BaseObject baseObject )
	{
		BaseObject boInDictionary;
		if ( m_deactiveObjects.TryGetValue( baseObject.name, out boInDictionary ) )
		{
			// 已经存在同名对象

			if ( baseObject == boInDictionary )
			{
				// 完全相同的对象
				Debug.LogWarning( "\"" + baseObject.name + "\" has already deactivated." );

				baseObject.gameObject.SetActive( false );	// 保险起见再次设置非活跃状态
				return true;
			}
			else
			{
				// 不同的对象
				Debug.LogError( "There is already a same name object in the dictionary." );
				return false;
			}
		}

		// 设置为非活动
		baseObject.gameObject.SetActive( false );
		m_deactiveObjects.Add( baseObject.name, baseObject );
		return true;
	}

	/// <summary>设置对象为活动状态</summary>
	/// 返回值表示该对象状态是否为活动
	public bool activate( BaseObject baseObject )
	{
		if ( m_deactiveObjects.ContainsKey( baseObject.name ) )
		{
			// 设置状态为活动
			baseObject.gameObject.SetActive( true );
			m_deactiveObjects.Remove( baseObject.name );

			return true;
		}

		Debug.LogWarning( "\"" + baseObject.name + "\" is NOT deactivated." );
		return false;
	}



	//==============================================================================================
	// 非公开变量

	/// <summary>存储非活动状态对象的字典</summary>
	private Dictionary< string, BaseObject > m_deactiveObjects = new Dictionary< string, BaseObject >();
}

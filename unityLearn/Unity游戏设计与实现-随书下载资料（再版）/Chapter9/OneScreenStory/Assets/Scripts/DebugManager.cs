
using System.Collections.Generic;
using UnityEngine;


/// <summary>该类用于提供观察条件参数以及修改值的功能</summary>
public class DebugManager : MonoBehaviour
{
	//==============================================================================================
	// 内部数据类型

	/// <summary>观察条件参数</summary>
	private struct WatchCV
	{
		public string target;     // 角色
		public string condition;  // 条件参数
	};


	//==============================================================================================
	// MonoBehaviour 相关的成员参数／方法

#if UNITY_EDITOR
	/// <summary>每帧更新方法</summary>
	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.P)) {

			Debug.Break();
		}
		if ( Input.GetKeyDown( KeyCode.W ) )
		{
			m_isActive = !m_isActive;
		}
	}

	/// <summary>GUI 处理方法</summary>
	private void OnGUI()
	{
		if ( m_isActive )
		{
			displayWatchCVS();
		}
	}
#endif //UNITY_EDITOR


	//==============================================================================================
	// 公开方法

	/// <summary>添加观察条件变量</summary>
	public void addWatchConditionVariable( string target, string condition )
	{
		WatchCV cv;
		cv.target    = target;
		cv.condition = condition;

		// 如果部存在于列表中则添加
		if ( m_watchCVS.FindIndex( x => ( x.target == target && x.condition == condition ) ) < 0 )
		{
			m_watchCVS.Add( cv );
		}
	}


	//==============================================================================================
	// 非公开方法

	/// <summary>显示观察条件参数</summary>
	private void displayWatchCVS()
	{
		// 初始位置
		int x = 100;
		int y =  50;
		int w = 150;
		int h =  20;

		foreach ( WatchCV cv in m_watchCVS )
		{
			GameObject go = GameObject.Find( cv.target );
			BaseObject bo = go != null ? go.GetComponent< BaseObject >() : null;
			if ( bo != null )
			{
				string status = " ";
				string value = bo.getVariable( cv.condition );

				if(value == null) {
					// 找不到条件变量（未定义）时
					status = "?";
					value  = "0";
				}

				// 显示角色名和条件变量名
				GUI.Label( new Rect( x, y, w, h ), status + cv.target + " " + cv.condition );
				// 用于改变值的文本输入框
				string newValue = GUI.TextField( new Rect( x + w, y, 50, h ), value );

				// 值改变后反映到游戏内变量
				if ( newValue != value )
				{
					bo.setVariable( cv.condition, newValue );
				}

				y += h;
			}
		}
	}


	//==============================================================================================
	// 非公开成员变量

	/// <summary>条件参数列表</summary>
	private List< WatchCV > m_watchCVS = new List< WatchCV >();

	/// <summary>该功能是否处于可用状态</summary>
	private bool m_isActive = false;
}

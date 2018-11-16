
using UnityEngine;


/// <summary>家对象专用类</summary>
public class HouseObject : BaseObject
{
	//==============================================================================================
	// 公开方法

	/// <summary>播放动画的对象</summary>
	public GameObject m_animatingObject = null;

	/// <summary>处理来自事件Actor的消息</summary>
	public override bool updateMessage( string message, string[] parameters )
	{
		bool	ret = false;

		if ( !m_isAnimated )
		{
			switch ( message )
			{

				// 打开
				case "open":
				{
					m_animatingObject.GetComponent<Animator>().SetTrigger("open");
					m_isAnimated = true;
					ret = true;
				}
				break;
	
				// 其他
				default:
				{
					// 立刻结束
					ret =  false;
				}
				break;
			}
		}
		else
		{
			if(!m_animatingObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Open")) {

				ret = true;

			} else {

				// 结束动画
				m_isAnimated = false;
				ret =  false;
			}
		}

		return(ret);
	}


	//==============================================================================================
	// 非公开变量

	/// <summary>是否正在播放动画</summary>
	private bool m_isAnimated = false;
}

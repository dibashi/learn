
using UnityEngine;


/// <summary>宝箱对象专用类</summary>
public class TreasureBoxObject : BaseObject
{
	//==============================================================================================
	// 公开方法

	/// <summary>播放动画的对象</summary>
	public GameObject m_animatingObject = null;

	/// <summary>处理来自事件Actor的消息</summary>
	public override bool updateMessage( string message, string[] parameters )
	{
		if ( !m_isAnimated )
		{
			switch ( message )
			{
			// 打开
			case "open":
				this.play_open_animation();
				m_isAnimated = true;
				return true;
				// break; return があるので “break” は実行されない.
			// 关闭
			case "close":
				this.play_close_animation();
				m_isAnimated = true;
				return true;
				// break;

			// 其他
			default:
				Debug.LogError( "Invalid message \"" + message + "\"");
				return false;  // 立刻结束
				// break;
			}
		}
		else
		{
			if (this.is_animation_playing())
			{
				return true;
			}
			else
			{
				// 动画结束
				m_isAnimated = false;
				return false;
			}
		}
	}

	private void	play_open_animation()
	{
		m_animatingObject.GetComponent<Animator>().SetTrigger("open");
	}
	private void	play_close_animation()
	{
		m_animatingObject.GetComponent<Animator>().SetTrigger("close");
	}
	private bool	is_animation_playing()
	{
		return(false);
		//return(m_animatingObject.GetComponent<Animator>().is.isPlaying);
	}
	//==============================================================================================
	// 非公开变量

	/// <summary>是否正在播放动画</summary>
	private bool m_isAnimated = false;
}


using UnityEngine;


/// <summary>该类用于当Terrain 和 DraggableObject 碰撞时播放音效</summary>
public class TerrainSoundPlayer : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 相关的成员变量／方法

	/// <summary>事件管理器对象</summary>
	public EventManager		m_manager = null;

	/// <summary>水面特效</summary>
	public bool				m_isWater = false;

	//==============================================================================================

	void	Awake()
	{
		m_manager = EventManager.get();
	}

	/// <summary>和对象发生冲突</summary>
	private void OnCollisionEnter( Collision collision )
	{
		if ( m_manager.isExecutingEvents() ) return;  // 事件执行过程中不发出声音

		DraggableObject draggable = collision.gameObject.GetComponent< DraggableObject >();
		if ( draggable != null && GetComponent<AudioSource>() != null )
		{
			GetComponent<AudioSource>().Play();
		}

		// 这里播放水面特效
		if(m_isWater) {

			EffectManager.get().playLandingWaterEffect(draggable);
		}
	}
}

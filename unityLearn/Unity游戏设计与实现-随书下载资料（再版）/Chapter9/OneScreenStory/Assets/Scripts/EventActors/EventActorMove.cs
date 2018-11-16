
using UnityEngine;


/// <summary>move 指令事件Actor</summary>
public class EventActorMove : EventActor
{
	//==============================================================================================
	// 公有方法

	/// <summary>构造函数</summary>
	public EventActorMove( BaseObject target, BaseObject to, float duration )
	{
		m_target         = target;
		m_targetPosition = m_target.transform.position;  // 记录当前的坐标
		m_to             = to;
		m_beginTime      = Time.time;
		m_endTime        = m_beginTime + duration;
	}

	/// <summary>在Actor被销毁前每帧执行的方法</summary>
	public override void execute( EventManager evman )
	{
		Vector3 presentPosition;

		if ( Time.time >= m_endTime )
		{
			presentPosition = m_to.transform.position;
			m_isDone = true;
		}
		else
		{
			// 完成进度（0.0～1.0）
			float ratio = Mathf.InverseLerp( m_beginTime, m_endTime, Time.time );

			presentPosition = Vector3.Lerp( m_targetPosition, m_to.transform.position, ratio );
		}

		// 调整y坐标（设置和Terrain 的碰撞点）
		RaycastHit hit;
		if ( Physics.Raycast( presentPosition + 10000.0f * Vector3.up,
		                      -Vector3.up,
		                      out hit,
		                      float.PositiveInfinity,
		                      1 << LayerMask.NameToLayer( "Terrain" ) ) )
		{
			m_target.transform.position = hit.point;
		}
		else
		{
			m_target.transform.position = new Vector3( presentPosition.x, 0.0f, presentPosition.z );
		}
	}

	/// <summary>判断Actor必须执行的处理是否结束</summary>
	public override bool isDone()
	{
		return m_isDone;
	}

	/// <summary>判断执行结束后是否需要等待点击</summary>
	public override bool isWaitClick( EventManager evman )
	{
		return false;
	}


	//==============================================================================================
	// 私有变量

	/// <summary>移动对象</summary>
	private BaseObject m_target;

	/// <summary>移动前的坐标</summary>
	private Vector3 m_targetPosition;

	/// <summary>移动目标对象</summary>
	private BaseObject m_to;

	/// <summary>移动的开始时间</summary>
	private float m_beginTime;

	/// <summary>移动的结束时间</summary>
	private float m_endTime;

	/// <summary>Actor的处理是否结束</summary>
	private bool m_isDone = false;


	//==============================================================================================
	// 静态方法

	/// <summary>生成事件Actor的实例</summary>
	public static EventActorMove CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 3 )
		{
			ObjectManager om = manager.GetComponent< ObjectManager >();
			BaseObject target = om.find( parameters[ 0 ] );
			BaseObject to     = om.find( parameters[ 1 ] );
			float duration;

			if ( target != null && to != null && float.TryParse( parameters[ 2 ], out duration ) )
			{
				// 生成Actor
				EventActorMove actor = new EventActorMove( target, to, duration );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}

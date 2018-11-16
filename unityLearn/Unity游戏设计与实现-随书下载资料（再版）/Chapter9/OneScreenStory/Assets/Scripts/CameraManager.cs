
using UnityEngine;


/// <summary>管理摄像机位置／旋转角度／平行投影尺寸的类</summary>
public class CameraManager : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 相关的成员变量／方法

	/// <summary>Terrain 的左端</summary>
	public float m_terrainEndLeft;
	/// <summary>Terrain 的右端</summary>
	public float m_terrainEndRight;
	/// <summary>Terrain 靠近前面一端</summary>
	public float m_terrainEndFront;
	/// <summary>Terrain 的内部端</summary>
	public float m_terrainEndBack;
	/// <summary>Terrain 内部的背景上端</summary>
	public float m_backgroundTop;

	/// <summary>启动方法</summary>
	private void Start()
	{
		// 记录下初始位置（＝开始时的当前位置）
		m_originalPosition  = m_currentPosition  = transform.position;
		m_originalRotationX = m_currentRotationX = transform.eulerAngles.x;
		m_originalSize      = m_currentSize      = GetComponent<Camera>().orthographicSize;
	}

	/// <summary>每帧更新方法</summary>
	private void Update()
	{
		if ( m_isMoving )
		{
			if ( Time.time >= m_endTime )
			{
				// 摄像机经过移动时间后首次 Update

				// 现在的位置 ＝ 目标位置
				transform.position      = m_currentPosition = m_destinationPosition;
				GetComponent<Camera>().orthographicSize = m_currentSize     = m_destinationSize;
				m_currentRotationX = m_destinationRotationX;
				transform.eulerAngles = new Vector3( m_currentRotationX, transform.eulerAngles.y, transform.eulerAngles.z );

				// 移动结束
				m_isMoving = false;
			}
			else
			{
				// 完成度（0.0～1.0）
				float ratio = Mathf.InverseLerp( m_beginTime, m_endTime, Time.time );

				// 对角度和投影的尺寸进行补间
				transform.eulerAngles = new Vector3( Mathf.Lerp( m_currentRotationX, m_destinationRotationX, ratio ),
				                                     transform.eulerAngles.y, transform.eulerAngles.z );
				GetComponent<Camera>().orthographicSize = Mathf.Lerp( m_currentSize, m_destinationSize, ratio );

				// 用补间对位置进行调试使 Terrain 的两端无法被看见
				transform.position = fixPosition( Vector3.Lerp( m_currentPosition, m_destinationPosition, ratio ), transform.eulerAngles.x, GetComponent<Camera>().orthographicSize );
			}
		}
	}


	//==============================================================================================
	// 公共方法

	/// <summary>向指定位置移动</summary>
	public void moveTo( Vector3 destinationPosition, float destinationRotationX, float destinationSize, float duration )
	{
		// 调整坐标
		destinationPosition = fixPosition( destinationPosition, destinationRotationX, destinationSize );

		m_destinationPosition  = destinationPosition;
		m_destinationRotationX = destinationRotationX;
		m_destinationSize      = destinationSize;

		m_beginTime = Time.time;
		m_endTime   = m_beginTime + duration;
		m_isMoving  = true;
	}

	/// <summary>获取初始位置</summary>
	public Vector3 getOriginalPosition()
	{
		return m_originalPosition;
	}

	/// <summary>获取初始 x 轴的旋转角度</summary>
	public float getOriginalRotationX()
	{
		return m_originalRotationX;
	}

	/// <summary>获取初始的平行投影尺寸</summary>
	public float getOriginalSize()
	{
		return m_originalSize;
	}

	/// <summary>取得当前的位置</summary>
	/// 如果摄像机正在移动则取移动前的位置
	public Vector3 getCurrentPosition()
	{
		return m_currentPosition;
	}

	/// <summary>取得当前 x 轴的旋转角度</summary>
	/// 如果摄像机正在移动时则取开始移动前的旋转角度
	public float getCurrentRotationX()
	{
		return m_currentRotationX;
	}

	/// <summary>获得当前的平行投影尺寸</summary>
	/// 如果摄像机正在移动则取开始移动前的尺寸
	public float getCurrentSize()
	{
		return m_currentSize;
	}

	/// <summary>摄像机是否正在移动</summary>
	public bool isMoving()
	{
		return m_isMoving;
	}


	public static CameraManager	get()
	{
		if(instance == null) {
			
			GameObject	go = GameObject.FindGameObjectWithTag("MainCamera");
			
			if(go == null) {
				
				Debug.Log("Can't find \"MainCamera\" GameObject.");
				
			} else {
				
				instance = go.GetComponent<CameraManager>();
			}
		}
		return(instance);
	}
	protected static CameraManager	instance = null;

	//==============================================================================================
	// 非公开方法

	/// <summary>调整坐标到看不见Terrain 两端的程度</summary>
	private Vector3 fixPosition( Vector3 position, float rotationX, float size )
	{
		Vector3 newPosition = new Vector3( position.x, position.y, position.z );
		float horizontalSize = size * Screen.width / Screen.height;

		// 左端
		if ( position.x - horizontalSize < m_terrainEndLeft )
		{
			newPosition.x = m_terrainEndLeft + horizontalSize;
		}

		// 右端
		if ( position.x + horizontalSize > m_terrainEndRight )
		{
			newPosition.x = m_terrainEndRight - horizontalSize;
		}

		// 靠前一端（向屏幕外）
		float terrainZOfBottom = position.z
		                       + position.y / Mathf.Tan( rotationX * Mathf.Deg2Rad )
		                       - size / Mathf.Sin( rotationX * Mathf.Deg2Rad );
		if ( terrainZOfBottom < m_terrainEndFront )
		{
			newPosition.z = position.z + m_terrainEndFront - terrainZOfBottom;
		}

		// 靠里一端
		float terrainYOfTop = position.y
		                    - ( m_terrainEndBack - position.z ) * Mathf.Tan( rotationX * Mathf.Deg2Rad )
		                    + size / Mathf.Cos( rotationX * Mathf.Deg2Rad );
		if ( terrainYOfTop > m_backgroundTop )
		{
			newPosition.z = position.z - ( terrainYOfTop - m_backgroundTop ) / Mathf.Tan( rotationX * Mathf.Deg2Rad );
		}

		return newPosition;
	}


	//==============================================================================================
	// 非公有变量

	/// <summary>初始位置</summary>
	private Vector3 m_originalPosition;

	/// <summary>初始 x 轴旋转角度</summary>
	private float m_originalRotationX;

	/// <summary>初始的平行投影尺寸</summary>
	private float m_originalSize;

	/// <summary>当前位置</summary>
	private Vector3 m_currentPosition;

	/// <summary>当前 x 轴的旋转角度</summary>
	private float m_currentRotationX;

	/// <summary>当前的平行投影尺寸</summary>
	private float m_currentSize;

	/// <summary>目标位置</summary>
	private Vector3 m_destinationPosition;

	/// <summary>目标的x轴旋转角度</summary>
	private float m_destinationRotationX;

	/// <summary>目标的平行投影尺寸</summary>
	private float m_destinationSize;

	/// <summary>摄像机开始移动的时间</summary>
	private float m_beginTime = 0.0f;

	/// <summary>摄像机移动的结束时间</summary>
	private float m_endTime = 0.0f;

	/// <summary>摄像机是否正在移动</summary>
	private bool m_isMoving = false;
}


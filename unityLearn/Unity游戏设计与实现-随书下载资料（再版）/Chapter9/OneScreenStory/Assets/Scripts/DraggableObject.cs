
using UnityEngine;


/// <summary>能够用鼠标拖动的对象</summary>
public class DraggableObject : BaseObject
{
	//==============================================================================================
	// MonoBehaviour 相关的成员变量／方法

	/// <summary>拖动鼠标时对象被抬起的高度</summary>
	public float m_pickupHeight = 150.0f;

	/// <summary>和对象的碰撞</summary>
	private void OnCollisionEnter( Collision collision )
	{
		if ( !m_isDragging && collision.gameObject.layer == m_terrainLayerIndex )
		{
			m_isLanding = true;

// Unity 5.0 以降で TerrainCollider にめり込んでしまうことの対処.
// かなりむりやりですが…….
this.GetComponent<Rigidbody>().useGravity = false;
this.GetComponent<Rigidbody>().velocity = Vector3.zero;

		}
	}


	//==============================================================================================
	// 公开方法

	/// <summary>鼠标开始拖动时每帧的更新方法</summary>
	public void onDragBegin( RaycastHit hit )
	{
		// 使重力无效.
		GetComponent<Rigidbody>().useGravity = false;

		// 初始化落下速度
		GetComponent<Rigidbody>().velocity = Vector3.zero;

		// 举起对象（更新对象的位置）
		updateDragPosition();

		// 清空落地标记
		m_isLanding = false;

		// 开始拖拽
		m_isDragging = true;
	}

	/// <summary>拖拽鼠标过程中的逐帧更新方法</summary>
	public void onDragUpdate()
	{
		// 更新对象的位置
		updateDragPosition();
	}

	/// <summary>鼠标拖动结束后的逐帧更新方法</summary>
	public void onDragEnd()
	{
		// 初始化落下速度（防止在落地前反复拖动导致落下速度越来越大）
		GetComponent<Rigidbody>().velocity = Vector3.zero;

		// 使重力有效
		GetComponent<Rigidbody>().useGravity = true;

		// 结束拖动
		m_isDragging = false;
	}


	//==============================================================================================
	// 非公开方法

	/// <summary>更新拖动鼠标过程中对象的位置</summary>
	private void updateDragPosition()
	{
		// 对象的移动目标
		Vector3 moveTo = Vector3.zero;

		// 求出位于鼠标光标位置的 Terrain 坐标
		Vector3 mousePosition = Input.mousePosition;
		Ray rayFromMouse = Camera.main.ScreenPointToRay( mousePosition );
		RaycastHit hitFromMouse;
		if ( mousePosition.x >= 0.0f && mousePosition.x <= Screen.width  &&
		     mousePosition.y >= 0.0f && mousePosition.y <= Screen.height &&
		     Physics.Raycast( rayFromMouse, out hitFromMouse, float.PositiveInfinity, 1 << m_terrainLayerIndex ) )
		{
			// 以Terrain 坐标位基准决定移动目标
			moveTo = hitFromMouse.point + m_pickupHeight * Vector3.up;
		}
		else
		{
			// 鼠标光标位于画面外 or 鼠标光标所在位置没有 Terrain 

			// 保持现在的位置
			moveTo = transform.position;
		}

		// 执行补间让举起过程更为平滑自然
		moveTo.y = Mathf.Lerp( transform.position.y, moveTo.y, 0.3f );

		transform.position = moveTo;
	}


	//==============================================================================================
	// 非公开变量

	/// <summary>是否在拖动过程中</summary>
	private bool m_isDragging = false;
}

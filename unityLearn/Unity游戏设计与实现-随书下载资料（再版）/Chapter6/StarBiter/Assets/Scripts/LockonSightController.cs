using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 控制锁定瞄准器
// ----------------------------------------------------------------------------
public class LockonSightController : MonoBehaviour {
	
	public GameObject lockonEnemy;				// 锁定的敌机
	public bool isEnabled = false;				// 使瞄准器有效
	
	void Update ()
	{
		// 如果存在敌机使用瞄准器跟踪敌机
		if ( lockonEnemy )
		{
			// 跟踪敌机
			transform.position = new Vector3(
				lockonEnemy.transform.position.x,
				lockonEnemy.transform.position.y + 1f,
				lockonEnemy.transform.position.z );
		}
		
		// 锁定了的敌机如果不存在时将瞄准器销毁
		if ( !lockonEnemy )
		{
			if ( isEnabled )
			{
				Destroy( this.gameObject );
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 记录锁定对象的敌机
	// ------------------------------------------------------------------------
	private void SetLockonEnemy( GameObject lockonEnemy )
	{
		this.lockonEnemy = lockonEnemy;
		isEnabled = true;
	}
	
	// ------------------------------------------------------------------------
	// 销毁瞄准器
	// ------------------------------------------------------------------------
	public void Destroy()
	{
		Destroy( this.gameObject );
	}
}

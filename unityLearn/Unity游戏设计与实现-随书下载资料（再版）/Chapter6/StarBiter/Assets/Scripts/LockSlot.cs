using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 控制显示SubScreen右上方的锁定槽
// ----------------------------------------------------------------------------
public class LockSlot : MonoBehaviour {

	private bool 			isEnable = false;	// 显示有效

	public GameObject[]		uiLockSlotImages;	// 各个槽的图片
	
	void Start ()
	{
		isEnable = true;
	
		// 显示初始值
		for( int i = 0; i < uiLockSlotImages.Length; i++ )
		{
			uiLockSlotImages[i].SetActive(false);
		}
	}
	
	// ------------------------------------------------------------------------
	// 设置使用中的锁定槽数量
	// ------------------------------------------------------------------------
	public void SetLockCount( int lockCount )
	{
		if ( isEnable )
		{
			for( int i = 0; i < uiLockSlotImages.Length; i++ )
			{
				if ( i < lockCount )
				{
					uiLockSlotImages[i].SetActive(true);
				}
				else
				{
					uiLockSlotImages[i].SetActive(false);
				}
			}			
		}
	}
}

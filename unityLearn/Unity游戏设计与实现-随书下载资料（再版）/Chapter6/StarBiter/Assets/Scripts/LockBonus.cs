using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 用于显示SubScreen右上方的锁定奖励
// ----------------------------------------------------------------------------
public class LockBonus : MonoBehaviour {
	
	public UnityEngine.Sprite[]		uiLockBonusSprites;		// 锁定奖励 x0 ~ x64 图片（精灵图片）
	public UnityEngine.UI.Image		uiLockBonusImage;		// 锁定奖励的图片

	private bool isEnable = false;			// 显示有效

	// ================================================================ //

	void Start ()
	{
		isEnable = true;

		// 显示初始值
		if(this.uiLockBonusSprites.Length > 0) {

			this.uiLockBonusImage.sprite = this.uiLockBonusSprites[0];
		}
	}
	
	// ------------------------------------------------------------------------
	// 显示指定的锁定奖励的图像
	// ------------------------------------------------------------------------
	public void SetLockCount( int lockCount )
	{
		if ( isEnable )
		{
			if(0 <= lockCount && lockCount < this.uiLockBonusSprites.Length) {

				this.uiLockBonusImage.sprite = this.uiLockBonusSprites[lockCount];
			}
		}
	}
}

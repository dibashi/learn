using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 游戏对象在画面外从下往上滚动
// ----------------------------------------------------------------------------
public class EndingScrollController : MonoBehaviour {

	public GameObject	uiStaffRoll;
	public FadeOut		fadeOut;

	public float		scrollSpeed				= 32.0f;		// 滚动速度
	public float		startPosition			= -1850;		// 滚动的开始位置
	public float 		distanceToStartEaseIn   = 0.05f;		// EaseIn开始位置到结束时的距离
	public bool 		isStoppedStarScroll		= true;
	
	private float		stopPositionY;							// 停止滚动的位置

	private OpeningSpaceController	endingSpace;

	private bool		isEaseIn = false;

	
	void Start () 
	{	
		// 获取停止滚动的位置

		this.stopPositionY = this.uiStaffRoll.GetComponent<RectTransform>().localPosition.y;
		
		// 移动消息到初始显示的位置
		Vector3		tmpPosition = this.uiStaffRoll.GetComponent<RectTransform>().localPosition;
		this.uiStaffRoll.GetComponent<RectTransform>().localPosition = new Vector3(tmpPosition.x, this.startPosition, tmpPosition.z);
	
		this.endingSpace = GameObject.Find("EndingSpace").GetComponent<OpeningSpaceController>();
	}
	
	void FixedUpdate () 
	{
		// 滚动到停止位置

		Vector3		position = this.uiStaffRoll.GetComponent<RectTransform>().localPosition;
		
		if(this.isEaseIn) {

			// EaseIn
			position.y += (Mathf.Abs(this.stopPositionY - position.y)/this.distanceToStartEaseIn )*this.scrollSpeed*Time.deltaTime;

			this.uiStaffRoll.GetComponent<RectTransform>().localPosition = position;

		} else {

			if(Mathf.Abs(this.stopPositionY - position.y ) < this.distanceToStartEaseIn) {

				// 开始EaseIn
				this.isEaseIn = true;
				if(this.isStoppedStarScroll) {

					this.endingSpace.SetEaseIn();
					this.fadeOut.SetEnable();
				}

			} else {

				// 滚动
				position.y += this.scrollSpeed*Time.deltaTime;
				this.uiStaffRoll.GetComponent<RectTransform>().localPosition = position;
			}

		}
	}
}

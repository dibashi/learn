using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 游戏对象横向滚动
// ----------------------------------------------------------------------------
public class EndingSideScrollController : MonoBehaviour {

	public float		scrollSpeed   = 10.0f;				// 滚动速度
	public Vector3 		startPosition = Vector3.zero;		// 滚动的开始位置

	protected float 	distanceToStartEaseIn = 10.0f;		// EaseIn开始位置到结束时的距离
	
	protected Vector3	stopPosition;						// 停止滚动的位置

	public GameObject	uiImage;

	void Awake () 
	{
		// 获取滚动的游戏对象实例
		this.stopPosition = this.uiImage.GetComponent<RectTransform>().localPosition;
	
		// 获取滚动的停止位置
		Vector3	v = this.stopPosition - this.startPosition;
		v.z = 0.0f;
	
		this.distanceToStartEaseIn = v.magnitude*0.1f;

		// 移动到游戏对象的初始显示位置
		Vector3 tmpPosition = this.uiImage.GetComponent<RectTransform>().localPosition;
		this.uiImage.GetComponent<RectTransform>().localPosition = new Vector3(startPosition.x, startPosition.y, tmpPosition.z);
	}
	
	void FixedUpdate () 
	{	
		// 滚动到停止位置
		Vector3 	position = this.uiImage.GetComponent<RectTransform>().localPosition;
		Vector3		to_goal  = this.stopPosition - position;

		to_goal.z = 0.0f;

		if(to_goal.magnitude < this.distanceToStartEaseIn) {

			// EaseIn
			float		rate = to_goal.magnitude/this.distanceToStartEaseIn;

			rate = Mathf.Max(0.01f, rate);

			float		additionDistance = rate*this.scrollSpeed*Time.deltaTime;

			if(to_goal.magnitude < additionDistance) {

				position = this.stopPosition;

			} else {

				to_goal.Normalize();
				position += to_goal*additionDistance;
			}

		} else {

			float		additionDistance = this.scrollSpeed * Time.deltaTime;

			to_goal.Normalize();

			// 滚动
			position += to_goal*additionDistance;
		}

		this.uiImage.GetComponent<RectTransform>().localPosition = position;
	}
}

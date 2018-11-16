using UnityEngine;
using System.Collections;

public class RankDisp : MonoBehaviour {

	protected const float	ZOOM_TIME = 0.4f;

	public float	timer = 0.0f;
	public float	scale = 1.0f;
	public float	alpha = 0.0f;

	public UnityEngine.UI.Image		uiImageGrade;		// 用于显示评价文字（优/良/可/不可）的图片

	public UnityEngine.Sprite[]		uiSpriteRank;		// 用于显示评价击杀数量，击杀敏捷度（优/良/可/不可）的精灵图片

	// ================================================================ //
	// 继承于MonoBehaviour

	void	Awake()
	{
	}

	void	Start()
	{
	}
	
	void	Update()
	{
		float	delta_time = Time.deltaTime;

		this.update_sub();

		this.timer += delta_time;
	}

	protected void		update_sub()
	{
		float	zoom_in_time = ZOOM_TIME;
		float	rate;

		if(this.timer < zoom_in_time) {

			rate = this.timer/zoom_in_time;
			rate = Mathf.Pow(rate, 2.5f);
			this.scale = Mathf.Lerp(1.5f, 1.0f, rate);

		} else {

			this.scale = 1.0f;
		}

		if(this.timer < zoom_in_time) {

			rate = this.timer/zoom_in_time;
			rate = Mathf.Pow(rate, 2.5f);
			this.alpha = Mathf.Lerp(0.0f, 1.0f, rate);

		} else {

			this.alpha = 1.0f;
		}

		// 为UI.Image设置透明度

		UnityEngine.UI.Image[]		images = this.GetComponentsInChildren<UnityEngine.UI.Image>();

		foreach(var image in images) {

			Color	color = image.color;

			color.a = this.alpha;

			image.color = color;
		}

		// 设置缩放
		this.GetComponent<RectTransform>().localScale = Vector3.one*this.scale;
	}

	// ================================================================ //

	public void		startDisp(int rank)
	{
		this.uiImageGrade.sprite = this.uiSpriteRank[rank];

		this.gameObject.SetActive(true);

		this.timer = 0.0f;

		this.update_sub();
	}
	public void		hide()
	{
		this.gameObject.SetActive(false);
	}

}

using UnityEngine;
using System.Collections;

public class BGControl : MonoBehaviour {

	public SimpleSprite	sprite = null;

	public float	y_max;
	public float	y_min;

	public float	rate;

	public float	height_target;

	public bool		is_scrolling;

	// ---------------------------------------------------------------- //

	void	Start()
	{

		this.sprite = this.gameObject.AddComponent<SimpleSprite>();

		this.sprite.SetSize(9.0f*1.2f, 56.25f*1.2f);

		//

		this.y_max = this.sprite.size.y/2.0f - 10.0f;
		this.y_min = -(this.sprite.size.y/2.0f - 7.0f);

		this.rate = 0.0f;

		this.setHeightRateDirect(this.rate);

		this.is_scrolling = false;
	}

	void	Update()
	{

		// 滚动

		if(this.is_scrolling) {

			Vector3	position = this.transform.position;

			float	speed = (this.y_max - this.y_min)/(float)SceneControl.MAX_HEIGHT_LEVEL*0.1f*(60.0f*Time.deltaTime);

			if(this.height_target < position.y) {

				position.y -= speed;

				if(position.y <= this.height_target) {

					position.y = this.height_target;

					this.is_scrolling = false;
				}
			}

			this.transform.position = position;
		}
	}

	public void	setHeightRateDirect(float rate)
	{
		this.rate = rate;

		this.setHeightRate(rate);

		//

		Vector3	position = this.transform.position;

		position.y = this.height_target;

		this.transform.position = position;

		//

		this.is_scrolling = false;
	}

	public void	setHeightRate(float rate)
	{
		this.rate = rate;

		this.height_target = Mathf.Lerp(this.y_max, this.y_min, this.rate);

		this.is_scrolling = true;
	}
}

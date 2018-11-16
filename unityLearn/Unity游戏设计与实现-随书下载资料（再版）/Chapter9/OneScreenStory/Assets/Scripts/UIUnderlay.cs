using UnityEngine;
using System.Collections;

public class UIUnderlay : MonoBehaviour {

	public UnityEngine.UI.Image		ui_image_c    = null;
	public UnityEngine.UI.Image		ui_image_r    = null;
	public UnityEngine.UI.Image		ui_image_rt   = null;
	public UnityEngine.UI.Image		ui_image_rb   = null;
	public UnityEngine.UI.Image		ui_image_l    = null;
	public UnityEngine.UI.Image		ui_image_lt   = null;
	public UnityEngine.UI.Image		ui_image_lb   = null;
	public UnityEngine.UI.Image		ui_image_knob = null;

	public Vector2	center_size;
	public Color 	color;

	public const float	KNOB_SIZE   = 16.0f;
	public const float	CORNER_SIZE = 16.0f;

	public bool	is_draw_knob  = false;
	public bool is_knob_below = true;

	// ================================================================ //

	void 	Start()
	{
	
	}
	
	void 	Update()
	{
	}

	// ================================================================ //

	public void		create(Color color)
	{
		this.color = color;
		this.setSize(this.center_size);
	}

	public void		drawKnob(bool is_below, float x, Color color)
	{
		this.is_draw_knob  = true;
		this.is_knob_below = is_below;

		float	knob_size  = KNOB_SIZE;
		float	knob_x_max = this.center_size.x/2.0f - knob_size/2.0f;

		this.ui_image_knob.enabled = true;

		x = Mathf.Clamp(x, -knob_x_max, knob_x_max);

		if(this.is_knob_below) {

			this.ui_image_knob.rectTransform.localPosition = new Vector2(x, -(this.center_size.y/2.0f + knob_size/2.0f));
			this.ui_image_knob.rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

		} else {

			this.ui_image_knob.rectTransform.localPosition = new Vector2(x, this.center_size.y/2.0f + knob_size/2.0f);
			this.ui_image_knob.rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
		}

		this.ui_image_knob.color = color;
	}

	public void		setSize(Vector2	size)
	{
		this.is_draw_knob = false;
		this.center_size  = size;

		//

		float	corner_size = CORNER_SIZE;

		this.center_size.y = Mathf.Max(corner_size*2.0f, this.center_size.y);

		this.ui_image_c.rectTransform.localScale = this.center_size;
		this.ui_image_c.color = color;

		float	side_height = this.center_size.y - 2.0f*corner_size;

		if(side_height > 0.0f) {

			this.ui_image_r.enabled = true;
			this.ui_image_r.rectTransform.localPosition = new Vector2(this.center_size.x/2.0f + corner_size/2.0f, 0.0f);
			this.ui_image_r.rectTransform.localScale    = new Vector2(corner_size, side_height);
			this.ui_image_r.color = color;

			this.ui_image_l.enabled = true;
			this.ui_image_l.rectTransform.localPosition = new Vector2(-(this.center_size.x/2.0f + corner_size/2.0f), 0.0f);
			this.ui_image_l.rectTransform.localScale    = new Vector2(corner_size, side_height);
			this.ui_image_l.color = color;

		} else {

			this.ui_image_r.enabled = false;
			this.ui_image_l.enabled = false;
		}

		this.ui_image_rt.rectTransform.localPosition = new Vector2(  this.center_size.x/2.0f + corner_size/2.0f,  this.center_size.y/2.0f - corner_size/2.0f);
		this.ui_image_rt.color = color;

		this.ui_image_rb.rectTransform.localPosition = new Vector2(  this.center_size.x/2.0f + corner_size/2.0f, -this.center_size.y/2.0f + corner_size/2.0f);
		this.ui_image_rb.color = color;

		this.ui_image_lt.rectTransform.localPosition = new Vector2(-(this.center_size.x/2.0f + corner_size/2.0f), this.center_size.y/2.0f - corner_size/2.0f);
		this.ui_image_lt.color = color;

		this.ui_image_lb.rectTransform.localPosition = new Vector2(-(this.center_size.x/2.0f + corner_size/2.0f), -this.center_size.y/2.0f + corner_size/2.0f);
		this.ui_image_lb.color = color;

		this.ui_image_knob.enabled = false;
	}

	public Rect		getRect()
	{
		float	corner_size = CORNER_SIZE;

		float	x_max =   this.center_size.x/2.0f + corner_size;
		float	x_min = -(this.center_size.x/2.0f + corner_size);
		float	y_max =  this.center_size.y/2.0f;
		float	y_min = -this.center_size.y/2.0f;

		if(this.is_draw_knob) {

			if(this.is_knob_below) {

				y_min -= KNOB_SIZE;

			} else {

				y_max += KNOB_SIZE;
			}
		}

		Rect	rect = Rect.MinMaxRect(x_min, y_min, x_max, y_max);

		return(rect);
	}
}

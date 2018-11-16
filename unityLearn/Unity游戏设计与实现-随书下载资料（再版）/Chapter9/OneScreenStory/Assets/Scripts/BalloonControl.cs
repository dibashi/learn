using UnityEngine;
using System.Collections;

public class BalloonControl : MonoBehaviour {

	public UnityEngine.UI.Text		ui_text = null;

	public GameObject	underlay_prefab;
	public UIUnderlay	underlay;


	protected float	center_width;
	protected float	center_height;

	// ================================================================ //

	void	Awake()
	{
		// 垫在下面
		this.underlay = GameObject.Instantiate(this.underlay_prefab).GetComponent<UIUnderlay>();
		this.underlay.GetComponent<RectTransform>().SetParent(this.GetComponent<RectTransform>());
		this.underlay.GetComponent<RectTransform>().SetSiblingIndex(0);

		this.gameObject.SetActive(false);
	}

	void	Start()
	{	
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	public void	show()
	{
		this.gameObject.SetActive(true);
	}

	public void	hide()
	{
		this.gameObject.SetActive(false);
	}

	public void		setPosition(Vector2 center)
	{
		// 为了字体使用不出现异常，提前将位置坐标取为整数
		center.x = Mathf.Round(center.x);
		center.y = Mathf.Round(center.y);

		this.gameObject.GetComponent<RectTransform>().localPosition = center;
	}

	public Rect		getRect()
	{
		Rect	rect = this.underlay.getRect();

		Vector3		center = this.gameObject.GetComponent<RectTransform>().localPosition;

		float	x_min = rect.xMin + center.x;
		float	x_max = rect.xMax + center.x;
		float	y_min = rect.yMin + center.y;
		float	y_max = rect.yMax + center.y;

		rect = Rect.MinMaxRect(x_min, y_min, x_max, y_max);

		return(rect);
	}

	public void		setText(string text, Color color, Vector2 center)
	{
		this.ui_text.text = text;

		this.gameObject.GetComponent<RectTransform>().localPosition = center;

		//

		Vector2		text_size = TextManager.createTextRect(this.ui_text);

		float	h_margin    = 16.0f;
		float	v_margin    = 8.0f;

		this.center_width  = text_size.x + h_margin*2.0f;
		this.center_height = text_size.y + v_margin*2.0f;

		this.underlay.center_size = new Vector2(this.center_width, this.center_height);
		this.underlay.create(color);

	}

	public void		drawKnob(bool is_below, float x, Color color)
	{
		this.underlay.drawKnob(is_below, x, color);
	}
}

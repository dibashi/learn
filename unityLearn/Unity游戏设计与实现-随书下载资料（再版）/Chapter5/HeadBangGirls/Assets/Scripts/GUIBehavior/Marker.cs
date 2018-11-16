using UnityEngine;
using System.Collections;

public class Marker : MonoBehaviour {

	protected const float	TEXTUER_SIZE = 32.0f;

	public UnityEngine.UI.Text	ui_line_number_text;

	// ================================================================ //

	void 	Awake()
	{
	}

	void 	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	public void	setVisible(bool is_visible)
	{
		this.gameObject.SetActive(is_visible);
	}

	public void	draw(float pos_x, float pos_y, float size)
	{
		this.setPosition(pos_x, pos_y);
		this.setSize(size);
		this.setVisible(true);
	}

	public void	setPosition(float x, float y)
	{
		this.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0.0f);
	}

	public void	setSize(float size)
	{
		this.GetComponent<RectTransform>().localScale = Vector3.one*size/TEXTUER_SIZE;
	}

	public void	dispLineNumberText(int index)
	{
		this.ui_line_number_text.gameObject.SetActive(true);
		this.ui_line_number_text.text = index.ToString();
	}
	public void	hideLineNumberText()
	{
		this.ui_line_number_text.gameObject.SetActive(false);
	}
}

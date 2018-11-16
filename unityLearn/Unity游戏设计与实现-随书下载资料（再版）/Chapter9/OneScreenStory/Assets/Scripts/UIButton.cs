using UnityEngine;
using System.Collections;

public class UIButton : MonoBehaviour {

	public GameObject	underlay_prefab;
	public UIUnderlay	underlay;

	public UnityEngine.UI.Text		ui_text;
	public UnityEngine.UI.Button	ui_button;
	
	// ================================================================ //

	void	Awake()
	{
		this.ui_text   = this.GetComponentInChildren<UnityEngine.UI.Text>();
		this.ui_button = this.GetComponent<UnityEngine.UI.Button>();

		// 垫在下方
		this.underlay = GameObject.Instantiate(this.underlay_prefab).GetComponent<UIUnderlay>();
		this.underlay.GetComponent<RectTransform>().SetParent(this.GetComponent<RectTransform>());
		this.underlay.GetComponent<RectTransform>().SetSiblingIndex(0);
	}

	void	Start()
	{	
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	public void		create(string text)
	{
		this.create(text, Color.white, new Color(0.0f, 0.0f, 0.0f, 0.5f));
	}

	public void		create(string text, Color text_color, Color under_color)
	{
		this.name = text;

		this.ui_text.text  = text;
		this.ui_text.color = text_color;

		Vector2		text_size = TextManager.createTextRect(this.ui_text);

		float	h_margin    = 16.0f;
		float	v_margin    = 8.0f;

		text_size.x += h_margin*2.0f;
		text_size.y += v_margin*2.0f;

		// 垫在下方
		this.underlay.center_size = text_size;
		this.underlay.create(under_color);
	}
}

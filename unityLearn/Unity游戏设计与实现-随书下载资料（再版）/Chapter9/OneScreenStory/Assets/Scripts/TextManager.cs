using System.Collections.Generic;
using UnityEngine;


/// <summary>该类用于显示说明文字／对话文字</summary>
public class TextManager : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 相关的成员变量／方法

	public GameObject	ui_button_prefab  = null;
	public GameObject	ui_balloon_prefab = null;
	public GameObject	underlay_prefab   = null;
	public GameObject	title_go          = null;

	protected const float	BALLOON_OBJECT_SPACE = 4.0f;				// 吹き出し部分とオブジェクトのすき間.
	protected const float	SCREEN_MARGIN_OF_BALLOON = 16.0f;			// スクリーン端の、吹き出しを表示しないエリア.

	protected readonly Color 	TEXT_BACK_COLOR = new Color(0.0f, 0.0f, 0.0f, 0.6f);	// 地の文の背景色.

	protected AudioSource 		dialog_sound_source = null;				// 会話文を表示するときに鳴らすオーディオソース.
	protected BalloonControl	balloon_control = null;
	protected List<UIButton>	buttons;

	public string			selected_button = "";						// 押されたボタンのテキスト.

	// ================================================================ //
	// 公开方法

	void	Awake()
	{
		this.dialog_sound_source = this.GetComponent<AudioSource>();
	}

	// ================================================================ //

	public void	showTitle()
	{
		this.title_go.SetActive(true);
	}

	/// <summary>隐藏说明文字／对话文字</summary>
	public void	hide()
	{
		if(this.balloon_control != null) {

			GameObject.Destroy(this.balloon_control.gameObject);
		}
	}

	/// <summary>显示说明文字</summary>
	public void showText( string text, Vector2 center, float marginX, float marginY, float radius )
	{
		// 先隐藏
		this.hide();

		this.balloon_control = this.create_balloon();
		this.balloon_control.setText(text, TEXT_BACK_COLOR, new Vector2(0.0f, -200.0f));

		this.balloon_control.show();
	}

	/// <summary>显示对话文字</summary>
	public void showDialog( BaseObject baseObject, string text, float marginX, float marginY, float radius )
	{
		// 先隐藏
		this.hide();
	
		this.balloon_control = this.create_balloon();
		this.balloon_control.setText(text, baseObject.getDialogBackgroundColor(), Vector2.zero);

		Rect	canvas_rect = this.GetComponent<RectTransform>().rect;

		float	screen_right  = canvas_rect.xMax;
		float	screen_left   = canvas_rect.xMin;
		float	screen_top    = canvas_rect.yMax;

		Rect 	balloon_rect = this.balloon_control.underlay.getRect();
		float 	balloon_width  = balloon_rect.width;
		float 	balloon_height = balloon_rect.height;

		Vector3		bo_pos = baseObject.gameObject.transform.position;

		// 获取GameObject 位于屏幕上的坐标
		Vector3		screenPointTop    = Camera.main.WorldToScreenPoint(bo_pos + new Vector3(0.0f, baseObject.getYTop(), 0.0f));
		Vector3 	screenPointBottom = Camera.main.WorldToScreenPoint(bo_pos + new Vector3(0.0f, baseObject.getYBottom(), 0.0f));

		screenPointTop.x -= 320.0f;
		screenPointTop.y -= 240.0f; 
		screenPointBottom.x -= 320.0f;
		screenPointBottom.y -= 240.0f; 

		float	balloon_x = screenPointTop.x + 0.2f*balloon_width; 			 // ちょっとだけ右にずらすと吹き出しっぽくなる.
		float	knob_x    = screenPointTop.x;

		// -------------------------------------------------------- //
		// 检测是否超出画面左右两侧

		if(balloon_x - balloon_width/2.0f < screen_left + SCREEN_MARGIN_OF_BALLOON) {

			// 如果向左突出
			balloon_x = screen_left + SCREEN_MARGIN_OF_BALLOON + balloon_width/2.0f;

		} else if(balloon_x + balloon_width/2.0f > screen_right - SCREEN_MARGIN_OF_BALLOON) {

			// 如果向右突出
			balloon_x = screen_right - SCREEN_MARGIN_OF_BALLOON - balloon_width/2.0f;
		}

		// -------------------------------------------------------- //
		// 检测是否超出画面上下范围
		// 选择在头上或脚底显示（原则是头上显示）

		bool	knob_below = true;

		float	balloon_y = screenPointTop.y + BALLOON_OBJECT_SPACE + UIUnderlay.KNOB_SIZE + balloon_height/2.0f;

		if(balloon_y + balloon_height/2.0f > screen_top + SCREEN_MARGIN_OF_BALLOON) {

			// 如果对话框向上突出则显示在脚下
			balloon_y = screenPointBottom.y - (BALLOON_OBJECT_SPACE + UIUnderlay.KNOB_SIZE + balloon_height/2.0f);
			
			knob_below = false;
		}

		// -------------------------------------------------------- //

		this.balloon_control.setPosition(new Vector2(balloon_x, balloon_y));
		this.balloon_control.drawKnob(knob_below, knob_x - balloon_x, baseObject.getDialogBackgroundColor());
		this.balloon_control.show();

		// 显示对话文字时发出音效
		dialog_sound_source.Play();
	}

	// ================================================================ //

	protected BalloonControl	create_balloon()
	{
		GameObject	balloon_go = GameObject.Instantiate(this.ui_balloon_prefab);

		balloon_go.GetComponent<RectTransform>().SetParent(this.GetComponent<RectTransform>());

		return(balloon_go.GetComponent<BalloonControl>());
	}

	// ================================================================ //

	// 生成大量按钮
	public void	createButtons(string[] texts)
	{
		this.createButtons(texts, Color.white, new Color(0.0f, 0.0f, 0.0f, 0.5f));
	}
	public void	createButtons(string[] texts, Color text_color, Color under_color)
	{
		this.selected_button = "";

		//

		float	button_pitch = 8.0f;

		this.buttons = new List<UIButton>();

		Vector2		position = Vector2.zero;
		float		max_center_width = 0.0f;
		float		max_width        = 0.0f;
		float		whole_height     = 0.0f;

		// 生成按钮
		foreach(var text in texts) {

			UIButton	button = this.createButton(text, text_color, under_color);

			this.buttons.Add(button);

			max_center_width = Mathf.Max(max_center_width, button.underlay.center_size.x);
			max_width = Mathf.Max(max_width, button.underlay.getRect().width);

			whole_height += button.underlay.getRect().height;
		}

		whole_height += button_pitch*(texts.Length  - 1);

		// 确定位置

		position.y = whole_height/2.0f;

		if(this.balloon_control != null) {

			// 当对话框显示时，设置在不会发生重叠的位置

			Rect	balloon_rect = this.balloon_control.getRect();

			if(-max_width/2.0f <= balloon_rect.xMax && balloon_rect.xMin <= max_width/2.0f) {

				if(balloon_rect.y > 0.0f) {

					position.y = balloon_rect.yMin;
					position.y -= button_pitch;

				} else {

					position.y = balloon_rect.yMax;
					position.y += button_pitch;
					position.y += whole_height;
				}
			}
		}

		// 排列按钮
		foreach(var button in this.buttons) {

			position.y -= button.underlay.center_size.y/2.0f;

			button.GetComponent<RectTransform>().localPosition = position;

			button.underlay.setSize(new Vector2(max_center_width, button.underlay.center_size.y));

			position.y -= button.underlay.center_size.y/2.0f + button_pitch;
		}
	}

	// 创建一个按钮
	public UIButton	createButton(string text, Color text_color, Color under_color)
	{
		UIButton	button = GameObject.Instantiate(this.ui_button_prefab).GetComponent<UIButton>();

		button.GetComponent<RectTransform>().SetParent(this.GetComponent<RectTransform>());
		button.GetComponent<RectTransform>().localPosition = Vector2.zero;

		button.create(text, text_color, under_color);

		button.ui_button.onClick.AddListener(() => this.selected_button = text);

		return(button);
	}

	// 删除所有按钮
	public void	deleteButtons()
	{
		foreach(var button in this.buttons) {

			GameObject.Destroy(button.gameObject);
		}
		this.buttons.Clear();

		this.selected_button = "";
	}

	// ================================================================ //

	public static Vector2	createTextRect(UnityEngine.UI.Text ui_text)
	{
		Rect	canvas_rect = ui_text.canvas.pixelRect;

		TextGenerationSettings	settings  = ui_text.GetGenerationSettings(new Vector2(canvas_rect.width, canvas_rect.height));
		TextGenerator			generator = ui_text.cachedTextGenerator;

		float	text_width  = generator.GetPreferredWidth(ui_text.text, settings);
		float	text_height = generator.GetPreferredHeight(ui_text.text, settings);

		return(new Vector2(text_width, text_height));
	}

	// ================================================================ //
	// 实例

	public	static TextManager	instance = null;

	public static TextManager	get()
	{
		if(TextManager.instance == null) {

			GameObject		go = GameObject.Find("TextManager");

			if(go != null) {

				TextManager.instance = go.GetComponent<TextManager>();

			} else {

				Debug.LogError("Can't find game object \"TextManager\".");
			}
		}

		return(TextManager.instance);
	}
}

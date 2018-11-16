using UnityEngine;
using System.Collections;

public class BlockControl : MonoBehaviour {

	public enum COLOR {

		NONE = -1,

		CYAN = 0,
		YELLOW,
		ORANGE,
		MAGENTA,
		GREEN,

		GRAY,

		NUM,

		NORMAL_COLOR_NUM = GRAY,
	};

	public COLOR	color = (COLOR)0;

	// ---------------------------------------------------------------- //
	void 	Start()
	{
		this.SetColor(this.color);
	}
	
	void 	Update()
	{
	
	}

	// 按下鼠标按键时
	void 	OnMouseDown()
	{
		// 被点击后颜色改变（调试用）

		COLOR	color = this.color;

		color = (COLOR)(((int)color + 1)%(int)COLOR.NORMAL_COLOR_NUM);

		this.SetColor(color);
	}

	public void	ToBeVanished(bool sw)
	{
		Color	color = this.GetComponent<Renderer>().material.color;

		if(sw) {

			color.a = 0.5f;

		} else {

			color.a = 1.0f;
		}

		this.GetComponent<Renderer>().material.color = color;
	}

	public void	SetColor(COLOR color)
	{
		this.color = color;

		Color	color_value;

		switch(this.color) {

			default:
			case COLOR.CYAN:	color_value = Color.cyan;		break;
			case COLOR.YELLOW:	color_value = Color.yellow;		break;
			case COLOR.ORANGE:	color_value = new Color(1.0f, 0.46f, 0.0f);	break;
			case COLOR.MAGENTA:	color_value = Color.magenta;	break;
			case COLOR.GREEN:	color_value = Color.green;		break;

			case COLOR.GRAY:	color_value = Color.gray;		break;
		}
		this.GetComponent<Renderer>().material.color = color_value;
	}

}

using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {

	// 方块的种类（颜色）
	public enum COLOR_TYPE {

		NONE = -1,

		CYAN = 0,
		YELLOW,
		ORANGE,
		MAGENTA,
		GREEN,
		PINK,

		RED,			// 连锁后
		GRAY,			// 连锁后
		CAKE0,			// 蛋糕
		CAKE1,			// 蛋糕

		NUM,

	};

	public static int			NORMAL_COLOR_NUM   = (int)COLOR_TYPE.RED;
	public static COLOR_TYPE	NORMAL_COLOR_FIRST = COLOR_TYPE.CYAN;
	public static COLOR_TYPE	NORMAL_COLOR_LAST  = COLOR_TYPE.PINK;
	public static COLOR_TYPE	CAKE_COLOR_FIRST = COLOR_TYPE.CAKE0;
	public static COLOR_TYPE	CAKE_COLOR_LAST  = COLOR_TYPE.CAKE1;
	
	public COLOR_TYPE	color_type = (COLOR_TYPE)0;

	public static float	SIZE_X = 1.0f;
	public static float	SIZE_Y = 1.0f;

	// 各种颜色的材质（具体在 SceneControl.cs）
	public static Material[]	materials;

	// ---------------------------------------------------------------- //

	// 普通颜色的方块？
	public bool isNormalColorBlock()
	{
		bool	ret;

		do {
			
			ret = false;

			//

			if((int)this.color_type < (int)Block.NORMAL_COLOR_FIRST) {

				break;
			}
			if((int)this.color_type > (int)Block.NORMAL_COLOR_LAST) {

				break;
			}

			//

			ret = true;

		} while(false);

		return(ret);
	}

	// 蛋糕？
	public bool isCakeBlock()
	{
		bool	ret;

		do {
			
			ret = false;

			//

			if((int)this.color_type < (int)Block.CAKE_COLOR_FIRST) {

				break;
			}
			if((int)this.color_type > (int)Block.CAKE_COLOR_LAST) {

				break;
			}

			//

			ret = true;

		} while(false);

		return(ret);
	}

	public void	setColorType(COLOR_TYPE type)
	{
		this.color_type = type;

		switch(this.color_type) {

			case COLOR_TYPE.RED:
			{
				this.GetComponent<Renderer>().material = Block.materials[(int)COLOR_TYPE.RED];
				this.GetComponent<Renderer>().material.SetFloat("_BlendRate", 0.0f);
			}
			break;

			case COLOR_TYPE.GRAY:
			{
				this.GetComponent<Renderer>().material = Block.materials[(int)COLOR_TYPE.GRAY];
				this.GetComponent<Renderer>().material.SetFloat("_BlendRate", 1.0f);
			}
			break;

			case COLOR_TYPE.CAKE0:
			{
				this.GetComponent<Renderer>().material = Block.materials[(int)COLOR_TYPE.CAKE0];
			}
			break;

			default:
			{
				if(0 <= (int)this.color_type && (int)this.color_type < Block.materials.Length) {
		
					this.GetComponent<Renderer>().material = Block.materials[(int)this.color_type];
					this.GetComponent<Renderer>().material.SetFloat("_BlendRate", 0.0f);
				}
			}
			break;
		}
	}

	public void setVisible(bool is_visible)
	{
		this.GetComponent<Renderer>().enabled = is_visible;
	}

	public bool	isVisible()
	{
		return(this.GetComponent<Renderer>().enabled);
	}

	public static COLOR_TYPE	getNextNormalColor(COLOR_TYPE color)
	{
		int	next = (int)color;

		next++;

		if(next > (int)NORMAL_COLOR_LAST) {

			next = (int)NORMAL_COLOR_FIRST;
		}

		return((COLOR_TYPE)next);
	}
}

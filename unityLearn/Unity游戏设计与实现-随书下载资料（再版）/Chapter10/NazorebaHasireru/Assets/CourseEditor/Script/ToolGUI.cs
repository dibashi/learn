using UnityEngine;
using System.Collections;
using System.Linq;

public class ToolGUI : MonoBehaviour {

	public enum BUTTON {

		NONE = -1,

		NEW = 0,					// 清空
		LOAD,						// 载入
		SAVE,						// 保存

		CREATE_ROAD,				// 生成道路
		RUN,						// 赛车奔跑

		TUNNEL_CREATE,				// 生成隧道
		TUNNEL_FORWARD,				// 向前移动隧道
		TUNNEL_BACKWARD,			// 向后移动隧道

		FOREST_CREATE,				// 生成树林
		FOREST_START_FORWARD,		// 将树林的开始地点向前移动
		FOREST_START_BACKWARD,		// 将树林的开始地点向后移动
		FOREST_END_FORWARD,			// 将树林的结束地点向前移动
		FOREST_END_BACKWARD,		// 将树林的结束地点向后移动

		BUIL_CREATE,				// 创建建筑街道
		BUIL_START_FORWARD,			// 将建筑街道开始地点往前移动
		BUIL_START_BACKWARD,		// 将建筑街道结束地点往后移动
		BUIL_END_FORWARD,			// 将建筑街道开始地点往前移动
		BUIL_END_BACKWARD,			// 将建筑街道结束地点往后移动

		JUMP_CREATE,				// 创建跳台
		JUMP_FORWARD,				// 将跳台往前移动
		JUMP_BACKWARD,				// 将跳台向后移动

		NUM,
	};

	public UIButton[] buttons;

	public AudioClip		audio_clip_click;		// 点击音效

	public bool				is_edit_mode = true;	// 是否处于编辑模式？

	// ------------------------------------------------------------------------ //

	public GameObject	uiCanvas;
	public GameObject	uiButtonPrefab;

	public GameObject	uiForestIconPrefab;
	public GameObject	uiBuildingIconPrefab;
	public GameObject	uiTunnelIconPrefab;
	public GameObject	uiJumpIconPrefab;

	// ================================================================ //
	// 继承于MonoBehaviour

	void 	Start()
	{

		this.buttons = new UIButton[(int)BUTTON.NUM];

		foreach(var i in this.buttons.Select((v, i) => i)) {

			UIButton	button = (GameObject.Instantiate(this.uiButtonPrefab) as GameObject).GetComponent<UIButton>();

			button.GetComponent<RectTransform>().SetParent(this.uiCanvas.GetComponent<RectTransform>());

			switch((BUTTON)i) {

				case BUTTON.TUNNEL_FORWARD:
				case BUTTON.TUNNEL_BACKWARD:
				case BUTTON.FOREST_START_FORWARD:
				case BUTTON.FOREST_START_BACKWARD:
				case BUTTON.FOREST_END_FORWARD:
				case BUTTON.FOREST_END_BACKWARD:
				case BUTTON.BUIL_START_FORWARD:
				case BUTTON.BUIL_START_BACKWARD:
				case BUTTON.BUIL_END_FORWARD:
				case BUTTON.BUIL_END_BACKWARD:
				case BUTTON.JUMP_FORWARD:
				case BUTTON.JUMP_BACKWARD:
				{
					button.is_repeat_button = true;
				}
				break;
			}

			this.buttons[i] = button;
		}

		this.is_edit_mode = true;

		//

		int		x, y;

		y = 210;
		x = -265;

		this.create_button_file(x, y);

		x += 105;
		this.create_button_road(x, y);

		x += 105;
		this.create_button_tunnel(x, y);

		x += 105;
		this.create_button_forest(x, y);

		x += 105;
		this.create_button_buil(x, y);

		x += 105;
		this.create_button_jump(x, y);
	}
	
	void Update ()
	{
		foreach(var i in this.buttons.Select((v, i) => i)) {

			// 按下瞬间的SE
			//
			if(this.buttons[i].trigger_on) {

				if(i == (int)BUTTON.CREATE_ROAD) {

				} else {

					this.GetComponent<AudioSource>().PlayOneShot(this.audio_clip_click);
				}
			}
		}
	}

	// ================================================================ //

	// 测试运行开始时调用的函数
	public void	onStartTestRun()
	{
		this.is_edit_mode = false;

		this.buttons[(int)BUTTON.RUN].setText("返回");
	}

	// 测试运行结束时调用的函数
	public void	onStopTestRun()
	{
		this.is_edit_mode = true;

		this.buttons[(int)BUTTON.RUN].setText("赛车奔跑");
	}

	// ================================================================ //

	// 创建按钮
	protected void	create_button(BUTTON id, int x, int y, string text)
	{
		UIButton	button = this.buttons[(int)id];

		button.setPosition(x, y);
		button.setText(text);
	}

	// 创建尺寸为一半的按钮
	protected void	create_half_button(BUTTON id, int x, int y, string text)
	{
		UIButton	button = this.buttons[(int)id];

		button.setPosition(x, y);
		button.setScale(0.4f, 1.0f);
		button.setText(text);
	}

	// 文件相关
	protected void	create_button_file(int x, int y)
	{
		this.create_button(BUTTON.NEW, x, y, "消除");
		y -= 30;

#if UNITY_EDITOR
		this.create_button(BUTTON.LOAD, x, y, "载入读取");
		y -= 30;
		this.create_button(BUTTON.SAVE, x, y, "写入");
		y -= 30;
#endif
	}


	// 道路生成相关
	protected void	create_button_road(int x, int y)
	{
		this.create_button(BUTTON.CREATE_ROAD, x, y, "生成道路");
		y -= 30;

		this.create_button(BUTTON.RUN, x, y, "赛车奔跑");
		y -= 30;
	}

	// 隧道相关
	protected void	create_button_tunnel(int x, int y)
	{
		this.create_button(BUTTON.TUNNEL_CREATE, x, y, "漫长隧道");
		y -= 30;

		this.create_half_button(BUTTON.TUNNEL_FORWARD,  x - 25,      y, "<<");
		this.create_half_button(BUTTON.TUNNEL_BACKWARD, x - 25 + 50, y, ">>");
		y -= 30;
	}

	// 树林相关
	protected void	create_button_forest(int x, int y)
	{
		this.create_button(BUTTON.FOREST_CREATE, x, y, "创建树林");
		y -= 30;

		this.create_half_button(BUTTON.FOREST_START_BACKWARD, x - 25,      y, "<<");
		this.create_half_button(BUTTON.FOREST_START_FORWARD,  x - 25 + 50, y, ">>");
		y -= 30;

		this.create_half_button(BUTTON.FOREST_END_BACKWARD, x - 25,      y, "<<");
		this.create_half_button(BUTTON.FOREST_END_FORWARD,  x - 25 + 50, y, ">>");
		y -= 30;
	}

	// 建筑相关
	protected void	create_button_buil(int x, int y)
	{
		this.create_button(BUTTON.BUIL_CREATE, x, y, "高大建筑");
		y -= 30;

		this.create_half_button(BUTTON.BUIL_START_BACKWARD, x - 25,      y, "<<");
		this.create_half_button(BUTTON.BUIL_START_FORWARD,  x - 25 + 50, y, ">>");
		y -= 30;

		this.create_half_button(BUTTON.BUIL_END_BACKWARD, x - 25,      y, "<<");
		this.create_half_button(BUTTON.BUIL_END_FORWARD,  x - 25 + 50, y, ">>");
		y -= 30;
	}

	// 跳台相关
	protected void	create_button_jump(int x, int y)
	{
		this.create_button(BUTTON.JUMP_CREATE, x, y, "还可以飞跃");
		y -= 30;

		this.create_half_button(BUTTON.JUMP_BACKWARD, x - 25,      y, "<<");
		this.create_half_button(BUTTON.JUMP_FORWARD,  x - 25 + 50, y, ">>");
		y -= 30;
	}

	// ================================================================ //

	// 创建“树林”图标
	public UIIcon		createForestIcon()
	{
		UIIcon	icon = GameObject.Instantiate(this.uiForestIconPrefab).GetComponent<UIIcon>();

		icon.GetComponent<RectTransform>().SetParent(this.uiCanvas.GetComponent<RectTransform>());

		return(icon);
	}

	// 创建“建筑”图标
	public UIIcon		createBuildingIcon()
	{
		UIIcon	icon = GameObject.Instantiate(this.uiBuildingIconPrefab).GetComponent<UIIcon>();

		icon.GetComponent<RectTransform>().SetParent(this.uiCanvas.GetComponent<RectTransform>());

		return(icon);
	}

	// // 创建“隧道”图标
	public UIIcon		createTunnelIcon()
	{
		UIIcon	icon = GameObject.Instantiate(this.uiTunnelIconPrefab).GetComponent<UIIcon>();

		icon.GetComponent<RectTransform>().SetParent(this.uiCanvas.GetComponent<RectTransform>());

		return(icon);
	}

	// // 创建“跳跃”图标
	public UIIcon		createJumpIcon()
	{
		UIIcon	icon = GameObject.Instantiate(this.uiJumpIconPrefab).GetComponent<UIIcon>();

		icon.GetComponent<RectTransform>().SetParent(this.uiCanvas.GetComponent<RectTransform>());

		return(icon);
	}
}

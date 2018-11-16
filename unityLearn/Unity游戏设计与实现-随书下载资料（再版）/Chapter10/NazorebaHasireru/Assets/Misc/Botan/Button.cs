using UnityEngine;
using System.Collections;
using MathExtension;

namespace Botan {

public class Button : ItemBase {

	public StateBit		pressed;

	public Sprite2DControl	sprite = null;

	protected float		scale_timer = 0.0f;


	public delegate	void	Func(string name);
	public Func		on_trigger_pressed = (name)=>{};

	protected struct PressingAction {

		public bool		is_active;
		public bool		press_down;
	};
	protected PressingAction	press_act;

	// ================================================================ //
	// 继承于MonoBehaviour

	public Sprite2DControl	getSprite()
	{
		return(this.sprite);
	}

	public void		setActive(bool is_active)
	{
		this.is_active = is_active;

		if(this.is_active) {

			this.sprite.setVertexColor(Color.white);

		} else {

			this.sprite.setVertexColor(new Color(0.7f, 0.7f, 0.7f));
		}
	}

	void	Awake()
	{
		this.reset();
	}
	
	void	Start()
	{
	}
	
	void	Update()
	{
	}

	public override void		reset()
	{
		base.reset();

		this.pressed = this.focused;

		this.press_act.is_active = false;
		this.press_act.press_down = false;

		this.scale_timer = 0.0f;

		if(this.sprite != null) {

			this.sprite.setScale(Vector2.one);
		}
	}

	public override void	execute_entity()
	{
		// ---------------------------------------------------------------- //
		// 点击

		this.pressed.previous = this.pressed.current;
		this.pressed.current  = false;
		
		if(this.root.input.button.trigger_on) {

			if(this.is_active) {

				if(this.focused.current) {
	
					this.pressed.current = true;
				}
			}
		}

		// 如果按下时已被激活，设置为按下状态
		if(this.press_act.is_active) {

			this.pressed.current = true;
		}

		this.pressed.update_trigger();

		if(this.pressed.trigger_on) {

			//this.on_trigger_pressed(this.name);

			this.press_act.is_active  = true;
			this.press_act.press_down = true;
		}

		// ---------------------------------------------------------------- //
		// 按下时的表现

		if(this.press_act.is_active) {

			if(this.press_act.press_down) {

				// 从原始位置到按下后的位置
				this.scale_timer -= Time.deltaTime;

				if(this.scale_timer <= 0.0f) {

					this.scale_timer = 0.0f;

					this.press_act.press_down = false;
				}

			} else {

				// 从按下状态 恢复到原始状态
				this.scale_timer += Time.deltaTime;

				if(this.scale_timer >= 0.1f) {

					this.scale_timer = 0.1f;
					this.press_act.is_active = false;

					this.on_trigger_pressed(this.name);
				}
			}

			this.sprite.setScale(Vector2.one*Mathf.Lerp(1.0f, 1.2f, this.scale_timer/0.1f));

		} else {

			if(this.focused.current) {
				
				this.scale_timer += Time.deltaTime;
				
			} else {
				
				this.scale_timer -= Time.deltaTime;
			}

			this.scale_timer = Mathf.Clamp(this.scale_timer, 0.0f, 0.1f);

			this.sprite.setScale(Vector2.one*Mathf.Lerp(1.0f, 1.2f, this.scale_timer/0.1f));
		}

		// ---------------------------------------------------------------- //

		// 获得焦点的按钮显示在最前面
		if(this.focused.current) {

			this.sprite.setDepthOffset(BotanRoot.FORCUSED_BUTTON_DEPTH);

		} else {

			this.sprite.setDepthOffset(0.0f);
		}
	}

	// ================================================================ //

	// 创建
	public void		create(Texture texture, Vector2 size)
	{
		this.sprite = Sprite2DRoot.get().createSprite(texture, 2, true);
		this.sprite.setSize(size);
		this.sprite.setDepthLayer("ui.item");
	}	

	// 设定位置
	public override void		setPosition(Vector2 position)
	{
		this.sprite.setPosition(position);
	}

	// 设定 显示/隐藏
	public override void		setVisible(bool is_visible)
	{
		base.setVisible(is_visible);

		this.sprite.setVisible(this.is_visible);
	}

	// 判断按钮是否位于某个项目上
	public override bool		isContainPoint(Vector2 point)
	{
		return(this.sprite.isContainPoint(point));
	}

}

} // namespace Botan

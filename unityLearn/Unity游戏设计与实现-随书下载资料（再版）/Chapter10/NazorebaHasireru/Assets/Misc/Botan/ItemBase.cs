using UnityEngine;
using System.Collections;
using MathExtension;

namespace Botan {

public class ItemBase : MonoBehaviour {

	protected bool		is_visible  = true;
	protected bool		is_active   = true;
	public bool		is_can_drag = false;

	public string		group_name;
	
	public struct StateBit {

		public bool	trigger_on;

		public bool	previous;
		public bool	current;

		public void	update_trigger()
		{
			this.trigger_on = (!this.previous && this.current);
		}
	};

	public StateBit		focused;
	public StateBit		dragging;

	protected Vector2	grab_offset = Vector2.zero;

	public BotanRoot	root;

	// ================================================================ //

	public virtual void		reset()
	{
		this.focused.trigger_on = false;
		this.focused.previous   = false;
		this.focused.current    = false;
	}

	// 设置位置
	public virtual void		setPosition(Vector2 position)
	{
	}

	// 获取位置
	public virtual Vector2	getPosition()
	{
		return(Vector2.zero);
	}

	// 设置显示/隐藏
	public virtual void		setVisible(bool is_visible)
	{
		this.is_visible = is_visible;

		if(!this.is_visible) {

			this.reset();
		}
	}

	public void		execute(bool is_focusable)
	{
		if(this.is_visible) {

			this.update_focuse_status(is_focusable);

			if(this.is_can_drag) {

				this.execute_dragging();
			}

			this.execute_entity();
		}
	}

	public virtual void		execute_entity()
	{

	}

	// 更新焦点获取状态
	protected void	update_focuse_status(bool is_focusable)
	{
		this.focused.previous = this.focused.current;
		
		// ---------------------------------------------------------------- //
		// 获得焦点（ROLL_OVER鼠标进入）

		Vector2		mouse_pos = this.root.input.mouse_position;

		mouse_pos = Sprite2DRoot.get().convertMousePosition(mouse_pos);

		this.focused.current = false;

		if(this.is_active) {

			if(is_focusable) {

				if(this.isContainPoint(mouse_pos)) {
	
					this.focused.current = true;	
				}
			}

		} else {

			// 如果从禁止焦点转移到焦点状态时，
			// 将一直保持焦点状态直到失去焦点

			if(this.focused.previous) {

				if(is_focusable) {
	
					if(this.isContainPoint(mouse_pos)) {
		
						this.focused.current = true;	
					}
				}
			}
		}

		this.focused.update_trigger();
	}

	// 拖动操作
	protected void		execute_dragging()
	{
		Vector2		mouse_pos = this.root.input.mouse_position;

		mouse_pos = Sprite2DRoot.get().convertMousePosition(mouse_pos);

		if(this.dragging.current) {

			if(!this.root.input.button.current) {

				this.dragging.current = false;
			}

		} else {

			if(this.focused.current) {

				if(this.root.input.button.current) {

					this.grab_offset = this.getPosition() - mouse_pos;

					this.dragging.current = true;
				}
			}
		}

		if(this.dragging.current) {

			this.setPosition(mouse_pos + this.grab_offset);
		}
	}

	// 判断按钮是否位于某项目上
	public virtual bool		isContainPoint(Vector2 point)
	{
		return(false);
	}
}


} // namespace Botan}

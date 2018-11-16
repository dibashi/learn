using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Botan {

	// 组
	public class Group {

		public string			name = "";
		public bool				is_visible = true;

		public ItemBase			focused_item = null;
		public List<ItemBase>	items = new List<ItemBase>();

		public void		setVisible(bool is_visible)
		{
			this.is_visible = is_visible;

			foreach(var item in this.items) {

				item.setVisible(this.is_visible);
			}

			if(!this.is_visible) {

				this.focused_item = null;
			}
		}
	}

	// 输入数据
	public struct InputData {

		public bool		enable;

		public struct ButtonData {
	
			public bool		trigger_on;
			public bool		current;
		}

		public Vector2		mouse_position;
		public ButtonData	button;
	}
	
} // namespace Botan





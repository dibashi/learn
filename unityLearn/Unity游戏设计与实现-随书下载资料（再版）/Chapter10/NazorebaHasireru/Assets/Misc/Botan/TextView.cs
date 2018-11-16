using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathExtension;

namespace Botan {

public class TextView : ItemBase {

	protected Botan.Waku	waku;
	protected Font			font;
	protected int			font_size;
	protected Vector2		window_size;

	public class TextLine {

		public string 				text = "";
		public TextSprite2DControl	sprite = null;
	}
	public List<TextLine>	lines = new List<TextLine>();

	public Vector2		text_offset = Vector2.zero;

	// ================================================================ //

	// 创建
	public void		create(Vector2 window_size, Font font, int font_size)
	{
		this.window_size = window_size;

		this.font      = font;
		this.font_size = font_size;

		this.waku = BotanRoot.get().createWaku(this.window_size);
		this.waku.setPosition(new Vector2(0.0f, 128.0f));
	}

	// 添加文本
	public void		addText(string text)
	{
		TextLine	new_line = new TextLine();

		new_line.text = text;

		new_line.sprite = Sprite2DRoot.get().createTextSprite(font, text, this.font_size, true);
		new_line.sprite.setDepthLayer("ui.item");
		new_line.sprite.setParent(this.waku.null_sprite);

		Vector2		position = Vector2.zero;

		position.x = -this.window_size.x/2.0f + text.Length*this.font_size/2.0f;
		position.y =  this.window_size.y/2.0f - this.font_size/2.0f - this.lines.Count*this.font_size;

		new_line.sprite.setPosition(position);

		this.lines.Add(new_line);
	}

	public void		execute()
	{
		float	delta_time = Time.deltaTime;
		float	speed = 64.0f;

		if(Input.GetKey(KeyCode.LeftArrow)) {

			this.text_offset.x -= speed*delta_time;
		}
		if(Input.GetKey(KeyCode.RightArrow)) {

			this.text_offset.x += speed*delta_time;
		}
		if(Input.GetKey(KeyCode.DownArrow)) {

			this.text_offset.y -= speed*delta_time;
		}
		if(Input.GetKey(KeyCode.UpArrow)) {

			this.text_offset.y += speed*delta_time;
		}

		for(int i = 0;i < this.lines.Count;i++) {

			TextLine		line = this.lines[i];

			Vector2		position = Vector2.zero;

			position.x = -this.window_size.x/2.0f + line.text.Length*this.font_size/2.0f;
			position.y =  this.window_size.y/2.0f - this.font_size/2.0f - i*this.font_size;

			position += this.text_offset;

			line.sprite.setPosition(position);

			Rect	rect = new Rect(-window_size.x/2.0f - position.x, -window_size.y/2.0f - position.y, window_size.x, window_size.y);

			line.sprite.clipByRect(rect);
		}
	}
}

} // namespace Botan}

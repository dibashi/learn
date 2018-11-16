using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathExtension;

namespace Botan {

public class Waku : ItemBase {

	public enum PART {

		NONE = -1,

		CENTER = 0,
		RIGHT_TOP,
		RIGHT_BOTTOM,
		LEFT_TOP,
		LEFT_BOTTOM,

		RIGHT,
		LEFT,
		TOP,
		BOTTOM,

		NUM,
	}

	public Sprite2DControl			null_sprite;
	public Sprite2DControl			sprite_back;
	public List<Sprite2DControl>	sprites = new List<Sprite2DControl>();

	protected float		scale_timer = 0.0f;
	protected Vector2	size;
	protected float		margin = 16.0f;

	// ================================================================ //
	// 继承于MonoBehaviour

	void	Awake()
	{
		this.is_can_drag = true;
	}

	// ================================================================ //

	// 设置位置
	public override void	setPosition(Vector2 position)
	{
		this.null_sprite.setPosition(position);
	}

	// 获取位置
	public override Vector2	getPosition()
	{
		return(this.null_sprite.getPosition());
	}

	// 判断某点是否位于项目上
	public override bool		isContainPoint(Vector2 point)
	{
		Vector2		position = this.null_sprite.getPosition();

		float	w = size.x + margin*2.0f;
		float	h = size.y + margin*2.0f;

		w *= this.null_sprite.transform.localScale.x;
		h *= this.null_sprite.transform.localScale.y;

		Rect	rect = new Rect(position.x - w/2.0f, position.y - h/2.0f, w, h);

		return(rect.Contains(point));
	}

	// ================================================================ //

	public override void	execute_entity()
	{
	}

	// 创建
	public void		create(Vector2 size)
	{
		this.size = size;
		this.null_sprite = Sprite2DRoot.get().createNull();

		//

		for(int i = 0;i < (int)PART.NUM;i++) {

			this.sprites.Add(null);
		}

		this.sprites[(int)PART.CENTER] = Sprite2DRoot.get().createSprite(this.root.texture_white8x8, 2, true);
		this.sprites[(int)PART.CENTER].name = "Botan.waku";
		this.sprites[(int)PART.CENTER].setSize(size);
		this.sprites[(int)PART.CENTER].setDepthLayer("ui.item");

		//
			
		this.sprites[(int)PART.RIGHT_TOP] = Sprite2DRoot.get().createSprite(this.root.texture_waku_kado, 2, true);
		this.sprites[(int)PART.RIGHT_TOP].name = "Botan.waku";
		this.sprites[(int)PART.RIGHT_TOP].setPosition(new Vector2( (size.x/2.0f + this.margin/2.0f), size.y/2.0f + this.margin/2.0f));
		this.sprites[(int)PART.RIGHT_TOP].setAngle(-90.0f);
		this.sprites[(int)PART.RIGHT_TOP].setSize(Vector2.one*this.margin);
		this.sprites[(int)PART.RIGHT_TOP].setDepthLayer("ui.item");

		this.sprites[(int)PART.RIGHT_BOTTOM] = Sprite2DRoot.get().createSprite(this.root.texture_waku_kado, 2, true);
		this.sprites[(int)PART.RIGHT_BOTTOM].name = "Botan.waku";
		this.sprites[(int)PART.RIGHT_BOTTOM].setPosition(new Vector2( (size.x/2.0f + this.margin/2.0f), -(size.y/2.0f + this.margin/2.0f)));
		this.sprites[(int)PART.RIGHT_BOTTOM].setAngle(-180.0f);
		this.sprites[(int)PART.RIGHT_BOTTOM].setSize(Vector2.one*this.margin);
		this.sprites[(int)PART.RIGHT_BOTTOM].setDepthLayer("ui.item");

		this.sprites[(int)PART.LEFT_TOP] = Sprite2DRoot.get().createSprite(this.root.texture_waku_kado, 2, true);
		this.sprites[(int)PART.LEFT_TOP].name = "Botan.waku";
		this.sprites[(int)PART.LEFT_TOP].setPosition(new Vector2(-(size.x/2.0f + this.margin/2.0f), size.y/2.0f + this.margin/2.0f));
		this.sprites[(int)PART.LEFT_TOP].setSize(Vector2.one*this.margin);
		this.sprites[(int)PART.LEFT_TOP].setDepthLayer("ui.item");

		this.sprites[(int)PART.LEFT_BOTTOM] = Sprite2DRoot.get().createSprite(this.root.texture_waku_kado, 2, true);
		this.sprites[(int)PART.LEFT_BOTTOM].name = "Botan.waku";
		this.sprites[(int)PART.LEFT_BOTTOM].setPosition(new Vector2(-(size.x/2.0f + this.margin/2.0f), -(size.y/2.0f + this.margin/2.0f)));
		this.sprites[(int)PART.LEFT_BOTTOM].setAngle(90.0f);
		this.sprites[(int)PART.LEFT_BOTTOM].setSize(Vector2.one*margin);
		this.sprites[(int)PART.LEFT_BOTTOM].setDepthLayer("ui.item");

		//

		this.sprites[(int)PART.RIGHT] = Sprite2DRoot.get().createSprite(this.root.texture_waku_ue, 2, true);
		this.sprites[(int)PART.RIGHT].name = "Botan.waku";
		this.sprites[(int)PART.RIGHT].setPosition(new Vector2(size.x/2.0f + this.margin/2.0f, 0.0f));
		this.sprites[(int)PART.RIGHT].setAngle(-90.0f);
		this.sprites[(int)PART.RIGHT].setSize(new Vector2(size.y, this.margin));
		this.sprites[(int)PART.RIGHT].setDepthLayer("ui.item");

		this.sprites[(int)PART.LEFT] = Sprite2DRoot.get().createSprite(this.root.texture_waku_ue, 2, true);
		this.sprites[(int)PART.LEFT].name = "Botan.waku";
		this.sprites[(int)PART.LEFT].setPosition(new Vector2(-(size.x/2.0f + this.margin/2.0f), 0.0f));
		this.sprites[(int)PART.LEFT].setAngle(90.0f);
		this.sprites[(int)PART.LEFT].setSize(new Vector2(size.y, this.margin));
		this.sprites[(int)PART.LEFT].setDepthLayer("ui.item");

		this.sprites[(int)PART.TOP] = Sprite2DRoot.get().createSprite(this.root.texture_waku_ue, 2, true);
		this.sprites[(int)PART.TOP].name = "Botan.waku";
		this.sprites[(int)PART.TOP].setPosition(new Vector2(0.0f, size.y/2.0f + this.margin/2.0f));
		this.sprites[(int)PART.TOP].setSize(new Vector2(size.x, this.margin));
		this.sprites[(int)PART.TOP].setDepthLayer("ui.item");

		this.sprites[(int)PART.BOTTOM] = Sprite2DRoot.get().createSprite(this.root.texture_waku_ue, 2, true);
		this.sprites[(int)PART.BOTTOM].name = "Botan.waku";
		this.sprites[(int)PART.BOTTOM].setPosition(new Vector2(0.0f, -(size.y/2.0f + this.margin/2.0f)));
		this.sprites[(int)PART.BOTTOM].setAngle(-180.0f);
		this.sprites[(int)PART.BOTTOM].setSize(new Vector2(size.x, this.margin));
		this.sprites[(int)PART.BOTTOM].setDepthLayer("ui.item");

		foreach(var sprite in this.sprites) {

			sprite.setParent(this.null_sprite);
		}
	}
	
}

} // namespace Botan}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FaceIcon : MonoBehaviour {

	public UnityEngine.Sprite	uiSpriteNormal;
	public UnityEngine.Sprite	uiSpriteChain1;
	public UnityEngine.Sprite	uiSpriteChain2;
	public UnityEngine.Sprite	uiSpriteFailed;

	public const float		WIDTH  = 64.0f;
	public const float		HEIGHT = 64.0f;

	public const float	SCALE_NORMAL  = 1.0f;
	public const float	SCALE_COMBO01 = 1.5f;
	public const float	SCALE_COMBO02 = 2.0f;

	protected const float	IP_RATE_NORMAL = 0.12f;
	protected const float	IP_RATE_MISS   = 0.06f;

	// ---------------------------------------------------------------- //

	public enum STEP {
		
		NONE = -1,
		
		NORMAL = 0,

		MISS,			// 错过
		VANISHED,		// 错过后，将在画面下方消失

		NUM,
	};
	public Step<STEP>			step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	protected Vector2	base_position = Vector2.zero;
	protected float		angle = 0.0f;

	protected ipModule.Asymptote	ip_scale = new ipModule.Asymptote();

	protected class StepMiss {

		public ipModule.Jump	ip_jump   = new ipModule.Jump();
	}
	protected StepMiss	step_miss = new StepMiss();

	protected UnityEngine.UI.Image	ui_image;

	// ================================================================ //
	// 继承于MonoBehaviour

	void	Awake()
	{
		this.ip_scale.ip_rate = IP_RATE_NORMAL;
		this.ip_scale.setCurrent(1.0f);

		this.step_miss.ip_jump.gravity = Physics.gravity*200.0f;
	}

	void	Start()
	{
		this.step.set_next(STEP.NORMAL);
	}
	
	void	Update()
	{
		float	delta_time = Time.deltaTime;

		Vector2		position = this.base_position;

		// ---------------------------------------------------------------- //
		// 检测是否需要迁移到下一状态
		
		switch(this.step.do_transition()) {

			case STEP.MISS:
			{
				if(this.step_miss.ip_jump.isDone()) {

					this.step.set_next(STEP.VANISHED);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 发生状态迁移时的初始化
		
		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				case STEP.MISS:
				{
					position.x += (this.ip_scale.getCurrent().x - 1.0f)*WIDTH/2.0f;

					Vector3		start = position;
					Vector3		goal  = position;

					// 在画面下方完全消失时的高度
					goal.y = -(Screen.height/2.0f + HEIGHT*SCALE_COMBO02);

					this.step_miss.ip_jump.start(start, goal, start.y + 64.0f);
				}
				break;

				case STEP.VANISHED:
				{
					this.gameObject.SetActive(false);
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 各个状态的执行处理
		
		switch(this.step.do_execution(delta_time)) {
			
			case STEP.NORMAL:
			{
				position = this.base_position;

				this.ip_scale.execute(delta_time);

				// 如果执行了缩放，要保证其位于合理范围内
				position.x += (this.ip_scale.getCurrent().x - 1.0f)*WIDTH/2.0f;
			}
			break;

			case STEP.MISS:
			{
				this.step_miss.ip_jump.execute(delta_time);

				position = this.step_miss.ip_jump.position;

				this.angle += -45.0f*delta_time;

				this.ip_scale.execute(delta_time);
			}
			break;
		}
		
		// ---------------------------------------------------------------- //

		this.set_position(position);
		this.set_angle(this.angle);
		this.set_scale(this.ip_scale.getCurrent().x);
	}

	// ================================================================ //

	public bool		isVanished()
	{
		return(this.step.get_current() == STEP.VANISHED);
	}

	public void		setPosition(Vector2 position)
	{
		this.base_position = position;
		this.set_position(this.base_position);
	}

	public void		setCombo(SceneControl.COMBO combo)
	{
		UnityEngine.Sprite	sprite = null;

		switch(combo) {

			case SceneControl.COMBO.NORMAL:
			{
				sprite = this.uiSpriteNormal;
				this.ip_scale.ip_rate = IP_RATE_NORMAL;
				this.ip_scale.start(SCALE_NORMAL);
			}
			break;
			case SceneControl.COMBO.CHAIN01:
			{
				sprite = this.uiSpriteChain1;
				this.ip_scale.ip_rate = IP_RATE_NORMAL;
				this.ip_scale.start(SCALE_COMBO01);
			}
			break;
			case SceneControl.COMBO.CHAIN02:
			{
				sprite = this.uiSpriteChain2;
				this.ip_scale.ip_rate = IP_RATE_NORMAL;
				this.ip_scale.start(SCALE_COMBO02);
			}
			break;
			case SceneControl.COMBO.FAILED:
			{
				sprite = this.uiSpriteFailed;
				this.step.set_next(STEP.MISS);
			}
			break;
		}

		if(sprite != null) {
			this.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
		}
	}

	// ================================================================ //

	public void		set_position(Vector2 position)
	{
		this.GetComponent<RectTransform>().localPosition = new Vector3(position.x, position.y, 0.0f);
	}
	public void		set_angle(float angle)
	{
		this.GetComponent<RectTransform>().localRotation = Quaternion.AngleAxis(this.angle, Vector3.forward);
	}
	public void		set_scale(float scale)
	{
		this.GetComponent<RectTransform>().localScale = Vector3.one*scale;
	}

}
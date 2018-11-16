using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

	public int	lx = 0;

	// 举起的方块的预设
	public 	GameObject	CarryBlockPrefab = null;

	public	GameObject	effect;

	// 纹理
	public Texture[]	textures_normal = null;		// 一般情况下
	public Texture[]	textures_carry  = null;		// 举起方块时
	public Texture[]	textures_eating = null;		// 正在吃蛋糕时
	public Texture		texture_hungry  = null;

	public AudioClip	audio_walk;
	public AudioClip	audio_pick;

	// ---------------------------------------------------------------- //

	// 举起的方块
	public CarryBlock	carry_block = null;

	public SceneControl	scene_control = null;


	// 用于显示的简易图片
	public SimpleSprite	sprite = null;


	// ---------------------------------------------------------------- //
	// 生命值

	public static int	LIFE_MIN = 0;				// 最小值.
	public static int	LIFE_MAX = 100;				// 最大值.
	public static int	LIFE_ADD_CAKE = LIFE_MAX;	// 吃了蛋糕后增加的值
	public static int	LIFE_SUB = -2;

	public int	life;								// 当前值

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		NORMAL = 0,			// 普通情况
		CARRY,				// 举起方块
		EATING,				// 正在吃蛋糕
		HUNGRY,				// 肚子饿时

		GOAL_ACT,			// 得分

		NUM,
	};

	public STEP			step;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;

	public bool			is_controlable = true;

	// ---------------------------------------------------------------- //

	void 	Start()
	{
		this.SetLinedPosition(StackBlockControl.BLOCK_NUM_X/2);

		GameObject game_object = Instantiate(this.CarryBlockPrefab) as GameObject;

		this.carry_block = game_object.GetComponent<CarryBlock>();

		this.carry_block.player             = this;
		this.carry_block.transform.position = this.transform.position + new Vector3(0.0f, 1.0f, 0.0f);
		this.carry_block.GetComponent<Renderer>().enabled   = false;

		//

		this.sprite = this.gameObject.AddComponent<SimpleSprite>();

		this.sprite.SetTexture(this.textures_normal[0]);

		//

		this.life = LIFE_MAX;

		this.is_controlable = true;
	}

	void Update ()
	{
		StackBlockControl	stack_control = this.scene_control.stack_control;

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
#if false
		// 按下“3”后，能量减少
		if(Input.GetKey(KeyCode.Keypad3)) {

			this.addLife(-1);
		}
		// 按下“4”后，能量增加
		if(Input.GetKey(KeyCode.Keypad4)) {

			this.addLife(1);
		}
#endif

		// 饥饿达到一定程度后，游戏结束
		if(this.life <= LIFE_MIN) {

			this.next_step = STEP.HUNGRY;
		}

		//
		// 检测是否移到下一个状态
		switch(this.step) {

			case STEP.NORMAL:
			case STEP.EATING:
			{
				// 举起

				if(this.next_step == STEP.NONE) {

					do {
	
						if(!this.is_carry_input()) {
						
							break;
						}
	
						// 脚下的方块
						StackBlock	ground_block = stack_control.blocks[this.lx, StackBlockControl.GROUND_LINE];
	
						// 灰色的方块不能被举起
						if(!ground_block.isCarriable()) {
	
							break;
						}
	
						// 正在执行交换的方块不能被举起
						if(ground_block.isNowSwapAction()) {
	
							break;
						}
	
						//
	
						// 将举起的方块的颜色设置为和脚下方块相同
						this.carry_block.setColorType(ground_block.color_type);
						this.carry_block.startCarry(this.lx);
	
						stack_control.pickBlock(this.lx);
	
						//

						this.GetComponent<AudioSource>().PlayOneShot(this.audio_pick);

						this.next_step = STEP.CARRY;
	
					} while(false);
				}

				if(this.next_step == STEP.NONE) {

					if(this.step == STEP.EATING) {

						if(this.step_timer > 3.0f) {
		
							this.next_step = STEP.NORMAL;
						}
					}
				}
			}
			break;

			case STEP.CARRY:
			{
				if(this.is_carry_input()) {

					// 放下

					if(this.carry_block.isCakeBlock()) {

						// 如果举起的时蛋糕，
						// 吃掉 & 改变颜色

						this.carry_block.startHide();

						stack_control.onEatCake();

						this.addLife(LIFE_ADD_CAKE);

						this.GetComponent<AudioSource>().PlayOneShot(scene_control.audio_clips[(int)SceneControl.SE.EATING]);

						//

						this.next_step = STEP.EATING;

					} else {

						// 如果举起的是普通的方块，则放下

						this.drop_block();

						this.addLife(LIFE_SUB);

						this.next_step = STEP.NORMAL;
					}
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 状态迁移时的初始化

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
	
				case STEP.NORMAL:
				{
				}
				break;

				case STEP.HUNGRY:
				{
				}
				break;

				case STEP.GOAL_ACT:
				{
					this.SetHeight(-1);
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 各个状态的执行处理

		switch(this.step) {

			case STEP.NORMAL:
			case STEP.CARRY:
			case STEP.EATING:
			{
				int		lx = this.lx;
		
				// 左右移动
		
				do {

					// 举起，放下过程中不能左右移动
					//
					// 如果习惯于这种操作，有时可能会无法移动影响了游戏操作
					// 屏蔽
					//
					/*if(this.carry_block.isMoving()) {
		
						break;
					}*/
		
					//

					if(!this.is_controlable) {

						break;
					}
		
					if(Input.GetKeyDown(KeyCode.LeftArrow)) {
			
						lx--;
			
					} else if(Input.GetKeyDown(KeyCode.RightArrow)) {
			
						lx++;

					} else {

						break;
					}
			
					lx = Mathf.Clamp(lx, 0, StackBlockControl.BLOCK_NUM_X - 1);
			
					this.GetComponent<AudioSource>().PlayOneShot(this.audio_walk);

					this.SetLinedPosition(lx);
		
				} while(false);
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 控制纹理模式

		switch(this.step) {

			default:
			case STEP.NORMAL:
			{
				// 左→闭上眼睛→右→闭上眼睛→循环

				int		texture_index;

				texture_index = (int)(this.step_timer*8.0f);
				texture_index %= 4;

				if(texture_index%2 == 0) {

					// 闭上眼睛
					texture_index = 0;

				} else {

					// 右，左
					texture_index = (texture_index/2)%2 + 1;
				}

				this.sprite.SetTexture(this.textures_normal[texture_index]);

			}
			break;

			case STEP.CARRY:
			{
				int		texture_index;

				texture_index = (int)(this.step_timer*8.0f);
				texture_index %= 4;

				if(texture_index%2 == 0) {

					texture_index = 0;

				} else {

					texture_index = (texture_index/2)%2 + 1;
				}

				this.sprite.SetTexture(this.textures_carry[texture_index]);
			}
			break;

			case STEP.EATING:
			{
				int		texture_index = ((int)(this.step_timer/0.1f))%this.textures_eating.Length;

				this.sprite.SetTexture(this.textures_eating[texture_index]);
			}
			break;

			case STEP.HUNGRY:
			{
				this.sprite.SetTexture(this.texture_hungry);
			}
			break;

			case STEP.GOAL_ACT:
			{
				const float		time0 = 0.5f;
				const float		time1 = 0.5f;

				float	time_all = time0 + time1;

				float	t = Mathf.Repeat(this.step_timer, time_all);

				if(t < time0) {

					this.sprite.SetTexture(this.textures_carry[1]);

				} else {

					t -= time0;

					int		texture_index = ((int)(t/0.1f))%this.textures_eating.Length;

					this.sprite.SetTexture(this.textures_eating[texture_index]);
				}
			}
			break;
		}
	}

	// 放下方块
	public void	dropBlock()
	{
		if(this.step == STEP.CARRY) {

			this.drop_block();

			this.next_step = STEP.NORMAL;
		}
	}
	private void	drop_block()
	{
		this.carry_block.startDrop(this.lx);
	
		this.scene_control.stack_control.dropBlock(this.lx, this.carry_block.color_type, this.carry_block.org_place.x);
	}

	// 设置位置
	public void	SetLinedPosition(int lx)
	{
		this.lx = lx;

		Vector3		position = this.transform.position;

		position.x = -(StackBlockControl.BLOCK_NUM_X/2.0f - 0.5f) + this.lx;

		this.transform.position = position;
	}

	// 设置高度
	public void	SetHeight(int height)
	{
		StackBlock.PlaceIndex place;

		place.x = this.lx;
		place.y = StackBlockControl.GROUND_LINE - 1 + height;

		this.transform.position = StackBlockControl.calcIndexedPosition(place);
	}

	// 增加或减少生命值
	public void		addLife(int val)
	{
		this.life += val;
	
		this.life = Mathf.Min(Mathf.Max(LIFE_MIN, this.life), LIFE_MAX);
	}

	// 获取现在的生命值（比率）
	public float	getLifeRate()
	{
		float	rate = Mathf.InverseLerp((float)LIFE_MIN, (float)LIFE_MAX, (float)this.life);

		return(rate);
	}

	// 生命值是否为0？
	public bool	isHungry()
	{
		bool	ret = (this.life <= LIFE_MIN);

		return(ret);
	}

	// 设置为不可操作
	public void	setControlable(bool sw)
	{
		this.is_controlable = sw;
	}

	// 得分时的动作
	public void	beginGoalAct()
	{
		this.next_step = STEP.GOAL_ACT;
	}

	// 失败后的复活（当还有残余生命时）
	public void	revive()
	{
		this.life = LIFE_MAX;

		this.next_step = STEP.NORMAL;
	}

	// 是否按下了 举起／放下 方块的按键？
	private bool	is_carry_input()
	{
		bool	ret;

		if(this.is_controlable) {

			ret = (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow));

		} else {

			ret = false;
		}

		return(ret);
	}

}
